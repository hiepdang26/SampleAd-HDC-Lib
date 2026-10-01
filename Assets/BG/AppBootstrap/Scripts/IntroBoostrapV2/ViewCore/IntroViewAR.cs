using System;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Intro
{
    public class IntroViewAR : View
    {
        public Image _introBackground; 
        public Image _introCover;
        
        public TMP_Text _introDescription;
        public TMP_Text _introCountDownTxt;
        public Button _introContinueBtn;
        
        private IntroConfig _config;
        private bool _isCompleteView;
        
        public override void RendererView(IntroConfig config)
        {
            _isCompleteView = false;
            _config = config;
            
            if(_introBackground != null && !string.IsNullOrEmpty(_config.IntroBackgroundImage))
                _introBackground.sprite = Resources.Load<Sprite>($"Intro/{_config.IntroBackgroundImage}");

            if (_introCover != null && !string.IsNullOrEmpty(_config.IntroCoverImage))
                _introCover.sprite = Resources.Load<Sprite>($"Intro/{_config.IntroCoverImage}");
            
            if(_introDescription != null && !string.IsNullOrEmpty(_config.IntroDescription))
                _introDescription.text = _config.IntroDescription;

            if (_introCountDownTxt != null)
                _introCountDownTxt.text = $"Next in {_config.IntroInterval}s.";
            
            _introContinueBtn.gameObject.SetActive(false);
            _introContinueBtn.onClick.AddListener(OnClickContinueBtn);
            CountDownIntro();
        }

        private async void CountDownIntro()
        {
            int remainingSeconds = Mathf.Max(0, (int)_config.IntroInterval);
            _introCountDownTxt.text = $"Next in {remainingSeconds}s.";

            while (remainingSeconds > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.DeltaTime);
                remainingSeconds--;
                _introCountDownTxt.text = $"Next in {remainingSeconds}s.";
            }
            _introCountDownTxt.gameObject.SetActive(false);
            _introContinueBtn.gameObject.SetActive(true);
        }

        private void OnClickContinueBtn()
        {
            _isCompleteView = true;
        }

        public override bool CanRendererConfig(IntroConfig config)
        {
            return config.IntroType == IntroType.AR;
        }

        public override bool IsCompleteView()
        {
            return _isCompleteView;
        }
    }
}