using System;
using System.Collections.Generic;
using System.Linq;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Intro
{
    public class IntroViewAvatar : MonoBehaviour
    {
        public PULayout layout;
        public GameObject loading;
        public Image _background;
        public Sprite selectedImage;
        public Sprite unSelectedImage;
        public List<Button> totalAvatars;
        public Button _introContinueBtn;
        public GameObject _grayButton;
        
        private IntroGroupAd _config;
        private bool _isCompleteView;
        private bool _isActiveBtn;
        
        public void RendererView(IntroGroupAd config)
        {
            _isCompleteView = false;
            _config = config;
            if(_background != null && !string.IsNullOrEmpty(config.IntroBackgroundImage))
                _background.sprite = Resources.Load<Sprite>($"Intro/{config.IntroBackgroundImage}");
            _introContinueBtn.gameObject.SetActive(false);
            _introContinueBtn.onClick.AddListener(OnClickContinueBtn);
            
            foreach (var totalAvatar in totalAvatars)
            {
                totalAvatar.onClick.AddListener(() => OnClickAvatar(totalAvatar));
            }
        }

        private void OnClickAvatar(Button btn)
        {
            foreach (var totalAvatar in totalAvatars)
            {
                var image = totalAvatar.GetComponent<Image>();
                image.sprite = totalAvatar == btn ? selectedImage : unSelectedImage;
            }

            if (!_isActiveBtn)
            {
                _grayButton.gameObject.SetActive(false);
                _introContinueBtn.gameObject.SetActive(true);
                _isActiveBtn = true;
            }
        }

        private async void OnClickContinueBtn()
        {
            NetCallerAPI.FA_InitManually(_config.NativeAfterInterstitialConfig);
            NetCallerAPI.PU_Hide(_config.PopupConfig);
            loading.SetActive(true);
            await UniTask.WhenAny(UniTask.WaitUntil(() =>
                    NetCallerAPI.FA_AbleToShow(_config.NativeAfterInterstitialConfig)),
                UniTask.Delay(TimeSpan.FromSeconds(_config.NativeAfterInterstitialConfigTimeOut), DelayType.DeltaTime));
            if (NetCallerAPI.FA_AbleToShow(_config.NativeAfterInterstitialConfig))
                await ShowAdAndWaitCompleteAd();
            // loading.SetActive(false);
            _isCompleteView = true;
        }

        private async UniTask ShowAdAndWaitCompleteAd()
        {
            var completeAd = false;
            NetCallerAPI.FA_Show(_config.NativeAfterInterstitialConfig, () =>
            {
                completeAd = true;
            });
            await UniTask.WaitUntil(() => completeAd);
        }

        public bool IsCompleteView()
        {
            return _isCompleteView;
        }
    }
}