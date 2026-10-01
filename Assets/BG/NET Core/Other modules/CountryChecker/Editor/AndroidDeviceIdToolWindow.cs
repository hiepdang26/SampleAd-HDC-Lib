#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CountryRegionCheck.EditorTools
{
    /// <summary>
    /// Tool Editor: dò các máy đang kết nối qua ADB và lấy Android ID của từng máy.
    /// Mở từ menu: Tools/Android Device ID.
    /// </summary>
    public class AndroidDeviceIdToolWindow : EditorWindow
    {
        private class DeviceInfo
        {
            public string serial;
            public string state;
            public string model;
            public string androidId;
            public string error;
        }

        private readonly List<DeviceInfo> _devices = new List<DeviceInfo>();
        private string _adbPath;
        private string _status;
        private Vector2 _scroll;
        private bool _busy;

        [MenuItem("Tools/Android Device ID")]
        public static void Open()
        {
            AndroidDeviceIdToolWindow window = GetWindow<AndroidDeviceIdToolWindow>("Android Device ID");
            window.minSize = new Vector2(560, 320);
            window.Show();
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(_adbPath))
                _adbPath = ResolveAdbPath();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ADB Android ID Tool", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("adb:", GUILayout.Width(34));
                EditorGUILayout.SelectableLabel(
                    string.IsNullOrEmpty(_adbPath) ? "(không tìm thấy adb)" : _adbPath,
                    EditorStyles.textField, GUILayout.Height(18));
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    string picked = EditorUtility.OpenFilePanel("Chọn file adb", "", "");
                    if (!string.IsNullOrEmpty(picked)) _adbPath = picked;
                }
                if (GUILayout.Button("Auto", GUILayout.Width(48)))
                {
                    _adbPath = ResolveAdbPath();
                }
            }

            using (new EditorGUI.DisabledScope(_busy))
            {
                if (GUILayout.Button(_busy ? "Đang quét..." : "Detect Devices & Lấy Android ID", GUILayout.Height(30)))
                {
                    Refresh();
                }
            }

            if (!string.IsNullOrEmpty(_status))
                EditorGUILayout.HelpBox(_status, MessageType.Info);

            EditorGUILayout.HelpBox(
                "Lưu ý: trên Android 8.0+ (API 26+), ANDROID_ID lấy qua 'adb shell' là giá trị cấp thiết bị, " +
                "KHÁC với giá trị app đọc bằng Settings.Secure.ANDROID_ID (app-scoped theo signing key). " +
                "Nếu allowlist so với ID app đọc lúc runtime, hãy cho app tự Debug.Log ANDROID_ID rồi lấy từ logcat.",
                MessageType.Warning);

            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_devices.Count == 0)
            {
                EditorGUILayout.LabelField("Chưa có thiết bị. Bấm nút phía trên để quét.");
            }
            foreach (DeviceInfo d in _devices)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(d.model + "   [" + d.serial + "]", EditorStyles.boldLabel);
                    if (!string.IsNullOrEmpty(d.error))
                    {
                        EditorGUILayout.HelpBox(d.error, MessageType.Error);
                    }
                    else
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Android ID:", GUILayout.Width(78));
                            EditorGUILayout.SelectableLabel(d.androidId, EditorStyles.textField, GUILayout.Height(18));
                            if (GUILayout.Button("Copy", GUILayout.Width(54)))
                            {
                                EditorGUIUtility.systemCopyBuffer = d.androidId;
                                ShowNotification(new GUIContent("Đã copy Android ID"));
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void Refresh()
        {
            _devices.Clear();
            _status = "";

            if (string.IsNullOrEmpty(_adbPath) || (!File.Exists(_adbPath) && _adbPath != "adb" && _adbPath != "adb.exe"))
            {
                _status = "Không tìm thấy adb. Bấm '...' để chọn thủ công (thường ở <Android SDK>/platform-tools/adb.exe).";
                return;
            }

            _busy = true;
            try
            {
                EditorUtility.DisplayProgressBar("Android Device ID", "Đang chạy 'adb devices'...", 0.2f);
                string devicesOut = RunAdb("devices -l", 10000, out string err);
                if (devicesOut == null)
                {
                    _status = "Lỗi chạy adb: " + err;
                    return;
                }

                List<DeviceInfo> parsed = ParseDevices(devicesOut);
                if (parsed.Count == 0)
                {
                    _status = "Không có thiết bị nào ở trạng thái 'device'. Kiểm tra cáp/USB debugging/authorize trên máy.";
                    return;
                }

                for (int i = 0; i < parsed.Count; i++)
                {
                    DeviceInfo d = parsed[i];
                    EditorUtility.DisplayProgressBar("Android Device ID",
                        "Lấy Android ID: " + d.serial, 0.2f + 0.8f * (i + 1f) / parsed.Count);

                    string idOut = RunAdb("-s " + d.serial + " shell settings get secure android_id", 8000, out string idErr);
                    if (idOut == null)
                    {
                        d.error = "Lấy android_id lỗi: " + idErr;
                    }
                    else
                    {
                        string id = idOut.Trim();
                        if (string.IsNullOrEmpty(id) || id == "null")
                            d.error = "android_id rỗng/null trên máy này.";
                        else
                            d.androidId = id;
                    }

                    if (string.IsNullOrEmpty(d.model) || d.model == d.serial)
                    {
                        string modelOut = RunAdb("-s " + d.serial + " shell getprop ro.product.model", 8000, out string modelErr);
                        if (modelOut != null && modelOut.Trim().Length > 0)
                            d.model = modelOut.Trim();
                    }

                    _devices.Add(d);
                }

                _status = "Tìm thấy " + _devices.Count + " thiết bị.";
            }
            catch (Exception e)
            {
                _status = "Exception: " + e.Message;
                Debug.LogException(e);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _busy = false;
                Repaint();
            }
        }

        private List<DeviceInfo> ParseDevices(string adbDevicesOutput)
        {
            List<DeviceInfo> list = new List<DeviceInfo>();
            string[] lines = adbDevicesOutput.Replace("\r", "").Split('\n');
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0) continue;
                if (line.StartsWith("List of devices")) continue;
                if (line.StartsWith("*")) continue; // thông báo daemon

                // format: "<serial> <state> [key:value ...]"
                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                string serial = parts[0];
                string state = parts[1];
                if (state != "device") continue; // bỏ qua offline/unauthorized

                DeviceInfo d = new DeviceInfo { serial = serial, state = state, model = serial };
                for (int i = 2; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("model:"))
                        d.model = parts[i].Substring("model:".Length);
                }
                list.Add(d);
            }
            return list;
        }

        private string RunAdb(string arguments, int timeoutMs, out string error)
        {
            error = "";
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = _adbPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using (Process p = new Process())
                {
                    p.StartInfo = psi;
                    StringBuilder outSb = new StringBuilder();
                    StringBuilder errSb = new StringBuilder();
                    p.OutputDataReceived += (s, e) => { if (e.Data != null) outSb.AppendLine(e.Data); };
                    p.ErrorDataReceived += (s, e) => { if (e.Data != null) errSb.AppendLine(e.Data); };
                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    if (!p.WaitForExit(timeoutMs))
                    {
                        try { p.Kill(); } catch { /* ignore */ }
                        error = "timeout sau " + timeoutMs + "ms";
                        return null;
                    }
                    error = errSb.ToString().Trim();
                    return outSb.ToString();
                }
            }
            catch (Exception e)
            {
                error = e.Message;
                return null;
            }
        }

        private static string ResolveAdbPath()
        {
            string exe = Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb";

            // 1) SDK do Unity cấu hình (reflection -> không cần ref assembly Android)
            string p = CombineAdb(TryGetUnityAndroidSdk(), exe);
            if (p != null) return p;

            // 2) EditorPrefs
            p = CombineAdb(EditorPrefs.GetString("AndroidSdkRoot"), exe);
            if (p != null) return p;

            // 3) Biến môi trường
            p = CombineAdb(Environment.GetEnvironmentVariable("ANDROID_HOME"), exe);
            if (p != null) return p;
            p = CombineAdb(Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"), exe);
            if (p != null) return p;

            // 4) Giả định adb có trong PATH
            return exe;
        }

        private static string CombineAdb(string sdkRoot, string exe)
        {
            if (string.IsNullOrEmpty(sdkRoot)) return null;
            string candidate = Path.Combine(sdkRoot, Path.Combine("platform-tools", exe));
            return File.Exists(candidate) ? candidate : null;
        }

        private static string TryGetUnityAndroidSdk()
        {
            try
            {
                Type t = Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
                if (t != null)
                {
                    var prop = t.GetProperty("sdkRootPath",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (prop != null)
                    {
                        object val = prop.GetValue(null, null);
                        string s = val as string;
                        if (!string.IsNullOrEmpty(s)) return s;
                    }
                }
            }
            catch { /* ignore */ }
            return null;
        }
    }
}
#endif
