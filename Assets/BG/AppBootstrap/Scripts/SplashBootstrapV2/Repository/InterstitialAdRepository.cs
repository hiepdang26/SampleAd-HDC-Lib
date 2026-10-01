using System;
using System.Threading;
#if boostrap_ios && UNITY_IOS
using BG_Library.NET.AndroidSDK;
#endif
using BG_Library.NET;
using BG_Library.NET.API;
using BG_Library.Common;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    public class InterstitialAdRepository : IAdRequester
    {
        private AdStatus _status;
        private string _position;
        private float _timeOut;
#if boostrap_ios && UNITY_IOS
        private IOSInterstitialInstance _interstitial;
#else
        private InterstitialAndroidInstance _interstitial;
#endif
        public Action<AdStatus> OnAdStatusChanged;

        public string Position => _position;

        public InterstitialAdRepository(string position, string adUnitId, float timeOut)
        {
            _position = position;
            _timeOut = timeOut;
#if boostrap_ios && UNITY_IOS
            _interstitial = new IOSInterstitialInstance(new[] { adUnitId }, 1, false);
            _interstitial.OnAdLoadFailedEvent += OnIosInterstitialFailedToLoad;
            _interstitial.OnAdDisplayedEvent += OnInterstitialDisplayed;
            _interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
            _interstitial.OnPaidAdImpressionEvent += OnInterstitialPaidImpression;
            _interstitial.OnAdHiddenEvent += OnInterstitialClosed;
            _interstitial.OnAdShowFailedEvent += OnInterstitialShowFailed;
#else
            _interstitial = new InterstitialAndroidInstance(new InterstitialAndroidInstance.InterstitialAndroidConfig(new [] { adUnitId }, false));
            _interstitial.OnInterstitialFailedToLoad += OnInterstitialFailedToLoad;
            _interstitial.OnInterstitialDisplayed += OnInterstitialDisplayed;
            _interstitial.OnInterstitialLoaded += OnInterstitialLoaded;
            _interstitial.OnInterstitialPaidImpression += OnInterstitialPaidImpression;
            _interstitial.OnInterstitialClosed += OnInterstitialClosed;
#endif
        }

        #region CallBack

#if boostrap_ios && UNITY_IOS
        private void OnInterstitialLoaded(BG_Library.NET.AndroidSDK.AdInfo info)
        {
            SplashTracking.Tracking($"5_{_position}_l_s_loaded");
            SplashLogger.Log($"Interstitial splash loaded");
            ChangeStatus(AdStatus.Loaded);
        }

        private void OnIosInterstitialFailedToLoad(string arg1, int arg2, string arg3)
        {
            SplashTracking.Tracking($"5_{_position}_l_s_failed");
            ChangeStatus(AdStatus.LoadFailed);
        }

        private void OnInterstitialDisplayed(BG_Library.NET.AndroidSDK.AdInfo info)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_displayed");
            ChangeStatus(AdStatus.AdDisplayed);
        }

        private void OnInterstitialPaidImpression(BG_Library.NET.AndroidSDK.AdInfo info, BG_Library.NET.AndroidSDK.AdValue paidInfo)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_paid");
            ChangeStatus(AdStatus.AdPaidImpression);
        }
        
        private void OnInterstitialClosed(BG_Library.NET.AndroidSDK.AdInfo info)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_closed");
            ChangeStatus(AdStatus.AdClosed);
        }

        private void OnInterstitialShowFailed(BG_Library.NET.AndroidSDK.AdInfo info, int errorCode, string errorMessage)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_failed");
            ChangeStatus(AdStatus.AdShowFailed);
        }
#else
        private void OnInterstitialLoaded(InterstitialInfo info)
        {
            SplashTracking.Tracking($"5_{_position}_l_s_loaded");
            SplashLogger.Log($"Interstitial splash loaded");
            ChangeStatus(AdStatus.Loaded);
        }

        private void OnInterstitialFailedToLoad(string arg1, int arg2, string arg3)
        {
            SplashTracking.Tracking($"5_{_position}_l_s_failed");
            ChangeStatus(AdStatus.LoadFailed);
        }

        private void OnInterstitialDisplayed(InterstitialInfo info)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_displayed");
            ChangeStatus(AdStatus.AdDisplayed);
        }

        private void OnInterstitialPaidImpression(InterstitialInfo info, InterstitialPaidInfo paidInfo)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_paid");
            ChangeStatus(AdStatus.AdPaidImpression);
        }

        private void OnInterstitialClosed(InterstitialInfo info)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_closed");
            ChangeStatus(AdStatus.AdClosed);
        }
#endif
        
        #endregion

        #region Core

        public async UniTask<AdStatus> LoadAsync(CancellationToken ct)
        {
            _status = AdStatus.Loading;
#if boostrap_ios && UNITY_IOS
            _interstitial.LoadAd();
#else
            _interstitial.LoadInterstitial();
#endif
            await UniTask.WhenAny(
                UniTask.WaitUntil(() => _status != AdStatus.Loading, cancellationToken: ct),
                UniTask.Delay(TimeSpan.FromSeconds(_timeOut), DelayType.DeltaTime, cancellationToken: ct));
            if (_status == AdStatus.Loading)
            {
                SplashTracking.Tracking($"5_{_position}_l_s_timeout");
                ChangeStatus(AdStatus.LoadTimeOut);
            }
            return _status;
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
#if boostrap_ios && UNITY_IOS
            try
            {
                _interstitial.ShowAd();
                await UniTask.WaitUntil(() => _status == AdStatus.AdClosed || _status == AdStatus.AdShowFailed, cancellationToken: ct);
            }
            catch (Exception ex)
            {
                SplashLogger.Warn($"IOS interstitial splash show failed: {ex.Message}");
                ChangeStatus(AdStatus.AdShowFailed);
            }
#else
            _interstitial.ShowInterstitial();
            await UniTask.WaitUntil(() => _status == AdStatus.AdClosed, cancellationToken: ct);
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

        #endregion

    }
}
