using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using AppLovinMax.Scripts.IntegrationManager.Editor;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SdkInspection
    {
        private static readonly Dictionary<BuildTarget, string> CachedFirebaseConfigPaths = new Dictionary<BuildTarget, string>();
        private static readonly Dictionary<string, string[]> CachedMediationAdapterSummaries = new Dictionary<string, string[]>();
        private static bool hasCachedGoogleMobileAdsVersionInfo;
        private static bool hasCachedAppLovinVersionInfo;
        private static (string androidVersion, string iosVersion) cachedGoogleMobileAdsVersionInfo;
        private static (string androidVersion, string iosVersion) cachedAppLovinVersionInfo;

        public static void ClearCaches()
        {
            CachedFirebaseConfigPaths.Clear();
            CachedMediationAdapterSummaries.Clear();
            hasCachedGoogleMobileAdsVersionInfo = false;
            hasCachedAppLovinVersionInfo = false;
            cachedGoogleMobileAdsVersionInfo = default;
            cachedAppLovinVersionInfo = default;
        }

        public static (string androidId, string iosId) GetAdmobAppIds()
        {
            ScriptableObject settings = Resources.Load<ScriptableObject>("GoogleMobileAdsSettings");
            if (settings == null)
                return (string.Empty, string.Empty);

            var serializedObject = new SerializedObject(settings);
            string androidId = serializedObject.FindProperty("adMobAndroidAppId")?.stringValue ?? string.Empty;
            string iosId = serializedObject.FindProperty("adMobIOSAppId")?.stringValue ?? string.Empty;
            return (androidId, iosId);
        }

        public static (string androidVersion, string iosVersion) GetGoogleMobileAdsVersionInfo()
        {
            if (!hasCachedGoogleMobileAdsVersionInfo)
            {
                string dependencyPath = BuildToolV2Utilities.GetGoogleMobileAdsDependenciesPath();
                cachedGoogleMobileAdsVersionInfo = ReadDependencyVersion(
                    dependencyPath,
                    "//androidPackages/androidPackage[@spec[contains(., 'play-services-ads')]]",
                    "//iosPods/iosPod[@name='Google-Mobile-Ads-SDK']");
                hasCachedGoogleMobileAdsVersionInfo = true;
            }

            return cachedGoogleMobileAdsVersionInfo;
        }

        public static (string androidVersion, string iosVersion) GetAppLovinVersionInfo()
        {
            if (!hasCachedAppLovinVersionInfo)
            {
                string dependencyPath = BuildToolV2Utilities.GetMaxDependenciesPath();
                (string androidVersion, string iosVersion) = ReadDependencyVersion(
                    dependencyPath,
                    "//androidPackages/androidPackage",
                    "//iosPods/iosPod");

                if (androidVersion != "Unknown" || iosVersion != "Unknown")
                {
                    cachedAppLovinVersionInfo = (androidVersion, iosVersion);
                }
                else
                {
                    string packageVersion = GetPackageVersion("com.applovin.mediation.ads");
                    cachedAppLovinVersionInfo = packageVersion == "Unknown"
                        ? ("Unknown", "Unknown")
                        : (packageVersion, packageVersion);
                }

                hasCachedAppLovinVersionInfo = true;
            }

            return cachedAppLovinVersionInfo;
        }

        public static string[] GetAdmobMediationAdapterSummaries()
        {
            string mediationFolder = ResolveMediationFolderPath("Assets/GoogleMobileAds/Mediation", "Mediation", "GoogleMobileAds", "Mediation");
            return string.IsNullOrWhiteSpace(mediationFolder)
                ? System.Array.Empty<string>()
                : GetMediationAdapterSummaries(mediationFolder);
        }

        public static string[] GetMaxMediationAdapterSummaries()
        {
            string mediationFolder = ResolveMediationFolderPath("Assets/MaxSdk/Mediation", "Mediation", "MaxSdk", "Mediation");
            string[] summariesFromAssets = string.IsNullOrWhiteSpace(mediationFolder)
                ? System.Array.Empty<string>()
                : GetMediationAdapterSummaries(mediationFolder);
            if (summariesFromAssets.Length > 0)
                return summariesFromAssets;

            return GetAppLovinAdapterPackageSummaries();
        }

        public static string[] GetFirebaseModuleSummaries()
        {
            string firebaseRoot = BuildToolV2Utilities.GetAssetFolderPath("Editor", "Firebase", "Editor");
            if (!Directory.Exists(firebaseRoot))
                return System.Array.Empty<string>();

            string[] dependencyFiles = Directory.GetFiles(firebaseRoot, "*Dependencies.xml", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path)
                .ToArray();
            var summaries = new List<string>(dependencyFiles.Length);

            for (int i = 0; i < dependencyFiles.Length; i++)
            {
                string filePath = dependencyFiles[i];
                string moduleName = Path.GetFileNameWithoutExtension(filePath).Replace("Dependencies", string.Empty);
                (string androidVersion, string iosVersion) = ReadDependencyVersion(
                    filePath,
                    "//androidPackages/androidPackage",
                    "//iosPods/iosPod");
                summaries.Add($"Firebase {moduleName} (Android: {androidVersion}, iOS: {iosVersion})");
            }

            return summaries.ToArray();
        }

        public static string GetAdjustSdkVersion()
        {
            string packagePath = BuildToolV2Utilities.GetAdjustPackageJsonPath();
            if (File.Exists(packagePath))
            {
                string json = File.ReadAllText(packagePath);
                var wrapper = JsonUtility.FromJson<PackageVersionWrapper>(json);
                if (!string.IsNullOrWhiteSpace(wrapper?.version))
                    return NormalizeVersionLikeString(wrapper.version);
            }

            (string androidVersion, string iosVersion) = ReadDependencyVersion(
                BuildToolV2Utilities.GetAdjustDependenciesPath(),
                "//androidPackages/androidPackage[contains(@spec, 'com.adjust.sdk:adjust-android')]",
                "//iosPods/iosPod[@name='Adjust']");
            if (androidVersion != "Unknown")
                return androidVersion;
            if (iosVersion != "Unknown")
                return iosVersion;

            return NormalizeVersionLikeString(GetPackageVersion("com.adjust.sdk"));
        }

        public static string GetGoogleMobileAdsUnityPluginVersion()
        {
            const string rootPath = "Assets/GoogleMobileAds";
            if (!Directory.Exists(rootPath))
                return "Unknown";

            string[] manifestPaths = Directory.GetFiles(rootPath, "GoogleMobileAds_version-*_manifest.txt", SearchOption.TopDirectoryOnly);
            if (manifestPaths.Length == 0)
                return "Unknown";

            string manifestFileName = Path.GetFileName(manifestPaths.OrderByDescending(path => path).First());
            Match match = Regex.Match(manifestFileName, @"GoogleMobileAds_version-([0-9.]+)_manifest\.txt", RegexOptions.IgnoreCase);
            return match.Success ? NormalizeVersionLikeString(match.Groups[1].Value) : "Unknown";
        }

        public static string GetAppLovinMaxUnityPluginVersion()
        {
            const string maxSdkScriptPath = "Assets/MaxSdk/Scripts/MaxSdk.cs";
            if (!File.Exists(maxSdkScriptPath))
                return "Unknown";

            string scriptContent = File.ReadAllText(maxSdkScriptPath);
            Match match = Regex.Match(scriptContent, "_version\\s*=\\s*\"([^\"]+)\"", RegexOptions.IgnoreCase);
            return match.Success ? NormalizeVersionLikeString(match.Groups[1].Value) : "Unknown";
        }

        public static string GetFirebaseUnityPackageVersion(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return "Unknown";

            string packageJsonPath = Path.Combine("Packages", packageName, "package.json");
            if (File.Exists(packageJsonPath))
            {
                string json = File.ReadAllText(packageJsonPath);
                var wrapper = JsonUtility.FromJson<PackageVersionWrapper>(json);
                if (!string.IsNullOrWhiteSpace(wrapper?.version))
                    return NormalizeVersionLikeString(wrapper.version);
            }

            return NormalizeVersionLikeString(GetPackageVersion(packageName));
        }

        public static SdkVersionInspectionEntryV2[] GetSdkVersionInspectionEntries(BuildProfileV2 profile)
        {
            var entries = new List<SdkVersionInspectionEntryV2>(24);

            AddInspectionEntry(entries, SdkPresetMediationTagV2.AdMob, SdkPresetCategoryTagV2.CoreSDK, "Google Mobile Ads Unity Plugin", SdkPresetPlatformV2.Unity, GetGoogleMobileAdsUnityPluginVersion());

            (string admobAndroidVersion, string admobIosVersion) = GetGoogleMobileAdsVersionInfo();
            AddInspectionEntry(entries, SdkPresetMediationTagV2.AdMob, SdkPresetCategoryTagV2.CoreSDK, "Google Mobile Ads SDK", SdkPresetPlatformV2.Android, admobAndroidVersion);
            AddInspectionEntry(entries, SdkPresetMediationTagV2.AdMob, SdkPresetCategoryTagV2.CoreSDK, "Google Mobile Ads SDK", SdkPresetPlatformV2.IOS, admobIosVersion);
            AddMediationAdapterInspectionEntries(entries, ResolveMediationFolderPath("Assets/GoogleMobileAds/Mediation", "Mediation", "GoogleMobileAds", "Mediation"), SdkPresetMediationTagV2.AdMob);

            AddInspectionEntry(entries, SdkPresetMediationTagV2.MAX, SdkPresetCategoryTagV2.CoreSDK, "AppLovin MAX Unity Plugin", SdkPresetPlatformV2.Unity, GetAppLovinMaxUnityPluginVersion());

            (string maxAndroidVersion, string maxIosVersion) = GetAppLovinVersionInfo();
            AddInspectionEntry(entries, SdkPresetMediationTagV2.MAX, SdkPresetCategoryTagV2.CoreSDK, "AppLovin SDK", SdkPresetPlatformV2.Android, maxAndroidVersion);
            AddInspectionEntry(entries, SdkPresetMediationTagV2.MAX, SdkPresetCategoryTagV2.CoreSDK, "AppLovin SDK", SdkPresetPlatformV2.IOS, maxIosVersion);
            AddMediationAdapterInspectionEntries(entries, ResolveMediationFolderPath("Assets/MaxSdk/Mediation", "Mediation", "MaxSdk", "Mediation"), SdkPresetMediationTagV2.MAX);

            AddInspectionEntry(entries, SdkPresetMediationTagV2.Firebase, SdkPresetCategoryTagV2.FirebaseModule, "Firebase App", SdkPresetPlatformV2.Unity, GetFirebaseUnityPackageVersion("com.google.firebase.app"));
            AddInspectionEntry(entries, SdkPresetMediationTagV2.Firebase, SdkPresetCategoryTagV2.FirebaseModule, "Firebase Analytics", SdkPresetPlatformV2.Unity, GetFirebaseUnityPackageVersion("com.google.firebase.analytics"));
            AddInspectionEntry(entries, SdkPresetMediationTagV2.Firebase, SdkPresetCategoryTagV2.FirebaseModule, "Firebase Remote Config", SdkPresetPlatformV2.Unity, GetFirebaseUnityPackageVersion("com.google.firebase.remote-config"));

            AddInspectionEntry(entries, SdkPresetMediationTagV2.Adjust, SdkPresetCategoryTagV2.CoreSDK, "Adjust SDK Version", SdkPresetPlatformV2.Unity, GetAdjustSdkVersion());
            AdjustSceneSnapshot adjustSnapshot = BuildToolV2Utilities.GetAdjustSceneSnapshot(profile);
            AddInspectionEntry(
                entries,
                SdkPresetMediationTagV2.Adjust,
                SdkPresetCategoryTagV2.Environment,
                "Adjust Environment",
                SdkPresetPlatformV2.Unity,
                adjustSnapshot.Exists ? adjustSnapshot.Environment : "Missing");

            return entries.ToArray();
        }

        public static SdkVersionPresetComparisonEntryV2[] CompareWithSdkPreset(BuildProfileV2 profile, SdkVersionPresetV2 preset)
        {
            SdkVersionInspectionEntryV2[] actualEntries = GetSdkVersionInspectionEntries(profile);
            var actualByKey = new Dictionary<SdkVersionPresetKeyV2, SdkVersionInspectionEntryV2>(actualEntries.Length);
            for (int i = 0; i < actualEntries.Length; i++)
            {
                SdkVersionInspectionEntryV2 actualEntry = actualEntries[i];
                if (actualEntry == null)
                    continue;

                actualByKey[actualEntry.Key] = actualEntry;
            }

            int presetCount = preset != null && preset.Entries != null ? preset.Entries.Count : 0;
            var results = new List<SdkVersionPresetComparisonEntryV2>(presetCount + actualByKey.Count);
            var seenKeys = new HashSet<SdkVersionPresetKeyV2>();

            // Pass 1: preset entries (in preset order). Each is either matched, mismatched, or missing.
            for (int i = 0; i < presetCount; i++)
            {
                SdkVersionPresetEntryV2 presetEntry = preset.Entries[i];
                if (presetEntry == null)
                    continue;

                var key = new SdkVersionPresetKeyV2(
                    presetEntry.mediationTag,
                    presetEntry.categoryTag,
                    presetEntry.item,
                    presetEntry.platform);

                seenKeys.Add(key);
                bool existsInProject = actualByKey.TryGetValue(key, out SdkVersionInspectionEntryV2 actualEntry);
                string actualVersion = existsInProject ? actualEntry.ActualVersion : string.Empty;
                bool isMatch = existsInProject && ArePresetValuesEquivalent(presetEntry.categoryTag, presetEntry.expectedVersion, actualVersion);

                results.Add(new SdkVersionPresetComparisonEntryV2
                {
                    Key = key,
                    ExpectedVersion = presetEntry.expectedVersion ?? string.Empty,
                    ActualVersion = actualVersion ?? string.Empty,
                    ExistsInPreset = true,
                    ExistsInProject = existsInProject,
                    IsMatch = isMatch,
                });
            }

            // Pass 2: project items not declared in preset (EXTRA). Appended after preset entries.
            for (int i = 0; i < actualEntries.Length; i++)
            {
                SdkVersionInspectionEntryV2 actualEntry = actualEntries[i];
                if (actualEntry == null || seenKeys.Contains(actualEntry.Key))
                    continue;

                results.Add(new SdkVersionPresetComparisonEntryV2
                {
                    Key = actualEntry.Key,
                    ExpectedVersion = string.Empty,
                    ActualVersion = actualEntry.ActualVersion ?? string.Empty,
                    ExistsInPreset = false,
                    ExistsInProject = true,
                    IsMatch = false,
                });
            }

            return results.ToArray();
        }

        public static CanonicalInspectionItemV2[] GetCanonicalInspectionItemsByMediation(BuildProfileV2 profile, SdkPresetMediationTagV2 mediationTag)
        {
            SdkVersionInspectionEntryV2[] inspectionEntries = GetSdkVersionInspectionEntries(profile);
            var byItem = new Dictionary<string, CanonicalInspectionItemV2>(StringComparer.OrdinalIgnoreCase);
            var itemOrder = new List<string>(inspectionEntries.Length);

            for (int i = 0; i < inspectionEntries.Length; i++)
            {
                SdkVersionInspectionEntryV2 entry = inspectionEntries[i];
                if (entry == null || entry.Key.MediationTag != mediationTag)
                    continue;

                string item = entry.Key.Item ?? string.Empty;
                if (!byItem.TryGetValue(item, out CanonicalInspectionItemV2 canonical))
                {
                    canonical = new CanonicalInspectionItemV2(item, entry.Key.CategoryTag);
                    byItem[item] = canonical;
                    itemOrder.Add(item);
                }

                canonical.AddPlatform(entry.Key.Platform, entry.ActualVersion);
            }

            var results = new CanonicalInspectionItemV2[itemOrder.Count];
            for (int i = 0; i < itemOrder.Count; i++)
                results[i] = byItem[itemOrder[i]];

            return results;
        }

        public static string GetMaxSdkKey()
        {
            AppLovinSettings settings = Resources.Load<AppLovinSettings>("AppLovinSettings");
            return settings == null ? string.Empty : settings.SdkKey ?? string.Empty;
        }

        public static string GetMaxPrivacyPolicyUrl()
        {
            try
            {
                return AppLovinInternalSettings.Instance?.ConsentFlowPrivacyPolicyUrl ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsMaxTermsAndPrivacyPolicyFlowEnabled()
        {
            try
            {
                return AppLovinInternalSettings.Instance != null && AppLovinInternalSettings.Instance.ConsentFlowEnabled;
            }
            catch
            {
                return false;
            }
        }

        public static string GetPackageVersion(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return "Unknown";

            const string lockPath = "Packages/packages-lock.json";
            if (File.Exists(lockPath))
            {
                string versionFromLock = ExtractPackageVersion(lockPath, packageName);
                if (versionFromLock != "Unknown")
                    return versionFromLock;
            }

            const string manifestPath = "Packages/manifest.json";
            if (File.Exists(manifestPath))
            {
                string versionFromManifest = ExtractPackageVersion(manifestPath, packageName);
                if (versionFromManifest != "Unknown")
                    return versionFromManifest;
            }

            return "Unknown";
        }

        public static string GetFirebaseConfigFilePath(BuildTarget target)
        {
            if (CachedFirebaseConfigPaths.TryGetValue(target, out string cachedPath))
                return cachedPath;

            string fileName = BuildToolV2Utilities.GetFirebaseConfigFileName(target);
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName));
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.Equals(Path.GetFileName(path), fileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    CachedFirebaseConfigPaths[target] = path;
                    return path;
                }
            }

            CachedFirebaseConfigPaths[target] = string.Empty;
            return string.Empty;
        }

        public static bool HasFirebaseConfigFile(BuildTarget target)
        {
            return !string.IsNullOrWhiteSpace(GetFirebaseConfigFilePath(target));
        }

        public static string GetFirebaseConfigSummary(BuildTarget target)
        {
            string fileName = BuildToolV2Utilities.GetFirebaseConfigFileName(target);
            if (string.IsNullOrWhiteSpace(fileName))
                return "(not required)";

            string path = GetFirebaseConfigFilePath(target);
            return string.IsNullOrWhiteSpace(path) ? $"{fileName} missing" : path;
        }

        public static void PingFirebaseConfigFile(BuildTarget target)
        {
            string path = GetFirebaseConfigFilePath(target);
            if (string.IsNullOrWhiteSpace(path))
                return;

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        public static void SelectGoogleMobileAdsSettings()
        {
            string assetPath = BuildToolV2Utilities.GetGoogleMobileAdsSettingsAssetPath();
            if (string.IsNullOrWhiteSpace(assetPath))
                return;

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
                return;

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        public static void OpenGoogleMobileAdsSettingsInspector()
        {
            string assetPath = BuildToolV2Utilities.GetGoogleMobileAdsSettingsAssetPath();
            if (string.IsNullOrWhiteSpace(assetPath))
                return;

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            BuildToolV2Utilities.OpenLockedInspector(asset);
        }

        public static void OpenAppLovinIntegrationManager()
        {
            System.Type windowType = FindTypeInEditorAssemblies("AppLovinIntegrationManagerWindow");
            MethodInfo showManagerMethod = windowType?.GetMethod("ShowManager", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (showManagerMethod != null)
            {
                showManagerMethod.Invoke(null, null);
                return;
            }

            EditorApplication.ExecuteMenuItem("AppLovin/Integration Manager");
        }

        private static string[] GetMediationAdapterSummaries(string mediationRootPath)
        {
            if (CachedMediationAdapterSummaries.TryGetValue(mediationRootPath, out string[] cachedSummaries))
                return cachedSummaries;

            if (!Directory.Exists(mediationRootPath))
            {
                CachedMediationAdapterSummaries[mediationRootPath] = System.Array.Empty<string>();
                return CachedMediationAdapterSummaries[mediationRootPath];
            }

            string[] directories = Directory.GetDirectories(mediationRootPath)
                .OrderBy(path => path)
                .ToArray();
            var summaries = new List<string>(directories.Length);

            for (int i = 0; i < directories.Length; i++)
            {
                string adapterDirectory = directories[i];
                string adapterName = new DirectoryInfo(adapterDirectory).Name;
                string editorPath = Path.Combine(adapterDirectory, "Editor");
                if (!Directory.Exists(editorPath))
                    continue;

                string[] dependencyFiles = Directory.GetFiles(editorPath, "*.xml", SearchOption.TopDirectoryOnly);
                (string androidVersion, string iosVersion) = ReadDependencyVersionsFromFiles(dependencyFiles);
                summaries.Add($"{adapterName} (Android: {androidVersion}, iOS: {iosVersion})");
            }

            string[] result = summaries.ToArray();
            CachedMediationAdapterSummaries[mediationRootPath] = result;
            return result;
        }

        private static string[] GetAppLovinAdapterPackageSummaries()
        {
            const string cacheKey = "Packages/com.applovin.mediation.adapters";
            if (CachedMediationAdapterSummaries.TryGetValue(cacheKey, out string[] cachedSummaries))
                return cachedSummaries;

            const string lockPath = "Packages/packages-lock.json";
            if (!File.Exists(lockPath))
            {
                CachedMediationAdapterSummaries[cacheKey] = System.Array.Empty<string>();
                return CachedMediationAdapterSummaries[cacheKey];
            }

            string json = File.ReadAllText(lockPath);
            MatchCollection matches = Regex.Matches(
                json,
                "\"com\\.applovin\\.mediation\\.adapters\\.([^\"]+?)\\.(android|ios)\"\\s*:\\s*\\{.*?\"version\"\\s*:\\s*\"([^\"]+)\"",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            var adapterVersions = new Dictionary<string, (string android, string ios)>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                string adapterName = match.Groups[1].Value.Replace('.', '-');
                string platform = match.Groups[2].Value.ToLowerInvariant();
                string version = match.Groups[3].Value;

                adapterVersions.TryGetValue(adapterName, out (string android, string ios) current);
                if (platform == "android")
                    current.android = version;
                else if (platform == "ios")
                    current.ios = version;

                adapterVersions[adapterName] = current;
            }

            string[] results = adapterVersions
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key} (Android: {DisplayPackageVersion(pair.Value.android)}, iOS: {DisplayPackageVersion(pair.Value.ios)})")
                .ToArray();

            CachedMediationAdapterSummaries[cacheKey] = results;
            return results;
        }

        private static void AddInspectionEntry(
            List<SdkVersionInspectionEntryV2> entries,
            SdkPresetMediationTagV2 mediationTag,
            SdkPresetCategoryTagV2 categoryTag,
            string item,
            SdkPresetPlatformV2 platform,
            string actualVersion)
        {
            entries.Add(new SdkVersionInspectionEntryV2
            {
                Key = new SdkVersionPresetKeyV2(mediationTag, categoryTag, item, platform),
                ActualVersion = string.IsNullOrWhiteSpace(actualVersion) ? "Unknown" : actualVersion.Trim(),
            });
        }

        private static void AddMediationAdapterInspectionEntries(List<SdkVersionInspectionEntryV2> entries, string mediationRootPath, SdkPresetMediationTagV2 mediationTag)
        {
            if (string.IsNullOrWhiteSpace(mediationRootPath) || !Directory.Exists(mediationRootPath))
                return;

            string[] directories = Directory.GetDirectories(mediationRootPath)
                .OrderBy(path => path)
                .ToArray();

            for (int i = 0; i < directories.Length; i++)
            {
                string adapterDirectory = directories[i];
                string adapterName = BuildMediationAdapterPresetItemName(new DirectoryInfo(adapterDirectory).Name);
                if (string.IsNullOrWhiteSpace(adapterName))
                    continue;

                string editorPath = Path.Combine(adapterDirectory, "Editor");
                string[] dependencyFiles = Directory.Exists(editorPath)
                    ? Directory.GetFiles(editorPath, "*.xml", SearchOption.TopDirectoryOnly)
                    : Array.Empty<string>();
                (string androidVersion, string iosVersion) = ReadDependencyVersionsFromFiles(dependencyFiles);

                AddInspectionEntry(entries, mediationTag, SdkPresetCategoryTagV2.MediationAdapter, adapterName, SdkPresetPlatformV2.Android, androidVersion);
                AddInspectionEntry(entries, mediationTag, SdkPresetCategoryTagV2.MediationAdapter, adapterName, SdkPresetPlatformV2.IOS, iosVersion);
            }
        }

        private static string BuildMediationAdapterPresetItemName(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                return string.Empty;

            switch (folderName.Trim())
            {
                case "MetaAudienceNetwork":
                    return "Meta Audience Network Adapter";
                case "LiftoffMonetize":
                    return "Liftoff Monetize (Vungle) Adapter";
                default:
                    return $"{SplitPascalCase(folderName)} Adapter";
            }
        }

        private static bool ArePresetValuesEquivalent(SdkPresetCategoryTagV2 categoryTag, string expectedValue, string actualValue)
        {
            if (categoryTag == SdkPresetCategoryTagV2.Environment)
                return string.Equals((expectedValue ?? string.Empty).Trim(), (actualValue ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

            return string.Equals(
                NormalizeVersionLikeString(expectedValue),
                NormalizeVersionLikeString(actualValue),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveMediationFolderPath(string preferredProjectPath, string folderName, params string[] requiredPathSegments)
        {
            string normalizedPreferredPath = NormalizePath(preferredProjectPath);
            if (!string.IsNullOrWhiteSpace(normalizedPreferredPath) && Directory.Exists(normalizedPreferredPath))
                return normalizedPreferredPath;

            string assetLookupPath = BuildToolV2Utilities.GetAssetFolderPath(folderName, requiredPathSegments);
            if (!string.IsNullOrWhiteSpace(assetLookupPath) && Directory.Exists(assetLookupPath))
                return NormalizePath(assetLookupPath);

            if (!Directory.Exists("Assets"))
                return string.Empty;

            string[] candidates = Directory.GetDirectories("Assets", folderName, SearchOption.AllDirectories);
            for (int i = 0; i < candidates.Length; i++)
            {
                string candidate = NormalizePath(candidates[i]);
                if (PathContainsAllSegments(candidate, requiredPathSegments))
                    return candidate;
            }

            return string.Empty;
        }

        private static string DisplayPackageVersion(string version)
        {
            return string.IsNullOrWhiteSpace(version) ? "Unknown" : version;
        }

        private static (string androidVersion, string iosVersion) ReadDependencyVersionsFromFiles(string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
                return ("Unknown", "Unknown");

            string androidVersion = "Unknown";
            string iosVersion = "Unknown";
            for (int i = 0; i < filePaths.Length; i++)
            {
                (string currentAndroid, string currentIos) = ReadDependencyVersion(
                    filePaths[i],
                    "//androidPackages/androidPackage",
                    "//iosPods/iosPod");

                if (androidVersion == "Unknown" && currentAndroid != "Unknown")
                    androidVersion = currentAndroid;
                if (iosVersion == "Unknown" && currentIos != "Unknown")
                    iosVersion = currentIos;
            }

            return (androidVersion, iosVersion);
        }

        private static (string androidVersion, string iosVersion) ReadDependencyVersion(string filePath, string androidXPath, string iosXPath)
        {
            if (!File.Exists(filePath))
                return ("Unknown", "Unknown");

            var document = new XmlDocument();
            document.Load(filePath);

            string androidVersion = ExtractAndroidVersion(document.SelectSingleNode(androidXPath));
            string iosVersion = ExtractIosVersion(document.SelectSingleNode(iosXPath));
            return (androidVersion, iosVersion);
        }

        private static string ExtractAndroidVersion(XmlNode androidNode)
        {
            string spec = androidNode?.Attributes?["spec"]?.Value;
            if (string.IsNullOrWhiteSpace(spec))
                return "Unknown";

            string[] parts = spec.Split(':');
            string version = parts.Length >= 3 ? parts[2] : spec;
            return CleanVersion(version);
        }

        private static string ExtractIosVersion(XmlNode iosNode)
        {
            string version = iosNode?.Attributes?["version"]?.Value;
            return CleanVersion(version);
        }

        private static string CleanVersion(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
                return "Unknown";

            string cleaned = Regex.Replace(version, "[^0-9.]", string.Empty);
            return string.IsNullOrWhiteSpace(cleaned) ? "Unknown" : cleaned;
        }

        private static string NormalizeVersionLikeString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Unknown";

            string trimmed = value.Trim();

            Match tagMatch = Regex.Match(trimmed, @"#v?(\d+(?:\.\d+)+)", RegexOptions.IgnoreCase);
            if (tagMatch.Success)
                return tagMatch.Groups[1].Value;

            Match directMatch = Regex.Match(trimmed, @"\d+(?:\.\d+)+");
            if (directMatch.Success)
                return directMatch.Value;

            string cleaned = Regex.Replace(trimmed, "[^0-9.]", string.Empty).Trim('.');
            return string.IsNullOrWhiteSpace(cleaned) ? "Unknown" : cleaned;
        }

        private static string SplitPascalCase(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            return Regex.Replace(raw.Trim(), "([a-z0-9])([A-Z])", "$1 $2");
        }

        private static string NormalizePath(string path)
        {
            return path?.Replace("\\", "/") ?? string.Empty;
        }

        private static bool PathContainsAllSegments(string path, string[] requiredPathSegments)
        {
            if (requiredPathSegments == null || requiredPathSegments.Length == 0)
                return true;

            string normalizedPath = NormalizePath(path);
            for (int i = 0; i < requiredPathSegments.Length; i++)
            {
                string segment = requiredPathSegments[i];
                if (string.IsNullOrWhiteSpace(segment))
                    continue;

                if (normalizedPath.IndexOf($"/{segment}/", StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                if (normalizedPath.EndsWith($"/{segment}", StringComparison.OrdinalIgnoreCase))
                    continue;

                return false;
            }

            return true;
        }

        private static string ExtractPackageVersion(string jsonPath, string packageName)
        {
            string json = File.ReadAllText(jsonPath);
            string escapedPackageName = Regex.Escape(packageName);
            Match packageBlockMatch = Regex.Match(
                json,
                $"\"{escapedPackageName}\"\\s*:\\s*\\{{.*?\"version\"\\s*:\\s*\"([^\"]+)\"",
                RegexOptions.Singleline);

            return packageBlockMatch.Success ? packageBlockMatch.Groups[1].Value : "Unknown";
        }

        private static System.Type FindTypeInEditorAssemblies(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                if (assembly == null || assembly.IsDynamic)
                    continue;

                System.Type type = assembly.GetType(typeName, false)
                    ?? assembly.GetTypes().FirstOrDefault(candidate => candidate != null && candidate.Name == typeName);
                if (type != null)
                    return type;
            }

            return null;
        }

        [System.Serializable]
        private sealed class PackageVersionWrapper
        {
            public string version = string.Empty;
        }
    }
}
