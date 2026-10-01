using UnityEditor;
using UnityEngine;

namespace BG_Library.Tools
{
    public static class PlayModeControl
    {
        private const string ToggleMenu = "BG/Tools/Play Mode/Toggle";
        private const string EnterMenu = "BG/Tools/Play Mode/Enter";
        private const string ExitMenu = "BG/Tools/Play Mode/Exit";

        [MenuItem(ToggleMenu, priority = 1500)]
        private static void TogglePlayMode()
        {
            RequestPlayModeChange(!EditorApplication.isPlaying);
        }

        [MenuItem(EnterMenu, priority = 1501)]
        private static void EnterPlayMode()
        {
            RequestPlayModeChange(true);
        }

        [MenuItem(ExitMenu, priority = 1502)]
        private static void ExitPlayMode()
        {
            RequestPlayModeChange(false);
        }

        [MenuItem(ToggleMenu, true)]
        [MenuItem(EnterMenu, true)]
        [MenuItem(ExitMenu, true)]
        private static bool ValidatePlayModeMenus()
        {
            return !EditorApplication.isCompiling && !EditorApplication.isUpdating;
        }

        private static void RequestPlayModeChange(bool shouldPlay)
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Debug.LogWarning("[BG Tools] Cannot change Play Mode while Unity is compiling or updating.");
                return;
            }

            if (EditorApplication.isPlaying == shouldPlay)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    Debug.LogWarning("[BG Tools] Play Mode change was skipped because Unity started compiling or updating.");
                    return;
                }

                EditorApplication.isPlaying = shouldPlay;
            };
        }
    }
}
