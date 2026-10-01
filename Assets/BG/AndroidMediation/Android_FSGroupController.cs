using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;

namespace BG_Library.NET.Mediation.Android
{
    public class Android_FSGroupController<T> : FS_GroupControllerBase<T>
        where T : Android_FSInfo
    {
        public Android_FSGroupController(T info, string adType, string groupName, string mediation, int budget)
            : base(info, adType, groupName, mediation)
        {
            Budget = budget;
            DisablePostInitReload = info.DisablePostInitReload;
            AndroidInterstitials = info.AndroidInterstitials;

            LayoutGroup = info.LayoutGroup;
            AdTimes = info.AdTimes;
        }

        public LayoutGroupConfig LayoutGroup { get; }
        public int[] AdTimes { get; }
        public AndroidInterstitials AndroidInterstitials;

        private Android_FSLogic<T> androidLogic;

        protected override void MediationSetup()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Setup {GroupName}",
                       () => $"adtype={Adtype} id={Id} layoutCount={LayoutGroup.Layouts.Length}"))
            {
                if (androidLogic == null)
                {
                    androidLogic = new Android_FSLogic<T>(this);
                    androidLogic.AttachForAdInstance();
                }

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Setup.OK {GroupName}",
                    () => $"logicCreated={(androidLogic != null)} hasInstance={(androidLogic != null && androidLogic.HasAdInstance)}");
            }
        }

        protected override void API_N_RequestAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Request {GroupName}",
                       () => $"adtype={Adtype} id={Id} mode=normal"))
            {
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.Logic {GroupName}", () => "Android_FSLogic.RequestAd()");
                androidLogic.RequestAd();
            }
        }

        protected override bool API_N_GetAdReady() => androidLogic.GetAdReady();

        protected override void API_N_Show()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Show {GroupName}",
                       () => $"adtype={Adtype} id={Id} pos={metric.lastPos} mode=normal"))
            {
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.Logic {GroupName}", () => "Android_FSLogic.Show()");
                androidLogic.Show();
            }
        }

        protected override void API_N_DestroyAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Destroy {GroupName}",
                       () => $"adtype={Adtype} id={Id} mode=normal"))
            {
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.Logic {GroupName}", () => "Android_FSLogic.DestroyAd()");
                androidLogic.DestroyAd();
            }
        }

        public override string GetDebugInfo()
        {
            var s = base.GetDebugInfo();
            if (string.IsNullOrEmpty(s)) s = "";

            s += "\n=== Android FS Extra ===";
            s += $"\nlogicCreated: {(androidLogic != null)}";

            s += $"\nLayouts: {LayoutGroup.Layouts.Length}";

            if (AdTimes != null && AdTimes.Length > 0)
                s += $"\nAdTimes: [{string.Join(",", AdTimes)}]";
            else
                s += "\nAdTimes: (empty)";

            if (androidLogic != null)
            {
                bool ready = false;
                try { ready = androidLogic.GetAdReady(); } catch { }

                s += $"\nLogic.GetAdReady(): {ready}";
                s += $"\nHasAdInstance: {androidLogic.HasAdInstance}";
            }

            return s;
        }

        private static string FormatLayouts(string[] layouts)
        {
            return layouts != null && layouts.Length > 0
                ? $"[{string.Join(",", layouts)}]"
                : "(empty)";
        }
    }
}
