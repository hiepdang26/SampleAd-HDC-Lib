using UnityEngine;

public class ONAPopupInstance
{
    private static int counter = 0;
    private string alias;
    private ONAPopupCallback callback;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass bridgeClass;
    private static AndroidJavaObject unityActivity;

    static ONAPopupInstance()
    {
        bridgeClass = new AndroidJavaClass("com.blackgems.aar.api.bridge.AdsPublicApi");
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
#endif

    public string Alias => alias;

    public event System.Action<NativeAdInfo> OnONAPopupLoaded
    {
        add => callback.OnONAPopupLoaded += value;
        remove => callback.OnONAPopupLoaded -= value;
    }

    public event System.Action<NativeAdInfo> OnONAPopupDisplayed
    {
        add => callback.OnONAPopupDisplayed += value;
        remove => callback.OnONAPopupDisplayed -= value;
    }

    public event System.Action<NativeAdInfo> OnONAPopupClosed
    {
        add => callback.OnONAPopupClosed += value;
        remove => callback.OnONAPopupClosed -= value;
    }

    public event System.Action<NativeAdInfo> OnONAPopupClicked
    {
        add => callback.OnONAPopupClicked += value;
        remove => callback.OnONAPopupClicked -= value;
    }

    public event System.Action<string, int, string> OnONAPopupFailedToload
    {
        add => callback.OnONAPopupFailedToLoad += value;
        remove => callback.OnONAPopupFailedToLoad -= value;
    }

    public event System.Action<NativeAdInfo, NativeAdPaidInfo> OnONAPopupPaidImpression
    {
        add => callback.OnONAPopupPaidImpression += value;
        remove => callback.OnONAPopupPaidImpression -= value;
    }

    public event System.Action OnONAPopupDisplayable
    {
        add => callback.OnONAPopupDisplayable += value;
        remove => callback.OnONAPopupDisplayable -= value;
    }

    public event System.Action<NativeAdInfo> OnONAPopupOpened
    {
        add => callback.OnONAPopupOpened += value;
        remove => callback.OnONAPopupOpened -= value;
    }

    public ONAPopupInstance(AndroidNAConfig config)
    {
        alias = "popup_" + (++counter);
        callback = new ONAPopupCallback();

#if UNITY_ANDROID && !UNITY_EDITOR
        string jsonConfig = JsonUtility.ToJson(config);
        bool created = bridgeClass.CallStatic<bool>("create", unityActivity, "POPUP", alias, jsonConfig);
        bool callbackSet = bridgeClass.CallStatic<bool>("setPopupNativeCallback", unityActivity, alias, callback);
        Debug.Log($"[ONAPopupInstance] create={created} setCallback={callbackSet} alias={alias} config={jsonConfig}");
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
        Debug.Log($"[ONAPopupInstance] preloadAll={result} alias={alias}");
#endif
    }

    public void ShowManualClose(string layoutName, int timeShowSeconds, int timeReload, float xDp, float yDp, float widthDp, float heightDp)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Show(layoutName, timeShowSeconds, timeReload, xDp, yDp, widthDp, heightDp, false, false, "showManual");
#endif
    }

    public void ShowAutoClose(string layoutName, int timeShowSeconds, int timeReload, float xDp, float yDp, float widthDp, float heightDp)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Show(layoutName, timeShowSeconds, timeReload, xDp, yDp, widthDp, heightDp, true, false, "showAuto");
#endif
    }

    public void Hide()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("hide", unityActivity, alias);
#endif
    }

    public void Close()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("closePopup", alias);
#endif
    }

    public void Clear()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("destroy", alias);
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private void Show(
        string layoutName,
        int timeShowSeconds,
        int timeReload,
        float xDp,
        float yDp,
        float widthDp,
        float heightDp,
        bool autoClose,
        bool enableCtrOverlay,
        string logLabel)
    {
        string showOptionsJson = JsonUtility.ToJson(
            new PopupShowOptions(layoutName, timeShowSeconds, timeReload, xDp, yDp, widthDp, heightDp, autoClose, enableCtrOverlay));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[ONAPopupInstance] {logLabel}={result} alias={alias} options={showOptionsJson}");
    }
#endif

    [System.Serializable]
    private class PopupShowOptions
    {
        public string layoutName;
        public long timeShow;
        public long timeReload;
        public float xDp;
        public float yDp;
        public float adWidthDp;
        public float adHeightDp;
        public bool autoClose;
        public bool enableCtrOverlay;

        public PopupShowOptions(
            string layoutName,
            long timeShow,
            long timeReload,
            float xDp,
            float yDp,
            float adWidthDp,
            float adHeightDp,
            bool autoClose,
            bool enableCtrOverlay)
        {
            this.layoutName = layoutName;
            this.timeShow = timeShow;
            this.timeReload = timeReload;
            this.xDp = xDp;
            this.yDp = yDp;
            this.adWidthDp = adWidthDp;
            this.adHeightDp = adHeightDp;
            this.autoClose = autoClose;
            this.enableCtrOverlay = enableCtrOverlay;
        }
    }
}

public class ONAPopupInstace : ONAPopupInstance
{
    public ONAPopupInstace(AndroidNAConfig config) : base(config)
    {
    }
}
