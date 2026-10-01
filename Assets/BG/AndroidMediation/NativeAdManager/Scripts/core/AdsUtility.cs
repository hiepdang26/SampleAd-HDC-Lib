using UnityEngine;

public static class AdsUtility
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass bridgeClass;

    static AdsUtility()
    {
        bridgeClass = new AndroidJavaClass("com.blackgems.aar.api.bridge.AdsPublicApi");
    }
#endif

    public static void SetEnvironment(string environment)
    {
        Debug.Log($"[AdsUtility][UnityWrapper] SetEnvironment({environment})");
#if UNITY_ANDROID && !UNITY_EDITOR
        bridgeClass.CallStatic("setEnvironment", environment);
#endif
    }

    public static void InitAds()
    {
        Debug.Log("[AdsUtility][UnityWrapper] InitAds()");
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            bridgeClass.CallStatic("initAds", activity);
        }
#endif
    }

    public static void OpenAdInspector()
    {
        Debug.Log("[AdsUtility][UnityWrapper] OpenAdInspector()");
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            bridgeClass.CallStatic("openAdInspector", activity);
        }
#endif
    }

    public static void StartInternetChecking()
    {
        Debug.Log("[AdsUtility][UnityWrapper] StartInternetChecking()");
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            bridgeClass.CallStatic("startInternetChecking", activity);
        }
#endif
    }

    public static void ShowLanguageIntroAds(string alias, string orientation, string rcJson)
    {
        Debug.Log($"[AdsUtility][UnityWrapper] ShowLanguageIntroAds(alias={alias}, orientation={orientation})");
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            bridgeClass.CallStatic("showLanguageIntroAds", activity, alias, orientation, rcJson);
        }
#endif
    }

    public static void OpenDebugPanel()
    {
        Debug.Log("[AdsUtility][UnityWrapper] OpenDebugPanel()");
#if UNITY_ANDROID && !UNITY_EDITOR
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            bridgeClass.CallStatic("openDebugPanel", activity);
        }
#endif
    }
}
