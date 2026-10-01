using System;
using AppBootstrap.Splash;
using BG_Library.NET.AdCore.MainAndroid;
#if boostrap_ios && UNITY_IOS
using BG_Library.NET.AdCore.MainIOS;
#endif
using BG_Library.NET.AdSystem;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaySceneLoader : MonoBehaviour
{
    public string sceneName;
    public GameObject loadingScreen;
    private PlaySceneConfig _config;
    private static NativeAndroidAdRepository _nativeForceAdRepository;
    private AsyncOperation _loadSceneAsync;
    
    private bool _isFirstInit;
    
    private void Awake()
    {
        _config = JsonUtility.FromJson<PlaySceneConfig>(NetCallerAPI.RemoteConfig_GetCustom("play_scene_config"));
#if boostrap_ios && UNITY_IOS
        var layout = ResolveIosLayoutGroupConfig(_config.layoutGroup);
        if (layout == null)
        {
            SplashLogger.Warn($"Play scene native load skipped. Layout group not found group={_config.layoutGroup}");
            return;
        }
#else
        var androidCore = AdsLogic.AdsCoreIns as AdCore_MainAndroid;
        var layout = androidCore.ConfigsIns.GetLayoutGroupConfigByGroupName(_config.layoutGroup);
#endif
        _nativeForceAdRepository ??= new NativeAndroidAdRepository("play_scene", _config.adUnitId, layout, false);
    }

    public async void LoadToPlayScene()
    {
        loadingScreen.SetActive(true);
        await UniTask.WhenAll(
            WaitLoadAd(), 
            WaitLoadScene());

#if boostrap_ios && UNITY_IOS
        if (_nativeForceAdRepository != null && _nativeForceAdRepository.IsReady())
#else
        if (_nativeForceAdRepository.IsReady())
#endif
        {
            var cancelToken = this.GetCancellationTokenOnDestroy();
            await _nativeForceAdRepository.ShowAsync(cancelToken);
        }
        _loadSceneAsync.allowSceneActivation = true;
    }

    private async UniTask WaitLoadAd()
    {
#if boostrap_ios && UNITY_IOS
        if(string.IsNullOrEmpty(_config.adUnitId) || _nativeForceAdRepository == null || _nativeForceAdRepository.IsReady()) return;
#else
        if(string.IsNullOrEmpty(_config.adUnitId) || _nativeForceAdRepository.IsReady()) return;
#endif
        
        var cancelToken = this.GetCancellationTokenOnDestroy();
        await UniTask.WhenAny(
            _nativeForceAdRepository.LoadAsync(cancelToken),
            UniTask.Delay(TimeSpan.FromSeconds(_config.loadAdTimeOut), DelayType.DeltaTime, cancellationToken: cancelToken));
    }

    private async UniTask WaitLoadScene()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f), DelayType.DeltaTime);
        _loadSceneAsync = SceneManager.LoadSceneAsync(sceneName);
        _loadSceneAsync.allowSceneActivation = false;
        await UniTask.WaitUntil(() => _loadSceneAsync.progress >= 0.9f);
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

[System.Serializable]
public class PlaySceneConfig
{
    public string adUnitId;
    public string layoutGroup;
    public float loadAdTimeOut;
}
