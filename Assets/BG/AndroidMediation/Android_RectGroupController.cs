using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.Mediation.Android
{
    public class Android_RectGroupController<T> : Rect_GroupControllerBase<T>
        where T : Android_RectBaseInfo
    {
        public Android_RectGroupController(T info, string adType, string groupName, string mediation)
            : base(info, adType, groupName, mediation)
        {
            DisablePostInitReload = info.DisablePostInitReload;
        }

        private Android_RectLogic<T> androidLogic;

        protected override void MediationSetup()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.MediationSetup {GroupName}",
                () => $"adtype={Adtype} id={Id} logicCreated={(androidLogic != null)}"))
            {
                if (androidLogic == null)
                {
                    androidLogic = new Android_RectLogic<T>(this);
                }

                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Group.MediationSetup {GroupName}",
                    () => $"logicCreated={(androidLogic != null)} attach={androidLogic?.GetAttachStateShort()}");
            }
        }

        #region Override Abstract

        protected override void API_RequestAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_RequestAd {GroupName}",
                () => $"adtype={Adtype} id={Id}"))
            {
                androidLogic.CreateAndLoad();
            }
        }

        protected override void API_Show()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_Show {GroupName}",
                () => $"adtype={Adtype} id={Id}"))
            {
                androidLogic.ShowAd();
            }
        }

        protected override void API_Hide()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_Hide {GroupName}",
                () => $"adtype={Adtype} id={Id}"))
            {
                androidLogic.HideAd();
            }
        }

        protected override bool API_Expand(bool enableClick)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_Expand {GroupName}",
                () => $"adtype={Adtype} id={Id} enableClick={enableClick}"))
            {
                return androidLogic.ExpandAd(enableClick);
            }
        }

        protected override void API_DestroyAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.API_DestroyAd {GroupName}",
                () => $"adtype={Adtype} id={Id}"))
            {
                androidLogic.DestroyAd();
            }
        }

        protected override bool TryValidateShowBeforeApi(string pos, bool isActivateFlow, out TrackingReason reason, out string detail)
        {
            if (androidLogic != null && !androidLogic.TryValidatePopupLayoutBeforeShow(out reason, out detail))
                return false;

            return base.TryValidateShowBeforeApi(pos, isActivateFlow, out reason, out detail);
        }

        #endregion

        public override void Pu_UpdatePos(float xDp, float yDp, float w, float h)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Group.PU_UpdatePos {GroupName}",
                () => $"adtype={Adtype} x={xDp:0.##} y={yDp:0.##} w={w:0.##} h={h:0.##}"))
            {
                androidLogic.UpdatePUPosition(xDp, yDp, w, h);
            }
        }

        public override string GetDebugInfo()
        {
            var s = base.GetDebugInfo();
            if (string.IsNullOrEmpty(s)) s = "";

            s += "\n=== Android Rect Extra ===";
            s += $"\nlogicCreated: {(androidLogic != null)}";

            if (androidLogic != null)
            {
                s += $"\nattach: {androidLogic.GetAttachStateShort()}";
                s += $"\nidKey: {androidLogic.GetIdKeySafe()}";
                s += $"\nhasAdInstance: {androidLogic.HasAdInstance}";
                s += $"\nadInstance: {androidLogic.GetAdInstanceStateShort()}";
            }

            return s;
        }
    }
}
