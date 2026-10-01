using System;
using System.Collections.Generic;
using BG_Library.Common;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.Mediation.Android;
using Sirenix.Utilities;
using Random = UnityEngine.Random;

namespace BG_Library.NET.AndroidSDK
{
    public class FSNativeInstance : IFSInstance
    {
        readonly string id;
        readonly string[] ids;
        readonly LayoutGroupConfig layoutGroup;
        readonly string orientation;

        readonly AndroidFSNativeInstance _androidFsNative;
        
        LayoutGroupPicker _layoutGroupPicker;
        string loadedAdSource;
        string loadedAdSourceId;
        string loadedMediationAdapter;
        

        public FSNativeInstance(
            string[] ids,
            LayoutGroupConfig layoutGroup,
            string orientation = null)
        {
            this.ids = ids ?? Array.Empty<string>();
            id = this.ids.Length > 0 ? this.ids[0] : string.Empty;
            this.layoutGroup = layoutGroup;
            this.orientation = string.IsNullOrEmpty(orientation) ? Master.GetOrientationString() : orientation;
            _layoutGroupPicker = new LayoutGroupPicker(layoutGroup);
            _androidFsNative = new AndroidFSNativeInstance(new AndroidNAConfig(this.ids));
            _androidFsNative.OnFSNALoaded += info =>
            {
                SetLoadedNativeInfo(info);
                OnAdLoadedEvent?.Invoke(Map(info));
            };
            _androidFsNative.OnFSNAFailedToLoad += (adUnit, errorCode, msg) => OnAdLoadFailedEvent?.Invoke(adUnit, errorCode, msg);
            _androidFsNative.OnFSNADisplayed += info =>
            {
                SetLoadedNativeInfo(info);
                OnAdDisplayedEvent?.Invoke(Map(info));
            };
            _androidFsNative.OnFSNAClicked += info =>
            {
                SetLoadedNativeInfo(info);
                OnAdClicked?.Invoke(Map(info));
            };
            _androidFsNative.OnFSNAPaidImpression += (info, paid) =>
            {
                SetLoadedNativeInfo(info);
                OnPaidAdImpressionEvent?.Invoke(Map(info), Map(paid));
            };
            _androidFsNative.OnFSNAClosed += info =>
            {
                SetLoadedNativeInfo(info);
                OnAdHiddenEvent?.Invoke(Map(info));
            };
            _androidFsNative.OnFSNAOpened += info =>
            {
                SetLoadedNativeInfo(info);
                OnAdOpenedEvent?.Invoke(Map(info));
            };
            _androidFsNative.OnFSNADisplayable += () => OnAdDisplayableEvent?.Invoke();
        }

        public void LoadAd()
        {
            _androidFsNative?.PreloadAll();
        }

        public void DestroyAd()
        {
            _androidFsNative?.Clear();
        }

        public bool IsReady() => _androidFsNative?.IsReady() ?? false;

        public void ShowAd()
        {
            if (_layoutGroupPicker.TryGetLayout(loadedAdSourceId, out var layoutConfig))
            {
                var delay = layoutConfig.Delay;
                var timeUpC = layoutConfig.TimeUpC;
                var assetVisibility = layoutGroup.ResolveAssetVisibility(layoutConfig.AssetConfigName);

                var layoutNames = new [] { layoutConfig.Layout };
                if (LayoutNamesContain(layoutNames, "cls"))
                {
                    ShowOverlayCls(layoutNames, (int)layoutConfig.LayoutTime, orientation, layoutConfig, delay, timeUpC, assetVisibility: assetVisibility);
                    return;
                }

                if (LayoutNamesContain(layoutNames, "nav"))
                {
                    ShowOverlayNav(layoutNames, (int)layoutConfig.LayoutTime, orientation, layoutConfig, delay, timeUpC, assetVisibility: assetVisibility);
                    return;
                }

                _androidFsNative?.ShowSingleWithPauseOption(new [] { layoutConfig.Layout }, (int)layoutConfig.LayoutTime, layoutConfig.PauseGameplay, orientation, !layoutConfig.DisableAdComeback, delay, timeUpC, assetVisibility: assetVisibility);
            }
            else
            {
                throw new NullReferenceException("Cant get layout config");
            }
        }

