using System;
using System.Collections.Generic;
using System.Linq;
using AppBootstrap.Splash;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NativeBannerLoader : MonoBehaviour
{
    public bool _autoInitialize = true;
    public PULayout _nativeLayout;
    private PopupAdRepository _firstBannerRepository;
    private Dictionary<NativeBannerConfig, PopupAdRepository>  _popupAdRepositories = new Dictionary<NativeBannerConfig, PopupAdRepository>();
    private FirstNativeBannerConfig _config;
    private float cappingTime = 2;
    private string _lastNativeBannerShowing;
    private bool _isInitialized;
    
    private readonly string _metaAdapterRegex = "meta";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        WaitFirebaseAndInitialize();
    }

    public async void WaitFirebaseAndInitialize()
    {
        await UniTask.WaitUntil(() => NetCallerAPI.RemoteConfig_IsFetchComplete);
        
        var rawConfig = NetCallerAPI.RemoteConfig_GetCustom("first_native_banner_config");
        _config = JsonUtility.FromJson<FirstNativeBannerConfig>(rawConfig);
        
        if (_config == null || _config.bannerLoops == null)
        {
            SplashLogger.Warn($"NativeBanner config invalid or empty, raw={rawConfig}");
            return;
        }
        
        if (!_isInitialized)
        {
            if (_autoInitialize && SceneManager.GetActiveScene().name.Equals(_config.activeSceneName))
            {
                Initialize();
                _isInitialized = true;
            }
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (!_isInitialized && scene.name.Equals(_config.activeSceneName))
        {
            Initialize();
            _isInitialized = true;
        }
    }

    public async void Initialize()
    {
        SplashLogger.Log("NativeBanner initialize start");

        if (_config == null)
        {
            return;
        }

        foreach (var nativeBannerConfig in _config.bannerLoops)
        {
            var loopRepository = new PopupAdRepository(nativeBannerConfig.groupName, nativeBannerConfig.groupName, nativeBannerConfig.timeOut, 0, false);
            _popupAdRepositories.Add(nativeBannerConfig, loopRepository);
        }

        SplashLogger.Log($"NativeBanner initialized loops={_config.bannerLoops.Length}");
        RunningBannerReload();
    }

    private async void RunningBannerReload()
    {
        int bannerCount = 0;
        while (HasBannerWaitShow())
        {
            var nativeBannerConfig = _config.bannerLoops[bannerCount];
            var loopRepository = _popupAdRepositories[nativeBannerConfig];
            if (nativeBannerConfig.isEnabled)
            {
                if (nativeBannerConfig.adShowCount > 0 || nativeBannerConfig.adShowCount == -1)
                    await LoadAndShowNativeBanner(nativeBannerConfig, loopRepository);
            }
            bannerCount++;
            if (bannerCount >= _config.bannerLoops.Length) bannerCount = 0;
        }
        SplashLogger.Log("NativeBanner reload loop ended (no banner waiting)");
    }

    private async UniTask LoadAndShowNativeBanner(NativeBannerConfig config, PopupAdRepository repository)
    {
        var cancel = this.GetCancellationTokenOnDestroy();
        SplashLogger.Log($"NativeBanner load start group={config.groupName}");
        var status = await repository.LoadAsync(cancel);

        if (!string.IsNullOrEmpty(_lastNativeBannerShowing))
        {
            SplashLogger.Log($"NativeBanner hide previous group={_lastNativeBannerShowing}");
            NetCallerAPI.PU_Hide(_lastNativeBannerShowing);
        }

        if (status == AdStatus.Loaded)
        {
            cappingTime = 2;
            _ = repository.ShowAsync(_nativeLayout, cancel);
            var timeShowing = config.defaultTimeConfig;
            if (repository.AdSourceAdapterClassName.ToLower().Contains(_metaAdapterRegex))
            {
                timeShowing = config.metaTimeConfig;
            }
            if (config.adShowCount > 0) config.adShowCount--;
            _lastNativeBannerShowing = config.groupName;
            SplashLogger.Log($"NativeBanner loaded group={config.groupName} source={repository.AdSourceAdapterClassName} showTime={timeShowing} remain={config.adShowCount}");
            await UniTask.Delay(TimeSpan.FromSeconds(timeShowing), DelayType.DeltaTime, cancellationToken: cancel);
        }
        else if(status == AdStatus.LoadFailed)
        {
            SplashLogger.Warn($"NativeBanner load failed group={config.groupName} retryIn={cappingTime}s");
            await UniTask.Delay(TimeSpan.FromSeconds(cappingTime), DelayType.DeltaTime, cancellationToken: cancel);
            cappingTime = Mathf.Min(cappingTime * 2, 64);
        }
    }

    private bool HasBannerWaitShow()
    {
        return _popupAdRepositories.Keys.ToList().Exists(banner => banner.isEnabled && (banner.adShowCount > 0 || banner.adShowCount == -1));
    }
}

[System.Serializable]
public class FirstNativeBannerConfig
{
    public string activeSceneName;
    public NativeBannerConfig[] bannerLoops;
}

[System.Serializable]
public class NativeBannerConfig
{
    public bool isEnabled;
    public string groupName;
    public float timeOut;
    public int adShowCount;
    
    public float defaultTimeConfig;
    public float metaTimeConfig;
}
