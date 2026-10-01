using BG_Library.NET.Mediation.Base;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using UnityEngine;

namespace BG_Library.NET.Mediation.Max
{
    public class Max_RectGroupController<T, API> : Rect_GroupControllerBase<T>
        where T : InfoBase
        where API : IMax_RectAccessAPI, new()
    {
        public Max_RectGroupController(T info, string format, string groupName, string mediation)
            : base(info, format, groupName, mediation)
        {
        }

        private Max_RectLogic<T, API> maxLogic;
        private readonly API maxApi = new();

        protected override void MediationSetup()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Setup {GroupName}",
                () => $"adtype={Adtype} id={Id}"))
            {
                if (maxLogic == null)
                {
                    maxLogic = new Max_RectLogic<T, API>(this, maxApi);
                    maxLogic.Attach_Once();
                }

                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Result {GroupName}",
                    () => $"logicCreated={(maxLogic != null)}");
            }
        }

        #region Override Abstract

        protected override void API_Show()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}",
                () => $"MaxSdk.ShowAd id={Id}");
            maxApi.ShowAd(Id);
        }

        protected override void API_Hide()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}",
                () => $"MaxSdk.HideAd id={Id}");
            maxApi.HideAd(Id);
        }

        protected override void API_RequestAd()
        {
            if (Info is Max_BNInfo bannerInfo && maxApi is Max_BNAccessAPI bannerApi)
            {
                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}",
                    () => $"MaxSdk.RequestBanner id={Id} placement={bannerInfo.Placement}");
                bannerApi.RequestAd(Id, bannerInfo.Placement);
                return;
            }

            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}",
                () => $"MaxSdk.RequestAd id={Id}");
            maxApi.RequestAd(Id);
        }

        protected override void API_DestroyAd()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}",
                () => $"MaxSdk.DestroyAd id={Id}");
            maxApi.DestroyAd(Id);
        }

        public override Vector2 Mrec_GetSize() => maxApi.GetSize(Id);

        public override void Mrec_UpdatePos(int pos)
        {
            if (maxLogic == null)
            {
                NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}",
                    () => "UpdatePos(preset) logic_null=true");
                return;
            }

            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}",
                () => $"UpdatePos(preset) pos={(MaxSdkBase.AdViewPosition)pos}");
            maxLogic.UpdatePos(pos);
        }

        public override void Mrec_UpdatePos(GameObject targetObj, Camera camera = null)
        {
            if (maxLogic == null)
            {
                NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {GroupName}",
                    () => "UpdatePos(anchor) logic_null=true");
                return;
            }

            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {GroupName}",
                () => $"UpdatePos(anchor) target={(targetObj != null)} cam={(camera != null)}");
            maxLogic.UpdatePos(targetObj, camera);
        }

        protected override string ResolveRequestTrackingTarget()
        {
            if (GetTrackingAdType != GroupAdType.Banner || Info is not Max_BNInfo bannerInfo)
                return string.Empty;

            return bannerInfo.Placement switch
            {
                MaxRectPlacement.FullBottom => NetTrackingSystem.BannerPlacementToken(BannerPlacement.FullBottom),
                MaxRectPlacement.FullTop => NetTrackingSystem.BannerPlacementToken(BannerPlacement.FullTop),
                MaxRectPlacement.TopLeft => NetTrackingSystem.BannerPlacementToken(BannerPlacement.TopLeft),
                MaxRectPlacement.TopRight => NetTrackingSystem.BannerPlacementToken(BannerPlacement.TopRight),
                MaxRectPlacement.BottomLeft => NetTrackingSystem.BannerPlacementToken(BannerPlacement.BottomLeft),
                MaxRectPlacement.BottomRight => NetTrackingSystem.BannerPlacementToken(BannerPlacement.BottomRight),
                _ => string.Empty
            };
        }

        #endregion
    }
}
