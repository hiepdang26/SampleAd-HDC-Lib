using System;
using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2AdCoreSdkSection
    {
        public static void Draw(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
                return;

            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (!BuildToolWindowV2Ui.DrawFoldoutHeader("AdCore & SDK", "BG_BuildToolV2_Foldout_AdCoreSdk"))
                    return;

                BuildToolWindowV2.RuntimeSdkState snapshot = owner.runtimeSdkState ?? new BuildToolWindowV2.RuntimeSdkState();

                GUILayout.Label("NET", EditorStyles.boldLabel);
                EditorGUILayout.Space(4f);
                DrawCompactInfoRow("AdsConfigs", owner.adsConfigsLength.ToString());
                EditorGUILayout.Space(4f);
                DrawCachedAdCoreInfos(owner);
                EditorGUILayout.Space(4f);
                DrawCustomRemoteConfigs(owner);

                EditorGUILayout.Space(6f);
                BuildToolWindowV2Ui.DrawWhiteDivider();
                GUILayout.Label("SDK Preset", EditorStyles.boldLabel);
                EditorGUILayout.Space(4f);
                DrawSdkPreset(owner);

                EditorGUILayout.Space(6f);
                BuildToolWindowV2Ui.DrawWhiteDivider();
                GUILayout.Label("SDK Information", EditorStyles.boldLabel);
                EditorGUILayout.Space(4f);
                DrawCompactInfoRow(
                    "MAX SDK",
                    $"Unity {snapshot.MaxUnityVersion} | Android {snapshot.MaxAndroidVersion} | iOS {snapshot.MaxIosVersion}",
                    "AppLovin Integration Manager",
                    BuildToolV2Utilities.OpenAppLovinIntegrationManager,
                    210f);
                DrawCompactList("Adapters", snapshot.MaxAdapters);
                EditorGUILayout.Space(4f);
                DrawCompactInfoRow(
                    "ADMOB SDK",
                    $"Unity {snapshot.AdmobUnityVersion} | Android {snapshot.AdmobAndroidVersion} | iOS {snapshot.AdmobIosVersion}",
                    "Google Mobile Ads Settings",
                    BuildToolV2Utilities.OpenGoogleMobileAdsSettingsInspector,
                    200f);
                DrawCompactList("Adapters", snapshot.AdmobAdapters);
                EditorGUILayout.Space(4f);
                DrawCompactInfoRow("Adjust SDK", snapshot.AdjustSdkVersion);
            }
        }

        public static void ReloadRuntimeSdkState(BuildToolWindowV2 owner)
        {
            BuildToolV2Utilities.ClearEditorCaches();
            owner.runtimeSdkState = new BuildToolWindowV2.RuntimeSdkState();
            owner.runtimeSdkState.MaxUnityVersion = BuildToolV2Utilities.GetAppLovinMaxUnityPluginVersion();
            (owner.runtimeSdkState.MaxAndroidVersion, owner.runtimeSdkState.MaxIosVersion) = BuildToolV2Utilities.GetAppLovinVersionInfo();
            owner.runtimeSdkState.AdmobUnityVersion = BuildToolV2Utilities.GetGoogleMobileAdsUnityPluginVersion();
            (owner.runtimeSdkState.AdmobAndroidVersion, owner.runtimeSdkState.AdmobIosVersion) = BuildToolV2Utilities.GetGoogleMobileAdsVersionInfo();
            owner.runtimeSdkState.AdjustSdkVersion = BuildToolV2Utilities.GetAdjustSdkVersion();
            owner.runtimeSdkState.MaxAdapters = BuildToolV2Utilities.GetMaxMediationAdapterSummaries();
            owner.runtimeSdkState.AdmobAdapters = BuildToolV2Utilities.GetAdmobMediationAdapterSummaries();
        }

        public static void ReloadAdCoreInfoState(BuildToolWindowV2 owner)
        {
            NetConfigsSO netConfigs = owner.profile?.ResolveNetConfigs();
            owner.adsConfigsLength = 0;
            owner.customConfigInfoStates = Array.Empty<BuildToolWindowV2.CustomConfigInfoState>();

            if (netConfigs == null)
            {
                owner.adCoreInfoStates = Array.Empty<BuildToolWindowV2.AdCoreInfoState>();
                return;
            }

            bool useAndroid = owner.profile != null && owner.profile.BuildTarget == BuildTarget.Android;
            string adsConfigs = netConfigs.EditorGetAdsConfigsDefault(useAndroid);
            owner.adsConfigsLength = string.IsNullOrEmpty(adsConfigs) ? 0 : adsConfigs.Length;

            NetConfigsSO.CustomRemoteConfigs[] customConfigs = netConfigs.ListCustomRemoteConfigs;
            if (customConfigs != null && customConfigs.Length > 0)
            {
                owner.customConfigInfoStates = new BuildToolWindowV2.CustomConfigInfoState[customConfigs.Length];
                for (int i = 0; i < customConfigs.Length; i++)
                {
                    NetConfigsSO.CustomRemoteConfigs info = customConfigs[i];
                    string key = info == null || string.IsNullOrWhiteSpace(info.Key) ? $"custom_{i}" : info.Key;
                    string content = info?.Content ?? string.Empty;
                    owner.customConfigInfoStates[i] = new BuildToolWindowV2.CustomConfigInfoState
                    {
                        Key = key,
                        ContentLength = string.IsNullOrEmpty(content) ? 0 : content.Length,
                    };
                }
            }

            if (netConfigs.ListAdCoreInfos == null || netConfigs.ListAdCoreInfos.Length == 0)
            {
                owner.adCoreInfoStates = Array.Empty<BuildToolWindowV2.AdCoreInfoState>();
                return;
            }

            NetConfigsSO.AdCoreInfos[] adCoreInfos = netConfigs.ListAdCoreInfos;
            owner.adCoreInfoStates = new BuildToolWindowV2.AdCoreInfoState[adCoreInfos.Length];
            for (int i = 0; i < adCoreInfos.Length; i++)
            {
                NetConfigsSO.AdCoreInfos info = adCoreInfos[i];
                string adCoreName = info?.Core == null ? "(missing)" : info.Core.AdCoreName;
                string initAtAwake = info != null && info.InitPluginAwake ? "ON" : "OFF";
                string defaultConfigs = info == null ? string.Empty : info.EditorGetDefaultConfigs(useAndroid);
                int defaultConfigsLength = string.IsNullOrEmpty(defaultConfigs) ? 0 : defaultConfigs.Length;
                owner.adCoreInfoStates[i] = new BuildToolWindowV2.AdCoreInfoState
                {
                    Name = string.IsNullOrWhiteSpace(adCoreName) ? $"adcore_{i}" : adCoreName,
                    InitAtAwake = initAtAwake,
                    DefaultConfigsLength = defaultConfigsLength,
                };
            }
        }

        private static void DrawCachedAdCoreInfos(BuildToolWindowV2 owner)
        {
            if (owner.adCoreInfoStates == null || owner.adCoreInfoStates.Length == 0)
            {
                EditorGUILayout.HelpBox("Configs SO hiện chưa có adcore nào.", MessageType.Info);
                return;
            }

            for (int i = 0; i < owner.adCoreInfoStates.Length; i++)
            {
                BuildToolWindowV2.AdCoreInfoState info = owner.adCoreInfoStates[i];
                DrawCompactInfoRow(
                    string.IsNullOrWhiteSpace(info.Name) ? $"adcore_{i}" : info.Name,
                    $"Init At Awake: {info.InitAtAwake} | {info.DefaultConfigsLength}");
            }
        }

        private static void DrawCustomRemoteConfigs(BuildToolWindowV2 owner)
        {
            if (owner.customConfigInfoStates == null || owner.customConfigInfoStates.Length == 0)
                return;

            for (int i = 0; i < owner.customConfigInfoStates.Length; i++)
            {
                BuildToolWindowV2.CustomConfigInfoState info = owner.customConfigInfoStates[i];
                DrawCompactInfoRow(info.Key, info.ContentLength.ToString());
            }
        }

        private static void DrawCompactInfoRow(string label, string value, string buttonLabel = null, Action buttonAction = null, float buttonWidth = 120f)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(220f));
                GUILayout.Label(string.IsNullOrWhiteSpace(value) ? "-" : value, EditorStyles.wordWrappedLabel);

                if (buttonAction != null && !string.IsNullOrWhiteSpace(buttonLabel))
                {
                    if (GUILayout.Button(buttonLabel, GUILayout.Width(buttonWidth)))
                        buttonAction.Invoke();
                }
            }
        }

        private static void DrawCompactList(string label, string[] items)
        {
            string value = items == null || items.Length == 0 ? "-" : string.Join("\n", items);
            DrawCompactInfoRow(label, value);
        }

        private static void DrawSdkPreset(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
                return;

            SdkVersionPresetV2 currentPreset = owner.profile.SdkVersionPreset;
            EditorGUI.BeginChangeCheck();
            SdkVersionPresetV2 nextPreset = (SdkVersionPresetV2)EditorGUILayout.ObjectField("Preset Asset", currentPreset, typeof(SdkVersionPresetV2), false);
            if (EditorGUI.EndChangeCheck())
            {
                owner.profile.SetSdkVersionPreset(nextPreset);
                EditorUtility.SetDirty(owner.profile);
                AssetDatabase.SaveAssets();
                owner.MarkValidationScanStale(true);
                owner.lastExecutionResult = null;
                owner.RefreshWindowData(false, false, false);
                GUIUtility.ExitGUI();
            }

            if (currentPreset == null)
            {
                EditorGUILayout.HelpBox("Khong chon SDK Preset. Validation Scan se bo qua phan check version/adapters theo preset.", MessageType.Info);
                return;
            }

            DrawCompactInfoRow("Preset Code", currentPreset.PresetCode);
            DrawCompactInfoRow("Entries", currentPreset.EntryCount.ToString());
            if (!string.IsNullOrWhiteSpace(currentPreset.Notes))
                EditorGUILayout.HelpBox(currentPreset.Notes, MessageType.None);
        }
    }
}
