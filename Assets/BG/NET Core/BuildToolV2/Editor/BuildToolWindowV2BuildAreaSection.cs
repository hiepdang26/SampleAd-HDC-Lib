using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2BuildAreaSection
    {
        internal static void DrawBottomPanel(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
                return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                BuildToolWindowV2Ui.DrawColoredDivider(new Color(0.85f, 0.58f, 0.18f, 1f), 3f, 0f);
                GUILayout.Label("Build Area", EditorStyles.boldLabel);

                if (BuildToolWindowV2Ui.DrawFoldoutHeader("Device Info", BuildToolWindowV2.BuildAreaDeviceFoldoutKey))
                    owner.DrawBottomDeviceSection();

                EditorGUILayout.Space(6f);
                BuildToolWindowV2Ui.DrawWhiteDivider();
                EditorGUILayout.Space(4f);

                if (BuildToolWindowV2Ui.DrawFoldoutHeader("Network", BuildToolWindowV2.BuildAreaNetworkFoldoutKey))
                    owner.DrawBottomNetworkSection();

                EditorGUILayout.Space(6f);
                BuildToolWindowV2Ui.DrawWhiteDivider();
                EditorGUILayout.Space(4f);

                if (BuildToolWindowV2Ui.DrawFoldoutHeader("Actions", BuildToolWindowV2.BuildAreaActionsFoldoutKey))
                    DrawBottomActions(owner);

                EditorGUILayout.Space(6f);
                BuildToolWindowV2Ui.DrawWhiteDivider();
                EditorGUILayout.Space(4f);

                if (!BuildToolWindowV2Ui.DrawFoldoutHeader("Info", BuildToolWindowV2.BuildAreaInfoFoldoutKey))
                    return;

                if (owner.lastExecutionResult == null)
                {
                    EditorGUILayout.HelpBox("No build or report generated in this session yet.", MessageType.Info);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(owner.lastExecutionResult.ErrorMessage))
                    EditorGUILayout.HelpBox(owner.lastExecutionResult.ErrorMessage, MessageType.Warning);

                string resultLabel = owner.lastExecutionResult.IsReportOnly
                    ? "Report Only"
                    : owner.lastExecutionResult.Report == null
                        ? "No BuildReport"
                        : owner.lastExecutionResult.Report.summary.result.ToString();
                BuildToolWindowV2Ui.DrawSummaryRow("Last Result", resultLabel);

                if (!string.IsNullOrWhiteSpace(owner.lastExecutionResult.OutputPath))
                    BuildToolWindowV2Ui.DrawSummaryRow("Output", owner.lastExecutionResult.OutputPath);
                if (!string.IsNullOrWhiteSpace(owner.lastExecutionResult.SummaryPath))
                    BuildToolWindowV2Ui.DrawSummaryRow("Report", owner.lastExecutionResult.SummaryPath);
                if (!string.IsNullOrWhiteSpace(owner.lastExecutionResult.RunDeviceSummary))
                    BuildToolWindowV2Ui.DrawSummaryRow("Run Device", owner.lastExecutionResult.RunDeviceSummary);
                if (!string.IsNullOrWhiteSpace(owner.lastExecutionResult.PreparationSummary))
                    BuildToolWindowV2Ui.DrawSummaryRow("Preparation", owner.lastExecutionResult.PreparationSummary);

                if (owner.lastExecutionResult.WasCancelled)
                    EditorGUILayout.HelpBox("Build was cancelled.", MessageType.Info);
            }
        }

        internal static void DrawBottomActions(BuildToolWindowV2 owner)
        {
            DrawBuildAreaRow(
                "1.",
                () =>
                {
                    if (DrawActionButton("Generate Report Manually", 200f, "Export a report snapshot of the current build configuration without starting a build."))
                        owner.GenerateReport();
                });

            BuildToolWindowV2Ui.DrawWhiteDivider();

            DrawBuildAreaRow(
                "2.",
                () =>
                {
                    bool canBuild = owner.validationResult == null || owner.validationResult.CanBuild;
                    bool autoOpenAfterBuildAndRunValue = owner.scrcpySettings != null && owner.scrcpySettings.autoOpenAfterBuildAndRun;
                    using (new EditorGUI.DisabledScope(!canBuild))
                    {
                        if (DrawActionButton("Build", 110f, "Build the current preset to the selected output path."))
                            owner.RunBuild(false);

                        if (DrawActionButton("Build & Run", 130f, "Build the current preset, then install and run it on the selected Android device."))
                            owner.RunBuild(true);
                    }

                    EditorGUI.BeginChangeCheck();
                    bool autoOpenAfterBuildAndRun = DrawActionToggleLeft(
                        "Auto Open After Build & Run",
                        autoOpenAfterBuildAndRunValue,
                        190f,
                        "When enabled, automatically open the phone preview after Build & Run finishes.");
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (owner.scrcpySettings == null)
                            owner.RefreshScrcpySettings();
                        owner.scrcpySettings.autoOpenAfterBuildAndRun = autoOpenAfterBuildAndRun;
                        owner.SaveScrcpySettings();
                    }
                });

            BuildToolWindowV2Ui.DrawWhiteDivider();

            DrawBuildAreaRow(
                "3.",
                () =>
                {
                    BuildAndroidDeviceInfo device = owner.GetSelectedDevice();
                    bool hasAndroidDevice = owner.profile != null && owner.profile.BuildTarget == BuildTarget.Android && device != null;
                    string packageName = !string.IsNullOrWhiteSpace(owner.summaryDraft?.PackageName)
                        ? owner.summaryDraft.PackageName
                        : (owner.profile?.PackageName ?? string.Empty);

                    using (new EditorGUI.DisabledScope(!hasAndroidDevice || string.IsNullOrWhiteSpace(owner.lastExecutionResult?.OutputPath)))
                    {
                        if (DrawActionButton("Install Build", 100f, "Install the latest built artifact onto the selected Android device."))
                        {
                            bool success = BuildToolV2Utilities.InstallBuildArtifact(device, owner.lastExecutionResult.OutputPath, out string message);
                            owner.ShowNotification(new GUIContent(string.IsNullOrWhiteSpace(message) ? "Install command finished." : message));
                            if (!success)
                                Debug.LogWarning(message);
                        }
                    }

                    using (new EditorGUI.DisabledScope(!hasAndroidDevice || string.IsNullOrWhiteSpace(packageName)))
                    {
                        if (DrawActionButton("Uninstall App", 100f, "Uninstall the app with the current Android package name from the selected device."))
                        {
                            bool success = BuildToolV2Utilities.UninstallApplication(device, packageName, out string message);
                            owner.ShowNotification(new GUIContent(string.IsNullOrWhiteSpace(message) ? "Uninstall command finished." : message));
                            if (!success)
                                Debug.LogWarning(message);
                        }
                    }

                    using (new EditorGUI.DisabledScope(!hasAndroidDevice || string.IsNullOrWhiteSpace(packageName)))
                    {
                        if (DrawActionButton("Launch App", 90f, "Launch the app with the current Android package name on the selected device."))
                        {
                            bool success = BuildToolV2Utilities.LaunchApplication(device, packageName, out string message);
                            owner.ShowNotification(new GUIContent(string.IsNullOrWhiteSpace(message) ? "Launch command sent." : message));
                            if (!success)
                                Debug.LogWarning(message);
                        }

                        if (DrawActionButton("Exit App", 90f, "Force stop the current app on the selected Android device."))
                        {
                            bool success = BuildToolV2Utilities.ForceStopApplication(device, packageName, out string message);
                            owner.ShowNotification(new GUIContent(string.IsNullOrWhiteSpace(message) ? "Stop command sent." : message));
                            if (!success)
                                Debug.LogWarning(message);
                        }
                    }

                    using (new EditorGUI.DisabledScope(!hasAndroidDevice || string.IsNullOrWhiteSpace(packageName)))
                    {
                        if (DrawActionButton("Clear Data", 90f, "Clear all app data for the current Android package name on the selected device."))
                        {
                            bool success = BuildToolV2Utilities.ClearApplicationData(device, packageName, out string message);
                            owner.ShowNotification(new GUIContent(string.IsNullOrWhiteSpace(message) ? "Clear data command finished." : message));
                            if (!success)
                                Debug.LogWarning(message);
                        }
                    }
                });

            BuildToolWindowV2Ui.DrawWhiteDivider();

            DrawBuildAreaRow(
                "4.",
                () =>
                {
                    BuildAndroidDeviceInfo device = owner.GetSelectedDevice();
                    bool hasAndroidDevice = owner.profile != null && owner.profile.BuildTarget == BuildTarget.Android && device != null;
                    bool hasScrcpy = hasAndroidDevice && BuildToolV2Utilities.HasScrcpyExecutable(owner.scrcpySettings);

                    using (new EditorGUI.DisabledScope(!hasScrcpy))
                    {
                        if (DrawActionButton("Open Preview", 100f, "Open phone preview for the selected Android device using scrcpy."))
                            owner.OpenScrcpyPreview(device, false);

                        if (DrawActionButton("Restart Preview", 110f, "Restart the phone preview process for the selected Android device."))
                            owner.OpenScrcpyPreview(device, true);
                    }

                    using (new EditorGUI.DisabledScope(!hasAndroidDevice || !BuildToolV2Utilities.IsScrcpyRunning(device.Serial)))
                    {
                        if (DrawActionButton("Stop Preview", 100f, "Stop the running scrcpy preview for the selected device."))
                        {
                            bool stopped = BuildToolV2Utilities.StopScrcpy(device.Serial);
                            owner.SetScrcpyMessage(stopped ? "Preview stopped." : "No tracked preview is running for the selected device.", stopped ? MessageType.Info : MessageType.Warning);
                        }
                    }

                    using (new EditorGUI.DisabledScope(!hasAndroidDevice))
                    {
                        if (DrawActionButton("Turn Screen Off", 110f, "Send a command to turn off the screen on the selected Android device."))
                        {
                            bool success = BuildToolV2Utilities.TurnDeviceScreenOff(device, out string message);
                            owner.ShowNotification(new GUIContent(string.IsNullOrWhiteSpace(message) ? "Screen off command sent." : message));
                            if (!success)
                                Debug.LogWarning(message);
                        }
                    }
                });

            BuildToolWindowV2Ui.DrawWhiteDivider();

            DrawBuildAreaRow(
                "5.",
                () =>
                {
                    using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(owner.lastExecutionResult?.OutputPath) && (string.IsNullOrWhiteSpace(owner.profile?.OutputDirectory) || !Directory.Exists(owner.profile.OutputDirectory))))
                    {
                        if (DrawActionButton("Open Build Folder", 150f, "Open the current build output folder or select the latest built artifact."))
                            OpenBuildOutputLocation(owner);
                    }

                    using (new EditorGUI.DisabledScope(owner.lastExecutionResult == null || string.IsNullOrWhiteSpace(owner.lastExecutionResult.SummaryPath)))
                    {
                        if (DrawActionButton("Open Report", 120f, "Select the most recently generated build/report file in Explorer."))
                            BuildToolV2Utilities.OpenAndSelectPath(owner.lastExecutionResult.SummaryPath);
                    }
                });
        }

        internal static void DrawBuildAreaRow(string indexLabel, Action drawButtons)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(indexLabel, GUILayout.Width(24f));

                using (new EditorGUILayout.HorizontalScope())
                {
                    drawButtons?.Invoke();
                }
            }
        }

        private static bool DrawActionButton(string label, float width, string tooltip)
        {
            return GUILayout.Button(new GUIContent(label, tooltip), GUILayout.Width(width));
        }

        private static bool DrawActionToggleLeft(string label, bool value, float width, string tooltip)
        {
            return EditorGUILayout.ToggleLeft(new GUIContent(label, tooltip), value, GUILayout.Width(width));
        }

        internal static void OpenBuildOutputLocation(BuildToolWindowV2 owner)
        {
            if (!string.IsNullOrWhiteSpace(owner.lastExecutionResult?.OutputPath))
            {
                BuildToolV2Utilities.OpenAndSelectPath(owner.lastExecutionResult.OutputPath);
                return;
            }

            if (!string.IsNullOrWhiteSpace(owner.profile?.OutputDirectory))
                BuildToolV2Utilities.OpenFolderInExplorer(owner.profile.OutputDirectory);
        }
    }
}
