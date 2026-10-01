using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    /// <summary>
    /// Per-machine, per-project Mfuscator state — which preset is active and whether
    /// Mfuscator should run on the next build. Persisted to
    /// <c>Library/BG_Lib/BuildToolV2/LocalState/mfuscator-state.json</c>; not committed
    /// (same layer as <c>scrcpy-settings.json</c>). Preset content itself lives in
    /// committed <see cref="MfuscatorPresetV2"/> assets under <c>NetCore/Mfuscator/Presets/</c>.
    /// </summary>
    [Serializable]
    internal sealed class MfuscatorLocalStateV2
    {
        /// <summary>GUID of the selected <see cref="MfuscatorPresetV2"/> asset, or empty when none picked.</summary>
        public string selectedPresetGuid = string.Empty;

        /// <summary>Whether Mfuscator should run on the next build — bridges to <c>MFS_IGNORE</c> PlayerPrefs.</summary>
        public bool enableForBuild = false;
    }

    internal static class BuildToolV2MfuscatorStateStore
    {
        private const string StateFileName = "mfuscator-state.json";

        private static MfuscatorLocalStateV2 cachedState;

        public static MfuscatorLocalStateV2 Get(bool forceReload = false)
        {
            if (forceReload)
                cachedState = null;

            if (cachedState != null)
                return cachedState;

            cachedState = Load(GetStatePath());
            return cachedState;
        }

        public static void Save(MfuscatorLocalStateV2 state)
        {
            if (state == null)
                return;

            string directory = GetStatesDirectory();
            Directory.CreateDirectory(directory);

            string path = GetStatePath();
            string json = JsonUtility.ToJson(state, true);
            File.WriteAllText(path, json);
            cachedState = state;
        }

        public static void ClearCache()
        {
            cachedState = null;
        }

        /// <summary>Resolves the selected preset by GUID. Returns null when nothing is selected or the asset was deleted.</summary>
        public static MfuscatorPresetV2 ResolveSelectedPreset(MfuscatorLocalStateV2 state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.selectedPresetGuid))
                return null;

            string assetPath = AssetDatabase.GUIDToAssetPath(state.selectedPresetGuid);
            if (string.IsNullOrWhiteSpace(assetPath))
                return null;

            return AssetDatabase.LoadAssetAtPath<MfuscatorPresetV2>(assetPath);
        }

        /// <summary>Persists the new preset selection by GUID and returns the loaded state.</summary>
        public static MfuscatorLocalStateV2 SelectPreset(MfuscatorPresetV2 preset)
        {
            MfuscatorLocalStateV2 state = Get();
            string assetPath = preset == null ? string.Empty : AssetDatabase.GetAssetPath(preset);
            state.selectedPresetGuid = string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : AssetDatabase.AssetPathToGUID(assetPath);
            Save(state);
            return state;
        }

        public static MfuscatorLocalStateV2 SetEnableForBuild(bool enabled)
        {
            MfuscatorLocalStateV2 state = Get();
            state.enableForBuild = enabled;
            Save(state);
            return state;
        }

        private static MfuscatorLocalStateV2 Load(string path)
        {
            if (!File.Exists(path))
                return new MfuscatorLocalStateV2();

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<MfuscatorLocalStateV2>(json) ?? new MfuscatorLocalStateV2();
            }
            catch
            {
                return new MfuscatorLocalStateV2();
            }
        }

        private static string GetStatePath()
        {
            return Path.Combine(GetStatesDirectory(), StateFileName);
        }

        private static string GetStatesDirectory()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            return Path.Combine(projectRoot, "Library", "BG_Lib", "BuildToolV2", "LocalState");
        }
    }
}
