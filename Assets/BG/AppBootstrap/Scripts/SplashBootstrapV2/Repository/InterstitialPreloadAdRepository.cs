/*
using System;
using System.Threading;
using BG_Library.NET;
using BG_Library.NET.API;
using BG_Library.Common;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    public class InterstitialPreloadAdRepository : IAdRequester
    {
        private AdStatus _status;
        private string _position;
        private float _timeOut;
        private int _preload;
        private int _preloadWaitCount;
        private InterstitialAdPreload _interstitial;
        
        public Action<AdStatus> OnAdStatusChanged;
        
        public string Position => _position;

        private int _interstitialLoadedCount;
        private int _interstitialLoadFailedCount;
        
        public InterstitialPreloadAdRepository(string position, string adUnitId, int preload, int preloadWaitCount, float timeOut)
        {
            _position = position;
            _preload = preload;
            _preloadWaitCount = preloadWaitCount;
            _timeOut = timeOut;
            _interstitial = new InterstitialAdPreload(adUnitId, false);

            _interstitial.AdDismissed += OnAdDismissed;
            _interstitial.AdShowed += OnAdShowed;
            _interstitial.AdFailedToShow += OnAdFailedToShow;
            
            _interstitial.AdPaid += OnAdFailedToShow;
            _interstitial.AdPreloaded += OnInterstitialPreloaded;
            _interstitial.AdFailedToPreload += OnInterstitialFailedToPreload;
        }

        #region CallBack
        
        private void OnInterstitialPreloaded(string preloadId, string responseId)
        {
            _interstitialLoadedCount++;
            SplashTracking.Tracking($"5_{_position}_l_s_loaded_{_interstitialLoadedCount}");
            SplashLogger.Log($"Interstitials preloaded {_position} - {preloadId} - {_interstitialLoadedCount}");
        }
        
        private void OnInterstitialFailedToPreload(string preloadId, string errorCode, string errorMessage)
        {
            _interstitialLoadedCount--;
            SplashTracking.Tracking($"5_{_position}_l_s_failed");
            SplashLogger.Log($"Interstitials preload failed {_position} - {preloadId} - {_interstitialLoadedCount} - {errorCode} - {errorMessage}");
        }
        
        private void OnAdShowed()
        {
            _interstitial.DestroyAd();
            SplashTracking.Tracking($"5_{_position}_s_s_displayed");
            ChangeStatus(AdStatus.AdDisplayed);
        }
        
        private void OnAdFailedToShow(string errorCode, string errorMessage)
        {
            _interstitial.DestroyAd();
            SplashTracking.Tracking($"5_{_position}_s_s_failed");
            ChangeStatus(AdStatus.AdShowFailed);
        }
        
        private void OnAdFailedToShow(long valueMicros, string currencyCode)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_paid");
            ChangeStatus(AdStatus.AdPaidImpression);
        }

        private void OnAdDismissed()
        {
            SplashTracking.Tracking($"5_{_position}_s_s_closed");
            ChangeStatus(AdStatus.AdClosed);
        }
        
        #endregion

        #region Core

        public async UniTask<AdStatus> LoadAsync(CancellationToken ct)
        {
            _status = AdStatus.Loading;
            _interstitial.PreloadAd(_preload);
            await UniTask.WhenAny(
                UniTask.WaitUntil(() => _interstitialLoadedCount >= _preloadWaitCount, cancellationToken: ct),
                UniTask.Delay(TimeSpan.FromSeconds(_timeOut), DelayType.DeltaTime, cancellationToken: ct));
            return _status;
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            _interstitial.ShowAd();
            await UniTask.WaitUntil(() => _status == AdStatus.AdClosed || _status == AdStatus.AdShowFailed, cancellationToken: ct);
        }

        public bool IsReady()
        {
            return _interstitial.IsAdAvailable();
        }
        
        public void ChangeStatus(AdStatus status)
        {
            _status = status;
            OnAdStatusChanged?.Invoke(status);
        }

        public void DestroyAd()
        {
            _interstitial.DestroyAd();
        }
        #endregion

    }
}
*/
