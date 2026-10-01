using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2TopSection
    {
        internal static void DrawToolbar(BuildToolWindowV2 owner)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Focused build flow for NET-linked builds", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Player Settings", EditorStyles.toolbarButton, GUILayout.Width(120f)))
                    SettingsService.OpenProjectSettings("Project/Player");

                if (GUILayout.Button("Configs SO", EditorStyles.toolbarButton, GUILayout.Width(120f)))
                {
                    NetConfigsSO netConfigs = owner.profile != null ? owner.profile.ResolveNetConfigs() : null;
                    if (netConfigs != null)
                        BuildToolV2Utilities.OpenLockedInspector(netConfigs);
                }

                if (GUILayout.Button("Refresh Ctrl + R", EditorStyles.toolbarButton, GUILayout.Width(125f)))
                    owner.RefreshWindowData(true, refreshDevices: true, refreshValidation: true);
            }
        }

        internal static void DrawTopOverviewSection(BuildToolWindowV2 owner)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (owner.profile == null)
                {
                    EditorGUILayout.HelpBox("Khong co preset build nao. Tool se tu tao preset mac dinh khi mo lai cua so.", MessageType.Info);
                    return;
                }

                DrawHeaderOverviewCard(owner);
            }
        }

        internal static void DrawPresetSectionClean(BuildToolWindowV2 owner)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                owner.DrawProfilePicker();

                if (owner.profile == null)
                {
                    EditorGUILayout.HelpBox("Khong co preset build nao. Tool se tu tao preset mac dinh khi mo lai cua so.", MessageType.Info);
                    return;
                }

                BuildToolWindowV2.HeaderOverviewState snapshot = owner.headerOverviewState ?? new BuildToolWindowV2.HeaderOverviewState();
                string message = snapshot.HasManagedSourceMismatch
                    ? "Preset hien tai dang lech so voi nguon that (Configs SO / SRDebugger). Bam Reset neu muon dua nguon ve dung preset."
                    : "Preset hien tai dang trung voi nguon that.";
                MessageType type = snapshot.HasManagedSourceMismatch ? MessageType.Warning : MessageType.Info;
                EditorGUILayout.HelpBox(message, type);

                if (snapshot.HasManagedSourceMismatch && snapshot.ManagedSourceMismatchSummaries != null && snapshot.ManagedSourceMismatchSummaries.Length > 0)
                    EditorGUILayout.HelpBox(string.Join(" | ", snapshot.ManagedSourceMismatchSummaries), MessageType.None);

                DrawPresetRulesCard(owner);
            }
        }

        internal static void DrawPresetRulesCard(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
                return;

            BuildToolWindowV2.HeaderOverviewState snapshot = owner.headerOverviewState ?? new BuildToolWindowV2.HeaderOverviewState();
            BuildProfileManagedSourceState expected = owner.profile.GetExpectedManagedSourceState();

            EditorGUILayout.Space(2f);
            if (!BuildToolWindowV2Ui.DrawFoldoutHeader("Preset", BuildToolWindowV2.PresetRulesFoldoutKey))
                return;

            EditorGUILayout.HelpBox(
                "Preset nay dieu khien build type, NET debug preset, hack, AdMob test mode, Adjust env, overlay debug va rule bundle/split.",
                MessageType.None);

            if (owner.profile.IsReleaseLikePreset)
            {
                EditorGUI.BeginChangeCheck();
                bool releaseDebugEnabled = EditorGUILayout.Toggle("Debug", owner.profile.ReleaseDebugEnabled);
                if (EditorGUI.EndChangeCheck())
                {
                    owner.profile.SetReleaseDebugEnabled(releaseDebugEnabled);
                    BuildToolV2Utilities.ApplyPresetManagedSourcesImmediately(owner.profile);
                    BuildToolV2Utilities.ClearEditorCaches();
                    owner.MarkValidationScanStale();
                    owner.lastExecutionResult = null;
                    owner.RefreshWindowData(false, false, false);
                    GUIUtility.ExitGUI();
                }

                if (owner.profile.ReleaseDebugEnabled)
                {
                    EditorGUILayout.HelpBox(
                        "Release preset dang bat debug cuong buc: Full Debug, Debug Overlay Canvas, SRDebugger va Unity logger se hoat dong nhu Test Debug.",
                        MessageType.Warning);
                }
            }

            BuildToolWindowV2Ui.DrawSummaryRow("Developer Build", owner.profile.PresetDevelopmentBuild ? "ON" : "OFF");
            BuildToolWindowV2Ui.DrawSummaryRow("Hack", FormatManagedValue(snapshot.Hack, expected.BuildHack ? "ON" : "OFF"));
            BuildToolWindowV2Ui.DrawSummaryRow("Debug Preset", FormatManagedValue(snapshot.DebugPreset, expected.DebugPreset.ToString()));
            BuildToolWindowV2Ui.DrawSummaryRow("AdMob Test Device", FormatManagedValue(snapshot.AdmobTestDevice, expected.AdmobTestDevice ? "ON" : "OFF"));
            BuildToolWindowV2Ui.DrawSummaryRow("AdMob Test IDs", FormatManagedValue(snapshot.AdmobTestIds, expected.AdmobTestIds ? "ON" : "OFF"));
            BuildToolWindowV2Ui.DrawSummaryRow("Tracking Firebase", FormatManagedValue(snapshot.TrackingFirebase, expected.TrackingFirebase ? "ON" : "OFF"));
            DrawDebugUiKeepLandscapeToggle(owner);
            BuildToolWindowV2Ui.DrawSummaryRow("Adjust Environment", owner.profile.UsesSandboxAdjust ? "Sandbox" : "Production");
            BuildToolWindowV2Ui.DrawSummaryRow("Debug Overlay Canvas", owner.profile.RequiresDebugOverlayCanvas ? "Required in Scene 0" : "Must be absent from Scene 0");
            BuildToolWindowV2Ui.DrawSummaryRow("SRDebugger", FormatManagedValue(snapshot.SrDebugger, expected.SrDebuggerEnabled ? "ON" : "OFF"));
            BuildToolWindowV2Ui.DrawSummaryRow("Change Log", owner.profile.RequiresChangeLog ? "Required before build" : "Optional");
            BuildToolWindowV2Ui.DrawSummaryRow("Build App Bundle", owner.profile.BuildAppBundle ? "ON" : "OFF");
            BuildToolWindowV2Ui.DrawSummaryRow("Split Application Binary", owner.profile.BuildAppBundle ? "Manual option for AAB only" : "Forced OFF");
        }

        internal static void DrawHeaderOverviewCard(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
                return;

            BuildToolWindowV2.HeaderOverviewState snapshot = owner.headerOverviewState ?? new BuildToolWindowV2.HeaderOverviewState();

            EditorGUILayout.Space(4f);
            GUILayout.Label("Overview", EditorStyles.boldLabel);
            BuildToolWindowV2Ui.DrawSummaryRow("Preset", snapshot.Preset);
            BuildToolWindowV2Ui.DrawSummaryRow("Target", snapshot.Target);
            BuildToolWindowV2Ui.DrawSummaryRow("Build Name", snapshot.BuildName);
            BuildToolWindowV2Ui.DrawSummaryRow("Adjust Environment", snapshot.AdjustEnvironment);
            BuildToolWindowV2Ui.DrawSummaryRow(
                "Output Folder",
                snapshot.OutputDirectory,
                snapshot.HasOutputDirectory ? "Open" : null,
                snapshot.HasOutputDirectory
                    ? () => BuildToolV2Utilities.OpenFolderInExplorer(snapshot.OutputDirectory)
                    : null,
                70f,
                "Set",
                () => owner.ChooseOutputDirectory(),
                70f);

            BuildToolWindowV2Ui.DrawWhiteDivider();
            BuildToolWindowV2Ui.DrawSummaryRow("Hack", snapshot.Hack);
            BuildToolWindowV2Ui.DrawSummaryRow("AdMob Test Device", snapshot.AdmobTestDevice);
            BuildToolWindowV2Ui.DrawSummaryRow("AdMob Test IDs", snapshot.AdmobTestIds);
            BuildToolWindowV2Ui.DrawSummaryRow("Debug Preset", snapshot.DebugPreset);
            BuildToolWindowV2Ui.DrawSummaryRow("Tracking Firebase", snapshot.TrackingFirebase);
            DrawDebugUiKeepLandscapeToggle(owner);
        }

        private static string FormatManagedValue(string current, string expected)
        {
            return string.Equals(current, expected, System.StringComparison.Ordinal)
                ? current
                : $"{current} (expected {expected})";
        }

        private static void DrawDebugUiKeepLandscapeToggle(BuildToolWindowV2 owner)
        {
            NetConfigsSO netConfigs = owner.profile != null ? owner.profile.ResolveNetConfigs() : null;
            if (netConfigs == null)
            {
                BuildToolWindowV2Ui.DrawSummaryRow("UI Landscape", "(missing Configs SO)");
                return;
            }

            EditorGUI.BeginChangeCheck();
            bool nextValue = EditorGUILayout.Toggle("UI Landscape", netConfigs.DebugUI_KeepLandscape);
            if (!EditorGUI.EndChangeCheck())
                return;

            var serializedObject = new SerializedObject(netConfigs);
            if (!BuildToolV2SceneSaveAndSummary.SetBool(serializedObject, "debugUi_KeepLandscape", nextValue))
                return;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(netConfigs);
            AssetDatabase.SaveAssets();

            owner.MarkValidationScanStale();
            owner.lastExecutionResult = null;
            owner.RefreshWindowData(false, false, false);
            GUIUtility.ExitGUI();
        }
    }
}
