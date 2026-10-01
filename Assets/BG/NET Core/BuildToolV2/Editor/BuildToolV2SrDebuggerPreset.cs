using System;
using System.Collections.Generic;
using SRDebugger;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2SrDebuggerPreset
    {
        private const string DisableSrDebuggerDefine = "DISABLE_SRDEBUGGER";

        public static bool ApplyNow(BuildProfileV2 profile)
        {
            var steps = new List<string>();
            var dirtyAssetPaths = new HashSet<string>();
            bool changed = Apply(profile, steps, dirtyAssetPaths);
            if (dirtyAssetPaths.Count == 0)
                return changed;

            Settings settings = Resources.Load<Settings>("SRDebugger/Settings");
            if (settings != null)
                AssetDatabase.SaveAssetIfDirty(settings);

            return true;
        }

        public static bool Apply(
            BuildProfileV2 profile,
            List<string> steps,
            HashSet<string> dirtyAssetPaths)
        {
            bool changed = SyncCompileDefine(profile, steps);

            Settings settings = Resources.Load<Settings>("SRDebugger/Settings");
            if (settings == null)
            {
                steps.Add("Skipped SRDebugger settings because Settings.asset was not found.");
                return changed;
            }

            bool shouldEnable = profile != null && profile.ShouldEnableSRDebugger;
            if (settings.IsEnabled == shouldEnable)
            {
                steps.Add($"SRDebugger settings already matched {(shouldEnable ? "enabled" : "disabled")} state.");
                return changed;
            }

            settings.IsEnabled = shouldEnable;
            EditorUtility.SetDirty(settings);

            string assetPath = AssetDatabase.GetAssetPath(settings);
            if (!string.IsNullOrWhiteSpace(assetPath))
                dirtyAssetPaths.Add(assetPath);

            steps.Add($"Set SRDebugger Settings.IsEnabled = {(shouldEnable ? "ON" : "OFF")}.");
            return true;
        }

        private static bool SyncCompileDefine(BuildProfileV2 profile, List<string> steps)
        {
            if (profile == null)
                return false;

            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(profile.BuildTarget);
            if (targetGroup == BuildTargetGroup.Unknown)
                return false;

            bool shouldDisableCompile = !profile.ShouldEnableSRDebugger;
            string defineString = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup) ?? string.Empty;
            string[] rawDefines = defineString.Split(';');
            var defines = new List<string>(rawDefines.Length + 1);
            bool hasDisableDefine = false;

            for (int i = 0; i < rawDefines.Length; i++)
            {
                string define = rawDefines[i].Trim();
                if (string.IsNullOrEmpty(define))
                    continue;

                if (string.Equals(define, DisableSrDebuggerDefine, StringComparison.Ordinal))
                {
                    hasDisableDefine = true;
                    if (!shouldDisableCompile)
                        continue;
                }

                if (!defines.Contains(define))
                    defines.Add(define);
            }

            if (shouldDisableCompile && !hasDisableDefine)
                defines.Add(DisableSrDebuggerDefine);

            bool changed = hasDisableDefine == profile.ShouldEnableSRDebugger || (!hasDisableDefine && shouldDisableCompile);
            if (!changed)
            {
                steps.Add($"SRDebugger compile define already matched {(profile.ShouldEnableSRDebugger ? "enabled" : "disabled")} state.");
                return false;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defines.ToArray()));
            steps.Add($"Set SRDebugger compile define for {targetGroup} = {(profile.ShouldEnableSRDebugger ? "enabled" : "disabled")}.");
            return true;
        }
    }
}
