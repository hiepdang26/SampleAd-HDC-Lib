using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SceneSaveAndSummary
    {
        public static string[] SaveDirtyAssets(HashSet<string> dirtyAssetPaths)
        {
            if (dirtyAssetPaths == null || dirtyAssetPaths.Count == 0)
                return System.Array.Empty<string>();

            AssetDatabase.SaveAssets();
            return dirtyAssetPaths.OrderBy(path => path).ToArray();
        }

        public static string[] SaveDirtyScenes(HashSet<string> dirtyScenePaths)
        {
            if (dirtyScenePaths == null || dirtyScenePaths.Count == 0)
                return System.Array.Empty<string>();

            var savedScenePaths = dirtyScenePaths.OrderBy(path => path).ToArray();
            for (int i = 0; i < savedScenePaths.Length; i++)
            {
                Scene scene = SceneManager.GetSceneByPath(savedScenePaths[i]);
                if (!scene.IsValid())
                    continue;

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            return savedScenePaths;
        }

        public static string BuildPreparationSummary(string[] steps, string[] savedScenePaths, string[] savedAssetPaths)
        {
            var builder = new StringBuilder();
            builder.Append($"Steps: {(steps == null ? 0 : steps.Length)}");
            builder.Append($" | Scenes saved: {(savedScenePaths == null ? 0 : savedScenePaths.Length)}");
            builder.Append($" | Assets saved: {(savedAssetPaths == null ? 0 : savedAssetPaths.Length)}");
            return builder.ToString();
        }

        public static bool SetBool(SerializedObject serializedObject, string propertyName, bool targetValue)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || property.boolValue == targetValue)
                return false;

            property.boolValue = targetValue;
            return true;
        }
    }
}
