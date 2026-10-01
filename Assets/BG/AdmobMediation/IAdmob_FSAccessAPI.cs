using GoogleMobileAds.Api;
using System;
using BG_Library.NET.Debug;

namespace BG_Library.NET.Mediation.Admob
{
    public interface IAdmob_FSAccessAPI
    {
        void N_Bind(object adInstance);
        void N_RequestAd(string id, Action<object, LoadAdError> onLoadCallback);
        bool N_GetAdReady();
        void N_DestroyAd();
        void N_Show();

        bool P_GetAdReady(string preloadKey);
        void P_DestroyAd(string preloadKey);
        void P_Preload(
            string preloadKey,
            PreloadConfiguration cfg,
            Action<string, ResponseInfo> onPreloaded,
            Action<string, AdError> onFailedToPreload,
            Action<string> onAdsExhausted
        );
        object P_DequeueAd(string preloadKey);
        void P_Show(object ad);

        void SubPaid(Action<AdValue> h);
        void SubClicked(Action h);
        void SubOpened(Action h);
        void SubClosed(Action h);
        void SubFailed(Action<AdError> h);
        void SubReceivedReward(Action h);

        string GetResponseInfoString();
        string GetAdSource();
    }

    // =========================
    // AO
    // =========================
    public sealed class Admob_AOAccessAPI : IAdmob_FSAccessAPI
    {
        private AppOpenAd ad;

        public void N_Bind(object adInstance) => ad = adInstance as AppOpenAd;

