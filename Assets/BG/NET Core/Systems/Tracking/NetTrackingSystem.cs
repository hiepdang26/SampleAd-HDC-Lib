using System;
using System.Collections.Generic;
using System.Text;
using AdjustSdk;
using BG_Library.Common;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using Firebase.Analytics;

namespace BG_Library.NET.Tracking
{
    public enum Channel
    {
        AppLaunch,
        AppResume,
        ForceAd,
        Rewarded,
        Banner,
        Mrec,
        Popup,
        Collap
    }

    public enum GroupAdType
    {
        AppOpen,
        ForceAd,
        Rewarded,
        Banner,
        Mrec,
        Popup,
        Collap,
        Other
    }

    public enum TrackingReason
    {
        Disabled,
        ConfigDisabled,
        IapRemoved,
        PositionBlocked,
        CappingBlocked,
        Ignored,
        GateBlocked,
        Blocked,
        GroupMissing,
        NotReady,
        Timeout,
        BlockedByFullscreen,
        RemoteConfigPending,
        AlreadyInitialized,
        MissingAdUnitId,
        AlreadyLoaded,
        LoadingInProgress,
        RetryWaiting,
        BudgetExhausted,
        HostBlocked,
        AlreadyShowing,
        NotShowing,
        NullData,
        UpdatePositionRequired,
        InvalidLayoutSize
    }

    public enum GroupRequestContext
    {
        Init,
        Retry,
        Idle,
        HideReload,
        DisplayFailReload
    }

    public enum TrackingHistoryActionFilter
    {
        All,
        Enter,
        Request,
        Show,
        Hide,
        Activate,
        Load,
        Event,
        Revenue
    }

    internal enum TrackingHistoryScope
    {
        System,
        AdCore,
        Group,
        Revenue
    }

    internal enum TrackingHistoryAction
    {
        Enter,
        Request,
        Show,
        Hide,
        Activate,
        Load,
        Event,
        Revenue,
        Other
    }

    public static class NetTrackingSystem
    {
        public const string EventBgAdImpression = "bg_ad_impression";

        public static Action<string> Emit;
        public static Action<string, Parameter[]> EmitWithParams;
        internal static Action<string> OnTrackedRawEvent;

        // Tracking rule:
        // 1. Event name stays short and stable across projects.
        // 2. system/adcore identity is channel-first.
        // 3. group identity is adtype + short stable gid only.
        // 4. groupName / pos / adUnitId are always clamped to avoid oversized event names.
        public static int MaxEventNameLen = 48;
        public static int MaxGroupTokenLen = 8;
        public static int MaxPosTokenLen = 8;
        public static int MaxGroupIdTokenLen = 5;

        public const string BannerDefaultPos = "bnDef";
        public const string MrecDefaultPos = "mrDef";
        public const string CollapDefaultPos = "clDef";

        public const string ParamAd_UnitId = "ad_unit_id";
        public const string ParamAd_Format = "ad_format";
        public const string ParamAd_Mediation = "ad_platform";
        public const string ParamAd_AdSource = "ad_source";
        public const string ParamAd_Group = "ad_group";
        public const string ParamAd_Pos = "ad_pos";
        public const string ParamAd_LoadTime = "ad_load_time";
        public const string ParamAd_HasAd = "has_ad";
        public const string ParamAd_Revenue = "value";
        public const string ParamAd_CurrencyCode = "currency";

        #region System

        public static string GroupTargetHint(string groupName)
            => string.IsNullOrEmpty(groupName) ? "" : ConvertTrackingTextToken(groupName);

        public static string PosTargetHint(string pos)
            => string.IsNullOrEmpty(pos) ? "" : ConvertTrackingTextToken(pos);

        public static string ConvertTrackingTextToken(string rawText)
            => string.IsNullOrEmpty(rawText) ? string.Empty : MatrixTextToken(rawText);

        public static string ConvertTrackingIdToken(string rawId, GroupAdType adType = GroupAdType.Other, bool includeAdTypePrefix = false)
        {
            string token = StableIdToken(rawId);
            return includeAdTypePrefix ? GroupTypeToken(adType) + token : token;
        }

        internal static string DebugPreviewGroupToken(string groupName)
            => ConvertTrackingTextToken(groupName);

        internal static string DebugPreviewPosToken(string pos)
            => ConvertTrackingTextToken(pos);

        internal static string DebugPreviewRawId(string identitySource)
            => ExtractIdentityIdSource(identitySource);

        internal static string DebugPreviewShortId(GroupAdType adType, string identitySource)
            => ConvertTrackingIdToken(ExtractIdentityIdSource(identitySource), adType, includeAdTypePrefix: true);

        internal static string DebugPreviewRequestIdentity(GroupAdType adType, string identitySource)
        {
            string gid = BuildGroupIdentityToken(adType, identitySource);
            string target = BuildGroupRequestTargetToken(adType, identitySource);
            return string.IsNullOrEmpty(target) ? gid : $"{gid} | {target}";
        }