        private void ShowOverlayCls(
            string[] layoutNames,
            int durationSeconds,
            string orientation,
            LayoutConfig layoutConfig,
            float delay = 0f,
            int timeUpC = 0,
            NativeClickAssetOptions clickAssets = null,
            NativeAssetVisibilityOptions assetVisibility = null)
        {
            _androidFsNative?.ShowOverlayCls(layoutNames, durationSeconds, orientation, layoutConfig.ShowTCD, layoutConfig.PauseGameplay, !layoutConfig.DisableAdComeback, delay, timeUpC, clickAssets, assetVisibility);
        }

        private void ShowOverlayNav(
            string[] layoutNames,
            int durationSeconds,
            string orientation,
            LayoutConfig layoutConfig,
            float delay = 0f,
            int timeUpC = 0,
            NativeClickAssetOptions clickAssets = null,
            NativeAssetVisibilityOptions assetVisibility = null)
        {
            _androidFsNative?.ShowOverlayNav(layoutNames, durationSeconds, orientation, layoutConfig.ShowTCD, layoutConfig.PauseGameplay, !layoutConfig.DisableAdComeback, delay, timeUpC, clickAssets, assetVisibility);
        }

        private void ShowSingleAd(
            string[] layoutNames,
            int durationSeconds,
            string orientation,
            LayoutConfig layoutConfig,
            float delay = 0f,
            int timeUpC = 0,
            NativeClickAssetOptions clickAssets = null,
            NativeAssetVisibilityOptions assetVisibility = null)
        {
            _androidFsNative?.ShowSingleWithPauseOption(layoutNames, durationSeconds, layoutConfig.PauseGameplay, orientation, !layoutConfig.DisableAdComeback, delay, timeUpC, clickAssets, assetVisibility);
        }

        static bool LayoutNamesContain(string[] layoutNames, string token)
        {
            if (layoutNames == null || string.IsNullOrEmpty(token))
            {
                return false;
            }

            foreach (var layoutName in layoutNames)
            {
                if (!string.IsNullOrEmpty(layoutName) &&
                    layoutName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public event Action<AdInfo> OnAdLoadedEvent;
        public event Action<string, int, string> OnAdLoadFailedEvent;
        public event Action<AdInfo> OnAdDisplayedEvent;
        public event Action<AdInfo> OnAdClicked;
        public event Action<AdInfo, AdValue> OnPaidAdImpressionEvent;
        public event Action<AdInfo> OnAdHiddenEvent;
        public event Action<AdInfo> OnAdOpenedEvent;
        public event Action OnAdDisplayableEvent;

        static AdInfo Map(NativeAdInfo src)
        {
            if (src == null)
            {
                return null;
            }

            return new AdInfo
            {
                adUnitId = src.adUnitId,
                /*headline = src.headline,
                body = src.body,
                callToAction = src.callToAction,
                advertiser = src.advertiser,
                store = src.store,
                price = src.price,*/
                mediationAdapter = src.mediationAdapter,
                responseId = src.responseId,
                adSource = src.adSource,
                adSourceId = src.adSourceId,
            };
        }

        static AdValue Map(NativeAdPaidInfo src)
        {
            if (src == null)
            {
                return null;
            }

            return new AdValue
            {
                revenueMicros = src.revenueMicros,
                currencyCode = src.currencyCode,
                precisionType = 0,
            };
        }

        void SetLoadedNativeInfo(NativeAdInfo info)
        {
            loadedAdSource = info?.adSource;
            loadedAdSourceId = info?.adSourceId;
            loadedMediationAdapter = info?.mediationAdapter;
        }
    }
}
