using System;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Intro
{
    public class IntroViewPT : View
    {
        public Image _background;
        public GameObject _countDownParent;
        public TMP_Text _introCountDownTxt;
        public Button _introContinueBtn;
        
        private IntroConfig _config;
        private bool _isCompleteView;
        
        public override void RendererView(IntroConfig config)
        {
            _isCompleteView = false;
            _config = config;
            if(_background != null && !string.IsNullOrEmpty(config.IntroBackgroundImage))
                _background.sprite = Resources.Load<Sprite>($"Intro/{config.IntroBackgroundImage}");
            _countDownParent.SetActive(true);
            _introContinueBtn.gameObject.SetActive(false);
            _introContinueBtn.onClick.AddListener(OnClickContinueBtn);
            CountDownIntro();
        }

        private async void CountDownIntro()
        {
            int remainingSeconds = Mathf.Max(0, (int)_config.IntroInterval);
            _introCountDownTxt.text = $"{remainingSeconds}";

            while (remainingSeconds > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.DeltaTime);
                remainingSeconds--;
                _introCountDownTxt.text = $"{remainingSeconds}";
            }
            _countDownParent.SetActive(false);
            _introContinueBtn.gameObject.SetActive(true);
        }

        private void OnClickContinueBtn()
        {
            _isCompleteView = true;
        }

        public override bool CanRendererConfig(IntroConfig config)
        {
            return config.IntroType == IntroType.PT;
        }

        public override bool IsCompleteView()
        {
            return _isCompleteView;
        }
    }
}