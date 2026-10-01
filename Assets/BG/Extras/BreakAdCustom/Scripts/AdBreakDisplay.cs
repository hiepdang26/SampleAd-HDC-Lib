using System;
using System.Collections;
using AppBootstrap.Splash;
using BG_Library.Common;
using BG_Library.NET;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.AdSystem;
using BG_Library.NET.API;
#if boostrap_ios && UNITY_IOS
using BG_Library.NET.AdCore.MainIOS;
using IOSFSNativeInstance = BG_Library.NET.AndroidSDK.IOSFSNativeInstance;
#endif
using Cysharp.Threading.Tasks;
using Eco.TweenAnimation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BreakAdConfig
{
    public string adUnitId;
    public string layoutGroup;
    public float showBreakAdTime = 60;
    public float notiBreakAdTime = 5;
}

public class AdBreakDisplay : MonoBehaviour
{
    [SerializeField] private TweenAnimationBase _animation;
    [SerializeField] private TweenAnimationBase _breakAdUI;

    [SerializeField] private GameObject loadingAdContent;
    [SerializeField] private GameObject countDownContent;
    
    [SerializeField] private Image _fillCountDown;
    [SerializeField] private TMP_Text _timeTxt;
    [SerializeField] private TMP_Text _text;

    private Coroutine _countDownCoroutine;
    private BreakAdConfig _config;
#if boostrap_ios && UNITY_IOS
    private static IOSFSNativeInstance _iosNativeAdInstance;
    private static AdStatus _iosAdStatus;
#else
    private static NativeAndroidAdRepository _nativeAndroidAdRepository;
#endif
    private bool _firstInitialize;

    private bool _callShowBreakAd;
    private float _currentTime;

    private void Awake()
    {
        var rawConfig = NetCallerAPI.RemoteConfig_GetCustom("break_ads_config");
        if (string.IsNullOrWhiteSpace(rawConfig))
        {
            SplashLogger.Warn("Break ad disabled: break_ads_config is empty.");
            return;
        }

        _config = JsonUtility.FromJson<BreakAdConfig>(rawConfig);
        if (_config == null || string.IsNullOrWhiteSpace(_config.adUnitId))
        {
            SplashLogger.Warn("Break ad disabled: break_ads_config has no adUnitId.");
            return;
        }

#if boostrap_ios && UNITY_IOS
        var iosCore = AdsLogic.AdsCoreIns as AdCore_MainIOS;
        var layout = iosCore?.ConfigsIns?.GetLayoutGroupConfigByGroupName(_config.layoutGroup);
        if (layout == null)
        {
            SplashLogger.Warn($"Break ad skipped on iOS: layout group not found group={_config.layoutGroup}");
            return;
        }

        _iosNativeAdInstance ??= new IOSFSNativeInstance(new[] { _config.adUnitId }, layout);
        _iosNativeAdInstance.OnAdLoadedEvent += OnIosAdLoaded;
        _iosNativeAdInstance.OnAdLoadFailedEvent += OnIosAdLoadFailed;
        _iosNativeAdInstance.OnAdDisplayedEvent += OnIosAdDisplayed;
        _iosNativeAdInstance.OnAdHiddenEvent += OnIosAdHidden;
        _iosNativeAdInstance.OnAdShowFailedEvent += OnIosAdShowFailed;
#else
        var androidCore = AdsLogic.AdsCoreIns as AdCore_MainAndroid;
        var layout = androidCore.ConfigsIns.GetLayoutGroupConfigByGroupName(_config.layoutGroup);

        _nativeAndroidAdRepository ??= new NativeAndroidAdRepository("fa_brk", _config.adUnitId, layout, false);
#endif
        NetEventSystem.OnFsDisplayed += OnFsDisplayed;
    }

#if boostrap_ios && UNITY_IOS
    private void OnIosAdLoaded(BG_Library.NET.AndroidSDK.AdInfo adInfo) => _iosAdStatus = AdStatus.Loaded;
    private void OnIosAdLoadFailed(string adUnitId, int errorCode, string errorMessage) => _iosAdStatus = AdStatus.LoadFailed;
    private void OnIosAdDisplayed(BG_Library.NET.AndroidSDK.AdInfo adInfo) => _iosAdStatus = AdStatus.AdDisplayed;
    private void OnIosAdHidden(BG_Library.NET.AndroidSDK.AdInfo adInfo) => _iosAdStatus = AdStatus.AdClosed;
    private void OnIosAdShowFailed(BG_Library.NET.AndroidSDK.AdInfo adInfo, int errorCode, string errorMessage) => _iosAdStatus = AdStatus.AdShowFailed;
#endif

    private void OnFsDisplayed(AdInfo adInfo)
    {
        StopDisplay();
        
        _currentTime = 0;
        _callShowBreakAd = false;
    }

