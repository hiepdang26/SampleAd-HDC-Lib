using AppBootstrap.Splash;
using UnityEditor;
using UnityEngine;

namespace AppBootstrap.Editor
{
    internal static class RectTransformJsonMenuItems
    {
        private const string ExportMenuPath = "Tools/RectTransform/Debug Selected To Json";

        [MenuItem(ExportMenuPath)]
        private static void DebugSelectedToJson()
        {
            var rectTransform = Selection.activeTransform as RectTransform;
            if (rectTransform == null)
            {
                Debug.LogWarning("[RectTransformJson] Selected object is not a RectTransform.");
                return;
            }

            string json = RectTransformJsonUtility.ExportToJson(rectTransform);
            Debug.Log($"[RectTransformJson] Selected: {rectTransform.name}\n{json}", rectTransform);
        }

        [MenuItem(ExportMenuPath, true)]
        private static bool ValidateDebugSelectedToJson()
        {
            return Selection.activeTransform is RectTransform;
        }
    }
}
