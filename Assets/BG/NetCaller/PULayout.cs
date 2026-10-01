using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BG_Library.NET.API
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class PULayout : MonoBehaviour
    {
        [SerializeField] private CanvasScaler _canvasScaler;
#if UNITY_EDITOR

        [Button("SETUP")]
        private void SetUp()
        {
            if (canvas == null) canvas = GetComponent<Canvas>();
            if (_canvasScaler == null) _canvasScaler = GetComponent<CanvasScaler>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            _canvasScaler.scaleFactor = 1;
            _canvasScaler.referencePixelsPerUnit = 100;
        }
#endif

        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform target;

        private bool isCenterCached;
        private bool isSizeCached;
        private Vector2 cachedCenterNormalized;
        private Vector2 cachedSizePixels;

        public Canvas Canvas => canvas;
        
        public CanvasScaler CanvasScaler => _canvasScaler;

        public RectTransform Target
        {
            get => target;
            set
            {
                target = value;
                InvalidateCache();
            }
        }

        public Vector2 CenterNormalized
        {
            get
            {
                if (!isCenterCached)
                {
                    cachedCenterNormalized = CalculateCenterNormalized();
                    isCenterCached = true;
                }

                return cachedCenterNormalized;
            }
        }

        public Vector2 SizeInPixels
        {
            get
            {
                if (!isSizeCached)
                {
                    cachedSizePixels = CalculateSizeInPixels();
                    isSizeCached = true;
                }

                return cachedSizePixels;
            }
        }

        public void InvalidateCache()
        {
            isCenterCached = false;
            isSizeCached = false;
        }

        private Vector2 CalculateCenterNormalized()
        {
            if (canvas == null || target == null)
                return Vector2.zero;

            Vector3 centerWorld = target.TransformPoint(target.rect.center);
            Vector2 centerScreen = RectTransformUtility.WorldToScreenPoint(null, centerWorld);
            Rect canvasRect = canvas.pixelRect;

            float nx = (centerScreen.x - canvasRect.xMin) / canvasRect.width;
            float ny = (centerScreen.y - canvasRect.yMin) / canvasRect.height;

            return new Vector2(
                Mathf.Clamp01(nx),
                Mathf.Clamp01(ny)
            );
        }

        private Vector2 CalculateSizeInPixels()
        {
            if (target == null)
                return Vector2.zero;

            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);

            float width = corners[2].x - corners[0].x;
            float height = corners[2].y - corners[0].y;

            return new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        }
    }
}