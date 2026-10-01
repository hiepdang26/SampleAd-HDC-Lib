 using System;
using System.Threading;
using AppBootstrap.Intro;
using BG_Library.Common;
using BG_Library.NET;
using BG_Library.NET.AdCore.MainAndroid;
#if boostrap_ios && UNITY_IOS
using BG_Library.NET.AdCore.MainIOS;
#endif
using BG_Library.NET.AdSystem;
using BG_Library.NET.API;
using BG_Library.NET.Mediation.Admob;
using CountryRegionCheck;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Firebase.Analytics;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace AppBootstrap.Splash
{
    public class SplashBoostrapV2 : MonoBehaviour
    {
        [SerializeField] private bool _isCheckInternet;
        [SerializeField] private bool _loadToNextScene;
        [SerializeField] private NoInternetTracking _noInternetTracking;
        [SerializeField] private PULayout _nativeSplashLayout;
        [SerializeField] private PULayout _nativeSplashAdmobLayout;

        public UnityEvent onFirebaseInitialized;
        public UnityEvent<float> onProgressChanged;
        public UnityEvent onSplashFinished;

        private string _admobAdapterRegex = "admob";
        private SplashConfigV2 _config;
        private PopupAdRepository _nativeSplashRepository;
        private PopupAdRepository _nativePreloadRepository;
        private InterstitialAdRepository _interstitialAdRepository;
        private NativeAndroidAdRepository _nativeForceAdRepository;
        private float _nativeDeltaTime = 2;

        private bool _isInterstitialLoadEnd;
        private bool _isInterstitialShowing;

        private bool _isNativeSplashLoadEnd;
        private bool _isNativeAfterInterstitialEnd;
        private bool _isCallShowNativeAfterInterstitial;
        private bool _isCallShowNativeAfterInterstitialEnd;
        
        private bool _isCallShowNativePreload;
        private bool _isNativePreloadLoadEnd;
        private float _loadingProgress;
        
        private Tweener _loadingTweener;
        
        private void Awake()
        {
            FirstSessionData.CheckAndCacheFirstOpenData();
            FirstSessionData.EndFirstOpenSession();
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            SplashLogger.Log("Bootstrap V2 start");
            SplashTracking.Tracking("4_admob_start");
            await WaitAdmobSdk();
            SplashTracking.Tracking("4_admob_end");
            SplashLogger.Log("Admob privacy gate completed");

            await WaitFirebaseInitialize();
            SplashLogger.Log("Firebase initialized");
            SplashTracking.Tracking("1_firebase_end");
            if (_isCheckInternet)
            {
                SplashTracking.Tracking("2_internet_start");
                await WaitInternetTracking();
                SplashTracking.Tracking("2_internet_end");
            }
            SplashLogger.Log("Internet available");
            SplashTracking.Tracking("3_remote_start");
            await WaitFirebaseRemote();
            onFirebaseInitialized?.Invoke();
            SplashTracking.Tracking("3_remote_end");
            SplashLogger.Log("Remote available");
            SplashTracking.Tracking("4_remote_start");
            SplashLogger.Log("Load remote config");
            LoadRemoteConfiguration();
            SplashLogger.Log("Load remote end");
            SplashTracking.Tracking("4_remote_end");
            SplashLogger.Log("Admob available");

            _ = Loading(0.9f, _config.loadingTime);
            if (!Admob_MediationManager.CanRequestAds)
            {
                SplashLogger.Warn("Ad request disabled by consent. Skip splash ads.");
                SplashTracking.Tracking("5_load_ad_skip_consent");
                await FinishSplashWithoutAds();
                return;
            }

            SplashTracking.Tracking("5_load_ad_start");
            SplashLogger.Log("Start load ads");
            await UniTask.WhenAll(WaitNativeSplashAndShowing(), WaitInterstitialsSplash());
            SplashLogger.Log("Start load end");
            SplashTracking.Tracking("5_load_ad_end");
            CallLoadNativeAfterInterSplash();
            SplashLogger.Log("Start load next scene");
            await Loading(1f, 0.25f);

            if (_loadToNextScene)
            {
                var nextScene = FirstSessionData.IsFirstOpen ? _config.firstOpenNextScene : _config.nextScene;
                await SceneManager.LoadSceneAsync(nextScene).ToUniTask();
            }

            if (_nativeSplashRepository != null)
            {
                NetCallerAPI.PU_Hide(_nativeSplashRepository.GroupName);
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(_config.delayShowInterstitials), DelayType.DeltaTime);
            await ShowingInterstitialAndNativeAfterInter();
            SplashLogger.Log("Bootstrap End Show Ad");
            onSplashFinished?.Invoke();
            SplashTracking.Tracking("6_end_splash");
            SplashLogger.Log("Bootstrap V2 done");
        }

        private async UniTask FinishSplashWithoutAds()
        {
            await Loading(1f, 0.25f);

            if (_loadToNextScene)
            {
                var nextScene = FirstSessionData.IsFirstOpen ? _config.firstOpenNextScene : _config.nextScene;
                await SceneManager.LoadSceneAsync(nextScene).ToUniTask();
            }

            SplashLogger.Log("Bootstrap End Without Ads");
            onSplashFinished?.Invoke();
            SplashTracking.Tracking("6_end_splash");
            SplashLogger.Log("Bootstrap V2 done");
        }

        private void LoadRemoteConfiguration()
        {
            var rawConfig = NetCallerAPI.RemoteConfig_GetCustom("splash_config");
            _config = JsonUtility.FromJson<SplashConfigV2>(rawConfig);
            SplashLogger.Log($"Remote config loaded raw={rawConfig}");
        }

        private async UniTask WaitInternetTracking()
        {
            SplashLogger.Log("Waiting for internet...");
            await UniTask.WaitUntil(() => _noInternetTracking.InternetStatus == InternetStatus.InternetAvailable);
            _noInternetTracking.Tracking();
        }

        private async UniTask WaitFirebaseRemote()
        {
            SplashLogger.Log("Waiting Firebase remote...");
            await UniTask.WaitUntil(() => NetCallerAPI.RemoteConfig_IsFetchComplete);
        }

        private async UniTask WaitAdmobSdk()
        {
            SplashLogger.Log("Waiting admob...");
            await UniTask.WaitUntil(() => Admob_MediationManager.IsInitComplete);
        }
        
        private async UniTask WaitInterstitialsSplash()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            _interstitialAdRepository = new InterstitialAdRepository("it", _config.interSplashAdUnitId, _config.interLoadAdTimeout);

            if (string.IsNullOrEmpty(_config.interSplashAdUnitId))
            {
                _isInterstitialLoadEnd = true;
                return;
            }

            if (_config.waitNativeSplashLoad)
            {
                await UniTask.WaitUntil(() => _isNativeSplashLoadEnd, cancellationToken: cancellationToken);
            }
            SplashTracking.Tracking("5_it_l_s");
            SplashLogger.Log($"Interstitial splash load start group={_config.interSplashAdUnitId}");
            await _interstitialAdRepository.LoadAsync(cancellationToken);
            SplashLogger.Log($"Interstitial splash load status={_interstitialAdRepository.IsReady()}");
            if (_interstitialAdRepository.IsReady())
            {
                SplashTracking.Tracking("5_it_l_e");
            }
            else
            {
                SplashTracking.Tracking("5_it_l_m");
            }
            _isInterstitialLoadEnd = true;
        }

        private void OnNativeAfterInterstitialChangeStatus(AdStatus adStatus)
        {
            if (adStatus == AdStatus.Loaded)
            {
                SplashTracking.Tracking("5_naf_l_e");
                CallLoadNativePreload();
            }
            if (adStatus == AdStatus.Loaded && _isInterstitialShowing)
            {
                var cancellationToken = this.GetCancellationTokenOnDestroy();
                _ = CallShowingNativeAfterInter(cancellationToken);
            }

            if (adStatus == AdStatus.LoadFailed)
            {
                CallLoadNativePreload();
                SplashTracking.Tracking("5_naf_l_m");
            }
            Debug.Log($"Native After Interstitials Splash OnChangeStatus: {adStatus}");
        }

        private async void CallLoadNativeAfterInterSplash()
        {
            if (string.IsNullOrEmpty(_config.nativeAfterInterId))
            {
                _isNativeAfterInterstitialEnd = true;
                return;
            }
            
            var cancellationToken = this.GetCancellationTokenOnDestroy();

#if boostrap_ios && UNITY_IOS
            var layout = ResolveIosLayoutGroupConfig(_config.nativeAfterLayoutGroup);
            if (layout == null)
            {
                SplashLogger.Warn($"Native after interstitials load skipped. Layout group not found group={_config.nativeAfterLayoutGroup}");
                _isNativeAfterInterstitialEnd = true;
                CallLoadNativePreload();
                return;
            }
#else
            var androidCore = AdsLogic.AdsCoreIns as AdCore_MainAndroid;
            var layout = androidCore.ConfigsIns.GetLayoutGroupConfigByGroupName(_config.nativeAfterLayoutGroup);
#endif
            _nativeForceAdRepository = new NativeAndroidAdRepository("naf", _config.nativeAfterInterId, layout);
            _nativeForceAdRepository.OnAdStatusChanged += OnNativeAfterInterstitialChangeStatus;
            
            SplashTracking.Tracking("5_naf_l_s");
            SplashLogger.Log($"Native after interstitials load start group={_config.nativeAfterInterId}");
            var status = await _nativeForceAdRepository.LoadAsync(cancellationToken);
            SplashLogger.Log($"Native after interstitials load status={status}");
            _isNativeAfterInterstitialEnd = true;
        }
        
        private async UniTask ShowingInterstitialAndNativeAfterInter()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            
            if (!_interstitialAdRepository.IsReady())
            {
                SplashLogger.Log($"Interstitial {_interstitialAdRepository.Position} load failed skipped");
                
                if (!_isNativeAfterInterstitialEnd)
                    await UniTask.WaitUntil(() => _isNativeAfterInterstitialEnd, cancellationToken: cancellationToken);
                await CallShowingNativeAfterInter(cancellationToken);
                return;
            }
            
            SplashLogger.Log($"Interstitial {_interstitialAdRepository.Position} splash showing isNativeAfterInterstitialEnd={_isNativeAfterInterstitialEnd}");
            SplashTracking.Tracking($"5_{_interstitialAdRepository.Position}_s_s");
            
            _isInterstitialShowing = true;
            
            if (_isNativeAfterInterstitialEnd)
                await UniTask.WhenAll(_interstitialAdRepository.ShowAsync(cancellationToken), CallShowingNativeAfterInter(cancellationToken));
            else
                await _interstitialAdRepository.ShowAsync(cancellationToken);
            SplashLogger.Log($"Interstitial {_interstitialAdRepository.Position} splash showing completed");

            if (_isCallShowNativeAfterInterstitial)
            {
                SplashLogger.Log($"Wait show native after interstitials splash showing");
                await UniTask.WaitUntil(() => _isCallShowNativeAfterInterstitialEnd, cancellationToken: cancellationToken);
                SplashLogger.Log("Native after interstitials splash showing completed");
            }
            
            SplashTracking.Tracking($"5_{_interstitialAdRepository.Position}_s_e");
        }

        private async UniTask CallShowingNativeAfterInter(CancellationToken cancellationToken)
        {
            if (_isCallShowNativeAfterInterstitial)
            {
                SplashLogger.Log($"Native After Inter skipped by is caller");
                return;
            }
            if(_nativeForceAdRepository == null)
            {
                SplashLogger.Log($"Native After Inter skipped by not initialize");
                return;
            }

            if (!_nativeForceAdRepository.IsReady())
            {
                SplashLogger.Log($"Native After Inter {_nativeForceAdRepository.Position} load failed skipped status={_nativeForceAdRepository.Status}");
                return;
            }
            _isCallShowNativeAfterInterstitial = true;
            
            SplashLogger.Log($"Native after interstitials {_nativeForceAdRepository.Position} splash showing");
            SplashTracking.Tracking($"5_{_nativeForceAdRepository.Position}_s_s");
            await _nativeForceAdRepository.ShowAsync(cancellationToken);
            SplashLogger.Log($"Native after interstitials {_nativeForceAdRepository.Position} splash showing completed");
            SplashTracking.Tracking($"5_{_nativeForceAdRepository.Position}_s_e");
            
            _isCallShowNativeAfterInterstitialEnd = true;
        }

        private async UniTask WaitNativeSplashAndShowing()
        {
            if (string.IsNullOrEmpty(_config.nativeSplashGroup))
            {
                _isNativeSplashLoadEnd = true;
                return;
            }
            
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            _nativeSplashRepository = new PopupAdRepository(_config.nativeSplashGroup, "na", _config.nativeSplashLoadTimeOut, _config.nativeSplashShowTime);
            SplashTracking.Tracking("5_na_l_s");
            SplashLogger.Log($"Native splash load start group={_config.nativeSplashGroup}");
            
            var status = await _nativeSplashRepository.LoadAsync(cancellationToken);
            _isNativeSplashLoadEnd = true;
            if (status == AdStatus.Loaded)
            {
                SplashLogger.Log("Native splash showing");
                SplashTracking.Tracking("5_na_l_e");
                SplashTracking.Tracking("5_na_s_s");

                if (_nativeSplashRepository.AdSourceAdapterClassName.ToLower().Contains(_admobAdapterRegex) && _config.enableAdmobLayout)
                    await _nativeSplashRepository.ShowAsync(_nativeSplashAdmobLayout, cancellationToken);
                else
                    await _nativeSplashRepository.ShowAsync(_nativeSplashLayout, cancellationToken);
                SplashTracking.Tracking("5_na_s_e");
                SplashLogger.Log("Native splash shown & completed");
            }
            else
            {
                SplashTracking.Tracking("5_na_l_m");
            }
        }
        
        private async void CallLoadNativePreload()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            
            if(!FirstSessionData.IsFirstOpen) return;
            
            var rawConfig = NetCallerAPI.RemoteConfig_GetCustom("intro_config");
            
            if(string.IsNullOrEmpty(rawConfig)) return;
            
            var introConfig = JsonConvert.DeserializeObject<IntroConfigV2>(rawConfig);

            var stringPreload = "";
            if (introConfig.Configs.Length > 0)
                stringPreload = introConfig.Configs[0].AdConfig.GroupName;
            else
                stringPreload = introConfig.Profile.PopupConfig;
            /*if (!IntroBoostrapV2.IsCompleteIntro(IntroBoostrapV2.ProfileIntroKey))
            {
                stringPreload = introConfig.Profile.PopupConfig;
            }
            else if (!IntroBoostrapV2.IsCompleteIntro(IntroBoostrapV2.AvatarIntroKey))
            {
                stringPreload = introConfig.Avatar.PopupConfig;
            }*/
            
            if(string.IsNullOrEmpty(stringPreload)) return;
            
            _nativePreloadRepository = new PopupAdRepository(stringPreload,"nap", 100, 0);
            SplashTracking.Tracking("5_nap_l_s");
            SplashLogger.Log($"Native preload load start group={stringPreload}");
            
            var status = await _nativePreloadRepository.LoadAsync(cancellationToken);
            SplashLogger.Log($"Native preload load status={status}");
            if (status == AdStatus.Loaded)
            {
                SplashTracking.Tracking("5_nap_l_e");
            }
            else
            {
                SplashTracking.Tracking("5_nap_l_m");
            }
            _isNativePreloadLoadEnd = true;
        }

        private async UniTask Loading(float value, float time)
        {
            var current = _loadingProgress;
            
            _loadingTweener?.Kill();
            _loadingTweener = DOVirtual.Float(current, value, time, (newProgress) =>
            {
                onProgressChanged?.Invoke(newProgress);
                _loadingProgress = newProgress;
            }).SetUpdate(false);
            await _loadingTweener.ToUniTask();
        }

        private async UniTask WaitFirebaseInitialize()
        {
            await UniTask.WhenAny(UniTask.WaitUntil(() => RemoteConfig.Ins.IsFirebaseInitialized));
        }

#if boostrap_ios && UNITY_IOS
        private static LayoutGroupConfig ResolveIosLayoutGroupConfig(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return null;

            var iosCore = AdsLogic.AdsCoreIns as AdCore_MainIOS;
            return iosCore?.ConfigsIns?.GetLayoutGroupConfigByGroupName(groupName);
        }
#endif
    }
}
