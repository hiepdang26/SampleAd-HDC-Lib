using System;
using AppBootstrap.Splash;
using BG_Library.Common;
using BG_Library.NET.AdCore.MainAndroid;
#if boostrap_ios && UNITY_IOS
using BG_Library.NET.AdCore.MainIOS;
#endif
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Channel = BG_Library.NET.Tracking.Channel;

namespace BG_Library.NET.AdSystem
{
    public class AppResumeSystem : MonoBehaviour
    {
        public bool IgnoreAd;

        private AdCoreBase core;
        private AdSystemConfigs.AppResumeChannelConfig configs;

        public void Setup(AdCoreBase _core, AdSystemConfigs.AppResumeChannelConfig _configs)
        {
            core = _core;
            configs = _configs;

            NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "Setup",
                () => $"core={null} enable={(configs != null && configs.IsEnabled)}");
        }

        #region (1) ===== LOGIC =====

        private bool activeBlockTimer;
        private float blockTimerFlag;
        private bool blockAppOpenAfterAds;
#if boostrap_ios && UNITY_IOS
        private bool blockAppOpenAfterAdsSeenPause;
#endif
        private bool firstTimeOpenApp = true;
        private bool isAppResumeShowing;
        private bool isInitialized;

        private System.Action onMediationCompletedHandler;
        private System.Action<AdInfo> onAnyFSBeforeOpenHandler;
        private System.Action<AdInfo> onAnyFSClosedHandler;
        private System.Action<AdInfo> onBannerClickedHandler;

        public static AppResumeSystem Instance;
        private NativeAndroidAdRepository _nativeAndroidAd;

        private void Awake()
        {
            Instance = this;
            
            onAnyFSBeforeOpenHandler = OnAnyFSBeforeOpen;
            onAnyFSClosedHandler = OnAnyFSClosed;
            onBannerClickedHandler = OnBannerClickedHandler;

            NetEventSystem.OnFsBeforeOpen += onAnyFSBeforeOpenHandler;
            NetEventSystem.OnFsClosed += onAnyFSClosedHandler;
            NetEventSystem.OnRectClicked += onBannerClickedHandler;

            onMediationCompletedHandler = () =>
            {
                using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_ar, "Init Auto", () => "OnAdCoreInitCompleted"))
                {
                    if (!ShouldAutoInit)
                    {
                        NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Init Auto Skip", () => "AutoInit=false");
                        return;
                    }

                    NetTrackingSystem.RequestSystemEntry(Channel.AppResume);
                    if (IsDisable)
                    {
                        NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Init Auto Blocked",
                            () => $"IsDisable=true");
                        NetTrackingSystem.RequestSystemFail(Channel.AppResume, DisableTrackingReason);

                        return;
                    }

                    if (!InitializeResumeNativeRepository())
                    {
                        return;
                    }

                    isInitialized = true;
                    NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "Init Auto Done", () => "AR_Initialize");
                   // core.AR_Initialize();
                }
            };

            NetEventSystem.OnAdCoreInitCompleted += onMediationCompletedHandler;

            NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "Awake", () => "bind events",
                d => d.AddKV("firstTimeOpenApp", firstTimeOpenApp));
        }

        private bool InitializeResumeNativeRepository()
        {
#if boostrap_ios && UNITY_IOS
            var iosCore = core as AdCore_MainIOS;
            if (iosCore == null)
            {
                NetFlowDebugSystem.Error(Layer.sys, Module.format_ar, "Init Native Repository Failed",
                    () => $"platform=iOS reason=core_not_ios core={(core == null ? "null" : core.AdCoreName)} layoutGroup={(configs == null ? "(null)" : configs.LayoutGroup)}");
                return false;
            }

            var layout = iosCore.ConfigsIns?.GetLayoutGroupConfigByGroupName(configs.LayoutGroup);
            if (layout == null)
            {
                NetFlowDebugSystem.Error(Layer.sys, Module.format_ar, "Init Native Repository Failed",
                    () => $"platform=iOS reason=layout_not_found core={iosCore.AdCoreName} layoutGroup={configs.LayoutGroup}");
                return false;
            }

            _nativeAndroidAd = new NativeAndroidAdRepository("native_resume", configs.AdUnitId, layout, false, false);
            _nativeAndroidAd.OnAdStatusChanged += OnChangedStats;
            return true;
#else
            var androidCore = core as AdCore_MainAndroid;
            var layout = androidCore.ConfigsIns.GetLayoutGroupConfigByGroupName(configs.LayoutGroup);
            
            _nativeAndroidAd = new NativeAndroidAdRepository("native_resume", configs.AdUnitId, layout, false, false);
            _nativeAndroidAd.OnAdStatusChanged += OnChangedStats;
            return true;
#endif
        }

        private void OnDestroy()
        {
            if (onAnyFSBeforeOpenHandler != null)
                NetEventSystem.OnFsBeforeOpen -= onAnyFSBeforeOpenHandler;

            if (onAnyFSClosedHandler != null)
                NetEventSystem.OnFsClosed -= onAnyFSClosedHandler;

            if (onMediationCompletedHandler != null)
                NetEventSystem.OnAdCoreInitCompleted -= onMediationCompletedHandler;
        }

        private void OnBannerClickedHandler(AdInfo adInfo)
        {
            if(!isInitialized) return;
            if (adInfo.adtype == BG_ConstValue.adtype_bn)
            {
                blockAppOpenAfterAds = true;
#if boostrap_ios && UNITY_IOS
                blockAppOpenAfterAdsSeenPause = false;
#endif
            }
        }

        private void OnAnyFSBeforeOpen(AdInfo _)
        {
            if(!isInitialized) return;
            blockAppOpenAfterAds = true;
#if boostrap_ios && UNITY_IOS
            blockAppOpenAfterAdsSeenPause = false;
#endif

            NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Gate BlockFS",
                () => "OnFsBeforeOpen -> blockAppOpenAfterAds=true");
        }

        private void OnAnyFSClosed(AdInfo _)
        {
#if boostrap_ios && UNITY_IOS
            if (!blockAppOpenAfterAds)
                return;

            if (blockAppOpenAfterAdsSeenPause)
            {
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Gate HoldFS",
                    () => "OnFsClosed after pause=true -> wait pause=false");
                return;
            }

            blockAppOpenAfterAds = false;
            blockAppOpenAfterAdsSeenPause = false;
            NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Gate ClearFS",
                () => "OnFsClosed without app pause -> clear block");