        public static string DefaultRectPos(GroupAdType adType)
        {
            return adType switch
            {
                GroupAdType.Banner => BannerDefaultPos,
                GroupAdType.Mrec => MrecDefaultPos,
                GroupAdType.Collap => CollapDefaultPos,
                _ => "main"
            };
        }

        public static string BannerPlacementToken(BannerPlacement placement)
        {
            return placement switch
            {
                BannerPlacement.FullBottom => "fb",
                BannerPlacement.FullTop => "ft",
                BannerPlacement.TopLeft => "tl",
                BannerPlacement.TopRight => "tr",
                BannerPlacement.BottomLeft => "bl",
                BannerPlacement.BottomRight => "br",
                _ => BannerDefaultPos
            };
        }

        public static void RequestSystemEntry(Channel channel, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("sy", channel, "rq", target: targetHint),
                Module.track_system,
                channel,
                TrackingHistoryAction.Request,
                TrackingHistoryScope.System);

        public static void AutoShowSystemEntry(Channel channel, string targetHint = "", GroupAdType? resolvedAdType = null, string resolvedIdentitySource = "")
            => Fire(
                BuildChannelEntryEvent("atsy", channel, "sh", targetHint, resolvedAdType, resolvedIdentitySource),
                Module.track_system,
                channel,
                TrackingHistoryAction.Show,
                TrackingHistoryScope.System);

        public static void AutoShowSystemFail(Channel channel, TrackingReason reason, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("atsy", channel, "sh", target: targetHint, reason: reason),
                Module.track_system,
                channel,
                TrackingHistoryAction.Show,
                TrackingHistoryScope.System);

        public static void RequestSystemFail(Channel channel, TrackingReason reason, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("sy", channel, "rq", target: targetHint, reason: reason),
                Module.track_system,
                channel,
                TrackingHistoryAction.Request,
                TrackingHistoryScope.System);

        public static void ShowSystemEntry(Channel channel, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("sy", channel, "sh", target: targetHint),
                Module.track_system,
                channel,
                TrackingHistoryAction.Show,
                TrackingHistoryScope.System);

        public static void ShowSystemFail(Channel channel, TrackingReason reason, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("sy", channel, "sh", target: targetHint, reason: reason),
                Module.track_system,
                channel,
                TrackingHistoryAction.Show,
                TrackingHistoryScope.System);

        public static void ActivateSystemEntry(Channel channel, string targetHint = "", GroupAdType? resolvedAdType = null, string resolvedIdentitySource = "")
            => Fire(
                BuildChannelEntryEvent("sy", channel, "act", targetHint, resolvedAdType, resolvedIdentitySource),
                Module.track_system,
                channel,
                TrackingHistoryAction.Activate,
                TrackingHistoryScope.System);

        public static void ActivateSystemFail(Channel channel, TrackingReason reason, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("sy", channel, "act", target: targetHint, reason: reason),
                Module.track_system,
                channel,
                TrackingHistoryAction.Activate,
                TrackingHistoryScope.System);

        public static void HideSystemEntry(Channel channel, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("sy", channel, "hid", target: targetHint),
                Module.track_system,
                channel,
                TrackingHistoryAction.Hide,
                TrackingHistoryScope.System);

        #endregion

        #region AdCore

        public static void RequestAdCoreResolveFail(Channel channel, TrackingReason reason, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("ac", channel, "rq", target: targetHint, reason: reason),
                Module.track_adcore,
                channel,
                TrackingHistoryAction.Request,
                TrackingHistoryScope.AdCore);

        public static void ShowAdCoreResolveFail(Channel channel, TrackingReason reason, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("ac", channel, "sh", target: targetHint, reason: reason),
                Module.track_adcore,
                channel,
                TrackingHistoryAction.Show,
                TrackingHistoryScope.AdCore);

        public static void ActivateAdCoreResolveFail(Channel channel, TrackingReason reason, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("ac", channel, "act", target: targetHint, reason: reason),
                Module.track_adcore,
                channel,
                TrackingHistoryAction.Activate,
                TrackingHistoryScope.AdCore);

        public static void HideAdCoreResolveFail(Channel channel, TrackingReason reason, string targetHint = "")
            => Fire(
                BuildChannelCommandEvent("ac", channel, "hid", target: targetHint, reason: reason),
                Module.track_adcore,
                channel,
                TrackingHistoryAction.Hide,
                TrackingHistoryScope.AdCore);

        #endregion

        #region Group

        public static void RequestGroupFail(GroupAdType adType, string adUnitId, GroupRequestContext context,
            TrackingReason reason, Channel? trackingChannel = null, string targetHint = "", int retryAttempt = 0)
            => Fire(
                BuildRequestGroupFailEvent(adType, adUnitId, context, reason, trackingChannel, targetHint, retryAttempt),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Request,
                TrackingHistoryScope.Group);

