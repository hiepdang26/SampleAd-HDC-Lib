using System.Collections.Generic;
using BG_Library.Common;
using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEngine;

namespace BG_Library.NET.AdCore
{
    public static class NetConfigsSOEditorStorage
    {
        private static readonly List<AdCoreBase> cachedAdCores = new List<AdCoreBase>();
        private static string[] cachedAdCoreNames = System.Array.Empty<string>();
        private static bool adCoreCacheDirty = true;

        static NetConfigsSOEditorStorage()
        {
            EditorApplication.projectChanged += MarkAdCoreCacheDirty;
        }

        public static string GetAdsConfigStorageDescription()
        {
            return $"NetConfigsSO/adsConfigs{GetPlatformLabel()} (Resources: Configs SO)";
        }

        public static string LoadAdsConfigJson()
        {
            var so = NetConfigsSO.Ins;
            return so != null ? so.EditorGetAdsConfigsDefault(IsAndroidTarget()) : "";
        }

        public static void SaveAdsConfigJson(string json)
        {
            var so = RequireNetConfigsSO();
            so.EditorSetAdsConfigsDefault(IsAndroidTarget(), json);

            var configs = JsonTool.DeserializeObject<AdSystemConfigs>(json);
            if (configs == null || string.IsNullOrEmpty(configs.SelectedAdCoreName))
                return;

            var adCore = FindAvailableAdCore(configs.SelectedAdCoreName);
            if (adCore != null)
                so.EditorEnsureAdCoreExists(adCore);
        }

        public static string GetAdCoreStorageDescription(string adCoreName, bool useAndroid)
        {
            string platform = useAndroid ? "Android" : "IOS";
            return $"NetConfigsSO/adCores[{adCoreName}].defaultConfigs{platform} (Resources: Configs SO)";
        }

        public static string LoadAdCoreConfigJson(string adCoreName, bool useAndroid)
        {
            var so = NetConfigsSO.Ins;
            return so != null ? so.EditorGetAdCoreDefault(adCoreName, useAndroid) : "";
        }

        public static void SaveAdCoreConfigJson(string adCoreName, bool useAndroid, string json)
        {
            var so = RequireNetConfigsSO();
            if (!so.EditorSetAdCoreDefault(adCoreName, useAndroid, json))
                throw new UnityException($"Cannot find ad core '{adCoreName}' inside NetConfigsSO.");
        }

        public static string[] GetAvailableAdCoreNames()
        {
            EnsureAdCoreCache();
            return cachedAdCoreNames;
        }

        public static AdCoreBase FindAvailableAdCore(string adCoreName)
        {
            if (string.IsNullOrEmpty(adCoreName))
                return null;

            EnsureAdCoreCache();
            for (int i = 0; i < cachedAdCores.Count; i++)
            {
                var core = cachedAdCores[i];
                if (core == null) continue;
                if (core.AdCoreName == adCoreName)
                    return core;
            }

            return null;
        }

        private static bool IsAndroidTarget()
        {
            return EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
        }

        private static string GetPlatformLabel()
        {
            return IsAndroidTarget() ? "Android" : "IOS";
        }

        private static NetConfigsSO RequireNetConfigsSO()
        {
            var so = NetConfigsSO.Ins;
            if (so != null) return so;

            throw new UnityException("Cannot load NetConfigsSO from Resources/Configs SO.");
        }

        private static void MarkAdCoreCacheDirty()
        {
            adCoreCacheDirty = true;
        }

        private static void EnsureAdCoreCache()
        {
            if (!adCoreCacheDirty)
                return;

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            cachedAdCores.Clear();
            var seen = new HashSet<string>();

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path)) continue;

                var core = AssetDatabase.LoadAssetAtPath<AdCoreBase>(path);
                if (core == null) continue;
                if (string.IsNullOrEmpty(core.AdCoreName)) continue;
                if (!seen.Add(core.AdCoreName)) continue;

                cachedAdCores.Add(core);
            }

            cachedAdCores.Sort((a, b) => string.CompareOrdinal(a.AdCoreName, b.AdCoreName));

            cachedAdCoreNames = new string[cachedAdCores.Count];
            for (int i = 0; i < cachedAdCores.Count; i++)
                cachedAdCoreNames[i] = cachedAdCores[i].AdCoreName;

            adCoreCacheDirty = false;
        }
    }
}
