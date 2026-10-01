using System;
using System.IO;
using BG_Library.NET.AdSystem;
using UnityEditor;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2DisplayState
    {
        internal static void ReloadDisplayState(BuildToolWindowV2 owner, bool reloadSdk)
        {
            ReloadHeaderOverviewState(owner);
            owner.ReloadAdCoreInfoState();
            ReloadBuildScenesState(owner);
            if (reloadSdk || owner.runtimeSdkState == null)
                owner.ReloadRuntimeSdkState();
        }

        internal static void ReloadHeaderOverviewState(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
            {
                owner.headerOverviewState = null;
                return;
            }

            NetConfigsSO netConfigs = owner.profile.ResolveNetConfigs();
            string outputDirectory = string.IsNullOrWhiteSpace(owner.profile.OutputDirectory) ? "(choose during build)" : owner.profile.OutputDirectory;
            BuildProfileManagedSourceState currentManagedState = owner.profile.GetCurrentManagedSourceState();
            string[] mismatchSummaries = owner.profile.GetManagedSourceMismatchSummaries();

            owner.headerOverviewState = new BuildToolWindowV2.HeaderOverviewState
            {
                Preset = owner.profile.Preset.ToString(),
                Target = BuildToolV2Utilities.GetTargetDisplayName(owner.profile.BuildTarget),
                BuildName = BuildToolV2Utilities.GetBuildDisplayName(owner.profile, netConfigs),
                AdjustEnvironment = owner.profile.UsesSandboxAdjust ? "Sandbox" : "Production",
                OutputDirectory = outputDirectory,
                HasOutputDirectory = !string.IsNullOrWhiteSpace(owner.profile.OutputDirectory) && Directory.Exists(owner.profile.OutputDirectory),
                ConfigsName = netConfigs == null ? "(missing)" : netConfigs.name,
                Hack = currentManagedState.BuildHack ? "ON" : "OFF",
                AdmobTestDevice = currentManagedState.AdmobTestDevice ? "ON" : "OFF",
                AdmobTestIds = currentManagedState.AdmobTestIds ? "ON" : "OFF",
                DebugPreset = currentManagedState.DebugPreset.ToString(),
                TrackingFirebase = currentManagedState.TrackingFirebase ? "ON" : "OFF",
                DebugUiKeepLandscape = netConfigs != null && netConfigs.DebugUI_KeepLandscape ? "ON" : "OFF",
                SrDebugger = currentManagedState.SrDebuggerEnabled ? "ON" : "OFF",
                NetConfigs = netConfigs,
                HasManagedSourceMismatch = mismatchSummaries.Length > 0,
                ManagedSourceMismatchSummaries = mismatchSummaries,
            };
        }

        internal static void ReloadBuildScenesState(BuildToolWindowV2 owner)
        {
            string primaryScenePath = BuildToolV2Utilities.GetPrimaryEnabledScenePath();
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            EditorBuildSettingsScene[] clonedScenes = new EditorBuildSettingsScene[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                clonedScenes[i] = scene == null
                    ? new EditorBuildSettingsScene(string.Empty, false)
                    : new EditorBuildSettingsScene(scene.path, scene.enabled);
            }

            owner.buildScenesState = new BuildToolWindowV2.BuildScenesState
            {
                PrimaryScenePath = primaryScenePath ?? string.Empty,
                PrimarySceneLabel = string.IsNullOrWhiteSpace(primaryScenePath)
                    ? "(none)"
                    : Path.GetFileNameWithoutExtension(primaryScenePath),
                Scenes = clonedScenes,
            };
        }
    }
}