        public static void RequestGroupReachedApi(GroupAdType adType, string adUnitId, GroupRequestContext context,
            Channel? trackingChannel = null, string targetHint = "", int retryAttempt = 0)
            => Fire(
                BuildRequestGroupApiEvent(adType, adUnitId, context, trackingChannel, targetHint, retryAttempt),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Request,
                TrackingHistoryScope.Group);

        public static void LoadGroupSuccess(GroupAdType adType, string adUnitId, GroupRequestContext context, int loadTimeMs,
            Channel? trackingChannel = null, string targetHint = "", int retryAttempt = 0)
            => Fire(
                BuildCallbackLoadEvent(adType, adUnitId, context, $"ls_{SpeedToken(loadTimeMs)}", targetHint, retryAttempt),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Load,
                TrackingHistoryScope.Group);

        public static void LoadGroupRefreshSuccess(GroupAdType adType, string adUnitId, GroupRequestContext context,
            Channel? trackingChannel = null, string targetHint = "", int retryAttempt = 0)
            => Fire(
                BuildCallbackLoadEvent(adType, adUnitId, context, "ls_rl", targetHint, retryAttempt),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Load,
                TrackingHistoryScope.Group);

        public static void LoadGroupFail(GroupAdType adType, string adUnitId, GroupRequestContext context, int errorCode, bool netOnline,
            int loadTimeMs, Channel? trackingChannel = null, string targetHint = "", int retryAttempt = 0)
            => Fire(
                BuildCallbackLoadEvent(adType, adUnitId, context, $"lf_ec{ClampErrorCode(errorCode)}_net{(netOnline ? 1 : 0)}_{SpeedToken(loadTimeMs)}", targetHint, retryAttempt),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Load,
                TrackingHistoryScope.Group);

        public static void LoadGroupFailNoSpeed(GroupAdType adType, string adUnitId, GroupRequestContext context, int errorCode, bool netOnline,
            Channel? trackingChannel = null, string targetHint = "", int retryAttempt = 0)
            => Fire(
                BuildCallbackLoadEvent(adType, adUnitId, context, $"lf_ec{ClampErrorCode(errorCode)}_net{(netOnline ? 1 : 0)}", targetHint, retryAttempt),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Load,
                TrackingHistoryScope.Group);

        public static void ShowGroupFail(GroupAdType adType, string pos, TrackingReason reason,
            Channel? trackingChannel = null)
            => Fire(
                BuildCommandEvent("gr", null, "sh", identityToken: "", contextToken: "", target: PosToken(pos), reason: reason, reachedApi: false),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Show,
                TrackingHistoryScope.Group);

        public static void ShowGroupFail(GroupAdType adType, string adUnitId, string pos, TrackingReason reason,
            Channel? trackingChannel = null)
            => Fire(
                BuildGroupCommandEvent("sh", adType, adUnitId, PosToken(pos), reason: reason),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Show,
                TrackingHistoryScope.Group);

        public static void ShowGroupApi(GroupAdType adType, string adUnitId, string pos,
            Channel? trackingChannel = null)
            => Fire(
                BuildGroupCommandEvent("sh", adType, adUnitId, PosToken(pos), reachedApi: true),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Show,
                TrackingHistoryScope.Group);

        public static void ActivateGroupFail(GroupAdType adType, string adUnitId, string pos, TrackingReason reason,
            Channel? trackingChannel = null)
            => Fire(
                BuildGroupCommandEvent("act", adType, adUnitId, PosToken(pos), reason: reason),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Activate,
                TrackingHistoryScope.Group);

        public static void ActivateGroupApi(GroupAdType adType, string adUnitId, string pos,
            Channel? trackingChannel = null)
            => Fire(
                BuildGroupCommandEvent("act", adType, adUnitId, PosToken(pos), reachedApi: true),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Activate,
                TrackingHistoryScope.Group);

        public static void HideGroupFail(GroupAdType adType, string adUnitId, string pos, TrackingReason reason,
            Channel? trackingChannel = null)
            => Fire(
                BuildGroupCommandEvent("hid", adType, adUnitId, PosToken(pos), reason: reason),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Hide,
                TrackingHistoryScope.Group);

        public static void HideGroupApi(GroupAdType adType, string adUnitId, string pos,
            Channel? trackingChannel = null)
            => Fire(
                BuildGroupCommandEvent("hid", adType, adUnitId, PosToken(pos), reachedApi: true),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Hide,
                TrackingHistoryScope.Group);

        public static void TrackGroupImpression(GroupAdType adType, string adUnitId, string pos,
            Channel? trackingChannel = null)
            => Fire(
                BuildCallbackEvent(adType, adUnitId, "imp", pos),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Event,
                TrackingHistoryScope.Group);

