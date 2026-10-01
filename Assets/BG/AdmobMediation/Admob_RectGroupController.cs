// =======================
// Admob_RectGroupController.cs
// =======================
using BG_Library.NET.Mediation.Base;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using UnityEngine;

namespace BG_Library.NET.Mediation.Admob
{
    public class Admob_RectGroupController<T> : Rect_GroupControllerBase<T>
        where T : Admob_RectInfo
    {
        public Admob_RectGroupController(T info, string format, string groupName, string mediation)
            : base(info, format, groupName, mediation)
        {
        }

        private Admob_RectLogic<T> admobLogic;

        protected override void MediationSetup()
        {
            // JOIN flow (do not create new flow id)
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Setup {GroupName}",
                () => $"adtype={Adtype} id={Id}"))
            {
                admobLogic ??= new Admob_RectLogic<T>(this);
                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Result {GroupName}", () => $"logicCreated={(admobLogic != null)}");
            }
        }

        #region Override Abstract

        protected override void API_Show() => admobLogic.ShowAd();
        protected override void API_Hide() => admobLogic.HideAd();
        protected override void API_RequestAd() => admobLogic.CreateAndLoad();
        protected override void API_DestroyAd() => admobLogic.DestroyAd();

        public override Vector2 Mrec_GetSize() => admobLogic != null ? admobLogic.GetSizeWorld() : Vector2.zero;

        public override void Mrec_UpdatePos(int pos)
        {
            if (admobLogic == null)
            {
                NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}",
                    () => "UpdatePos(preset) logic_null=true");
                return;
            }

            admobLogic.UpdatePos(pos);
        }

        public override void Mrec_UpdatePos(GameObject targetObj, Camera camera = null)
        {
            if (admobLogic == null)
            {
                NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}",
                    () => "UpdatePos(anchor) logic_null=true");
                return;
            }

            admobLogic.UpdatePos(targetObj, camera);
        }

        protected override string ResolveRequestTrackingTarget()
        {
            if (GetTrackingAdType != GroupAdType.Banner)
                return string.Empty;

            return Info.Placement switch
            {
                AdmobRectPlacement.FullBottom => NetTrackingSystem.BannerPlacementToken(BannerPlacement.FullBottom),
                AdmobRectPlacement.FullTop => NetTrackingSystem.BannerPlacementToken(BannerPlacement.FullTop),
                AdmobRectPlacement.TopLeft => NetTrackingSystem.BannerPlacementToken(BannerPlacement.TopLeft),
                AdmobRectPlacement.TopRight => NetTrackingSystem.BannerPlacementToken(BannerPlacement.TopRight),
                AdmobRectPlacement.BottomLeft => NetTrackingSystem.BannerPlacementToken(BannerPlacement.BottomLeft),
                AdmobRectPlacement.BottomRight => NetTrackingSystem.BannerPlacementToken(BannerPlacement.BottomRight),
                _ => string.Empty
            };
        }

        #endregion

        public override string GetDebugInfo()
        {
            var s = base.GetDebugInfo();

            if (string.IsNullOrEmpty(s)) s = "";

            s += "\n=== Admob Rect Extra ===";
            s += $"\nlogicCreated: {(admobLogic != null)}";

            if (admobLogic != null)
            {
                s += $"\nviewCreated: {admobLogic.HasView}";
                s += $"\nplacement: {admobLogic.Placement}";
                s += $"\nsizeWorld(px): {admobLogic.GetSizeWorld()}";
                s += $"\nsizeDp: {admobLogic.GetSizeDp()}";
                s += $"\nresp: {admobLogic.GetResponseInfoShort()}";
            }

            return s;
        }
    }
}
