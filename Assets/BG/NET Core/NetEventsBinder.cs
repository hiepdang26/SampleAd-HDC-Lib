// =======================
// NetEventsBinder.cs
// =======================
using BG_Library.NET.AdSystem;
using BG_Library.NET.API;
using BG_Library.NET.Mediation.Admob;
using BG_Library.NET.Mediation.Max;
using BG_Library.NET.Tracking;

namespace BG_Library.NET
{
    public static class NetEventsBinder
    {
        private static BannerPlacement ToBannerPlacement(NetCallerBannerPlacement placement)
        {
            return placement switch
            {
                NetCallerBannerPlacement.FullBottom => BannerPlacement.FullBottom,
                NetCallerBannerPlacement.FullTop => BannerPlacement.FullTop,
                NetCallerBannerPlacement.TopLeft => BannerPlacement.TopLeft,
                NetCallerBannerPlacement.TopRight => BannerPlacement.TopRight,
                NetCallerBannerPlacement.BottomLeft => BannerPlacement.BottomLeft,
                NetCallerBannerPlacement.BottomRight => BannerPlacement.BottomRight,
                _ => BannerPlacement.FullBottom
            };
        }

        private static bool ShouldSendTrackingToFirebase()
        {
            var configs = NetConfigsSO.Ins;
            if (configs == null)
                return false;

            return configs.Tracking_SendToFirebase;
        }

        private static void ConfigureTrackingRouting(NetEventsHub hub)
        {
            if (hub == null)
                return;

            hub.SetUserProperty = (propertiesName, value) => FirebaseAnalyticsBridge.SetUserProperty(propertiesName, value);

            if (ShouldSendTrackingToFirebase())
            {
                hub.LogEvent = (eventName) => FirebaseAnalyticsBridge.LogEvent(eventName);
                hub.LogEvent_Params = (eventName, parameters) => FirebaseAnalyticsBridge.LogEvent(eventName, parameters);
                NetTrackingSystem.Emit = (eventName) => FirebaseAnalyticsBridge.LogEvent(eventName);
                NetTrackingSystem.EmitWithParams = (eventName, parameters) => FirebaseAnalyticsBridge.LogEvent(eventName, parameters);
                return;
            }

            hub.LogEvent = _ => { };
            hub.LogEvent_Params = (_, __) => { };
            NetTrackingSystem.Emit = _ => { };
            NetTrackingSystem.EmitWithParams = (_, __) => { };
        }

        public static void RefreshTrackingRouting()
        {
            ConfigureTrackingRouting(NetHubLocator.GetHub());
        }

