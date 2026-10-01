// =======================
// NetEventsHub.cs
// =======================
using Firebase.Analytics;
using System;
using UnityEngine;

namespace BG_Library.NET.API
{
    /// <summary>
    /// Central hub chứa các delegate để module implementation bind vào.
    /// Asset recommended path: Assets/Resources/NET/NetEventsHub.asset
    /// </summary>
    [CreateAssetMenu(fileName = "NetEventsHub", menuName = "BG_Library/NET/Net Events Hub")]
    public class NetEventsHub : ScriptableObject
    {
        [Header("Hub State")]
        [SerializeField] private bool isBound;
        [SerializeField] private string boundBy;
        [SerializeField] private int bindCount;
        [SerializeField] private string lastBindTimeUtc;

        public bool IsBound => isBound;
        public string BoundBy => boundBy;
        public int BindCount => bindCount;
        public string LastBindTimeUtc => lastBindTimeUtc;

        // =========================
        // API delegates (match NetCallerAPI)
        // =========================

        #region RemoteConfigs
        public Func<string, string> RemoteConfig_GetCustom;
        public Func<bool> RemoteConfig_IsFetchComplete;
        public Action<Action> RemoteConfig_SubAllJsonsComplete;
        public Action<Action> RemoteConfig_UnsubAllJsonsComplete;
        #endregion

        #region Tracking
        public Action<string> LogEvent;
        public Action<string, Parameter[]> LogEvent_Params;
        public Action<string, string> SetUserProperty;
        #endregion

        #region Common
        public Func<bool> IsRemovedAd;
        public Func<bool> AdmobMediation_IsInitComplete;
        public Func<bool> MaxMediation_IsInitComplete;
        #endregion

        #region Remove Ads (IAP)
        public Action PurchaseRemoveAds;
        public Action RevertPurchaseRemoveAds;
        #endregion

        // =========================
        // Ads APIs
        // =========================
        //BreakAd_SubNotifyBeforeShow
        #region AL (App Launch)
        public Action AL_InitManually;
        public Action<Action> AL_SubOnBeforeAdShow;
        public Action<Action> AL_UnsubOnBeforeAdShow;
        public Action<Action> AL_SubOnAdComplete;
        public Action<Action> AL_UnsubOnAdComplete;
        public Func<bool> AL_AbleToShow;
        #endregion

        #region AR (App Resume)
        public Action AR_InitManually;
        #endregion

        #region FA (Force Ad / Interstitial)
        public Action<string> FA_InitManually;
        public Action<string> FA_ForceInit;
        public Func<string, Action, bool> FA_Show;
        public Func<string, bool> FA_AbleToShow;
        public Action FA_StartBreakAd;
        public Action FA_StopBreakAd;
		#endregion

		#region BreakAd Events
		public Action<Action<string, int>> BreakAd_SubNotifyBeforeShow;
		public Action<Action<string, int>> BreakAd_UnsubNotifyBeforeShow;
		#endregion

		#region RW (Rewarded)
		public Action RW_InitManually;
        public Func<string, Action, bool> RW_Show;
        public Func<bool> RW_AbleToShow;
        public Func<bool> RW_GetIgnoreAd;
        public Action<bool> RW_SetIgnoreAd;
        #endregion

        #region BN (Banner)
        public Action BN_InitManually;
        public Action<NetCallerBannerPlacement> BN_InitManually_Placement;
        public Action BN_Show;
        public Action<NetCallerBannerPlacement> BN_Show_Placement;
        public Func<bool> BN_ActivateView;
        public Func<NetCallerBannerPlacement, bool> BN_ActivateView_Placement;
        public Action BN_Hide;
        public Action<NetCallerBannerPlacement> BN_Hide_Placement;
        public Func<bool> BN_AbleToShow;
        public Func<NetCallerBannerPlacement, bool> BN_AbleToShow_Placement;
        public Func<bool, bool> BN_Expand;
        #endregion

        #region MREC
        public Action Mrec_InitManually;
        public Action Mrec_Show;
        public Func<bool> Mrec_ActivateView;
        public Action Mrec_Hide;
        public Action<int> Mrec_UpdatePos_AdPosition;
        public Action<GameObject, Camera> Mrec_UpdatePos_TargetObj;
        public Func<bool> Mrec_AbleToShow;
        public Func<Vector2> Mrec_GetSize;
        #endregion

        #region PU (Popup)
        public Action<string> PU_InitManually;
        public Action<string> PU_ForceInit;
        public Action<string> PU_Show;
        public Action<string> PU_Hide;
        public Action<string, PULayout> PU_UpdatePos;
        public Func<string, bool> PU_AbleToShow;
        #endregion

        #region CL (Collap)
        public Action CL_InitManually;
        public Action CL_Show;
        public Action CL_Hide;
        public Func<bool> CL_AbleToShow;
        #endregion

        // =========================
        // Binder helpers
        // =========================
        public void MarkBound(string binderName)
        {
            isBound = true;
            boundBy = binderName;
            bindCount++;
            lastBindTimeUtc = DateTime.UtcNow.ToString("O");
        }

        public void ClearAllDelegates()
        {
            // ---- Existing ----
            RemoteConfig_GetCustom = null;
            RemoteConfig_IsFetchComplete = null;
            RemoteConfig_SubAllJsonsComplete = null;
            RemoteConfig_UnsubAllJsonsComplete = null;

            LogEvent = null;
            LogEvent_Params = null;
            SetUserProperty = null;

            IsRemovedAd = null;
            AdmobMediation_IsInitComplete = null;
            MaxMediation_IsInitComplete = null;

            PurchaseRemoveAds = null;
            RevertPurchaseRemoveAds = null;

            // ---- Ads ----
            AL_InitManually = null;
            AL_SubOnBeforeAdShow = null;
            AL_UnsubOnBeforeAdShow = null;
            AL_SubOnAdComplete = null;
            AL_UnsubOnAdComplete = null;
            AL_AbleToShow = null;

            AR_InitManually = null;

            FA_InitManually = null;
            FA_ForceInit = null;
            FA_Show = null;
            FA_AbleToShow = null;

			BreakAd_SubNotifyBeforeShow = null;
			BreakAd_UnsubNotifyBeforeShow = null;

			RW_InitManually = null;
            RW_Show = null;
            RW_AbleToShow = null;
            RW_GetIgnoreAd = null;
            RW_SetIgnoreAd = null;

            BN_InitManually = null;
            BN_InitManually_Placement = null;
            BN_Show = null;
            BN_Show_Placement = null;
            BN_ActivateView = null;
            BN_ActivateView_Placement = null;
            BN_Hide = null;
            BN_Hide_Placement = null;
            BN_AbleToShow = null;
            BN_AbleToShow_Placement = null;
            BN_Expand = null;

            Mrec_InitManually = null;
            Mrec_Show = null;
            Mrec_ActivateView = null;
            Mrec_Hide = null;
            Mrec_UpdatePos_AdPosition = null;
            Mrec_UpdatePos_TargetObj = null;
            Mrec_AbleToShow = null;
            Mrec_GetSize = null;

            PU_InitManually = null;
            PU_ForceInit = null;
            PU_Show = null;
            PU_Hide = null;
            PU_UpdatePos = null;
            PU_AbleToShow = null;

            CL_InitManually = null;
            CL_Show = null;
            CL_Hide = null;
            CL_AbleToShow = null;

            // ---- State ----
            isBound = false;
            boundBy = "";
        }
    }
}
