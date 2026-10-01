using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    /// <summary>
    /// Bridges <see cref="MfuscatorLocalStateV2.enableForBuild"/> into Mfuscator's
    /// <c>MFS_IGNORE</c> PlayerPrefs key for the duration of a build. Mirrors the
    /// <c>BuildToolV2NetConfigPreset</c> / <c>BuildToolV2SrDebuggerPreset</c> /
    /// <c>BuildToolV2AdjustPreset</c> Apply pattern called from
    /// <see cref="BuildToolV2BuildPreparation"/>. The PlayerPrefs mutation is captured
    /// and restored by <see cref="BuildToolV2SharedStateSnapshot"/>.
    /// </summary>
    internal static class BuildToolV2MfuscatorPreset
    {
        internal const string MfsIgnoreKey = "MFS_IGNORE";
        internal const string MfsIgnoreOffValue = "-";

        public static void Apply(BuildProfileV2 profile, List<string> steps)
        {
            MfuscatorLocalStateV2 state = BuildToolV2MfuscatorStateStore.Get();
            bool enable = state != null && state.enableForBuild;

            if (enable)
            {
                PlayerPrefs.DeleteKey(MfsIgnoreKey);
                steps.Add("Mfuscator: cleared MFS_IGNORE — Mfuscator will run on this build.");
            }
            else
            {
                PlayerPrefs.SetString(MfsIgnoreKey, MfsIgnoreOffValue);
                steps.Add("Mfuscator: set MFS_IGNORE=\"-\" — Mfuscator will skip this build.");
            }

            PlayerPrefs.Save();
        }
    }
}
