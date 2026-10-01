using BG_Library.NET.Debug;
using UnityEngine;

namespace BG_Library.NET
{
	[CreateAssetMenu(fileName = "AdCore SO", menuName = "BG_Library/NET/AdCore/Default Core")]
	public class AdsCoreDefault : AdCoreBase
	{
		public override string AdCoreName => "adcore_default_editor";

		#region ===== INIT =====

		public override void InitPluginAtAwake()
		{
			// Lifecycle debug (new system)
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "InitPluginAtAwake", () => $"core={AdCoreName}"))
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => "DefaultCore (no-op)");
			}
		}

		public override void InitStats(string configSt)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "InitStats", () => $"core={AdCoreName} len={(configSt?.Length ?? 0)}"))
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => "DefaultCore (no-op)");
			}
		}

		public override void InitializeMediation()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "InitializeMediation", () => $"core={AdCoreName}"))
			{
				NetEventSystem.OnAdCoreInitCompleted?.Invoke();
				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Emit", () => "OnAdCoreInitCompleted");
			}
		}

		public override string DebugRecheckLogic()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "DebugRecheckLogic", () => $"core={AdCoreName}"))
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => "DefaultCore: No real mediation.");
			}
			return "DefaultCore: No real mediation.";
		}

		#endregion

		#region ===== GROUP RESOLVE (No spam) =====
		// IMPORTANT:
		// - Runtime flow logs are already handled in AdsCore base wrappers (format_* modules).
		// - Therefore DefaultCore should NOT spam logs here; it only returns null/empty as a no-op core.

		#region FA
		protected override IFSGroup FA_GetGroup(string groupName) => null;
		public override string FA_GroupByPos(string pos) => string.Empty;
		public override string[] FA_GetListGroup() => null;
		public override string[] FA_GetAutoInitGroupNames(BG_Library.NET.AdSystem.AdSystemConfigs.ForceAdChannelConfig channelConfig) => new string[0];
		#endregion

		#region RW
		protected override IFSGroup RW_GetGroup() => null;
		#endregion

		#region AL
		protected override IFSGroup AL_GetGroup() => null;
		#endregion

		#region AR
		protected override IFSGroup AR_GetGroup() => null;
		#endregion

		#region BN
		protected override IRectGroup BN_GetGroup(BannerPlacement placement) => null;
		#endregion

		#region MREC
		protected override IRectGroup Mrec_GetGroup() => null;
		#endregion

		#region POPUP
		protected override IRectGroup PU_GetGroup(string groupName) => null;
		public override string PU_GroupByPos(string pos) => string.Empty;
		public override string[] PU_GetListGroup() => null;
		public override string[] PU_GetAutoInitGroupNames(BG_Library.NET.AdSystem.AdSystemConfigs.PopupChannelConfig channelConfig) => new string[0];
		#endregion

		#region COLLAP
		protected override IRectGroup CL_GetGroup() => null;
		#endregion

		#endregion
	}
}
