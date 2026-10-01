using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BG_Library.DEBUG.Editor
{
    public static class DebugSectionsAutoBindTool
    {
        [MenuItem("BG/Tools/Debug UI/Auto Bind Sections")]
        public static void AutoBindSections()
        {
            int boundCount = 0;
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var behaviour in behaviours)
            {
                if (behaviour == null)
                    continue;

                var method = behaviour.GetType().GetMethod("AutoBindReferences", BindingFlags.Instance | BindingFlags.Public);
                if (method == null)
                    continue;

                method.Invoke(behaviour, null);
                EditorUtility.SetDirty(behaviour);
                boundCount++;
            }

            if (boundCount > 0)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"[Debug UI] Auto Bind Sections finished. Bound={boundCount}");
        }
    }
}
