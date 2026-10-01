using System.Collections.Generic;
using BG_Library.NET.AdSystem;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2BuildPreparation
    {
        public static BuildScenePreparationResult Prepare(BuildProfileV2 profile, NetConfigsSO netConfigs)
        {
            var result = new BuildScenePreparationResult
            {
                Succeeded = false,
            };

            var steps = new List<string>();
            var dirtyScenePaths = new HashSet<string>();
            var dirtyAssetPaths = new HashSet<string>();

            BuildToolV2NetConfigPreset.Apply(profile, netConfigs, steps, dirtyAssetPaths);
            BuildToolV2SrDebuggerPreset.Apply(profile, steps, dirtyAssetPaths);
            BuildToolV2AdjustPreset.Apply(profile, steps, dirtyScenePaths);
            BuildToolV2MfuscatorPreset.Apply(profile, steps);

            if (profile.RequiresDebugOverlayCanvas)
            {
                if (!BuildToolV2SceneSetupMutation.EnsureSetupChildPrefab("Debug Overlay Canvas", null, steps, dirtyScenePaths, out string errorMessage))
                {
                    result.ErrorMessage = errorMessage;
                    result.Steps = steps.ToArray();
                    return result;
                }
            }
            else
            {
                BuildToolV2SceneSetupMutation.RemoveDebugOverlayInCurrentSceneIfExists(steps, dirtyScenePaths);
            }

            string[] savedAssetPaths = BuildToolV2SceneSaveAndSummary.SaveDirtyAssets(dirtyAssetPaths);
            string[] savedScenePaths = BuildToolV2SceneSaveAndSummary.SaveDirtyScenes(dirtyScenePaths);

            result.Steps = steps.ToArray();
            result.SavedAssetPaths = savedAssetPaths;
            result.SavedScenePaths = savedScenePaths;
            result.Succeeded = true;
            result.Summary = BuildToolV2SceneSaveAndSummary.BuildPreparationSummary(result.Steps, result.SavedScenePaths, result.SavedAssetPaths);
            return result;
        }
    }
}