        public static void Bind()
        {
            var hub = NetHubLocator.GetHub();
            if (hub == null) return;

            // Nếu bạn muốn chắc chắn không bị bind nhầm / bind chồng:
            hub.ClearAllDelegates();
            NetTrackingSystem.Emit = null;
            NetTrackingSystem.EmitWithParams = null;

            // =========================
            // Remote Configs
            // =========================
            hub.RemoteConfig_GetCustom = (key) => RemoteConfig.Ins.GetCustomRemoteConfigs(key);
            hub.RemoteConfig_IsFetchComplete = () => RemoteConfig.Ins.IsDataFetched;
            hub.RemoteConfig_SubAllJsonsComplete = (handler) =>
            {
                if (handler == null) return;
                RemoteConfig.OnAllJsonsComplete -= handler;
                RemoteConfig.OnAllJsonsComplete += handler;
            };
            hub.RemoteConfig_UnsubAllJsonsComplete = (handler) =>
            {
                if (handler == null) return;
                RemoteConfig.OnAllJsonsComplete -= handler;
            };

            // =========================
            // Tracking
            // =========================
            ConfigureTrackingRouting(hub);

            // =========================
            // Common
            // =========================
            hub.IsRemovedAd = () => AdsLogic.IsRemovedAd;
            hub.AdmobMediation_IsInitComplete = () => Admob_MediationManager.IsInitComplete;
            hub.MaxMediation_IsInitComplete = () => Max_MediationManager.IsInitComplete;

            // =========================
            // Remove Ads (IAP)
            // =========================
            hub.PurchaseRemoveAds = () => AdsLogic.Ins.PurchaseRemoveAds();
            hub.RevertPurchaseRemoveAds = () => AdsLogic.Ins.RevertPurchaseRemoveAds();

            // =========================
            // Ads
            // =========================

            // ---- AL ----
            hub.AL_InitManually = () => AdsLogic.Ins.AL_ManagerIns.InitManually();
            hub.AL_SubOnBeforeAdShow = (handler) =>
            {
                if (handler == null) return;
                AdsLogic.Ins.AL_ManagerIns.OnAdLaunchBeforeShow += handler;
            };
            hub.AL_UnsubOnBeforeAdShow = (handler) =>
            {
                if (handler == null) return;
                AdsLogic.Ins.AL_ManagerIns.OnAdLaunchBeforeShow -= handler;
            };
            hub.AL_SubOnAdComplete = (handler) =>
            {
                if (handler == null) return;
                AdsLogic.Ins.AL_ManagerIns.OnAdLaunchComplete += handler;
            };
            hub.AL_UnsubOnAdComplete = (handler) =>
            {
                if (handler == null) return;
                AdsLogic.Ins.AL_ManagerIns.OnAdLaunchComplete -= handler;
            };
            hub.AL_AbleToShow = () => AdsLogic.Ins.AL_ManagerIns.AbleToShow;

            // ---- AR ----
            hub.AR_InitManually = () => AdsLogic.Ins.AR_ManagerIns.InitManually();

            // ---- FA ----
            hub.FA_InitManually = (groupName) => AdsLogic.Ins.FA_ManagerIns.InitManually(groupName);
            hub.FA_ForceInit = (groupName) => AdsLogic.Ins.FA_ManagerIns.ForceInitManually(groupName);
            hub.FA_Show = (pos, actionDone) => AdsLogic.Ins.FA_ManagerIns.Show(pos, actionDone);
            hub.FA_AbleToShow = (pos) => AdsLogic.Ins.FA_ManagerIns.AbleToShow(pos);
            hub.FA_StartBreakAd = () => AdsLogic.Ins.FA_ManagerIns.StartBreakAd();
            hub.FA_StopBreakAd = () => AdsLogic.Ins.FA_ManagerIns.StopBreakAd();

			// ---- BreakAd Events (ForceAdLogic) ----
			hub.BreakAd_SubNotifyBeforeShow = (handler) =>
			{
				if (handler == null) return;
				AdsLogic.Ins.FA_ManagerIns.BreakAd_OnNotifyBeforeShow += handler;
			};

			hub.BreakAd_UnsubNotifyBeforeShow = (handler) =>
			{
				if (handler == null) return;
				AdsLogic.Ins.FA_ManagerIns.BreakAd_OnNotifyBeforeShow -= handler;
			};

			// ---- RW ----
			hub.RW_InitManually = () => AdsLogic.Ins.RW_ManagerIns.InitManually();
            hub.RW_Show = (pos, onRewardSuccess) => AdsLogic.Ins.RW_ManagerIns.Show(pos, onRewardSuccess);
            hub.RW_AbleToShow = () => AdsLogic.Ins.RW_ManagerIns.AbleToShow;
            hub.RW_GetIgnoreAd = () => AdsLogic.Ins.RW_ManagerIns.IgnoreAd;
            hub.RW_SetIgnoreAd = (value) => AdsLogic.Ins.RW_ManagerIns.IgnoreAd = value;

            // ---- BN ----
            hub.BN_InitManually = () => AdsLogic.Ins.BN_ManagerIns.InitManually(BannerPlacement.FullBottom);
            hub.BN_InitManually_Placement = (placement) => AdsLogic.Ins.BN_ManagerIns.InitManually(ToBannerPlacement(placement));
            hub.BN_Show = () => AdsLogic.Ins.BN_ManagerIns.ActivateView(BannerPlacement.FullBottom);
            hub.BN_Show_Placement = (placement) => AdsLogic.Ins.BN_ManagerIns.ActivateView(ToBannerPlacement(placement));
            hub.BN_ActivateView = () => AdsLogic.Ins.BN_ManagerIns.ActivateView(BannerPlacement.FullBottom);
            hub.BN_ActivateView_Placement = (placement) => AdsLogic.Ins.BN_ManagerIns.ActivateView(ToBannerPlacement(placement));
            hub.BN_Hide = () => AdsLogic.Ins.BN_ManagerIns.Hide(BannerPlacement.FullBottom);
            hub.BN_Hide_Placement = (placement) => AdsLogic.Ins.BN_ManagerIns.Hide(ToBannerPlacement(placement));
            hub.BN_AbleToShow = () => AdsLogic.Ins.BN_ManagerIns.AbleToShowAt(BannerPlacement.FullBottom);
            hub.BN_AbleToShow_Placement = (placement) => AdsLogic.Ins.BN_ManagerIns.AbleToShowAt(ToBannerPlacement(placement));
            hub.BN_Expand = (enableClick) => AdsLogic.Ins.BN_ManagerIns.Expand(BannerPlacement.FullBottom, enableClick);

            // ---- MREC ----
            hub.Mrec_InitManually = () => AdsLogic.Ins.Mrec_ManagerIns.InitManually();
            hub.Mrec_Show = () => AdsLogic.Ins.Mrec_ManagerIns.ActivateView();
            hub.Mrec_ActivateView = () => AdsLogic.Ins.Mrec_ManagerIns.ActivateView();
            hub.Mrec_Hide = () => AdsLogic.Ins.Mrec_ManagerIns.Hide();
            hub.Mrec_UpdatePos_AdPosition = (adPosition) => AdsLogic.Ins.Mrec_ManagerIns.UpdatePos(adPosition);
            hub.Mrec_UpdatePos_TargetObj = (targetObj, camera) => AdsLogic.Ins.Mrec_ManagerIns.UpdatePos(targetObj, camera);
            hub.Mrec_AbleToShow = () => AdsLogic.Ins.Mrec_ManagerIns.AbleToShow;
            hub.Mrec_GetSize = () => AdsLogic.Ins.Mrec_ManagerIns.GetSize;

            // ---- PU ----
            hub.PU_InitManually = (groupName) => AdsLogic.Ins.PU_ManagerIns.InitManually(groupName);
            hub.PU_ForceInit = (groupName) => AdsLogic.Ins.PU_ManagerIns.ForceInitManually(groupName);
            hub.PU_Show = (pos) => AdsLogic.Ins.PU_ManagerIns.Show(pos);
            hub.PU_Hide = (pos) => AdsLogic.Ins.PU_ManagerIns.Hide(pos);
            hub.PU_UpdatePos = (pos, layout) => AdsLogic.Ins.PU_ManagerIns.UpdatePos(pos, layout);
            hub.PU_AbleToShow = (pos) => AdsLogic.Ins.PU_ManagerIns.AbleToShow(pos);

            // ---- CL ----
            hub.CL_InitManually = () => AdsLogic.Ins.CL_ManagerIns.InitManually();
            hub.CL_Show = () => AdsLogic.Ins.CL_ManagerIns.Show();
            hub.CL_Hide = () => AdsLogic.Ins.CL_ManagerIns.Hide();
            hub.CL_AbleToShow = () => AdsLogic.Ins.CL_ManagerIns.AbleToShow;

            NetCallerAPI.FlushDeferredEventSubscriptions();

            // Done
            hub.MarkBound("AdsLogic");
        }
    }
}
