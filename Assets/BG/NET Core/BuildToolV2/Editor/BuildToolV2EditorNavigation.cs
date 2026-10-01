using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2EditorNavigation
    {
        public static void PingObject(Object target)
        {
            if (target == null)
                return;

            Selection.activeObject = target;
            EditorGUIUtility.PingObject(target);
        }

        public static void OpenLockedInspector(Object target)
        {
            if (target == null)
                return;

            EditorApplication.delayCall += () =>
            {
                System.Type inspectorWindowType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
                if (inspectorWindowType == null)
                    return;

                Object[] previousSelection = Selection.objects;
                Selection.objects = new[] { target };

                EditorWindow inspectorWindow = ScriptableObject.CreateInstance(inspectorWindowType) as EditorWindow;
                if (inspectorWindow == null)
                {
                    RestorePreviousSelection(previousSelection);
                    return;
                }

                inspectorWindow.minSize = new Vector2(360f, 480f);
                inspectorWindow.Show();
                inspectorWindow.Focus();

                PropertyInfo isLockedProperty = inspectorWindowType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (isLockedProperty != null && isLockedProperty.CanWrite)
                    isLockedProperty.SetValue(inspectorWindow, true, null);

                inspectorWindow.Repaint();

                EditorApplication.delayCall += () => RestorePreviousSelection(previousSelection);
            };
        }

        private static void RestorePreviousSelection(Object[] previousSelection)
        {
            if (previousSelection == null || previousSelection.Length == 0)
                Selection.activeObject = null;
            else
                Selection.objects = previousSelection;
        }

        public static void PingAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return;

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            PingObject(asset);
        }

        public static void OpenPlayerSettings()
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
    }
}
