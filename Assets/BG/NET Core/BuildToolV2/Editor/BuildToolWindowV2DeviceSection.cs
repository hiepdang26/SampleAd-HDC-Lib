using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2DeviceSection
    {
        internal static void Draw(BuildToolWindowV2 owner)
        {
            GUILayout.Label("Device", EditorStyles.boldLabel);

            BuildToolWindowV2Ui.DrawSummaryRow("ADB", BuildToolV2Utilities.GetAdbExecutablePath());

            if (owner.profile.BuildTarget != BuildTarget.Android)
            {
                EditorGUILayout.HelpBox("Device controls are only used for Android Build & Run.", MessageType.None);
                return;
            }

            if (owner.connectedDevices == null || owner.connectedDevices.Length == 0)
            {
                EditorGUILayout.HelpBox("No Android device is currently connected.", MessageType.Warning);
                return;
            }

            string[] options = new string[owner.connectedDevices.Length];
            for (int i = 0; i < owner.connectedDevices.Length; i++)
                options[i] = owner.connectedDevices[i].DisplayName;

            int safeIndex = Mathf.Clamp(owner.selectedDeviceIndex, 0, owner.connectedDevices.Length - 1);
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Build & Run Device", safeIndex, options);
            if (EditorGUI.EndChangeCheck())
            {
                owner.selectedDeviceIndex = newIndex;
                PersistSelectedDevice(owner);
            }

            BuildAndroidDeviceInfo selectedDevice = GetSelectedDevice(owner);
            if (selectedDevice == null)
                return;

            BuildToolWindowV2Ui.DrawSummaryRow("Serial", selectedDevice.Serial);
            BuildToolWindowV2Ui.DrawSummaryRow("Model", string.IsNullOrWhiteSpace(selectedDevice.Model) ? "(unknown)" : selectedDevice.Model);
            BuildToolWindowV2Ui.DrawSummaryRow("Android", string.IsNullOrWhiteSpace(selectedDevice.AndroidVersion) ? "(unknown)" : selectedDevice.AndroidVersion);
            BuildToolWindowV2Ui.DrawSummaryRow("SDK", string.IsNullOrWhiteSpace(selectedDevice.SdkInt) ? "(unknown)" : selectedDevice.SdkInt);
            BuildToolWindowV2Ui.DrawSummaryRow("Resolution", string.IsNullOrWhiteSpace(selectedDevice.Resolution) ? "(unknown)" : selectedDevice.Resolution);
            BuildToolWindowV2Ui.DrawSummaryRow("DPI", string.IsNullOrWhiteSpace(selectedDevice.Dpi) ? "(unknown)" : selectedDevice.Dpi);

            EditorGUILayout.Space(6f);
            BuildToolWindowV2Ui.DrawWhiteDivider();
            EditorGUILayout.Space(4f);

            GUILayout.Label("Phone Preview (scrcpy)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Preview settings are stored locally for this machine and project. They are not shared through the submodule.", MessageType.None);

            string scrcpyPath = BuildToolV2Utilities.GetScrcpyExecutablePath(owner.scrcpySettings);
            bool hasScrcpy = BuildToolV2Utilities.HasScrcpyExecutable(owner.scrcpySettings);
            bool isPreviewRunning = BuildToolV2Utilities.IsScrcpyRunning(selectedDevice.Serial);
            bool isPreviewScheduled = BuildToolV2Utilities.IsScrcpyLaunchScheduled(selectedDevice.Serial);
            string previewStatus = !hasScrcpy
                ? "scrcpy.exe not found"
                : isPreviewRunning
                    ? "Running"
                    : isPreviewScheduled
                        ? "Scheduled"
                        : "Ready";

            BuildToolWindowV2Ui.DrawSummaryRow("scrcpy", hasScrcpy ? scrcpyPath : "(missing)");
            BuildToolWindowV2Ui.DrawSummaryRow("Preview Status", previewStatus);

            EditorGUI.BeginChangeCheck();
            BuildToolScrcpyPresetV2 nextPreset = (BuildToolScrcpyPresetV2)EditorGUILayout.EnumPopup("Preview Preset", owner.scrcpySettings.preset);
            if (EditorGUI.EndChangeCheck())
            {
                owner.scrcpySettings.preset = nextPreset;
                SaveScrcpySettings(owner);
            }

            EditorGUILayout.HelpBox(BuildToolV2Utilities.GetScrcpyPresetDescription(owner.scrcpySettings.preset), MessageType.None);

            BuildToolWindowV2Ui.DrawSummaryRow("Custom Path", string.IsNullOrWhiteSpace(owner.scrcpySettings.customExecutablePath) ? "(use PATH)" : owner.scrcpySettings.customExecutablePath);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Browse scrcpy.exe", GUILayout.Width(150f)))
                {
                    string initialDirectory = string.Empty;
                    if (!string.IsNullOrWhiteSpace(owner.scrcpySettings.customExecutablePath))
                        initialDirectory = Path.GetDirectoryName(owner.scrcpySettings.customExecutablePath);

                    string selectedPath = EditorUtility.OpenFilePanel("Select scrcpy.exe", initialDirectory ?? string.Empty, "exe");
                    if (!string.IsNullOrWhiteSpace(selectedPath))
                    {
                        owner.scrcpySettings.customExecutablePath = selectedPath;
                        SaveScrcpySettings(owner);
                    }
                }

                using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(owner.scrcpySettings.customExecutablePath)))
                {
                    if (GUILayout.Button("Clear Custom Path", GUILayout.Width(140f)))
                    {
                        owner.scrcpySettings.customExecutablePath = string.Empty;
                        SaveScrcpySettings(owner);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(owner.scrcpyMessage))
                EditorGUILayout.HelpBox(owner.scrcpyMessage, owner.scrcpyMessageType);
        }

        internal static void RefreshDeviceCache(BuildToolWindowV2 owner)
        {
            owner.connectedDevices = BuildToolV2Utilities.GetConnectedAndroidDevices();
            RestoreSelectedDevice(owner);
            owner.Repaint();
        }

        internal static void RefreshSelectedDeviceNetwork(BuildToolWindowV2 owner)
        {
            BuildAndroidDeviceInfo selectedDevice = GetSelectedDevice(owner);
            if (selectedDevice == null)
                return;

            if (!string.Equals(selectedDevice.State, "device", StringComparison.OrdinalIgnoreCase))
                return;

            BuildToolV2Utilities.RefreshAndroidDeviceNetworkInfo(selectedDevice);
            owner.Repaint();
        }

        internal static void RefreshScrcpySettings(BuildToolWindowV2 owner)
        {
            owner.scrcpySettings = BuildToolV2LocalStateStore.GetScrcpySettings(forceReload: true);
            if (owner.scrcpySettings == null)
                owner.scrcpySettings = new BuildToolScrcpySettingsV2();
            BuildToolV2Utilities.InvalidateScrcpyExecutableCache();
        }

        internal static void SaveScrcpySettings(BuildToolWindowV2 owner)
        {
            if (owner.scrcpySettings == null)
                return;

            BuildToolV2LocalStateStore.SaveScrcpySettings(owner.scrcpySettings);
        }

        internal static void OpenScrcpyPreview(BuildToolWindowV2 owner, BuildAndroidDeviceInfo selectedDevice, bool restartIfRunning)
        {
            if (selectedDevice == null)
            {
                SetScrcpyMessage(owner, "No Android device is selected.", MessageType.Warning);
                return;
            }

            if (!BuildToolV2Utilities.LaunchScrcpy(selectedDevice, owner.scrcpySettings, restartIfRunning, out string message))
            {
                SetScrcpyMessage(owner, message, MessageType.Warning);
                return;
            }

            SetScrcpyMessage(owner, message, MessageType.Info);
        }

        internal static void SetScrcpyMessage(BuildToolWindowV2 owner, string message, MessageType type)
        {
            owner.scrcpyMessage = message ?? string.Empty;
            owner.scrcpyMessageType = type;
            owner.Repaint();
        }

        internal static void PersistSelectedDevice(BuildToolWindowV2 owner)
        {
            BuildAndroidDeviceInfo selected = GetSelectedDevice(owner);
            if (selected == null)
            {
                SessionState.EraseString(BuildToolWindowV2.SessionDeviceSerialKey);
                return;
            }

            SessionState.SetString(BuildToolWindowV2.SessionDeviceSerialKey, selected.Serial ?? string.Empty);
        }

        internal static void RestoreSelectedDevice(BuildToolWindowV2 owner)
        {
            if (owner.connectedDevices == null || owner.connectedDevices.Length == 0)
            {
                owner.selectedDeviceIndex = -1;
                return;
            }

            string serial = SessionState.GetString(BuildToolWindowV2.SessionDeviceSerialKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(serial))
            {
                for (int i = 0; i < owner.connectedDevices.Length; i++)
                {
                    if (string.Equals(owner.connectedDevices[i].Serial, serial, StringComparison.Ordinal))
                    {
                        owner.selectedDeviceIndex = i;
                        return;
                    }
                }
            }

            owner.selectedDeviceIndex = 0;
        }

        internal static BuildAndroidDeviceInfo GetSelectedDevice(BuildToolWindowV2 owner)
        {
            if (owner.connectedDevices == null || owner.connectedDevices.Length == 0)
                return null;
            if (owner.selectedDeviceIndex < 0 || owner.selectedDeviceIndex >= owner.connectedDevices.Length)
                return null;
            return owner.connectedDevices[owner.selectedDeviceIndex];
        }
    }
}