#else
            // blockAppOpenAfterAds = true;
            // NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Gate BlockFS", () => "OnFsBeforeOpen -> blockAppOpenAfterAds=true");
            // NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Gate TimerStart", () => "OnFsClosed -> start 4s unblock");
#endif
        }

        /*private void Update()
        {
            if (!activeBlockTimer) return;

            blockTimerFlag += Time.unscaledDeltaTime;

            if (blockTimerFlag >= 4f)
            {
                blockAppOpenAfterAds = false;
                activeBlockTimer = false;
                blockTimerFlag = 0f;

                NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "Gate TimerDone", () => "unblocked (after 4s)");
            }
        }*/

        private void OnApplicationPause(bool pauseStatus)
        {
            NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "AppPause", () => $"pause={pauseStatus}");

            if (!isInitialized)
            {
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked", () => "isInitialized=false");
                return;
            }
            
            if (blockAppOpenAfterAds)
            {
#if boostrap_ios && UNITY_IOS
                if (pauseStatus)
                {
                    blockAppOpenAfterAdsSeenPause = true;
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked",
                        () => "blockedByRecentFS=true phase=pause holdUntilResume=true");
                    NetTrackingSystem.AutoShowSystemFail(Channel.AppResume, TrackingReason.BlockedByFullscreen);
                    return;
                }

                blockAppOpenAfterAds = false;
                blockAppOpenAfterAdsSeenPause = false;
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked",
                    () => "blockedByRecentFS=true phase=resume clear=true");
                NetTrackingSystem.AutoShowSystemFail(Channel.AppResume, TrackingReason.BlockedByFullscreen);
                return;
#else
                blockAppOpenAfterAds = false;
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked", () => "blockedByRecentFS=true");
                NetTrackingSystem.AutoShowSystemFail(Channel.AppResume, TrackingReason.BlockedByFullscreen);
                return;
