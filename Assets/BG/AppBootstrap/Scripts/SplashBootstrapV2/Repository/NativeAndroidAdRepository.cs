using System;
using System.Threading;
using BG_Library.NET;
using BG_Library.NET.API;
using BG_Library.Common;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.AdSystem;
using BG_Library.NET.AndroidSDK;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AppBootstrap.Splash
{
    public class NativeAndroidAdRepository : IAdRequester
    {
        private readonly string _adUnitId;
        private AdStatus _status;
        private string _position;
        private bool _isTracking;
        private bool _isRaisedUpdateLastFSAd;

#if boostrap_ios && UNITY_IOS
        private IOSFSNativeInstance _instance;
#else
        private AndroidFSNativeInstance _instance;
#endif
        private LayoutGroupPicker _layoutGroupPicker;
        private string _loadedAdSourceId;

        public string Position => _position;
        public AdStatus Status => _status;
        public Action<AdStatus> OnAdStatusChanged;
        public Action<NativeAdInfo, NativeAdPaidInfo> OnAdPaid;

        public NativeAndroidAdRepository(string position, string adUnitId, LayoutGroupConfig layoutConfig, bool tracking = true, bool isRaisedUpdateLastFSAd = true)
        {
            _adUnitId = adUnitId;
            _position = position;
            _layoutGroupPicker = new LayoutGroupPicker(layoutConfig);
            _isTracking = tracking;
            _isRaisedUpdateLastFSAd = isRaisedUpdateLastFSAd;

#if boostrap_ios && UNITY_IOS
            _instance = new IOSFSNativeInstance(new[] {_adUnitId}, layoutConfig);

            _instance.OnAdLoadedEvent += OnAdLoaded;
            _instance.OnAdLoadFailedEvent += OnIosAdLoadFailed;
            _instance.OnPaidAdImpressionEvent += OnAdPaidImpression;
            _instance.OnAdDisplayedEvent += OnRectDisplayed;
            _instance.OnAdHiddenEvent += OnAdClosed;
            _instance.OnAdShowFailedEvent += OnAdShowFailed;
#else
            _instance = new AndroidFSNativeInstance(new AndroidNAConfig(new string[] {_adUnitId}));

            _instance.OnFSNALoaded += OnAdLoaded;
            _instance.OnFSNAFailedToLoad += OnAdLoadFailed;
            _instance.OnFSNAPaidImpression += OnAdPaidImpression;
            _instance.OnFSNADisplayed += OnRectDisplayed;
            _instance.OnFSNAClosed += OnAdClosed;
#endif
        }

        #region CallBack

#if boostrap_ios && UNITY_IOS
        private void OnAdLoaded(BG_Library.NET.AndroidSDK.AdInfo adInfo)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_l_s_loaded");
            _loadedAdSourceId = adInfo?.adSourceId;
            ChangeStatus(AdStatus.Loaded);
        }

        private void OnAdClosed(BG_Library.NET.AndroidSDK.AdInfo adInfo)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_closed");
            ChangeStatus(AdStatus.AdClosed);
        }

        private void OnRectDisplayed(BG_Library.NET.AndroidSDK.AdInfo obj)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_displayed");
            ChangeStatus(AdStatus.AdDisplayed);
        }

        private void OnAdPaidImpression(BG_Library.NET.AndroidSDK.AdInfo adInfo, BG_Library.NET.AndroidSDK.AdValue adPaidInfo)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_paid");
            ChangeStatus(AdStatus.AdPaidImpression);
            OnAdPaid?.Invoke(ToNativeAdInfo(adInfo), ToNativePaidInfo(adPaidInfo));
        }

        private void OnIosAdLoadFailed(string arg1, int arg2, string arg3)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_l_s_failed");
            ChangeStatus(AdStatus.LoadFailed);
        }

        private void OnAdShowFailed(BG_Library.NET.AndroidSDK.AdInfo adInfo, int errorCode, string errorMessage)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_failed");
            ChangeStatus(AdStatus.AdShowFailed);
        }
