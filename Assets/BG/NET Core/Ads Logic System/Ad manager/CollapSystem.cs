using UnityEngine;
using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.AdSystem
{
	public class CollapSystem : MonoBehaviour
	{
		private AdCoreBase core;
		private AdSystemConfigs.CollapChannelConfig configs;

		public void Setup(AdCoreBase _core, AdSystemConfigs.CollapChannelConfig _configs)
		{
			core = _core;
			configs = _configs;

			NetFlowDebugSystem.Log(Layer.sys, Module.format_cl, "Setup",
				() => $"core={(core != null)} enable={(configs != null && configs.IsEnabled)}");
		}

		#region (1) ===== LOGIC =====

		private System.Action onMediationCompletedHandler;

		private void Awake()
		{
			NetFlowDebugSystem.Log(Layer.sys, Module.format_cl, "Awake", () => "bind OnAdCoreInitCompleted");

			onMediationCompletedHandler = () =>
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_cl, "Init Auto", () => "OnAdCoreInitCompleted"))
				{
					if (!ShouldAutoInit)
					{
						NetFlowDebugSystem.Warn(Layer.sys, Module.format_cl, "Init Auto Skip",
							() => "AutoInit=false");
						return;
					}

					NetTrackingSystem.RequestSystemEntry(Channel.Collap);
					if (IsDisable)
					{
						NetFlowDebugSystem.Warn(Layer.sys, Module.format_cl, "Init Auto Blocked",
							() => "IsDisable=true");
						NetTrackingSystem.RequestSystemFail(Channel.Collap, DisableTrackingReason);

						return;
					}

					NetFlowDebugSystem.Log(Layer.sys, Module.format_cl, "Init Auto CallCore",
						() => "CL_Initialize");

					core.CL_Initialize();
				}
			};

			NetEventSystem.OnAdCoreInitCompleted += onMediationCompletedHandler;
		}

		private void OnDestroy()
		{
			if (onMediationCompletedHandler != null)
				NetEventSystem.OnAdCoreInitCompleted -= onMediationCompletedHandler;
		}

		#endregion

		#region (2) ===== PUBLIC API =====

		public void InitManually()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_cl, "InitManually", () => "call"))
			{
				if (ShouldAutoInit)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_cl, "InitManually Rejected",
						() => "AutoInit=true");
					return;
				}

				NetTrackingSystem.RequestSystemEntry(Channel.Collap);
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_cl, "InitManually Blocked",
						() => "IsDisable=true");
					NetTrackingSystem.RequestSystemFail(Channel.Collap, DisableTrackingReason);

					return;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_cl, "InitManually CallCore",
					() => "CL_Initialize");

				core.CL_Initialize();
			}
		}

		public void Show()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_cl, "Show", () => "call"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(NetTrackingSystem.CollapDefaultPos);
				NetTrackingSystem.ShowSystemEntry(Channel.Collap, posHint);
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_cl, "Show Blocked",
						() => "IsDisable=true");
					NetTrackingSystem.ShowSystemFail(Channel.Collap, DisableTrackingReason, posHint);
					return;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_cl, "Show CallCore",
					() => "CL_ShowAd");

				core.CL_ShowAd();
			}
		}

		public void Hide()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_cl, "Hide", () => "call"))
			{
				// Hide không chặn bởi IsDisable
				string posHint = NetTrackingSystem.PosTargetHint(NetTrackingSystem.CollapDefaultPos);
				NetTrackingSystem.HideSystemEntry(Channel.Collap, posHint);
				NetFlowDebugSystem.Log(Layer.sys, Module.format_cl, "Hide CallCore",
					() => "CL_HideAd");

				core.CL_HideAd();
			}
		}

		public bool AbleToShow
		{
			get
			{
				if (IsDisable) return false;
				return core.CL_IsLoaded;
			}
		}

		#endregion

		#region (3) ===== HELPERS =====

		private bool IsDisable
		{
			get
			{
				if (AdsLogic.IsRemovedAd) return true;
				if (configs == null || !configs.IsEnabled) return true;

				return false;
			}
		}
		private bool ShouldAutoInit => configs != null && configs.AutoInit;
		private TrackingReason DisableTrackingReason => AdsLogic.IsRemovedAd ? TrackingReason.IapRemoved : TrackingReason.ConfigDisabled;

		public string GetDebugInfo()
		{
			var sb = new System.Text.StringBuilder(900);

			sb.AppendLine("=== CollapLogic (CL) Overview ===");

			sb.AppendLine("-- Configs --");
			if (configs == null)
			{
				sb.AppendLine("(null)");
			}
			else
			{
				sb.Append("isEnabled: ").AppendLine(configs.IsEnabled.ToString());
			}

			sb.AppendLine();
			sb.AppendLine("-- Runtime --");
			sb.Append("IsDisable: ").AppendLine(IsDisable.ToString());
			sb.Append("IsRemovedAd: ").AppendLine(AdsLogic.IsRemovedAd.ToString());
			sb.Append("coreNull: ").AppendLine((core == null).ToString());
			sb.Append("AutoInit: ").AppendLine(configs != null
				? configs.AutoInit.ToString()
				: "(configs null)");

			sb.AppendLine();
			sb.AppendLine("-- Core (AdsCore) --");
			if (core == null)
			{
				sb.AppendLine("core: (null)");
			}
			else
			{
				bool loaded = false;
				try { loaded = core.CL_IsLoaded; } catch { }

				sb.Append("CL_IsLoaded: ").AppendLine(loaded.ToString());
				sb.Append("AbleToShow: ").AppendLine(AbleToShow.ToString());
			}

			sb.AppendLine();
			sb.AppendLine("-- Public API --");
			sb.AppendLine("InitManually()");
			sb.AppendLine("Show()");
			sb.AppendLine("Hide()");
			sb.AppendLine("AbleToShow (bool)");

			return sb.ToString();
		}

		public string GetDebugGroup()
		{
			return core.CL_GetDebugInfo();
		}

		#endregion
	}
}
