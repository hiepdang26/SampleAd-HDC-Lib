using System.Collections.Generic;
using UnityEngine;

public class AndroidFSNativeInstance
{
    private const string OverlayClsMode = "OVERLAY_CLS";
    private const string OverlayNavMode = "OVERLAY_NAV";

    private static int counter = 0;
    private string alias;
    private FSNACallback callback;

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass bridgeClass;
    private static AndroidJavaObject unityActivity;

    static AndroidFSNativeInstance()
    {
        bridgeClass = new AndroidJavaClass("com.blackgems.aar.api.bridge.AdsPublicApi");
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }
#endif

    public string Alias => alias;

    public event System.Action<NativeAdInfo> OnFSNALoaded
    {
        add => callback.OnFSNALoaded += value;
        remove => callback.OnFSNALoaded -= value;
    }

    public event System.Action<NativeAdInfo> OnFSNADisplayed
    {
        add => callback.OnFSNADisplayed += value;
        remove => callback.OnFSNADisplayed -= value;
    }

    public event System.Action<NativeAdInfo> OnFSNAClosed
    {
        add => callback.OnFSNAClosed += value;
        remove => callback.OnFSNAClosed -= value;
    }

    public event System.Action<NativeAdInfo> OnFSNAClicked
    {
        add => callback.OnFSNAClicked += value;
        remove => callback.OnFSNAClicked -= value;
    }

    public event System.Action<string, int, string> OnFSNAFailedToLoad
    {
        add => callback.OnFSNAFailedToLoad += value;
        remove => callback.OnFSNAFailedToLoad -= value;
    }

    public event System.Action<NativeAdInfo, NativeAdPaidInfo> OnFSNAPaidImpression
    {
        add => callback.OnFSNAPaidImpression += value;
        remove => callback.OnFSNAPaidImpression -= value;
    }

    public event System.Action OnFSNADisplayable
    {
        add => callback.OnFSNADisplayable += value;
        remove => callback.OnFSNADisplayable -= value;
    }

    public event System.Action<NativeAdInfo> OnFSNAOpened
    {
        add => callback.OnFSNAOpened += value;
        remove => callback.OnFSNAOpened -= value;
    }

    public AndroidFSNativeInstance(AndroidNAConfig config)
    {
        alias = "fullscreen_" + (++counter);
        callback = new FSNACallback();

#if UNITY_ANDROID && !UNITY_EDITOR
        string jsonConfig = JsonUtility.ToJson(config);
        bool created = bridgeClass.CallStatic<bool>("create", unityActivity, "FULLSCREEN", alias, jsonConfig);
        bool callbackSet = bridgeClass.CallStatic<bool>("setFullscreenNativeCallback", alias, callback);
        Debug.Log($"[FSNAInstance] create={created} setCallback={callbackSet} alias={alias} config={jsonConfig}");
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
        Debug.Log($"[FSNAInstance] preloadAll={result} alias={alias}");
#endif
    }

