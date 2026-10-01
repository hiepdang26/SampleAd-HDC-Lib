using AppBootstrap.Splash;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Editor
{
    internal static class CanvasScalerJsonMenuItems
    {
        private const string ExportMenuPath = "Tools/CanvasScaler/Debug Selected To Json";

        [MenuItem(ExportMenuPath)]
        private static void DebugSelectedToJson()
        {
            var canvasScaler = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<CanvasScaler>()
                : null;

            if (canvasScaler == null)
            {
                Debug.LogWarning("[CanvasScalerJson] Selected object does not contain CanvasScaler.");
                return;
            }

            try
            {
                string json = CanvasScalerJsonUtility.ExportToJson(canvasScaler);
                Debug.Log($"[CanvasScalerJson] Selected: {canvasScaler.name}\n{json}", canvasScaler);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CanvasScalerJson] Export failed: {ex.Message}", canvasScaler);
            }
        }

        [MenuItem(ExportMenuPath, true)]
        private static bool ValidateDebugSelectedToJson()
        {
            return Selection.activeGameObject != null
                && Selection.activeGameObject.GetComponent<CanvasScaler>() != null;
        }
    }
}
