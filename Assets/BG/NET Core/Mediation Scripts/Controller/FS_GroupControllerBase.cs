using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using System;
using UnityEngine;
using AdInfo = BG_Library.Common.AdInfo;

namespace BG_Library.NET.Mediation.Base
{
    public abstract class FS_GroupControllerBase<T> : IFSGroup
        where T : InfoBase
    {
        public Action onAdComplete;

        public FS_GroupControllerBase(T info, string adtype, string groupName, string mediation)
        {
            Info = info;
            Adtype = adtype;     // (adtype)
            GroupName = groupName;
            Mediation = mediation;
        }

        protected FSMetric metric;
        public T Info { get; private set; }

        public string Adtype { get; private set; }     // adtype
        public string GroupName { get; private set; }
        public string Mediation { get; private set; }
        private Channel? trackingChannel;
        private GroupRequestContext lastLoadRequestApiContext = GroupRequestContext.Init;
        private int lastLoadRequestRetryAttempt;
        private bool isAwaitingTerminalShowCallback;
        private string TrackingIdentitySourceValue => GetTrackingAdType switch
        {
            GroupAdType.ForceAd or GroupAdType.Popup => $"{GroupName}|{Id}",
            _ => Id ?? ""
        };

        #region ===== POLICY =====

        public RetryLoadLogic<T> RetryLogic { get; private set; }
        public StopLogic<T> StopLogic { get; private set; }

        #endregion

        #region ===== RUNTIME STATE / GATES =====

        public bool IsInit { get; protected set; }
        public bool IsLoading { get; protected set; }
        public string Id => Info.Id;

        public bool IsPreloadAd { get; protected set; } = false;
        public int Budget { get; protected set; }
        protected bool DisablePostInitReload { get; set; }

        #endregion

        #region CORE

        public void Initialize()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Init {GroupName}", () => $"adtype={Adtype} id={Info?.Id} preload={IsPreloadAd}"))
            {
                if (IsInit)
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Init.Skipped {GroupName}", () => $"adtype={Adtype} id={Info?.Id} isInit={IsInit}");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.Init, TrackingReason.AlreadyInitialized, trackingChannel);
                    return;
                }

                if (string.IsNullOrEmpty(Info.Id))
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Init.Skipped {GroupName}", () => $"adtype={Adtype} id={Info?.Id} isInit={IsInit}");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.Init, TrackingReason.MissingAdUnitId, trackingChannel);

                    return;
                }

                MediationSetup();
                
                IsInit = true;
                IsLoading = false;

                metric = new FSMetric();

                RetryLogic = new RetryLoadLogic<T>(this);
                StopLogic = new StopLogic<T>(this);

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Init.OK {GroupName}", () => $"adtype={Adtype} id={Info?.Id}");