    public void ShowSingle(
        string[] layoutNames,
        int durationSeconds,
        string orientation = "auto",
        NativeAssetVisibilityOptions assetVisibility = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var normalizedLayouts = NormalizeLayoutNames(layoutNames);
        string showOptionsJson = JsonUtility.ToJson(new FullscreenShowOptions(
            mode: ResolveSingleMode(normalizedLayouts, useCtrMode: false),
            layoutNames: normalizedLayouts,
            duration: durationSeconds,
            orientation: orientation,
            pauseGameplay: false,
            enableAdComeback: true,
            assetVisibility: assetVisibility));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[FSNAInstance] showSingle={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void ShowSingleWithPauseOption(
        string[] layoutNames,
        int durationSeconds,
        bool enablePause,
        string orientation = "auto",
        bool enableAdComeback = true,
        float delay = 0f,
        int timeUpC = 0,
        NativeClickAssetOptions clickAssets = null,
        NativeAssetVisibilityOptions assetVisibility = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var normalizedLayouts = NormalizeLayoutNames(layoutNames);
        string showOptionsJson = JsonUtility.ToJson(new FullscreenShowOptions(
            mode: ResolveSingleMode(normalizedLayouts, useCtrMode: false),
            layoutNames: normalizedLayouts,
            duration: durationSeconds,
            orientation: orientation,
            pauseGameplay: enablePause,
            enableAdComeback: enableAdComeback,
            delay: delay,
            timeUpC: timeUpC,
            clickAssets: clickAssets,
            assetVisibility: assetVisibility));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[FSNAInstance] showSinglePause={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void ShowOverlayCls(
        string layoutName = "fs_single_cls_01",
        int durationSeconds = 3,
        string orientation = "auto",
        bool showTCD = true,
        bool pauseGameplay = false,
        bool enableAdComeback = true,
        float delay = 0f,
        int timeUpC = 0,
        NativeClickAssetOptions clickAssets = null,
        NativeAssetVisibilityOptions assetVisibility = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var normalizedLayouts = NormalizeLayoutNames(new[] { layoutName });
        string showOptionsJson = JsonUtility.ToJson(new FullscreenShowOptions(
            mode: OverlayClsMode,
            layoutNames: normalizedLayouts,
            duration: durationSeconds,
            orientation: orientation,
            pauseGameplay: pauseGameplay,
            enableAdComeback: enableAdComeback,
            showTCD: showTCD,
            delay: delay,
            timeUpC: timeUpC,
            clickAssets: clickAssets,
            assetVisibility: assetVisibility));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[FSNAInstance] showOverlayCls={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void ShowOverlayCls(
        string[] layoutNames,
        int durationSeconds = 3,
        string orientation = "auto",
        bool showTCD = true,
        bool pauseGameplay = false,
        bool enableAdComeback = true,
        float delay = 0f,
        int timeUpC = 0,
        NativeClickAssetOptions clickAssets = null,
        NativeAssetVisibilityOptions assetVisibility = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var normalizedLayouts = NormalizeLayoutNames(layoutNames);
        string showOptionsJson = JsonUtility.ToJson(new FullscreenShowOptions(
            mode: OverlayClsMode,
            layoutNames: normalizedLayouts,
            duration: durationSeconds,
            orientation: orientation,
            pauseGameplay: pauseGameplay,
            enableAdComeback: enableAdComeback,
            showTCD: showTCD,
            delay: delay,
            timeUpC: timeUpC,
            clickAssets: clickAssets,
            assetVisibility: assetVisibility));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[FSNAInstance] showOverlayCls={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void ShowOverlayNav(
        string layoutName = "fs_single_nav_01",
        int durationSeconds = 5,
        string orientation = "auto",
        bool showTCD = true,
        bool pauseGameplay = false,
        bool enableAdComeback = true,
        float delay = 0f,
        int timeUpC = 0,
        NativeClickAssetOptions clickAssets = null,
        NativeAssetVisibilityOptions assetVisibility = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var normalizedLayouts = NormalizeLayoutNames(new[] { layoutName });
        string showOptionsJson = JsonUtility.ToJson(new FullscreenShowOptions(
            mode: OverlayNavMode,
            layoutNames: normalizedLayouts,
            duration: durationSeconds,
            orientation: orientation,
            pauseGameplay: pauseGameplay,
            enableAdComeback: enableAdComeback,
            showTCD: showTCD,
            delay: delay,
            timeUpC: timeUpC,
            clickAssets: clickAssets,
            assetVisibility: assetVisibility));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[FSNAInstance] showOverlayNav={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void ShowOverlayNav(
        string[] layoutNames,
        int durationSeconds = 5,
        string orientation = "auto",
        bool showTCD = true,
        bool pauseGameplay = false,
        bool enableAdComeback = true,
        float delay = 0f,
        int timeUpC = 0,
        NativeClickAssetOptions clickAssets = null,
        NativeAssetVisibilityOptions assetVisibility = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var normalizedLayouts = NormalizeLayoutNames(layoutNames);
        string showOptionsJson = JsonUtility.ToJson(new FullscreenShowOptions(
            mode: OverlayNavMode,
            layoutNames: normalizedLayouts,
            duration: durationSeconds,
            orientation: orientation,
            pauseGameplay: pauseGameplay,
            enableAdComeback: enableAdComeback,
            showTCD: showTCD,
            delay: delay,
            timeUpC: timeUpC,
            clickAssets: clickAssets,
            assetVisibility: assetVisibility));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[FSNAInstance] showOverlayNav={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void ShowMultiple(
        string[] layoutNames,
        int durationSeconds,
        string orientation = "auto",
        bool pauseGameplay = false,
        bool enableAdComeback = true,
        NativeAssetVisibilityOptions assetVisibility = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var normalizedLayouts = NormalizeLayoutNamesKeepingDuplicates(layoutNames);
        string showOptionsJson = JsonUtility.ToJson(new FullscreenShowOptions(
            mode: "MULTIPLE",
            layoutNames: normalizedLayouts,
            duration: durationSeconds,
            durations: new int[0],
            orientation: orientation,
            pauseGameplay: pauseGameplay,
            enableAdComeback: enableAdComeback,
            assetVisibility: assetVisibility));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[FSNAInstance] showMultiple={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void ShowSequence(
        string[] layoutNames,
        int[] durationsSeconds,
        string orientation = "auto",
        bool pauseGameplay = false,
        bool enableAdComeback = true,
        NativeAssetVisibilityOptions assetVisibility = null)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        var normalizedLayouts = NormalizeLayoutNamesKeepingDuplicates(layoutNames);
        var normalizedDurations = NormalizeDurations(durationsSeconds);
        string showOptionsJson = JsonUtility.ToJson(new FullscreenShowOptions(
            mode: "SEQUENCE",
            layoutNames: normalizedLayouts,
            duration: normalizedDurations.Length > 0 ? normalizedDurations[0] : 0,
            durations: normalizedDurations,
            orientation: orientation,
            pauseGameplay: pauseGameplay,
            enableAdComeback: enableAdComeback,
            assetVisibility: assetVisibility));
        bool result = bridgeClass.CallStatic<bool>("show", unityActivity, alias, showOptionsJson);
        Debug.Log($"[FSNAInstance] showSequence={result} alias={alias} options={showOptionsJson}");
#endif
    }

    public void Clear()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic<bool>("destroy", alias);
#endif
    }

    private static string ResolveSingleMode(string[] layoutNames, bool useCtrMode)
    {
        var isOverlay = false;
        var isTransparent = false;

        if (layoutNames != null)
        {
            for (var i = 0; i < layoutNames.Length; i++)
            {
                var layoutName = layoutNames[i];
                if (string.IsNullOrWhiteSpace(layoutName))
                {
                    continue;
                }

                var normalizedLayout = layoutName.ToLowerInvariant();
                if (normalizedLayout.Contains("overlay"))
                {
                    isOverlay = true;
                }

                if (normalizedLayout.Contains("transparent"))
                {
                    isTransparent = true;
                }
            }
        }

        if (useCtrMode)
        {
            return isOverlay ? "OVERLAY_CTR" : "CTR";
        }

        if (isOverlay)
        {
            return isTransparent ? "OVERLAY_TRANSPARENT" : "OVERLAY";
        }

        return isTransparent ? "TRANSPARENT" : "SINGLE";
    }

    private static string[] NormalizeLayoutNames(string[] layoutNames)
    {
        if (layoutNames == null || layoutNames.Length == 0)
        {
            return new string[0];
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

    private static string[] NormalizeLayoutNamesKeepingDuplicates(string[] layoutNames)
    {
        if (layoutNames == null || layoutNames.Length == 0)
        {
            return new string[0];
        }

        var normalized = new List<string>(layoutNames.Length);
        foreach (var candidate in layoutNames)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            normalized.Add(candidate.Trim());
        }

        return normalized.ToArray();
    }

    private static int[] NormalizeDurations(int[] durationsSeconds)
    {
        if (durationsSeconds == null || durationsSeconds.Length == 0)
        {
            return new int[0];
        }

        var normalized = new List<int>(durationsSeconds.Length);
        for (var i = 0; i < durationsSeconds.Length; i++)
        {
            if (durationsSeconds[i] > 0)
            {
                normalized.Add(durationsSeconds[i]);
            }
        }

        return normalized.ToArray();
    }

    [System.Serializable]
    private class FullscreenShowOptions
    {
        public string mode;
        public string[] layoutNames;
        public int duration;
        public int[] durations;
        public string orientation;
        public bool pauseGameplay;
        public bool enableAdComeback;
        public bool showTCD;
        public float delay;
        public int timeUpC;
        public NativeClickAssetOptions clickAssets;
        public NativeAssetVisibilityOptions assetVisibility;

        public FullscreenShowOptions(
            string mode,
            string[] layoutNames,
            int duration,
            string orientation,
            bool pauseGameplay,
            bool enableAdComeback,
            bool showTCD = true,
            float delay = 0f,
            int timeUpC = 0,
            NativeClickAssetOptions clickAssets = null,
            NativeAssetVisibilityOptions assetVisibility = null)
            : this(mode, layoutNames, duration, new int[0], orientation, pauseGameplay, enableAdComeback, showTCD, delay, timeUpC, clickAssets, assetVisibility)
        {
        }

        public FullscreenShowOptions(
            string mode,
            string[] layoutNames,
            int duration,
            int[] durations,
            string orientation,
            bool pauseGameplay,
            bool enableAdComeback,
            bool showTCD = true,
            float delay = 0f,
            int timeUpC = 0,
            NativeClickAssetOptions clickAssets = null,
            NativeAssetVisibilityOptions assetVisibility = null)
        {
            this.mode = mode;
            this.layoutNames = layoutNames;
            this.duration = duration;
            this.durations = durations ?? new int[0];
            this.orientation = orientation;
            this.pauseGameplay = pauseGameplay;
            this.enableAdComeback = enableAdComeback;
            this.showTCD = showTCD;
            this.delay = delay;
            this.timeUpC = timeUpC;
            this.clickAssets = clickAssets;
            this.assetVisibility = assetVisibility;
        }
    }

}
