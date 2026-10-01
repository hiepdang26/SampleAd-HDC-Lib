using System;

namespace BG_Library.NET.AndroidSDK
{
    public class FSInterstitialPreloadInstance : IFSInstance
    {
        public event Action<AdInfo> OnAdLoadedEvent;
        public event Action<string, int, string> OnAdLoadFailedEvent;
        public event Action<AdInfo> OnAdDisplayedEvent;
        public event Action<AdInfo> OnAdClicked;
        public event Action<AdInfo, AdValue> OnPaidAdImpressionEvent;
        public event Action<AdInfo> OnAdHiddenEvent;
        /*public event Action<AdInfo> OnAdOpenedEvent;
        public event Action OnAdDisplayableEvent;*/

        private readonly InterstitialAdPreload _instance;
        private readonly AdInfo _adInfo;
        private readonly int _bufferSize;
        private bool _isPreloaded;

        public FSInterstitialPreloadInstance(string adUnitId, int bufferSize)
        {
            _instance = new InterstitialAdPreload(adUnitId);
            _bufferSize = bufferSize;
            _adInfo = new AdInfo { adUnitId = adUnitId };

            _instance.Log($"FSPreload create unit={adUnitId} bufferSize={bufferSize}");
            ListenerCallBack();
        }

        #region CallBack

        private void ListenerCallBack()
        {
            _instance.AdPreloaded += OnInterstitialLoaded;
            _instance.AdFailedToPreload += OnInterstitialFailedToLoad;
            _instance.AdShowed += OnInterstitialDisplayed;
            _instance.AdClicked += OnInterstitialAdClicked;
            _instance.AdPaid += OnInterstitialPaidImpression;
            _instance.AdDismissed += OnInterstitialClosed;

            _instance.Log($"FSPreload listeners attached unit={_instance.AdUnitId}");
        }

        private void OnInterstitialPaidImpression(long valueMicros, string currencyCode)
        {
            _instance.Log($"FSPreload paid unit={_instance.AdUnitId} valueMicros={valueMicros} currency={currencyCode}");
            OnPaidAdImpressionEvent?.Invoke(_adInfo, new AdValue
            {
                revenueMicros = valueMicros,
                currencyCode = currencyCode,
                precisionType = 0
            });
        }

        private void OnInterstitialClosed()
        {
            _instance.Log($"FSPreload closed unit={_instance.AdUnitId} available={_instance.IsAdAvailable()}");
            OnAdHiddenEvent?.Invoke(_adInfo);

        }

        private void OnInterstitialDisplayed()
        {
            _instance.Log($"FSPreload displayed unit={_instance.AdUnitId}");
            OnAdDisplayedEvent?.Invoke(_adInfo);
        }

        private void OnInterstitialFailedToLoad(string preloadId, string errorCode, string errorMessage)
        {
            int.TryParse(errorCode, out var code);
            _instance.LogError($"FSPreload load failed unit={_instance.AdUnitId} preloadId={preloadId} code={code} raw={errorCode} msg={errorMessage}");
            OnAdLoadFailedEvent?.Invoke(null, code, errorMessage);
        }

        private void OnInterstitialLoaded(string preloadId, string responseId)
        {
            _instance.Log($"FSPreload loaded unit={_instance.AdUnitId} preloadId={preloadId} responseId={responseId}");
            OnAdLoadedEvent?.Invoke(_adInfo);
        }

        private void OnInterstitialAdClicked()
        {
            _instance.Log($"FSPreload clicked unit={_instance.AdUnitId}");
            OnAdClicked?.Invoke(_adInfo);
        }

        #endregion

        #region Core

        public void LoadAd()
        {
            if (!_isPreloaded)
            {
                _instance.Log($"FSPreload preload start unit={_instance.AdUnitId} bufferSize={_bufferSize}");
                _instance.PreloadAd(_bufferSize);
                _isPreloaded = true;
            }
            else
            {
                _instance.LogError($"preload {_instance.AdUnitId} failed by preloaded!");
            }
        }

        public void ShowAd()
        {
            _instance.Log($"FSPreload show request unit={_instance.AdUnitId} available={_instance.IsAdAvailable()}");
            _instance.ShowAd();
        }

        public void DestroyAd()
        {
            _instance.Log($"FSPreload destroy skipped unit={_instance.AdUnitId} reason=not-implemented");
            // Empty
        }

        public bool IsReady()
        {
            return _instance.IsAdAvailable();
        }

        #endregion
    }
}
