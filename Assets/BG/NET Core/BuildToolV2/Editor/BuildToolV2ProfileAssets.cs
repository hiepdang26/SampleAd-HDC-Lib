using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2ProfileAssets
    {
        private static string cachedBuildToolRootFolder;
        private static string cachedProfilesFolder;

        public static void ClearCaches()
        {
            cachedBuildToolRootFolder = null;
            cachedProfilesFolder = null;
        }

        public static BuildProfileV2[] EnsureDefaultProfiles()
        {
            string buildToolRootFolder = GetBuildToolRootFolder();
            string profilesFolder = GetProfilesFolder();

            if (!EnsureFolderExists(buildToolRootFolder) || !EnsureFolderExists(profilesFolder))
            {
                Debug.LogError($"BuildToolV2: could not ensure profile folders. Root='{buildToolRootFolder}', Profiles='{profilesFolder}'.");
                return System.Array.Empty<BuildProfileV2>();
            }

            var profiles = new[]
            {
                EnsureProfileAsset("Android Test Debug", BuildProfilePresetV2.TestDebug, BuildTarget.Android),
                EnsureProfileAsset("Android Test Release", BuildProfilePresetV2.TestRelease, BuildTarget.Android),
                EnsureProfileAsset("Android Release", BuildProfilePresetV2.Release, BuildTarget.Android),
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return profiles.Where(profile => profile != null).ToArray();
        }

        public static BuildProfileV2[] LoadProfiles()
        {
            string[] guids = AssetDatabase.FindAssets("t:BuildProfileV2");
            var profiles = new BuildProfileV2[guids.Length];
            for (int i = 0; i < guids.Length; i++)
                profiles[i] = AssetDatabase.LoadAssetAtPath<BuildProfileV2>(AssetDatabase.GUIDToAssetPath(guids[i]));

            return profiles;
        }

        private static BuildProfileV2 EnsureProfileAsset(string displayName, BuildProfilePresetV2 preset, BuildTarget target)
        {
            string assetName = displayName.Replace(" ", "_") + ".asset";
            string profilesFolder = GetProfilesFolder();
            if (string.IsNullOrWhiteSpace(profilesFolder) || !EnsureFolderExists(profilesFolder))
            {
                Debug.LogError($"BuildToolV2: Cannot create profile asset '{displayName}' because the Profiles folder could not be resolved.");
                return null;
            }

            string assetPath = Path.Combine(profilesFolder, assetName).Replace("\\", "/");
            BuildProfileV2 profile = AssetDatabase.LoadAssetAtPath<BuildProfileV2>(assetPath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<BuildProfileV2>();
                AssetDatabase.CreateAsset(profile, assetPath);
            }

            profile.ConfigurePreset(preset, target, displayName);
            return profile;
        }

        private static string GetBuildToolRootFolder()
        {
            if (!string.IsNullOrWhiteSpace(cachedBuildToolRootFolder) && AssetDatabase.IsValidFolder(cachedBuildToolRootFolder))
                return cachedBuildToolRootFolder;

            BuildProfileV2 tempProfile = null;
            try
            {
                tempProfile = ScriptableObject.CreateInstance<BuildProfileV2>();
                MonoScript profileScript = MonoScript.FromScriptableObject(tempProfile);
                string scriptPath = AssetDatabase.GetAssetPath(profileScript);
                if (!string.IsNullOrWhiteSpace(scriptPath))
                {
                    string editorFolder = Path.GetDirectoryName(scriptPath)?.Replace("\\", "/");
                    string rootFolder = Path.GetDirectoryName(editorFolder ?? string.Empty)?.Replace("\\", "/");
                    if (string.IsNullOrWhiteSpace(rootFolder))
                        return string.Empty;

                    cachedBuildToolRootFolder = rootFolder;
                    return cachedBuildToolRootFolder;
                }
            }
            finally
            {
                if (tempProfile != null)
                    Object.DestroyImmediate(tempProfile);
            }

            string[] allPaths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < allPaths.Length; i++)
            {
                string path = allPaths[i].Replace("\\", "/");
                if (!path.EndsWith("/BuildToolV2", System.StringComparison.Ordinal) || !AssetDatabase.IsValidFolder(path))
                    continue;

                string editorFolder = $"{path}/Editor";
                if (!AssetDatabase.IsValidFolder(editorFolder))
                    continue;

                cachedBuildToolRootFolder = path;
                return cachedBuildToolRootFolder;
            }

            Debug.LogError("BuildToolV2: Could not resolve the BuildToolV2 root folder from the current project layout.");
            cachedBuildToolRootFolder = string.Empty;
            return cachedBuildToolRootFolder;
        }

        private static string GetProfilesFolder()
        {
            if (!string.IsNullOrWhiteSpace(cachedProfilesFolder) && AssetDatabase.IsValidFolder(cachedProfilesFolder))
                return cachedProfilesFolder;

            string rootFolder = GetBuildToolRootFolder();
            if (string.IsNullOrWhiteSpace(rootFolder))
            {
                cachedProfilesFolder = string.Empty;
                return cachedProfilesFolder;
            }

            cachedProfilesFolder = Path.Combine(rootFolder, "Profiles").Replace("\\", "/");
            return cachedProfilesFolder;
        }

        private static bool EnsureFolderExists(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return false;

            string normalizedPath = folderPath.Replace("\\", "/").TrimEnd('/');
            if (AssetDatabase.IsValidFolder(normalizedPath))
                return true;

            string parentPath = Path.GetDirectoryName(normalizedPath)?.Replace("\\", "/");
            string folderName = Path.GetFileName(normalizedPath);
            if (string.IsNullOrWhiteSpace(parentPath) || string.IsNullOrWhiteSpace(folderName))
                return false;

            if (!EnsureFolderExists(parentPath))
                return false;

            AssetDatabase.CreateFolder(parentPath, folderName);
            return AssetDatabase.IsValidFolder(normalizedPath);
        }
    }
}
