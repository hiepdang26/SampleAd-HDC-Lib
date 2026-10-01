using BG_Library.NET.AdSystem;
using SRDebugger;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SharedStateSnapshot
    {
        public static BuildSharedStateSnapshot Capture(BuildProfileV2 profile)
        {
            bool mfsIgnoreExisted = PlayerPrefs.HasKey(BuildToolV2MfuscatorPreset.MfsIgnoreKey);
            string mfsIgnoreOriginal = mfsIgnoreExisted ? PlayerPrefs.GetString(BuildToolV2MfuscatorPreset.MfsIgnoreKey, string.Empty) : string.Empty;

            var snapshot = new BuildSharedStateSnapshot
            {
                IsValid = true,
                DevelopmentBuild = EditorUserBuildSettings.development,
                BuildAppBundle = EditorUserBuildSettings.buildAppBundle,
                MfsIgnoreKeyExisted = mfsIgnoreExisted,
                MfsIgnoreOriginalValue = mfsIgnoreOriginal,
                SceneManagerSetup = EditorSceneManager.GetSceneManagerSetup() ?? System.Array.Empty<UnityEditor.SceneManagement.SceneSetup>(),
            };

            BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, "ProjectSettings/ProjectSettings.asset");
            BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, "ProjectSettings/EditorBuildSettings.asset");
            BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, "ProjectSettings/AndroidResolverDependencies.xml");
            BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, "ProjectSettings/AppLovinInternalSettings.json");
            BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, "Assets/Plugins/Android/gradleTemplate.properties");
            BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, "Assets/Plugins/Android/mainTemplate.gradle");
            BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, "Assets/Plugins/Android/settingsTemplate.gradle");
            BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, "Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib/AndroidManifest.xml");

            if (profile != null)
            {
                string primaryScenePath = BuildToolV2SceneNavigation.GetPrimaryEnabledScenePath();
                if (!string.IsNullOrWhiteSpace(primaryScenePath))
                    BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, primaryScenePath);

                NetConfigsSO netConfigs = profile.ResolveNetConfigs();
                if (netConfigs != null)
                    BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, AssetDatabase.GetAssetPath(netConfigs));
            }

            Settings srDebuggerSettings = Resources.Load<Settings>("SRDebugger/Settings");
            if (srDebuggerSettings != null)
                BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, AssetDatabase.GetAssetPath(srDebuggerSettings));

            string googleMobileAdsSettingsPath = BuildToolV2AssetLookup.GetGoogleMobileAdsSettingsAssetPath();
            if (!string.IsNullOrWhiteSpace(googleMobileAdsSettingsPath))
                BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, googleMobileAdsSettingsPath);

            AppLovinSettings appLovinSettings = Resources.Load<AppLovinSettings>("AppLovinSettings");
            if (appLovinSettings != null)
                BuildToolV2SnapshotIO.CaptureSnapshotFile(snapshot, AssetDatabase.GetAssetPath(appLovinSettings));

            return snapshot;
        }

        public static void Restore(BuildSharedStateSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.IsValid)
                return;

            try
            {
                AssetDatabase.SaveAssets();
            }
            catch
            {
                // Ignore; file restore below will still reset persisted state.
            }

            try
            {
                EditorSceneManager.SaveOpenScenes();
            }
            catch
            {
                // Ignore; restoring the saved files and scene setup is the priority.
            }

            for (int i = 0; i < snapshot.Files.Count; i++)
                BuildToolV2SnapshotIO.RestoreSnapshotFile(snapshot.Files[i]);

            EditorUserBuildSettings.development = snapshot.DevelopmentBuild;
            EditorUserBuildSettings.buildAppBundle = snapshot.BuildAppBundle;

            if (snapshot.MfsIgnoreKeyExisted)
                PlayerPrefs.SetString(BuildToolV2MfuscatorPreset.MfsIgnoreKey, snapshot.MfsIgnoreOriginalValue ?? string.Empty);
            else
                PlayerPrefs.DeleteKey(BuildToolV2MfuscatorPreset.MfsIgnoreKey);
            PlayerPrefs.Save();

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            try
            {
                if (snapshot.SceneManagerSetup != null && snapshot.SceneManagerSetup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(snapshot.SceneManagerSetup);
            }
            catch
            {
                // If reopening the previous scene setup fails, keep the restored files on disk.
            }

            BuildToolV2Utilities.ClearEditorCaches();
        }
    }
}
