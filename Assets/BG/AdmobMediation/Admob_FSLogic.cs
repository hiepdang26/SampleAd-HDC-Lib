using BG_Library.Common;
using GoogleMobileAds.Api;
using System;
using UnityEngine;
using BG_Library.NET.Debug;

namespace BG_Library.NET.Mediation.Admob
{
    [Serializable]
    public sealed class Admob_FSLogic<T, API>
        where T : Admob_FSInfo
        where API : IAdmob_FSAccessAPI, new()
    {
        private readonly Admob_FSGroupController<T, API> core;
        private readonly IAdmob_FSAccessAPI api;

        public Admob_FSLogic(Admob_FSGroupController<T, API> core, IAdmob_FSAccessAPI api)
        {
            this.core = core;
            this.api = api;
        }

        public void N_RequestAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Logic.Request {core.GroupName}", () => $"adtype={core.Adtype} id={core.Id}"))
            {
                api.N_DestroyAd();
                // log “đến AccessAPI rồi” (để đối chiếu với AccessAPI log “SDK.Load”)
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.AccessAPI {core.GroupName}", () => "N_RequestAd()");
                api.N_RequestAd(core.Id, OnLoadResult_Normal);
            }
        }

        public void P_StartLoad()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Logic.PreloadStart {core.GroupName}"
                , () => $"adtype={core.Adtype} preloadKey={core.PreloadKey}"))
            {
                if (core.StopLogic == null || core.StopLogic.ShouldIgnore()) return;

                var buf = Mathf.Clamp(core.AdBufferSize <= 0 ? 2 : core.AdBufferSize, 1, 5);

                var cfg = new PreloadConfiguration
                {
                    AdUnitId = core.Id,
                    BufferSize = (uint)buf,
                    Request = new AdRequest(),
                };

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.AccessAPI {core.GroupName}"
                    , () => $"P_Preload(preloadKey={core.PreloadKey} buf={buf})");
                api.P_Preload(core.PreloadKey, cfg, OnAdPreloaded, OnAdFailedToPreload, OnAdsExhausted);
            }
        }

        public bool P_Show(string pos, Action onBeforeAdShow = null, Action onAdShowComplete = null)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Logic.PreloadShow {core.GroupName}"
                , () => $"adtype={core.Adtype} pos={pos} preloadKey={core.PreloadKey}"))
            {
                var ad = api.P_DequeueAd(core.PreloadKey);
                if (ad == null)
                {
                    core.RaiseFsShowFailedPreApi(pos);
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"PreloadShow.Fail {core.GroupName}", () => "reason=DequeueNull");
                    return false;
                }

                if (core.StopLogic != null && core.StopLogic.IsStopped)
                {
                    try { api.P_DestroyAd(core.PreloadKey); } catch { }
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"PreloadShow.Blocked {core.GroupName}", () => "reason=Stopped");
                    return false;
                }

                onBeforeAdShow?.Invoke();
                core.onAdComplete = onAdShowComplete;

                core.RaiseFsBeforeOpen(pos);

                AttachForAdInstance(ad, isPreloaded: true);

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.AccessAPI {core.GroupName}", () => "P_Show(ad)");
                api.P_Show(ad);

                return true;
            }
        }

        private void OnAdPreloaded(string pid, ResponseInfo responseInfo)
        {
            UnityMainThreadDispatcher.EnqueueCallback(() =>
            {
                // callback => flow mới (vì đã nhảy luồng)
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Callback.Preloaded {core.GroupName}", () => $"preloadKey={pid}");
            });
        }

        private void OnAdFailedToPreload(string pid, AdError adError)
        {
            UnityMainThreadDispatcher.EnqueueCallback(() =>
            {
                string err;
                try { err = adError != null ? adError.GetMessage() : "(null)"; }
                catch { err = adError != null ? adError.ToString() : "(null)"; } // NextGen GMA: getMessage() signature mismatch
                NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Callback.FailedToPreload {core.GroupName}", () => $"preloadKey={pid} err={err}");
            });
        }

        private void OnAdsExhausted(string pid)
        {
            UnityMainThreadDispatcher.EnqueueCallback(() =>
            {
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Callback.AdsExhausted {core.GroupName}", () => $"preloadKey={pid}");
            });
        }

        private void OnLoadResult_Normal(object ad, LoadAdError error)
        {
            UnityMainThreadDispatcher.EnqueueCallback(() =>
            {
                using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"Callback.LoadResult {core.GroupName}"
                    , () => $"adNull={(ad == null)} errNull={(error == null)}"))
                {
                    // FAIL (giữ đúng logic detect fail & return)
                    if (error != null || ad == null)
                    {
                        string errMsg;
                        if (error != null)
                        {
                            try { errMsg = error.GetMessage(); }
                            catch { errMsg = error.ToString(); }
                        }
                        else errMsg = "LoadFailed (ad null, error null)";

                        int code;
                        try { code = error != null ? error.GetCode() : int.MinValue; }
                        catch { code = int.MinValue; }
                        core.OnAdLoadFailedEvent(code, errMsg);
                        return;
                    }

                    // SUCCESS
                    core.OnAdLoadedEvent(api.GetResponseInfoString(), api.GetAdSource());
                    AttachForAdInstance(ad, isPreloaded: false);
                }
            });
        }

        private void AttachForAdInstance(object adInstance, bool isPreloaded)
        {
            if (adInstance == null) return;

            api.N_Bind(adInstance);

            // Paid
            api.SubPaid(adValue =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"Callback.Paid {core.GroupName}", () => $"adtype={core.Adtype}"))
                    {
                        var adSourceNow = api.GetAdSource();

                        var rev = 0d;
                        try { if (adValue != null) rev = adValue.Value / 1000000d; } catch { }
                        var currency = adValue != null ? adValue.CurrencyCode : "USD";

                        core.OnAdRevenuePaidEvent(rev, currency, adSourceNow);
                    }
                });
            });

            // Clicked
            api.SubClicked(() =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"Callback.Clicked {core.GroupName}", () => $"adtype={core.Adtype}"))
                    {
                        core.OnAdClickedEvent(api.GetAdSource());
                    }
                });
            });

            // Opened (Displayed)
            api.SubOpened(() =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"Callback.Opened {core.GroupName}", () => $"adtype={core.Adtype}"))
                    {
                        core.OnAdDisplayedEvent(api.GetAdSource());
                    }
                });
            });

            // Closed (Hidden)
            api.SubClosed(() =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"Callback.Closed {core.GroupName}", () => $"adtype={core.Adtype}"))
                    {
                        core.OnAdHiddenEvent(api.GetAdSource(), loadAd: !isPreloaded);
                    }
                });
            });

            // Failed
            api.SubFailed(error =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"Callback.ShowFail {core.GroupName}", () => $"adtype={core.Adtype}"))
                        {
                            var err = error != null ? error.ToString() : "(null)";
                            var adInfoSt = api.GetResponseInfoString();
                            int errorCode;
                            try { errorCode = error != null ? error.GetCode() : int.MinValue; }
                            catch { errorCode = int.MinValue; }
                            core.OnAdDisplayFailedEvent(adInfoSt, err, loadAd: !isPreloaded, errorCode: errorCode);
                        }
                    });
                });

            // Received reward
            api.SubReceivedReward(() =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"Callback.Reward {core.GroupName}", () => $"adtype={core.Adtype}"))
                    {
                        core.OnAdReceivedRewardEvent(api.GetResponseInfoString());
                    }
                });
            });
        }
    }
}
