using System;

namespace BG_Library.NET.AndroidSDK
{
    public class FSInterstitialInstance : IFSInstance
    {
        public event Action<AdInfo> OnAdLoadedEvent;
        public event Action<string, int, string> OnAdLoadFailedEvent;
        public event Action<AdInfo> OnAdDisplayedEvent;
        public event Action<AdInfo> OnAdClicked;
        public event Action<AdInfo, AdValue> OnPaidAdImpressionEvent;
        public event Action<AdInfo> OnAdHiddenEvent;
        /*public event Action<AdInfo> OnAdOpenedEvent;
        public event Action OnAdDisplayableEvent;*/

        private readonly InterstitialAndroidInstance _instance;
        private readonly AdInfo _adInfo;

        public FSInterstitialInstance(string adUnitId)
        {
            _instance = new InterstitialAndroidInstance(new InterstitialAndroidInstance.InterstitialAndroidConfig(new []{adUnitId}, false));
            _adInfo = new AdInfo { adUnitId = adUnitId };
            
            ListenerCallBack();
        }

        #region CallBack

        private void ListenerCallBack()
        {
            _instance.OnInterstitialLoaded += OnInterstitialLoaded;
            _instance.OnInterstitialFailedToLoad += OnInterstitialFailedToLoad;
            _instance.OnInterstitialDisplayed += OnInterstitialDisplayed;
            _instance.OnAdClicked += OnInterstitialAdClicked;
            _instance.OnInterstitialPaidImpression += OnInterstitialPaidImpression;
            _instance.OnInterstitialClosed += OnInterstitialClosed;
        }

        private void OnInterstitialClosed(InterstitialInfo info)
        {
            OnAdHiddenEvent?.Invoke(_adInfo);
        }

        private void OnInterstitialPaidImpression(InterstitialInfo info, InterstitialPaidInfo paidInfo)
        {
            OnPaidAdImpressionEvent?.Invoke(_adInfo, new AdValue
            {
                revenueMicros = paidInfo.revenueMicros,
                currencyCode = paidInfo.currencyCode,
                precisionType = 0
            });
        }

        private void OnInterstitialAdClicked()
        {
            OnAdClicked?.Invoke(_adInfo);
        }

        private void OnInterstitialDisplayed(InterstitialInfo info)
        {
            OnAdDisplayedEvent?.Invoke(_adInfo);
        }

        private void OnInterstitialFailedToLoad(string adUnit, int errorCode, string msg)
        {
            OnAdLoadFailedEvent?.Invoke(adUnit, errorCode, msg);
        }

        private void OnInterstitialLoaded(InterstitialInfo info)
        {
            _adInfo.mediationAdapter = info.mediationAdapter;
            _adInfo.adSource = info.adSource;
            _adInfo.responseId = info.responseId;
            
            OnAdLoadedEvent?.Invoke(_adInfo);
        }

        #endregion

        #region Core

        public void LoadAd()
        {
            _instance.LoadInterstitial();
        }

        public void ShowAd()
        {
            _instance.ShowInterstitial();
        }

        public void DestroyAd()
        {
            // Empty
        }

        public bool IsReady()
        {
            return _instance.IsReady();
        }

        #endregion
    }
}