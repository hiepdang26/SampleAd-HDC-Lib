using System;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2ValidationSection
    {
        internal static void Draw(BuildToolWindowV2 owner)
        {
            BuildValidationResult result = owner.validationResult ?? new BuildValidationResult();

            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (!BuildToolWindowV2Ui.DrawFoldoutHeader("Validation Scan", BuildToolWindowV2.ValidationScanFoldoutKey, GetValidationStatusIcon(result)))
                    return;

                EditorGUILayout.HelpBox(
                    "Validation Scan không tự động chạy khi Unity restore cửa sổ Build Tool V2. Bấm Refresh để chạy scan thủ công. Tool hiện chỉ kiểm tra scene 0 và báo rõ scene 0 đang thiếu hoặc lệch gì.",
                    MessageType.None);

                if (owner.validationScanStale)
                {
                    EditorGUILayout.HelpBox(
                        owner.validationResult == null
                            ? "Validation Scan của preset hiện tại chưa được cập nhật. Bấm Refresh để chạy scan."
                            : "Danh sách bên dưới là kết quả scan trước đó. Sau các thay đổi gần đây hoặc sau khi Unity restore cửa sổ, hãy bấm Refresh để cập nhật lại scan.",
                        MessageType.Warning);
                }

                EditorGUILayout.LabelField($"Errors: {result.ErrorCount} | Warnings: {result.WarningCount}", EditorStyles.boldLabel);

                if (result.Checks.Count == 0)
                {
                    EditorGUILayout.HelpBox("Chưa có phần tử check nào để hiển thị.", MessageType.Info);
                    return;
                }

                float listHeight = Mathf.Min(Mathf.Max(156f, result.Checks.Count * 72f), 340f);
                using (var scrollView = new EditorGUILayout.ScrollViewScope(owner.checksScrollPosition, "box", GUILayout.Height(listHeight)))
                {
                    owner.checksScrollPosition = scrollView.scrollPosition;
                    for (int i = 0; i < result.Checks.Count; i++)
                        DrawValidationCheckItem(owner, result.Checks[i]);
                }
            }
        }

        internal static void RefreshChecks(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
                return;

            owner.validationResult = BuildToolV2ValidationRunner.Run(owner.profile);
            owner.validationScanStale = false;
            owner.Repaint();
        }

        internal static void MarkScanStale(BuildToolWindowV2 owner, bool clearResult = false)
        {
            owner.validationScanStale = true;
            if (clearResult)
                owner.validationResult = null;
        }

        private static void DrawValidationCheckItem(BuildToolWindowV2 owner, BuildCheckItem item)
        {
            if (item == null)
                return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(item.Name ?? "-", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(GetCheckStatusLabel(item.Status), GUILayout.Width(72f));

                    using (new EditorGUI.DisabledScope(item.Action == null || string.IsNullOrWhiteSpace(item.ActionLabel)))
                    {
                        if (GUILayout.Button(string.IsNullOrWhiteSpace(item.ActionLabel) ? "No Action" : item.ActionLabel, GUILayout.Width(120f)))
                        {
                            item.Action?.Invoke();
                            BuildToolV2Utilities.ClearEditorCaches();
                            MarkScanStale(owner);
                            owner.RefreshWindowData(false, false, false);
                            GUIUtility.ExitGUI();
                        }
                    }
                }

                EditorGUILayout.LabelField(item.Description ?? "-", EditorStyles.wordWrappedMiniLabel);

                if (!string.IsNullOrWhiteSpace(item.StatusMessage) && !string.Equals(item.StatusMessage, "OK", StringComparison.Ordinal))
                    EditorGUILayout.HelpBox(item.StatusMessage, item.Status == BuildCheckStatus.Error ? MessageType.Error : MessageType.Warning);
            }
        }

        private static string GetCheckStatusLabel(BuildCheckStatus status)
        {
            switch (status)
            {
                case BuildCheckStatus.Error:
                    return "ERROR";
                case BuildCheckStatus.Warning:
                    return "WARNING";
                default:
                    return "NORMAL";
            }
        }

        private static Texture GetValidationStatusIcon(BuildValidationResult result)
        {
            if (result == null)
                return null;

            if (result.ErrorCount > 0)
                return EditorGUIUtility.IconContent("console.erroricon.sml").image;

            if (result.WarningCount > 0)
                return EditorGUIUtility.IconContent("console.warnicon.sml").image;

            return EditorGUIUtility.IconContent("TestPassed").image;
        }
    }
}
