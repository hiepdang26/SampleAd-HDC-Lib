using System;
using BG_Library.Common;
using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEngine;

namespace BG_Library.NET.AdCore
{
    public class NetStatsWindow : GenericJsonInspectorWindow<AdSystemConfigs, NetStatsSettings>
    {
        private const string ConfigsSoFoldoutKey = "BG_NetStatsWindow_ConfigsSoFoldout";

        private Editor netConfigsEditor;

        [MenuItem(NetStatsConst.MENU)]
        public static void Open()
        {
            var window = GetWindow<NetStatsWindow>(NetStatsConst.TITLE);
            window.minSize = new Vector2(900f, 680f);
            window.Show();
        }

        protected override void OnDisable()
        {
            if (netConfigsEditor != null)
            {
                DestroyImmediate(netConfigsEditor);
                netConfigsEditor = null;
            }

            base.OnDisable();
        }

        protected override void DrawBeforeInspector(SerializedObject serializedObject, SerializedProperty dataProp)
        {
            DrawAdCoreSection(dataProp);
        }

        protected override void DrawAfterJson()
        {
            DrawNetConfigsSection();
        }

        private void DrawNetConfigsSection()
        {
            NetConfigsSO netConfigs = NetConfigsSO.Ins;

            EditorGUILayout.Space(12f);
            DrawSectionSeparator("Configs SO");
            EditorGUILayout.Space(4f);

            bool isExpanded = SessionState.GetBool(ConfigsSoFoldoutKey, false);
            bool nextExpanded = EditorGUILayout.Foldout(isExpanded, "NetConfigs SO", true, EditorStyles.foldoutHeader);
            if (nextExpanded != isExpanded)
                SessionState.SetBool(ConfigsSoFoldoutKey, nextExpanded);

            if (!nextExpanded)
                return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.ObjectField("Configs SO", netConfigs, typeof(NetConfigsSO), false);

                    using (new EditorGUI.DisabledScope(netConfigs == null))
                    {
                        if (GUILayout.Button("Ping", GUILayout.Width(70f)))
                        {
                            Selection.activeObject = netConfigs;
                            EditorGUIUtility.PingObject(netConfigs);
                        }

                        if (GUILayout.Button("Open Asset", GUILayout.Width(100f)))
                        {
                            Selection.activeObject = netConfigs;
                            EditorGUIUtility.PingObject(netConfigs);
                        }
                    }
                }

                if (netConfigs == null)
                {
                    EditorGUILayout.HelpBox("Khong tim thay NetConfigsSO trong Resources.", MessageType.Warning);
                    return;
                }

                EditorGUILayout.HelpBox(
                    "Day la inspector cua NetConfigsSO duoc nhung vao Edit ads_config. Section nay mac dinh dong de tranh lam nang window khi ban chi sua ads_config.",
                    MessageType.Info);

                Editor.CreateCachedEditor(netConfigs, null, ref netConfigsEditor);
                if (netConfigsEditor != null)
                    netConfigsEditor.OnInspectorGUI();
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawAdCoreSection(SerializedProperty dataProp)
        {
            SerializedProperty adCoreProp = dataProp.FindPropertyRelative("selectedAdCoreName");
            if (adCoreProp == null)
                return;

            string[] availableNames = NetConfigsSOEditorStorage.GetAvailableAdCoreNames();
            if (availableNames.Length == 0)
            {
                EditorGUILayout.HelpBox("Khong tim thay AdCoreBase nao trong project.", MessageType.Warning);
                return;
            }

            int selectedIndex = Array.IndexOf(availableNames, adCoreProp.stringValue);
            string[] displayNames = availableNames;

            if (selectedIndex < 0 && !string.IsNullOrEmpty(adCoreProp.stringValue))
            {
                displayNames = new string[availableNames.Length + 1];
                displayNames[0] = $"{adCoreProp.stringValue} (chua co trong project)";
                Array.Copy(availableNames, 0, displayNames, 1, availableNames.Length);
                selectedIndex = 0;
            }
            else if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("AdCore Selection", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Selected AdCore", selectedIndex, displayNames);
            if (EditorGUI.EndChangeCheck())
            {
                adCoreProp.stringValue = displayNames == availableNames
                    ? availableNames[newIndex]
                    : (newIndex == 0 ? adCoreProp.stringValue : availableNames[newIndex - 1]);
            }

            EditorGUILayout.HelpBox(
                "Danh sach lay tu cac AdCoreBase asset dang co trong project. Khi Save JSON to Source, NetConfigsSO se tu them AdCore con thieu vao danh sach adCores.",
                MessageType.Info);
            EditorGUILayout.Space(6f);
        }

        private static void DrawSectionSeparator(string label)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 20f);
            float labelWidth = Mathf.Min(150f, GUI.skin.label.CalcSize(new GUIContent(label)).x + 16f);
            float lineY = rect.y + 10f;
            float gap = 6f;

            Rect leftLine = new Rect(rect.x, lineY, 8f, 1f);
            Rect rightLine = new Rect(rect.x + labelWidth + gap, lineY, Mathf.Max(0f, rect.width - labelWidth - gap), 1f);
            EditorGUI.DrawRect(leftLine, new Color(0.32f, 0.32f, 0.32f, 1f));
            EditorGUI.DrawRect(rightLine, new Color(0.32f, 0.32f, 0.32f, 1f));

            Rect labelRect = new Rect(rect.x + 12f, rect.y, labelWidth, rect.height);
            EditorGUI.LabelField(labelRect, label, EditorStyles.miniBoldLabel);
        }
    }

    public static class NetStatsConst
    {
        public const string MENU = "BG/Edit stats/Ads configs #n";
        public const string TITLE = "Edit ads_config";
    }

    public class NetStatsSettings : IGenericJsonInspectorSettings<AdSystemConfigs>
    {
        public string WindowTitle => NetStatsConst.TITLE;
        public string LogTag => "AdSystemConfigs";
        public string StorageDescription => NetConfigsSOEditorStorage.GetAdsConfigStorageDescription();

        public float JsonHeight => 420f;
        public bool WordWrapJson => false;

        public AdSystemConfigs CreateDefault() => new AdSystemConfigs();

        public string Serialize(AdSystemConfigs data) => JsonTool.SerializeObject(data);
        public AdSystemConfigs Deserialize(string json) => JsonTool.DeserializeObject<AdSystemConfigs>(json);
        public string LoadStoredJson() => NetConfigsSOEditorStorage.LoadAdsConfigJson();
        public void SaveStoredJson(string json) => NetConfigsSOEditorStorage.SaveAdsConfigJson(json);
    }
}
