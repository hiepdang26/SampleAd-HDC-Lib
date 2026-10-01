using System.Text.RegularExpressions;
using BG_Library.Common;
using BG_Library.NET.AdSystem;
using AdjustSdk;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2Utilities
    {
        static BuildToolV2Utilities()
        {
            EditorApplication.projectChanged += ClearEditorCaches;
        }

        public static void ClearEditorCaches()
        {
            BuildToolV2AssetLookup.ClearCaches();
            BuildToolV2ExecutablePaths.ClearCaches();
            BuildToolV2IconTools.ClearCaches();
            BuildToolV2ProfileAssets.ClearCaches();
            BuildToolV2SdkInspection.ClearCaches();
        }

        public static void ClearPrimaryIconCache()
        {
            BuildToolV2IconTools.ClearCaches();
        }

        public static string ExtractDigits(string input)
        {
            return Regex.Replace(input ?? string.Empty, "[^0-9]", "");
        }

        public static string GetTargetDisplayName(BuildTarget target)
        {
            return target.ToString();
        }

        public static string GetBuildDisplayName(BuildProfileV2 profile, NetConfigsSO netConfigs)
        {
            return BuildToolV2OutputTools.GetBuildDisplayName(profile, netConfigs);
        }

        public static void OpenFolderInExplorer(string directory)
        {
            BuildToolV2OutputTools.OpenFolderInExplorer(directory);
        }

        public static void OpenAndSelectPath(string path)
        {
            BuildToolV2OutputTools.OpenAndSelectPath(path);
        }

        public static void PingObject(UnityEngine.Object target)
        {
            BuildToolV2EditorNavigation.PingObject(target);
        }

        public static void OpenLockedInspector(UnityEngine.Object target)
        {
            BuildToolV2EditorNavigation.OpenLockedInspector(target);
        }

        public static void PingAssetPath(string assetPath)
        {
            BuildToolV2EditorNavigation.PingAssetPath(assetPath);
        }

        public static string ChooseOutputDirectory(BuildProfileV2 profile)
        {
            return BuildToolV2OutputTools.ChooseOutputDirectory(profile);
        }

        public static string GetOutputPath(BuildProfileV2 profile, NetConfigsSO netConfigs)
        {
            return BuildToolV2OutputTools.GetOutputPath(profile, netConfigs);
        }

        public static bool HasGoogleMobileAdsSettings()
        {
            return !string.IsNullOrWhiteSpace(GetGoogleMobileAdsSettingsAssetPath());
        }

        public static string GetAdbExecutablePath()
        {
            return BuildToolV2ExecutablePaths.GetAdbExecutablePath();
        }

        public static string GetScrcpyExecutablePath(BuildToolScrcpySettingsV2 settings = null)
        {
            return BuildToolV2ExecutablePaths.GetScrcpyExecutablePath(settings);
        }

        public static bool HasScrcpyExecutable(BuildToolScrcpySettingsV2 settings = null)
        {
            return BuildToolV2ExecutablePaths.HasScrcpyExecutable(settings);
        }

        public static string GetNetworkShareToolPath(BuildToolScrcpySettingsV2 settings = null)
        {
            return BuildToolV2ExecutablePaths.GetNetworkShareToolPath(settings);
        }

        public static void InvalidateScrcpyExecutableCache()
        {
            BuildToolV2ExecutablePaths.InvalidateScrcpyExecutableCache();
        }

        public static bool IsScrcpyRunning(string serial)
        {
            return BuildToolV2ScrcpyLifecycle.IsRunning(serial);
        }

        public static bool IsScrcpyLaunchScheduled(string serial)
        {
            return BuildToolV2ScrcpyLifecycle.IsLaunchScheduled(serial);
        }

        public static bool StopScrcpy(string serial)
        {
            return BuildToolV2ScrcpyLifecycle.Stop(serial);
        }

        public static bool LaunchScrcpy(BuildAndroidDeviceInfo device, BuildToolScrcpySettingsV2 settings, bool restartIfRunning, out string message)
        {
            return BuildToolV2ScrcpyLifecycle.Launch(device, settings, restartIfRunning, out message);
        }

        public static void ScheduleScrcpyLaunch(BuildAndroidDeviceInfo device, BuildToolScrcpySettingsV2 settings, bool restartIfRunning)
        {
            BuildToolV2ScrcpyLifecycle.ScheduleLaunch(device, settings, restartIfRunning);
        }

        public static BuildAndroidDeviceInfo FindConnectedAndroidDevice(string serial)
        {
            return BuildToolV2AndroidDeviceDiscovery.FindConnectedAndroidDevice(serial);
        }

        public static BuildAndroidDeviceInfo[] GetConnectedAndroidDevices()
        {
            return BuildToolV2AndroidDeviceDiscovery.GetConnectedAndroidDevices();
        }

        public static void RefreshAndroidDeviceNetworkInfo(BuildAndroidDeviceInfo device)
        {
            BuildToolV2AndroidNetworkInfo.RefreshAndroidDeviceNetworkInfo(device);
        }

        public static string GetApplicationIdentifier(BuildTarget target)
        {
            return BuildToolV2AndroidAppActions.GetApplicationIdentifier(target);
        }

        public static bool InstallBuildArtifact(BuildAndroidDeviceInfo device, string buildOutputPath, out string message)
        {
            return BuildToolV2AndroidAppActions.InstallBuildArtifact(device, buildOutputPath, out message);
        }

        public static bool UninstallApplication(BuildAndroidDeviceInfo device, string packageName, out string message)
        {
            return BuildToolV2AndroidAppActions.UninstallApplication(device, packageName, out message);
        }

        public static bool LaunchApplication(BuildAndroidDeviceInfo device, string packageName, out string message)
        {
            return BuildToolV2AndroidAppActions.LaunchApplication(device, packageName, out message);
        }

        public static bool ForceStopApplication(BuildAndroidDeviceInfo device, string packageName, out string message)
        {
            return BuildToolV2AndroidAppActions.ForceStopApplication(device, packageName, out message);
        }

        public static bool ClearApplicationData(BuildAndroidDeviceInfo device, string packageName, out string message)
        {
            return BuildToolV2AndroidAppActions.ClearApplicationData(device, packageName, out message);
        }

        public static bool TurnDeviceScreenOff(BuildAndroidDeviceInfo device, out string message)
        {
            return BuildToolV2AndroidAppActions.TurnDeviceScreenOff(device, out message);
        }

        public static bool ShareDeviceNetwork(BuildAndroidDeviceInfo device, BuildToolScrcpySettingsV2 settings, out string message)
        {
            return BuildToolV2NetworkShareAction.ShareDeviceNetwork(device, settings, out message);
        }

        public static bool StopSharedDeviceNetwork(BuildAndroidDeviceInfo device, BuildToolScrcpySettingsV2 settings, out string message)
        {
            return BuildToolV2NetworkShareAction.StopSharedDeviceNetwork(device, settings, out message);
        }

        public static (string androidId, string iosId) GetAdmobAppIds()
        {
            return BuildToolV2SdkInspection.GetAdmobAppIds();
        }

        public static (string androidVersion, string iosVersion) GetGoogleMobileAdsVersionInfo()
        {
            return BuildToolV2SdkInspection.GetGoogleMobileAdsVersionInfo();
        }

        public static (string androidVersion, string iosVersion) GetAppLovinVersionInfo()
        {
            return BuildToolV2SdkInspection.GetAppLovinVersionInfo();
        }

        public static string[] GetAdmobMediationAdapterSummaries()
        {
            return BuildToolV2SdkInspection.GetAdmobMediationAdapterSummaries();
        }

        public static string[] GetMaxMediationAdapterSummaries()
        {
            return BuildToolV2SdkInspection.GetMaxMediationAdapterSummaries();
        }

        public static string[] GetFirebaseModuleSummaries()
        {
            return BuildToolV2SdkInspection.GetFirebaseModuleSummaries();
        }

        public static string GetLocalIpAddress()
        {
            return BuildToolV2AndroidNetworkInfo.GetLocalIpAddress();
        }

        public static string GetAdjustSdkVersion()
        {
            return BuildToolV2SdkInspection.GetAdjustSdkVersion();
        }

        public static string GetGoogleMobileAdsUnityPluginVersion()
        {
            return BuildToolV2SdkInspection.GetGoogleMobileAdsUnityPluginVersion();
        }

        public static string GetAppLovinMaxUnityPluginVersion()
        {
            return BuildToolV2SdkInspection.GetAppLovinMaxUnityPluginVersion();
        }

        public static SdkVersionInspectionEntryV2[] GetSdkVersionInspectionEntries(BuildProfileV2 profile)
        {
            return BuildToolV2SdkInspection.GetSdkVersionInspectionEntries(profile);
        }

        public static SdkVersionPresetComparisonEntryV2[] CompareSdkVersionPreset(BuildProfileV2 profile, SdkVersionPresetV2 preset)
        {
            return BuildToolV2SdkInspection.CompareWithSdkPreset(profile, preset);
        }

        public static AdjustSceneSnapshot GetAdjustSceneSnapshot(BuildProfileV2 profile)
        {
            var snapshot = new AdjustSceneSnapshot();
            if (profile == null)
                return snapshot;

            using var context = new BuildValidationContext(profile);
            if (!context.TryLoadPrimarySceneForValidation(out _))
                return snapshot;

            Adjust adjust = context.FindFirstInPrimaryScene<Adjust>();
            if (adjust == null)
                return snapshot;

            snapshot.Exists = true;
            snapshot.AppToken = adjust.appToken ?? string.Empty;
            snapshot.Environment = adjust.environment.ToString();
            snapshot.StartManually = adjust.startManually;
            return snapshot;
        }

        public static string GetMaxSdkKey()
        {
            return BuildToolV2SdkInspection.GetMaxSdkKey();
        }

        public static string GetMaxPrivacyPolicyUrl()
        {
            return BuildToolV2SdkInspection.GetMaxPrivacyPolicyUrl();
        }

        public static bool IsMaxTermsAndPrivacyPolicyFlowEnabled()
        {
            return BuildToolV2SdkInspection.IsMaxTermsAndPrivacyPolicyFlowEnabled();
        }

        public static string GetPackageVersion(string packageName)
        {
            return BuildToolV2SdkInspection.GetPackageVersion(packageName);
        }

        public static string GetFirebaseConfigFilePath(BuildTarget target)
        {
            return BuildToolV2SdkInspection.GetFirebaseConfigFilePath(target);
        }

        public static bool HasFirebaseConfigFile(BuildTarget target)
        {
            return BuildToolV2SdkInspection.HasFirebaseConfigFile(target);
        }

        public static string GetFirebaseConfigSummary(BuildTarget target)
        {
            return BuildToolV2SdkInspection.GetFirebaseConfigSummary(target);
        }

        public static void PingFirebaseConfigFile(BuildTarget target)
        {
            BuildToolV2SdkInspection.PingFirebaseConfigFile(target);
        }

        public static void SelectGoogleMobileAdsSettings()
        {
            BuildToolV2SdkInspection.SelectGoogleMobileAdsSettings();
        }

        public static void OpenGoogleMobileAdsSettingsInspector()
        {
            BuildToolV2SdkInspection.OpenGoogleMobileAdsSettingsInspector();
        }

        public static void OpenAppLovinIntegrationManager()
        {
            BuildToolV2SdkInspection.OpenAppLovinIntegrationManager();
        }

        public static Texture2D GetPrimaryAppIcon(BuildTarget target)
        {
            return BuildToolV2IconTools.GetPrimaryAppIcon(target);
        }

        public static void OpenPlayerSettings()
        {
            BuildToolV2EditorNavigation.OpenPlayerSettings();
        }

        public static bool TrySetDefaultAppIcon(Texture2D icon, out string errorMessage)
        {
            return BuildToolV2IconTools.TrySetDefaultAppIcon(icon, out errorMessage);
        }

        public static bool IsAdaptiveIconApplied(Texture2D sourceIcon, out string statusMessage)
        {
            return BuildToolV2IconTools.IsAdaptiveIconApplied(sourceIcon, out statusMessage);
        }


        public static Texture2D CreateAdaptiveInsetPreviewTexture(Texture2D sourceIcon, float safeAreaScale = 0.75f)
        {
            return BuildToolV2IconTools.CreateAdaptiveInsetPreviewTexture(sourceIcon, safeAreaScale);
        }

        public static Texture2D CreateAdaptiveAppliedPreviewTexture(Texture2D sourceIcon, float safeAreaScale = 0.75f)
        {
            return BuildToolV2IconTools.CreateAdaptiveAppliedPreviewTexture(sourceIcon, safeAreaScale);
        }

        public static bool TryGenerateAndApplyAdaptiveIcon(Texture2D sourceIcon, float safeAreaScale, out string errorMessage)
        {
            return BuildToolV2IconTools.TryGenerateAndApplyAdaptiveIcon(sourceIcon, safeAreaScale, out errorMessage);
        }

        public static void CreateBGSetupInCurrentScene()
        {
            BGSetupEditorUtilities.CreateSetupInCurrentScene();
        }

        public static bool TryOpenSceneSingle(string scenePath, out string errorMessage)
        {
            return BuildToolV2SceneNavigation.TryOpenSceneSingle(scenePath, out errorMessage);
        }

        public static bool GetAndroidSplitApplicationBinary()
        {
            return BuildToolV2AndroidPlayerSettings.GetSplitApplicationBinary();
        }

        public static void SetAndroidSplitApplicationBinary(bool value)
        {
            BuildToolV2AndroidPlayerSettings.SetSplitApplicationBinary(value);
        }

        public static string BuildNameFromProduct(string version)
        {
            return BuildToolV2OutputTools.BuildNameFromProduct(version);
        }

        public static BuildProfileV2[] EnsureDefaultProfiles()
        {
            return BuildToolV2ProfileAssets.EnsureDefaultProfiles();
        }

        public static BuildProfileV2[] LoadProfiles()
        {
            return BuildToolV2ProfileAssets.LoadProfiles();
        }

        internal static string GetGoogleMobileAdsSettingsAssetPath()
        {
            return BuildToolV2AssetLookup.GetGoogleMobileAdsSettingsAssetPath();
        }

        internal static string GetGoogleMobileAdsDependenciesPath()
        {
            return BuildToolV2AssetLookup.GetGoogleMobileAdsDependenciesPath();
        }

        internal static string GetMaxDependenciesPath()
        {
            return BuildToolV2AssetLookup.GetMaxDependenciesPath();
        }

        internal static string GetAdjustPackageJsonPath()
        {
            return BuildToolV2AssetLookup.GetAdjustPackageJsonPath();
        }

        internal static string GetAdjustDependenciesPath()
        {
            return BuildToolV2AssetLookup.GetAdjustDependenciesPath();
        }

        internal static string GetAssetFolderPath(string folderName, params string[] requiredPathSegments)
        {
            return BuildToolV2AssetLookup.GetAssetFolderPath(folderName, requiredPathSegments);
        }

        public static BuildExecutionResult RunBuild(BuildProfileV2 profile, bool buildAndRun, string preferredDeviceSerial = null)
        {
            return BuildToolV2BuildExecutor.Run(profile, buildAndRun, preferredDeviceSerial);
        }

        public static bool ApplyNetConfigPresetImmediately(BuildProfileV2 profile)
        {
            if (profile == null)
                return false;

            NetConfigsSO netConfigs = profile.ResolveNetConfigs();
            if (netConfigs == null)
                return false;

            bool changed = BuildToolV2NetConfigPreset.ApplyNow(profile, netConfigs);
            if (changed)
                AssetDatabase.SaveAssetIfDirty(netConfigs);

            return changed;
        }

        public static bool ApplyPresetManagedSourcesImmediately(BuildProfileV2 profile)
        {
            if (profile == null)
                return false;

            BuildToolV2ProfileSettingsApplier.Apply(profile);
            bool netChanged = ApplyNetConfigPresetImmediately(profile);
            bool srDebuggerChanged = BuildToolV2SrDebuggerPreset.ApplyNow(profile);
            return netChanged || srDebuggerChanged;
        }

        public static bool EnsureNetPrefabInCurrentScene(out string message)
        {
            return BuildToolV2SceneSetupActions.EnsureNetPrefabInCurrentScene(out message);
        }

        public static bool EnsureAdjustInCurrentScene(out string message)
        {
            return BuildToolV2SceneSetupActions.EnsureAdjustInCurrentScene(out message);
        }

        public static bool EnsureDebugOverlayInCurrentScene(out string message)
        {
            return BuildToolV2SceneSetupActions.EnsureDebugOverlayInCurrentScene(out message);
        }

        public static bool RemoveDebugOverlayInCurrentScene(out string message)
        {
            return BuildToolV2SceneSetupActions.RemoveDebugOverlayInCurrentScene(out message);
        }

        public static bool MoveEnabledBuildSceneToFirst(string scenePath)
        {
            return BuildToolV2SceneOrdering.MoveEnabledBuildSceneToFirst(scenePath);
        }

        public static string GetSceneDisplayName(string scenePath)
        {
            return BuildToolV2SceneNavigation.GetSceneDisplayName(scenePath);
        }

        public static bool TryPingFirstInCurrentScene<T>(out string errorMessage) where T : Component
        {
            return BuildToolV2SceneNavigation.TryPingFirstInCurrentScene<T>(out errorMessage);
        }

        public static string GetPrimaryEnabledScenePath()
        {
            return BuildToolV2SceneNavigation.GetPrimaryEnabledScenePath();
        }

        internal static string GetFirebaseConfigFileName(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "google-services.json";
                case BuildTarget.iOS:
                    return "GoogleService-Info.plist";
                default:
                    return string.Empty;
            }
        }

        internal static bool TryRunProcess(string fileName, string arguments, out string output)
        {
            return BuildToolV2ProcessRunner.TryRunProcess(fileName, arguments, out output);
        }

        public static string GetScrcpyPresetDescription(BuildToolScrcpyPresetV2 preset)
        {
            return BuildToolV2ScrcpyArguments.GetPresetDescription(preset);
        }

    }

}
