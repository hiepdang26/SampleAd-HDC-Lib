using UnityEngine;
using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.AdSystem
{
	public class MrecSystem : MonoBehaviour
	{
		private AdCoreBase core;
		private AdSystemConfigs.MrecChannelConfig configs;

		public void Setup(AdCoreBase _core, AdSystemConfigs.MrecChannelConfig _configs)
		{
			core = _core;
			configs = _configs;

			NetFlowDebugSystem.Log(Layer.sys, Module.format_mrec, "Setup",
				() => $"core={(core != null)} enable={(configs != null && configs.IsEnabled)}");
		}

		#region (1) ===== LOGIC =====

		private System.Action onMediationCompletedHandler;

		private void Awake()
		{
			NetFlowDebugSystem.Log(Layer.sys, Module.format_mrec, "Awake", () => "bind OnAdCoreInitCompleted");

			onMediationCompletedHandler = () =>
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_mrec, "Init Auto", () => "OnAdCoreInitCompleted"))
				{
					if (!ShouldAutoInit)
					{
						NetFlowDebugSystem.Warn(Layer.sys, Module.format_mrec, "Init Auto Skip",
							() => "AutoInit=false");
						return;
					}

					NetTrackingSystem.RequestSystemEntry(Channel.Mrec);
					if (IsDisable)
					{
						NetFlowDebugSystem.Warn(Layer.sys, Module.format_mrec, "Init Auto Blocked",
							() => "IsDisable=true");
						NetTrackingSystem.RequestSystemFail(Channel.Mrec, DisableTrackingReason);

						return;
					}

					NetFlowDebugSystem.Log(Layer.sys, Module.format_mrec, "Init Auto CallCore", () => "Mrec_Initialize");
					core.Mrec_Initialize();
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
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_mrec, "InitManually", () => "call"))
			{
				if (ShouldAutoInit)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_mrec, "InitManually Rejected",
						() => "AutoInit=true");
					return;
				}

				NetTrackingSystem.RequestSystemEntry(Channel.Mrec);
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_mrec, "InitManually Blocked",
						() => "IsDisable=true");
					NetTrackingSystem.RequestSystemFail(Channel.Mrec, DisableTrackingReason);

					return;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_mrec, "InitManually CallCore",
					() => "Mrec_Initialize");

				core.Mrec_Initialize();
			}
		}

		public void Show()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_mrec, "Show(alias)", () => "ActivateView"))
			{
				ActivateView();
			}
		}

        public bool ActivateView()
        {
            using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_mrec, "ActivateView", () => "call"))
            {
                string posHint = NetTrackingSystem.PosTargetHint(NetTrackingSystem.MrecDefaultPos);
                if (core != null && core.Mrec_TryGetTrackingIdentity(out var entryAdType, out var entryAdUnitId))
                    NetTrackingSystem.ActivateSystemEntry(Channel.Mrec, posHint, entryAdType, entryAdUnitId);
                else
                    NetTrackingSystem.ActivateSystemEntry(Channel.Mrec, posHint);

                if (IsDisable)
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_mrec, "ActivateView Blocked", () => "IsDisable=true");
                    NetTrackingSystem.ActivateSystemFail(Channel.Mrec, DisableTrackingReason, posHint);
                    return false;
                }

                NetFlowDebugSystem.Log(Layer.sys, Module.format_mrec, "ActivateView CallCore", () => "Mrec_ActivateView");
                return core.Mrec_ActivateView();
            }
        }

		public void Hide()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_mrec, "Hide", () => "call"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(NetTrackingSystem.MrecDefaultPos);
				NetTrackingSystem.HideSystemEntry(Channel.Mrec, posHint);
				// Hide không chặn bởi IsDisable
				NetFlowDebugSystem.Log(Layer.sys, Module.format_mrec, "Hide CallCore",
					() => "Mrec_HideAd");

				core.Mrec_HideAd();
			}
		}

		public void UpdatePos(int adPosition)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_mrec, "UpdatePos(int)",
				() => $"adPosition={adPosition}"))
			{
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_mrec, "UpdatePos Blocked",
						() => "IsDisable=true");
					return;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_mrec, "UpdatePos CallCore",
					() => $"Mrec_UpdatePos({adPosition})");

				core.Mrec_UpdatePos(adPosition);
			}
		}

		public void UpdatePos(GameObject targetObj, Camera camera = null)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_mrec, "UpdatePos(target)",
				() => $"target={(targetObj != null)} camera={(camera != null)}"))
			{
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_mrec, "UpdatePos Blocked",
						() => "IsDisable=true");
					return;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_mrec, "UpdatePos CallCore",
					() => "Mrec_UpdatePos(targetObj,camera)");

				core.Mrec_UpdatePos(targetObj, camera);
			}
		}

		public bool AbleToShow
		{
			get
			{
				if (IsDisable) return false;
				return core.Mrec_IsLoaded;
			}
		}

		public Vector2 GetSize => core != null ? core.Mrec_GetSize() : Vector2.zero;

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
			var sb = new System.Text.StringBuilder(1000);

			sb.AppendLine("=== MrecLogic (MREC) Overview ===");

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

			bool loaded = false;
			Vector2 size = Vector2.zero;

			try
			{
				if (core != null)
				{
					loaded = core.Mrec_IsLoaded;
					size = core.Mrec_GetSize();
				}
			}
			catch { }

			sb.Append("Mrec_IsLoaded: ").AppendLine(loaded.ToString());
			sb.Append("AbleToShow: ").AppendLine(AbleToShow.ToString());
			sb.Append("SizeWorld(px): ").AppendLine(size.ToString());

			sb.AppendLine();
			sb.AppendLine("-- Notes --");
			sb.AppendLine("• Group debug: use core.Mrec_GetDebugInfo() if exposed from AdsCore.");
			sb.AppendLine("• Position state is managed inside RectGroupController.");

			return sb.ToString();
		}

		public string GetDebugGroup()
		{
			return core.Mrec_GetDebugInfo();
		}

		#endregion
	}
}
