using UnityEngine;

namespace CountryRegionCheck
{
    public static class AndroidTelephonyUtils
    {
        public static string GetSimCountryIso()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (AndroidJavaObject telephonyManager = context.Call<AndroidJavaObject>("getSystemService", "phone"))
                {
                    if (telephonyManager == null) return "";
                    string simCountry = telephonyManager.Call<string>("getSimCountryIso");
                    return string.IsNullOrEmpty(simCountry) ? "" : simCountry.ToLowerInvariant();
                }
            }
            catch { return ""; }
#else
            return "";
#endif
        }

        public static string GetNetworkCountryIso()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (AndroidJavaObject telephonyManager = context.Call<AndroidJavaObject>("getSystemService", "phone"))
                {
                    if (telephonyManager == null) return "";
                    string networkCountry = telephonyManager.Call<string>("getNetworkCountryIso");
                    return string.IsNullOrEmpty(networkCountry) ? "" : networkCountry.ToLowerInvariant();
                }
            }
            catch { return ""; }
#else
            return "";
#endif
        }
    }
}