        public static void TrackGroupClick(GroupAdType adType, string adUnitId, string pos,
            Channel? trackingChannel = null)
            => Fire(
                BuildCallbackEvent(adType, adUnitId, "clk", pos),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Event,
                TrackingHistoryScope.Group);

        public static void TrackGroupDisplayed(GroupAdType adType, string adUnitId, string pos,
            Channel? trackingChannel = null)
            => Fire(
                BuildCallbackEvent(adType, adUnitId, "dsp", pos),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Event,
                TrackingHistoryScope.Group);

        public static void TrackGroupClosed(GroupAdType adType, string adUnitId, string pos,
            Channel? trackingChannel = null)
            => Fire(
                BuildCallbackEvent(adType, adUnitId, "cls", pos),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Event,
                TrackingHistoryScope.Group);

        public static void TrackGroupRewarded(GroupAdType adType, string adUnitId, string pos,
            Channel? trackingChannel = null)
            => Fire(
                BuildCallbackEvent(adType, adUnitId, "rwd", pos),
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Event,
                TrackingHistoryScope.Group);

        public static void TrackGroupShowFailed(GroupAdType adType, string adUnitId, string pos,
            int errorCode = int.MinValue, Channel? trackingChannel = null)
        {
            string tail = BuildCallbackEvent(adType, adUnitId, "shf", pos, ErrorNetToken(errorCode, NetworkMonitor.Current.CheckNetwork));

            Fire(
                tail,
                Module.track_group,
                trackingChannel,
                TrackingHistoryAction.Event,
                TrackingHistoryScope.Group);
        }

        #endregion

        #region Impression

        public static void TrackAdImpression(AdInfo adInfo, AdValueInfo adValue)
        {
            TrackAdjustAdRevenue(
                adInfo.mediation,
                adValue.adRevenue,
                adValue.currencyCode,
                adInfo.adSource,
                adInfo.id,
                adInfo.pos);

            var parameters = BuildAdRevenueParams(adInfo, adValue);
            if (ShouldTrackFirebaseAdImpression(adInfo.mediation))
            {
                Fire(
                    FirebaseAnalytics.EventAdImpression,
                    parameters,
                    Module.track_revenue,
                    BuildAdRevenueDebugDetails(adInfo, adValue),
                    null,
                    TrackingHistoryAction.Revenue,
                    TrackingHistoryScope.Revenue);
            }

            if (!AdsLogic.IsTrackingBgAdImpression)
                return;

            Fire(
                EventBgAdImpression,
                parameters,
                Module.track_revenue,
                BuildAdRevenueDebugDetails(adInfo, adValue),
                null,
                TrackingHistoryAction.Revenue,
                TrackingHistoryScope.Revenue);
        }

        #endregion

        #region Internal

        private static void Fire(string eventName, Module debugModule, Channel? channel,
            TrackingHistoryAction action, TrackingHistoryScope scope)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            if (!ShouldTrackHistoryAction(channel, action))
                return;

            OnTrackedRawEvent?.Invoke(eventName);
            NetFlowDebugSystem.LogTracking(debugModule, eventName);
            Emit?.Invoke(eventName);
        }

        private static void Fire(string eventName, Parameter[] parameters, Module debugModule,
            Action<NetFlowDebugSystem.Detail> details, Channel? channel, TrackingHistoryAction action,
            TrackingHistoryScope scope)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            if (!ShouldTrackHistoryAction(channel, action))
                return;

