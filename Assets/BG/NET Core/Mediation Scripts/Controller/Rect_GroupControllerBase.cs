// =======================
// Rect_GroupControllerBase.cs
// =======================
using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using System;
using System.Text;
using UnityEngine;

namespace BG_Library.NET.Mediation.Base
{
    public abstract class Rect_GroupControllerBase<T> : IRectGroup
        where T : InfoBase
    {
        public Rect_GroupControllerBase(T info, string format, string groupName, string mediation)
        {
            Info = info;
            Adtype = format;
            GroupName = groupName;
            Mediation = mediation;
        }

        protected RectMetric metric;
        public T Info { get; private set; }

        public string Adtype { get; private set; }
        public string GroupName { get; private set; }
        public string Mediation { get; private set; }
        private string lastTrackingPos;
        private Channel? trackingChannel;
        private GroupRequestContext lastLoadRequestApiContext = GroupRequestContext.Init;
        private string lastLoadRequestTrackingTarget;
        private string TrackingIdentitySourceValue => GetTrackingAdType switch
        {
            GroupAdType.ForceAd or GroupAdType.Popup => $"{GroupName}|{Id}",
            _ => Id ?? ""
        };

        #region ===== RUNTIME STATE / GATES =====

        public bool IsInit { get; protected set; }
        public bool IsShowing { get; protected set; }
        public bool IsLoaded { get; protected set; }
        protected bool DisablePostInitReload { get; set; }

        public string Id => Info != null ? Info.Id : "";

        #endregion

        #region CORE Rect Ad

        public void Initialize()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Core.Init {GroupName}", () => $"adtype={Adtype} id={Id}"))
            {
                string requestTrackingTarget = ResolveRequestTrackingTarget();

                if (IsInit)
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}",
                        () => $"skip=true isInit={IsInit} idEmpty={string.IsNullOrEmpty(Id)}");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.Init, TrackingReason.AlreadyInitialized, trackingChannel, requestTrackingTarget);

                    return;
                }

                if (string.IsNullOrEmpty(Id))
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}",
                        () => $"skip=true isInit={IsInit} idEmpty={string.IsNullOrEmpty(Id)}");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.Init, TrackingReason.MissingAdUnitId, trackingChannel, requestTrackingTarget);

                    return;
                }

                MediationSetup();

                IsInit = true;
                IsShowing = false;
                IsLoaded = false;

                metric = new RectMetric();

                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}", () => "Load()");
                Load();
            }
        }

        public bool Rebuild()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Core.Rebuild {GroupName}",
                () => $"adtype={Adtype} id={Id} init={IsInit} loaded={IsLoaded} showing={IsShowing}"))
            {
                if (IsShowing)
                {
                    Message(() => "Rebuild skipped. Ad is currently showing", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}", () => "rebuild_blocked=showing");
                    return false;
                }

                if (IsInit)
                {
                    try
                    {
                        NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}", () => "API_DestroyAd()");
                        API_DestroyAd();
                    }
                    catch (Exception ex)
                    {
                        Message(() => $"Rebuild destroy failed. {ex.Message}", LogLevel.Warning);
                        NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}", () => $"rebuild_destroy_fail={ex.Message}");
                        return false;
                    }
                }

                ResetRuntimeForRebuild();
                Initialize();
                return true;
            }
        }

        public void Show(string pos = "")
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Core.Show {GroupName}",
                () => $"adtype={Adtype} id={Id} pos={pos} loaded={IsLoaded} showing={IsShowing}"))
            {
                string trackingPos = ResolveTrackingPos(pos);
                lastTrackingPos = trackingPos;
                if (!IsLoaded)
                {
                    Message(
                        () => "Show fail. Never loaded success",
                        LogLevel.Warning
                    );
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}", () => "loaded=false");
                    NetTrackingSystem.ShowGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, trackingPos, TrackingReason.NotReady, trackingChannel);
                    return;
                }

                if (ActivateViewCore(trackingPos, isActivateFlow: false))
                {
                    // Displayed event/track is emitted inside ActivateViewCore when ad is actually present.
                }
            }
        }

        public bool ActivateView(string pos = "")
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Core.ActivateView {GroupName}",
                () => $"adtype={Adtype} id={Id} pos={pos}"))
            {
                string trackingPos = ResolveTrackingPos(pos);
                lastTrackingPos = trackingPos;

                if (!IsInit)
                {
                    Message(
                        () => "Activate skipped. Group is not initialized yet",
                        LogLevel.Warning
                    );
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}", () => "isInit=false");
                    NetTrackingSystem.ActivateGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, trackingPos, TrackingReason.NotReady, trackingChannel);
                    return false;
                }

                return ActivateViewCore(trackingPos, isActivateFlow: true);
            }
        }

        public bool Expand(bool enableClick = true)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Core.Expand {GroupName}",
                () => $"adtype={Adtype} id={Id} enableClick={enableClick}"))
            {
                if (!IsInit)
                {
                    Message(
                        () => "Expand skipped. Group is not initialized yet",
                        LogLevel.Warning
                    );
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}", () => "isInit=false");
                    return false;
                }

                return API_Expand(enableClick);
            }
        }

        public void Hide()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Core.Hide {GroupName}",
                () => $"adtype={Adtype} id={Id} showing={IsShowing}"))
            {
                string trackingPos = string.IsNullOrEmpty(lastTrackingPos) ? ResolveTrackingPos("") : lastTrackingPos;

                if (!IsShowing)
                {
                    Message(
                        () => "Hide skipped. Ad is not showing",
                        LogLevel.Warning
                    );
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}", () => "showing=false");
                    NetTrackingSystem.HideGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, trackingPos, TrackingReason.NotShowing, trackingChannel);
                    return;
                }

                IsShowing = false;
                NetTrackingSystem.HideGroupApi(GetTrackingAdType, TrackingIdentitySourceValue, trackingPos, trackingChannel);
                NetEventSystem.OnRectHidden?.Invoke(MakeInfo());

                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}", () => "API_Hide()");
                API_Hide();
            }
        }

        public virtual Vector2 Mrec_GetSize() => Vector2.zero;
        public virtual void Mrec_UpdatePos(int pos) { }
        public virtual void Mrec_UpdatePos(GameObject targetObj, Camera camera = null) { }
        public virtual void Pu_UpdatePos(float xDp, float yDp, float w, float h) { }

        protected virtual string ResolveRequestTrackingTarget() => string.Empty;

        private void ResetRuntimeForRebuild()
        {
            IsInit = false;
            IsLoaded = false;
            IsShowing = false;
            metric = default;
            lastTrackingPos = string.Empty;
            lastLoadRequestApiContext = GroupRequestContext.Init;
            lastLoadRequestTrackingTarget = string.Empty;
        }

        private string ResolveTrackingPos(string pos)
        {
            if (!string.IsNullOrEmpty(pos))
                return pos;

            return NetTrackingSystem.DefaultRectPos(GetTrackingAdType);
        }

        public virtual string GetDebugInfo()
        {
            var sb = new StringBuilder(1200);

            sb.AppendLine("=== Rect Group Debug ===");

            // Identity
            sb.AppendLine("-- Identity --");
            sb.Append("GroupName: ").AppendLine(GroupName ?? "");
            sb.Append("Format: ").AppendLine(Adtype ?? "");
            sb.Append("Mediation: ").AppendLine(Mediation ?? "");
            sb.Append("AdUnitId: ").AppendLine(string.IsNullOrEmpty(Id) ? "(empty)" : Id);

            // State
            sb.AppendLine();
            sb.AppendLine("-- State --");
            sb.Append("IsInit: ").Append(IsInit).Append(" | ");
            sb.Append("IsLoaded: ").Append(IsLoaded).Append(" | ");
            sb.Append("IsShowing: ").Append(IsShowing).Append(" | ");
            sb.Append("DisablePostInitReload: ").AppendLine(DisablePostInitReload.ToString());

            // Metric
            sb.AppendLine();
            sb.AppendLine("-- Metric --");
            sb.Append("requestCount: ").Append(metric.requestCount).Append(" | ");
            sb.Append("loadedCountTotal: ").Append(metric.loadedCountTotal).Append(" | ");
            sb.Append("loadFailedCountTotal: ").Append(metric.loadFailedCountTotal).AppendLine();

            sb.Append("initialFailCount: ").Append(metric.initialFailCount).Append(" | ");
            sb.Append("refreshLoadedCount: ").Append(metric.refreshLoadedCount).Append(" | ");
            sb.Append("refreshFailCount: ").Append(metric.refreshFailCount).AppendLine();

            sb.Append("clickCount: ").Append(metric.clickCount).AppendLine();

            // Timing
            sb.AppendLine();
            sb.AppendLine("-- Timing --");
            sb.Append("firstRequestTime: ").Append(metric.firstRequestTime <= 0f ? "(none)" : metric.firstRequestTime.ToString("0.000")).AppendLine();
            sb.Append("firstResultLoadTimeMs: ").Append(metric.hasFirstResult ? metric.firstResultLoadTimeMs : -1).AppendLine();
            sb.Append("firstSuccessLoadTimeMs: ").Append(metric.hasFirstSuccess ? metric.firstSuccessLoadTimeMs : -1).AppendLine();
            sb.Append("lastResultDeltaMs: ").Append(metric.lastResultDeltaMs).AppendLine();
            sb.Append("currentResultLoadTimeMs: ").Append(metric.currentResultLoadTimeMs).AppendLine();

            // Revenue
            sb.AppendLine();
            sb.AppendLine("-- Revenue --");
            sb.Append("impressionCount: ").Append(metric.impressionCount).Append(" | ");
            sb.Append("totalRevenue: ").Append(metric.totalRevenue.ToString("0.########")).AppendLine();

            sb.Append("lastRevenue: ").Append(metric.lastRevenue.ToString("0.########")).Append(" | ");
            sb.Append("lastCurrencyCode: ").AppendLine(metric.lastCurrencyCode ?? "");

            // Last snapshot
            sb.AppendLine();
            sb.AppendLine("-- Last Snapshot --");
            sb.Append("lastResultIsSuccess: ").AppendLine(metric.lastResultIsSuccess.ToString());
            sb.Append("lastAdSource: ").AppendLine(metric.lastAdSource ?? "");
            sb.Append("lastLoadedInfo: ").AppendLine(metric.lastLoadedInfo ?? "");
            sb.Append("lastLoadedError: ").AppendLine(metric.lastLoadedError ?? "");

            return sb.ToString();
        }

        protected virtual void MediationSetup() { }

        #endregion

        #region Policy Interface Function

        public void Message(Func<string> msg, LogLevel logLevel = LogLevel.Info)
        {
            switch (logLevel)
            {
                case LogLevel.Warning:
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Message {GroupName}", msg);
                    break;
                case LogLevel.Error:
                    NetFlowDebugSystem.Error(Layer.group, Module.rect_group, $"Message {GroupName}", msg);
                    break;
                default:
                    NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Message {GroupName}", msg);
                    break;
            }
        }

        #endregion

        #region Normal

        protected void Load()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Core.Load {GroupName}",
                () => $"adtype={Adtype} id={Id} loaded={IsLoaded}"))
            {
                if (IsLoaded)
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}", () => "loaded=true");
                    NetTrackingSystem.RequestGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, GroupRequestContext.Init, TrackingReason.AlreadyLoaded, trackingChannel, ResolveRequestTrackingTarget());

                    return;
                }

                metric.OnRequest();

                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}", () => "API_RequestAd()");
                lastLoadRequestApiContext = GroupRequestContext.Init;
                lastLoadRequestTrackingTarget = ResolveRequestTrackingTarget();
                NetTrackingSystem.RequestGroupReachedApi(GetTrackingAdType, TrackingIdentitySourceValue, lastLoadRequestApiContext, trackingChannel, lastLoadRequestTrackingTarget);

                API_RequestAd();

                NetEventSystem.OnRectRequest?.Invoke(MakeInfo());
            }
        }

        #endregion

        private bool ActivateViewCore(string pos, bool isActivateFlow)
        {
            if (IsShowing)
            {
                Message(
                    () => isActivateFlow ? "Activate skipped. Already showing" : "Show skipped. Already showing",
                    LogLevel.Warning
                );
                NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}", () => "already_showing=true");

                if (isActivateFlow)
                    NetTrackingSystem.ActivateGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, pos, TrackingReason.AlreadyShowing, trackingChannel);
                else
                    NetTrackingSystem.ShowGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, pos, TrackingReason.AlreadyShowing, trackingChannel);

                return false;
            }

            if (!TryValidateShowBeforeApi(pos, isActivateFlow, out var failReason, out var failDetail))
            {
                string message = isActivateFlow ? "Activate skipped." : "Show skipped.";
                if (!string.IsNullOrEmpty(failDetail))
                    message += $" {failDetail}";

                Message(() => message, LogLevel.Warning);
                NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}",
                    () => $"show_validation_fail=true reason={failReason} detail={failDetail}");

                if (isActivateFlow)
                    NetTrackingSystem.ActivateGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, pos, failReason, trackingChannel);
                else
                    NetTrackingSystem.ShowGroupFail(GetTrackingAdType, TrackingIdentitySourceValue, pos, failReason, trackingChannel);

                return false;
            }

            bool hasAd = IsLoaded;
            Message(() => isActivateFlow ? "Activate View" : "Show Loaded View");

            IsShowing = true;

            if (isActivateFlow)
            {
                NetTrackingSystem.ActivateGroupApi(GetTrackingAdType, TrackingIdentitySourceValue, pos, trackingChannel);
            }
            else
            {
                NetTrackingSystem.ShowGroupApi(GetTrackingAdType, TrackingIdentitySourceValue, pos, trackingChannel);
            }

            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}",
                () => isActivateFlow ? $"API_Show() activate hasAd={hasAd}" : "API_Show()");
            API_Show();

            if (isActivateFlow)
            {
                NetEventSystem.OnRectViewActivated?.Invoke(MakeInfo(), hasAd);
            }

            if (hasAd)
            {
                NetEventSystem.OnRectDisplayed?.Invoke(MakeInfo());
            }

            return true;
        }

        #region Event Callback

        // NOTE: Rect callbacks are async from SDK => FlowNew() to separate operation id.
        public void OnAdLoadedEvent(string loadedInfo, string adSource)
        {
            using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.Loaded {GroupName}",
                () => $"adtype={Adtype} id={Id}"))
            {
                bool isFirstSuccess = !metric.hasFirstSuccess;
                metric.OnLoaded(loadedInfo, adSource);
                if (isFirstSuccess)
                    NetTrackingSystem.LoadGroupSuccess(GetTrackingAdType, TrackingIdentitySourceValue, lastLoadRequestApiContext, metric.firstSuccessLoadTimeMs, trackingChannel, lastLoadRequestTrackingTarget);
                else
                    NetTrackingSystem.LoadGroupRefreshSuccess(GetTrackingAdType, TrackingIdentitySourceValue, lastLoadRequestApiContext, trackingChannel, lastLoadRequestTrackingTarget);

                if (!IsLoaded)
                {
                    IsLoaded = true;
                }

                NetEventSystem.OnRectLoaded?.Invoke(MakeInfo());
            }
        }

        public void OnAdLoadFailedEvent(int errorCode, string errorMessage)
        {
            using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.LoadFailed {GroupName}",
                () => $"adtype={Adtype} id={Id} err={(string.IsNullOrEmpty(errorMessage) ? "(null/empty)" : "has")}"))
            {
                metric.OnLoadFailed(errorMessage);

                NetTrackingSystem.LoadGroupFailNoSpeed(GetTrackingAdType, TrackingIdentitySourceValue, lastLoadRequestApiContext, errorCode,
                    NetworkMonitor.Current.CheckNetwork, trackingChannel, lastLoadRequestTrackingTarget);
                NetEventSystem.OnRectLoadFailed?.Invoke(MakeInfo(), errorCode, errorMessage ?? "AdLoadFailed_ErrorMessageNull");
            }
        }

        public void OnAdClickedEvent(string adSource)
        {
            using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.Clicked {GroupName}",
                () => $"adtype={Adtype} id={Id}"))
            {
                metric.OnClicked(adSource);
                NetTrackingSystem.TrackGroupClick(GetTrackingAdType, TrackingIdentitySourceValue, lastTrackingPos, trackingChannel);
                NetEventSystem.OnRectClicked?.Invoke(MakeInfo());
            }
        }

        public void OnAdImpressionEvent(string adSource)
        {
            using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.Impression {GroupName}",
                () => $"adtype={Adtype} id={Id} source={adSource}"))
            {
                // TrackGroupImpression / TrackAdImpression run on the paid event; tracking here would double count.
                NetEventSystem.OnRectImpression?.Invoke(MakeInfo());
            }
        }

        public void OnAdRevenuePaidEvent(double rev, string currencyCode, string adSource)
        {
            using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.Paid {GroupName}",
                () => $"adtype={Adtype} id={Id} rev={rev:0.########} cur={currencyCode}"))
            {
                metric.OnPaid(rev, currencyCode, adSource);
                NetTrackingSystem.TrackGroupImpression(GetTrackingAdType, TrackingIdentitySourceValue, lastTrackingPos, trackingChannel);
                var adValueInfo = new AdValueInfo
                {
                    adRevenue = rev,
                    currencyCode = currencyCode
                };
                NetTrackingSystem.TrackAdImpression(MakeInfo(), adValueInfo);
                NetEventSystem.OnRectPaid?.Invoke(MakeInfo(), adValueInfo);
            }
        }

        #endregion

        #region Abstract

        protected abstract void API_RequestAd();
        protected abstract void API_Show();
        protected abstract void API_Hide();
        protected abstract void API_DestroyAd();

        protected virtual bool API_Expand(bool enableClick) => false;

        protected virtual bool TryValidateShowBeforeApi(string pos, bool isActivateFlow, out TrackingReason reason, out string detail)
        {
            reason = default;
            detail = string.Empty;
            return true;
        }

        #endregion

        #region ===== HELPERS =====

        private AdInfo MakeInfo()
        {
            return new AdInfo
            {
                id = Id,
                adtype = Adtype,
                mediation = Mediation,
                adSource = metric.lastAdSource ?? "",

                group = GroupName,
                pos = "",
                loadTime = metric.currentResultLoadTimeMs,

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

        public void SetTrackingChannel(Channel channel)
        {
            trackingChannel = channel;
        }

        #endregion
    }

    [Serializable]
    public struct RectMetric
    {
        #region ===== COUNTERS =====
        public int requestCount;

        public int loadedCountTotal;
        public int loadFailedCountTotal;

        public int initialFailCount;
        public int refreshLoadedCount;
        public int refreshFailCount;

        public int clickCount;

        #endregion

        #region ===== TIMING =====

        public float firstRequestTime;

        public bool hasFirstResult;
        public int firstResultLoadTimeMs;

        public bool hasFirstSuccess;
        public int firstSuccessLoadTimeMs;

        public float lastResultTime;
        public int lastResultDeltaMs;

        public int currentResultLoadTimeMs;

        #endregion

        #region ===== SNAPSHOT (LAST) =====

        public string lastAdSource;
        public string lastLoadedInfo;
        public string lastLoadedError;

        public bool lastResultIsSuccess;

        #endregion

        #region ===== REVENUE =====

        public int impressionCount;
        public double totalRevenue;

        public double lastRevenue;
        public string lastCurrencyCode;

        #endregion

        #region ===== API (CALL FROM CONTROLLER) =====

        public void OnRequest()
        {
            requestCount++;

            if (firstRequestTime <= 0f)
                firstRequestTime = Time.realtimeSinceStartup;

            lastLoadedInfo = "";
            lastLoadedError = "";
            lastResultIsSuccess = false;
        }

        public void OnLoaded(string loadedInfo, string adSource)
        {
            loadedCountTotal++;

            lastLoadedInfo = loadedInfo ?? "";
            lastLoadedError = "";

            if (!string.IsNullOrEmpty(adSource))
                lastAdSource = adSource;

            OnResultArrivedInternal(isSuccess: true);
        }

        public void OnLoadFailed(string errorMessage)
        {
            loadFailedCountTotal++;

            lastLoadedError = errorMessage ?? "";
            lastLoadedInfo = "";

            OnResultArrivedInternal(isSuccess: false);
        }

        public void OnClicked(string adSource)
        {
            clickCount++;

            if (!string.IsNullOrEmpty(adSource))
                lastAdSource = adSource;
        }

        public void OnPaid(double revenue, string currencyCode, string adSource)
        {
            impressionCount++;
            totalRevenue += revenue;

            lastRevenue = revenue;
            lastCurrencyCode = currencyCode ?? "";

            if (!string.IsNullOrEmpty(adSource))
                lastAdSource = adSource;
        }

        #endregion

        #region ===== INTERNAL =====

        private void OnResultArrivedInternal(bool isSuccess)
        {
            lastResultIsSuccess = isSuccess;

            float now = Time.realtimeSinceStartup;

            if (!hasFirstResult)
            {
                hasFirstResult = true;

                firstResultLoadTimeMs = (firstRequestTime > 0f)
                    ? (int)((now - firstRequestTime) * 1000f)
                    : 0;

                currentResultLoadTimeMs = firstResultLoadTimeMs;
                lastResultDeltaMs = firstResultLoadTimeMs;
            }
            else
            {
                lastResultDeltaMs = (lastResultTime > 0f)
                    ? (int)((now - lastResultTime) * 1000f)
                    : 0;

                currentResultLoadTimeMs = lastResultDeltaMs;
            }

            lastResultTime = now;

            if (isSuccess)
            {
                if (!hasFirstSuccess)
                {
                    hasFirstSuccess = true;

                    firstSuccessLoadTimeMs = (firstRequestTime > 0f)
                        ? (int)((now - firstRequestTime) * 1000f)
                        : 0;
                }
                else
                {
                    refreshLoadedCount++;
                }
            }
            else
            {
                if (!hasFirstSuccess) initialFailCount++;
                else refreshFailCount++;
            }
        }

        #endregion
    }
}
