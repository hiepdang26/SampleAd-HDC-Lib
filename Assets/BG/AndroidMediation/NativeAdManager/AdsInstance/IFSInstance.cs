using System;

namespace BG_Library.NET.AndroidSDK
{
    public interface IFSInstance
    {
        event Action<AdInfo> OnAdLoadedEvent;
        event Action<string, int, string> OnAdLoadFailedEvent;
        event Action<AdInfo> OnAdDisplayedEvent;
        event Action<AdInfo> OnAdClicked;
        event Action<AdInfo, AdValue> OnPaidAdImpressionEvent;
        event Action<AdInfo> OnAdHiddenEvent;
        /*event Action<AdInfo> OnAdOpenedEvent;
        event Action OnAdDisplayableEvent;*/

        void LoadAd();   
        void ShowAd();
        void DestroyAd();
        bool IsReady();
    }
}