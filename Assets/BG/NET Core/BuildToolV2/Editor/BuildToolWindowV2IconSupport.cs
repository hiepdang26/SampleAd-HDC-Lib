using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2IconSupport
    {
        private const float AdaptiveIconSafeAreaScale = 0.75f;
        private const int DefaultIconObjectPickerControlId = 420731;

        public static void DrawIconPreviewTile(Texture2D texture, float size)
        {
            Rect previewRect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            GUI.Box(previewRect, GUIContent.none);
            if (texture != null)
                GUI.DrawTexture(previewRect, texture, ScaleMode.ScaleToFit, true);
        }

        public static void DrawLabeledIconPreview(string label, Texture2D texture, float size)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(size + 8f)))
            {
                EditorGUILayout.LabelField(label, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(size + 8f));
                DrawIconPreviewTile(texture, size);
            }
        }

        public static void RefreshIconSelectionFromCurrent(BuildToolWindowV2 owner, bool force = false)
        {
            if (owner.profile == null)
                return;

            if (!force && owner.selectedSourceIcon != null)
                return;

            if (force)
                BuildToolV2Utilities.ClearPrimaryIconCache();

            owner.selectedSourceIcon = BuildToolV2Utilities.GetPrimaryAppIcon(owner.profile.BuildTarget);
            RefreshAdaptivePreviewTextures(owner);
        }

        public static void RefreshAdaptivePreviewTextures(BuildToolWindowV2 owner)
        {
            DestroyAdaptivePreviewTextures(owner);

            if (owner.selectedSourceIcon == null)
            {
                owner.selectedSourceIconName = string.Empty;
                owner.adaptiveStatusMessage = "No icon found in Player Settings.";
                owner.adaptiveStatusApplied = false;
                return;
            }

            owner.adaptiveInsetPreview = BuildToolV2Utilities.CreateAdaptiveInsetPreviewTexture(owner.selectedSourceIcon, AdaptiveIconSafeAreaScale);
            owner.adaptiveAppliedPreview = BuildToolV2Utilities.CreateAdaptiveAppliedPreviewTexture(owner.selectedSourceIcon, AdaptiveIconSafeAreaScale);
            owner.selectedSourceIconName = owner.selectedSourceIcon.name;
            owner.adaptiveStatusApplied = BuildToolV2Utilities.IsAdaptiveIconApplied(owner.selectedSourceIcon, out string message);
            owner.adaptiveStatusMessage = string.IsNullOrWhiteSpace(message) ? "Adaptive icon status is unavailable." : message;
        }

        public static void ShowDefaultIconObjectPicker(BuildToolWindowV2 owner)
        {
            EditorGUIUtility.ShowObjectPicker<Texture2D>(owner.selectedSourceIcon, false, string.Empty, DefaultIconObjectPickerControlId);
        }

        public static void HandleDefaultIconObjectPickerEvent(BuildToolWindowV2 owner)
        {
            Event currentEvent = Event.current;
            if (currentEvent == null)
                return;

            if (currentEvent.commandName != "ObjectSelectorUpdated" && currentEvent.commandName != "ObjectSelectorClosed")
                return;

            if (EditorGUIUtility.GetObjectPickerControlID() != DefaultIconObjectPickerControlId)
                return;

            Texture2D pickedIcon = EditorGUIUtility.GetObjectPickerObject() as Texture2D;
            if (pickedIcon == null)
                return;

            ApplyDefaultIconSelection(owner, pickedIcon);
        }

        public static void ApplyDefaultIconSelection(BuildToolWindowV2 owner, Texture2D icon)
        {
            if (!BuildToolV2Utilities.TrySetDefaultAppIcon(icon, out string errorMessage))
            {
                EditorUtility.DisplayDialog("Default Icon", errorMessage, "OK");
                return;
            }

            BuildToolV2Utilities.ClearEditorCaches();
            RefreshIconSelectionFromCurrent(owner, true);
            owner.Repaint();
        }

        public static void DestroyAdaptivePreviewTextures(BuildToolWindowV2 owner)
        {
            if (owner.adaptiveInsetPreview != null)
                Object.DestroyImmediate(owner.adaptiveInsetPreview);
            if (owner.adaptiveAppliedPreview != null)
                Object.DestroyImmediate(owner.adaptiveAppliedPreview);

            owner.adaptiveInsetPreview = null;
            owner.adaptiveAppliedPreview = null;
        }

        public static void ApplyAdaptiveIconSelection(BuildToolWindowV2 owner)
        {
            if (!BuildToolV2Utilities.TryGenerateAndApplyAdaptiveIcon(owner.selectedSourceIcon, AdaptiveIconSafeAreaScale, out string errorMessage))
            {
                EditorUtility.DisplayDialog("Adaptive Icon", errorMessage, "OK");
                return;
            }

            BuildToolV2Utilities.ClearEditorCaches();
            RefreshIconSelectionFromCurrent(owner, true);
            owner.Repaint();
        }
    }
}
