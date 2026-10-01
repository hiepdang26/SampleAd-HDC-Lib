using BG_Library.Common;
using BG_Library.NET.AdSystem;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BG_Library.DEBUG
{
    public class ActivePanelDebugController : MonoBehaviour
    {
        private enum ActivationCorner
        {
            TopLeft,
            TopRight
        }

        public GameObject debugPanel;

        [SerializeField] private ActivationCorner activationCorner = ActivationCorner.TopLeft;
        [SerializeField, Range(0.05f, 0.3f)] private float activationWidthPercent = 0.14f;
        [SerializeField, Range(0.05f, 0.3f)] private float activationHeightPercent = 0.14f;
        [SerializeField, Min(2)] private int requiredTapCount = 3;
        [SerializeField, Range(0.25f, 1.5f)] private float tapWindowSeconds = 0.9f;
        [SerializeField, Range(0f, 1f)] private float gestureCooldownSeconds = 0.25f;
        [SerializeField] private bool allowKeyboardToggle = true;

        private int tapCount;
        private float lastTapTime = float.MinValue;
        private float lastToggleTime = float.MinValue;
        private ScreenOrientation startOrientation;
        private bool warnedMissingPanel;
        private bool keepLandscapeSession;
        private CanvasScaler overlayCanvasScaler;
        private Vector2 defaultReferenceResolution;
        private bool hasDefaultReferenceResolution;

        private const string RuntimeDebugPanelName = "Flow + Tracking panel";

        private void Awake()
        {
            TryResolveDebugPanel();
            ConfigurePanelRuntimeMode();
            CacheOverlayCanvasScaler();
            RestoreOverlayReferenceResolution();
        }

        private void Start()
        {
            TryResolveDebugPanel();
            ConfigurePanelRuntimeMode();
            CacheOverlayCanvasScaler();
            RestoreOverlayReferenceResolution();
            startOrientation = Screen.orientation;
            if (debugPanel != null)
                debugPanel.SetActive(false);
        }

        private void Update()
        {
            if (!TryResolveDebugPanel())
                return;

            if (TryHandleKeyboardToggle())
                return;

            if (debugPanel.activeSelf)
                return;

            TryHandleActivationGesture();
        }

        private bool IsInActivationCorner(Vector2 screenPosition)
        {
            float marginX = Screen.width * activationWidthPercent;
            float marginY = Screen.height * activationHeightPercent;

            if (screenPosition.y < Screen.height - marginY)
                return false;

            return activationCorner == ActivationCorner.TopLeft
                ? screenPosition.x <= marginX
                : screenPosition.x >= Screen.width - marginX;
        }

        private void HandleTap()
        {
            float currentTime = Time.unscaledTime;
            if (currentTime - lastToggleTime < gestureCooldownSeconds)
                return;

            if (currentTime - lastTapTime > tapWindowSeconds)
                tapCount = 0;

            tapCount++;
            lastTapTime = currentTime;

            if (tapCount >= requiredTapCount)
            {
                tapCount = 0;
                OpenPanel();
            }
        }

        private void TogglePanel()
        {
            if (debugPanel.activeSelf)
            {
                OnClickCloseBtn();
            }
            else
            {
                OpenPanel();
            }
        }

        private void OpenPanel()
        {
            if (!TryResolveDebugPanel())
                return;

            tapCount = 0;
            lastToggleTime = Time.unscaledTime;
            startOrientation = Screen.orientation;
            ConfigurePanelRuntimeMode();
            CacheOverlayCanvasScaler();
            keepLandscapeSession = ShouldKeepLandscapeDebugUi();

            if (keepLandscapeSession)
            {
                ApplyLandscapeOverlayReferenceResolution();
            }
            else
            {
                RestoreOverlayReferenceResolution();
            }

            debugPanel.SetActive(true);

            if (!keepLandscapeSession)
                ManualOrientationController.ForcePortrait();
        }

        public void OnClickCloseBtn()
        {
            if (debugPanel == null)
                return;

            tapCount = 0;
            lastToggleTime = Time.unscaledTime;
            debugPanel.SetActive(false);
            RestoreOverlayReferenceResolution();

            if (keepLandscapeSession)
            {
                keepLandscapeSession = false;
                return;
            }

            if (startOrientation == ScreenOrientation.LandscapeLeft ||
                startOrientation == ScreenOrientation.LandscapeRight)
            {
                ManualOrientationController.ForceLandscape();
            }
            else
            {
                ManualOrientationController.ForcePortrait();
            }
        }

        private bool ShouldKeepLandscapeDebugUi()
        {
            return NetConfigsSO.Ins != null
                && NetConfigsSO.Ins.DebugUI_KeepLandscape
                && ManualOrientationController.IsLandscape();
        }

        private void CacheOverlayCanvasScaler()
        {
            if (overlayCanvasScaler != null && hasDefaultReferenceResolution)
                return;

            var canvasTransform = transform.Find("Canvas");
            if (canvasTransform == null)
                return;

            overlayCanvasScaler = canvasTransform.GetComponent<CanvasScaler>();
            if (overlayCanvasScaler == null)
                return;

            defaultReferenceResolution = overlayCanvasScaler.referenceResolution;
            hasDefaultReferenceResolution = true;
        }

        private void ApplyLandscapeOverlayReferenceResolution()
        {
            if (overlayCanvasScaler == null && !hasDefaultReferenceResolution)
                CacheOverlayCanvasScaler();

            if (overlayCanvasScaler == null || !hasDefaultReferenceResolution)
                return;

            overlayCanvasScaler.referenceResolution = new Vector2(
                defaultReferenceResolution.y,
                defaultReferenceResolution.x);
        }

        private void RestoreOverlayReferenceResolution()
        {
            if (overlayCanvasScaler == null && !hasDefaultReferenceResolution)
                CacheOverlayCanvasScaler();

            if (overlayCanvasScaler == null || !hasDefaultReferenceResolution)
                return;

            overlayCanvasScaler.referenceResolution = defaultReferenceResolution;
        }

        private bool TryResolveDebugPanel()
        {
            if (debugPanel != null)
                return true;

            var safeArea = transform.Find("Canvas/Safe area");
            if (safeArea != null)
            {
                var found = safeArea.Find(RuntimeDebugPanelName);
                if (found != null)
                {
                    debugPanel = found.gameObject;
                    return true;
                }
            }

            var direct = FindDescendantByName(transform, RuntimeDebugPanelName);
            if (direct != null)
            {
                debugPanel = direct.gameObject;
                return true;
            }

            if (!warnedMissingPanel)
            {
                warnedMissingPanel = true;
                Debug.LogWarning("ActivePanelDebugController: could not find the debug panel root on 'Debug Overlay Canvas'.");
            }

            return false;
        }

        private void ConfigurePanelRuntimeMode()
        {
            if (debugPanel == null)
                return;

            var rootTabs = debugPanel.GetComponent<SelectPanels>();
            if (rootTabs == null)
                return;

            if (rootTabs.BtnsParent != null)
                rootTabs.BtnsParent.gameObject.SetActive(false);

            if (rootTabs.PanelsParent == null)
                return;

            var runtimePanel = rootTabs.PanelsParent.Find(RuntimeDebugPanelName);
            if (runtimePanel == null)
                return;

            for (int i = 0; i < rootTabs.PanelsParent.childCount; i++)
            {
                var child = rootTabs.PanelsParent.GetChild(i);
                if (child != null)
                    child.gameObject.SetActive(child == runtimePanel);
            }
        }

        private bool TryHandleKeyboardToggle()
        {
#if ENABLE_INPUT_SYSTEM
            if (allowKeyboardToggle && Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
            {
                TogglePanel();
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (allowKeyboardToggle && Input.GetKeyDown(KeyCode.F10))
            {
                TogglePanel();
                return true;
            }
#endif

            return false;
        }

        private void TryHandleActivationGesture()
        {
#if ENABLE_INPUT_SYSTEM
            var touchscreen = Touchscreen.current;
            if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                if (IsInActivationCorner(touchscreen.primaryTouch.position.ReadValue()))
                {
                    HandleTap();
                    return;
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                if (IsInActivationCorner(mouse.position.ReadValue()))
                {
                    HandleTap();
                    return;
                }
            }
#endif
#endif

#if !ENABLE_INPUT_SYSTEM
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase != TouchPhase.Began)
                        continue;

                    if (IsInActivationCorner(touch.position))
                    {
                        HandleTap();
                        return;
                    }
                }
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButtonDown(0) && IsInActivationCorner(Input.mousePosition))
                HandleTap();
#endif
#endif
        }

        private static Transform FindDescendantByName(Transform root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
                return null;

            if (root.name == name)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                var found = FindDescendantByName(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
