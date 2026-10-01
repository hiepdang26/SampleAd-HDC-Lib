namespace BG_Library.NET.AndroidSDK
{
    using System;
    using System.Threading;
    using UnityEngine;

    /// <summary>
    /// Bridge cho bg.blackgems.nextgen.InterstitialAdSnippets (Google Ads next-gen SDK).
    ///
    /// Mỗi instance quản lý MỘT ad unit id — tạo nhiều instance để show nhiều id:
    ///   var gameOverAd = new InterstitialAdManager("ca-app-pub-xxx/1111");
    ///   var shopAd     = new InterstitialAdManager("ca-app-pub-xxx/2222");
    ///
    /// Lưu ý: mỗi ad unit id chỉ tạo MỘT instance. InterstitialAdPreloader phía Java là static,
    /// key theo id — instance thứ hai cùng id sẽ không đăng ký được callback (start() bị bỏ qua).
    ///
    /// Callback từ Java đến trên thread của Android:
    /// - dispatchToMainThread = true (mặc định): event được Post về Unity main thread,
    ///   an toàn để đụng UnityEngine API (UI, GameObject, PlayerPrefs...).
    /// - dispatchToMainThread = false: event bắn ngay trên thread Android — dùng khi callback
    ///   chỉ để log analytics (Firebase...), không đụng UnityEngine API.
    ///
    /// Constructor phải được gọi từ Unity main thread (Awake/Start) để bắt được SynchronizationContext.
    /// </summary>
    public class InterstitialAdPreload : IDisposable
    {
        private const string JavaClass = "bg.blackgems.interstitials.nextgen.InterstitialAdSnippets";
        private const string JavaCallbackInterface = "bg.blackgems.interstitials.nextgen.AdCallback";
        private const string TAG = "InterstitialAdPreload";

        public string AdUnitId { get; }

        // ---------- Preload events ----------
        /// <summary>(preloadId, responseId) — có ad sẵn trong buffer.</summary>
        public event Action<string, string> AdPreloaded;

        /// <summary>(preloadId, errorCode, errorMessage)</summary>
        public event Action<string, string, string> AdFailedToPreload;

        /// <summary>(preloadId) — buffer đã cạn.</summary>
        public event Action<string> AdsExhausted;

        // ---------- Fullscreen events ----------
        public event Action AdShowed;
        public event Action AdDismissed;

        /// <summary>(errorCode, errorMessage)</summary>
        public event Action<string, string> AdFailedToShow;

        public event Action AdImpression;
        public event Action AdClicked;

        /// <summary>(valueMicros, currencyCode) — doanh thu ad.</summary>
        public event Action<long, string> AdPaid;

        private readonly bool dispatchToMainThread;
        private readonly SynchronizationContext unityContext;
        private readonly AndroidJavaObject activity;
        private readonly AndroidJavaObject snippets;
        private readonly AdCallbackProxy callbackProxy;

        public InterstitialAdPreload(string adUnitId, bool dispatchToMainThread = true)
        {
            AdUnitId = adUnitId;
            this.dispatchToMainThread = dispatchToMainThread;
            unityContext = SynchronizationContext.Current;

            if (Application.platform != RuntimePlatform.Android)
            {
                LogWarring($"[InterstitialAdManager] {adUnitId}: chỉ hoạt động trên thiết bị Android.");
                return;
            }

            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            }

            // Gọi constructor Java: InterstitialAdSnippets(String adUnitId, AdCallback adCallback)
            callbackProxy = new AdCallbackProxy(this);
            snippets = new AndroidJavaObject(JavaClass, adUnitId, callbackProxy);
        }

        // ================== Public API ==================

        /// <summary>Bắt đầu preload với buffer size mặc định của SDK. Gọi Java: loadAd().</summary>
        public void LoadAd()
        {
            snippets?.Call("loadAd");
        }

        /// <summary>Bắt đầu preload, giữ sẵn tối đa bufferSize ad. Gọi Java: preloadAd(int).</summary>
        public void PreloadAd(int bufferSize)
        {
            snippets?.Call("preloadAd", bufferSize);
        }

        /// <summary>
        /// Lấy ad sẵn có trong buffer và show ngay. Gọi Java: pollAndShowAd(Activity, String).
        /// Trả về false nếu buffer chưa có ad — trường hợp này sẽ không có callback nào bắn ra.
        /// </summary>
        public bool ShowAd()
        {
            if (snippets == null) return false;
            return snippets.Call<bool>("pollAndShowAd", activity);
        }

        /// <summary>Kiểm tra buffer có ad sẵn không. Gọi Java: isAdAvailable(String).</summary>
        public bool IsAdAvailable()
        {
            if (snippets == null) return false;
            return snippets.Call<bool>("isAdAvailable");
        }

        /// <summary>Xem responseId của ad kế tiếp mà không tiêu thụ nó. Gọi Java: peekAdResponseInfo(String).</summary>
        public string PeekAdResponseInfo()
        {
            if (snippets == null) return string.Empty;
            return snippets.Call<string>("peekAdResponseInfo");
        }

        public void DestroyAd()
        {
            if (snippets == null) return;
            snippets.Call("destroyAd");
        }

        public void Dispose()
        {
            DestroyAd();
            snippets?.Dispose();
            activity?.Dispose();
        }

        // ================== Dispatch ==================

        /// <summary>
        /// Đưa event về nơi cần chạy. Luôn bọc try/catch: exception lọt ra khỏi AndroidJavaProxy
        /// sẽ propagate ngược vào Java và có thể crash app.
        /// </summary>
        private void Emit(Action action)
        {
            if (action == null) return;

            if (dispatchToMainThread && unityContext != null)
            {
                unityContext.Post(_ => SafeInvoke(action), null);
            }
            else
            {
                SafeInvoke(action);
            }
        }

        private static void SafeInvoke(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void Log(string message)
        {
            UnityEngine.Debug.Log($"[{TAG}] {message}");
        }
        
        public void LogError(string message)
        {
            UnityEngine.Debug.LogError($"[{TAG}] {message}");
        }
        
        public void LogWarring(string message)
        {
            UnityEngine.Debug.LogWarning($"[{TAG}] {message}");
        }

        // ================== AndroidJavaProxy ==================

        /// <summary>
        /// Implement interface Java bg.blackgems.nextgen.AdCallback.
        /// Tên và chữ ký method phải trùng khớp tuyệt đối với interface Java.
        /// Các method này được Java gọi từ thread của Android.
        /// </summary>
        private sealed class AdCallbackProxy : AndroidJavaProxy
        {
            private readonly InterstitialAdPreload owner;

            public AdCallbackProxy(InterstitialAdPreload owner) : base(JavaCallbackInterface)
            {
                this.owner = owner;
            }

            // --- Preload callbacks ---

            public void onAdPreloaded(string preloadId, string responseId)
                => owner.AdPreloaded?.Invoke(preloadId, responseId);

            public void onAdFailedToPreload(string preloadId, string errorCode, string errorMessage)
                => owner.AdFailedToPreload?.Invoke(preloadId, errorCode, errorMessage);

            public void onAdsExhausted(string preloadId)
                => owner.AdsExhausted?.Invoke(preloadId);

            // --- Fullscreen callbacks ---

            public void onAdShowedFullScreenContent()
                => owner.AdShowed?.Invoke();

            public void onAdDismissedFullScreenContent()
                => owner.AdDismissed?.Invoke();

            public void onAdFailedToShowFullScreenContent(string errorCode, string errorMessage)
                => owner.AdFailedToShow?.Invoke(errorCode, errorMessage);

            public void onAdImpression()
                => owner.AdImpression?.Invoke();

            public void onAdClicked()
                => owner.AdClicked?.Invoke();

            public void onAdPaid(long valueMicros, string currencyCode)
                => owner.AdPaid?.Invoke(valueMicros, currencyCode);
        }
    }
}