            OnTrackedRawEvent?.Invoke(eventName);
            NetFlowDebugSystem.LogTracking(debugModule, eventName, isRevenue: true, details: details);
            EmitWithParams?.Invoke(eventName, parameters ?? Array.Empty<Parameter>());
        }

        private static Action<NetFlowDebugSystem.Detail> BuildAdRevenueDebugDetails(AdInfo adInfo, AdValueInfo adValue)
        {
            if (!NetFlowDebugSystem.EnableTrackingParamDetails)
                return null;

            return detail =>
            {
                detail.AddKV(ParamAd_UnitId, adInfo.id ?? "");
                detail.AddKV(ParamAd_Format, adInfo.adtype ?? "");
                detail.AddKV(ParamAd_Mediation, adInfo.mediation ?? "");
                detail.AddKV(ParamAd_AdSource, adInfo.adSource ?? "");
                detail.AddKV(ParamAd_Group, adInfo.group ?? "");
                detail.AddKV(ParamAd_Pos, adInfo.pos ?? "");
                detail.AddKV(ParamAd_LoadTime, adInfo.loadTime);
                detail.AddKV(ParamAd_Revenue, adValue.adRevenue);
                detail.AddKV(ParamAd_CurrencyCode, adValue.currencyCode ?? "");
            };
        }

        private static string BuildChannelCommandEvent(string layerToken, Channel channel, string actionToken,
            string target = "", TrackingReason? reason = null, bool reachedApi = false, string identityToken = "")
            => BuildCommandEvent(layerToken, channel, actionToken, identityToken, contextToken: "", target, reason, reachedApi);

        private static string BuildChannelEntryEvent(string layerToken, Channel channel, string actionToken,
            string targetHint, GroupAdType? resolvedAdType, string resolvedIdentitySource)
        {
            string identityToken = string.Empty;
            if (string.IsNullOrEmpty(targetHint) && resolvedAdType.HasValue && !string.IsNullOrEmpty(resolvedIdentitySource))
                identityToken = BuildGroupIdentityToken(resolvedAdType.Value, resolvedIdentitySource);

            return BuildChannelCommandEvent(layerToken, channel, actionToken, targetHint, identityToken: identityToken);
        }

        private static string BuildGroupCommandEvent(string actionToken, GroupAdType adType, string identitySource,
            string target, TrackingReason? reason = null, bool reachedApi = false)
            => BuildCommandEvent("gr", null, actionToken, BuildGroupIdentityToken(adType, identitySource), contextToken: "", target, reason, reachedApi);

        private static string BuildCallbackLoadEvent(GroupAdType adType, string identitySource, GroupRequestContext context, string resultTail, string targetOverride = "", int retryAttempt = 0)
        {
            string gid = BuildGroupIdentityToken(adType, identitySource);
            string target = string.IsNullOrEmpty(targetOverride) ? BuildGroupRequestTargetToken(adType, identitySource) : PosToken(targetOverride);
            string[] resultTokens = string.IsNullOrEmpty(resultTail) ? Array.Empty<string>() : resultTail.Split('_');

            var parts = new List<string> { "ad", "evt" };
            if (resultTokens.Length > 0)
                parts.Add(resultTokens[0]);
            parts.Add("rq");
            parts.Add(gid);
            parts.Add(ContextToken(context, retryAttempt));
            if (!string.IsNullOrEmpty(target))
                parts.Add(target);
            for (int i = 1; i < resultTokens.Length; i++)
                parts.Add(resultTokens[i]);
            return SanitizeAndClamp(string.Join("_", parts));
        }

        private static string BuildCallbackEvent(GroupAdType adType, string identitySource, string callbackToken, string pos, string extraTail = "")
        {
            var parts = new List<string>
            {
                "ad",
                "evt",
                callbackToken,
                "sh",
                BuildGroupIdentityToken(adType, identitySource),
                PosToken(pos)
            };

            if (!string.IsNullOrEmpty(extraTail))
            {
                string[] extraTokens = extraTail.Split('_');
                for (int i = 0; i < extraTokens.Length; i++)
                    parts.Add(extraTokens[i]);
            }

            return SanitizeAndClamp(string.Join("_", parts));
        }

        private static string BuildGroupIdentityToken(GroupAdType adType, string identitySource)
            => GroupTypeToken(adType) + StableIdToken(ExtractIdentityIdSource(identitySource));

        private static string BuildRequestGroupFailEvent(GroupAdType adType, string identitySource, GroupRequestContext context, TrackingReason reason, Channel? trackingChannel, string targetOverride = "", int retryAttempt = 0)
        {
            return BuildCommandEvent(
                "gr",
                trackingChannel,
                "rq",
                BuildGroupIdentityToken(adType, identitySource),
                ContextToken(context, retryAttempt),
                string.IsNullOrEmpty(targetOverride) ? BuildGroupRequestTargetToken(adType, identitySource) : PosToken(targetOverride),
                reason,
                reachedApi: false);
        }

        private static string BuildRequestGroupApiEvent(GroupAdType adType, string identitySource, GroupRequestContext context, Channel? trackingChannel, string targetOverride = "", int retryAttempt = 0)
        {
            return BuildCommandEvent(
                "gr",
                trackingChannel,
                "rq",
                BuildGroupIdentityToken(adType, identitySource),
                ContextToken(context, retryAttempt),
                string.IsNullOrEmpty(targetOverride) ? BuildGroupRequestTargetToken(adType, identitySource) : PosToken(targetOverride),
                reason: null,
                reachedApi: true);
        }

        private static string BuildCommandEvent(string layerToken, Channel? channel, string actionToken, string identityToken,
            string contextToken, string target, TrackingReason? reason, bool reachedApi)
        {
            var parts = new List<string> { "ad", layerToken };
            if (channel.HasValue)
                parts.Add(ChannelToken(channel.Value));

            parts.Add(actionToken);

            if (!string.IsNullOrEmpty(identityToken))
                parts.Add(identityToken);

            bool hasContext = !string.IsNullOrEmpty(contextToken);
            bool hasTarget = !string.IsNullOrEmpty(target);
            if (hasContext)
                parts.Add(contextToken);

            if (!hasContext && hasTarget)
                parts.Add(target);

            if (reason.HasValue)
            {
                parts.Add("n");
                parts.Add(ReasonToken(reason.Value));
            }

            if (hasContext && hasTarget)
                parts.Add(target);

            if (reachedApi)
                parts.Add("api");

            return SanitizeAndClamp(string.Join("_", parts));
        }

        private static string BuildGroupRequestTargetToken(GroupAdType adType, string identitySource)
            => ExtractIdentityGroupNameToken(adType, identitySource);

        private static string ChannelToken(Channel channel)
        {
            return channel switch
            {
                Channel.AppLaunch => "al",
                Channel.AppResume => "ar",
                Channel.ForceAd => "fa",
                Channel.Rewarded => "rw",
                Channel.Banner => "bn",
                Channel.Mrec => "mr",
                Channel.Popup => "pu",
                Channel.Collap => "cl",
                _ => "xx"
            };
        }

        private static string GroupTypeToken(GroupAdType adType)
        {
            return adType switch
            {
                GroupAdType.AppOpen => "ao",
                GroupAdType.ForceAd => "fa",
                GroupAdType.Rewarded => "rw",
                GroupAdType.Banner => "bn",
                GroupAdType.Mrec => "mr",
                GroupAdType.Popup => "pu",
                GroupAdType.Collap => "cl",
                _ => "ot"
            };
        }

        private static string ContextToken(GroupRequestContext context, int retryAttempt = 0)
        {
            return context switch
            {
                GroupRequestContext.Init => "ini",
                GroupRequestContext.Retry => $"rt{Math.Max(1, retryAttempt)}",
                GroupRequestContext.Idle => "idl",
                GroupRequestContext.HideReload => "hdr",
                GroupRequestContext.DisplayFailReload => "sfr",
                _ => "oth"
            };
        }

        private static string ReasonToken(TrackingReason reason)
        {
            return reason switch
            {
                TrackingReason.Disabled => "dis",
                TrackingReason.ConfigDisabled => "cfg",
                TrackingReason.IapRemoved => "iap",
                TrackingReason.PositionBlocked => "pos",
                TrackingReason.CappingBlocked => "cap",
                TrackingReason.Ignored => "ign",
                TrackingReason.GateBlocked => "gate",
                TrackingReason.Blocked => "block",
                TrackingReason.GroupMissing => "group",
                TrackingReason.NotReady => "nready",
                TrackingReason.Timeout => "tmo",
                TrackingReason.BlockedByFullscreen => "fsblk",
                TrackingReason.RemoteConfigPending => "rcfg",
                TrackingReason.AlreadyInitialized => "inited",
                TrackingReason.MissingAdUnitId => "noid",
                TrackingReason.AlreadyLoaded => "aload",
                TrackingReason.LoadingInProgress => "loading",
                TrackingReason.RetryWaiting => "retry",
                TrackingReason.BudgetExhausted => "budget",
                TrackingReason.HostBlocked => "host",
                TrackingReason.AlreadyShowing => "showing",
                TrackingReason.NotShowing => "nshowing",
                TrackingReason.NullData => "null",
                TrackingReason.UpdatePositionRequired => "upos",
                TrackingReason.InvalidLayoutSize => "size0",
                _ => "oth"
            };
        }

        private static string GroupToken(string group)
        {
            string token = MatrixTextToken(group);
            if (string.IsNullOrEmpty(token))
                return "group";

            return token;
        }

        private static string PosToken(string pos)
        {
            string token = MatrixTextToken(pos);
            if (string.IsNullOrEmpty(token))
                return "main";

            if (token.Length > MaxPosTokenLen)
                token = token.Substring(0, MaxPosTokenLen);

            return token;
        }

        private static string StableIdToken(string rawId)
        {
            string token = SanitizeStableToken(rawId);
            if (string.IsNullOrEmpty(token))
                return new string('0', MaxGroupIdTokenLen);

            string digitsOnly = ExtractDigitsOnly(token);
            string hintSource = string.IsNullOrEmpty(digitsOnly) ? token : digitsOnly;
            string hint = hintSource.Length >= 3
                ? hintSource.Substring(hintSource.Length - 3, 3)
                : hintSource.PadLeft(3, '0');

            string hashLetters = StableHashLetters(token, 2);
            string combined = $"{hint}{hashLetters}";

            if (combined.Length > MaxGroupIdTokenLen)
                combined = combined.Substring(combined.Length - MaxGroupIdTokenLen, MaxGroupIdTokenLen);

            return combined;
        }

        private static string SpeedToken(int ms)
        {
            if (ms < 5000) return "f";
            if (ms <= 10000) return "n";
            if (ms <= 15000) return "s";
            return "x";
        }

        private static int ClampErrorCode(int errorCode)
        {
            if (errorCode < 0)
                errorCode = -errorCode;

            if (errorCode > 9999)
                errorCode = 9999;

            return errorCode;
        }

        private static string ErrorNetToken(int errorCode, bool netOnline)
        {
            string errorToken = errorCode == int.MinValue
                ? "noec"
                : $"ec{ClampErrorCode(errorCode)}";

            return $"{errorToken}_net{(netOnline ? 1 : 0)}";
        }

        private static Parameter[] BuildAdRevenueParams(AdInfo adInfo, AdValueInfo adValue)
        {
            return new[]
            {
                new Parameter(ParamAd_UnitId, adInfo.id ?? ""),
                new Parameter(ParamAd_Format, adInfo.adtype ?? ""),
                new Parameter(ParamAd_Mediation, adInfo.mediation ?? ""),
                new Parameter(ParamAd_AdSource, adInfo.adSource ?? ""),
                new Parameter(ParamAd_Group, adInfo.group ?? ""),
                new Parameter(ParamAd_Pos, adInfo.pos ?? ""),
                new Parameter(ParamAd_LoadTime, adInfo.loadTime),
                new Parameter(ParamAd_Revenue, adValue.adRevenue),
                new Parameter(ParamAd_CurrencyCode, adValue.currencyCode ?? "")
            };
        }

        private static void TrackAdjustAdRevenue(string platform, double adRevenue, string currencyCode,
            string adSource, string adUnitId, string adPlacement)
        {
            if (IsGoogleMediation(platform))
                platform = BG_ConstValue.mediation_admob;

            var revenue = new AdjustAdRevenue(platform);
            revenue.SetRevenue(adRevenue, currencyCode);
            revenue.AdRevenueNetwork = adSource;
            revenue.AdRevenueUnit = adUnitId;
            revenue.AdRevenuePlacement = adPlacement;
            Adjust.TrackAdRevenue(revenue);
        }

        private static bool ShouldTrackFirebaseAdImpression(string mediation)
        {
            if (AdsLogic.IsAlwaysTrackingRev)
                return true;

            return !IsGoogleMediation(mediation);
        }

        private static bool IsGoogleMediation(string mediation)
        {
            return mediation == BG_ConstValue.mediation_android
                || mediation == BG_ConstValue.mediation_admob
                || mediation == BG_ConstValue.mediation_ios;
        }

        private static bool ShouldTrackHistoryAction(Channel? channel, TrackingHistoryAction action)
        {
            return action switch
            {
                TrackingHistoryAction.Request => AdsLogic.ShouldTrackChannel(channel),
                TrackingHistoryAction.Load => AdsLogic.ShouldTrackChannel(channel),
                TrackingHistoryAction.Show => AdsLogic.ShouldTrackChannel(channel),
                TrackingHistoryAction.Hide => AdsLogic.ShouldTrackChannel(channel),
                TrackingHistoryAction.Activate => AdsLogic.ShouldTrackChannel(channel),
                TrackingHistoryAction.Event => AdsLogic.ShouldTrackChannel(channel),
                _ => true
            };
        }

        private static string StableHashLetters(string value, int width)
        {
            unchecked
            {
                uint hash = 2166136261;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619;
                }

                var buffer = new char[Math.Max(width, 1)];
                for (int i = buffer.Length - 1; i >= 0; i--)
                {
                    buffer[i] = (char)('a' + (hash % 26));
                    hash /= 26;
                }

                return new string(buffer);
            }
        }

        private static string SanitizeAndClamp(string name)
        {
            string token = SanitizeEventName(name);
            if (!token.StartsWith("ad_"))
                token = "ad_" + token;

            if (token.Length > MaxEventNameLen)
                token = token.Substring(0, MaxEventNameLen);

            while (token.Length > 0 && token[token.Length - 1] == '_')
                token = token.Substring(0, token.Length - 1);

            return token.Length == 0 ? "ad_x" : token;
        }

        private static string MatrixTextToken(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "";

            List<string> words = SplitTrackingWords(raw);
            if (words.Count == 0)
                return "";

            List<string> compactWords = CompactTrackingWords(words, MaxGroupTokenLen);
            string token = JoinTrackingWords(compactWords);

            if (token.Length > MaxGroupTokenLen)
                token = token.Substring(0, MaxGroupTokenLen);

            return token;
        }

        private static List<string> SplitTrackingWords(string raw)
        {
            var words = new List<string>();
            if (string.IsNullOrEmpty(raw))
                return words;

            var builder = new StringBuilder(raw.Length);
            TokenWordKind currentKind = TokenWordKind.None;

            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (!char.IsLetterOrDigit(c))
                {
                    FlushTrackingWord(builder, words);
                    currentKind = TokenWordKind.None;
                    continue;
                }

                TokenWordKind nextKind = char.IsDigit(c) ? TokenWordKind.Number : TokenWordKind.Text;
                bool shouldStartNewWord = builder.Length > 0 &&
                                          (currentKind != nextKind ||
                                           (nextKind == TokenWordKind.Text && char.IsUpper(c)));
                if (shouldStartNewWord)
                {
                    FlushTrackingWord(builder, words);
                    currentKind = TokenWordKind.None;
                }

                builder.Append(char.ToLowerInvariant(c));
                currentKind = nextKind;
            }

            FlushTrackingWord(builder, words);
            return words;
        }

        private static void FlushTrackingWord(StringBuilder builder, List<string> words)
        {
            if (builder.Length == 0)
                return;

            words.Add(builder.ToString());
            builder.Length = 0;
        }

        private static List<string> CompactTrackingWords(List<string> words, int maxLength)
        {
            var compactWords = new List<string>(words);
            if (JoinTrackingWords(compactWords).Length <= maxLength)
                return compactWords;

            while (JoinTrackingWords(compactWords).Length > maxLength)
            {
                bool anyTrimmed = false;

                for (int i = 0; i < compactWords.Count; i++)
                {
                    string trimmed = TryTrimTrackingWordSoft(compactWords[i]);
                    if (!string.Equals(trimmed, compactWords[i], StringComparison.Ordinal))
                    {
                        compactWords[i] = trimmed;
                        anyTrimmed = true;
                    }
                }

                if (!anyTrimmed)
                    break;
            }

            string compactToken = JoinTrackingWords(compactWords);
            if (compactToken.Length <= maxLength)
                return compactWords;

            int overflow = compactToken.Length - maxLength;
            for (int i = compactWords.Count - 1; i >= 0 && overflow > 0; i--)
            {
                int removable = compactWords[i].Length - 1;
                if (removable <= 0)
                    continue;

                int cutCount = Math.Min(removable, overflow);
                compactWords[i] = compactWords[i].Substring(0, compactWords[i].Length - cutCount);
                overflow -= cutCount;
            }

            return compactWords;
        }

        private static string TryTrimTrackingWordSoft(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length <= 1 || IsDigitsOnly(word))
                return word;

            int trailingDigitStart = word.Length;
            while (trailingDigitStart > 0 && char.IsDigit(word[trailingDigitStart - 1]))
                trailingDigitStart--;

            if (trailingDigitStart < word.Length)
            {
                if (trailingDigitStart <= 1)
                    return word;

                return word.Remove(trailingDigitStart - 1, 1);
            }

            return word.Substring(0, word.Length - 1);
        }

        private static bool IsDigitsOnly(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsDigit(value[i]))
                    return false;
            }

            return true;
        }

        private static string JoinTrackingWords(List<string> words)
        {
            if (words == null || words.Count == 0)
                return "";

            var builder = new StringBuilder();
            for (int i = 0; i < words.Count; i++)
            {
                string word = words[i];
                if (string.IsNullOrEmpty(word))
                    continue;

                if (builder.Length == 0)
                {
                    builder.Append(word);
                    continue;
                }

                if (char.IsLetter(word[0]))
                {
                    builder.Append(char.ToUpperInvariant(word[0]));
                    if (word.Length > 1)
                        builder.Append(word.Substring(1));
                }
                else
                {
                    builder.Append(word);
                }
            }

            return builder.ToString();
        }

        private static string SanitizeStableToken(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "";

            var builder = new StringBuilder(raw.Length);

            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (c >= 'A' && c <= 'Z')
                    c = (char)(c + 32);

                bool isAllowed = (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9');
                if (isAllowed)
                    builder.Append(c);
            }

            return builder.ToString();
        }

        private static string ExtractDigitsOnly(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "";

            var builder = new StringBuilder(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (c >= '0' && c <= '9')
                    builder.Append(c);
            }

            return builder.ToString();
        }

        private enum TokenWordKind
        {
            None,
            Text,
            Number
        }

        private static string SanitizeEventName(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return "";

            var builder = new StringBuilder(raw.Length);
            bool lastWasUnderscore = false;

            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];

                bool isAllowed =
                    (c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    (c >= '0' && c <= '9');
                if (isAllowed)
                {
                    builder.Append(c);
                    lastWasUnderscore = false;
                    continue;
                }

                if (lastWasUnderscore)
                    continue;

                builder.Append('_');
                lastWasUnderscore = true;
            }

            int start = 0;
            int end = builder.Length;
            while (start < end && builder[start] == '_') start++;
            while (end > start && builder[end - 1] == '_') end--;

            if (end <= start)
                return "";

            return builder.ToString(start, end - start);
        }

        private static string ExtractIdentityIdSource(string identitySource)
        {
            if (string.IsNullOrEmpty(identitySource))
                return "";

            int sep = identitySource.IndexOf('|');
            if (sep < 0 || sep + 1 >= identitySource.Length)
                return identitySource;

            return identitySource.Substring(sep + 1);
        }

        private static string ExtractIdentityGroupNameToken(GroupAdType adType, string identitySource)
        {
            if (adType != GroupAdType.ForceAd && adType != GroupAdType.Popup)
                return "";

            if (string.IsNullOrEmpty(identitySource))
                return "";

            int sep = identitySource.IndexOf('|');
            if (sep <= 0)
                return "";

            return GroupToken(identitySource.Substring(0, sep));
        }

        #endregion
    }
}
