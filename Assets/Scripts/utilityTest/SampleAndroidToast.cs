using UnityEngine;

namespace BG_Lib.AndroidMediation.Scripts.sample
{
    public static class SampleAndroidToast
    {
        private const string LogTag = "SampleAndroidToast";

        public static void Show(string message, bool longDuration = false)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (activity == null)
                    {
                        Debug.Log($"[{LogTag}] {message}");
                        return;
                    }

                    activity.Call(
                        "runOnUiThread",
                        new AndroidJavaRunnable(() =>
                        {
                            using (var toastClass = new AndroidJavaClass("android.widget.Toast"))
                            {
                                var duration = toastClass.GetStatic<int>(longDuration ? "LENGTH_LONG" : "LENGTH_SHORT");
                                using (var toast = toastClass.CallStatic<AndroidJavaObject>("makeText", activity, message, duration))
                                {
                                    toast.Call("show");
                                }
                            }
                        }));
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"[{LogTag}] Failed to show Android toast: {exception.Message}. message={message}");
            }
#else
            Debug.Log($"[{LogTag}] {message}");
#endif
        }
    }
}
