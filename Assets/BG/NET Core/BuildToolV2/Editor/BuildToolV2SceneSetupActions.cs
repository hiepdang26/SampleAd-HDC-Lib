using System.Collections.Generic;
using AdjustSdk;
using BG_Library.NET.AdSystem;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SceneSetupActions
    {
        public static bool EnsureNetPrefabInCurrentScene(out string message)
        {
            var dirtyScenePaths = new HashSet<string>();
            bool succeeded = BuildToolV2SceneSetupMutation.EnsureSetupChildPrefab("NET", nameof(AdsLogic), null, dirtyScenePaths, out message);
            if (succeeded)
                BuildToolV2SceneSaveAndSummary.SaveDirtyScenes(dirtyScenePaths);
            return succeeded;
        }

        public static bool EnsureAdjustInCurrentScene(out string message)
        {
            var dirtyScenePaths = new HashSet<string>();
            bool succeeded = BuildToolV2SceneSetupMutation.EnsureSetupChildPrefab("Adjust", nameof(Adjust), null, dirtyScenePaths, out message);
            if (succeeded)
                BuildToolV2SceneSaveAndSummary.SaveDirtyScenes(dirtyScenePaths);
            return succeeded;
        }

        public static bool EnsureDebugOverlayInCurrentScene(out string message)
        {
            var dirtyScenePaths = new HashSet<string>();
            bool succeeded = BuildToolV2SceneSetupMutation.EnsureSetupChildPrefab("Debug Overlay Canvas", null, null, dirtyScenePaths, out message);
            if (succeeded)
                BuildToolV2SceneSaveAndSummary.SaveDirtyScenes(dirtyScenePaths);
            return succeeded;
        }

        public static bool RemoveDebugOverlayInCurrentScene(out string message)
        {
            var dirtyScenePaths = new HashSet<string>();
            bool succeeded = BuildToolV2SceneSetupMutation.RemoveSetupChildPrefab("Debug Overlay Canvas", null, null, dirtyScenePaths, out message);
            if (succeeded)
                BuildToolV2SceneSaveAndSummary.SaveDirtyScenes(dirtyScenePaths);
            return succeeded;
        }
    }
}
