using BG_Library.Common;
using BG_Library.NET.Debug;
using System;
using UnityEngine;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.AdSystem
{
	public class RewardedSystem : MonoBehaviour
	{
		public bool IgnoreAd;

		public bool IsGetReward;
		private Action rwAction;

		private AdCoreBase core;
		private AdSystemConfigs.RewardedChannelConfig configs;

		public void Setup(AdCoreBase _core, AdSystemConfigs.RewardedChannelConfig _configs)
		{
			core = _core;
			configs = _configs;

			NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Setup",
				() => $"core={(core != null)} enable={(configs != null && configs.IsEnabled)}");
		}

		#region (1) ===== LOGIC =====

		private Action onMediationCompletedHandler;
		private Action<AdInfo> onReceivedRewardHandler;
		private Action<AdInfo> onDisplayedHandler;

		private void Awake()
		{
			NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Awake", () => "bind events");

			onMediationCompletedHandler = () =>
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_rw, "Init Auto", () => "OnAdCoreInitCompleted"))
				{
					if (!ShouldAutoInit)
					{
						NetFlowDebugSystem.Warn(Layer.sys, Module.format_rw, "Init Auto Skip",
							() => "AutoInit=false");
						return;
					}

					NetTrackingSystem.RequestSystemEntry(Channel.Rewarded);
					if (IsDisable)
					{
						NetFlowDebugSystem.Warn(Layer.sys, Module.format_rw, "Init Auto Blocked", () => "IsDisable=true");
						NetTrackingSystem.RequestSystemFail(Channel.Rewarded, TrackingReason.ConfigDisabled);

						return;
					}

					NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Init Auto CallCore",
						() => "RW_Initialize");

					core.RW_Initialize();
				}
			};

			onReceivedRewardHandler = _ =>
			{
				// This only sets flag, actual reward callback happens in Update().
				IsGetReward = true;

				NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Event Reward",
					() => "OnFsRewarded -> IsGetReward=true");
			};

			onDisplayedHandler = info =>
			{
				if (info.adtype == BG_ConstValue.adtype_rw)
				{
					NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Event Displayed",
						() => "OnFsDisplayed(format=rw) -> IncreaseImpressionCount");
					IncreaseImpressionCount();
				}
			};

			NetEventSystem.OnAdCoreInitCompleted += onMediationCompletedHandler;
			NetEventSystem.OnFsRewarded += onReceivedRewardHandler;
			NetEventSystem.OnFsDisplayed += onDisplayedHandler;
		}

		private void OnDestroy()
		{
			if (onMediationCompletedHandler != null)
				NetEventSystem.OnAdCoreInitCompleted -= onMediationCompletedHandler;

			if (onReceivedRewardHandler != null)
				NetEventSystem.OnFsRewarded -= onReceivedRewardHandler;

			if (onDisplayedHandler != null)
				NetEventSystem.OnFsDisplayed -= onDisplayedHandler;
		}

		private void Update()
		{
			if (!IsGetReward) return;

			// Reset flag first to prevent re-entrance issues.
			IsGetReward = false;

			// Reward callback is deferred to Update().
			if (rwAction != null)
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_rw, "Grant Reward", () => "Update()"))
				{
					NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Grant Reward Invoke",
						() => "rwAction()");
					rwAction.Invoke();
				}
			}
			else
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.format_rw, "Grant Reward Miss",
					() => "IsGetReward=true but rwAction=null");
			}

			rwAction = null;
		}

		#endregion

		#region (2) ===== PUBLIC API =====

		public void InitManually()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_rw, "InitManually", () => "call"))
			{
				if (ShouldAutoInit)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_rw, "InitManually Rejected", () => "AutoInit=true");
					return;
				}

                NetTrackingSystem.RequestSystemEntry(Channel.Rewarded);
                if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_rw, "InitManually Blocked", () => "IsDisable=true");
                    NetTrackingSystem.RequestSystemFail(Channel.Rewarded, TrackingReason.ConfigDisabled);

                    return;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "InitManually CallCore", () => "RW_Initialize");

				core.RW_Initialize();
			}
		}

		public bool Show(string pos, Action onRewardSuccess)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_rw, "Show", () => $"pos={pos}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				NetTrackingSystem.ShowSystemEntry(Channel.Rewarded, posHint);
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_rw, "Show Blocked", () => "IsDisable=true");
					NetTrackingSystem.ShowSystemFail(Channel.Rewarded, TrackingReason.ConfigDisabled, posHint);

					return false;
				}

				// Ignore => treat as success + grant immediately
				if (IgnoreAd)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_rw, "Show Ignored", () => "IgnoreAd=true -> grant immediately");
					NetTrackingSystem.ShowSystemFail(Channel.Rewarded, TrackingReason.Ignored, posHint);
					onRewardSuccess?.Invoke();
					return true;
				}

				// Actual show: defer reward callback to Update() via rwAction assignment.
				NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Show CallCore", () => "RW_ShowAd(pos, onRewardSet)");

				bool ok = core.RW_ShowAd(pos, () =>
				{
					rwAction = onRewardSuccess;
					NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Show PendingReward", () => "rwAction set (wait OnFsRewarded)");
				});

				NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Show Result", () => $"returned={ok}");

				return ok;
			}
		}

		public bool AbleToShow
		{
			get
			{
				if (IsDisable) return false;
				if (!core.RW_GetReady()) return false;
				return true;
			}
		}

		#endregion

		#region (3) ===== HELPERS =====

		private bool IsDisable
		{
			get
			{
				if (configs == null || !configs.IsEnabled) return true;
				return false;
			}
		}
		private bool ShouldAutoInit => configs != null && configs.AutoInit;

		private int GetImpressionCount => PlayerPrefs.GetInt("rw_count", 0);

		private void IncreaseImpressionCount()
		{
			var newCount = PlayerPrefs.GetInt("rw_count", 0) + 1;
			PlayerPrefs.SetInt("rw_count", newCount);

			NetFlowDebugSystem.Log(Layer.sys, Module.format_rw, "Counter rw_count", () => $"new={newCount}");
		}

		public string GetDebugInfo()
		{
			var sb = new System.Text.StringBuilder(900);

			sb.AppendLine("=== RewardedLogic (RW) Overview ===");

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
			sb.Append("IgnoreAd: ").AppendLine(IgnoreAd.ToString());
			sb.Append("IsGetReward(flag): ").AppendLine(IsGetReward.ToString());
			sb.Append("rwActionPending: ").AppendLine((rwAction != null).ToString());

			sb.AppendLine();
			sb.AppendLine("-- Gates --");
			sb.Append("IsDisable: ").AppendLine(IsDisable.ToString());
			sb.Append("CoreNull: ").AppendLine((core == null).ToString());
			bool ready = false;
			try { ready = core != null && core.RW_GetReady(); } catch { }
			sb.Append("RW_GetReady(): ").AppendLine(ready.ToString());
			sb.Append("AbleToShow: ").AppendLine(AbleToShow.ToString());

			sb.AppendLine();
			sb.AppendLine("-- Counters (PlayerPrefs) --");
			sb.Append("rw_count: ").AppendLine(GetImpressionCount.ToString());

			sb.AppendLine();
			sb.AppendLine("-- Notes --");
			sb.AppendLine("• Group debug: use core.RW_GetDebugInfo() if you add it to AdsCore (optional).");

			return sb.ToString();
		}

		public string GetDebugGroup()
		{
			return core.RW_GetDebugInfo();
		}

		#endregion
	}
}
