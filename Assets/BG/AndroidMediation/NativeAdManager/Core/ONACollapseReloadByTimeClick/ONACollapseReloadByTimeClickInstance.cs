using UnityEngine;

public class ONACollapseReloadByTimeClickInstance
{
    private static int counter = 0;
    private string alias;
    private ONACollapseReloadByTimeClickCallback callback;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass bridgeClass;
    private static AndroidJavaObject unityActivity;

    static ONACollapseReloadByTimeClickInstance()
    {
        bridgeClass = new AndroidJavaClass("com.blackgems.aar.api.bridge.AdsPublicApi");
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
#endif

    public string Alias => alias;

    public event System.Action<NativeAdInfo> OnCollapseReloadLoaded
    {
        add => callback.OnONACollapseReloadLoaded += value;
        remove => callback.OnONACollapseReloadLoaded -= value;
    }

    public event System.Action<NativeAdInfo> OnCollapseReloadDisplayed
    {
        add => callback.OnONACollapseReloadDisplayed += value;
        remove => callback.OnONACollapseReloadDisplayed -= value;
    }

    public event System.Action<NativeAdInfo> OnCollapseReloadClosed
    {
        add => callback.OnONACollapseReloadClosed += value;
        remove => callback.OnONACollapseReloadClosed -= value;
    }

    public event System.Action<NativeAdInfo> OnAdClickCloseButton
    {
        add => callback.OnONACollapseReloadClickCloseButton += value;
        remove => callback.OnONACollapseReloadClickCloseButton -= value;
    }

    public event System.Action<NativeAdInfo, NativeAdPaidInfo> OnCollapseReloadPaidImpression
    {
        add => callback.OnONACollapseReloadPaidImpression += value;
        remove => callback.OnONACollapseReloadPaidImpression -= value;
    }

    public event System.Action OnCollapseReloadDisplayable
    {
        add => callback.OnONACollapseReloadDisplayable += value;
        remove => callback.OnONACollapseReloadDisplayable -= value;
    }

    public event System.Action<NativeAdInfo> OnCollapseReloadOpened
    {
        add => callback.OnONACollapseReloadOpened += value;
        remove => callback.OnONACollapseReloadOpened -= value;
    }

    public event System.Action<NativeAdInfo> OnCollapseReloadClicked
    {
        add => callback.OnONACollapseReloadClicked += value;
        remove => callback.OnONACollapseReloadClicked -= value;
    }

    public event System.Action<string, int, string> OnCollapseReloadFailedToLoad
    {
        add => callback.OnONACollapseReloadFailedToLoad += value;
        remove => callback.OnONACollapseReloadFailedToLoad -= value;
    }

    public ONACollapseReloadByTimeClickInstance(AndroidNAConfig config)
    {
        alias = "collapse_reload_" + (++counter);
        callback = new ONACollapseReloadByTimeClickCallback();

#if UNITY_ANDROID && !UNITY_EDITOR
        string jsonConfig = JsonUtility.ToJson(config);
        bool created = bridgeClass.CallStatic<bool>("create", unityActivity, "COLLAPSE", alias, jsonConfig);
        bool callbackSet = bridgeClass.CallStatic<bool>("setCollapseNativeCallback", unityActivity, alias, callback);
        Debug.Log($"[ONACollapseReloadInstance] create={created} setCallback={callbackSet} alias={alias} config={jsonConfig}");
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

    public string GetLatestNativeMediationAdapter()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return bridgeClass.CallStatic<string>("getLatestNativeMediationAdapter", alias);
#else
        return string.Empty;
#endif
    }

    public string LatestNativeMediationAdapter => GetLatestNativeMediationAdapter();

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
        Debug.Log($"[ONACollapseReloadInstance] preloadAll={result} alias={alias}");
#endif
    }

    public void ShowCollapseReloadAuto(string layoutName, int timeCloseSeconds, int timeReloadSeconds, bool enableReload = true, int thresholdOrClick = 1)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string lowerLayout = layoutName == null ? string.Empty : layoutName.ToLowerInvariant();
        string mode = lowerLayout.Contains("_45") ? "HEIGHT_45" :
            lowerLayout.Contains("_25") ? "HEIGHT_25" : "HEIGHT_35";
        ShowCollapseReload(layoutName, mode, timeCloseSeconds, timeReloadSeconds, enableReload, thresholdOrClick);
#endif
    }

    public void ShowCollapseReload45(string layoutName, int timeCloseSeconds, int timeReloadSeconds, bool enableReload = true, int timeClickToReload = 1)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        ShowCollapseReload(layoutName, "HEIGHT_45", timeCloseSeconds, timeReloadSeconds, enableReload, timeClickToReload);
#endif
    }

    public void ShowCollapseReload35(string layoutName, int timeCloseSeconds, int timeReloadSeconds, bool enableReload = true, int closeThreshold = 1)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        ShowCollapseReload(layoutName, "HEIGHT_35", timeCloseSeconds, timeReloadSeconds, enableReload, closeThreshold);
#endif
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
        bridgeClass.CallStatic<bool>("stopCollapse", alias);
#endif
    }

    public void Clear()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("destroy", alias);
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void ShowCollapseReload(string layoutName, string mode, int timeCloseSeconds, int timeReloadSeconds, bool enableReload, int threshold)
    {
        string showOptionsJson = JsonUtility.ToJson(new CollapseReloadShowOptions(layoutName, mode, timeCloseSeconds, timeReloadSeconds, enableReload, threshold));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[ONACollapseReloadInstance] show={result} alias={alias} options={showOptionsJson}");
    }
#endif

    [System.Serializable]
    private class CollapseReloadShowOptions
    {
        public string layoutName;
        public string mode;
        public int timeClose;
        public int timeReload;
        public bool enableReload;
        public int threshold;

        public CollapseReloadShowOptions(string layoutName, string mode, int timeClose, int timeReload, bool enableReload, int threshold)
        {
            this.layoutName = layoutName;
            this.mode = mode;
            this.timeClose = timeClose;
            this.timeReload = timeReload;
            this.enableReload = enableReload;
            this.threshold = threshold;
        }
    }
}