    private void OnDestroy()
    {
        StopDisplay();
        NetEventSystem.OnFsDisplayed -= OnFsDisplayed;
#if boostrap_ios && UNITY_IOS
        if (_iosNativeAdInstance != null)
        {
            _iosNativeAdInstance.OnAdLoadedEvent -= OnIosAdLoaded;
            _iosNativeAdInstance.OnAdLoadFailedEvent -= OnIosAdLoadFailed;
            _iosNativeAdInstance.OnAdDisplayedEvent -= OnIosAdDisplayed;
            _iosNativeAdInstance.OnAdHiddenEvent -= OnIosAdHidden;
            _iosNativeAdInstance.OnAdShowFailedEvent -= OnIosAdShowFailed;
            _iosNativeAdInstance.DestroyAd();
            _iosNativeAdInstance = null;
        }
#endif
    }

#if boostrap_ios && UNITY_IOS
    private void CallLoadBreakAd()
    {
        if (_iosNativeAdInstance == null || _iosNativeAdInstance.IsReady())
            return;

        _iosAdStatus = AdStatus.Loading;
        _iosNativeAdInstance.LoadAd();
    }
#else
    private async void CallLoadBreakAd()
    {
        if (_nativeAndroidAdRepository.IsReady())
        {
            return;
        }

        var cancel = this.GetCancellationTokenOnDestroy();
        await _nativeAndroidAdRepository.LoadAsync(cancel);
    }
#endif

    private void Update()
    {
        if (_config == null || _config.showBreakAdTime == 0) return;
        _currentTime += Time.unscaledDeltaTime;
        if (_currentTime >= _config.showBreakAdTime - _config.notiBreakAdTime && !_callShowBreakAd)
        {
            DisplayTime(_config.notiBreakAdTime);
            _callShowBreakAd = true;
        }

        if (_currentTime > _config.showBreakAdTime)
        {
            _currentTime = 0;
            _callShowBreakAd = false;
        }
    }

    private void DisplayTime(float time)
    {
        CallLoadBreakAd();
        if (_countDownCoroutine != null)
            StopCoroutine(_countDownCoroutine);
        _text.text = "BREAK AD...";
        _countDownCoroutine = StartCoroutine(IERunCountDown(time));
    }

    private void StopDisplay()
    {
        if (_countDownCoroutine != null)
            StopCoroutine(_countDownCoroutine);
        _breakAdUI.gameObject.SetActive(false);
    }

    private IEnumerator IERunCountDown(float time)
    {
        loadingAdContent.gameObject.SetActive(false);
        countDownContent.gameObject.SetActive(true);
        _breakAdUI.gameObject.SetActive(true);
        float endTime = Time.realtimeSinceStartup + time;
        _timeTxt.text = $"{(int)Mathf.Ceil(time)}";
        while (Time.realtimeSinceStartup < endTime)
        {
            float currentTime = endTime - Time.realtimeSinceStartup;
            _fillCountDown.fillAmount = currentTime / time;
            _timeTxt.text = $"{(int)Mathf.Ceil(currentTime)}";
            yield return null;
        }

        var cancel = this.GetCancellationTokenOnDestroy();

#if boostrap_ios && UNITY_IOS
        if (_iosAdStatus == AdStatus.Loading)
        {
            _text.text = "LOADING AD...";
            loadingAdContent.gameObject.SetActive(true);
            countDownContent.gameObject.SetActive(false);
            yield return UniTask.WaitUntil(() => _iosAdStatus != AdStatus.Loading, cancellationToken: cancel).ToCoroutine();
        }

            if (_iosNativeAdInstance != null && _iosNativeAdInstance.IsReady())
            {
                bool waitForClose = false;
                try
                {
                    AppResumeSystem.Instance?.BlockAdResume();
                    _iosAdStatus = AdStatus.AdDisplayed;
                    _iosNativeAdInstance.ShowAd();
                    waitForClose = true;
                }
                catch (Exception ex)
                {
                    _iosAdStatus = AdStatus.AdShowFailed;
                    SplashLogger.Warn($"iOS break ad show failed: {ex.Message}");
                }

                if (waitForClose)
                {
                    yield return UniTask.WaitUntil(
                        () => _iosAdStatus == AdStatus.AdClosed || _iosAdStatus == AdStatus.AdShowFailed,
                        cancellationToken: cancel).ToCoroutine();
                }
            }
#else
        if (_nativeAndroidAdRepository.Status == AdStatus.Loading)
        {
            _text.text = "LOADING AD...";
            loadingAdContent.gameObject.SetActive(true);
            countDownContent.gameObject.SetActive(false);
            yield return UniTask.WaitUntil(() => _nativeAndroidAdRepository.Status != AdStatus.Loading, cancellationToken: cancel).ToCoroutine();
        }
        
        if (_nativeAndroidAdRepository.IsReady())
            yield return _nativeAndroidAdRepository.ShowAsync(cancel).ToCoroutine();
#endif
        
        _breakAdUI.gameObject.SetActive(false);
    }
}
