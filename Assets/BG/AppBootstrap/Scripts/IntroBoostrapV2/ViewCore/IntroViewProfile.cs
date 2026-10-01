using System;
using System.Collections.Generic;
using System.Linq;
using AppBootstrap.Splash;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AppBootstrap.Intro
{
    public class IntroViewProfile : MonoBehaviour
    {
        public PULayout layout;
        public GameObject loading;
        public Image _background;
        public List<DropdownPro> totalDropdowns = new List<DropdownPro>();
        public Button _introContinueBtn;
        public GameObject _buttonGray;
        
        private IntroGroupAd _config;
        private string _preloadAd;
        private bool _isCompleteView;
        private bool _isShowContinueBtn;
        
        //private InterstitialAdRepository _interstitialAd;
        
        public void RendererView(IntroGroupAd config, string preloadAd)
        {
            _isCompleteView = false;
            _preloadAd = preloadAd;
            _config = config;
            if(_background != null && !string.IsNullOrEmpty(config.IntroBackgroundImage))
                _background.sprite = Resources.Load<Sprite>($"Intro/{config.IntroBackgroundImage}");

            //_interstitialAd = new InterstitialAdRepository("inter_profile", _config.InterstitialConfig, 10);
            
            _introContinueBtn.gameObject.SetActive(false);
            _introContinueBtn.onClick.AddListener(OnClickContinueBtn);
        }
        
        private void Update()
        {
            var isAnyEmpty = totalDropdowns.Exists(input => !input.HasSelected());
            if (!isAnyEmpty && !_isShowContinueBtn)
            {
                _isShowContinueBtn = true;
                _buttonGray.gameObject.SetActive(false);
                _introContinueBtn.gameObject.SetActive(true);
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
            NetCallerAPI.PU_InitManually(_preloadAd);
            if (NetCallerAPI.FA_AbleToShow(_config.NativeAfterInterstitialConfig))
                await ShowAdAndWaitCompleteAd();
            loading.SetActive(false);
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