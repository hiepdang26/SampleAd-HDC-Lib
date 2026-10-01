using System.IO;
using UnityEditor;

namespace BG_Library.BuildToolV2
{
    /// <summary>
    /// Resolves Mfuscator package paths at runtime via AssetDatabase, so the validator
    /// and section work regardless of where NetCore is mounted. Two known mount layouts:
    /// <c>Assets/BG_Lib/NetCore/Mfuscator</c> (submodule layout used by
    /// BG-Library-Full-Module) and <c>Assets/BG/NET Core/Mfuscator</c> (older
    /// flat-checked-in layout used by older game projects).
    /// </summary>
    internal static class BuildToolV2MfuscatorPaths
    {
        private const string AsmdefFilter = "MfuscatorEditor t:asmdef";

        /// <summary>Path of the Mfuscator vendor folder, or null when not present in the project.</summary>
        public static string ResolveFolderPath()
        {
            string asmdefPath = ResolveAsmdefPath();
            if (string.IsNullOrEmpty(asmdefPath))
                return null;

            // <Mfuscator>/Scripts/MfuscatorEditor.asmdef -> walk up two folders to get <Mfuscator>.
            string scriptsFolder = Path.GetDirectoryName(asmdefPath);
            string mfuscatorFolder = string.IsNullOrEmpty(scriptsFolder) ? null : Path.GetDirectoryName(scriptsFolder);
            return mfuscatorFolder?.Replace('\\', '/');
        }

        /// <summary>Path of the Presets/ subfolder under the Mfuscator vendor folder, or null when not present.</summary>
        public static string ResolvePresetsFolderPath()
        {
            string mfuscatorFolder = ResolveFolderPath();
            return mfuscatorFolder == null ? null : $"{mfuscatorFolder}/Presets";
        }

        private static string ResolveAsmdefPath()
        {
            string[] guids = AssetDatabase.FindAssets(AsmdefFilter);
            if (guids == null || guids.Length == 0)
                return null;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!string.IsNullOrEmpty(path))
                    return path;
            }
            return null;
        }
    }
}
