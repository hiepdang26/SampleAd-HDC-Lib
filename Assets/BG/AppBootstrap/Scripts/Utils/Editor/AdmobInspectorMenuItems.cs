using AppBootstrap.Splash;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AppBootstrap.Editor
{
    internal static class AdmobInspectorMenuItems
    {
        private const string AddMenuPath = "BG/Tools/Add MetaAndInspector To Open Scene";
        private const string ObjectName = "MetaAndInspector";

        [MenuItem(AddMenuPath)]
        private static void AddToOpenScene()
        {
            var inspector = Object.FindObjectOfType<AdmobInspector>(true);
            GameObject target;
            if (inspector != null)
            {
                // Upgrade an existing AdmobInspector object instead of adding a second one.
                target = inspector.gameObject;
                Undo.RecordObject(target, "Upgrade MetaAndInspector");
                target.name = ObjectName;
                if (target.GetComponent<AdMetaToggle>() == null)
                    Undo.AddComponent<AdMetaToggle>(target);
            }
            else
            {
                target = new GameObject(ObjectName, typeof(AdmobInspector), typeof(AdMetaToggle));
                Undo.RegisterCreatedObjectUndo(target, "Add MetaAndInspector");
            }

            EditorSceneManager.MarkSceneDirty(target.scene);
            Selection.activeGameObject = target;
            Debug.Log($"[MetaAndInspector] Ready in scene {target.scene.name}. Save the scene to keep it.", target);
        }

        [MenuItem(AddMenuPath, true)]
        private static bool ValidateAddToOpenScene()
        {
            return !EditorApplication.isPlaying;
        }
    }
}
