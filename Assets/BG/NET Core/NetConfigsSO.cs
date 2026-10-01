using System;
using BG_Library.Common;
using Sirenix.OdinInspector;
using UnityEngine;
using BG_Library.NET.Debug;

namespace BG_Library.NET.AdSystem
{
    public enum InitNetType
    {
        /// <summary>
        /// Configs default
        /// </summary>
        Default = 0,
        /// <summary>
        /// Configs từ Firebase Remote Configs default
        /// </summary>
        RemoteConfigs = 1,
        /// <summary>
        /// Configs từ Firebase Remote Configs theo MMP
        /// </summary>
    }

    [CreateAssetMenu(fileName = "Configs SO", menuName = "BG_Library/NET/Config/Configs SO")]
    public class NetConfigsSO : ScriptableObject
    {
        public static NetConfigsSO Ins
        {
            get
            {
                if (!ins)
                {
                    ins = LoadSource.LoadObject<NetConfigsSO>("Configs SO");
                }

                return ins;
            }
        }
        private static NetConfigsSO ins;

        [BoxGroup("Configs"), SerializeField, LabelText("Debug Preset")] DebugPreset debug_Preset = DebugPreset.FullDebug;
        [BoxGroup("Configs"), SerializeField, LabelText("Hack")] bool build_Hack;
        [BoxGroup("Configs"), SerializeField, LabelText("AdMob Test Device")] bool admob_testDevice;
        [BoxGroup("Configs"), SerializeField, LabelText("AdMob Test IDs")] bool admob_testId;
        [BoxGroup("Configs"), ReadOnly, SerializeField, LabelText("Tracking Firebase")] bool tracking_SendToFirebase;
        [BoxGroup("Configs"), SerializeField, LabelText("Debug UI Keep Landscape")] bool debugUi_KeepLandscape;

        [BoxGroup("Adjust"), SerializeField] private string adjustEventIAPAndroid;
        [BoxGroup("Adjust"), SerializeField] private string adjustEventIAPIOS;

        [BoxGroup("Mediation"), SerializeField] private InitNetType setupConfigNETType;

        [FoldoutGroup("SetDefault Remote Configs", expanded: false)]
        [SerializeField, LabelText("Ads Configs Android (JSON)"), TextArea(4, 10)]
        private string adsConfigsAndroid;

        [FoldoutGroup("SetDefault Remote Configs")]
        [SerializeField, LabelText("Ads Configs IOS (JSON)"), TextArea(4, 10)]
        private string adsConfigsIOS;
        
        [FoldoutGroup("SetDefault Remote Configs", expanded: false)]
        [SerializeField, LabelText("Ads Configs Country (JSON)"), TextArea(4, 10)]
        private string adsConfigsCountry;  

