using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2ProfileSelection
    {
        internal static void DrawProfilePicker(BuildToolWindowV2 owner)
        {
            if (owner.availableProfiles == null || owner.availableProfiles.Length == 0)
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel("Build Preset");
                DrawPresetSelectionButtons(owner);

                using (new EditorGUI.DisabledScope(owner.profile == null || !owner.profile.HasManagedSourceMismatches()))
                {
                    if (GUILayout.Button("Reset", GUILayout.Width(72f)))
                    {
                        owner.profile.ResetManagedOverridesToPreset();
                        BuildToolV2Utilities.ApplyPresetManagedSourcesImmediately(owner.profile);
                        BuildToolV2Utilities.ClearEditorCaches();
                        owner.MarkValidationScanStale();
                        owner.lastExecutionResult = null;
                        owner.RefreshWindowData(false, false, false);
                        owner.Repaint();
                    }
                }
            }
        }

        internal static void RefreshProfileCache(BuildToolWindowV2 owner)
        {
            owner.availableProfiles = BuildToolV2Utilities.LoadProfiles();
        }

        internal static void EnsureDefaultProfiles(BuildToolWindowV2 owner)
        {
            owner.availableProfiles = BuildToolV2Utilities.EnsureDefaultProfiles();
        }

        internal static void PersistSelectedProfile(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
            {
                SessionState.EraseString(BuildToolWindowV2.SessionProfileGuidKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(owner.profile);
            string guid = AssetDatabase.AssetPathToGUID(path);
            SessionState.SetString(BuildToolWindowV2.SessionProfileGuidKey, guid ?? string.Empty);
        }

        internal static void RestoreSelectedProfile(BuildToolWindowV2 owner)
        {
            string guid = SessionState.GetString(BuildToolWindowV2.SessionProfileGuidKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                owner.profile = AssetDatabase.LoadAssetAtPath<BuildProfileV2>(path);
            }

            if (owner.profile == null && owner.availableProfiles.Length > 0)
                owner.profile = owner.availableProfiles[0];

            owner.RefreshSummaryDraft(true);
        }

        private static void DrawPresetSelectionButtons(BuildToolWindowV2 owner)
        {
            DrawPresetSelectionButton(owner, BuildProfilePresetV2.TestDebug, "Test Debug");
            DrawPresetSelectionButton(owner, BuildProfilePresetV2.TestRelease, "Test Release");
            DrawPresetSelectionButton(owner, BuildProfilePresetV2.Release, "Release");
        }

        private static void DrawPresetSelectionButton(BuildToolWindowV2 owner, BuildProfilePresetV2 preset, string label)
        {
            BuildProfileV2 presetProfile = FindProfileByPreset(owner, preset);
            bool isSelected = presetProfile != null && presetProfile == owner.profile;

            Color previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = isSelected
                ? new Color(0.28f, 0.56f, 0.92f, 1f)
                : new Color(0.32f, 0.32f, 0.32f, 1f);

            using (new EditorGUI.DisabledScope(presetProfile == null))
            {
                if (GUILayout.Button(label, GUILayout.Width(120f)))
                    SelectBuildProfile(owner, presetProfile);
            }

            GUI.backgroundColor = previousBackground;
        }

        private static BuildProfileV2 FindProfileByPreset(BuildToolWindowV2 owner, BuildProfilePresetV2 preset)
        {
            if (owner.availableProfiles == null)
                return null;

            for (int i = 0; i < owner.availableProfiles.Length; i++)
            {
                BuildProfileV2 current = owner.availableProfiles[i];
                if (current != null && current.Preset == preset)
                    return current;
            }

            return null;
        }

        private static void SelectBuildProfile(BuildToolWindowV2 owner, BuildProfileV2 nextProfile)
        {
            if (nextProfile == null || nextProfile == owner.profile)
                return;

            owner.profile = nextProfile;
            PersistSelectedProfile(owner);
            BuildToolV2Utilities.ClearEditorCaches();
            owner.lastExecutionResult = null;
            owner.RefreshWindowData(false, false, true);
        }
    }
}
