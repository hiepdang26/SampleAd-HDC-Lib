using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;

namespace BG_Library.NET.Mediation.IOS
{
	public class IOS_FSGroupController<T> : FS_GroupControllerBase<T>
		where T : IOS_FSInfo
	{
		private IOS_FSLogic<T> iosLogic;

		public IOS_FSGroupController(T info, string adType, string groupName, string mediation, int budget)
			: base(info, adType, groupName, mediation)
		{
			Budget = budget;
			DisablePostInitReload = info.DisablePostInitReload;
			LayoutGroup = info.LayoutGroup;
		}

		public LayoutGroupConfig LayoutGroup { get; }

		protected override void MediationSetup()
		{
			int layoutCount = LayoutGroup?.Layouts?.Length ?? 0;

			using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Setup {GroupName}",
				       () => $"adtype={Adtype} id={Id} layoutCount={layoutCount}"))
			{
				if (iosLogic == null)
				{
					iosLogic = new IOS_FSLogic<T>(this);
					iosLogic.AttachForAdInstance();
				}

				NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Setup.OK {GroupName}",
					() => $"logicCreated={(iosLogic != null)} hasInstance={(iosLogic != null && iosLogic.HasAdInstance)}");
			}
		}

		protected override void API_N_RequestAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Request {GroupName}",
				       () => $"adtype={Adtype} id={Id} mode=normal"))
			{
				NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.Logic {GroupName}", () => "IOS_FSLogic.RequestAd()");
				iosLogic.RequestAd();
			}
		}

		protected override bool API_N_GetAdReady() => iosLogic != null && iosLogic.GetAdReady();

		protected override void API_N_Show()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Show {GroupName}",
				       () => $"adtype={Adtype} id={Id} pos={metric.lastPos} mode=normal"))
			{
				NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.Logic {GroupName}", () => "IOS_FSLogic.Show()");
				iosLogic.Show();
			}
		}

		protected override void API_N_DestroyAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"API.Destroy {GroupName}",
				       () => $"adtype={Adtype} id={Id} mode=normal"))
			{
				NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Call.Logic {GroupName}", () => "IOS_FSLogic.DestroyAd()");
				iosLogic?.DestroyAd();
			}
		}

		public override string GetDebugInfo()
		{
			var s = base.GetDebugInfo();
			if (string.IsNullOrEmpty(s)) s = "";

			s += "\n=== IOS FS Extra ===";
			s += $"\nlogicCreated: {(iosLogic != null)}";
			s += $"\nLayouts: {(LayoutGroup?.Layouts?.Length ?? 0)}";

			if (iosLogic != null)
			{
				bool ready = false;
				try { ready = iosLogic.GetAdReady(); } catch { }

				s += $"\nLogic.GetAdReady(): {ready}";
				s += $"\nHasAdInstance: {iosLogic.HasAdInstance}";
			}

			return s;
		}
	}
}
