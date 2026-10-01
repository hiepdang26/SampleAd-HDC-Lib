using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SceneNavigation
    {
        public static bool TryOpenSceneSingle(string scenePath, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(scenePath))
            {
                errorMessage = "Scene path rong.";
                return false;
            }

            if (!File.Exists(scenePath))
            {
                errorMessage = $"Khong tim thay scene: {scenePath}";
                return false;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == scenePath)
                return true;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                errorMessage = "Da huy mo scene can fix.";
                return false;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            return true;
        }

        public static string GetSceneDisplayName(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
                return "(missing)";

            return Path.GetFileNameWithoutExtension(scenePath);
        }

        public static bool TryPingFirstInCurrentScene<T>(out string errorMessage) where T : Component
        {
            errorMessage = null;

            T component = Object.FindFirstObjectByType<T>();
            if (component == null)
            {
                errorMessage = $"Khong tim thay {typeof(T).Name} trong scene hien tai.";
                return false;
            }

            Selection.activeObject = component.gameObject;
            EditorGUIUtility.PingObject(component.gameObject);
            return true;
        }

        public static string GetPrimaryEnabledScenePath()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes == null)
                return string.Empty;

            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled)
                    continue;

                return scenes[i].path ?? string.Empty;
            }

            return string.Empty;
        }

        public static string[] GetEnabledScenes()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            int count = 0;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                    count++;
            }

            var result = new string[count];
            int index = 0;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (!scenes[i].enabled)
                    continue;

                result[index++] = scenes[i].path;
            }

            return result;
        }
    }
}