#else
        private void OnAdLoaded(NativeAdInfo adInfo)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_l_s_loaded");
            _loadedAdSourceId = adInfo.adSourceId;
            ChangeStatus(AdStatus.Loaded);
        }

        private void OnAdClosed(NativeAdInfo adInfo)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_closed");
            ChangeStatus(AdStatus.AdClosed);
        }

        private void OnRectDisplayed(NativeAdInfo obj)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_displayed");
            ChangeStatus(AdStatus.AdDisplayed);
        }

        private void OnAdPaidImpression(NativeAdInfo adInfo, NativeAdPaidInfo adPaidInfo)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_s_s_paid");
            ChangeStatus(AdStatus.AdPaidImpression);
            OnAdPaid?.Invoke(adInfo, adPaidInfo);
        }

        private void OnAdLoadFailed(string arg1, int arg2, string arg3)
        {
            if(_isTracking) SplashTracking.Tracking($"5_{_position}_l_s_failed");
            ChangeStatus(AdStatus.LoadFailed);
        }
#endif

        #endregion

        #region Core

        public async UniTask<AdStatus> LoadAsync(CancellationToken ct)
        {
            ChangeStatus(AdStatus.Loading);
#if boostrap_ios && UNITY_IOS
            _instance.LoadAd();
#else
            _instance.PreloadAll();
#endif
            await UniTask.WaitUntil(() => _status != AdStatus.Loading, cancellationToken: ct);
            return _status;
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
#if boostrap_ios && UNITY_IOS
            try
            {
                AppResumeSystem.Instance.BlockAdResume();
                _instance.ShowAd();
                await UniTask.WaitUntil(() => _status == AdStatus.AdClosed || _status == AdStatus.AdShowFailed, cancellationToken: ct);
                if (_status == AdStatus.AdClosed && _isRaisedUpdateLastFSAd) AdsLogic.UpdateLastTimeFSAd();
            }
            catch (Exception ex)
            {
                SplashLogger.Warn($"IOS native fullscreen show failed position={_position} error={ex.Message}");
                ChangeStatus(AdStatus.AdShowFailed);
            }
#else
            if (_layoutGroupPicker.TryGetLayout(_loadedAdSourceId, out var layoutConfig))
            {
                AppResumeSystem.Instance.BlockAdResume();
                var delay = layoutConfig.Delay;
                var timeUpC = layoutConfig.TimeUpC;
                var time = layoutConfig.LayoutTime;

                if (layoutConfig.Layout.ToLower().Contains("cls"))
                    _instance.ShowOverlayCls(layoutConfig.Layout, (int)time, "auto", layoutConfig.ShowTCD, layoutConfig.PauseGameplay, !layoutConfig.DisableAdComeback, delay, timeUpC);
                else if (layoutConfig.Layout.ToLower().Contains("nav"))
                    _instance.ShowOverlayNav(layoutConfig.Layout, (int)time, "auto",  layoutConfig.ShowTCD, layoutConfig.PauseGameplay, !layoutConfig.DisableAdComeback, delay, timeUpC);
                else
                    _instance.ShowSingleWithPauseOption(new [] { layoutConfig.Layout }, (int)layoutConfig.LayoutTime, layoutConfig.PauseGameplay, "auto", !layoutConfig.DisableAdComeback, delay, timeUpC);

                await UniTask.WaitUntil(() => _status == AdStatus.AdClosed, cancellationToken: ct);
                if(_isRaisedUpdateLastFSAd) AdsLogic.UpdateLastTimeFSAd();
            }
            else
            {
                throw new NullReferenceException("Cant get layout config");
            }
#endif
        }

        public bool IsReady()
        {
            return _status == AdStatus.Loaded;
        }

        public void ChangeStatus(AdStatus status)
        {
            _status = status;
            OnAdStatusChanged?.Invoke(status);
        }

#if boostrap_ios && UNITY_IOS
        private static NativeAdInfo ToNativeAdInfo(BG_Library.NET.AndroidSDK.AdInfo adInfo)
        {
            if (adInfo == null)
                return null;

            return new NativeAdInfo
            {
                adUnitId = adInfo.adUnitId,
                mediationAdapter = adInfo.mediationAdapter,
                responseId = adInfo.responseId,
                adSource = adInfo.adSource,
                adSourceId = adInfo.adSourceId
            };
        }

        private static NativeAdPaidInfo ToNativePaidInfo(BG_Library.NET.AndroidSDK.AdValue adValue)
        {
            if (adValue == null)
                return null;

            return new NativeAdPaidInfo
            {
                revenueMicros = (long)adValue.revenueMicros,
                currencyCode = adValue.currencyCode
            };
        }
#endif
        #endregion

    }
}