        [BoxGroup("SetDefault Remote Configs"), SerializeField, ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        private CustomRemoteConfigs[] listCustomRemoteConfigs;
        
        [BoxGroup("Ad cores"), SerializeField, ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        private AdCoreInfos[] adCores;

        public bool Build_Hack => build_Hack;
        public DebugPreset Debug_Preset => NetFlowDebugSystem.NormalizePreset(debug_Preset);

        public bool Admob_TestDevice => admob_testDevice;
        public bool Admob_TestId => admob_testId;
        public bool Tracking_SendToFirebase => tracking_SendToFirebase;
        public bool DebugUI_KeepLandscape => debugUi_KeepLandscape;

        public string AdjustEventIAP
        {
            get
            {
#if UNITY_ANDROID
                return adjustEventIAPAndroid;
#else
                return adjustEventIAPIOS;
#endif
            }
        }

        public InitNetType SetupConfigNETType => setupConfigNETType;

        public string AdsConfigsDefault
        {
            get
            {
#if UNITY_ANDROID
                return adsConfigsAndroid;
#else
                return adsConfigsIOS;
#endif
            }
        }

        public string AdsConfigsCountry => adsConfigsCountry;

        public CustomRemoteConfigs[] ListCustomRemoteConfigs => listCustomRemoteConfigs;
        public AdCoreInfos[] ListAdCoreInfos => adCores;

#if UNITY_EDITOR
        public string EditorGetAdsConfigsDefault(bool useAndroid)
        {
            return useAndroid ? adsConfigsAndroid : adsConfigsIOS;
        }

        public void EditorSetAdsConfigsDefault(bool useAndroid, string json)
        {
            if (useAndroid)
                adsConfigsAndroid = json ?? "";
            else
                adsConfigsIOS = json ?? "";

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public string EditorGetAdCoreDefault(string adCoreName, bool useAndroid)
        {
            if (adCores == null || adCores.Length == 0 || string.IsNullOrEmpty(adCoreName))
                return "";

            for (int i = 0; i < adCores.Length; i++)
            {
                var info = adCores[i];
                if (info?.Core == null) continue;
                if (info.Core.AdCoreName != adCoreName) continue;

                return info.EditorGetDefaultConfigs(useAndroid);
            }

            return "";
        }

        public bool EditorSetAdCoreDefault(string adCoreName, bool useAndroid, string json)
        {
            if (adCores == null || adCores.Length == 0 || string.IsNullOrEmpty(adCoreName))
                return false;

            for (int i = 0; i < adCores.Length; i++)
            {
                var info = adCores[i];
                if (info?.Core == null) continue;
                if (info.Core.AdCoreName != adCoreName) continue;

                info.EditorSetDefaultConfigs(useAndroid, json ?? "");

                UnityEditor.EditorUtility.SetDirty(this);
                UnityEditor.AssetDatabase.SaveAssets();
                return true;
            }

            return false;
        }

        public bool EditorEnsureAdCoreExists(BG_Library.NET.AdCoreBase adCore)
        {
            if (adCore == null)
                return false;

            if (adCores != null)
            {
                for (int i = 0; i < adCores.Length; i++)
                {
                    var info = adCores[i];
                    if (info?.Core == null) continue;
                    if (info.Core == adCore) return false;
                    if (info.Core.AdCoreName == adCore.AdCoreName) return false;
                }
            }

            int oldLength = adCores?.Length ?? 0;
            Array.Resize(ref adCores, oldLength + 1);

            var newInfo = new AdCoreInfos();
            newInfo.EditorSetCore(adCore);
            newInfo.EditorSetDefaultConfigs(true, "");
            newInfo.EditorSetDefaultConfigs(false, "");

            adCores[oldLength] = newInfo;

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            return true;
        }
#endif

        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder(1024);

            sb.AppendLine("===== NetConfigsSO Debug Info =====");

            sb.AppendLine();
            sb.AppendLine("[General]");
            sb.AppendLine($"Build_Hack: {Build_Hack}");

            sb.AppendLine();
            sb.AppendLine("[Ads Test]");
            sb.AppendLine($"Admob_TestDevice: {Admob_TestDevice}");
            sb.AppendLine($"Admob_TestId: {Admob_TestId}");

            sb.AppendLine();
            sb.AppendLine("[Debug Configs]");
            sb.AppendLine($"Debug_Preset: {Debug_Preset}");
            sb.AppendLine(NetFlowDebugSystem.DescribePresetVi(Debug_Preset));
            sb.AppendLine($"Layers: {NetFlowDebugSystem.DescribeLayersVi(Debug_Preset)}");
            sb.AppendLine($"Tracking: {NetFlowDebugSystem.DescribeTrackingVi(Debug_Preset)}");
            sb.AppendLine($"Tracking_SendToFirebase: {Tracking_SendToFirebase}");
            sb.AppendLine($"DebugUI_KeepLandscape: {DebugUI_KeepLandscape}");

            sb.AppendLine();
            sb.AppendLine("[Adjust]");
            sb.AppendLine($"AdjustEventIAP: {AdjustEventIAP}");

            sb.AppendLine();
            sb.AppendLine("[Mediation]");
            sb.AppendLine($"SetupConfigNETType: {SetupConfigNETType}");

            sb.AppendLine();
            sb.AppendLine("[Custom Remote Configs]");
            if (listCustomRemoteConfigs == null || listCustomRemoteConfigs.Length == 0)
            {
                sb.AppendLine("None");
            }
            else
            {
                for (int i = 0; i < listCustomRemoteConfigs.Length; i++)
                {
                    var item = listCustomRemoteConfigs[i];
                    if (item == null) continue;

                    sb.AppendLine($"  #{i}");
                    sb.AppendLine($"    Key: {item.Key}");
                    sb.AppendLine($"    ContentLength: {(string.IsNullOrEmpty(item.Content) ? 0 : item.Content.Length)}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("[Ad Cores]");
            if (adCores == null || adCores.Length == 0)
            {
                sb.AppendLine("None");
            }
            else
            {
                for (int i = 0; i < adCores.Length; i++)
                {
                    var core = adCores[i];
                    if (core == null) continue;

                    string unityName = core.Core != null ? core.Core.name : "NULL";
                    string adCoreName = core.Core != null ? core.Core.AdCoreName : "NULL";

                    sb.AppendLine($"  #{i}");
                    sb.AppendLine($"    CoreSO: {unityName}");
                    sb.AppendLine($"    selectedAdCoreName: {adCoreName}");
                    sb.AppendLine($"    InitPluginAwake: {core.InitPluginAwake}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("===== End NetConfigsSO Debug =====");

            return sb.ToString();
        }

        public void SetDebugPreset(DebugPreset preset)
        {
            debug_Preset = NetFlowDebugSystem.NormalizePreset(preset);
            PersistChanges();
        }

        public void SetTrackingSendToFirebase(bool value)
        {
            tracking_SendToFirebase = value;
            PersistChanges();
        }

        private void PersistChanges()
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif

            if (Application.isPlaying)
                BG_SETUP.RefreshRuntimeDebugState();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            debug_Preset = NetFlowDebugSystem.NormalizePreset(debug_Preset);
        }
#endif

        [System.Serializable]
        public class AdCoreInfos
        {
            [SerializeField] BG_Library.NET.AdCoreBase core;

            [SerializeField] bool initPluginAwake;
            [SerializeField, LabelText("Default Configs Android (JSON)"), TextArea(4, 10)]
            string defaultConfigsAndroid;

            [SerializeField, LabelText("Default Configs IOS (JSON)"), TextArea(4, 10)]
            string defaultConfigsIOS;
            
            [SerializeField, LabelText("Default Configs Country (JSON)"), TextArea(4, 10)]
            string defaultConfigsCountry;

            public BG_Library.NET.AdCoreBase Core => core;
            public bool InitPluginAwake => initPluginAwake;
            
            public string DefaultConfigsCountry => defaultConfigsCountry;

            public string DefaultConfigs
            {
                get
                {
#if UNITY_ANDROID
                    return defaultConfigsAndroid;
#else
                    return defaultConfigsIOS;
#endif
                }
            }

#if UNITY_EDITOR
            public string EditorGetDefaultConfigs(bool useAndroid)
            {
                return useAndroid ? defaultConfigsAndroid : defaultConfigsIOS;
            }

            public void EditorSetDefaultConfigs(bool useAndroid, string json)
            {
                if (useAndroid)
                    defaultConfigsAndroid = json ?? "";
                else
                    defaultConfigsIOS = json ?? "";
            }

            public void EditorSetCore(BG_Library.NET.AdCoreBase value)
            {
                core = value;
            }
#endif
        }

        [System.Serializable]
        public class CustomRemoteConfigs
        {
            [SerializeField] private string key;
            [SerializeField, LabelText("Content Android (JSON)"), TextArea(3, 10)]
            private string contentAndroid;

            [SerializeField, LabelText("Content IOS (JSON)"), TextArea(3, 10)]
            private string contentIOS;
            
            [SerializeField, LabelText("Content Country (JSON)"), TextArea(3, 10)]
            private string contentCountry;

            public string Key
            {
                get => key;
                set => key = value;
            }
            
            public string ContentCountry => contentCountry;

            public string Content
            {
#if UNITY_ANDROID
                get => contentAndroid;
                set => contentAndroid = value;
#else
                get => contentIOS;
                set => contentIOS = value;
#endif
            }

            public CustomRemoteConfigs() { }

            public CustomRemoteConfigs(string key, string content)
            {
                this.key = key;
#if UNITY_ANDROID
                contentAndroid = content;
#else
                contentIOS = content;
#endif
            }
        }
    }
}
