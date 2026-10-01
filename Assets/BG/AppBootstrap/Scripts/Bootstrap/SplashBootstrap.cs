using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BG_Library.NET.AdSystem;
using CountryRegionCheck;
using DG.Tweening;
using Firebase;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AppBootstrap.Splash
{
    [DefaultExecutionOrder(-1000)]
    public sealed class SplashBootstrap : MonoBehaviour
    {
        private const string FirstOpenCompletedKey = "splash.first_open.completed";

        [SerializeField] private SplashLoadingView _view;
        [SerializeField] private PULayout _puLayoutTemplate;
        [SerializeField] private AdStorageService _adStorageService;
        [SerializeField] private GameObject _noInternetPopup;
        [SerializeField] private int _networkTimeoutSeconds = 3;

        [ShowInInspector, ReadOnly] private SplashConfig _splashConfig;

        private ISplashNetworkService _networkService;
        private ISplashSdkInitializationService _sdkInitializationService;
        private IConfigProvider _configProvider;
        private ISplashAdsRunner _adsRunner;
        private ISplashSceneTransitionService _sceneTransitionService;
        public static bool IsFirstOpenSession;

        public static string TrackingPrefix => IsFirstOpenSession ? "fo" : "ff";

        private void Awake()
        {
            IsFirstOpenSession = PlayerPrefs.GetInt(FirstOpenCompletedKey, 0) == 0;
            MarkFirstOpenCompleted();
            Installer();
        }

        private async void Start()
        {
            try
            {
                SplashLogger.Log($"Bootstrap start firstOpenSession={IsFirstOpenSession}");
                await WaitFirebaseInitializeOrTimeOut();
                await WaitNetworkConnection();
                await WaitSdkInitialization();
                await UpdateRemoteSettingsAsync();
                await HandleSceneTransitionAsync();

                SplashLogger.Log("Bootstrap completed current splash flow");
            }
            catch (OperationCanceledException)
            {
                SplashLogger.Warn("Bootstrap canceled");
            }
            catch (Exception ex)
            {
                SplashLogger.Error($"Bootstrap failed: {ex}");
                throw;
            }
        }

        private async UniTask WaitFirebaseInitializeOrTimeOut()
        {
            await UniTask.WhenAny(
                UniTask.WaitUntil(() => RemoteConfig.Ins.IsFirebaseInitialized),
                UniTask.Delay(TimeSpan.FromSeconds(5)));
        }

        private void Installer()
        {
            _networkService = new SplashNetworkService(new NetworkDetected(_networkTimeoutSeconds));
            _sdkInitializationService = new SplashSdkInitializationService();
            _configProvider = new RemoteConfigProvider();
            _sceneTransitionService = new SplashSceneTransitionService();

            if (_adStorageService == null)
            {
                SplashLogger.Warn("AdStorageService asset is missing, creating runtime instance");
                _adStorageService = ScriptableObject.CreateInstance<AdStorageService>();
            }

            IAdStorage adStorage = _adStorageService;
            adStorage.Initialize();

            var handlers = new List<ISplashAdHandler>
            {
                new SplashPUAdHandler(),
                new SplashFAAdHandler()
            };

            var splashContext = new SplashAdShowContext(_puLayoutTemplate);
            _adsRunner = new SplashAdsRunner(handlers, splashContext, adStorage, _configProvider);
        }

        private async UniTask WaitNetworkConnection()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            FirebaseAnalytics.LogEvent($"{TrackingPrefix}_1_it");
            _view?.SetRealLoadingText("System checking for Internet Connection");
            _view?.SetProgressInstant(0.1f);
            TryConnectInternet();
            await UniTask.WaitUntil(() => _networkService.GetNetworkStatus().HasInternet,
                cancellationToken: cancellationToken);
            FirebaseAnalytics.LogEvent($"{TrackingPrefix}_1_it_d");
        }

        public void TryConnectInternet()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            _noInternetPopup.SetActive(false);
            _networkService.WaitUntilInternetAvailableAsync((status) =>
            {
                if (!status.IsInternetAvailable)
                {
                    _noInternetPopup.SetActive(true);
                    _view?.SetRealLoadingText("No internet. Turn it on for the best gameplay experience.");
                    FirebaseAnalytics.LogEvent($"{TrackingPrefix}_1_w_it_f");
                }
            }, cancellationToken).Forget();
        }

        private async UniTask WaitSdkInitialization()
        {
            // FirebaseAnalytics.LogEvent($"{TrackingPrefix}_2_sdk");
            _view?.SetRealLoadingText("System downloading game content");
            _view?.StartFakeLoading(0.1f, 0.2f, 2f);
            await _sdkInitializationService.WaitUntilReadyAsync(this.GetCancellationTokenOnDestroy());
            await UniTask.Delay(500, DelayType.DeltaTime);
            _view?.StopLoadingAndCompleteLoading("Game content downloaded");
            FirebaseAnalytics.LogEvent($"{TrackingPrefix}_2_sdk_d");
        }

        private async UniTask UpdateRemoteSettingsAsync()
        {
            FirebaseAnalytics.LogEvent($"{TrackingPrefix}_3_ads");

            _view?.SetRealLoadingText("Updating splash remote settings");
            _splashConfig = _configProvider.LoadRemoteConfig<SplashConfig>("splash_config");
            if (_splashConfig == null)
            {
                SplashLogger.Warn("Splash config is null");
                return;
            }

            _view?.StartFakeLoading(0.2f, 0.9f, Mathf.Max(1f, _splashConfig.seconds));
            SplashLogger.Log(
                $"Splash config parsed seconds={_splashConfig.seconds} adsCount={_splashConfig.adEntryLoadings.Length}");
            await _adsRunner.RunAsync(_splashConfig, this.GetCancellationTokenOnDestroy());
            _view?.StopLoadingAndCompleteLoading("Remote settings updated");
            SplashLogger.Log("UpdateRemoteSettings complete");
            FirebaseAnalytics.LogEvent($"{TrackingPrefix}_3_ads_d");
        }


        private async UniTask HandleSceneTransitionAsync()
        {
            FirebaseAnalytics.LogEvent($"{TrackingPrefix}_4_next_scene");
            _view?.SetRealLoadingText("Opening next scene");
            _view?.StartFakeLoading(0.9f, 1.0f, 1.5f);
            var deferredTrigger = _adsRunner.PendingDeferredTrigger;

            if (deferredTrigger == AdShowTriggerType.OnCloseAndNextScene)
            {
                SplashLogger.Log("Handling deferred app launcher on close and next scene");
                await _adsRunner.CheckAndShowDeferredAsync(AdShowTriggerType.OnCloseAndNextScene,
                    this.GetCancellationTokenOnDestroy());
            }

            bool keepAliveForNextSceneStarted = deferredTrigger == AdShowTriggerType.OnNextSceneStarted;
            if (keepAliveForNextSceneStarted)
            {
                DontDestroyOnLoad(gameObject);
            }

            _adsRunner.OnEndSceneHideAd();

            string targetSceneName = ResolveNextSceneName();
            bool loadedNextScene =
                await _sceneTransitionService.LoadNextSceneAsync(targetSceneName, this.GetCancellationTokenOnDestroy());
            if (!loadedNextScene)
            {
                _view?.StopLoadingAndCompleteLoading("Next scene load failed");
                return;
            }

            if (keepAliveForNextSceneStarted)
            {
                SplashLogger.Log("Handling deferred app launcher after next scene started");
                await UniTask.NextFrame();
                await _adsRunner.CheckAndShowDeferredAsync(AdShowTriggerType.OnNextSceneStarted,
                    this.GetCancellationTokenOnDestroy());
                Destroy(gameObject);
            }

            FirebaseAnalytics.LogEvent($"{TrackingPrefix}_4_next_scene_d");
            _view?.StopLoadingAndCompleteLoading("Next scene opened");
        }

        private void HandlerEndSceneDestroyAllAds()
        {
        }

        private string ResolveNextSceneName()
        {
            string resolvedSceneName = _splashConfig?.nextSceneName;
            if (IsFirstOpenSession && !string.IsNullOrWhiteSpace(_splashConfig?.firstOpenNextSceneName))
            {
                resolvedSceneName = _splashConfig.firstOpenNextSceneName;
            }

            SplashLogger.Log(
                $"Resolved splash next scene firstOpenSession={IsFirstOpenSession} sceneName={resolvedSceneName}");
            return resolvedSceneName;
        }

        private void MarkFirstOpenCompleted()
        {
            PlayerPrefs.SetInt(FirstOpenCompletedKey, 1);
            PlayerPrefs.Save();
        }
    }
}