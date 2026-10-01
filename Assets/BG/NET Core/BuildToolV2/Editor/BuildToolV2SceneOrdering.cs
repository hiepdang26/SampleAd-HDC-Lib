using System.Collections.Generic;
using UnityEditor;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SceneOrdering
    {
        public static bool MoveEnabledBuildSceneToFirst(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
                return false;

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes ?? System.Array.Empty<EditorBuildSettingsScene>();
            if (scenes.Length <= 1)
                return false;

            int targetIndex = -1;
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (scene == null || !scene.enabled)
                    continue;

                if (!string.Equals(scene.path, scenePath, System.StringComparison.OrdinalIgnoreCase))
                    continue;

                targetIndex = i;
                break;
            }

            if (targetIndex <= 0)
                return targetIndex == 0;

            var reordered = new List<EditorBuildSettingsScene>(scenes.Length)
            {
                scenes[targetIndex]
            };

            for (int i = 0; i < scenes.Length; i++)
            {
                if (i == targetIndex)
                    continue;

                reordered.Add(scenes[i]);
            }

            EditorBuildSettings.scenes = reordered.ToArray();
            return true;
        }
    }
}
