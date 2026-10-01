using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using SRDebugger;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    public enum BuildProfilePresetV2
    {
        TestDebug,
        TestRelease,
        Release,
    }

    public enum BuildProfileAdmobIdModeV2
    {
        RealIds,
        TestIds,
    }

    [CreateAssetMenu(fileName = "Build Profile", menuName = "BG_Library/Build/Profile")]
    public sealed class BuildProfileV2 : ScriptableObject
    {
        [SerializeField] private string profileDisplayName = "";
        [SerializeField] private BuildProfilePresetV2 preset = BuildProfilePresetV2.TestDebug;
        [SerializeField] private BuildTarget buildTarget = BuildTarget.Android;
        [SerializeField] private bool applyNetSetupBeforeBuild = true;
        [SerializeField] private bool openOutputFolderAfterBuild = true;
        [SerializeField] private bool generateReportArtifacts = true;
        [SerializeField] private NetConfigsSO netConfigsOverride;
        [SerializeField] private SdkVersionPresetV2 sdkVersionPreset;
        [SerializeField] private bool ignoreAdmobValidation;
        [SerializeField] private bool ignoreAdjustValidation;
        [SerializeField] private bool ignoreMaxValidation;

        public string DisplayName => string.IsNullOrWhiteSpace(profileDisplayName) ? name : profileDisplayName.Trim();
        public BuildProfilePresetV2 Preset => preset;
        public BuildTarget BuildTarget => buildTarget;
        public bool BuildAppBundle => buildTarget == BuildTarget.Android && preset == BuildProfilePresetV2.Release;
        public bool DevelopmentBuild => GetCurrentDevelopmentBuild();
        public bool ApplyNetSetupBeforeBuild => applyNetSetupBeforeBuild;
        public bool OpenOutputFolderAfterBuild => openOutputFolderAfterBuild;
        public bool GenerateReportArtifacts => generateReportArtifacts;
        public bool BuildHack => GetPresetBuildHack();
        public string OutputDirectory => GetLocalState().outputDirectory ?? "";
        public string ChangeLog => GetLocalState().changeLog ?? "";
        public SdkVersionPresetV2 SdkVersionPreset => sdkVersionPreset;
        public string KeystorePassword => GetCurrentKeystorePassword();
        public string ProductName => PlayerSettings.productName ?? string.Empty;
        public string CompanyName => PlayerSettings.companyName ?? string.Empty;
        public string Version => PlayerSettings.bundleVersion ?? string.Empty;
        public int VersionCode => GetCurrentVersionCode();
        public string PackageName => GetCurrentPackageName();
        public bool SplitApplicationBinary => GetCurrentSplitApplicationBinary();
        public bool IgnoreAdmobValidation => ignoreAdmobValidation;
        public bool IgnoreAdjustValidation => ignoreAdjustValidation;
        public bool IgnoreMaxValidation => ignoreMaxValidation;
        public bool RequiresChangeLog => preset != BuildProfilePresetV2.TestDebug;
        public bool UsesSandboxAdjust => preset == BuildProfilePresetV2.TestDebug;
        public bool UsesDebugMode => NetFlowDebugSystem.IsEnabledForPreset(DebugPreset);
        public bool IsReleaseLikePreset => preset == BuildProfilePresetV2.TestRelease || preset == BuildProfilePresetV2.Release;
        public bool ReleaseDebugEnabled => IsReleaseLikePreset && GetLocalState().releaseDebugEnabled;
        public bool RequiresDebugOverlayCanvas => preset == BuildProfilePresetV2.TestDebug || ReleaseDebugEnabled;
        public bool ShouldEnableSRDebugger => RequiresDebugOverlayCanvas;
        public bool IsReleaseBuildWithDebugWarning => IsReleaseLikePreset && UsesDebugMode;
        public bool UsesAdmobTestDevice => GetPresetAdmobTestDevice();
        public bool UsesAdmobTestIds => TestDebugAdmobIdMode == BuildProfileAdmobIdModeV2.TestIds;
        public BuildProfileAdmobIdModeV2 TestDebugAdmobIdMode => GetPresetAdmobIdMode();
        public DebugPreset DebugPreset => GetEffectiveDebugPreset();
        public bool PresetDevelopmentBuild => GetPresetDevelopmentBuild();
        public bool PresetBuildHack => GetPresetBuildHack();
        public DebugPreset PresetDebugPreset => GetPresetDebugPreset();
        public bool PresetUsesAdmobTestDevice => GetPresetAdmobTestDevice();
        public BuildProfileAdmobIdModeV2 PresetAdmobIdMode => GetPresetAdmobIdMode();
        public NetConfigsSO ResolveNetConfigs()
        {
            return netConfigsOverride != null ? netConfigsOverride : NetConfigsSO.Ins;
        }

        public string ResolveVersion()
        {
            return (Version ?? string.Empty).Trim();
        }

        public void SetOutputDirectory(string directory)
        {
            BuildProfileLocalStateV2 state = GetLocalState();
            state.outputDirectory = directory ?? "";
            BuildToolV2LocalStateStore.SaveState(this, state);
        }

        public void SetChangeLog(string value)
        {
            BuildProfileLocalStateV2 state = GetLocalState();
            state.changeLog = value ?? string.Empty;
            BuildToolV2LocalStateStore.SaveState(this, state);
        }

        public void SetSdkVersionPreset(SdkVersionPresetV2 value)
        {
            if (sdkVersionPreset == value)
                return;

            sdkVersionPreset = value;
            EditorUtility.SetDirty(this);
        }

        public void SetKeystorePassword(string value)
        {
            string password = value ?? string.Empty;
            PlayerSettings.Android.useCustomKeystore = !string.IsNullOrWhiteSpace(password) || PlayerSettings.Android.useCustomKeystore;
            PlayerSettings.Android.keystorePass = password;
            PlayerSettings.Android.keyaliasPass = password;
        }

        public void SetDevelopmentBuildValue(bool value)
        {
            EditorUserBuildSettings.development = value;
        }

        public void ResetManagedOverridesToPreset()
        {
            BuildProfileLocalStateV2 state = GetLocalState();
            state.releaseDebugEnabled = false;
            BuildToolV2LocalStateStore.SaveState(this, state);
        }

        public void SetReleaseDebugEnabled(bool enabled)
        {
            if (!IsReleaseLikePreset)
                return;

            BuildProfileLocalStateV2 state = GetLocalState();
            state.releaseDebugEnabled = enabled;
            BuildToolV2LocalStateStore.SaveState(this, state);
        }

        public void SetProductName(string value)
        {
            PlayerSettings.productName = value ?? string.Empty;
        }

        public void SetCompanyName(string value)
        {
            PlayerSettings.companyName = value ?? string.Empty;
        }

        public void SetVersion(string value)
        {
            PlayerSettings.bundleVersion = value ?? string.Empty;
        }

        public void SetVersionCode(int value)
        {
            PlayerSettings.Android.bundleVersionCode = Mathf.Max(1, value);
        }

        public void SetPackageName(string value)
        {
            string packageName = value ?? string.Empty;
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(buildTarget));
            PlayerSettings.SetApplicationIdentifier(namedBuildTarget, packageName);
        }

        public void SetSplitApplicationBinary(bool value)
        {
            if (buildTarget != BuildTarget.Android)
                return;

            BuildToolV2Utilities.SetAndroidSplitApplicationBinary(value);
        }

        public void ConfigurePreset(BuildProfilePresetV2 newPreset, BuildTarget target, string displayName)
        {
            string normalizedDisplayName = displayName ?? "";
            bool presetChanged = preset != newPreset;
            bool targetChanged = buildTarget != target;
            bool displayNameChanged = !string.Equals(profileDisplayName, normalizedDisplayName, StringComparison.Ordinal);

            preset = newPreset;
            buildTarget = target;
            profileDisplayName = normalizedDisplayName;

            if (!presetChanged && !targetChanged && !displayNameChanged)
                return;

            if (presetChanged || targetChanged)
                ResetManagedOverridesToPreset();

            EditorUtility.SetDirty(this);
        }

        internal BuildProfileManagedSourceState GetExpectedManagedSourceState()
        {
            return new BuildProfileManagedSourceState
            {
                BuildHack = GetPresetBuildHack(),
                AdmobTestDevice = GetPresetAdmobTestDevice(),
                AdmobTestIds = GetPresetAdmobIdMode() == BuildProfileAdmobIdModeV2.TestIds,
                DebugPreset = GetEffectiveDebugPreset(),
                TrackingFirebase = BuildAppBundle,
                SrDebuggerEnabled = ShouldEnableSRDebugger,
            };
        }

        internal BuildProfileManagedSourceState GetCurrentManagedSourceState()
        {
            NetConfigsSO netConfigs = ResolveNetConfigs();
            Settings srDebuggerSettings = Resources.Load<Settings>("SRDebugger/Settings");
            return new BuildProfileManagedSourceState
            {
                BuildHack = netConfigs != null && netConfigs.Build_Hack,
                AdmobTestDevice = netConfigs != null && netConfigs.Admob_TestDevice,
                AdmobTestIds = netConfigs != null && netConfigs.Admob_TestId,
                DebugPreset = netConfigs != null ? netConfigs.Debug_Preset : DebugPreset.Off,
                TrackingFirebase = netConfigs != null && netConfigs.Tracking_SendToFirebase,
                SrDebuggerEnabled = srDebuggerSettings != null && srDebuggerSettings.IsEnabled,
            };
        }

        public bool HasManagedSourceMismatches()
        {
            BuildProfileManagedSourceState expected = GetExpectedManagedSourceState();
            BuildProfileManagedSourceState current = GetCurrentManagedSourceState();
            return !expected.Equals(current);
        }

        public string[] GetManagedSourceMismatchSummaries()
        {
            var summaries = new List<string>(6);
            BuildProfileManagedSourceState expected = GetExpectedManagedSourceState();
            BuildProfileManagedSourceState current = GetCurrentManagedSourceState();

            AppendMismatchSummary(summaries, "Hack", expected.BuildHack, current.BuildHack);
            AppendMismatchSummary(summaries, "AdMob Test Device", expected.AdmobTestDevice, current.AdmobTestDevice);
            AppendMismatchSummary(summaries, "AdMob Test IDs", expected.AdmobTestIds, current.AdmobTestIds);
            AppendMismatchSummary(summaries, "Debug Preset", expected.DebugPreset, current.DebugPreset);
            AppendMismatchSummary(summaries, "Tracking Firebase", expected.TrackingFirebase, current.TrackingFirebase);
            AppendMismatchSummary(summaries, "SRDebugger", expected.SrDebuggerEnabled, current.SrDebuggerEnabled);

            return summaries.ToArray();
        }

        private BuildProfileLocalStateV2 GetLocalState()
        {
            BuildProfileLocalStateV2 state = BuildToolV2LocalStateStore.GetState(this);
            return state;
        }

        private DebugPreset GetEffectiveDebugPreset()
        {
            if (preset == BuildProfilePresetV2.TestDebug)
                return DebugPreset.FullDebug;

            if (IsReleaseLikePreset && GetLocalState().releaseDebugEnabled)
                return DebugPreset.FullDebug;

            return DebugPreset.Off;
        }

        private static bool GetCurrentDevelopmentBuild()
        {
            try
            {
                return EditorUserBuildSettings.development;
            }
            catch
            {
                return false;
            }
        }

        private DebugPreset GetPresetDebugPreset()
        {
            return preset == BuildProfilePresetV2.TestDebug
                ? DebugPreset.FullDebug
                : DebugPreset.Off;
        }

        private bool GetPresetAdmobTestDevice()
        {
            return preset == BuildProfilePresetV2.TestDebug;
        }

        private BuildProfileAdmobIdModeV2 GetPresetAdmobIdMode()
        {
            return preset == BuildProfilePresetV2.TestDebug
                ? BuildProfileAdmobIdModeV2.TestIds
                : BuildProfileAdmobIdModeV2.RealIds;
        }

        private bool GetPresetBuildHack()
        {
            return preset == BuildProfilePresetV2.TestDebug;
        }

        private bool GetPresetDevelopmentBuild()
        {
            return false;
        }

        private static int GetCurrentVersionCode()
        {
            try
            {
                return Mathf.Max(1, PlayerSettings.Android.bundleVersionCode);
            }
            catch
            {
                return 1;
            }
        }

        private string GetCurrentPackageName()
        {
            try
            {
                NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(buildTarget));
                return PlayerSettings.GetApplicationIdentifier(namedBuildTarget) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private bool GetCurrentSplitApplicationBinary()
        {
            try
            {
                return buildTarget == BuildTarget.Android && BuildToolV2Utilities.GetAndroidSplitApplicationBinary();
            }
            catch
            {
                return false;
            }
        }

        private static string GetCurrentKeystorePassword()
        {
            try
            {
                return PlayerSettings.Android.keystorePass ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void AppendMismatchSummary<T>(List<string> summaries, string label, T expected, T current)
        {
            if (EqualityComparer<T>.Default.Equals(expected, current))
                return;

            summaries.Add($"{label}: expected {expected}, current {current}");
        }

    }

    internal struct BuildProfileManagedSourceState : IEquatable<BuildProfileManagedSourceState>
    {
        public bool BuildHack;
        public bool AdmobTestDevice;
        public bool AdmobTestIds;
        public DebugPreset DebugPreset;
        public bool TrackingFirebase;
        public bool SrDebuggerEnabled;

        public bool Equals(BuildProfileManagedSourceState other)
        {
            return BuildHack == other.BuildHack
                && AdmobTestDevice == other.AdmobTestDevice
                && AdmobTestIds == other.AdmobTestIds
                && DebugPreset == other.DebugPreset
                && TrackingFirebase == other.TrackingFirebase
                && SrDebuggerEnabled == other.SrDebuggerEnabled;
        }

        public override bool Equals(object obj)
        {
            return obj is BuildProfileManagedSourceState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = BuildHack ? 1 : 0;
                hash = (hash * 397) ^ (AdmobTestDevice ? 1 : 0);
                hash = (hash * 397) ^ (AdmobTestIds ? 1 : 0);
                hash = (hash * 397) ^ (int)DebugPreset;
                hash = (hash * 397) ^ (TrackingFirebase ? 1 : 0);
                hash = (hash * 397) ^ (SrDebuggerEnabled ? 1 : 0);
                return hash;
            }
        }
    }

    [Serializable]
    internal sealed class BuildProfileLocalStateV2
    {
        public bool releaseDebugEnabled;
        public string outputDirectory = string.Empty;
        public string changeLog = string.Empty;
    }

    [Serializable]
    internal sealed class BuildToolSharedLocalStateV2
    {
        public string outputDirectory = string.Empty;
        public string changeLog = string.Empty;
    }

    [Serializable]
    internal enum BuildToolScrcpyPresetV2
    {
        Balanced,
        LowLatency,
        HighQuality,
    }

    [Serializable]
    internal sealed class BuildToolScrcpySettingsV2
    {
        public string customExecutablePath = string.Empty;
        public bool autoOpenAfterBuildAndRun = false;
        public BuildToolScrcpyPresetV2 preset = BuildToolScrcpyPresetV2.Balanced;
        public int startupDelayMs = 1500;
        public bool turnScreenOff = false;
        public bool stayAwake = true;

        public BuildToolScrcpySettingsV2 Clone()
        {
            return new BuildToolScrcpySettingsV2
            {
                customExecutablePath = customExecutablePath,
                autoOpenAfterBuildAndRun = autoOpenAfterBuildAndRun,
                preset = preset,
                startupDelayMs = startupDelayMs,
                turnScreenOff = turnScreenOff,
                stayAwake = stayAwake,
            };
        }
    }

    internal static class BuildToolV2LocalStateStore
    {
        private static readonly Dictionary<string, BuildProfileLocalStateV2> CachedStates = new Dictionary<string, BuildProfileLocalStateV2>();
        private static BuildToolSharedLocalStateV2 cachedSharedState;
        private static BuildToolScrcpySettingsV2 cachedScrcpySettings;

        public static BuildProfileLocalStateV2 GetState(BuildProfileV2 profile)
        {
            if (profile == null)
                return new BuildProfileLocalStateV2();

            string profileGuid = GetProfileGuid(profile);
            if (string.IsNullOrWhiteSpace(profileGuid))
                return new BuildProfileLocalStateV2();

            if (CachedStates.TryGetValue(profileGuid, out BuildProfileLocalStateV2 cachedState))
                return cachedState;

            string path = GetStatePath(profileGuid);
            BuildProfileLocalStateV2 state = LoadState(path);
            BuildToolSharedLocalStateV2 sharedState = GetSharedState();
            bool sharedStateDirty = false;
            if (string.IsNullOrWhiteSpace(sharedState.outputDirectory) && !string.IsNullOrWhiteSpace(state.outputDirectory))
            {
                sharedState.outputDirectory = state.outputDirectory;
                sharedStateDirty = true;
            }
            else
            {
                state.outputDirectory = sharedState.outputDirectory ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(sharedState.changeLog) && !string.IsNullOrWhiteSpace(state.changeLog))
            {
                sharedState.changeLog = state.changeLog;
                sharedStateDirty = true;
            }
            else
            {
                state.changeLog = sharedState.changeLog ?? string.Empty;
            }

            if (sharedStateDirty)
                SaveSharedState(sharedState);

            CachedStates[profileGuid] = state;
            return state;
        }

        public static void SaveState(BuildProfileV2 profile, BuildProfileLocalStateV2 state)
        {
            if (profile == null || state == null)
                return;

            string profileGuid = GetProfileGuid(profile);
            if (string.IsNullOrWhiteSpace(profileGuid))
                return;

            BuildToolSharedLocalStateV2 sharedState = GetSharedState();
            sharedState.outputDirectory = state.outputDirectory ?? string.Empty;
            sharedState.changeLog = state.changeLog ?? string.Empty;
            SaveSharedState(sharedState);

            string directory = GetStatesDirectory();
            Directory.CreateDirectory(directory);

            string path = GetStatePath(profileGuid);
            var perProfileState = new BuildProfileLocalStateV2
            {
                releaseDebugEnabled = state.releaseDebugEnabled,
                outputDirectory = string.Empty,
                changeLog = string.Empty,
            };
            string json = JsonUtility.ToJson(perProfileState, true);
            File.WriteAllText(path, json);
            CachedStates[profileGuid] = state;
            PropagateSharedFieldsToCachedStates(sharedState, profileGuid);
        }

        private static void PropagateSharedFieldsToCachedStates(BuildToolSharedLocalStateV2 sharedState, string excludeProfileGuid)
        {
            foreach (KeyValuePair<string, BuildProfileLocalStateV2> kvp in CachedStates)
            {
                if (string.Equals(kvp.Key, excludeProfileGuid, StringComparison.Ordinal))
                    continue;

                BuildProfileLocalStateV2 cached = kvp.Value;
                if (cached == null)
                    continue;

                cached.outputDirectory = sharedState.outputDirectory ?? string.Empty;
                cached.changeLog = sharedState.changeLog ?? string.Empty;
            }
        }

        public static BuildToolScrcpySettingsV2 GetScrcpySettings(bool forceReload = false)
        {
            if (forceReload)
                cachedScrcpySettings = null;

            if (cachedScrcpySettings != null)
                return cachedScrcpySettings;

            string path = GetScrcpySettingsPath();
            cachedScrcpySettings = LoadScrcpySettings(path);
            return cachedScrcpySettings;
        }

        public static void SaveScrcpySettings(BuildToolScrcpySettingsV2 settings)
        {
            if (settings == null)
                return;

            string directory = GetStatesDirectory();
            Directory.CreateDirectory(directory);

            string path = GetScrcpySettingsPath();
            string json = JsonUtility.ToJson(settings, true);
            File.WriteAllText(path, json);
            cachedScrcpySettings = settings;
        }

        public static void ClearCache()
        {
            CachedStates.Clear();
            cachedSharedState = null;
            cachedScrcpySettings = null;
        }

        private static BuildProfileLocalStateV2 LoadState(string path)
        {
            if (!File.Exists(path))
                return new BuildProfileLocalStateV2();

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<BuildProfileLocalStateV2>(json) ?? new BuildProfileLocalStateV2();
            }
            catch
            {
                return new BuildProfileLocalStateV2();
            }
        }

        private static BuildToolScrcpySettingsV2 LoadScrcpySettings(string path)
        {
            if (!File.Exists(path))
                return new BuildToolScrcpySettingsV2();

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<BuildToolScrcpySettingsV2>(json) ?? new BuildToolScrcpySettingsV2();
            }
            catch
            {
                return new BuildToolScrcpySettingsV2();
            }
        }

        private static BuildToolSharedLocalStateV2 GetSharedState()
        {
            if (cachedSharedState != null)
                return cachedSharedState;

            string path = GetSharedStatePath();
            cachedSharedState = LoadSharedState(path);
            return cachedSharedState;
        }

        private static void SaveSharedState(BuildToolSharedLocalStateV2 state)
        {
            if (state == null)
                return;

            string directory = GetStatesDirectory();
            Directory.CreateDirectory(directory);

            string path = GetSharedStatePath();
            string json = JsonUtility.ToJson(state, true);
            File.WriteAllText(path, json);
            cachedSharedState = state;
        }

        private static BuildToolSharedLocalStateV2 LoadSharedState(string path)
        {
            if (!File.Exists(path))
                return new BuildToolSharedLocalStateV2();

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<BuildToolSharedLocalStateV2>(json) ?? new BuildToolSharedLocalStateV2();
            }
            catch
            {
                return new BuildToolSharedLocalStateV2();
            }
        }

        private static string GetProfileGuid(BuildProfileV2 profile)
        {
            string assetPath = AssetDatabase.GetAssetPath(profile);
            return string.IsNullOrWhiteSpace(assetPath) ? string.Empty : AssetDatabase.AssetPathToGUID(assetPath);
        }

        private static string GetStatePath(string profileGuid)
        {
            return Path.Combine(GetStatesDirectory(), profileGuid + ".json");
        }

        private static string GetScrcpySettingsPath()
        {
            return Path.Combine(GetStatesDirectory(), "scrcpy-settings.json");
        }

        private static string GetSharedStatePath()
        {
            return Path.Combine(GetStatesDirectory(), "shared-state.json");
        }

        private static string GetStatesDirectory()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            return Path.Combine(projectRoot, "Library", "BG_Lib", "BuildToolV2", "LocalState");
        }
    }
}
