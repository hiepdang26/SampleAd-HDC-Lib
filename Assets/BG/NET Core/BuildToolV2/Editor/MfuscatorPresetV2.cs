using UnityEngine;

namespace BG_Library.BuildToolV2
{
    /// <summary>
    /// Project-specific Mfuscator configuration produced by <c>/audit-mfuscator-preset</c>.
    /// One asset per game lives in <c>Assets/BG_Lib/NetCore/Mfuscator/Presets/</c>; the
    /// <see cref="BuildToolWindowV2MfuscatorSection"/> lists them, syncs the selected
    /// preset to <c>Mfuscator.Settings.Object</c> via <see cref="BuildToolV2MfuscatorBridge"/>,
    /// and verifies drift on demand.
    /// </summary>
    [CreateAssetMenu(menuName = "BG_Library/Mfuscator Preset V2", fileName = "MfuscatorPreset", order = 411)]
    public sealed class MfuscatorPresetV2 : ScriptableObject
    {
        [Tooltip("Display name shown in the BuildTool window dropdown (does not have to match the asset filename).")]
        public string presetName = string.Empty;

        [Tooltip("Free-form note about which game this preset belongs to and what audit produced it.")]
        [TextArea(2, 4)]
        public string notes = string.Empty;

        [Header("Master")]

        [Tooltip("Mfuscator's master enable flag — turn obfuscation on at build time.")]
        public bool enable = true;

        [Tooltip("Order in which Mfuscator runs vs. other build callbacks. Larger = later. Keep higher than BuildToolV2's own processors.")]
        public int callbackOrder = 1000;

        [Tooltip("Log Mfuscator step details to the Unity console during build.")]
        public bool logInfo = false;

        [Header("Per-build flags (low risk → high risk top-down)")]

        [Tooltip("Encrypt string literals. Low risk for most projects — does not affect reflection-based code.")]
        public bool removeStringLiterals = true;

        [Tooltip("RECOMMENDED ON. Keeps Unity's crash handler intact so Crashlytics / Cloud Diagnostics still report.")]
        public bool preserveUnityCrashHandler = true;

        [Tooltip("Remove mono_* IL2CPP development exports. Safe in shipping builds.")]
        public bool removeMonoExports = true;

        [Tooltip("Mutate IL2CPP runtime metadata structures. Core anti-dumper. Moderate risk against Unity version changes.")]
        public bool modifyInternalStructures = true;

        [Tooltip("Runtime integrity check on function call sites. Adds perf cost; can clash with profilers / hot-reload tools.")]
        public bool checkFunctionCalls = false;

        [Tooltip("HIGH RISK. Renames native exports — breaks any reflection or P/Invoke that resolves by name. Provide a blacklist below before enabling.")]
        public bool renameExports = false;

        [Tooltip("One pattern per line. Mfuscator preserves matching export names. Required when renameExports is on.")]
        [TextArea(4, 12)]
        public string renameExportsBlacklist = string.Empty;

        [Tooltip("HIGH RISK. Detects proxy/injected .so/.dll at runtime. Whitelist must cover every legitimate native plugin (AdMob/MAX/Adjust adapters, etc.) or the game crashes.")]
        public bool detectProxyLibraries = false;

        [Tooltip("One pattern per line. Whitelist of allowed native libraries when detectProxyLibraries is on.")]
        [TextArea(4, 12)]
        public string detectProxyLibrariesWhitelist = string.Empty;

        /// <summary>Returns the display label used in dropdowns — falls back to the asset name.</summary>
        public string GetDisplayLabel()
        {
            return string.IsNullOrWhiteSpace(presetName) ? name : presetName;
        }

        /// <summary>True when the high-risk renameExports flag is on but no blacklist is provided.</summary>
        public bool HasRenameExportsWithoutBlacklist()
        {
            return renameExports && string.IsNullOrWhiteSpace(renameExportsBlacklist);
        }
    }
}
