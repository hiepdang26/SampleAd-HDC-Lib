using TMPro;
using UnityEngine;

namespace AppBootstrap.Intro
{
    public class TextAnimation : MonoBehaviour
    {
        public TMP_Text text;
        public string[] texts;

        [Tooltip("Thời gian (giây) giữa mỗi lần đổi text")]
        public float interval = 1f;

        [Tooltip("Lặp lại từ đầu khi chạy hết mảng")]
        public bool loop = true;

        [Tooltip("Dùng thời gian không phụ thuộc Time.timeScale (nên bật cho màn loading)")]
        public bool useUnscaledTime = true;

        private float _timer;
        private int _index;

        private void OnEnable()
        {
            _index = 0;
            _timer = 0f;
            ShowCurrent();
        }

        private void Update()
        {
            if (texts == null || texts.Length == 0) return;

            _timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (_timer < interval) return;

            _timer = 0f;
            _index++;

            if (_index >= texts.Length)
            {
                if (loop)
                {
                    _index = 0;
                }
                else
                {
                    _index = texts.Length - 1;
                    enabled = false; // dừng ở text cuối
                    return;
                }
            }

            ShowCurrent();
        }

        private void ShowCurrent()
        {
            if (text != null && texts != null && _index >= 0 && _index < texts.Length)
                text.text = texts[_index];
        }
    }
}
