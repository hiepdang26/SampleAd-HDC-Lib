using AppBootstrap.Splash;
using BG_Library.NET.API;
using BG_Library.NET.AdSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Splash
{
    internal sealed class SplashPUAdHandler : ISplashAdHandler, IEndSceneCloseAds
    {
        private readonly Dictionary<string, PULayout> _layoutsByEntryKey = new Dictionary<string, PULayout>();
        public AdGroupType GroupType => AdGroupType.PU;

        public void Initialize(string groupName)
        {
            NetCallerAPI.PU_InitManually(groupName);
        }

        public bool IsReady(string groupName)
        {
            return AdsLogic.Ins.PU_ManagerIns.IsGroupReady(groupName);
        }

        public void AdHide(SplashAdExecutionResult result)
        {
            NetCallerAPI.PU_Hide(result.SelectedAdsPosition);
        }

        public async void AdShow(AdGroupEntry groupEntry, AdInfoConfig adInfoConfig, IAdStorage adStorage, SplashAdExecutionResult result, SplashAdShowContext context, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(result.SelectedAdsPosition))
            {
                return;
            }

            var layout = EnsureLayout(groupEntry, context);
            if (layout == null)
            {
                return;
            }

            var adData = adStorage.GetAdGroupDataEntry(groupEntry.type, result.SelectedGroupName);
            var layoutId = groupEntry.defaultLayoutId;
            if (groupEntry.layouts != null)
            {
                foreach (var groupEntryLayout in groupEntry.layouts)
                {
                    if (groupEntryLayout.adSourceId == adData.adInfo.adSource)
                        layoutId = groupEntryLayout.layoutId;
                }
            }
            var layoutConfig = adInfoConfig.GetAdLayoutConfig(layoutId);
            
            if (layoutConfig.canvasScalerData != null && layout.CanvasScaler != null)
            {
                CanvasScalerJsonUtility.ApplyData(layout.CanvasScaler, layoutConfig.canvasScalerData);
            }

            if (layoutConfig.layoutData != null && layout.Target != null)
            {
                RectTransformJsonUtility.ApplyData(layout.Target, layoutConfig.layoutData);
            }
            else
            {
                SplashLogger.Warn($"PU layout config missing rectTransformData for group={result.SelectedGroupName} layoutId={layoutId}");
            }

            RefreshLayout(layout);
            layout.InvalidateCache();
            
            adStorage.SetStatus(groupEntry.type, result.SelectedGroupName, AdGroupStatus.AdsShowing);
            NetCallerAPI.PU_UpdatePos(result.SelectedAdsPosition, layout);
            NetCallerAPI.PU_Show(result.SelectedAdsPosition);

            float showDurationSeconds = Math.Max(0f, groupEntry.showDurationSeconds);
            if (showDurationSeconds <= 0f)
            {
                return;
            }

            SplashLogger.Log($"Waiting PU show duration type={groupEntry.type} position={result.SelectedAdsPosition} seconds={showDurationSeconds}");
            await UniTask.Delay(TimeSpan.FromSeconds(showDurationSeconds), DelayType.DeltaTime, cancellationToken: cancellationToken);
            adStorage.SetStatus(groupEntry.type, groupEntry.groupName, AdGroupStatus.AdsComplete);
            /*if (groupEntry.isAdCompleteAutoHidden)
            {
                NetCallerAPI.PU_Hide(result.SelectedAdsPosition);
            }*/
        }

        public async UniTask WaitToCompleteHandlerAsync(AdGroupEntry groupEntry, IAdStorage adStorage, CancellationToken cancellationToken)
        {
            if(adStorage.GetStatus(groupEntry.type, groupEntry.groupName) != AdGroupStatus.AdsShowing) 
                return;
            
            await UniTask.WaitUntil(() => adStorage.GetStatus(groupEntry.type, groupEntry.groupName) == AdGroupStatus.AdsComplete, cancellationToken: cancellationToken);
        }

        private PULayout EnsureLayout(AdGroupEntry groupEntry, SplashAdShowContext context)
        {
            string entryKey = BuildEntryKey(groupEntry);
            if (_layoutsByEntryKey.TryGetValue(entryKey, out var existingLayout) && existingLayout != null)
            {
                return existingLayout;
            }

            var layout = CreateLayoutInstance(context.PuLayoutTemplate, entryKey);
            if (layout != null)
            {
                _layoutsByEntryKey[entryKey] = layout;
            }

            return layout;
        }

        private static string BuildEntryKey(AdGroupEntry groupEntry)
        {
            return $"{groupEntry.groupName}|{groupEntry.adsPosition}|{groupEntry.groupNameBackup}|{groupEntry.adsPositionBackup}";
        }

        private static void RefreshLayout(PULayout layout)
        {
            Canvas.ForceUpdateCanvases();

            RectTransform layoutRoot = null;
            if (layout?.Canvas != null)
            {
                layoutRoot = layout.Canvas.transform as RectTransform;
            }

            if (layoutRoot == null)
            {
                layoutRoot = layout?.Target;
            }

            if (layoutRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);
            }

            Canvas.ForceUpdateCanvases();
        }

        private static PULayout CreateLayoutInstance(PULayout template, string entryKey)
        {
            if (template == null)
            {
                SplashLogger.Error($"PULayout template is missing for entry={entryKey}");
                return null;
            }

            var instance = UnityEngine.Object.Instantiate(template);
            instance.name = $"Splash PU layout [{entryKey}]";
            instance.gameObject.SetActive(true);
            instance.InvalidateCache();
            SplashLogger.Log($"Spawn PULayout from template for entry={entryKey}");
            return instance;
        }
    }
}
