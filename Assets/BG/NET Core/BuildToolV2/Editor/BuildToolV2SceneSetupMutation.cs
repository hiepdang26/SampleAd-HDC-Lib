using System.Collections.Generic;
using BG_Library.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SceneSetupMutation
    {
        public static bool EnsureSetupChildPrefab(
            string prefabName,
            string componentName,
            List<string> steps,
            HashSet<string> dirtyScenePaths,
            out string errorMessage)
        {
            errorMessage = null;

            BG_SETUP setup = Object.FindFirstObjectByType<BG_SETUP>();
            if (setup == null)
            {
                errorMessage = "Khong tim thay BG_SETUP trong primary build scene de prepare build.";
                return false;
            }

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var existingObjects = new HashSet<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject candidate = allObjects[i];
                if (candidate == null || candidate.scene != setup.gameObject.scene)
                    continue;

                GameObject existingRoot = BuildToolV2SceneSetupMatching.ResolveSetupChildRoot(candidate, prefabName, componentName);
                if (existingRoot != null)
                    existingObjects.Add(existingRoot);
            }

            bool changed = false;
            string stepMessage;
            var existingRoots = new List<GameObject>(existingObjects);
            for (int i = existingRoots.Count - 1; i >= 1; i--)
            {
                Object.DestroyImmediate(existingRoots[i]);
                existingRoots.RemoveAt(i);
                changed = true;
            }

            GameObject targetObject;
            if (existingRoots.Count == 0)
            {
                GameObject prefab = BGSetupEditorUtilities.FindPrefabWithComponent(prefabName, componentName);
                if (prefab == null)
                {
                    errorMessage = string.IsNullOrWhiteSpace(componentName)
                        ? $"Khong tim thay prefab '{prefabName}'."
                        : $"Khong tim thay prefab '{prefabName}' co component {componentName}.";
                    return false;
                }

                targetObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                changed = true;
                stepMessage = $"Spawned {prefabName}.";
            }
            else
            {
                targetObject = existingRoots[0];
                stepMessage = $"{prefabName} already existed.";
            }

            Transform targetTransform = targetObject.transform;
            if (targetTransform.parent != setup.transform)
            {
                targetTransform.SetParent(setup.transform);
                changed = true;
                stepMessage = $"{prefabName} re-parented under BG_SETUP.";
            }

            if (targetTransform.localPosition != Vector3.zero)
            {
                targetTransform.localPosition = Vector3.zero;
                changed = true;
            }

            if (!changed)
            {
                steps?.Add($"{prefabName} already matched scene bootstrap.");
                return true;
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(setup);
            PrefabUtility.RecordPrefabInstancePropertyModifications(targetObject);
            EditorUtility.SetDirty(setup);
            EditorUtility.SetDirty(targetObject);

            Scene scene = setup.gameObject.scene;
            if (scene.IsValid() && !string.IsNullOrWhiteSpace(scene.path))
                dirtyScenePaths?.Add(scene.path);

            steps?.Add(stepMessage);
            return true;
        }

        public static void RemoveDebugOverlayInCurrentSceneIfExists(
            List<string> steps,
            HashSet<string> dirtyScenePaths)
        {
            RemoveSetupChildPrefab("Debug Overlay Canvas", null, steps, dirtyScenePaths, out _);
        }

        public static bool RemoveSetupChildPrefab(
            string prefabName,
            string componentName,
            List<string> steps,
            HashSet<string> dirtyScenePaths,
            out string errorMessage)
        {
            errorMessage = null;

            BG_SETUP setup = Object.FindFirstObjectByType<BG_SETUP>();
            if (setup == null)
            {
                steps?.Add($"Skipped removing {prefabName} because BG_SETUP was not found.");
                return true;
            }

            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var targets = new HashSet<GameObject>();
            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject candidate = allObjects[i];
                if (candidate == null || candidate.scene != setup.gameObject.scene)
                    continue;

                GameObject removalTarget = BuildToolV2SceneSetupMatching.ResolveRemovalTarget(candidate, prefabName, componentName);
                if (removalTarget != null)
                    targets.Add(removalTarget);
            }

            if (targets.Count == 0)
            {
                steps?.Add($"{prefabName} was already absent.");
                return true;
            }

            foreach (GameObject target in targets)
            {
                if (target != null)
                    Object.DestroyImmediate(target);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(setup);
            EditorUtility.SetDirty(setup);

            Scene scene = setup.gameObject.scene;
            if (scene.IsValid() && !string.IsNullOrWhiteSpace(scene.path))
                dirtyScenePaths?.Add(scene.path);

            steps?.Add($"Removed {prefabName} from the primary scene.");
            return true;
        }
    }
}
