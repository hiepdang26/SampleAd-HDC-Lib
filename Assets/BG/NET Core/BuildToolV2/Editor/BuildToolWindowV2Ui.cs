using System;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2Ui
    {
        internal static void DrawSummaryCard(string title, Action drawBody, string foldoutKey = null)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (string.IsNullOrWhiteSpace(foldoutKey))
                {
                    GUILayout.Label(title, EditorStyles.boldLabel);
                    EditorGUILayout.Space(2f);
                }
                else
                {
                    if (!DrawFoldoutHeader(title, foldoutKey))
                        return;
                }

                drawBody?.Invoke();
            }

            EditorGUILayout.Space(4f);
        }

        internal static void DrawColoredDivider(Color color, float thickness, float spacing)
        {
            if (spacing > 0f)
                EditorGUILayout.Space(spacing);

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(thickness), GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, color);

            if (spacing > 0f)
                EditorGUILayout.Space(spacing);
        }

        internal static void DrawWhiteDivider()
        {
            DrawColoredDivider(new Color(1f, 1f, 1f, 0.32f), 1f, 1f);
        }

        internal static void DrawSummaryRow(
            string label,
            string value,
            string buttonLabel = null,
            Action buttonAction = null,
            float buttonWidth = 90f,
            string secondaryButtonLabel = null,
            Action secondaryButtonAction = null,
            float secondaryButtonWidth = 90f)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(150f));
                GUILayout.Label(string.IsNullOrWhiteSpace(value) ? "-" : value, EditorStyles.wordWrappedLabel);

                if (buttonAction != null && !string.IsNullOrWhiteSpace(buttonLabel))
                {
                    if (GUILayout.Button(buttonLabel, GUILayout.Width(buttonWidth)))
                        buttonAction.Invoke();
                }

                if (secondaryButtonAction != null && !string.IsNullOrWhiteSpace(secondaryButtonLabel))
                {
                    if (GUILayout.Button(secondaryButtonLabel, GUILayout.Width(secondaryButtonWidth)))
                        secondaryButtonAction.Invoke();
                }
            }
        }

        internal static void DrawStringList(string label, string[] items)
        {
            string value = items == null || items.Length == 0 ? "-" : string.Join("\n", items);
            DrawSummaryRow(label, value);
        }

        internal static bool DrawFoldoutHeader(string label, string key, Texture icon = null)
        {
            string prefsKey = $"{Application.productName}_{key}";
            bool expanded = EditorPrefs.GetBool(prefsKey, false);
            GUIContent content = icon == null ? new GUIContent(label) : new GUIContent(label, icon);
            bool next = EditorGUILayout.Foldout(expanded, content, true, EditorStyles.foldoutHeader);
            if (next != expanded)
                EditorPrefs.SetBool(prefsKey, next);

            return next;
        }
    }
}
