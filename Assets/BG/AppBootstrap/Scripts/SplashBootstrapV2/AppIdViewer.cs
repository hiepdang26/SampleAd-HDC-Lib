using System;
using CountryRegionCheck;
using UnityEngine;

namespace AppBootstrap.Splash
{
    public class AppIdViewer : MonoBehaviour
    {
        private string _text = "";
        private int _fontSize = 26;
        private Color _color = Color.white;
        private TextAnchor _alignment = TextAnchor.MiddleLeft;
        private RectOffset _margin;
        private GUIStyle _style;
        public RectTransform _target;

        private void Start()
        {
            _margin = new RectOffset(0, 0, 0, -57);
            Show($"v{Application.version}-{AndroidDeviceIdUtils.GetAndroidId()}", _target);
        }

        public void Show(string text, RectTransform target = null)
        {
            _text = text;
            if (target != null) _target = target;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_text)) return;

            if (_style == null) _style = new GUIStyle(GUI.skin.label);
            _style.fontSize = _fontSize;
            _style.normal.textColor = _color;
            _style.alignment = _alignment;
            _style.wordWrap = true;

            Rect box = ApplyOffset(RectFromTarget(), _margin);
            GUI.Label(box, _text, _style);
        }

        private static Rect ApplyOffset(Rect r, RectOffset o)
        {
            if (o == null) return r;
            return new Rect(
                r.x + o.left,
                r.y + o.top,
                Mathf.Max(0, r.width - o.left - o.right),
                Mathf.Max(0, r.height - o.top - o.bottom));
        }

        private Rect RectFromTarget()
        {
            if (_target == null)
                return new Rect(0, Screen.height * 0.5f - 40, Screen.width, 80);

            Vector3[] c = new Vector3[4];
            _target.GetWorldCorners(c);
            Canvas canvas = _target.GetComponentInParent<Canvas>();
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                ? canvas.worldCamera
                : null;

            Vector2 bl = RectTransformUtility.WorldToScreenPoint(cam, c[0]);
            Vector2 tr = RectTransformUtility.WorldToScreenPoint(cam, c[2]);

            float xMin = Mathf.Min(bl.x, tr.x);
            float w = Mathf.Abs(tr.x - bl.x);
            float h = Mathf.Abs(tr.y - bl.y);
            float yGui = Screen.height - Mathf.Max(bl.y, tr.y);
            return new Rect(xMin, yGui, w, h);
        }
    }
}