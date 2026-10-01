using System;
using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal sealed class BuildToolWindowV2 : EditorWindow
    {
        internal sealed class RuntimeSdkState
        {
            public string MaxUnityVersion = "Unknown";
            public string MaxAndroidVersion = "Unknown";
            public string MaxIosVersion = "Unknown";
            public string AdmobUnityVersion = "Unknown";
            public string AdmobAndroidVersion = "Unknown";
            public string AdmobIosVersion = "Unknown";
            public string AdjustSdkVersion = "Unknown";
            public string[] MaxAdapters = Array.Empty<string>();
            public string[] AdmobAdapters = Array.Empty<string>();
        }

        internal sealed class AdCoreInfoState
        {
            public string Name = "-";
            public string InitAtAwake = "OFF";
            public int DefaultConfigsLength;
        }

        internal sealed class CustomConfigInfoState
        {
            public string Key = "-";
            public int ContentLength;
        }

        internal sealed class HeaderOverviewState
        {
            public string Preset = "-";
            public string Target = "-";
            public string BuildName = "-";
            public string AdjustEnvironment = "-";
            public string OutputDirectory = "-";
            public bool HasOutputDirectory;
            public string ConfigsName = "-";
            public string Hack = "-";
            public string AdmobTestDevice = "-";
            public string AdmobTestIds = "-";
            public string DebugPreset = "-";
            public string TrackingFirebase = "-";
            public string DebugUiKeepLandscape = "-";
            public string SrDebugger = "-";
            public NetConfigsSO NetConfigs;
            public bool HasManagedSourceMismatch;
            public string[] ManagedSourceMismatchSummaries = Array.Empty<string>();
        }

        internal sealed class BuildScenesState
        {
            public string PrimaryScenePath = string.Empty;
            public string PrimarySceneLabel = "(none)";
            public EditorBuildSettingsScene[] Scenes = Array.Empty<EditorBuildSettingsScene>();
        }

        internal const string SessionProfileGuidKey = "BG_BuildToolV2_SelectedProfileGuid";
        internal const string SessionDeviceSerialKey = "BG_BuildToolV2_SelectedDeviceSerial";
        internal const string PresetRulesFoldoutKey = "BG_BuildToolV2_Foldout_PresetRules";
        internal const string ValidationScanFoldoutKey = "BG_BuildToolV2_Foldout_ValidationScan";
        internal const string BuildAreaDeviceFoldoutKey = "BG_BuildToolV2_Foldout_BuildAreaDevice";
        internal const string BuildAreaNetworkFoldoutKey = "BG_BuildToolV2_Foldout_BuildAreaNetwork";
        internal const string BuildAreaActionsFoldoutKey = "BG_BuildToolV2_Foldout_BuildAreaActions";
        internal const string BuildAreaInfoFoldoutKey = "BG_BuildToolV2_Foldout_BuildAreaInfo";
        internal BuildProfileV2[] availableProfiles = Array.Empty<BuildProfileV2>();
        internal BuildProfileV2 profile;
        internal BuildValidationResult validationResult;
        internal bool validationScanStale;
        internal BuildExecutionResult lastExecutionResult;
        internal BuildAndroidDeviceInfo[] connectedDevices = Array.Empty<BuildAndroidDeviceInfo>();
        internal RuntimeSdkState runtimeSdkState;
        internal AdCoreInfoState[] adCoreInfoStates = Array.Empty<AdCoreInfoState>();
        internal CustomConfigInfoState[] customConfigInfoStates = Array.Empty<CustomConfigInfoState>();
        internal int adsConfigsLength;
        internal HeaderOverviewState headerOverviewState;
        internal BuildScenesState buildScenesState;
        internal BuildToolScrcpySettingsV2 scrcpySettings;
        internal int selectedDeviceIndex = -1;
        private Vector2 scrollPosition;
        internal Vector2 checksScrollPosition;
        internal Vector2 buildScenesScrollPosition;
        internal Vector2 changeLogScrollPosition;
        internal BuildToolSummaryDraftV2 summaryDraft = new BuildToolSummaryDraftV2();
        internal BuildProfileV2 cachedDraftProfile;
        internal string scrcpyMessage = string.Empty;
        internal MessageType scrcpyMessageType = MessageType.None;
        internal Texture2D selectedSourceIcon;
        internal Texture2D adaptiveInsetPreview;
        internal Texture2D adaptiveAppliedPreview;
        internal string selectedSourceIconName = string.Empty;
        internal string adaptiveStatusMessage = "No icon found in Player Settings.";
        internal bool adaptiveStatusApplied;

        [MenuItem("BG/Build Tool V2 #b")]
        public static void Open()
        {
            var window = GetWindow<BuildToolWindowV2>("BG Build Tool V2");
            window.minSize = new Vector2(920f, 720f);
            window.Show();
        }

        private void OnEnable()
        {
            BuildToolWindowV2LifecycleCoordinator.OnEnable(this);
        }

        private void OnFocus()
        {
            Repaint();
        }

        private void OnDisable()
        {
            BuildToolWindowV2IconSupport.DestroyAdaptivePreviewTextures(this);
        }

        private void OnGUI()
        {
            BuildToolWindowV2LifecycleCoordinator.HandleRefreshShortcut(this);
            BuildToolWindowV2IconSupport.HandleDefaultIconObjectPickerEvent(this);
            BuildToolWindowV2TopSection.DrawToolbar(this);
            BuildToolWindowV2TopSection.DrawTopOverviewSection(this);
            BuildToolWindowV2Ui.DrawColoredDivider(new Color(0.85f, 0.58f, 0.18f, 1f), 3f, 0f);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            BuildToolWindowV2TopSection.DrawPresetSectionClean(this);
            BuildToolWindowV2ValidationSection.Draw(this);
            BuildToolWindowV2SummarySection.Draw(this);
            BuildToolWindowV2BuildScenesSection.Draw(this);
            BuildToolWindowV2AdCoreSdkSection.Draw(this);
            BuildToolWindowV2MfuscatorSection.Draw(this);
            EditorGUILayout.EndScrollView();
            BuildToolWindowV2BuildAreaSection.DrawBottomPanel(this);
        }

        internal void DrawPresetRulesCard()
        {
            BuildToolWindowV2TopSection.DrawPresetRulesCard(this);
        }

        internal void DrawHeaderOverviewCard()
        {
            BuildToolWindowV2TopSection.DrawHeaderOverviewCard(this);
        }

        internal void SaveBuildScenes(EditorBuildSettingsScene[] scenes)
        {
            EditorBuildSettings.scenes = scenes ?? Array.Empty<EditorBuildSettingsScene>();
            ReloadBuildScenesState();
            MarkValidationScanStale();
            lastExecutionResult = null;
            Repaint();
        }

        internal void DrawBottomDeviceSection()
        {
            BuildToolWindowV2DeviceSection.Draw(this);
        }

        internal void DrawBottomNetworkSection()
        {
            BuildToolWindowV2NetworkSection.Draw(this);
        }

        internal void DrawProfilePicker()
        {
            BuildToolWindowV2ProfileSelection.DrawProfilePicker(this);
        }

        internal void OpenBuildOutputLocation()
        {
            BuildToolWindowV2BuildAreaSection.OpenBuildOutputLocation(this);
        }

        internal void RefreshChecks()
        {
            BuildToolWindowV2ValidationSection.RefreshChecks(this);
        }

        internal void MarkValidationScanStale(bool clearResult = false)
        {
            BuildToolWindowV2ValidationSection.MarkScanStale(this, clearResult);
        }

        internal void ReloadHeaderOverviewState()
        {
            BuildToolWindowV2DisplayState.ReloadHeaderOverviewState(this);
        }

        internal void ReloadBuildScenesState()
        {
            BuildToolWindowV2DisplayState.ReloadBuildScenesState(this);
        }

        internal void ReloadDisplayState(bool reloadSdk)
        {
            BuildToolWindowV2DisplayState.ReloadDisplayState(this, reloadSdk);
        }

        internal void GenerateReport()
        {
            BuildToolWindowV2ReportActions.GenerateReport(this);
        }

        internal void RunBuild(bool buildAndRun)
        {
            BuildToolWindowV2BuildActions.RunBuild(this, buildAndRun);
        }

        internal string ChooseOutputDirectory()
        {
            return BuildToolWindowV2ReportActions.ChooseOutputDirectory(this);
        }

        internal void RefreshProfileCache()
        {
            BuildToolWindowV2ProfileSelection.RefreshProfileCache(this);
        }

        internal void EnsureDefaultProfiles()
        {
            BuildToolWindowV2ProfileSelection.EnsureDefaultProfiles(this);
        }

        internal void RestoreSelectedProfile()
        {
            BuildToolWindowV2ProfileSelection.RestoreSelectedProfile(this);
        }

        internal void RefreshDeviceCache()
        {
            BuildToolWindowV2DeviceSection.RefreshDeviceCache(this);
        }

        internal void RefreshSelectedDeviceNetwork()
        {
            BuildToolWindowV2DeviceSection.RefreshSelectedDeviceNetwork(this);
        }

        internal void RefreshScrcpySettings()
        {
            BuildToolWindowV2DeviceSection.RefreshScrcpySettings(this);
        }

        internal void SaveScrcpySettings()
        {
            BuildToolWindowV2DeviceSection.SaveScrcpySettings(this);
        }

        internal void OpenScrcpyPreview(BuildAndroidDeviceInfo selectedDevice, bool restartIfRunning)
        {
            BuildToolWindowV2DeviceSection.OpenScrcpyPreview(this, selectedDevice, restartIfRunning);
        }

        internal void SetScrcpyMessage(string message, MessageType type)
        {
            BuildToolWindowV2DeviceSection.SetScrcpyMessage(this, message, type);
        }

        internal BuildAndroidDeviceInfo GetSelectedDevice()
        {
            return BuildToolWindowV2DeviceSection.GetSelectedDevice(this);
        }

        internal void RefreshSummaryDraft(bool force = false)
        {
            BuildToolWindowV2LifecycleCoordinator.RefreshSummaryDraft(this, force);
        }

        internal void RefreshWindowData(bool refreshSdk, bool refreshDevices = true, bool refreshValidation = false)
        {
            BuildToolWindowV2LifecycleCoordinator.RefreshWindowData(this, refreshSdk, refreshDevices, refreshValidation);
        }

        internal void RefreshIconSelectionFromCurrent(bool force = false)
        {
            BuildToolWindowV2IconSupport.RefreshIconSelectionFromCurrent(this, force);
        }

        internal void ReloadRuntimeSdkState()
        {
            BuildToolWindowV2AdCoreSdkSection.ReloadRuntimeSdkState(this);
        }

        internal void ReloadAdCoreInfoState()
        {
            BuildToolWindowV2AdCoreSdkSection.ReloadAdCoreInfoState(this);
        }
    }
}

