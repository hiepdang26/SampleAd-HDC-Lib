using System;
using System.Collections.Generic;
using UnityEngine;

public class BNNAInstance
{
    private static int counter = 0;
    private string alias;
    private BNNACallback callback;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass bridgeClass;
    private static AndroidJavaObject unityActivity;

    static BNNAInstance()
    {
        bridgeClass = new AndroidJavaClass("com.blackgems.aar.api.bridge.AdsPublicApi");
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
#endif

    public string Alias => alias;

    public event System.Action<NativeAdInfo> OnBannerLoaded
    {
        add => callback.OnBNNALoaded += value;
        remove => callback.OnBNNALoaded -= value;
    }

    public event System.Action<NativeAdInfo> OnBannerDisplayed
    {
        add => callback.OnBNNADisplayed += value;
        remove => callback.OnBNNADisplayed -= value;
    }

    public event System.Action<NativeAdInfo> OnBannerClicked
    {
        add => callback.OnBNNAClicked += value;
        remove => callback.OnBNNAClicked -= value;
    }

    public event System.Action<NativeAdInfo> OnBannerClosed
    {
        add => callback.OnBNNAClosed += value;
        remove => callback.OnBNNAClosed -= value;
    }

    public event System.Action<string, int, string> OnBannerFailedToLoad
    {
        add => callback.OnBNNAFailedToLoad += value;
        remove => callback.OnBNNAFailedToLoad -= value;
    }

    public event System.Action<NativeAdInfo, NativeAdPaidInfo> OnBannerPaidImpression
    {
        add => callback.OnBNNAPaidImpression += value;
        remove => callback.OnBNNAPaidImpression -= value;
    }

    public event System.Action OnBannerDisplayable
    {
        add => callback.OnBNNADisplayable += value;
        remove => callback.OnBNNADisplayable -= value;
    }

    public event System.Action OnBannerCollapsed
    {
        add { }
        remove { }
    }

    public event System.Action<NativeAdInfo> OnBannerOpened
    {
        add => callback.OnBNNAOpened += value;
        remove => callback.OnBNNAOpened -= value;
    }

    public BNNAInstance(AndroidNAConfig config)
    {
        alias = "banner_" + (++counter);
        callback = new BNNACallback();

#if UNITY_ANDROID && !UNITY_EDITOR
        string jsonConfig = JsonUtility.ToJson(config);
        bool created = bridgeClass.CallStatic<bool>("create", unityActivity, "BANNER", alias, jsonConfig);
        bool callbackSet = bridgeClass.CallStatic<bool>("setBannerNativeCallback", unityActivity, alias, callback);
        Debug.Log($"[BNNAInstance] create={created} setCallback={callbackSet} alias={alias} config={jsonConfig}");
#endif
    }

    public BNNAInstance(string[] ids)
        : this(new AndroidNAConfig(ids ?? Array.Empty<string>()))
    {
    }

    public bool IsReady()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return bridgeClass.CallStatic<bool>("isReady", alias);
#else
        return false;
#endif
    }

    public string GetLatestNativeMediationAdapter()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return bridgeClass.CallStatic<string>("getLatestNativeMediationAdapter", alias);
#else
        return string.Empty;
#endif
    }

    public string LatestNativeMediationAdapter => GetLatestNativeMediationAdapter();

    public void LoadAd()
    {
        PreloadAll();
    }

    public void LoadAd(string adUnitId)
    {
        if (string.IsNullOrWhiteSpace(adUnitId))
        {
            PreloadAll();
            return;
        }

        PreloadOne(adUnitId);
    }

    public void PreloadOne(string adUnitId)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("preloadOne", unityActivity, alias, adUnitId);
#endif
    }

    public void PreloadAll()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool result = bridgeClass.CallStatic<bool>("preloadAll", unityActivity, alias);
        Debug.Log($"[BNNAInstance] preloadAll={result} alias={alias}");
#endif
    }

    public void ShowAd(string[] layoutNames, int timeReloadSeconds, int timeCountdownSeconds = 5)
    {
        Show(layoutNames, timeReloadSeconds, timeCountdownSeconds);
    }

    public void Show(string[] layoutNames, int timeReloadSeconds, int timeCountdownSeconds = 5)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string showOptionsJson = JsonUtility.ToJson(new BannerShowOptions(
            NormalizeLayoutNames(layoutNames),
            timeReloadSeconds,
            timeCountdownSeconds));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[BNNAInstance] show={result} alias={alias} options={showOptionsJson}");
#endif
    }
    
    public bool ExpandAd(bool enableClick = true)
    {
        return Expand(enableClick);
    }

    public bool Expand(bool enableClick = true)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool result = bridgeClass.CallStatic<bool>("expandBanner", unityActivity, alias, enableClick);
        Debug.Log($"[BNNAInstance] expandBanner={result} alias={alias} enableClick={enableClick}");
        return result;
#else
        Debug.Log($"[BNNAInstance] expandBanner skipped outside Android player. enableClick={enableClick}");
        return false;
#endif
    }

    public void HideAd()
    {
        Hide();
    }

    public void Hide()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("hide", unityActivity, alias);
#endif
    }

    public void Stop()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("stopBanner", alias);
#endif
    }

    public void StopAd()
    {
        Stop();
    }

    public void DestroyAd()
    {
        Clear();
    }

    public void Clear()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("destroy", alias);
#endif
    }

    private static string[] NormalizeLayoutNames(string[] layoutNames)
    {
        if (layoutNames == null || layoutNames.Length == 0)
        {
            return Array.Empty<string>();
        }

        var normalized = new List<string>(layoutNames.Length);
        foreach (var candidate in layoutNames)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var trimmed = candidate.Trim();
            if (!normalized.Contains(trimmed))
            {
                normalized.Add(trimmed);
            }
        }

        return normalized.ToArray();
    }

    [System.Serializable]
    private class BannerShowOptions
    {
        public string[] layoutNames;
        public int timeReload;
        public int timeCountdown;

        public BannerShowOptions(string[] layoutNames, int timeReload, int timeCountdown = 5)
        {
            this.layoutNames = layoutNames;
            this.timeReload = timeReload;
            this.timeCountdown = Math.Max(0, timeCountdown);
        }
    }
}
