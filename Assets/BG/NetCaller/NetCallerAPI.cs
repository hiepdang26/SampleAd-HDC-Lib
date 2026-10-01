// =======================
// NetCallerAPI.cs
// =======================
using Firebase.Analytics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.NET.API
{
    public static class NetCallerAPI
    {
        private const string TAG = "[NetCallerAPI]";
        private static NetEventsHub Hub => NetHubLocator.Hub;
        private static readonly HashSet<Action> PendingRemoteConfigAllJsonsCompleteHandlers = new HashSet<Action>();
        private static readonly HashSet<Action> PendingALOnBeforeAdShowHandlers = new HashSet<Action>();
        private static readonly HashSet<Action> PendingALOnAdCompleteHandlers = new HashSet<Action>();
        private static readonly HashSet<Action<string, int>> PendingBreakAdNotifyBeforeShowHandlers = new HashSet<Action<string, int>>();

        private static void WarnMissingHub(string apiName)
            => UnityEngine.Debug.LogWarning($"{TAG} Hub missing. Ignored: {apiName}");

        private static void WarnNotBound(string apiName)
            => UnityEngine.Debug.LogWarning($"{TAG} Delegate null (not bound). Ignored: {apiName}");

        private static void FlushDeferredRemoteConfigSubscriptions(NetEventsHub hub)
        {
            if (hub?.RemoteConfig_SubAllJsonsComplete == null || PendingRemoteConfigAllJsonsCompleteHandlers.Count == 0)
                return;

            foreach (var handler in PendingRemoteConfigAllJsonsCompleteHandlers)
            {
                if (handler == null)
                    continue;

                hub.RemoteConfig_SubAllJsonsComplete.Invoke(handler);
            }

            PendingRemoteConfigAllJsonsCompleteHandlers.Clear();
        }

        private static void FlushDeferredALSubscriptions(NetEventsHub hub)
        {
            if (hub?.AL_SubOnBeforeAdShow != null && PendingALOnBeforeAdShowHandlers.Count > 0)
            {
                foreach (var handler in PendingALOnBeforeAdShowHandlers)
                {
                    if (handler == null)
                        continue;

                    hub.AL_SubOnBeforeAdShow.Invoke(handler);
                }

                PendingALOnBeforeAdShowHandlers.Clear();
            }

            if (hub?.AL_SubOnAdComplete == null || PendingALOnAdCompleteHandlers.Count == 0)
                return;

            foreach (var handler in PendingALOnAdCompleteHandlers)
            {
                if (handler == null)
                    continue;

                hub.AL_SubOnAdComplete.Invoke(handler);
            }

            PendingALOnAdCompleteHandlers.Clear();
        }

        private static void FlushDeferredBreakAdSubscriptions(NetEventsHub hub)
        {
            if (hub?.BreakAd_SubNotifyBeforeShow == null || PendingBreakAdNotifyBeforeShowHandlers.Count == 0)
                return;

            foreach (var handler in PendingBreakAdNotifyBeforeShowHandlers)
            {
                if (handler == null)
                    continue;

                hub.BreakAd_SubNotifyBeforeShow.Invoke(handler);
            }

            PendingBreakAdNotifyBeforeShowHandlers.Clear();
        }

        public static void FlushDeferredEventSubscriptions()
        {
            var hub = Hub;
            if (hub == null)
                return;

            FlushDeferredRemoteConfigSubscriptions(hub);
            FlushDeferredALSubscriptions(hub);
            FlushDeferredBreakAdSubscriptions(hub);
        }

        #region  General
        public static bool IsRemovedAd
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.IsRemovedAd?.Invoke() ?? false;
            }
        }

        public static bool AdmobMediation_IsInitComplete
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.AdmobMediation_IsInitComplete?.Invoke() ?? false;
            }
        }

        public static bool MaxMediation_IsInitComplete
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.MaxMediation_IsInitComplete?.Invoke() ?? false;
            }
        }
        #endregion

        #region RemoteConfigs
        public static string RemoteConfig_GetCustom(string key)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(RemoteConfig_GetCustom)); return ""; }
            if (hub.RemoteConfig_GetCustom == null) { WarnNotBound(nameof(RemoteConfig_GetCustom)); return ""; }
            return hub.RemoteConfig_GetCustom.Invoke(key);
        }

        public static bool RemoteConfig_IsFetchComplete
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.RemoteConfig_IsFetchComplete?.Invoke() ?? false;
            }
        }

        public static void RemoteConfig_SubAllJsonsComplete(Action handler)
        {
            if (handler == null)
                return;

            var hub = Hub;
            if (hub == null)
            {
                PendingRemoteConfigAllJsonsCompleteHandlers.Add(handler);
                WarnMissingHub(nameof(RemoteConfig_SubAllJsonsComplete));
                return;
            }

            if (hub.RemoteConfig_SubAllJsonsComplete == null)
            {
                PendingRemoteConfigAllJsonsCompleteHandlers.Add(handler);
                WarnNotBound(nameof(RemoteConfig_SubAllJsonsComplete));
                return;
            }

            PendingRemoteConfigAllJsonsCompleteHandlers.Remove(handler);
            hub.RemoteConfig_SubAllJsonsComplete.Invoke(handler);
        }

        public static void RemoteConfig_UnsubAllJsonsComplete(Action handler)
        {
            if (handler == null)
                return;

            PendingRemoteConfigAllJsonsCompleteHandlers.Remove(handler);
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(RemoteConfig_UnsubAllJsonsComplete)); return; }
            if (hub.RemoteConfig_UnsubAllJsonsComplete == null) { WarnNotBound(nameof(RemoteConfig_UnsubAllJsonsComplete)); return; }
            hub.RemoteConfig_UnsubAllJsonsComplete.Invoke(handler);
        }
        #endregion

        #region Tracking
        public static void LogEvent(string eventName)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(LogEvent)); return; }
            if (hub.LogEvent == null) { WarnNotBound(nameof(LogEvent)); return; }
            hub.LogEvent.Invoke(eventName);
        }

        public static void LogEvent(string eventName, params Parameter[] parameters)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(LogEvent)); return; }
            if (hub.LogEvent_Params == null) { WarnNotBound(nameof(LogEvent)); return; }
            hub.LogEvent_Params.Invoke(eventName, parameters);
        }

        public static void SetUserProperty(string propertiesName, string value)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(SetUserProperty)); return; }
            if (hub.SetUserProperty == null) { WarnNotBound(nameof(SetUserProperty)); return; }
            hub.SetUserProperty.Invoke(propertiesName, value);
        }
        #endregion

        #region Remove Ads (IAP)
        public static void PurchaseRemoveAds()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(PurchaseRemoveAds)); return; }
            if (hub.PurchaseRemoveAds == null) { WarnNotBound(nameof(PurchaseRemoveAds)); return; }
            hub.PurchaseRemoveAds.Invoke();
        }

        public static void RevertPurchaseRemoveAds()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(RevertPurchaseRemoveAds)); return; }
            if (hub.RevertPurchaseRemoveAds == null) { WarnNotBound(nameof(RevertPurchaseRemoveAds)); return; }
            hub.RevertPurchaseRemoveAds.Invoke();
        }
        #endregion

        // =========================
        // Ads
        // =========================

        #region AL (App Launch)
        public static void AL_InitManually()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(AL_InitManually)); return; }
            if (hub.AL_InitManually == null) { WarnNotBound(nameof(AL_InitManually)); return; }
            hub.AL_InitManually.Invoke();
        }

        public static void AL_SubOnBeforeAdShow(Action handler)
        {
            if (handler == null)
                return;

            var hub = Hub;
            if (hub == null)
            {
                PendingALOnBeforeAdShowHandlers.Add(handler);
                WarnMissingHub(nameof(AL_SubOnBeforeAdShow));
                return;
            }

            if (hub.AL_SubOnBeforeAdShow == null)
            {
                PendingALOnBeforeAdShowHandlers.Add(handler);
                WarnNotBound(nameof(AL_SubOnBeforeAdShow));
                return;
            }

            PendingALOnBeforeAdShowHandlers.Remove(handler);
            hub.AL_SubOnBeforeAdShow.Invoke(handler);
        }

        public static void AL_UnsubOnBeforeAdShow(Action handler)
        {
            if (handler == null)
                return;

            PendingALOnBeforeAdShowHandlers.Remove(handler);
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(AL_UnsubOnBeforeAdShow)); return; }
            if (hub.AL_UnsubOnBeforeAdShow == null) { WarnNotBound(nameof(AL_UnsubOnBeforeAdShow)); return; }
            hub.AL_UnsubOnBeforeAdShow.Invoke(handler);
        }

        public static void AL_SubOnAdComplete(Action handler)
        {
            if (handler == null)
                return;

            var hub = Hub;
            if (hub == null)
            {
                PendingALOnAdCompleteHandlers.Add(handler);
                WarnMissingHub(nameof(AL_SubOnAdComplete));
                return;
            }

            if (hub.AL_SubOnAdComplete == null)
            {
                PendingALOnAdCompleteHandlers.Add(handler);
                WarnNotBound(nameof(AL_SubOnAdComplete));
                return;
            }

            PendingALOnAdCompleteHandlers.Remove(handler);
            hub.AL_SubOnAdComplete.Invoke(handler);
        }

        public static void AL_UnsubOnAdComplete(Action handler)
        {
            if (handler == null)
                return;

            PendingALOnAdCompleteHandlers.Remove(handler);
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(AL_UnsubOnAdComplete)); return; }
            if (hub.AL_UnsubOnAdComplete == null) { WarnNotBound(nameof(AL_UnsubOnAdComplete)); return; }
            hub.AL_UnsubOnAdComplete.Invoke(handler);
        }

        public static bool AL_AbleToShow
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.AL_AbleToShow?.Invoke() ?? false;
            }
        }
        #endregion

        #region AR (App Resume)
        public static void AR_InitManually()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(AR_InitManually)); return; }
            if (hub.AR_InitManually == null) { WarnNotBound(nameof(AR_InitManually)); return; }
            hub.AR_InitManually.Invoke();
        }
        #endregion

        #region FA (Force Ad / Interstitial)
        public static void FA_InitManually(string groupName)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(FA_InitManually)); return; }
            if (hub.FA_InitManually == null) { WarnNotBound(nameof(FA_InitManually)); return; }
            hub.FA_InitManually.Invoke(groupName);
        }

        public static void FA_ForceInit(string groupName)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(FA_ForceInit)); return; }
            if (hub.FA_ForceInit == null) { WarnNotBound(nameof(FA_ForceInit)); return; }
            hub.FA_ForceInit.Invoke(groupName);
        }

        public static bool FA_Show(string pos, Action actionDone = null)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(FA_Show)); return false; }
            if (hub.FA_Show == null) { WarnNotBound(nameof(FA_Show)); return false; }
            return hub.FA_Show.Invoke(pos, actionDone);
        }

        public static bool FA_AbleToShow(string pos)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(FA_AbleToShow)); return false; }
            if (hub.FA_AbleToShow == null) { WarnNotBound(nameof(FA_AbleToShow)); return false; }
            return hub.FA_AbleToShow.Invoke(pos);
        }

        public static void FA_StartBreakAd()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(FA_StartBreakAd)); return; }
            if (hub.FA_StartBreakAd == null) { WarnNotBound(nameof(FA_StartBreakAd)); return; }
            hub.FA_StartBreakAd.Invoke();
        }

        public static void FA_StopBreakAd()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(FA_StopBreakAd)); return; }
            if (hub.FA_StopBreakAd == null) { WarnNotBound(nameof(FA_StopBreakAd)); return; }
            hub.FA_StopBreakAd.Invoke();
        }

		#endregion

		#region BreakAd Events
		public static void BreakAd_SubNotifyBeforeShow(Action<string, int> handler)
		{
			if (handler == null)
				return;

			var hub = Hub;
			if (hub == null)
			{
				PendingBreakAdNotifyBeforeShowHandlers.Add(handler);
				WarnMissingHub(nameof(BreakAd_SubNotifyBeforeShow));
				return;
			}

			if (hub.BreakAd_SubNotifyBeforeShow == null)
			{
				PendingBreakAdNotifyBeforeShowHandlers.Add(handler);
				WarnNotBound(nameof(BreakAd_SubNotifyBeforeShow));
				return;
			}

			PendingBreakAdNotifyBeforeShowHandlers.Remove(handler);
			hub.BreakAd_SubNotifyBeforeShow.Invoke(handler);
		}

		public static void BreakAd_UnsubNotifyBeforeShow(Action<string, int> handler)
		{
			if (handler == null)
				return;

			PendingBreakAdNotifyBeforeShowHandlers.Remove(handler);
			var hub = Hub;
			if (hub == null) { WarnMissingHub(nameof(BreakAd_UnsubNotifyBeforeShow)); return; }
			if (hub.BreakAd_UnsubNotifyBeforeShow == null) { WarnNotBound(nameof(BreakAd_UnsubNotifyBeforeShow)); return; }
			hub.BreakAd_UnsubNotifyBeforeShow.Invoke(handler);
		}
		#endregion

		#region RW (Rewarded)
		public static void RW_InitManually()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(RW_InitManually)); return; }
            if (hub.RW_InitManually == null) { WarnNotBound(nameof(RW_InitManually)); return; }
            hub.RW_InitManually.Invoke();
        }

        public static bool RW_Show(string pos, Action onRewardSuccess)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(RW_Show)); return false; }
            if (hub.RW_Show == null) { WarnNotBound(nameof(RW_Show)); return false; }
            return hub.RW_Show.Invoke(pos, onRewardSuccess);
        }

        public static bool RW_AbleToShow
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.RW_AbleToShow?.Invoke() ?? false;
            }
        }

        public static bool RW_IgnoreAd
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.RW_GetIgnoreAd?.Invoke() ?? false;
            }
            set
            {
                var hub = Hub;
                if (hub == null) { WarnMissingHub(nameof(RW_IgnoreAd)); return; }
                if (hub.RW_SetIgnoreAd == null) { WarnNotBound(nameof(RW_IgnoreAd)); return; }
                hub.RW_SetIgnoreAd.Invoke(value);
            }
        }
        #endregion

        #region BN (Banner)
        public static void BN_InitManually()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_InitManually)); return; }
            if (hub.BN_InitManually == null) { WarnNotBound(nameof(BN_InitManually)); return; }
            hub.BN_InitManually.Invoke();
        }

        public static void BN_InitManually(NetCallerBannerPlacement placement)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_InitManually)); return; }
            if (hub.BN_InitManually_Placement == null) { WarnNotBound(nameof(BN_InitManually)); return; }
            hub.BN_InitManually_Placement.Invoke(placement);
        }

        public static void BN_Show()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_Show)); return; }
            if (hub.BN_ActivateView == null) { WarnNotBound(nameof(BN_Show)); return; }
            hub.BN_ActivateView.Invoke();
        }

        public static void BN_Show(NetCallerBannerPlacement placement)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_Show)); return; }
            if (hub.BN_ActivateView_Placement == null) { WarnNotBound(nameof(BN_Show)); return; }
            hub.BN_ActivateView_Placement.Invoke(placement);
        }

        public static bool BN_ActivateView()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_ActivateView)); return false; }
            if (hub.BN_ActivateView == null) { WarnNotBound(nameof(BN_ActivateView)); return false; }
            return hub.BN_ActivateView.Invoke();
        }

        public static bool BN_ActivateView(NetCallerBannerPlacement placement)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_ActivateView)); return false; }
            if (hub.BN_ActivateView_Placement == null) { WarnNotBound(nameof(BN_ActivateView)); return false; }
            return hub.BN_ActivateView_Placement.Invoke(placement);
        }

        public static void BN_Hide()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_Hide)); return; }
            if (hub.BN_Hide == null) { WarnNotBound(nameof(BN_Hide)); return; }
            hub.BN_Hide.Invoke();
        }

        public static void BN_Hide(NetCallerBannerPlacement placement)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_Hide)); return; }
            if (hub.BN_Hide_Placement == null) { WarnNotBound(nameof(BN_Hide)); return; }
            hub.BN_Hide_Placement.Invoke(placement);
        }

        public static bool BN_AbleToShow
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.BN_AbleToShow?.Invoke() ?? false;
            }
        }

        public static bool BN_AbleToShowAt(NetCallerBannerPlacement placement)
        {
            var hub = Hub;
            if (hub == null) return false;
            return hub.BN_AbleToShow_Placement?.Invoke(placement) ?? false;
        }

        public static bool BN_Expand(bool enableClick = true)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(BN_Expand)); return false; }
            if (hub.BN_Expand == null) { WarnNotBound(nameof(BN_Expand)); return false; }
            return hub.BN_Expand.Invoke(enableClick);
        }
        #endregion

        #region MREC
        public static void Mrec_InitManually()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(Mrec_InitManually)); return; }
            if (hub.Mrec_InitManually == null) { WarnNotBound(nameof(Mrec_InitManually)); return; }
            hub.Mrec_InitManually.Invoke();
        }

        public static void Mrec_Show()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(Mrec_Show)); return; }
            if (hub.Mrec_ActivateView == null) { WarnNotBound(nameof(Mrec_Show)); return; }
            hub.Mrec_ActivateView.Invoke();
        }

        public static bool Mrec_ActivateView()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(Mrec_ActivateView)); return false; }
            if (hub.Mrec_ActivateView == null) { WarnNotBound(nameof(Mrec_ActivateView)); return false; }
            return hub.Mrec_ActivateView.Invoke();
        }

        public static void Mrec_Hide()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(Mrec_Hide)); return; }
            if (hub.Mrec_Hide == null) { WarnNotBound(nameof(Mrec_Hide)); return; }
            hub.Mrec_Hide.Invoke();
        }

        public static void Mrec_UpdatePos(int adPosition)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(Mrec_UpdatePos)); return; }
            if (hub.Mrec_UpdatePos_AdPosition == null) { WarnNotBound(nameof(Mrec_UpdatePos)); return; }
            hub.Mrec_UpdatePos_AdPosition.Invoke(adPosition);
        }

        public static void Mrec_UpdatePos(GameObject targetObj, Camera camera = null)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(Mrec_UpdatePos)); return; }
            if (hub.Mrec_UpdatePos_TargetObj == null) { WarnNotBound(nameof(Mrec_UpdatePos)); return; }
            hub.Mrec_UpdatePos_TargetObj.Invoke(targetObj, camera);
        }

        public static bool Mrec_AbleToShow
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.Mrec_AbleToShow?.Invoke() ?? false;
            }
        }

        public static Vector2 Mrec_GetSize
        {
            get
            {
                var hub = Hub;
                if (hub == null) return Vector2.zero;
                return hub.Mrec_GetSize?.Invoke() ?? Vector2.zero;
            }
        }
        #endregion

        #region PU (Popup)
        public static void PU_InitManually(string groupName)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(PU_InitManually)); return; }
            if (hub.PU_InitManually == null) { WarnNotBound(nameof(PU_InitManually)); return; }
            hub.PU_InitManually.Invoke(groupName);
        }

        public static void PU_ForceInit(string groupName)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(PU_ForceInit)); return; }
            if (hub.PU_ForceInit == null) { WarnNotBound(nameof(PU_ForceInit)); return; }
            hub.PU_ForceInit.Invoke(groupName);
        }

        public static void PU_Show(string pos)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(PU_Show)); return; }
            if (hub.PU_Show == null) { WarnNotBound(nameof(PU_Show)); return; }
            hub.PU_Show.Invoke(pos);
        }

        public static void PU_Hide(string pos)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(PU_Hide)); return; }
            if (hub.PU_Hide == null) { WarnNotBound(nameof(PU_Hide)); return; }
            hub.PU_Hide.Invoke(pos);
        }

        public static void PU_UpdatePos(string pos, PULayout layout)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(PU_UpdatePos)); return; }
            if (hub.PU_UpdatePos == null) { WarnNotBound(nameof(PU_UpdatePos)); return; }
            hub.PU_UpdatePos.Invoke(pos, layout);
        }

        public static bool PU_AbleToShow(string pos)
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(PU_AbleToShow)); return false; }
            if (hub.PU_AbleToShow == null) { WarnNotBound(nameof(PU_AbleToShow)); return false; }
            return hub.PU_AbleToShow.Invoke(pos);
        }
        #endregion

        #region CL (Collap)
        public static void CL_InitManually()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(CL_InitManually)); return; }
            if (hub.CL_InitManually == null) { WarnNotBound(nameof(CL_InitManually)); return; }
            hub.CL_InitManually.Invoke();
        }

        public static void CL_Show()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(CL_Show)); return; }
            if (hub.CL_Show == null) { WarnNotBound(nameof(CL_Show)); return; }
            hub.CL_Show.Invoke();
        }

        public static void CL_Hide()
        {
            var hub = Hub;
            if (hub == null) { WarnMissingHub(nameof(CL_Hide)); return; }
            if (hub.CL_Hide == null) { WarnNotBound(nameof(CL_Hide)); return; }
            hub.CL_Hide.Invoke();
        }

        public static bool CL_AbleToShow
        {
            get
            {
                var hub = Hub;
                if (hub == null) return false;
                return hub.CL_AbleToShow?.Invoke() ?? false;
            }
        }
        #endregion
    }
}
