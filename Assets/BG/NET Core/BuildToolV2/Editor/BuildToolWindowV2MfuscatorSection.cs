using System;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    /// <summary>
    /// BuildTool window section that manages Mfuscator's per-build toggle and preset
    /// selection. UI only — preset content lives in committed
    /// <see cref="MfuscatorPresetV2"/> assets and per-machine selection in
    /// <see cref="MfuscatorLocalStateV2"/>. Reflection bridge + drift check via
    /// <see cref="BuildToolV2MfuscatorBridge"/>; the build-time MFS_IGNORE write is
    /// done by <see cref="BuildToolV2MfuscatorPreset"/> from BuildPreparation.
    /// </summary>
    internal static class BuildToolWindowV2MfuscatorSection
    {
        private const string FoldoutKey = "BG_BuildToolV2_Foldout_Mfuscator";
        private const string OpenMfsSettingsMenu = "Window/MFS Settings";

        private static MfuscatorPresetV2[] cachedPresets = Array.Empty<MfuscatorPresetV2>();
        private static string[] cachedPresetLabels = Array.Empty<string>();
        private static bool presetCacheValid;

        private static VerifyStatus lastVerifyStatus = VerifyStatus.Unknown;
        private static string lastVerifyMessage = string.Empty;

        private enum VerifyStatus
        {
            Unknown,
            Match,
            Drift,
            Unavailable,
        }

        internal static void Draw(BuildToolWindowV2 owner)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (!BuildToolWindowV2Ui.DrawFoldoutHeader("Mfuscator IL2CPP Encryption", FoldoutKey))
                    return;

                EditorGUILayout.HelpBox(
                    "Quản lý Mfuscator cho build hiện tại. Preset chi tiết (flag obfuscation) sinh ra qua /audit-mfuscator-preset cho từng project. Setting cụ thể của Mfuscator mở qua Window/MFS Settings.",
                    MessageType.None);

                EnsurePresetCache();
                MfuscatorLocalStateV2 state = BuildToolV2MfuscatorStateStore.Get();

                DrawEnableToggle(state);
                EditorGUILayout.Space(4f);
                DrawPresetSelector(state);
                EditorGUILayout.Space(4f);
                DrawActionRow(owner, state);
                EditorGUILayout.Space(4f);
                DrawStatusBlock();
            }
        }

        /// <summary>Forces a reload of the preset asset cache (call after a preset asset is added/removed).</summary>
        internal static void InvalidatePresetCache()
        {
            presetCacheValid = false;
        }

        private static void DrawEnableToggle(MfuscatorLocalStateV2 state)
        {
            bool current = state != null && state.enableForBuild;
            bool next = EditorGUILayout.ToggleLeft(
                new GUIContent("Run Mfuscator on this build", "Clears MFS_IGNORE before BuildPipeline.BuildPlayer runs. When off, sets MFS_IGNORE=\"-\" so Mfuscator skips this build. The PlayerPref is restored after the build."),
                current);
            if (next != current)
                BuildToolV2MfuscatorStateStore.SetEnableForBuild(next);
        }

        private static void DrawPresetSelector(MfuscatorLocalStateV2 state)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Preset", GUILayout.Width(60f));

                int selectedIndex = ResolveSelectedIndex(state);
                int nextIndex = EditorGUILayout.Popup(selectedIndex, cachedPresetLabels);
                if (nextIndex != selectedIndex)
                    PersistPresetSelection(nextIndex);

                if (GUILayout.Button("Refresh", GUILayout.Width(72f)))
                {
                    InvalidatePresetCache();
                    EnsurePresetCache();
                }

                MfuscatorPresetV2 selected = nextIndex >= 0 && nextIndex < cachedPresets.Length ? cachedPresets[nextIndex] : null;
                using (new EditorGUI.DisabledScope(selected == null))
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(48f)) && selected != null)
                        EditorGUIUtility.PingObject(selected);
                }
            }

            if (cachedPresets.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    $"Chưa có preset nào trong {BuildToolV2MfuscatorPaths.ResolvePresetsFolderPath() ?? "(Mfuscator/Presets/)"}. Chạy /audit-mfuscator-preset để sinh preset cho project hiện tại.",
                    MessageType.Info);
            }
        }

        private static void DrawActionRow(BuildToolWindowV2 owner, MfuscatorLocalStateV2 state)
        {
            MfuscatorPresetV2 preset = BuildToolV2MfuscatorStateStore.ResolveSelectedPreset(state);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(preset == null))
                {
                    if (GUILayout.Button(new GUIContent("Apply Preset → MFS Settings", "Sync the selected preset's flags to Mfuscator.Settings.Object via reflection."), GUILayout.Height(22f)))
                        TryApplyPreset(preset);
                }

                if (GUILayout.Button(new GUIContent("Verify", "Read MFS Settings via reflection and diff against the selected preset."), GUILayout.Width(72f), GUILayout.Height(22f)))
                    RunVerify(preset);

                if (GUILayout.Button(new GUIContent("Open MFS Settings", "Open Mfuscator's own settings window for advanced flags / blacklist editing."), GUILayout.Width(150f), GUILayout.Height(22f)))
                    EditorApplication.ExecuteMenuItem(OpenMfsSettingsMenu);
            }

            if (preset != null && preset.HasRenameExportsWithoutBlacklist())
            {
                EditorGUILayout.HelpBox(
                    $"Preset {preset.GetDisplayLabel()} bật renameExports nhưng blacklist rỗng — reflection-based SDK sẽ vỡ runtime. Mở MFS Settings nhập blacklist trước khi build.",
                    MessageType.Warning);
            }
        }

        private static void DrawStatusBlock()
        {
            switch (lastVerifyStatus)
            {
                case VerifyStatus.Unknown:
                    EditorGUILayout.LabelField("Status: chưa Verify", EditorStyles.miniLabel);
                    break;
                case VerifyStatus.Match:
                    EditorGUILayout.HelpBox(lastVerifyMessage, MessageType.Info);
                    break;
                case VerifyStatus.Drift:
                    EditorGUILayout.HelpBox(lastVerifyMessage, MessageType.Warning);
                    break;
                case VerifyStatus.Unavailable:
                    EditorGUILayout.HelpBox(lastVerifyMessage, MessageType.None);
                    break;
            }
        }

        private static void TryApplyPreset(MfuscatorPresetV2 preset)
        {
            if (preset == null)
                return;

            if (BuildToolV2MfuscatorBridge.TryApply(preset))
            {
                lastVerifyStatus = VerifyStatus.Match;
                lastVerifyMessage = $"✓ Applied preset {preset.GetDisplayLabel()} to MFS Settings.";
            }
            else
            {
                lastVerifyStatus = VerifyStatus.Unavailable;
                lastVerifyMessage = "⊘ Không apply được preset — Mfuscator.Settings.Object không đọc/ghi được qua reflection. Mở MFS Settings nhập tay.";
            }
        }

        private static void RunVerify(MfuscatorPresetV2 preset)
        {
            if (preset == null)
            {
                lastVerifyStatus = VerifyStatus.Unknown;
                lastVerifyMessage = string.Empty;
                return;
            }

            if (!BuildToolV2MfuscatorBridge.TryReadCurrent(out MfuscatorSettingsSnapshot live))
            {
                lastVerifyStatus = VerifyStatus.Unavailable;
                lastVerifyMessage = "⊘ Không đọc được MFS Settings (Mfuscator có thể đổi internal layout). Mở Window/MFS Settings kiểm tra thủ công.";
                return;
            }

            string drift = DiffPresetVsLive(preset, live);
            if (string.IsNullOrEmpty(drift))
            {
                lastVerifyStatus = VerifyStatus.Match;
                lastVerifyMessage = $"✓ Matches preset {preset.GetDisplayLabel()}.";
            }
            else
            {
                lastVerifyStatus = VerifyStatus.Drift;
                lastVerifyMessage = $"⚠ MFS Settings lệch khỏi preset {preset.GetDisplayLabel()}:\n{drift}";
            }
        }

        private static string DiffPresetVsLive(MfuscatorPresetV2 preset, MfuscatorSettingsSnapshot live)
        {
            var diffs = new System.Collections.Generic.List<string>();
            if (preset.enable != live.enable) diffs.Add($"× enable: expected {BoolLabel(preset.enable)}, actual {BoolLabel(live.enable)}");
            if (preset.callbackOrder != live.callbackOrder) diffs.Add($"× callbackOrder: expected {preset.callbackOrder}, actual {live.callbackOrder}");
            if (preset.logInfo != live.logInfo) diffs.Add($"× logInfo: expected {BoolLabel(preset.logInfo)}, actual {BoolLabel(live.logInfo)}");
            if (preset.removeStringLiterals != live.removeStringLiterals) diffs.Add($"× removeStringLiterals: expected {BoolLabel(preset.removeStringLiterals)}, actual {BoolLabel(live.removeStringLiterals)}");
            if (preset.preserveUnityCrashHandler != live.preserveUnityCrashHandler) diffs.Add($"× preserveUnityCrashHandler: expected {BoolLabel(preset.preserveUnityCrashHandler)}, actual {BoolLabel(live.preserveUnityCrashHandler)}");
            if (preset.checkFunctionCalls != live.checkFunctionCalls) diffs.Add($"× checkFunctionCalls: expected {BoolLabel(preset.checkFunctionCalls)}, actual {BoolLabel(live.checkFunctionCalls)}");
            if (preset.renameExports != live.renameExports) diffs.Add($"× renameExports: expected {BoolLabel(preset.renameExports)}, actual {BoolLabel(live.renameExports)}");
            if (!string.Equals((preset.renameExportsBlacklist ?? string.Empty).Trim(), (live.renameExportsBlacklist ?? string.Empty).Trim(), StringComparison.Ordinal)) diffs.Add("× renameExportsBlacklist: text differs");
            if (preset.removeMonoExports != live.removeMonoExports) diffs.Add($"× removeMonoExports: expected {BoolLabel(preset.removeMonoExports)}, actual {BoolLabel(live.removeMonoExports)}");
            if (preset.modifyInternalStructures != live.modifyInternalStructures) diffs.Add($"× modifyInternalStructures: expected {BoolLabel(preset.modifyInternalStructures)}, actual {BoolLabel(live.modifyInternalStructures)}");
            if (preset.detectProxyLibraries != live.detectProxyLibraries) diffs.Add($"× detectProxyLibraries: expected {BoolLabel(preset.detectProxyLibraries)}, actual {BoolLabel(live.detectProxyLibraries)}");
            if (!string.Equals((preset.detectProxyLibrariesWhitelist ?? string.Empty).Trim(), (live.detectProxyLibrariesWhitelist ?? string.Empty).Trim(), StringComparison.Ordinal)) diffs.Add("× detectProxyLibrariesWhitelist: text differs");
            return diffs.Count == 0 ? string.Empty : string.Join("\n", diffs);
        }

        private static string BoolLabel(bool value)
        {
            return value ? "ON" : "OFF";
        }

        private static int ResolveSelectedIndex(MfuscatorLocalStateV2 state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.selectedPresetGuid))
                return -1;

            for (int i = 0; i < cachedPresets.Length; i++)
            {
                MfuscatorPresetV2 candidate = cachedPresets[i];
                if (candidate == null)
                    continue;

                string assetPath = AssetDatabase.GetAssetPath(candidate);
                if (string.IsNullOrWhiteSpace(assetPath))
                    continue;

                if (string.Equals(AssetDatabase.AssetPathToGUID(assetPath), state.selectedPresetGuid, StringComparison.Ordinal))
                    return i;
            }
            return -1;
        }

        private static void PersistPresetSelection(int index)
        {
            MfuscatorPresetV2 preset = index >= 0 && index < cachedPresets.Length ? cachedPresets[index] : null;
            BuildToolV2MfuscatorStateStore.SelectPreset(preset);
            // Selection changed — invalidate the last verify result so the UI doesn't lie about a different preset.
            lastVerifyStatus = VerifyStatus.Unknown;
            lastVerifyMessage = string.Empty;
        }

        private static void EnsurePresetCache()
        {
            if (presetCacheValid)
                return;

            string[] guids = AssetDatabase.FindAssets("t:" + nameof(MfuscatorPresetV2));
            var presets = new MfuscatorPresetV2[guids.Length];
            var labels = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                MfuscatorPresetV2 preset = AssetDatabase.LoadAssetAtPath<MfuscatorPresetV2>(path);
                presets[i] = preset;
                labels[i] = preset != null ? preset.GetDisplayLabel() : "(missing)";
            }

            cachedPresets = presets;
            cachedPresetLabels = labels;
            presetCacheValid = true;
        }
    }
}
