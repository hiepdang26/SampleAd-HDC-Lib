using System;
using BG_Library.NET.Debug;

namespace BG_Library.NET.Mediation.Max
{
    public interface IMax_FSAccessAPI
    {
        void SubLoaded(Action<string, MaxSdkBase.AdInfo> h);
        void SubLoadFailed(Action<string, MaxSdkBase.ErrorInfo> h);
        void SubDisplayed(Action<string, MaxSdkBase.AdInfo> h);
        void SubClicked(Action<string, MaxSdkBase.AdInfo> h);
        void SubRevenuePaid(Action<string, MaxSdkBase.AdInfo> h);
        void SubHidden(Action<string, MaxSdkBase.AdInfo> h);
        void SubDisplayFailed(Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo> h);
        void SubReceivedReward(Action<string, MaxSdkBase.Reward, MaxSdkBase.AdInfo> h);

        void RequestAd(string id);
        bool GetAdReady(string id);
        void Show(string id);
    }

    public sealed class Max_AOAccessAPI : IMax_FSAccessAPI
    {
        public void SubLoaded(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "Sub.Loaded", () => "MaxSdkCallbacks.AppOpen.OnAdLoadedEvent +=");
            MaxSdkCallbacks.AppOpen.OnAdLoadedEvent += h;
        }
        public void SubLoadFailed(Action<string, MaxSdkBase.ErrorInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "Sub.LoadFailed", () => "MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent +=");
            MaxSdkCallbacks.AppOpen.OnAdLoadFailedEvent += h;
        }
        public void SubDisplayed(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "Sub.Displayed", () => "MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent +=");
            MaxSdkCallbacks.AppOpen.OnAdDisplayedEvent += h;
        }
        public void SubClicked(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "Sub.Clicked", () => "MaxSdkCallbacks.AppOpen.OnAdClickedEvent +=");
            MaxSdkCallbacks.AppOpen.OnAdClickedEvent += h;
        }
        public void SubRevenuePaid(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "Sub.Paid", () => "MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent +=");
            MaxSdkCallbacks.AppOpen.OnAdRevenuePaidEvent += h;
        }
        public void SubHidden(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "Sub.Hidden", () => "MaxSdkCallbacks.AppOpen.OnAdHiddenEvent +=");
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += h;
        }
        public void SubDisplayFailed(Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "Sub.DisplayFailed", () => "MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent +=");
            MaxSdkCallbacks.AppOpen.OnAdDisplayFailedEvent += h;
        }
        public void SubReceivedReward(Action<string, MaxSdkBase.Reward, MaxSdkBase.AdInfo> h) { }

        public void RequestAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "SDK.Load", () => $"id={id}");
            MaxSdk.LoadAppOpenAd(id);
        }

        public bool GetAdReady(string id) => MaxSdk.IsAppOpenAdReady(id);

        public void Show(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_ao, "SDK.Show", () => $"id={id}");
            MaxSdk.ShowAppOpenAd(id);
        }
    }

    public sealed class Max_FAAccessAPI : IMax_FSAccessAPI
    {
        public void SubLoaded(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "Sub.Loaded", () => "MaxSdkCallbacks.Interstitial.OnAdLoadedEvent +=");
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += h;
        }
        public void SubLoadFailed(Action<string, MaxSdkBase.ErrorInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "Sub.LoadFailed", () => "MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent +=");
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += h;
        }
        public void SubDisplayed(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "Sub.Displayed", () => "MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent +=");
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += h;
        }
        public void SubClicked(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "Sub.Clicked", () => "MaxSdkCallbacks.Interstitial.OnAdClickedEvent +=");
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += h;
        }
        public void SubRevenuePaid(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "Sub.Paid", () => "MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent +=");
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += h;
        }
        public void SubHidden(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "Sub.Hidden", () => "MaxSdkCallbacks.Interstitial.OnAdHiddenEvent +=");
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += h;
        }
        public void SubDisplayFailed(Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "Sub.DisplayFailed", () => "MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent +=");
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += h;
        }
        public void SubReceivedReward(Action<string, MaxSdkBase.Reward, MaxSdkBase.AdInfo> h) { }

        public void RequestAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "SDK.Load", () => $"id={id}");
            MaxSdk.LoadInterstitial(id);
        }

        public bool GetAdReady(string id) => MaxSdk.IsInterstitialReady(id);

        public void Show(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_fa, "SDK.Show", () => $"id={id}");
            MaxSdk.ShowInterstitial(id);
        }
    }

    public sealed class Max_RWAccessAPI : IMax_FSAccessAPI
    {
        public void SubLoaded(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "Sub.Loaded", () => "MaxSdkCallbacks.Rewarded.OnAdLoadedEvent +=");
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += h;
        }
        public void SubLoadFailed(Action<string, MaxSdkBase.ErrorInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "Sub.LoadFailed", () => "MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent +=");
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += h;
        }
        public void SubDisplayed(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "Sub.Displayed", () => "MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent +=");
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += h;
        }
        public void SubClicked(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "Sub.Clicked", () => "MaxSdkCallbacks.Rewarded.OnAdClickedEvent +=");
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += h;
        }
        public void SubRevenuePaid(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "Sub.Paid", () => "MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent +=");
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += h;
        }
        public void SubHidden(Action<string, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "Sub.Hidden", () => "MaxSdkCallbacks.Rewarded.OnAdHiddenEvent +=");
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += h;
        }
        public void SubDisplayFailed(Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "Sub.DisplayFailed", () => "MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent +=");
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += h;
        }
        public void SubReceivedReward(Action<string, MaxSdkBase.Reward, MaxSdkBase.AdInfo> h)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "Sub.Reward", () => "MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent +=");
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += h;
        }

        public void RequestAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "SDK.Load", () => $"id={id}");
            MaxSdk.LoadRewardedAd(id);
        }

        public bool GetAdReady(string id) => MaxSdk.IsRewardedAdReady(id);

        public void Show(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_rw, "SDK.Show", () => $"id={id}");
            MaxSdk.ShowRewardedAd(id);
        }
    }
}