#endif
            }

            if (isAppResumeShowing)
            {
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked", () => "isAppResumeShowing=true");
                return;
            }

            if (pauseStatus)
            {
                NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "Resume Load", () => "");
                var cancelToken = this.GetCancellationTokenOnDestroy();
                _ = _nativeAndroidAd.LoadAsync(cancelToken);
            }
            
            /*if (firstTimeOpenApp)
            {
                firstTimeOpenApp = false;
                NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "Resume Skip", () => "firstTimeOpenApp");
                return;
            }*/

            /*using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_ar, "Resume Try", () => "OnApplicationPause(false)"))
            {
                TryShowOnResume();
            }*/
        }

        private void OnChangedStats(AdStatus adStatus)
        {
            if (adStatus == AdStatus.Loaded)
            {
                var cancelToken = this.GetCancellationTokenOnDestroy();
                _ = _nativeAndroidAd.ShowAsync(cancelToken);
            }
            
            if (adStatus == AdStatus.AdDisplayed)
            {
                isAppResumeShowing = true;
            }
            
            if (adStatus == AdStatus.AdClosed)
            {
                isAppResumeShowing = false;
            }
        }

        private void TryShowOnResume()
        {
            string posHint = NetTrackingSystem.PosTargetHint("app_resume");
            NetTrackingSystem.AutoShowSystemEntry(Channel.AppResume, posHint);

            // Guard order is kept 1:1 with your logic.
            if (IsDisable)
            {
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked", () => "IsDisable=true");
                NetTrackingSystem.AutoShowSystemFail(Channel.AppResume, DisableTrackingReason, posHint);
                return;
            }

            if (IgnoreAd)
            {
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked", () => "IgnoreAd=true");
                NetTrackingSystem.AutoShowSystemFail(Channel.AppResume, TrackingReason.Ignored, posHint);
                return;
            }

            if (!IsRemoteConfigReady())
            {
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked", () => "RemoteConfig not ready");
                NetTrackingSystem.AutoShowSystemFail(Channel.AppResume, TrackingReason.RemoteConfigPending, posHint);
                return;
            }

            /*
            if (!core.AR_GetReady())
            {
                NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "Resume Blocked", () => "AR_GetReady=false");
                NetTrackingSystem.AutoShowSystemFail(Channel.AppResume, TrackingReason.NotReady, posHint);
                return;
            }
            */

            NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "Resume Show", () => "core.AR_ShowAd(app_resume)");

            // core.AR_ShowAd("app_resume");
        }

        #endregion

        #region (2) ===== PUBLIC API =====

        public void InitManually()
        {
            using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_ar, "InitManually", () => "call"))
            {
                if (ShouldAutoInit)
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "InitManually Rejected",
                        () => "AutoInit=true");
                    return;
                }

                NetTrackingSystem.RequestSystemEntry(Channel.AppResume);
                if (IsDisable)
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_ar, "InitManually Blocked",
                        () => "IsDisable=true");
                    NetTrackingSystem.RequestSystemFail(Channel.AppResume, DisableTrackingReason);

                    return;
                }

                if (!InitializeResumeNativeRepository())
                {
                    return;
                }
                
                isInitialized = true;
                NetFlowDebugSystem.Log(Layer.sys, Module.format_ar, "InitManually CallCore", () => "AR_Initialize");
                // core.AR_Initialize();
            }
        }

        public void BlockAdResume()
        {
            if(!isInitialized) return;
            blockAppOpenAfterAds = true;
#if boostrap_ios && UNITY_IOS
            blockAppOpenAfterAdsSeenPause = false;
#endif
        }

        #endregion

        #region (3) ===== HELPERS =====

        public bool IsDisable
        {
            get
            {
                if (AdsLogic.IsRemovedAd) return true;
                if (configs == null || !configs.IsEnabled) return true;
                return false;
            }
        }

        private TrackingReason DisableTrackingReason =>
            AdsLogic.IsRemovedAd ? TrackingReason.IapRemoved : TrackingReason.ConfigDisabled;

        private bool ShouldAutoInit => configs != null && configs.AutoInit;

        private bool IsRemoteConfigReady()
        {
            return RemoteConfig.Ins != null && RemoteConfig.Ins.IsDataFetched;
        }

        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder(1000);

            sb.AppendLine("=== AppResumeLogic (AR) Overview ===");

            sb.AppendLine("-- Configs --");
            if (configs == null)
            {
                sb.AppendLine("(null)");
            }
            else
            {
                sb.Append("isEnabled: ").AppendLine(configs.IsEnabled.ToString());
            }

            sb.AppendLine();
            sb.AppendLine("-- Runtime --");
            sb.Append("IgnoreAd: ").AppendLine(IgnoreAd.ToString());

            sb.Append("firstTimeOpenApp: ").AppendLine(firstTimeOpenApp.ToString());

            sb.Append("blockAppOpenAfterAds: ").AppendLine(blockAppOpenAfterAds.ToString());
#if boostrap_ios && UNITY_IOS
            sb.Append("blockAppOpenAfterAdsSeenPause: ").AppendLine(blockAppOpenAfterAdsSeenPause.ToString());
#endif
            sb.Append("activeBlockTimer: ").AppendLine(activeBlockTimer.ToString());
            sb.Append("blockTimerFlag: ").AppendLine(blockTimerFlag.ToString("0.###"));

            sb.AppendLine();
            sb.AppendLine("-- Gates --");
            sb.Append("IsDisable: ").AppendLine(IsDisable.ToString());

            bool rcReady = false;
            try
            {
                rcReady = IsRemoteConfigReady();
            }
            catch
            {
            }

            sb.Append("RemoteConfigReady: ").AppendLine(rcReady.ToString());

            bool ready = false;
            //try { ready = (core != null && core.AR_GetReady()); } catch { }
            //sb.Append("AR_GetReady(): ").AppendLine(ready.ToString());

            sb.AppendLine();
            sb.AppendLine("-- Derived --");

            sb.Append("IsBlockedByFS: ").AppendLine(blockAppOpenAfterAds.ToString());

            if (activeBlockTimer)
            {
                var remain = Mathf.Max(0f, 4f - blockTimerFlag);
                sb.Append("UnblockIn: ").Append(remain.ToString("0.###")).AppendLine("s");
            }
            else
            {
                sb.AppendLine("UnblockIn: (inactive)");
            }

            sb.AppendLine();
            sb.AppendLine("-- Notes --");
            sb.AppendLine("• Group debug: call GetDebugGroup()");

            return sb.ToString();
        }

        public string GetDebugGroup()
        {
            return "null";
            // return core.AR_GetDebugInfo();
        }

        #endregion
    }
}
