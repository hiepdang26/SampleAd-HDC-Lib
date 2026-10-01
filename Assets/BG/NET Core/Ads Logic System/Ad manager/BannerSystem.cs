using UnityEngine;
using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.AdSystem
{
	public class BannerSystem : MonoBehaviour
	{
		private static readonly BannerPlacement[] ManagedPlacements =
		{
			BannerPlacement.FullBottom,
			BannerPlacement.FullTop,
			BannerPlacement.TopLeft,
			BannerPlacement.TopRight,
			BannerPlacement.BottomLeft,
			BannerPlacement.BottomRight
		};

		private AdCoreBase core;
		private AdSystemConfigs.BannerChannelConfig configs;

		private readonly System.Collections.Generic.HashSet<BannerPlacement> initializedPlacements = new();
		private readonly System.Collections.Generic.HashSet<BannerPlacement> autoShownPlacements = new();

		private static string PlacementTrackingHint(BannerPlacement placement)
			=> NetTrackingSystem.PosTargetHint(NetTrackingSystem.BannerPlacementToken(placement));

		public void Setup(AdCoreBase _core, AdSystemConfigs.BannerChannelConfig _configs)
		{
			core = _core;
			configs = _configs;

			NetFlowDebugSystem.Log(Layer.sys, Module.format_bn, "Setup",
				() => $"core={(core != null)} enable={(configs != null && configs.IsEnabled)}");
		}

		#region (1) ===== LOGIC =====

		private System.Action onMediationCompletedHandler;
		private System.Action<AdInfo> onBannerLoadedHandler;

		private void Awake()
		{
			NetFlowDebugSystem.Log(Layer.sys, Module.format_bn, "Awake", () => "bind events");

			onMediationCompletedHandler = () =>
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_bn, "Init Auto", () => "OnAdCoreInitCompleted"))
				{
					bool hasAnyPlacement = false;
					for (int i = 0; i < ManagedPlacements.Length; i++)
					{
						var placement = ManagedPlacements[i];
						if (!ShouldAutoInitPlacement(placement))
							continue;

						hasAnyPlacement = true;
						TryInitializePlacement(placement, "Init Auto");
					}

					if (!hasAnyPlacement)
					{
						NetFlowDebugSystem.Warn(Layer.sys, Module.format_bn, "Init Auto Skip", () => "no enabled banner placement");
					}
				}
			};

			onBannerLoadedHandler = info =>
			{
				if (info.adtype != BG_ConstValue.adtype_bn)
					return;

				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_bn, "OnRectLoaded", () => $"autoShownCount={autoShownPlacements.Count}"))
				{
					bool triggeredAnyShow = false;
					for (int i = 0; i < ManagedPlacements.Length; i++)
					{
						var placement = ManagedPlacements[i];
						if (!ShouldAutoShowPlacement(placement))
							continue;

						if (autoShownPlacements.Contains(placement))
							continue;

						if (!core.BN_IsLoadedAt(placement))
							continue;

						NetFlowDebugSystem.Log(Layer.sys, Module.format_bn, "AutoShow Call", () => $"Show({placement})");
						Show(placement);
						autoShownPlacements.Add(placement);
						triggeredAnyShow = true;
					}

					if (!triggeredAnyShow)
					{
						NetFlowDebugSystem.Log(Layer.sys, Module.format_bn, "AutoShow Skip", () => "no placement ready to auto-show");
					}
				}
			};

			NetEventSystem.OnAdCoreInitCompleted += onMediationCompletedHandler;
			NetEventSystem.OnRectLoaded += onBannerLoadedHandler;
		}

		private void OnDestroy()
		{
			if (onMediationCompletedHandler != null)
				NetEventSystem.OnAdCoreInitCompleted -= onMediationCompletedHandler;

			if (onBannerLoadedHandler != null)
				NetEventSystem.OnRectLoaded -= onBannerLoadedHandler;
		}

		#endregion

		#region (2) ===== PUBLIC API =====

		public void InitManually(BannerPlacement placement)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_bn, "InitManually", () => $"placement={placement}"))
			{
				if (ShouldAutoInitPlacement(placement))
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_bn, "InitManually Rejected",
						() => "AutoInit=true");
					return;
				}

				TryInitializePlacement(placement, "InitManually");
			}
		}

		public void Show(BannerPlacement placement)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_bn, "Show(alias)", () => $"placement={placement} -> ActivateView"))
			{
				ActivateView(placement);
			}
		}

        public bool ActivateView(BannerPlacement placement)
        {
            using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_bn, "ActivateView", () => $"placement={placement}"))
            {
                string posHint = PlacementTrackingHint(placement);
                if (core != null && core.BN_TryGetTrackingIdentity(placement, out var entryAdType, out var entryAdUnitId))
                    NetTrackingSystem.ActivateSystemEntry(Channel.Banner, posHint, entryAdType, entryAdUnitId);
                else
                    NetTrackingSystem.ActivateSystemEntry(Channel.Banner, posHint);

                if (IsDisable)
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_bn, "ActivateView Blocked", () => "IsDisable=true");
                    NetTrackingSystem.ActivateSystemFail(Channel.Banner, DisableTrackingReason, posHint);
                    return false;
                }

                NetFlowDebugSystem.Log(Layer.sys, Module.format_bn, "ActivateView CallCore", () => $"BN_ActivateView({placement})");
                return core.BN_ActivateView(placement);
            }
        }

        public bool Expand(BannerPlacement placement, bool enableClick = true)
        {
            using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_bn, "Expand", () => $"placement={placement} enableClick={enableClick}"))
            {
                if (IsDisable)
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_bn, "Expand Blocked", () => "IsDisable=true");
                    return false;
                }

                return core.BN_Expand(placement, enableClick);
            }
        }

		public void Hide(BannerPlacement placement)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_bn, "Hide", () => $"placement={placement}"))
			{
				string posHint = PlacementTrackingHint(placement);
				NetTrackingSystem.HideSystemEntry(Channel.Banner, posHint);
				// Hide không chặn bởi IsDisable

				NetFlowDebugSystem.Log(Layer.sys, Module.format_bn, "Hide CallCore", () => $"BN_HideAd({placement})");

				core.BN_HideAd(placement);
			}
		}

		public bool AbleToShowAt(BannerPlacement placement)
		{
			if (IsDisable) return false;
			return core.BN_IsLoadedAt(placement);
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
		private TrackingReason DisableTrackingReason => AdsLogic.IsRemovedAd ? TrackingReason.IapRemoved : TrackingReason.ConfigDisabled;

		public string GetDebugInfo()
		{
			var sb = new System.Text.StringBuilder(900);

			sb.AppendLine("=== BannerLogic (BN) Overview ===");

			sb.AppendLine("-- Configs --");
			if (configs == null)
			{
				sb.AppendLine("(null)");
			}
			else
			{
				sb.Append("isEnabled: ").AppendLine(configs.IsEnabled.ToString());
				sb.AppendLine("placementTokens: fb, ft, tl, tr, bl, br");
			}

			sb.AppendLine();
			sb.AppendLine("-- Runtime --");
			sb.Append("initializedPlacements: ").AppendLine(initializedPlacements.Count.ToString());
			sb.Append("autoShownPlacements: ").AppendLine(autoShownPlacements.Count.ToString());

			sb.AppendLine();
			sb.AppendLine("-- Gates --");
			sb.Append("IsDisable: ").AppendLine(IsDisable.ToString());
			sb.Append("IsRemovedAd: ").AppendLine(AdsLogic.IsRemovedAd.ToString());
			sb.Append("coreNull: ").AppendLine((core == null).ToString());

			sb.AppendLine();
			sb.AppendLine("-- Placement State --");
			for (int i = 0; i < ManagedPlacements.Length; i++)
			{
				var placement = ManagedPlacements[i];
				var enabled = IsPlacementEnabled(placement);
				var initialized = initializedPlacements.Contains(placement);
				var autoShown = autoShownPlacements.Contains(placement);
				var autoShow = ShouldAutoShowPlacement(placement);
				bool loaded = false;
				try { loaded = core != null && core.BN_IsLoadedAt(placement); } catch { }

				sb.Append(placement).Append(": ");
				sb.Append("enabled=").Append(enabled);
				sb.Append(" | autoShow=").Append(autoShow);
				sb.Append(" | initialized=").Append(initialized);
				sb.Append(" | loaded=").Append(loaded);
				sb.Append(" | autoShown=").Append(autoShown);
				sb.AppendLine();
			}

			sb.AppendLine();
			sb.AppendLine("-- Notes --");
			sb.AppendLine("• Group debug: use GetDebugGroup(BannerPlacement)");

			return sb.ToString();
		}

		public string GetDebugGroup(BannerPlacement placement)
		{
			return core.BN_GetDebugInfo(placement);
		}

		#endregion

		private void TryInitializePlacement(BannerPlacement placement, string source)
		{
			string targetHint = PlacementTrackingHint(placement);
			NetTrackingSystem.RequestSystemEntry(Channel.Banner, targetHint);

			if (IsDisable)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.format_bn, $"{source} Blocked", () => $"placement={placement} IsDisable=true");
				NetTrackingSystem.RequestSystemFail(Channel.Banner, DisableTrackingReason, targetHint);
				return;
			}

			if (!IsPlacementEnabled(placement))
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.format_bn, $"{source} Skip", () => $"placement={placement} disabled");
				return;
			}

			if (initializedPlacements.Contains(placement))
			{
				NetFlowDebugSystem.Log(Layer.sys, Module.format_bn, $"{source} Skip", () => $"placement={placement} already initialized");
				return;
			}

			NetFlowDebugSystem.Log(Layer.sys, Module.format_bn, $"{source} CallCore", () => $"BN_Initialize({placement})");
			core.BN_Initialize(placement);
			initializedPlacements.Add(placement);
		}

		private bool IsPlacementEnabled(BannerPlacement placement)
		{
			if (configs == null || !configs.IsEnabled)
				return false;

			var slot = GetSlotConfig(placement);
			return slot != null && slot.IsEnabled;
		}

		private bool ShouldAutoShowPlacement(BannerPlacement placement)
		{
			if (!IsPlacementEnabled(placement))
				return false;

			var slot = GetSlotConfig(placement);
			return slot != null && slot.AutoShowOnLoad;
		}

		private bool ShouldAutoInitPlacement(BannerPlacement placement)
		{
			if (!IsPlacementEnabled(placement))
				return false;

			var slot = GetSlotConfig(placement);
			return slot != null && slot.AutoInit;
		}

		private AdSystemConfigs.BannerChannelConfig.BannerSlotChannelConfig GetSlotConfig(BannerPlacement placement)
		{
			if (configs == null)
				return null;

			return placement switch
			{
				BannerPlacement.FullBottom => configs.FullBottom,
				BannerPlacement.FullTop => configs.FullTop,
				BannerPlacement.TopLeft => configs.TopLeft,
				BannerPlacement.TopRight => configs.TopRight,
				BannerPlacement.BottomLeft => configs.BottomLeft,
				BannerPlacement.BottomRight => configs.BottomRight,
				_ => null
			};
		}
	}
}
