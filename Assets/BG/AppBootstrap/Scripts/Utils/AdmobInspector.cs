using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Mediation.Admob;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Splash
{
    /// <summary>
    /// While this GameObject is active, shows a faint button in the bottom-right corner; tapping it
    /// <see cref="_tapsToOpen"/> times opens the AdMob Ad Inspector (e.g. "Ad source testing" to force Meta).
    /// Each tap flashes the button. AdMob only opens the inspector on a test device, so opening it first
    /// registers this device as one. Deactivate the GameObject before publishing.
    /// </summary>
    public class AdmobInspector : MonoBehaviour
    {
        [Tooltip("Hide the button unless the build is a Development Build or AdMob Test Device is on in Configs SO.")]
        [SerializeField] private bool _onlyInTestBuilds;
        [Tooltip("Extra AdMob test device IDs, e.g. from the log line '<Google> To get test ads on this device, set: ...'. " +
                 "On iOS the ID is computed from the IDFA when tracking is allowed.")]
        [SerializeField] private string[] _testDeviceIds = System.Array.Empty<string>();
        [SerializeField, Min(1)] private int _tapsToOpen = 3;
        [Tooltip("The tap count restarts when taps are further apart than this.")]
        [SerializeField, Min(0.1f)] private float _tapTimeoutSeconds = 1f;
        [Tooltip("Button diameter and margin in dp (160 dpi units), kept inside the safe area.")]
        [SerializeField, Min(24f)] private float _buttonSizeDp = 56f;
        [SerializeField, Min(0f)] private float _marginDp = 16f;
        [SerializeField, Range(0f, 1f)] private float _idleAlpha = 0.3f;

        private const float FlashSeconds = 0.25f;
        private const float FlashScale = 1.2f;
        private const int CircleTextureSize = 128;

        private static AdmobInspector _instance;
        private static Sprite _circleSprite;

        private Canvas _canvas;
        private RectTransform _button;
        private Image _buttonImage;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private int _tapCount;
        private float _lastTapTime;
        private float _flashRemaining;
        private bool _isOpening;

        private void Awake()
        {
            if (_onlyInTestBuilds && !Debug.isDebugBuild && !NetConfigsSO.Ins.Admob_TestDevice)
            {
                SplashLogger.Warn("[AdmobInspector] hidden: needs a Development Build or AdMob Test Device in Configs SO");
                enabled = false;
                return;
            }

            if (_instance != null && _instance != this)
            {
                enabled = false;
                return;
            }

            _instance = this;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (_instance == this && _canvas == null)
                BuildButton();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void Update()
        {
            if (_canvas == null)
                return;

            UpdateLayoutIfNeeded();
            HandleTap();
            UpdateFlash();
        }

        [ContextMenu("Open Ad Inspector")]
        public void Open()
        {
            if (_isOpening)
                return;

            if (!Admob_MediationManager.IsInitComplete)
            {
                SplashLogger.Warn("[AdmobInspector] AdMob SDK is not initialized yet");
                return;
            }

            try
            {
                RegisterAsTestDevice();
            }
            catch (System.Exception exception)
            {
                SplashLogger.Error($"[AdmobInspector] test device registration failed: {exception.Message}");
            }

            _isOpening = true;
            SplashLogger.Log("[AdmobInspector] open");
            MobileAds.OpenAdInspector(error =>
            {
                // May run off the main thread: only plain Debug logging here.
                _isOpening = false;
                if (error != null)
                    Debug.LogError($"[AdmobInspector] open failed code={error.GetCode()} message={error.GetMessage()}");
                else
                    Debug.Log("[AdmobInspector] closed");
            });
        }

        // Google's test device ID is MD5(IDFA) on iOS and MD5(ANDROID_ID) on Android (from GetAutoTestDeviceIds).
        private void RegisterAsTestDevice()
        {
            var ids = new List<string>(Admob_MediationManager.GetAutoTestDeviceIds(includeSimulator: false));
            foreach (string id in _testDeviceIds)
            {
                if (!string.IsNullOrWhiteSpace(id))
                    ids.Add(id.Trim());
            }

#if UNITY_IOS && !UNITY_EDITOR
            string idfa = UnityEngine.iOS.Device.advertisingIdentifier;
            if (!string.IsNullOrEmpty(idfa) && idfa != "00000000-0000-0000-0000-000000000000")
                ids.Add(Md5Hex(idfa));
#endif

            var configuration = MobileAds.GetRequestConfiguration() ?? new RequestConfiguration();
            configuration.TestDeviceIds ??= new List<string>();
            foreach (string id in ids)
            {
                if (!configuration.TestDeviceIds.Contains(id))
                    configuration.TestDeviceIds.Add(id);
            }

            MobileAds.SetRequestConfiguration(configuration);
            SplashLogger.Log($"[AdmobInspector] test device ids={string.Join(",", configuration.TestDeviceIds)}");
        }

        private static string Md5Hex(string value)
        {
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(value));
            var builder = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
                builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

        private void BuildButton()
        {
            var canvasObject = new GameObject("AdmobInspectorCanvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            _canvas = canvasObject.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue;
            _canvas.scaleFactor = Screen.dpi > 0f ? Screen.dpi / 160f : 1f;

            var buttonObject = new GameObject("Button", typeof(RectTransform), typeof(Image));
            buttonObject.transform.SetParent(canvasObject.transform, false);
            _button = (RectTransform)buttonObject.transform;
            _button.anchorMin = _button.anchorMax = _button.pivot = new Vector2(1f, 0f);
            _button.sizeDelta = new Vector2(_buttonSizeDp, _buttonSizeDp);

            _buttonImage = buttonObject.GetComponent<Image>();
            _buttonImage.sprite = CircleSprite();
            _buttonImage.color = new Color(1f, 1f, 1f, _idleAlpha);
            // Blocks the UI underneath when the scene has an EventSystem; taps are read in HandleTap.
            _buttonImage.raycastTarget = true;

            _lastScreenSize = Vector2Int.zero;
            UpdateLayoutIfNeeded();
        }

        private void UpdateLayoutIfNeeded()
        {
            var safeArea = Screen.safeArea;
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == _lastSafeArea && screenSize == _lastScreenSize)
                return;

            _lastSafeArea = safeArea;
            _lastScreenSize = screenSize;
            float scale = _canvas.scaleFactor;
            float right = (Screen.width - safeArea.xMax) / scale + _marginDp;
            float bottom = safeArea.yMin / scale + _marginDp;
            _button.anchoredPosition = new Vector2(-right, bottom);
        }

        private void HandleTap()
        {
            if (!TryGetPressPosition(out var position) ||
                !RectTransformUtility.RectangleContainsScreenPoint(_button, position, null))
                return;

            float now = Time.unscaledTime;
            if (now - _lastTapTime > _tapTimeoutSeconds)
                _tapCount = 0;

            _lastTapTime = now;
            _tapCount++;
            _flashRemaining = FlashSeconds;

            if (_tapCount >= _tapsToOpen)
            {
                _tapCount = 0;
                Open();
            }
        }

        private void UpdateFlash()
        {
            if (_flashRemaining <= 0f)
                return;

            _flashRemaining = Mathf.Max(0f, _flashRemaining - Time.unscaledDeltaTime);
            float strength = _flashRemaining / FlashSeconds;
            _buttonImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(_idleAlpha, 1f, strength));
            _button.localScale = Vector3.one * Mathf.Lerp(1f, FlashScale, strength);
        }

        private static bool TryGetPressPosition(out Vector2 position)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    position = touch.position;
                    return true;
                }
            }

            // Touches also simulate the mouse: only read it when no finger is down.
            if (Input.touchCount == 0 && Input.GetMouseButtonDown(0))
            {
                position = Input.mousePosition;
                return true;
            }
#elif ENABLE_INPUT_SYSTEM
            var touchscreen = UnityEngine.InputSystem.Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (var touch in touchscreen.touches)
                {
                    if (touch.press.wasPressedThisFrame)
                    {
                        position = touch.position.ReadValue();
                        return true;
                    }
                }
            }

            var mouse = UnityEngine.InputSystem.Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                position = mouse.position.ReadValue();
                return true;
            }
#endif
            position = default;
            return false;
        }

        // Dark translucent disc with a white ring, so the button shows on light and dark backgrounds.
        private static Sprite CircleSprite()
        {
            if (_circleSprite != null)
                return _circleSprite;

            const int size = CircleTextureSize;
            const float ringWidth = 6f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "AdmobInspectorCircle",
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[size * size];
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.5f - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    var color = distance > radius - ringWidth
                        ? new Color(1f, 1f, 1f, 0.9f)
                        : new Color(0f, 0f, 0f, 0.5f);
                    color.a *= Mathf.Clamp01(radius - distance);
                    pixels[y * size + x] = color;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            _circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _circleSprite;
        }
    }
}
