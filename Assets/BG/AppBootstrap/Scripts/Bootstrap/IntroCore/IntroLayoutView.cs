using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Splash
{
    public sealed class IntroLayoutView : MonoBehaviour
    {
        [SerializeField] private IntroLayoutType _introLayoutType;
        [SerializeField] private GameObject _blackBackgroundRoot;
        [SerializeField] private Image _introBackground;
        [SerializeField] private Image _introCover;
        [SerializeField] private TMP_Text _introText;
        [SerializeField] private TMP_Text _introCountdownText;
        [SerializeField] private Button _introNextButton;

        private UniTaskCompletionSource _nextClickSource;
        private bool _shouldShowCountdownText;

        public IntroLayoutType LayoutType => _introLayoutType;

        private void Awake()
        {
            if (_introNextButton != null)
            {
                _introNextButton.onClick.AddListener(HandleNextClicked);
            }
        }

        private void OnDestroy()
        {
            if (_introNextButton != null)
            {
                _introNextButton.onClick.RemoveListener(HandleNextClicked);
            }
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        internal void ApplyEntry(IntroEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            ApplyTextIfNeeded(_introText, entry.introText);
            ApplySpriteIfNeeded(_introBackground, entry.introBackgroundName, "intro background");
            ApplySpriteIfNeeded(_introCover, entry.introCoverName, "intro cover");

            if (_blackBackgroundRoot != null)
            {
                _blackBackgroundRoot.SetActive(entry.showBlackBackground);
            }

            _shouldShowCountdownText = entry.showCountDownText;
            if (_introCountdownText != null)
            {
                _introCountdownText.gameObject.SetActive(_shouldShowCountdownText);
            }

            ShowNextButton(false);
        }

        public void SetCountdown(int secondsRemaining)
        {
            if (_introCountdownText == null)
            {
                return;
            }

            _introCountdownText.text = $"Next in {Mathf.Max(0, secondsRemaining)}s.";
        }

        public void ShowNextButton(bool isVisible)
        {
            if (_introNextButton != null)
            {
                _introNextButton.gameObject.SetActive(isVisible);
                _introNextButton.interactable = isVisible;
            }

            if (_introCountdownText != null)
            {
                _introCountdownText.gameObject.SetActive(!isVisible && _shouldShowCountdownText);
            }
        }

        public UniTask WaitForNextClickAsync()
        {
            if (_introNextButton == null)
            {
                return UniTask.CompletedTask;
            }

            _nextClickSource = new UniTaskCompletionSource();
            return _nextClickSource.Task;
        }

        private void HandleNextClicked()
        {
            _nextClickSource?.TrySetResult();
            _nextClickSource = null;
        }

        private static void ApplyTextIfNeeded(TMP_Text target, string value)
        {
            if (target == null || string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            target.text = value;
        }

        private static void ApplySpriteIfNeeded(Image target, string spriteName, string logName)
        {
            if (target == null || string.IsNullOrWhiteSpace(spriteName))
            {
                return;
            }

            var sprite = Resources.Load<Sprite>($"Intro/{spriteName}");
            if (sprite == null)
            {
                SplashLogger.Warn($"Missing {logName} sprite name={spriteName}");
                return;
            }

            target.sprite = sprite;
        }
    }
}
