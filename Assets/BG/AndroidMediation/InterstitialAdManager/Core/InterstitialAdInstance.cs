using System;
using GoogleMobileAds.Api;
using UnityEngine;

public class InterstitialAdInstance
{
    private static int counter;
    private readonly string alias;
    private readonly InterstitialCallback callback;
    private readonly int preloadBufferSize;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass bridgeClass;
    private static AndroidJavaObject unityActivity;

    static InterstitialAdInstance()
    {
        bridgeClass = new AndroidJavaClass("com.blackgems.aar.api.bridge.AdsPublicApi");
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
#endif

    public string Alias => alias;

    public event Action<InterstitialInfo> OnInterstitialLoaded
    {
        add => callback.OnInterstitialLoaded += value;
        remove => callback.OnInterstitialLoaded -= value;
    }

    public event Action<InterstitialInfo> OnInterstitialDisplayed
    {
        add => callback.OnInterstitialDisplayed += value;
        remove => callback.OnInterstitialDisplayed -= value;
    }

    public event Action<InterstitialInfo> OnInterstitialOpened
    {
        add => callback.OnInterstitialOpened += value;
        remove => callback.OnInterstitialOpened -= value;
    }

    public event Action<InterstitialInfo> OnInterstitialClosed
    {
        add => callback.OnInterstitialClosed += value;
        remove => callback.OnInterstitialClosed -= value;
    }

    public event Action<InterstitialInfo> OnInterstitialClicked
    {
        add => callback.OnInterstitialClicked += value;
        remove => callback.OnInterstitialClicked -= value;
    }

    public event Action<string, int, string> OnInterstitialFailedToLoad
    {
        add => callback.OnInterstitialFailedToLoad += value;
        remove => callback.OnInterstitialFailedToLoad -= value;
    }

    public event Action<InterstitialInfo, InterstitialPaidInfo> OnInterstitialPaidImpression
    {
        add => callback.OnInterstitialPaidImpression += value;
        remove => callback.OnInterstitialPaidImpression -= value;
    }

    public event Action OnInterstitialDisplayable
    {
        add => callback.OnInterstitialDisplayable += value;
        remove => callback.OnInterstitialDisplayable -= value;
    }

    public event Action<LoadAdError> OnAdLoadFailedCompat
    {
        add => callback.OnAdLoadFailedCompat += value;
        remove => callback.OnAdLoadFailedCompat -= value;
    }

    public event Action<AdValue> OnAdPaid
    {
        add => callback.OnAdPaid += value;
        remove => callback.OnAdPaid -= value;
    }

    public event Action OnAdClicked
    {
        add => callback.OnAdClicked += value;
        remove => callback.OnAdClicked -= value;
    }

    public event Action OnAdFullScreenContentOpened
    {
        add => callback.OnAdFullScreenContentOpened += value;
        remove => callback.OnAdFullScreenContentOpened -= value;
    }

    public event Action OnAdFullScreenContentClosed
    {
        add => callback.OnAdFullScreenContentClosed += value;
        remove => callback.OnAdFullScreenContentClosed -= value;
    }

    public event Action<AdError> OnAdFullScreenContentFailed
    {
        add => callback.OnAdFullScreenContentFailed += value;
        remove => callback.OnAdFullScreenContentFailed -= value;
    }

    public InterstitialAdInstance(InterstitialConfig config)
    {
        alias = "interstitial_" + (++counter);
        callback = new InterstitialCallback();
        preloadBufferSize = Mathf.Max(1, config?.PreloadBufferSize ?? 1);

#if UNITY_ANDROID && !UNITY_EDITOR
        var jsonConfig = JsonUtility.ToJson(config ?? new InterstitialConfig(Array.Empty<string>()));
        var created = bridgeClass.CallStatic<bool>("createInterstitial", unityActivity, alias, jsonConfig);
        var callbackSet = bridgeClass.CallStatic<bool>("setInterstitialCallback", unityActivity, alias, callback);
        Debug.Log($"[InterstitialAdInstance] create={created} setCallback={callbackSet} alias={alias} config={jsonConfig}");
#endif
    }

    public bool IsReady()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return bridgeClass.CallStatic<bool>("isReady", alias);
#else
        return false;
#endif
    }

    public void Load()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var result = bridgeClass.CallStatic<bool>("loadInterstitial", unityActivity, alias, preloadBufferSize);
        Debug.Log($"[InterstitialAdInstance] load={result} alias={alias} preloadBufferSize={preloadBufferSize}");
#endif
    }

    public void Load(int customPreloadBufferSize)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var resolvedBuffer = Mathf.Max(1, customPreloadBufferSize);
        var result = bridgeClass.CallStatic<bool>("loadInterstitial", unityActivity, alias, resolvedBuffer);
        Debug.Log($"[InterstitialAdInstance] load={result} alias={alias} preloadBufferSize={resolvedBuffer}");
#endif
    }

    public void Show(bool immersiveMode = false)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var showOptionsJson = JsonUtility.ToJson(new InterstitialShowOptionsConfig(immersiveMode));
        var result = bridgeClass.CallStatic<bool>("showInterstitial", unityActivity, alias, showOptionsJson);
        Debug.Log($"[InterstitialAdInstance] show={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void Clear()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("destroy", alias);
#endif
    }

    [Serializable]
    public class InterstitialConfig
    {
        [SerializeField] private string[] ids;
        [SerializeField] private bool autoReload;
        [SerializeField] private int preloadBufferSize;

        public int PreloadBufferSize => preloadBufferSize;

        public InterstitialConfig(string[] ids, bool autoReload = true, int preloadBufferSize = 1)
        {
            this.ids = ids ?? Array.Empty<string>();
            this.autoReload = autoReload;
            this.preloadBufferSize = Mathf.Max(1, preloadBufferSize);
        }
    }

    [Serializable]
    private class InterstitialShowOptionsConfig
    {
        [SerializeField] private bool immersiveMode;

        public InterstitialShowOptionsConfig(bool immersiveMode)
        {
            this.immersiveMode = immersiveMode;
        }
    }
}
