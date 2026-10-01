using UnityEngine;

namespace CountryRegionCheck
{
    public static class AndroidDeviceIdUtils
    {
        /// <summary>
        /// Lấy Settings.Secure.ANDROID_ID của thiết bị.
        /// Android 8+ : giá trị ổn định theo (app signing key + user + device), không cần quyền.
        /// Lưu ý: build debug và build release ký bằng key khác nhau -> ANDROID_ID khác nhau.
        /// Trả về "" nếu không lấy được hoặc đang chạy ngoài Android (Editor dùng fallback).
        /// </summary>
        public static string GetAndroidId()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (AndroidJavaObject contentResolver = context.Call<AndroidJavaObject>("getContentResolver"))
                using (AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure"))
                {
                    // "android_id" chính là hằng Settings.Secure.ANDROID_ID
                    string androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");
                    return string.IsNullOrEmpty(androidId) ? "" : androidId;
                }
            }
            catch
            {
                return "";
            }
#else
            // Editor / nền tảng khác: fallback để test cho tiện
            return SystemInfo.deviceUniqueIdentifier;
#endif
        }
    }
}
