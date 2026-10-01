using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BG_Library.NET.AdCore
{
    [Serializable]
    public class BG_ManagedRefWrapper : ScriptableObject
    {
        [SerializeReference] public object data;
    }

    public abstract class GenericJsonInspectorWindow<TData, TSettings> : EditorWindow
        where TData : class, new()
        where TSettings : IGenericJsonInspectorSettings<TData>, new()
    {
        private TSettings _settings;

        private BG_ManagedRefWrapper _wrapper;
        private SerializedObject _so;
        private SerializedProperty _dataProp;

        private string _json;
        private string _sourceSnapshotJson;
        private Vector2 _scroll;
        private Vector2 _jsonScroll;
        private GUIStyle _jsonTextAreaStyle;
        private bool _showInspector = true;
        private bool _showJson;
        private bool _hasUnsavedChanges;
        private bool _jsonEditedSinceDeserialize;
        private bool _objectEditedSinceSync;
        private string _restoreMessage;
        private MessageType _restoreMessageType = MessageType.Info;

        [Serializable]
        private sealed class DraftPayload
        {
            public string json;
            public string sourceSnapshot;
            public string updatedAtUtc;
        }

        private void EnsureSettings()
        {
            if (_settings == null) _settings = new TSettings();
            if (_jsonTextAreaStyle == null)
            {
                _jsonTextAreaStyle = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = _settings.WordWrapJson
                };
            }
        }

        protected virtual void OnEnable()
        {
            EnsureSettings();
            CreateWrapperIfNeeded();
            RestoreDraftOrLoadSource();
        }

        protected virtual void OnDisable()
        {
            EnsureSettings();
            SaveDraftIfNeeded();

            if (_wrapper != null)
            {
                DestroyImmediate(_wrapper);
                _wrapper = null;
                _so = null;
                _dataProp = null;
            }
        }

        private void CreateWrapperIfNeeded()
        {
            if (_wrapper != null) return;

            _wrapper = ScriptableObject.CreateInstance<BG_ManagedRefWrapper>();

            if (_wrapper == null)
            {
                UnityEngine.Debug.LogError($"[{typeof(TSettings).Name}] CreateInstance<BG_ManagedRefWrapper>() returned NULL.");
                return;
            }

            _wrapper.hideFlags =
                HideFlags.HideInHierarchy |
                HideFlags.DontSaveInEditor |
                HideFlags.DontSaveInBuild;

            _so = new SerializedObject(_wrapper);
            _dataProp = _so.FindProperty(nameof(BG_ManagedRefWrapper.data));

            if (_dataProp == null)
                UnityEngine.Debug.LogError($"[{typeof(TSettings).Name}] Cannot find SerializedProperty: {nameof(BG_ManagedRefWrapper.data)}");
        }

        protected virtual void OnGUI()
        {
            EnsureSettings();
            CreateWrapperIfNeeded();

            if (_wrapper == null || _so == null || _dataProp == null)
            {
                EditorGUILayout.HelpBox("Wrapper/SerializedObject chưa sẵn sàng (null). Hãy kiểm tra Console log.", MessageType.Error);
                return;
            }

            DrawActions();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawInspectorLikeObject();

            EditorGUILayout.Space(10);

            DrawJsonArea();

            DrawAfterJson();

            EditorGUILayout.EndScrollView();
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                GUILayout.Label("ACTIONS", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Load Source", GUILayout.Height(26)))
                        LoadFromSource();

                    if (GUILayout.Button("Serialize -> JSON", GUILayout.Height(26)))
                        SerializeToJson();

                    if (GUILayout.Button("Deserialize <- JSON", GUILayout.Height(26)))
                        DeserializeFromJson();

                    if (GUILayout.Button("Save JSON to Source", GUILayout.Height(26)))
                        SaveToSource();

                    if (GUILayout.Button("Reset Source", GUILayout.Height(26)))
                        ResetSourceToDefault();
                }

                EditorGUILayout.HelpBox(
                    $"Window: {_settings.WindowTitle}\nSource: {_settings.StorageDescription}",
                    MessageType.Info);

                if (!string.IsNullOrEmpty(_restoreMessage))
                    EditorGUILayout.HelpBox(_restoreMessage, _restoreMessageType);
            }
        }

        private void DrawInspectorLikeObject()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                _showInspector = EditorGUILayout.Foldout(_showInspector, "OBJECT (Inspector-like)", true);

                if (_showInspector)
                {
                    if (_dataProp.managedReferenceValue == null)
                    {
                        EditorGUILayout.HelpBox("Data đang NULL. Hãy Load/Deserialize để bắt đầu.", MessageType.Warning);
                    }
                    else
                    {
                        _so.Update();
                        EditorGUI.BeginChangeCheck();
                        DrawBeforeInspector(_so, _dataProp);
                        EditorGUILayout.PropertyField(_dataProp, includeChildren: true);
                        DrawAfterInspector(_so, _dataProp);
                        _so.ApplyModifiedProperties();

                        if (EditorGUI.EndChangeCheck())
                            MarkDirtyFromObject();
                    }

                    EditorGUILayout.Space(4f);
                }
            }
        }

        private void DrawJsonArea()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                string label = string.IsNullOrEmpty(_json) ? "JSON" : $"JSON ({_json.Length:N0} chars)";
                _showJson = EditorGUILayout.Foldout(_showJson, label, true);

                if (_showJson)
                {
                    float height = Mathf.Max(80f, _settings.JsonHeight);

                    using (var sv = new EditorGUILayout.ScrollViewScope(_jsonScroll, GUILayout.Height(height)))
                    {
                        _jsonScroll = sv.scrollPosition;
                        string newJson = EditorGUILayout.TextArea(_json ?? "", _jsonTextAreaStyle, GUILayout.ExpandHeight(true));
                        if (!string.Equals(newJson, _json, StringComparison.Ordinal))
                        {
                            _json = newJson;
                            MarkDirtyFromJson();
                        }
                    }

                    EditorGUILayout.Space(4f);
                }
            }
        }

        private void LoadFromSource()
        {
            if (_hasUnsavedChanges)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    _settings.WindowTitle,
                    "Bạn đang có bản draft chưa lưu. Nếu tiếp tục, draft hiện tại sẽ bị bỏ.",
                    "Bỏ draft",
                    "Cancel",
                    string.Empty);

                if (choice != 0)
                    return;
            }

            LoadCurrentSourceIntoEditor();
            DeleteDraft();
        }

        private void LoadCurrentSourceIntoEditor()
        {
            _json = _settings.LoadStoredJson() ?? "";
            _sourceSnapshotJson = _json ?? "";
            _restoreMessage = "";
            _restoreMessageType = MessageType.Info;

            if (string.IsNullOrEmpty(_json))
            {
                var obj = _settings.CreateDefault() ?? new TData();
                SetDataObject(obj);
                UnityEngine.Debug.Log($"[{_settings.LogTag}] Source rỗng -> đã tạo mới {typeof(TData).Name}.");
                ClearDirtyState();
                return;
            }

            TryDeserializeCurrentJson(logFailure: true);
            ClearDirtyState();
        }

        private void SaveToSource()
        {
            if (_jsonEditedSinceDeserialize && _objectEditedSinceSync)
            {
                int conflictChoice = EditorUtility.DisplayDialogComplex(
                    _settings.WindowTitle,
                    "Cả OBJECT và JSON đều đang có thay đổi chưa sync. Bạn muốn lưu bản nào vào Configs SO?",
                    "Save JSON",
                    "Save OBJECT",
                    "Cancel");

                if (conflictChoice == 2)
                    return;

                if (conflictChoice == 0)
                {
                    if (!TryDeserializeCurrentJson(logFailure: true))
                        return;
                }
                else if (!TrySerializeCurrentObject())
                {
                    return;
                }
            }
            else if (_jsonEditedSinceDeserialize)
            {
                if (!TryDeserializeCurrentJson(logFailure: true))
                    return;
            }
            else if (!TrySerializeCurrentObject())
            {
                return;
            }

            if (string.IsNullOrEmpty(_json))
            {
                UnityEngine.Debug.LogWarning($"[{_settings.LogTag}] JSON empty. Nothing to save.");
                return;
            }

            _settings.SaveStoredJson(_json);
            _sourceSnapshotJson = _json ?? "";
            ClearDirtyState();
            DeleteDraft();
            UnityEngine.Debug.Log($"[{_settings.LogTag}] JSON saved to source.");
        }

        private void ResetSourceToDefault()
        {
            if (_hasUnsavedChanges)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    _settings.WindowTitle,
                    "Bạn đang có bản draft chưa lưu. Nếu reset source, draft hiện tại sẽ bị bỏ.",
                    "Reset Source",
                    "Cancel",
                    string.Empty);

                if (choice != 0)
                    return;
            }

            try
            {
                var obj = _settings.CreateDefault() ?? new TData();
                _json = _settings.Serialize(obj);
                _settings.SaveStoredJson(_json);
                _sourceSnapshotJson = _json ?? "";
                SetDataObject(obj);
                ClearDirtyState();
                DeleteDraft();
                UnityEngine.Debug.Log($"[{_settings.LogTag}] Source reset to default {typeof(TData).Name}.");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[{_settings.LogTag}] Reset source failed: {e}");
            }
        }

        private void SerializeToJson()
        {
            if (_jsonEditedSinceDeserialize)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    _settings.WindowTitle,
                    "JSON đang có thay đổi chưa apply vào OBJECT. Serialize sẽ ghi đè phần JSON hiện tại.",
                    "Overwrite JSON",
                    "Cancel",
                    string.Empty);

                if (choice != 0)
                    return;
            }

            TrySerializeCurrentObject(logSuccess: true);
            _hasUnsavedChanges = !string.Equals(_json ?? "", _sourceSnapshotJson ?? "", StringComparison.Ordinal);
        }

        private void DeserializeFromJson()
        {
            if (string.IsNullOrEmpty(_json))
            {
                UnityEngine.Debug.LogWarning($"[{_settings.LogTag}] JSON empty. Cannot deserialize.");
                return;
            }

            if (_objectEditedSinceSync)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    _settings.WindowTitle,
                    "OBJECT đang có thay đổi chưa sync vào JSON. Deserialize sẽ ghi đè OBJECT hiện tại.",
                    "Overwrite OBJECT",
                    "Cancel",
                    string.Empty);

                if (choice != 0)
                    return;
            }

            if (!TryDeserializeCurrentJson(logFailure: true))
                return;

            _hasUnsavedChanges = !string.Equals(_json ?? "", _sourceSnapshotJson ?? "", StringComparison.Ordinal);
            UnityEngine.Debug.Log($"[{_settings.LogTag}] JSON -> {typeof(TData).Name}.");
        }

        private void SetDataObject(TData obj)
        {
            _so.Update();
            _dataProp.managedReferenceValue = obj;
            _so.ApplyModifiedPropertiesWithoutUndo();
            Repaint();
        }

        protected virtual void DrawBeforeInspector(SerializedObject serializedObject, SerializedProperty dataProp)
        {
        }

        protected virtual void DrawAfterInspector(SerializedObject serializedObject, SerializedProperty dataProp)
        {
        }

        protected virtual void DrawAfterJson()
        {
        }

        private bool TrySerializeCurrentObject(bool logSuccess = false)
        {
            _so.Update();
            _so.ApplyModifiedProperties();

            var typed = _dataProp.managedReferenceValue as TData;
            if (typed == null)
            {
                UnityEngine.Debug.LogWarning($"[{_settings.LogTag}] Data is null or wrong type ({typeof(TData).Name}).");
                return false;
            }

            try
            {
                _json = _settings.Serialize(typed);

                if (logSuccess)
                    UnityEngine.Debug.Log($"[{_settings.LogTag}] Serialized {typeof(TData).Name} -> JSON.");

                _jsonEditedSinceDeserialize = false;
                _objectEditedSinceSync = false;
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[{_settings.LogTag}] Serialize failed: {e}");
                return false;
            }
        }

        private bool TryDeserializeCurrentJson(bool logFailure)
        {
            try
            {
                var obj = _settings.Deserialize(_json) ?? new TData();
                SetDataObject(obj);
                _jsonEditedSinceDeserialize = false;
                _objectEditedSinceSync = false;
                return true;
            }
            catch (Exception e)
            {
                if (logFailure)
                    UnityEngine.Debug.LogError($"[{_settings.LogTag}] Deserialize failed: {e}");
                return false;
            }
        }

        private void MarkDirtyFromObject()
        {
            _hasUnsavedChanges = true;
            _objectEditedSinceSync = true;
        }

        private void MarkDirtyFromJson()
        {
            _hasUnsavedChanges = !string.Equals(_json ?? "", _sourceSnapshotJson ?? "", StringComparison.Ordinal);
            _jsonEditedSinceDeserialize = true;
        }

        private void ClearDirtyState()
        {
            _hasUnsavedChanges = false;
            _jsonEditedSinceDeserialize = false;
            _objectEditedSinceSync = false;
        }

        private void RestoreDraftOrLoadSource()
        {
            string currentSource = _settings.LoadStoredJson() ?? "";
            _sourceSnapshotJson = currentSource;
            _restoreMessage = "";
            _restoreMessageType = MessageType.Info;

            DraftPayload draft = LoadDraft();
            if (draft == null || string.IsNullOrEmpty(draft.json))
            {
                LoadCurrentSourceIntoEditor();
                return;
            }

            if (string.Equals(draft.json, currentSource, StringComparison.Ordinal))
            {
                DeleteDraft();
                LoadCurrentSourceIntoEditor();
                return;
            }

            bool sourceChangedSinceDraft = !string.Equals(draft.sourceSnapshot ?? "", currentSource, StringComparison.Ordinal);
            string message = sourceChangedSinceDraft
                ? "Đang có draft chưa lưu và nguồn trong Configs SO đã thay đổi kể từ lúc draft được tạo.\n\nBạn muốn giữ draft hay lấy bản mới từ Configs SO?"
                : "Đang có draft chưa lưu khác với dữ liệu trong Configs SO.\n\nBạn muốn giữ draft hay lấy bản từ Configs SO?";

            int choice = EditorUtility.DisplayDialogComplex(
                _settings.WindowTitle,
                message,
                "Keep Draft",
                "Use Configs SO",
                "Cancel");

            if (choice == 0)
            {
                _json = draft.json ?? "";
                _sourceSnapshotJson = currentSource;
                if (!string.IsNullOrEmpty(_json) && !TryDeserializeCurrentJson(logFailure: false))
                {
                    if (string.IsNullOrEmpty(currentSource))
                        SetDataObject(_settings.CreateDefault() ?? new TData());
                    else
                        LoadCurrentSourceIntoEditor();

                    _json = draft.json ?? "";
                    _showJson = true;
                    _restoreMessage = "Draft JSON không deserialize được tự động. Raw draft vẫn được giữ trong phần JSON để bạn tiếp tục sửa.";
                    _restoreMessageType = MessageType.Warning;
                }

                _hasUnsavedChanges = true;
                _jsonEditedSinceDeserialize = false;
                _objectEditedSinceSync = false;
                return;
            }

            if (choice == 1)
            {
                DeleteDraft();
                LoadCurrentSourceIntoEditor();
                return;
            }

            EditorApplication.delayCall += Close;
        }

        private void SaveDraftIfNeeded()
        {
            try
            {
                if (!_hasUnsavedChanges)
                {
                    DeleteDraft();
                    return;
                }

                string draftJson = _json ?? "";
                if (!_jsonEditedSinceDeserialize && (_objectEditedSinceSync || string.IsNullOrEmpty(draftJson)))
                    draftJson = TrySerializeCurrentObject() ? (_json ?? "") : draftJson;

                if (string.IsNullOrEmpty(draftJson) || string.Equals(draftJson, _sourceSnapshotJson ?? "", StringComparison.Ordinal))
                {
                    DeleteDraft();
                    return;
                }

                var payload = new DraftPayload
                {
                    json = draftJson,
                    sourceSnapshot = _sourceSnapshotJson ?? "",
                    updatedAtUtc = DateTime.UtcNow.ToString("O"),
                };

                string path = GetDraftFilePath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonUtility.ToJson(payload, true), Encoding.UTF8);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[{_settings.LogTag}] Save draft failed: {e.Message}");
            }
        }

        private DraftPayload LoadDraft()
        {
            try
            {
                EnsureSettings();
                string path = GetDraftFilePath();
                if (!File.Exists(path))
                    return null;

                string content = File.ReadAllText(path, Encoding.UTF8);
                if (string.IsNullOrEmpty(content))
                    return null;

                return JsonUtility.FromJson<DraftPayload>(content);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[{_settings.LogTag}] Load draft failed: {e.Message}");
                return null;
            }
        }

        private void DeleteDraft()
        {
            try
            {
                EnsureSettings();
                string path = GetDraftFilePath();
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[{_settings.LogTag}] Delete draft failed: {e.Message}");
            }
        }

        private string GetDraftFilePath()
        {
            EnsureSettings();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            string folder = Path.Combine(projectRoot, "Library", "BG_Lib", "JsonEditorDrafts");
            string key = $"{GetType().FullName}|{_settings.WindowTitle}|{_settings.StorageDescription}";
            string hash = ComputeStableHash(key);
            return Path.Combine(folder, $"{hash}.json");
        }

        private static string ComputeStableHash(string value)
        {
            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(value ?? "");
            byte[] hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString();
        }
    }

    public interface IGenericJsonInspectorSettings<TData>
    {
        string WindowTitle { get; }
        string LogTag { get; }
        string StorageDescription { get; }

        float JsonHeight { get; }
        bool WordWrapJson { get; }

        TData CreateDefault();

        string Serialize(TData data);
        TData Deserialize(string json);
        string LoadStoredJson();
        void SaveStoredJson(string json);
    }
}
