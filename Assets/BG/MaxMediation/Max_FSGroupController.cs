using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;

namespace BG_Library.NET.Mediation.Max
{
    public class Max_FSGroupController<T, API> : FS_GroupControllerBase<T>
        where T : InfoBase
        where API : IMax_FSAccessAPI, new()
    {
        public Max_FSGroupController(T info, string format, string groupName, string mediation)
            : base(info, format, groupName, mediation)
        {
        }

        private Max_FSLogic<T, API> maxLogic;
        private readonly API maxApi = new();

        protected override void MediationSetup()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Setup {GroupName}",
                       () => $"adtype={Adtype} id={Id} mediation={Mediation}"))
            {
                if (maxLogic == null)
                {
                    maxLogic = new Max_FSLogic<T, API>(this, maxApi);
                    maxLogic.Attach_Once();
                }

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Setup.OK {GroupName}",
                    () => $"logicCreated={(maxLogic != null)} attached={(maxLogic != null && maxLogic.IsAttached)}");
            }
        }

        protected override void API_N_RequestAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Request {GroupName}",
                       () => $"adtype={Adtype} id={Id} mode=normal"))
            {
                // “đến AccessAPI rồi”
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.AccessAPI {GroupName}", () => "RequestAd(id)");
                maxApi.RequestAd(Id);
            }
        }

        protected override bool API_N_GetAdReady() => maxApi.GetAdReady(Id);

        protected override void API_N_Show()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Show {GroupName}",
                       () => $"adtype={Adtype} id={Id} pos={metric.lastPos} mode=normal"))
            {
                // “đến AccessAPI rồi”
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.AccessAPI {GroupName}", () => "Show(id)");
                maxApi.Show(Id);
            }
        }

        protected override void API_N_DestroyAd()
        {
            // Max không có Destroy API => vẫn giữ message cũ
            NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"API.Destroy {GroupName} failed",
                () => "Max do not support DestroyAd API, Ad loaded maybe still exist");
        }

        public override string GetDebugInfo()
        {
            var s = base.GetDebugInfo();
            if (string.IsNullOrEmpty(s)) s = "";

            s += "\n=== Max FS Extra ===";
            s += $"\nlogicCreated: {(maxLogic != null)}";

            bool ready = false;
            try { ready = maxApi.GetAdReady(Id); } catch { }

            s += $"\nAPI_GetAdReady(): {ready}";
            s += $"\nAdUnitId: {Id}";

            if (maxLogic != null)
            {
                s += $"\nIsAttached: {maxLogic.IsAttached}";
            }

            return s;
        }
    }
}
