using System;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;

namespace BG_Library.NET.Mediation.Admob
{
    public class Admob_FSGroupController<T, API> : FS_GroupControllerBase<T>
        where T : Admob_FSInfo
        where API : IAdmob_FSAccessAPI, new()
    {
        public Admob_FSGroupController(T info, string format, string groupName, string mediation, int budget)
            : base(info, format, groupName, mediation)
        {
            IsPreloadAd = info.PreloadAd;
            AdBufferSize = info.AdBufferSize;
            DisablePostInitReload = info.DisablePostInitReload;

            PreloadKey = $"{format}_{info.Id}";
            Budget = budget;
        }

        public int AdBufferSize { get; }
        public string PreloadKey { get; }

        private Admob_FSLogic<T, API> admobLogic;
        private readonly API admobApi = new();

        protected override void MediationSetup()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Setup {GroupName}", () => $"adtype={Adtype} id={Id} preload={IsPreloadAd}"))
            {
                admobLogic ??= new Admob_FSLogic<T, API>(this, admobApi);
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, "Setup.OK", () => $"logicCreated={(admobLogic != null)}");
            }
        }

        protected override void API_N_RequestAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Request {GroupName}", () => $"adtype={Adtype} id={Id} mode=normal"))
            {
                admobLogic.N_RequestAd();
            }
        }

        protected override bool API_N_GetAdReady() => admobApi.N_GetAdReady();

        protected override void API_N_Show()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Show {GroupName}", () => $"adtype={Adtype} mode=normal pos={metric.lastPos}"))
            {
                admobApi.N_Show();
            }
        }

        protected override void API_N_DestroyAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Destroy {GroupName}", () => $"adtype={Adtype} mode=normal"))
            {
                admobApi.N_DestroyAd();
            }
        }

        protected override bool API_P_GetAdReady() => admobApi.P_GetAdReady(PreloadKey);

        protected override void API_P_DestroyAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Destroy {GroupName}", () => $"adtype={Adtype} mode=preload preloadKey={PreloadKey}"))
            {
                admobApi.P_DestroyAd(PreloadKey);
            }
        }

        protected override void P_StartLoad()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Preload.Start {GroupName}", () => $"adtype={Adtype} preloadKey={PreloadKey} buffer={AdBufferSize}"))
            {
                admobLogic.P_StartLoad();
            }
        }

        protected override bool P_Show(string pos, Action onBeforeAdShow = null, Action onAdShowComplete = null)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Preload.Show {GroupName}", () => $"adtype={Adtype} pos={pos} preloadKey={PreloadKey}"))
            {
                return admobLogic.P_Show(pos, onBeforeAdShow, onAdShowComplete);
            }
        }

        public override string GetDebugInfo()
        {
            var s = base.GetDebugInfo();
            if (string.IsNullOrEmpty(s)) s = "";

            s += "\n=== Admob FS Extra ===";
            s += $"\nlogicCreated: {(admobLogic != null)}";

            s += $"\nIsPreloadAd: {IsPreloadAd}";
            s += $"\nAdBufferSize: {AdBufferSize}";
            s += $"\nPreloadKey: {PreloadKey}";

            bool nReady = false;
            bool pReady = false;

            try { nReady = admobApi.N_GetAdReady(); } catch { }
            try { pReady = admobApi.P_GetAdReady(PreloadKey); } catch { }

            s += $"\nAPI_N_GetAdReady(): {nReady}";
            s += $"\nAPI_P_GetAdReady(): {pReady}";

            return s;
        }
    }
}
