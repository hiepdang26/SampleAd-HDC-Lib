using System;
using System.Linq;
using System.Threading;
using AppBootstrap.Splash;
using BG_Library.Common;
using BG_Library.NET;
using BG_Library.NET.API;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AppBootstrap.Intro
{
    public class IntroBoostrapV2 : MonoBehaviour
    {
        public const string AvatarIntroKey = "avatar";
        public const string ProfileIntroKey = "profile";
        
        public View[] views;
        public IntroViewProfile introViewProfile;
        public IntroViewAvatar introViewAvatar;
        private IntroConfigV2 _introConfig;

        private CancellationTokenSource _showAdCts;

        private async void Start()
        {
            // NetCallerAPI.AR_InitManually();
            SplashTracking.Tracking("7_start_intro");
            SplashLogger.Log("Intro V2 start");

            var rawConfig = NetCallerAPI.RemoteConfig_GetCustom("intro_config");
            SplashLogger.Log($"Intro remote config loaded raw={rawConfig}");

            _introConfig = JsonConvert.DeserializeObject<IntroConfigV2>(rawConfig);
            SplashLogger.Log($"Intro config parsed count={_introConfig.Configs?.Length ?? 0}");

            var index = 0;
            if (_introConfig.Configs != null)
            {
                foreach (var intro in _introConfig.Configs)
                {
                    SplashTracking.Tracking($"7_start_intro_{index}");
                    SplashLogger.Log($"Intro [{index}] start type={intro.IntroType} interval={intro.IntroInterval}");

                    var viewSelect = views[0];
                    var isCompleteIntro = IsCompleteIntro(index);
                    if (intro.EnableCompleteIntro && isCompleteIntro) continue;
                    foreach (var view in views)
                    {
                        var canView = view.CanRendererConfig(intro);
                        if (canView) viewSelect = view;
                        view.gameObject.SetActive(canView);
                    }

                    _showAdCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

                    if (index + 1 < _introConfig.Configs.Length)
                    {
                        var nextIntro = _introConfig.Configs[index + 1];
                        DelayAndPreloadAd(nextIntro, intro.TimeDelayPreload);
                    }
                    else
                    {
                        if(!IsCompleteIntro(_introConfig.Configs.Length)) DelayAndPreloadAdProfile(intro.TimeDelayPreload);
                    }

                    if (intro.IntroType != IntroType.FORCE_AD)
                    {
                        ShowAd(viewSelect, intro, _showAdCts.Token);
                        SplashLogger.Log($"Intro [{index}] view selected={viewSelect.GetType().Name}");
                        viewSelect.RendererView(intro);
                        await UniTask.WaitUntil(() => viewSelect.IsCompleteView());
                        SplashLogger.Log($"Intro [{index}] view completed={viewSelect.GetType().Name}");
                        CancelPendingShowAd();
                    }
                    else if (intro.IntroType == IntroType.FORCE_AD)
                    {
                        if (intro.AdConfig.PreloadType == PreloadType.ForceAd)
                            await ShowForceAdAndClose(intro);
                        else
                            ShowAd(viewSelect, intro, _showAdCts.Token);
                    }
                    HideAd(intro);
                    viewSelect.gameObject.SetActive(false);
                    CompleteIntro(index);
                    index++;
                }
            }

            await StartProfileIntro();
            await StartAvatarIntro();
            
            SceneManager.LoadScene(_introConfig.nextSceneName);
            SplashTracking.Tracking($"7_end_intro");
            SplashLogger.Log("Intro V2 done");
        }

        private async UniTask StartProfileIntro()
        {
            if(IsCompleteIntro(ProfileIntroKey) || !_introConfig.Profile.isEnable) return;
            
            SplashTracking.Tracking($"7_start_intro_{_introConfig.Configs.Length}");
            SplashLogger.Log($"Intro start profile");
            introViewProfile.gameObject.SetActive(true);
            introViewProfile.RendererView(_introConfig.Profile, _introConfig.Avatar.PopupConfig);
            
            var showAdCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            if (_introConfig.Profile.PreloadPopup)
            {
                NetCallerAPI.PU_InitManually(_introConfig.Avatar.PopupConfig);
            }
            
            ShowOrWaitAdReadyToShowPopup(_introConfig.Profile.PopupConfig, introViewProfile.layout, showAdCts.Token);
            await UniTask.WaitUntil(() => introViewProfile.IsCompleteView(), cancellationToken: showAdCts.Token);
            CompleteIntro(ProfileIntroKey);
            showAdCts.Cancel();
            showAdCts.Dispose();
            SplashLogger.Log($"Intro end profile");
            introViewProfile.gameObject.SetActive(false);
        }

        private async UniTask StartAvatarIntro()
        {
            if(IsCompleteIntro(AvatarIntroKey) || !_introConfig.Profile.isEnable) return;
            
            SplashTracking.Tracking($"7_start_intro_{_introConfig.Configs.Length + 1}");
            SplashLogger.Log($"Intro start avatar");
            introViewAvatar.gameObject.SetActive(true);
            introViewAvatar.RendererView(_introConfig.Avatar);
            
            var showAdCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            
            ShowOrWaitAdReadyToShowPopup(_introConfig.Avatar.PopupConfig, introViewAvatar.layout, showAdCts.Token);
            await UniTask.WaitUntil(() => introViewAvatar.IsCompleteView(), cancellationToken: showAdCts.Token);
            CompleteIntro(AvatarIntroKey);
            showAdCts.Cancel();
            showAdCts.Dispose();
            SplashLogger.Log($"Intro end avatar");
            // introViewAvatar.gameObject.SetActive(false);
        }

        private async UniTask ShowForceAdAndClose(IntroConfig introConfig)
        {
            var groupName = introConfig.AdConfig.GroupName;
            SplashLogger.Log($"Intro show ad {groupName} ready={NetCallerAPI.FA_AbleToShow(groupName)}");
            if (NetCallerAPI.FA_AbleToShow(groupName))
            {
                var onCloseAd = false;
                NetCallerAPI.FA_Show(groupName);
                var func = new Action<AdInfo>((AdInfo) =>
                {
                    if (AdInfo.group.Equals(groupName))
                        onCloseAd = true;
                });
                NetEventSystem.OnFsClosed += func;
                await UniTask.WaitUntil(() => onCloseAd);
                NetEventSystem.OnFsClosed -= func;
            }
        }

        private async void ShowOrWaitAdReadyToShowPopup(string groupName, PULayout layout, CancellationToken token)
        {
            if (!NetCallerAPI.PU_AbleToShow(groupName))
            {
                try
                {
                    await UniTask.WaitUntil(() => NetCallerAPI.PU_AbleToShow(groupName), cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    SplashLogger.Log($"{groupName} wait canceled, skip showing");
                    return;
                }
            }
            
            SplashLogger.Log($"{groupName} ready after waiting, showing");
            
            NetCallerAPI.PU_UpdatePos(groupName, layout);
            NetCallerAPI.PU_Show(groupName);
        }

        private void OnDestroy()
        {
            CancelPendingShowAd();
        }

        private async void DelayAndPreloadAd(IntroConfig intro, float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.DeltaTime);
            PreloadAd(intro);
        }
        
        private async void DelayAndPreloadAdProfile(float delay)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), DelayType.DeltaTime);
            NetCallerAPI.PU_InitManually(_introConfig.Profile.PopupConfig);
        }

        private void CancelPendingShowAd()
        {
            if (_showAdCts == null) return;

            _showAdCts.Cancel();
            _showAdCts.Dispose();
            _showAdCts = null;
        }

        private void HideAd(IntroConfig config)
        {
            if (config.AdConfig.PreloadType == PreloadType.Popup)
            {
                SplashLogger.Log($"Popup {config.AdConfig.GroupName} hide");
                NetCallerAPI.PU_Hide(config.AdConfig.GroupName);
            }
        }

        private void ShowAd(View view, IntroConfig config, CancellationToken token)
        {
            var showConfig = config.AdConfig;
            if (showConfig.PreloadType != PreloadType.Popup && showConfig.PreloadType != PreloadType.ForceAd)
            {
                return;
            }

            if (IsAdReady(showConfig))
            {
                SplashLogger.Log($"{showConfig.PreloadType} {showConfig.GroupName} ready, showing");
                ShowAdNow(view, showConfig);
            }
            else
            {
                WaitAndShowAd(view, showConfig, token).Forget();
            }
        }

        private async UniTaskVoid WaitAndShowAd(View view, AdConfig showConfig, CancellationToken token)
        {
            SplashLogger.Log($"{showConfig.PreloadType} {showConfig.GroupName} not ready, waiting...");

            try
            {
                await UniTask.WaitUntil(() => IsAdReady(showConfig), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                SplashLogger.Log($"{showConfig.PreloadType} {showConfig.GroupName} wait canceled, skip showing");
                return;
            }

            SplashLogger.Log($"{showConfig.PreloadType} {showConfig.GroupName} ready after waiting, showing");
            ShowAdNow(view, showConfig);
        }

        private static bool IsAdReady(AdConfig showConfig)
        {
            return showConfig.PreloadType == PreloadType.Popup
                ? NetCallerAPI.PU_AbleToShow(showConfig.GroupName)
                : NetCallerAPI.FA_AbleToShow(showConfig.GroupName);
        }

        private static void ShowAdNow(View view, AdConfig showConfig)
        {
            if (showConfig.PreloadType == PreloadType.Popup)
            {
                NetCallerAPI.PU_UpdatePos(showConfig.GroupName, view.PuLayout);
                NetCallerAPI.PU_Show(showConfig.GroupName);
            }
            else
            {
                NetCallerAPI.FA_Show(showConfig.GroupName);
            }
        }

        private void PreloadAd(IntroConfig config)
        {
            var preloadConfig = config.AdConfig;
            if (preloadConfig.PreloadType == PreloadType.Popup)
            {
                SplashLogger.Log($"Popup {preloadConfig.GroupName} preload");
                NetCallerAPI.PU_InitManually(preloadConfig.GroupName);
            }
            else if (preloadConfig.PreloadType == PreloadType.ForceAd)
            {
                SplashLogger.Log($"ForceAd {preloadConfig.GroupName} preload");
                NetCallerAPI.FA_InitManually(preloadConfig.GroupName);
            }
        }

        private bool IsCompleteIntro(int index)
        {
            return PlayerPrefs.GetInt($"intro_{index}", 0) == 1;
        }

        public static bool IsCompleteIntro(string introName)
        {
            return PlayerPrefs.GetInt($"intro_{introName}", 0) == 1;
        }

        public void CompleteIntro(string introName)
        {
            PlayerPrefs.SetInt($"intro_{introName}", 1);
            PlayerPrefs.Save();
        }
        
        private void CompleteIntro(int index)
        {
            PlayerPrefs.SetInt($"intro_{index}", 1);
            PlayerPrefs.Save();
        }
    }
}