using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    /// <summary>
    /// Validator that surfaces Mfuscator setup problems before a build runs. Five checks:
    /// IL2CPP backend, Mfuscator folder presence, preset selection, preset drift vs the
    /// live <c>Mfuscator.Settings.Object</c>, and blacklist completeness when
    /// <c>renameExports</c> is on. Each issue carries a one-click fix where possible.
    /// </summary>
    internal sealed class MfuscatorValidator : IBuildValidator
    {
        public string Name => nameof(MfuscatorValidator);
        public string DisplayName => "Mfuscator";
        public string Description => "Kiểm tra Mfuscator: IL2CPP backend, package đã import, preset đã chọn và setting MFS Settings có khớp preset không.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            MfuscatorLocalStateV2 state = BuildToolV2MfuscatorStateStore.Get();
            bool mfuscatorEnabledForBuild = state != null && state.enableForBuild;

            // Mfuscator only affects IL2CPP builds — surface the mismatch loudly.
            if (mfuscatorEnabledForBuild && !IsActiveTargetUsingIL2CPP(context))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MFS_BACKEND_NOT_IL2CPP",
                    Severity = BuildValidationSeverity.Error,
                    Message = "Mfuscator bật nhưng Scripting Backend không phải IL2CPP — obfuscation sẽ không có tác dụng.",
                    FixLabel = "Switch to IL2CPP",
                    FixAction = () => SwitchActiveTargetToIL2CPP(context),
                });
            }

            // Find the Mfuscator package by its asmdef GUID — path-independent so this
            // works whether NetCore is mounted at Assets/BG_Lib/NetCore (submodule layout)
            // or Assets/BG/NET Core (older flat-checked-in layout).
            string mfuscatorFolderPath = BuildToolV2MfuscatorPaths.ResolveFolderPath();
            if (mfuscatorFolderPath == null)
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MFS_FOLDER_MISSING",
                    Severity = mfuscatorEnabledForBuild ? BuildValidationSeverity.Error : BuildValidationSeverity.Warning,
                    Message = "Không tìm thấy Mfuscator trong project (asmdef MfuscatorEditor không có).",
                    Details = "Import Mfuscator unitypackage, hoặc init đầy đủ NetCore submodule (Mfuscator/ folder ở root NetCore).",
                });
                return;
            }

            MfuscatorPresetV2 preset = BuildToolV2MfuscatorStateStore.ResolveSelectedPreset(state);

            if (mfuscatorEnabledForBuild && preset == null)
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MFS_PRESET_NOT_SELECTED",
                    Severity = BuildValidationSeverity.Warning,
                    Message = "Mfuscator bật nhưng chưa chọn preset trong BuildTool window.",
                    Details = $"Chạy /audit-mfuscator-preset cho project hiện tại để sinh preset, sau đó chọn trong section Mfuscator.",
                });
                return;
            }

            if (preset == null)
                return;

            // Blacklist must be non-empty when the high-risk rename flag is on, otherwise
            // every reflection-based SDK call breaks at runtime.
            if (preset.HasRenameExportsWithoutBlacklist())
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MFS_RENAME_EXPORTS_NO_BLACKLIST",
                    Severity = BuildValidationSeverity.Warning,
                    Message = $"Preset {preset.GetDisplayLabel()} bật renameExports nhưng blacklist rỗng — reflection-based code sẽ vỡ.",
                    FixLabel = "Ping Preset",
                    FixAction = () => EditorGUIUtility.PingObject(preset),
                });
            }

            // Drift check — read live MFS Settings via reflection and diff vs preset.
            if (!BuildToolV2MfuscatorBridge.TryReadCurrent(out MfuscatorSettingsSnapshot live))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MFS_BRIDGE_UNAVAILABLE",
                    Severity = BuildValidationSeverity.Info,
                    Message = "Không đọc được Mfuscator.Settings.Object (Mfuscator có thể đổi internal layout).",
                    Details = "Mở Window/MFS Settings kiểm tra thủ công.",
                    FixLabel = "Open MFS Settings",
                    FixAction = OpenMfsSettings,
                });
                return;
            }

            string driftDetails = DiffPresetVsLive(preset, live);
            if (!string.IsNullOrEmpty(driftDetails))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MFS_PRESET_DRIFT",
                    Severity = BuildValidationSeverity.Warning,
                    Message = $"MFS Settings hiện tại lệch khỏi preset {preset.GetDisplayLabel()}.",
                    Details = driftDetails,
                    FixLabel = "Re-apply Preset",
                    FixAction = () => BuildToolV2MfuscatorBridge.TryApply(preset),
                });
            }
        }

        private static bool IsActiveTargetUsingIL2CPP(BuildValidationContext context)
        {
            BuildTarget target = context.Profile != null ? context.Profile.BuildTarget : EditorUserBuildSettings.activeBuildTarget;
            NamedBuildTarget namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
            return PlayerSettings.GetScriptingBackend(namedTarget) == ScriptingImplementation.IL2CPP;
        }

        private static void SwitchActiveTargetToIL2CPP(BuildValidationContext context)
        {
            BuildTarget target = context.Profile != null ? context.Profile.BuildTarget : EditorUserBuildSettings.activeBuildTarget;
            NamedBuildTarget namedTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
            PlayerSettings.SetScriptingBackend(namedTarget, ScriptingImplementation.IL2CPP);
        }

        private static void OpenMfsSettings()
        {
            EditorApplication.ExecuteMenuItem("Window/MFS Settings");
        }

        private static string DiffPresetVsLive(MfuscatorPresetV2 preset, MfuscatorSettingsSnapshot live)
        {
            var diffs = new List<string>();
            AppendBoolDiff(diffs, "enable", preset.enable, live.enable);
            AppendIntDiff(diffs, "callbackOrder", preset.callbackOrder, live.callbackOrder);
            AppendBoolDiff(diffs, "logInfo", preset.logInfo, live.logInfo);
            AppendBoolDiff(diffs, "removeStringLiterals", preset.removeStringLiterals, live.removeStringLiterals);
            AppendBoolDiff(diffs, "preserveUnityCrashHandler", preset.preserveUnityCrashHandler, live.preserveUnityCrashHandler);
            AppendBoolDiff(diffs, "checkFunctionCalls", preset.checkFunctionCalls, live.checkFunctionCalls);
            AppendBoolDiff(diffs, "renameExports", preset.renameExports, live.renameExports);
            AppendStringDiff(diffs, "renameExportsBlacklist", preset.renameExportsBlacklist, live.renameExportsBlacklist);
            AppendBoolDiff(diffs, "removeMonoExports", preset.removeMonoExports, live.removeMonoExports);
            AppendBoolDiff(diffs, "modifyInternalStructures", preset.modifyInternalStructures, live.modifyInternalStructures);
            AppendBoolDiff(diffs, "detectProxyLibraries", preset.detectProxyLibraries, live.detectProxyLibraries);
            AppendStringDiff(diffs, "detectProxyLibrariesWhitelist", preset.detectProxyLibrariesWhitelist, live.detectProxyLibrariesWhitelist);
            return diffs.Count == 0 ? string.Empty : string.Join("\n", diffs);
        }

        private static void AppendBoolDiff(List<string> diffs, string name, bool expected, bool actual)
        {
            if (expected != actual)
                diffs.Add($"× {name}: expected {(expected ? "ON" : "OFF")}, actual {(actual ? "ON" : "OFF")}");
        }

        private static void AppendIntDiff(List<string> diffs, string name, int expected, int actual)
        {
            if (expected != actual)
                diffs.Add($"× {name}: expected {expected}, actual {actual}");
        }

        private static void AppendStringDiff(List<string> diffs, string name, string expected, string actual)
        {
            string a = expected ?? string.Empty;
            string b = actual ?? string.Empty;
            if (!string.Equals(a.Trim(), b.Trim(), System.StringComparison.Ordinal))
                diffs.Add($"× {name}: text differs");
        }
    }
}
