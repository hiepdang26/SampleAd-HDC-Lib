using System;
using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2SummarySection
    {
        private sealed class SummaryEditState
        {
            public bool DevelopmentBuildChanged;
            public string CurrentProductName;
            public string NextProductName;
            public string CurrentCompanyName;
            public string NextCompanyName;
            public bool VersionValueChanged;
            public bool VersionCodeChanged;
            public bool KeystorePasswordChanged;
            public bool PackageNameChanged;
            public bool SplitApplicationBinaryChanged;
        }

        public static void Draw(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
                return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (!BuildToolWindowV2Ui.DrawFoldoutHeader("Summary", "BG_BuildToolV2_Foldout_Summary"))
                    return;

                BuildToolWindowV2.HeaderOverviewState overviewState = owner.headerOverviewState ?? new BuildToolWindowV2.HeaderOverviewState
                {
                    Preset = owner.profile.Preset.ToString(),
                    Target = BuildToolV2Utilities.GetTargetDisplayName(owner.profile.BuildTarget),
                    BuildName = BuildToolV2Utilities.GetBuildDisplayName(owner.profile, null),
                };
                NetConfigsSO netConfigs = overviewState.NetConfigs;
                var editState = new SummaryEditState
                {
                    CurrentProductName = owner.summaryDraft.ProductName,
                    NextProductName = owner.summaryDraft.ProductName,
                    CurrentCompanyName = owner.summaryDraft.CompanyName,
                    NextCompanyName = owner.summaryDraft.CompanyName,
                };

                bool localStateChanged = false;

                EditorGUILayout.HelpBox(
                    $"{owner.profile.DisplayName} | {overviewState.Target} | {overviewState.BuildName}",
                    MessageType.None);

                localStateChanged |= DrawSummaryAppInformationCard(owner, editState);
                BuildToolWindowV2Ui.DrawWhiteDivider();

                DrawSummaryConfigsCard(owner, editState);
                BuildToolWindowV2Ui.DrawWhiteDivider();

                bool productNameChanged = !string.Equals(editState.CurrentProductName, editState.NextProductName, StringComparison.Ordinal);
                bool companyNameChanged = !string.Equals(editState.CurrentCompanyName, editState.NextCompanyName, StringComparison.Ordinal);
                if (editState.DevelopmentBuildChanged || productNameChanged || companyNameChanged || editState.VersionValueChanged || editState.VersionCodeChanged || editState.KeystorePasswordChanged || editState.PackageNameChanged || editState.SplitApplicationBinaryChanged || localStateChanged)
                {
                    var applyResult = BuildToolV2SummaryLogic.ApplyChanges(new BuildToolSummaryApplyRequestV2
                    {
                        Profile = owner.profile,
                        LocalStateChanged = localStateChanged,
                        DevelopmentBuildChanged = editState.DevelopmentBuildChanged,
                        DevelopmentBuild = owner.summaryDraft.DevelopmentBuild,
                        ProductNameChanged = productNameChanged,
                        CompanyNameChanged = companyNameChanged,
                        VersionValueChanged = editState.VersionValueChanged,
                        VersionCodeChanged = editState.VersionCodeChanged,
                        KeystorePasswordChanged = editState.KeystorePasswordChanged,
                        PackageNameChanged = editState.PackageNameChanged,
                        SplitApplicationBinaryChanged = editState.SplitApplicationBinaryChanged,
                        ProductName = editState.NextProductName,
                        CompanyName = editState.NextCompanyName,
                        Version = owner.summaryDraft.Version,
                        VersionCode = owner.summaryDraft.VersionCode,
                        KeystorePassword = owner.summaryDraft.KeystorePassword,
                        PackageName = owner.summaryDraft.PackageName,
                        SplitApplicationBinary = owner.summaryDraft.SplitApplicationBinary,
                    });

                    if (applyResult.RefreshedDisplaySnapshots)
                        owner.ReloadDisplayState(false);
                    if (applyResult.ResetBuildResults)
                    {
                        owner.MarkValidationScanStale();
                        owner.lastExecutionResult = null;
                    }

                    owner.RefreshSummaryDraft(true);
                    BuildToolWindowV2IconSupport.RefreshIconSelectionFromCurrent(owner, true);
                }
            }
        }

        private static bool DrawSummaryAppInformationCard(BuildToolWindowV2 owner, SummaryEditState editState)
        {
            bool localStateChanged = false;
            EditorGUILayout.LabelField("Product Settings", EditorStyles.boldLabel);
            editState.NextProductName = EditorGUILayout.DelayedTextField("Product Name", editState.CurrentProductName);
            editState.NextCompanyName = EditorGUILayout.DelayedTextField("Company Name", editState.CurrentCompanyName);

            string nextVersion = EditorGUILayout.DelayedTextField("Version", owner.summaryDraft.Version);
            if (!string.Equals(nextVersion, owner.summaryDraft.Version, StringComparison.Ordinal))
            {
                owner.summaryDraft.Version = nextVersion;
                editState.VersionValueChanged = true;
            }

            int nextVersionCode = EditorGUILayout.DelayedIntField("Version Code", owner.summaryDraft.VersionCode);
            if (nextVersionCode != owner.summaryDraft.VersionCode)
            {
                owner.summaryDraft.VersionCode = nextVersionCode;
                editState.VersionCodeChanged = true;
            }

            string nextKeystorePassword = EditorGUILayout.DelayedTextField("Keystore Password", owner.summaryDraft.KeystorePassword);
            if (!string.Equals(nextKeystorePassword, owner.summaryDraft.KeystorePassword, StringComparison.Ordinal))
            {
                owner.summaryDraft.KeystorePassword = nextKeystorePassword;
                editState.KeystorePasswordChanged = true;
            }

            EditorGUILayout.Space(4f);
            localStateChanged |= DrawSummaryChangeLogEditor(owner);

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope("box"))
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(280f)))
                {
                    EditorGUILayout.LabelField("Icon Previews", EditorStyles.boldLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope(GUILayout.Width(80f)))
                        {
                            BuildToolWindowV2IconSupport.DrawLabeledIconPreview("Default", owner.selectedSourceIcon, 72f);
                            if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(56f)))
                                BuildToolWindowV2IconSupport.ShowDefaultIconObjectPicker(owner);
                        }
                        BuildToolWindowV2IconSupport.DrawLabeledIconPreview("Safe Area", owner.adaptiveInsetPreview, 72f);
                        BuildToolWindowV2IconSupport.DrawLabeledIconPreview("Applied", owner.adaptiveAppliedPreview, 72f);
                    }
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField("Adaptive Status", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(
                        string.IsNullOrWhiteSpace(owner.selectedSourceIconName) ? "No icon found in Player Settings." : owner.selectedSourceIconName,
                        EditorStyles.wordWrappedMiniLabel);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        using (new EditorGUI.DisabledScope(owner.selectedSourceIcon == null))
                        {
                            if (GUILayout.Button("Generate & Apply Adaptive", GUILayout.Width(180f)))
                                BuildToolWindowV2IconSupport.ApplyAdaptiveIconSelection(owner);
                        }
                    }

                    EditorGUILayout.Space(4f);
                    EditorGUILayout.HelpBox(
                        owner.adaptiveStatusMessage,
                        owner.selectedSourceIcon == null ? MessageType.Info : owner.adaptiveStatusApplied ? MessageType.Info : MessageType.Warning);
                }
            }

            EditorGUILayout.LabelField(
                "Use Select to change the default icon in Player Settings. This panel only checks adaptive icon status and applies adaptive assets when needed.",
                EditorStyles.wordWrappedMiniLabel);

            return localStateChanged;
        }

        private static bool DrawSummaryChangeLogEditor(BuildToolWindowV2 owner)
        {
            bool localStateChanged = false;

            bool showChangeLog = BuildToolWindowV2Ui.DrawFoldoutHeader("Change Log", "BG_BuildToolV2_Foldout_ChangeLog");
            if (!showChangeLog)
                return false;

            if (!owner.profile.RequiresChangeLog)
            {
                EditorGUILayout.HelpBox(
                    "This preset does not require a change log, but it is still useful for tracking builds later.",
                    MessageType.None);
            }

            float height = Mathf.Clamp(owner.position.height * 0.098f, 54f, 100f);
            using (var scrollView = new EditorGUILayout.ScrollViewScope(owner.changeLogScrollPosition, "box", GUILayout.Height(height)))
            {
                owner.changeLogScrollPosition = scrollView.scrollPosition;
                string nextChangeLog = EditorGUILayout.TextArea(owner.summaryDraft.ChangeLog, GUILayout.ExpandHeight(true));
                if (!string.Equals(nextChangeLog, owner.summaryDraft.ChangeLog, StringComparison.Ordinal))
                {
                    owner.summaryDraft.ChangeLog = nextChangeLog;
                    owner.profile.SetChangeLog(nextChangeLog);
                    localStateChanged = true;
                }
            }

            return localStateChanged;
        }

        private static void DrawSummaryConfigsCard(BuildToolWindowV2 owner, SummaryEditState editState)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Configs", EditorStyles.boldLabel);
            bool nextDevelopmentBuild = EditorGUILayout.Toggle("Developer Build", owner.summaryDraft.DevelopmentBuild);
            if (nextDevelopmentBuild != owner.summaryDraft.DevelopmentBuild)
            {
                owner.summaryDraft.DevelopmentBuild = nextDevelopmentBuild;
                editState.DevelopmentBuildChanged = true;
            }

            string nextPackageName = EditorGUILayout.DelayedTextField("Package Name", owner.summaryDraft.PackageName);
            if (!string.Equals(nextPackageName, owner.summaryDraft.PackageName, StringComparison.Ordinal))
            {
                owner.summaryDraft.PackageName = nextPackageName ?? string.Empty;
                editState.PackageNameChanged = true;
            }

            bool isAndroidTarget = owner.profile.BuildTarget == BuildTarget.Android;
            if (isAndroidTarget && owner.profile.BuildAppBundle)
            {
                bool nextSplitBinary = EditorGUILayout.Toggle("Split Application Binary", owner.summaryDraft.SplitApplicationBinary);
                if (nextSplitBinary != owner.summaryDraft.SplitApplicationBinary)
                {
                    owner.summaryDraft.SplitApplicationBinary = nextSplitBinary;
                    editState.SplitApplicationBinaryChanged = true;
                }
            }
            else if (isAndroidTarget)
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.Toggle("Split Application Binary", false);

                if (owner.summaryDraft.SplitApplicationBinary)
                {
                    owner.summaryDraft.SplitApplicationBinary = false;
                    editState.SplitApplicationBinaryChanged = true;
                }
            }

            EditorGUILayout.HelpBox(
                "PlayerSettings va Configs SO se duoc doc truc tiep tu nguon that. Chi cac gia tri local-only nhu output folder, change log, scrcpy va Debug cua release preset moi duoc luu local tren may build.",
                MessageType.None);
        }
    }
}
