using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2AssetLookup
    {
        private static readonly Dictionary<string, string> CachedResolvedAssetPaths = new Dictionary<string, string>();

        public static void ClearCaches()
        {
            CachedResolvedAssetPaths.Clear();
        }

        public static string GetGoogleMobileAdsSettingsAssetPath()
        {
            return FindAssetPathByFileName("GoogleMobileAdsSettings.asset", "GoogleMobileAds", "Resources");
        }

        public static string GetGoogleMobileAdsDependenciesPath()
        {
            return FindAssetPathByFileName("GoogleMobileAdsDependencies.xml", "GoogleMobileAds", "Editor");
        }

        public static string GetMaxDependenciesPath()
        {
            return FindAssetPathByFileName("Dependencies.xml", "MaxSdk", "AppLovin", "Editor");
        }

        public static string GetAdjustPackageJsonPath()
        {
            return FindAssetPathByFileName("package.json", "Adjust");
        }

        public static string GetAdjustDependenciesPath()
        {
            return FindAssetPathByFileName("Dependencies.xml", "Adjust", "Native", "Editor");
        }

        public static string GetAssetFolderPath(string folderName, params string[] requiredPathSegments)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                return string.Empty;

            string cacheKey = $"FOLDER::{folderName}::{string.Join("|", requiredPathSegments ?? System.Array.Empty<string>())}";
            if (CachedResolvedAssetPaths.TryGetValue(cacheKey, out string cachedPath) && !string.IsNullOrWhiteSpace(cachedPath))
                return cachedPath;

            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < allPaths.Length; i++)
            {
                string path = allPaths[i].Replace("\\", "/");
                if (!AssetDatabase.IsValidFolder(path))
                    continue;

                if (!string.Equals(Path.GetFileName(path), folderName, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!PathContainsAllSegments(path, requiredPathSegments))
                    continue;

                CachedResolvedAssetPaths[cacheKey] = path;
                return path;
            }

            CachedResolvedAssetPaths[cacheKey] = string.Empty;
            return string.Empty;
        }

        private static string FindAssetPathByFileName(string fileName, params string[] requiredPathSegments)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            string cacheKey = $"FILE::{fileName}::{string.Join("|", requiredPathSegments ?? System.Array.Empty<string>())}";
            if (CachedResolvedAssetPaths.TryGetValue(cacheKey, out string cachedPath) && !string.IsNullOrWhiteSpace(cachedPath))
                return cachedPath;

            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(fileName));
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]).Replace("\\", "/");
                if (!string.Equals(Path.GetFileName(path), fileName, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!PathContainsAllSegments(path, requiredPathSegments))
                    continue;

                CachedResolvedAssetPaths[cacheKey] = path;
                return path;
            }

            CachedResolvedAssetPaths[cacheKey] = string.Empty;
            return string.Empty;
        }

        private static bool PathContainsAllSegments(string path, string[] requiredPathSegments)
        {
            if (requiredPathSegments == null || requiredPathSegments.Length == 0)
                return true;

            string normalizedPath = path?.Replace("\\", "/") ?? string.Empty;
            for (int i = 0; i < requiredPathSegments.Length; i++)
            {
                string segment = requiredPathSegments[i];
                if (string.IsNullOrWhiteSpace(segment))
                    continue;

                if (normalizedPath.IndexOf($"/{segment}/", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                if (normalizedPath.EndsWith($"/{segment}", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                return false;
            }

            return true;
        }
    }
}
