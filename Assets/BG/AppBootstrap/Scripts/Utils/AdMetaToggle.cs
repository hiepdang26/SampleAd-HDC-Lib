using System.Runtime.InteropServices;
using UnityEngine;

namespace AppBootstrap.Splash
{
    /// <summary>
    /// While this component is active, registers this device as a Meta Audience Network test device straight on
    /// the Meta SDK (FBAdSettings on iOS, AdSettings on Android), without going through shared.xcframework.
    /// Keep it in the first scene: Meta only serves test ads to requests made after it runs.
    /// Deactivate the GameObject or untick Enable Meta Test Mode before publishing.
    /// </summary>
    public class AdMetaToggle : MonoBehaviour
    {
        /// <summary>Raw FBAdTestAdType values (FBAudienceNetwork FBAdSettings.h).</summary>
        public enum TestAdType
        {
            Default = 0,
            Image16x9AppInstall = 1,
            Image16x9Link = 2,
            Image9x16AppInstall = 3,
            Image9x16Link = 4,
            Image1x1AppInstall = 5,
            Image1x1Link = 6,
            VideoHd16x9Long46sAppInstall = 7,
            VideoHd16x9Long46sLink = 8,
            VideoHd16x9Short15sAppInstall = 9,
            VideoHd16x9Short15sLink = 10,
            VideoHd9x16Long39sAppInstall = 11,
            VideoHd9x16Long39sLink = 12,
            CarouselImageSquareAppInstall = 13,
            CarouselImageSquareLink = 14,
            CarouselVideoSquareLink = 15,
            Playable = 16,
            RewardedVideo = 17
        }

        [Tooltip("On: register this device as a Meta test device. Off: clear Meta test devices left by an earlier run.")]
        [SerializeField] private bool _enableMetaTestMode = true;
        [Tooltip("Test creative Meta returns while test mode is on. Use image/video types for native ads.")]
        [SerializeField] private TestAdType _testAdType = TestAdType.Default;
        [Tooltip("Other Meta test device hashes. This device's hash is added automatically.")]
        [SerializeField] private string[] _extraDeviceHashes = System.Array.Empty<string>();
        [Tooltip("iOS: print Meta Audience Network verbose logs to the Xcode console.")]
        [SerializeField] private bool _verboseMetaLogs;

        private void Awake()
        {
            if (_enableMetaTestMode)
                Enable();
            else
                Disable();
        }

        [ContextMenu("Enable Meta Test Mode")]
        public void Enable()
        {
            if (!Debug.isDebugBuild)
                SplashLogger.Warn("[MetaTest] Meta test mode is ON in a release build: deactivate it before publishing");

            string extraHashes = string.Join(",", _extraDeviceHashes ?? System.Array.Empty<string>());
#if UNITY_IOS && !UNITY_EDITOR
            bool testMode = BGMetaTest_Enable(extraHashes, (int)_testAdType, _verboseMetaLogs ? 1 : 0) != 0;
            string deviceHash = BGMetaTest_DeviceHash();
            if (testMode)
                SplashLogger.Log($"[MetaTest] enabled deviceHash={deviceHash} testAdType={_testAdType}");
            else
                SplashLogger.Error($"[MetaTest] enable failed deviceHash={deviceHash}, see [MetaTest] native logs");
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var adSettings = new AndroidJavaClass("com.facebook.ads.AdSettings");
                adSettings.CallStatic("setTestMode", true);
                foreach (string hash in _extraDeviceHashes ?? System.Array.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(hash))
                        adSettings.CallStatic("addTestDevice", hash.Trim());
                }
                SplashLogger.Log($"[MetaTest] enabled extraDevices={extraHashes}");
            }
            catch (System.Exception exception)
            {
                SplashLogger.Error($"[MetaTest] enable failed: {exception.Message}");
            }
#else
            SplashLogger.Log($"[MetaTest] enable skipped in editor testAdType={_testAdType} extraDevices={extraHashes}");
#endif
        }

        [ContextMenu("Disable Meta Test Mode")]
        public void Disable()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (BGMetaTest_IsEnabled() != 0)
                BGMetaTest_Disable();
#elif UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var adSettings = new AndroidJavaClass("com.facebook.ads.AdSettings");
                adSettings.CallStatic("setTestMode", false);
                adSettings.CallStatic("clearTestDevices");
            }
            catch (System.Exception exception)
            {
                SplashLogger.Warn($"[MetaTest] disable failed: {exception.Message}");
            }
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int BGMetaTest_Enable(string extraDeviceHashesCsv, int testAdType, int verboseLogs);

        [DllImport("__Internal")]
        private static extern void BGMetaTest_Disable();

        [DllImport("__Internal")]
        private static extern int BGMetaTest_IsEnabled();

        [DllImport("__Internal")]
        private static extern string BGMetaTest_DeviceHash();
#endif
    }
}