        public void N_RequestAd(string id, Action<object, LoadAdError> onLoadCallback)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_ao, "SDK.Load", () => $"id={id}");
            AppOpenAd.Load(id, new AdRequest(), onLoadCallback);
        }

        public bool N_GetAdReady() => ad != null && ad.CanShowAd();

        public void N_DestroyAd()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_ao, "SDK.Destroy", () => $"hasAd={(ad != null)}");
            ad?.Destroy();
        }

        public void N_Show()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_ao, "SDK.Show", () => $"hasAd={(ad != null)}");
            ad?.Show();
        }

        public bool P_GetAdReady(string preloadKey) => AppOpenAdPreloader.IsAdAvailable(preloadKey);

        public void P_DestroyAd(string preloadKey)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_ao, "SDK.PreloadDestroy", () => $"preloadKey={preloadKey}");
            if (!string.IsNullOrEmpty(preloadKey))
                AppOpenAdPreloader.Destroy(preloadKey);
        }

        public void P_Preload(string preloadKey, PreloadConfiguration cfg,
            Action<string, ResponseInfo> onPreloaded, Action<string, AdError> onFailedToPreload, Action<string> onAdsExhausted)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_ao, "SDK.Preload", () => $"preloadKey={preloadKey} adUnit={cfg?.AdUnitId} buf={cfg?.BufferSize}");
            AppOpenAdPreloader.Preload(preloadKey, cfg, onPreloaded, onFailedToPreload, onAdsExhausted);
        }

        public object P_DequeueAd(string preloadKey)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_ao, "SDK.Dequeue", () => $"preloadKey={preloadKey}");
            return AppOpenAdPreloader.DequeueAd(preloadKey);
        }

        public void P_Show(object ad)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_ao, "SDK.ShowPreloaded", () => $"adNull={(ad == null)}");
            if (ad is AppOpenAd adIns) adIns?.Show();
        }

        public void SubPaid(Action<AdValue> h) { if (ad != null) ad.OnAdPaid += h; }
        public void SubClicked(Action h) { if (ad != null) ad.OnAdClicked += h; }
        public void SubOpened(Action h) { if (ad != null) ad.OnAdFullScreenContentOpened += h; }
        public void SubClosed(Action h) { if (ad != null) ad.OnAdFullScreenContentClosed += h; }
        public void SubFailed(Action<AdError> h) { if (ad != null) ad.OnAdFullScreenContentFailed += h; }
        public void SubReceivedReward(Action h) { }

        public string GetResponseInfoString() => ad != null ? ad.GetResponseInfo().ToString() : "";
        public string GetAdSource() => ad != null ? ad.GetResponseInfo().GetMediationAdapterClassName() : "";
    }

    // =========================
    // FA
    // =========================
    public sealed class Admob_FAAccessAPI : IAdmob_FSAccessAPI
    {
        private InterstitialAd ad;

        public void N_Bind(object adInstance) => ad = adInstance as InterstitialAd;

        public void N_RequestAd(string id, Action<object, LoadAdError> onLoadCallback)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_fa, "SDK.Load", () => $"id={id}");
            InterstitialAd.Load(id, new AdRequest(), onLoadCallback);
        }

        public bool N_GetAdReady() => ad != null && ad.CanShowAd();

        public void N_DestroyAd()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_fa, "SDK.Destroy", () => $"hasAd={(ad != null)}");
            ad?.Destroy();
        }

        public void N_Show()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_fa, "SDK.Show", () => $"hasAd={(ad != null)}");
            ad?.Show();
        }

        public bool P_GetAdReady(string preloadKey) => InterstitialAdPreloader.IsAdAvailable(preloadKey);

        public void P_DestroyAd(string preloadKey)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_fa, "SDK.PreloadDestroy", () => $"preloadKey={preloadKey}");
            if (!string.IsNullOrEmpty(preloadKey))
                InterstitialAdPreloader.Destroy(preloadKey);
        }

        public void P_Preload(string preloadKey, PreloadConfiguration cfg,
            Action<string, ResponseInfo> onPreloaded, Action<string, AdError> onFailedToPreload, Action<string> onAdsExhausted)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_fa, "SDK.Preload", () => $"preloadKey={preloadKey} adUnit={cfg?.AdUnitId} buf={cfg?.BufferSize}");
            InterstitialAdPreloader.Preload(preloadKey, cfg, onPreloaded, onFailedToPreload, onAdsExhausted);
        }

        public object P_DequeueAd(string preloadKey)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_fa, "SDK.Dequeue", () => $"preloadKey={preloadKey}");
            return InterstitialAdPreloader.DequeueAd(preloadKey);
        }

        public void P_Show(object ad)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_fa, "SDK.ShowPreloaded", () => $"adNull={(ad == null)}");
            if (ad is InterstitialAd adIns) adIns?.Show();
        }

        public void SubPaid(Action<AdValue> h) { if (ad != null) ad.OnAdPaid += h; }
        public void SubClicked(Action h) { if (ad != null) ad.OnAdClicked += h; }
        public void SubOpened(Action h) { if (ad != null) ad.OnAdFullScreenContentOpened += h; }
        public void SubClosed(Action h) { if (ad != null) ad.OnAdFullScreenContentClosed += h; }
        public void SubFailed(Action<AdError> h) { if (ad != null) ad.OnAdFullScreenContentFailed += h; }
        public void SubReceivedReward(Action h) { }

        public string GetResponseInfoString() => ad != null ? ad.GetResponseInfo().ToString() : "";
        public string GetAdSource() => ad != null ? ad.GetResponseInfo().GetMediationAdapterClassName() : "";
    }

    // =========================
    // RW
    // =========================
    public sealed class Admob_RWAccessAPI : IAdmob_FSAccessAPI
    {
        private Action<Reward> userRewardEarnedCallback;
        private RewardedAd ad;

        public void N_Bind(object adInstance) => ad = adInstance as RewardedAd;

        public void N_RequestAd(string id, Action<object, LoadAdError> onLoadCallback)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_rw, "SDK.Load", () => $"id={id}");
            RewardedAd.Load(id, new AdRequest(), onLoadCallback);
        }

        public bool N_GetAdReady() => ad != null && ad.CanShowAd();

        public void N_DestroyAd()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_rw, "SDK.Destroy", () => $"hasAd={(ad != null)}");
            ad?.Destroy();
        }

        public void N_Show()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_rw, "SDK.Show", () => $"hasAd={(ad != null)}");
            ad?.Show(userRewardEarnedCallback);
        }

        public bool P_GetAdReady(string preloadKey) => RewardedAdPreloader.IsAdAvailable(preloadKey);

        public void P_DestroyAd(string preloadKey)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_rw, "SDK.PreloadDestroy", () => $"preloadKey={preloadKey}");
            if (!string.IsNullOrEmpty(preloadKey))
                RewardedAdPreloader.Destroy(preloadKey);
        }

        public void P_Preload(string preloadKey, PreloadConfiguration cfg,
            Action<string, ResponseInfo> onPreloaded, Action<string, AdError> onFailedToPreload, Action<string> onAdsExhausted)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_rw, "SDK.Preload", () => $"preloadKey={preloadKey} adUnit={cfg?.AdUnitId} buf={cfg?.BufferSize}");
            RewardedAdPreloader.Preload(preloadKey, cfg, onPreloaded, onFailedToPreload, onAdsExhausted);
        }

        public object P_DequeueAd(string preloadKey)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_rw, "SDK.Dequeue", () => $"preloadKey={preloadKey}");
            return RewardedAdPreloader.DequeueAd(preloadKey);
        }

        public void P_Show(object ad)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.admob_api_rw, "SDK.ShowPreloaded", () => $"adNull={(ad == null)}");
            if (ad is RewardedAd adIns) adIns?.Show(userRewardEarnedCallback);
        }

        public void SubPaid(Action<AdValue> h) { if (ad != null) ad.OnAdPaid += h; }
        public void SubClicked(Action h) { if (ad != null) ad.OnAdClicked += h; }
        public void SubOpened(Action h) { if (ad != null) ad.OnAdFullScreenContentOpened += h; }
        public void SubClosed(Action h) { if (ad != null) ad.OnAdFullScreenContentClosed += h; }
        public void SubFailed(Action<AdError> h) { if (ad != null) ad.OnAdFullScreenContentFailed += h; }

        public void SubReceivedReward(Action h)
        {
            userRewardEarnedCallback += rw => { h?.Invoke(); };
        }

        public string GetResponseInfoString() => ad != null ? ad.GetResponseInfo().ToString() : "";
        public string GetAdSource() => ad != null ? ad.GetResponseInfo().GetMediationAdapterClassName() : "";
    }
}