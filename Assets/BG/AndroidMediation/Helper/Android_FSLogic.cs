using System.Threading;
using AppBootstrap.Splash;
using BG_Library.Common;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.AdSystem;
using BG_Library.NET.AndroidSDK;
using BG_Library.NET.Debug;

namespace BG_Library.NET.Mediation.Android
{
    public sealed class Android_FSLogic<T>
        where T : Android_FSInfo
    {
        private readonly Android_FSGroupController<T> core;
        private NativeAndroidAdRepository _nativeForceAdRepository;
        private IFSInstance _ad;
        private CancellationToken _cancellationToken;

        private readonly string idKey;

        public bool HasAdInstance => _ad != null;

        public Android_FSLogic(Android_FSGroupController<T> core)
        {
            this.core = core;

            idKey = this.core.Id;
            if (!core.AndroidInterstitials.SwitchToInterstitialAndroid)
            {
                _ad = new FSNativeInstance(new string[] { idKey }, core.LayoutGroup);
            }
            else
            {
                if (core.AndroidInterstitials.IsPreloadAd)
                {
                    _ad = new FSInterstitialPreloadInstance(idKey, core.AndroidInterstitials.BufferSize);
                }
                else
                {
                    _ad = new FSInterstitialInstance(idKey);
                }

                if (core.AndroidInterstitials.UseNativeAfterInterstitial)
                {
                    var androidCore = AdsLogic.AdsCoreIns as AdCore_MainAndroid;
                    var layout = androidCore.ConfigsIns.GetLayoutGroupConfigByGroupName(core.AndroidInterstitials.NativeAfterInterstitialLayout);
                    _nativeForceAdRepository = new NativeAndroidAdRepository("naf", core.AndroidInterstitials.NativeAfterInterstitialId, layout);
                    _nativeForceAdRepository.OnAdStatusChanged += OnNativeAfterInterstitialChangedStatus;
                    _nativeForceAdRepository.OnAdPaid += OnAdPaid;
                }
            }

            NetFlowDebugSystem.Log(Layer.group, Module.android_api_fs, $"Create.Instance {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey} layoutNames={(core.LayoutGroup.Layouts != null && core.LayoutGroup.Layouts.Length > 0 ? $"[{string.Join(",", core.LayoutGroup.Layouts)}]" : "(empty)")}");
        }

        public void RequestAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_fs, $"SDK.Load {core.GroupName}",
                       () => $"adtype={core.Adtype} id={idKey} call=FSInstance.LoadAd()"))
            {
                _ad.LoadAd();
            }
        }

        public bool GetAdReady() => _ad != null && _ad.IsReady();

        public void Show()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_fs, $"SDK.Show {core.GroupName}",
                       () => $"adtype={core.Adtype} id={idKey} call=FSInstance.ShowAd()"))
            {
                _ad.ShowAd();
            }
        }

        public void DestroyAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_fs, $"SDK.Destroy {core.GroupName}",
                       () => $"adtype={core.Adtype} id={idKey} call=FSInstance.DestroyAd()"))
            {
                _ad?.DestroyAd();
                _ad = null;
            }
        }

        private void ShowNativeAfterInterstitial()
        {
            if (!_nativeForceAdRepository.IsReady())
                _ = _nativeForceAdRepository.LoadAsync(CancellationToken.None);
            else
                _ = _nativeForceAdRepository.ShowAsync(CancellationToken.None);
        }
        
        private void OnNativeAfterInterstitialChangedStatus(AdStatus adStatus)
        {
            if (adStatus == AdStatus.Loaded)
            {
                _ = _nativeForceAdRepository.ShowAsync(CancellationToken.None);
            }
        }

        private void OnAdPaid(NativeAdInfo info, NativeAdPaidInfo adPaidInfo)
        {
            UnityMainThreadDispatcher.EnqueueCallback(() =>
            {
                using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_fs, $"CB.Paid {core.GroupName}",
                           () => $"adtype={core.Adtype} id={idKey} adapter={(info != null ? info.mediationAdapter : "")}"))
                {
                    var rev = 0d;
                    var currency = "USD";

                    try
                    {
                        if (adPaidInfo != null)
                        {
                            rev = adPaidInfo.revenueMicros / 1000000d;
                            currency = adPaidInfo.currencyCode;
                        }
                    }
                    catch { }

                    core.OnAdRevenuePaidEvent(rev, currency, info.mediationAdapter);
                }
            });
        }
        
        public void AttachForAdInstance()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Attach {core.GroupName}",
                       () => $"adtype={core.Adtype} id={idKey}"))
            {
                if (string.IsNullOrEmpty(idKey))
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Attach.Fail {core.GroupName}", () => "reason=idKey_empty");
                    return;
                }

                if (_ad == null)
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Attach.Fail {core.GroupName}", () => "reason=ad_null");
                    return;
                }

                // Loaded
                _ad.OnAdLoadedEvent += adInfo =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_fs, $"CB.Loaded {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={idKey} adapter={(adInfo != null ? adInfo.mediationAdapter : "")}"))
                        {
                            core.OnAdLoadedEvent(adInfo.GetInfo(), adInfo.mediationAdapter);
                        }
                    });
                };

                // Load Failed
                _ad.OnAdLoadFailedEvent += (adunit, errorCode, errorMessage) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_fs, $"CB.LoadFailed {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={idKey} errNull={(errorMessage == null)}"))
                        {
                            core.OnAdLoadFailedEvent(errorCode, errorMessage ?? "LoadFailed (null)");
                        }
                    });
                };

                // Displayed
                _ad.OnAdDisplayedEvent += adInfo =>
                {
                    if (core.AndroidInterstitials.UseNativeAfterInterstitial)
                    {
                        ShowNativeAfterInterstitial();
                    }
                    
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_fs, $"CB.Displayed {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={idKey} adapter={(adInfo != null ? adInfo.mediationAdapter : "")}"))
                        {
                            core.OnAdDisplayedEvent(adInfo.mediationAdapter);
                        }
                    });
                };

                // Clicked
                _ad.OnAdClicked += adInfo =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_fs, $"CB.Clicked {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={idKey} adapter={(adInfo != null ? adInfo.mediationAdapter : "")}"))
                        {
                            core.OnAdClickedEvent(adInfo.mediationAdapter);
                        }
                    });
                };

                // Paid
                _ad.OnPaidAdImpressionEvent += (adInfo, adValue) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_fs, $"CB.Paid {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={idKey} adapter={(adInfo != null ? adInfo.mediationAdapter : "")}"))
                        {
                            var rev = 0d;
                            var currency = "USD";

                            try
                            {
                                if (adValue != null)
                                {
                                    rev = adValue.revenueMicros / 1000000d;
                                    currency = adValue.currencyCode;
                                }
                            }
                            catch { }

                            core.OnAdRevenuePaidEvent(rev, currency, adInfo.mediationAdapter);
                        }
                    });
                };

                // Hidden (+ reward for rewarded)
                _ad.OnAdHiddenEvent += adInfo =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_fs, $"CB.Hidden {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={idKey} adapter={(adInfo != null ? adInfo.mediationAdapter : "")}"))
                        {
                            var wasAwaitingTerminalCallback = core.IsAwaitingTerminalShowCallback;
                            core.OnAdHiddenEvent(adInfo.mediationAdapter);

                            // NOTE: giữ nguyên logic của bạn: rewarded => reward at hidden
                            if (wasAwaitingTerminalCallback && core.Info.IsRewarded)
                                core.OnAdReceivedRewardEvent(adInfo.GetInfo());
                        }
                    });
                };

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Attach.OK {core.GroupName}", () => $"id={idKey}");
            }
        }
    }
}
