using BG_Library.Common;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BG_Library.Common
{
    internal static class BGSetupEditorUtilities
    {
        public static BG_SETUP CreateSetupInCurrentScene()
        {
            GameObject setupPrefab = FindPrefabWithComponent("BLACKGEMS SETUP", nameof(BG_SETUP));
            if (setupPrefab == null)
            {
                Debug.LogError("Khong tim thay prefab BLACKGEMS SETUP trong project.");
                return null;
            }

            GameObject setupObject = (GameObject)PrefabUtility.InstantiatePrefab(setupPrefab);
            setupObject.transform.position = Vector3.zero;

            var setup = setupObject.GetComponent<BG_SETUP>();
            EditorSceneManager.MarkSceneDirty(setupObject.scene);
            EditorSceneManager.SaveScene(setupObject.scene);
            return setup;
        }

        public static GameObject FindPrefabWithComponent(string prefabName, string componentName)
        {
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null || prefab.name != prefabName)
                    continue;

                if (string.IsNullOrWhiteSpace(componentName))
                    return prefab;

                if (HasComponent(prefab, componentName))
                    return prefab;
            }

            return null;
        }

        private static bool HasComponent(GameObject target, string componentName)
        {
            if (target == null || string.IsNullOrWhiteSpace(componentName))
                return false;

            Component[] components = target.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component != null && component.GetType().Name == componentName)
                    return true;
            }

            return false;
        }
    }
}