                if (IsPreloadAd) P_StartLoad();
                else N_Load("init");
            }
        }

        public bool ForceInitialize()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"ForceInit {GroupName}",
                () => $"adtype={Adtype} id={Info?.Id} preload={IsPreloadAd} init={IsInit} loading={IsLoading} awaiting={isAwaitingTerminalShowCallback}"))
            {
                if (string.IsNullOrEmpty(Info.Id))
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"ForceInit.Skipped {GroupName}",
                        () => $"adtype={Adtype} id={Info?.Id} reason=MissingAdUnitId");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.Init, TrackingReason.MissingAdUnitId, trackingChannel);
                    return false;
                }

                if (isAwaitingTerminalShowCallback)
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"ForceInit.Blocked {GroupName}",
                        () => $"adtype={Adtype} reason=AlreadyShowing");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.Init, TrackingReason.AlreadyShowing, trackingChannel);
                    return false;
                }

                DoBestEffortCleanup();
                ResetRuntimeForForceInitialize();
                Initialize();
                return true;
            }
        }

        public bool Show(string pos, Action onBeforeAdShow = null, Action onAdShowComplete = null)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Show {GroupName}", () => $"adtype={Adtype} pos={pos}"))
            {
                if (!string.IsNullOrEmpty(pos)) metric.lastPos = pos;

                // Gate: not-ready => show-fail event
                if (!GetReady())
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Show.Blocked {GroupName}", () => $"adtype={Adtype} pos={pos} reason=IsReadyFail");
                    NetTrackingSystem.ShowGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, pos, TrackingReason.NotReady, trackingChannel);
                    NetEventSystem.OnFsShowFailed?.Invoke(MakeInfo(pos));

                    // Self-heal (NORMAL only): only based on IsReady false, only when idle
                    if (!IsPreloadAd && !DisablePostInitReload)
                    {
                        TrySelfHeal_LoadIfIdle();
                    }

                    return false;
                }

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Show.Resolve {GroupName}", () => $"adtype={Adtype} mode={(IsPreloadAd ? "preload" : "normal")} pos={pos}");
                ArmTerminalShowCallbackGate();

                var showAccepted = false;
                try
                {
                    if (IsPreloadAd)
                        showAccepted = P_Show(pos, onBeforeAdShow, onAdShowComplete);
                    else
                        showAccepted = N_Show(pos, onBeforeAdShow, onAdShowComplete);

                    return showAccepted;
                }
                finally
                {
                    if (!showAccepted)
                        ResetTerminalShowCallbackGate();
                }
            }
        }

        public bool GetReady()
        {
            var systemCond =
                IsInit &&
                StopLogic != null &&
                !StopLogic.IsStopped &&
                !string.IsNullOrEmpty(Info.Id);

            if (IsPreloadAd)
            {
                return systemCond && API_P_GetAdReady();
            }
            else
            {
                return systemCond && API_N_GetAdReady();
            }
        }

        public void Stop()
        {
            // Stop thường do sys gọi -> join flow luôn
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Stop {GroupName}", () => $"adtype={Adtype}"))
            {
                StopLogic.Stop();
            }
        }

        public virtual string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder(1200);

            sb.AppendLine("=== FS Group Debug ===");

            // Identity
            sb.AppendLine("-- Identity --");
            sb.Append("GroupName: ").AppendLine(GroupName ?? "");
            sb.Append("Format: ").AppendLine(Adtype ?? ""); // (adtype)
            sb.Append("Mediation: ").AppendLine(Mediation ?? "");
            sb.Append("AdUnitId: ").AppendLine(Info != null ? (Info.Id ?? "") : "(null)");

            // State
            sb.AppendLine();
            sb.AppendLine("-- State --");
            sb.Append("IsInit: ").Append(IsInit).Append(" | ");
            sb.Append("IsLoading: ").Append(IsLoading).Append(" | ");
            sb.Append("IsPreloadAd: ").Append(IsPreloadAd).Append(" | ");
            sb.Append("Budget: ").Append(Budget).Append(" | ");
            sb.Append("DisablePostInitReload: ").Append(DisablePostInitReload).Append(" | ");
            sb.Append("AwaitingTerminalCallback: ").AppendLine(isAwaitingTerminalShowCallback.ToString());

            // Policies
            sb.AppendLine();
            sb.AppendLine("-- Policies --");

            if (StopLogic == null) sb.AppendLine("StopLogic: (null)");
            else
            {
                sb.AppendLine(StopLogic.GetDebugInfo());

                sb.Append("Stop.IsStopped: ").Append(StopLogic.IsStopped).Append(" | ");
                sb.Append("Stop.RemainingShows: ").AppendLine(StopLogic.RemainingShows.ToString());

                bool shouldIgnore = false;
                try { shouldIgnore = StopLogic.ShouldIgnore(); } catch { }
                sb.Append("Stop.ShouldIgnore(): ").AppendLine(shouldIgnore.ToString());
            }

            if (RetryLogic == null) sb.AppendLine("RetryLogic: (null)");
            else
            {
                sb.AppendLine(RetryLogic.GetDebugInfo());

                sb.Append("Retry.Attempt: ").Append(RetryLogic.Attempt).Append(" | ");
                sb.Append("Retry.IsWaiting: ").AppendLine(RetryLogic.IsWaiting.ToString());
            }

            // Ready (debug string only; runtime GetReady log disabled)
            sb.AppendLine();
            sb.AppendLine("-- Ready --");
            bool ready = false;
            try { ready = GetReady(); } catch { }
            sb.Append("GetReady(): ").AppendLine(ready.ToString());

            // Metric
            sb.AppendLine();
            sb.AppendLine("-- Metric --");
            sb.Append("requestCount: ").Append(metric.requestCount).Append(" | ");
            sb.Append("loadSuccessCount: ").Append(metric.loadSuccessCount).Append(" | ");
            sb.Append("loadFailCount: ").Append(metric.loadFailCount).AppendLine();

            sb.Append("startLoadTime: ").Append(metric.startLoadTime.ToString("0.000")).AppendLine();
            sb.Append("loadTimeMs: ").Append(metric.loadTimeMs).AppendLine();

            sb.Append("totalImpression: ").Append(metric.totalImpression).Append(" | ");
            sb.Append("totalRevenue: ").Append(metric.totalRevenue.ToString("0.########")).AppendLine();

            // Last snapshot
            sb.AppendLine();
            sb.AppendLine("-- Last Snapshot --");
            sb.Append("lastPos: ").AppendLine(metric.lastPos ?? "");
            sb.Append("lastAdSource: ").AppendLine(metric.lastAdSource ?? "");
            sb.Append("lastLoadedInfo: ").AppendLine(metric.lastLoadedInfo ?? "");
            sb.Append("lastLoadedError: ").AppendLine(metric.lastLoadedError ?? "");
            sb.Append("lastShownError: ").AppendLine(metric.lastShownError ?? "");

            return sb.ToString();
        }

        protected virtual void MediationSetup() { }

        #endregion

        #region Policy Function

        public bool CanRetryGate()
        {
            return IsInit
                && StopLogic != null
                && !StopLogic.IsStopped
                && !string.IsNullOrEmpty(Info.Id)
                && !IsLoading;
        }

        public void DoBestEffortCleanup()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Cleanup {GroupName}"
                , () => $"adtype={Adtype} mode={(IsPreloadAd ? "preload" : "normal")}"))
            {
                IsLoading = false;
                RetryLogic?.Stop();
                ResetTerminalShowCallbackGate();

                if (IsPreloadAd) API_P_DestroyAd();
                else API_N_DestroyAd();
            }
        }

        #endregion

        #region Normal

        public void N_Load(string loadContext)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Load {GroupName}", () => $"adtype={Adtype} mode=normal"))
            {
                if (StopLogic == null || StopLogic.ShouldIgnore()) return;
                if (string.IsNullOrEmpty(Info.Id)) return;
                if (DisablePostInitReload && !string.Equals(loadContext, "init", StringComparison.OrdinalIgnoreCase))
                {
                    NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Load.Blocked {GroupName}",
                        () => $"adtype={Adtype} reason=DisablePostInitReload context={loadContext}");
                    int retryAttempt = ResolveRetryAttempt(loadContext);
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, MapLoadContext(loadContext), TrackingReason.Disabled, trackingChannel, retryAttempt: retryAttempt);
                    return;
                }

                if (API_N_GetAdReady())
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Load.Skipped {GroupName}", () => $"adtype={Adtype} reason=APIReady");
                    int retryAttempt = ResolveRetryAttempt(loadContext);
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, MapLoadContext(loadContext), TrackingReason.AlreadyLoaded, trackingChannel, retryAttempt: retryAttempt);
                    return;
                }
                
                if (IsLoading)
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Load.Skipped {GroupName}", () => $"adtype={Adtype} reason=IsLoading");
                    int retryAttempt = ResolveRetryAttempt(loadContext);
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, MapLoadContext(loadContext), TrackingReason.LoadingInProgress, trackingChannel, retryAttempt: retryAttempt);
                    return;
                }

                IsLoading = true;
                metric.Request();

                if (RetryLogic != null && RetryLogic.IsWaiting)
                    RetryLogic.Stop();

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Load.CallAPI {GroupName}", () => $"adtype={Adtype} id={Info?.Id}");

                lastLoadRequestApiContext = MapLoadContext(loadContext);
                lastLoadRequestRetryAttempt = ResolveRetryAttempt(loadContext);
                NetTrackingSystem.RequestGroupReachedApi(GetTrackingAdType, TrackingIdentitySourceValue, lastLoadRequestApiContext, trackingChannel, retryAttempt: lastLoadRequestRetryAttempt);
                API_N_RequestAd();

                NetEventSystem.OnFsRequest?.Invoke(MakeInfo(""));
            }
        }

        protected bool N_Show(string pos, Action onBeforeAdShow = null, Action onAdShowComplete = null)
        {
            // Join flow từ Show(...)
            onBeforeAdShow?.Invoke();
            onAdComplete = onAdShowComplete;

            NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Show.CallAPI {GroupName}", () => $"adtype={Adtype} mode=normal pos={pos}");
            NetTrackingSystem.ShowGroupApi(GetTrackingAdType, TrackingIdentitySourceValue, pos, trackingChannel);
            NetEventSystem.OnFsBeforeOpen?.Invoke(MakeInfo(pos));

            API_N_Show();
            return true;
        }

        private void TrySelfHeal_LoadIfIdle()
        {
            if (RetryLogic != null && RetryLogic.IsWaiting) 
            {
                NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.Idle, TrackingReason.RetryWaiting, trackingChannel);
                return;
            }

            NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"SelfHeal {GroupName}", () => $"adtype={Adtype} action=LoadIfIdle");
            N_Load("idle");
        }

        // Initialize, Retry load failed, Self Heal Idle, Hidden Ad, Display failed Ad 
        public enum LoadContext
        {
            init, rt, sh, hd, df
        }

        #endregion

        #region PreloadAd

        protected virtual void P_StartLoad()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Preload.Start {GroupName}", () => $"adtype={Adtype}");
        }

        protected virtual bool P_Show(string pos, Action onBeforeAdShow = null, Action onAdShowComplete = null)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Preload.Show {GroupName}", () => $"adtype={Adtype} pos={pos}");
            return false;
        }

        protected virtual void API_P_DestroyAd()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Preload.Destroy {GroupName}", () => $"adtype={Adtype}");
        }

        protected virtual bool API_P_GetAdReady()
        {
            return false;
        }

        #endregion

        #region Event Callback

        public void OnAdLoadedEvent(string loadedInfo, string adSource)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.Loaded {GroupName}"
                , () => $"adtype={Adtype} adSource={adSource}"))
            {
                IsLoading = false;
                metric.Loaded(loadedInfo, adSource);

                NetTrackingSystem.LoadGroupSuccess(GetTrackingAdType, TrackingIdentitySourceValue, lastLoadRequestApiContext, metric.loadTimeMs, trackingChannel, retryAttempt: lastLoadRequestRetryAttempt);
                NetEventSystem.OnFsLoaded?.Invoke(MakeInfo(""));

                if (StopLogic.ShouldIgnore())
                    StopLogic.Stop();
                else
                    RetryLogic?.Stop();
            }
        }

        public void OnAdLoadFailedEvent(int errorCode, string errorMessage)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.LoadFail {GroupName}"
                , () => $"adtype={Adtype} err={errorMessage}"))
            {
                IsLoading = false;
                metric.LoadFailed(errorMessage);

                NetTrackingSystem.LoadGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, lastLoadRequestApiContext, errorCode, NetworkMonitor.Current.CheckNetwork, metric.loadTimeMs, trackingChannel, retryAttempt: lastLoadRequestRetryAttempt);
                NetEventSystem.OnFsLoadFailed?.Invoke(MakeInfo(""), errorMessage ?? "AdLoadFailed_ErrorMessageNull");

                if (DisablePostInitReload)
                {
                    NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Reload.Blocked {GroupName}",
                        () => $"adtype={Adtype} reason=DisablePostInitReload source=load_fail");
                }
                else
                {
                    // Retry là nhánh tự chạy; bản thân RetryLogic có thể FlowNew nếu bạn muốn tách
                    RetryLogic?.Schedule();
                }
            }
        }

        public void OnAdDisplayedEvent(string adSource)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.Displayed {GroupName}", () => $"adtype={Adtype} adSource={adSource}"))
            {
                metric.Displayed(adSource);
                NetTrackingSystem.TrackGroupDisplayed(GetTrackingAdType, TrackingIdentitySourceValue, metric.lastPos, trackingChannel);
                NetEventSystem.OnFsDisplayed?.Invoke(MakeInfo(metric.lastPos));
            }
        }

        public void OnAdImpressionEvent(string adSource)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.Impression {GroupName}", () => $"adtype={Adtype} adSource={adSource} pos={metric.lastPos}"))
            {
                // TrackGroupImpression / TrackAdImpression run on the paid event; tracking here would double count.
                NetEventSystem.OnFsImpression?.Invoke(MakeInfo(metric.lastPos));
            }
        }

        public void OnAdClickedEvent(string adSource)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.Clicked {GroupName}", () => $"adtype={Adtype} pos={metric.lastPos}"))
            {
                NetTrackingSystem.TrackGroupClick(GetTrackingAdType, TrackingIdentitySourceValue, metric.lastPos, trackingChannel);
                NetEventSystem.OnFsClicked?.Invoke(MakeInfo(metric.lastPos));
            }
        }

        public void OnAdRevenuePaidEvent(double rev, string currencyCode, string adSource)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.Paid {GroupName}", () => $"adtype={Adtype} rev={rev} {currencyCode} pos={metric.lastPos}"))
            {
                metric.RevenuePaid(rev, adSource);
                NetTrackingSystem.TrackGroupImpression(GetTrackingAdType, TrackingIdentitySourceValue, metric.lastPos, trackingChannel);
                var adValueInfo = new AdValueInfo
                {
                    adRevenue = rev,
                    currencyCode = currencyCode
                };
                NetTrackingSystem.TrackAdImpression(MakeInfo(metric.lastPos), adValueInfo);

                NetEventSystem.OnFsPaid?.Invoke(MakeInfo(metric.lastPos), adValueInfo);
            }
        }

        public void OnAdHiddenEvent(string adSource, bool loadAd = true)
        {
            if (!TryConsumeTerminalShowCallbackGate("Closed", adSource, loadAd))
                return;

            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.Closed {GroupName}", () => $"adtype={Adtype} pos={metric.lastPos} loadAd={loadAd}"))
            {
                NetTrackingSystem.TrackGroupClosed(GetTrackingAdType, TrackingIdentitySourceValue, metric.lastPos, trackingChannel);
                NetEventSystem.OnFsClosed?.Invoke(MakeInfo(metric.lastPos));

                onAdComplete?.Invoke();
                onAdComplete = null;

                var hasBudget = StopLogic.ConsumeOneShowBudget();

                if (!hasBudget && loadAd) 
                {
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.HideReload, TrackingReason.BudgetExhausted, trackingChannel);
                }

                if (!loadAd)
                    return;

                if (!DisablePostInitReload)
                {
                    N_Load("hide");
                }
                else
                {
                    NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Reload.Blocked {GroupName}",
                        () => $"adtype={Adtype} reason=DisablePostInitReload source=hide");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.HideReload, TrackingReason.Disabled, trackingChannel);
                }
            }
        }

        public void OnAdDisplayFailedEvent(string adInfo, string errorMessage, bool loadAd = true, int errorCode = int.MinValue)
        {
            if (!TryConsumeTerminalShowCallbackGate("ShowFail", adInfo, loadAd))
                return;

            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.ShowFail {GroupName}", () => $"adtype={Adtype} pos={metric.lastPos} loadAd={loadAd}"))
            {
                metric.DisplayFailed(adInfo, errorMessage);
                NetTrackingSystem.TrackGroupShowFailed(GetTrackingAdType, TrackingIdentitySourceValue, metric.lastPos, errorCode, trackingChannel);

                onAdComplete?.Invoke();
                onAdComplete = null;

                NetEventSystem.OnFsShowFailed?.Invoke(MakeInfo(metric.lastPos));

                if (!loadAd)
                    return;

                if (!DisablePostInitReload)
                {
                    N_Load("difa");
                }
                else
                {
                    NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Reload.Blocked {GroupName}",
                        () => $"adtype={Adtype} reason=DisablePostInitReload source=display_fail");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.DisplayFailReload, TrackingReason.Disabled, trackingChannel);
                }
            }
        }

        public void OnAdReceivedRewardEvent(string adInfo)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Event.Reward {GroupName}", () => $"adtype={Adtype} pos={metric.lastPos}"))
            {
                NetTrackingSystem.TrackGroupRewarded(GetTrackingAdType, TrackingIdentitySourceValue, metric.lastPos, trackingChannel);
                NetEventSystem.OnFsRewarded?.Invoke(MakeInfo(metric.lastPos));
            }
        }

        #endregion

        #region Abstract

        protected abstract void API_N_RequestAd();
        protected abstract bool API_N_GetAdReady();
        protected abstract void API_N_Show();
        protected abstract void API_N_DestroyAd();

        #endregion

        #region ===== HELPERS =====

        public AdInfo MakeInfo(string pos)
        {
            return new AdInfo
            {
                id = Info.Id,
                adtype = Adtype, // (adtype)
                mediation = Mediation,
                adSource = metric.lastAdSource ?? "",

                group = GroupName,
                pos = pos,
                loadTime = metric.loadTimeMs,

                extra = ""
            };
        }

        public GroupAdType GetTrackingAdType
        {
            get
            {
                return Adtype switch
                {
                    BG_ConstValue.adtype_ao => GroupAdType.AppOpen,
                    BG_ConstValue.adtype_fa => GroupAdType.ForceAd,
                    BG_ConstValue.adtype_rw => GroupAdType.Rewarded,
                    BG_ConstValue.adtype_bn => GroupAdType.Banner,
                    BG_ConstValue.adtype_mrec => GroupAdType.Mrec,
                    BG_ConstValue.adtype_pu => GroupAdType.Popup,
                    BG_ConstValue.adtype_cl => GroupAdType.Collap,
                    _ => GroupAdType.Other
                };
            }
        }

        public GroupAdType TrackingAdType => GetTrackingAdType;
        public string TrackingIdentitySource => TrackingIdentitySourceValue;
        internal Channel? CurrentTrackingChannel => trackingChannel;
        internal bool IsAwaitingTerminalShowCallback => isAwaitingTerminalShowCallback;

        internal void RaiseFsBeforeOpen(string pos)
        {
            NetEventSystem.OnFsBeforeOpen?.Invoke(MakeInfo(pos));
        }

        internal void RaiseFsShowFailedPreApi(string pos)
        {
            NetEventSystem.OnFsShowFailed?.Invoke(MakeInfo(pos));
        }

        public void SetTrackingChannel(Channel channel)
        {
            trackingChannel = channel;
        }

        private static GroupRequestContext MapLoadContext(string loadContext)
        {
            return loadContext switch
            {
                "retry" => GroupRequestContext.Retry,
                "idle" => GroupRequestContext.Idle,
                "hide" => GroupRequestContext.HideReload,
                "difa" => GroupRequestContext.DisplayFailReload,
                _ => GroupRequestContext.Init
            };
        }

        private int ResolveRetryAttempt(string loadContext)
        {
            if (!string.Equals(loadContext, "retry", StringComparison.OrdinalIgnoreCase))
                return 0;

            return RetryLogic?.Attempt ?? 0;
        }

        private void ArmTerminalShowCallbackGate()
        {
            isAwaitingTerminalShowCallback = true;
        }

        private void ResetTerminalShowCallbackGate()
        {
            isAwaitingTerminalShowCallback = false;
        }

        private bool TryConsumeTerminalShowCallbackGate(string callbackName, string callbackSource, bool loadAd)
        {
            if (!isAwaitingTerminalShowCallback)
            {
                NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Event.{callbackName}.DuplicateIgnored {GroupName}",
                    () => $"adtype={Adtype} pos={metric.lastPos} loadAd={loadAd} source={callbackSource}");
                return false;
            }

            isAwaitingTerminalShowCallback = false;
            return true;
        }

        private void ResetRuntimeForForceInitialize()
        {
            IsInit = false;
            IsLoading = false;
            metric = default;
            RetryLogic = null;
            StopLogic = null;
            onAdComplete = null;
            lastLoadRequestApiContext = GroupRequestContext.Init;
            lastLoadRequestRetryAttempt = 0;
            ResetTerminalShowCallbackGate();
        }

        #endregion
    }

    public struct FSMetric
    {
        // Số lần Request được gọi
        public int requestCount;

        // Số lần load thành công ad
        public int loadSuccessCount;
        // Số lần load không thành công ad
        public int loadFailCount;

        // Thời điểm load ad cuối cùng
        public float startLoadTime;
        // Tổng thời gian load được ad
        public int loadTimeMs;

        // Tổng impression đã được show
        public int totalImpression;
        // Tổng lượng revenue thu về
        public double totalRevenue;

        // Lần gọi show cuối cùng, bất kể show có thành công hay không
        public string lastPos;

        // Adsource từ EVENT gần nhất, bất kể show có thành công hay không
        public string lastAdSource;

        // Info từ lần loaded cuối cùng
        public string lastLoadedInfo;
        // Error từ lần load failed cuối cùng
        public string lastLoadedError;

        // Error từ lần show failed cuối cùng
        public string lastShownError;

        public void Request()
        {
            startLoadTime = Time.realtimeSinceStartup;
            loadTimeMs = 0;

            requestCount++;

            lastLoadedInfo = "";
            lastAdSource = "";
            // lastLoadedError/lastShownError giữ nguyên như "last known error" cho debug
        }

        public void Loaded(string loadedInfo, string adSource)
        {
            loadTimeMs = (int)((Time.realtimeSinceStartup - startLoadTime) * 1000);
            loadSuccessCount++;

            lastLoadedInfo = loadedInfo ?? "";
            lastLoadedError = "";
            lastAdSource = adSource ?? "";
        }

        public void LoadFailed(string errorMessage)
        {
            loadTimeMs = (int)((Time.realtimeSinceStartup - startLoadTime) * 1000);

            loadFailCount++;

            lastLoadedError = errorMessage ?? "";
            lastLoadedInfo = "";
            lastAdSource = "";
        }

        public void Displayed(string adSource)
        {
            lastAdSource = adSource ?? "";
        }

        public void DisplayFailed(string adSource, string errorMessage)
        {
            lastShownError = errorMessage ?? "";
            lastAdSource = adSource ?? "";
        }

        public void RevenuePaid(double rev, string adSource)
        {
            totalImpression++;
            totalRevenue += rev;

            lastAdSource = adSource ?? "";
        }
    }
}
