using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using System;
using UnityEngine;

namespace BG_Library.NET.AdSystem
{
	public class ForceAdSystem : MonoBehaviour
	{
		public bool IsFirstFA;

		public bool IgnoreAd;

		private AdCoreBase core;
		private AdSystemConfigs.ForceAdChannelConfig configs;

		public void Setup(AdCoreBase _core, AdSystemConfigs.ForceAdChannelConfig _configs)
		{
			core = _core;
			configs = _configs;

			NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Setup",
				() => $"core={(core != null)} enable={(configs != null && configs.IsEnabled)} posCount={(configs?.PositionConfigs?.Length ?? 0)} breakEnable={(configs?.BreakAdConfig != null && configs.BreakAdConfig.IsEnabled)}");
		}

		#region (1) ===== LOGIC =====

		private Action<AdInfo> onFSClosedHandler;
		private Action<AdInfo> onFSOnAdShowFail;
		private Action<AdInfo> onFSDisplayedHandler;
		private Action onAdCoreInitCompletedHandler;

		// BreakAd hooks
		private Action<AdInfo> onFSBeforeOpen_BreakReset;

		private void Awake()
		{
			IsFirstFA = true;

			NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Awake", () => "bind FS events + BreakAd awake");

			onFSClosedHandler = info =>
			{
				// BreakAd close tracking (only cares when BreakAd is attempting its own FA)
				Break_OnFSClosed(info);
			};

			onFSOnAdShowFail = info =>
			{
				Break_OnFSShowFail(info);
			};

			onFSDisplayedHandler = info =>
			{
				if (info.adtype == BG_ConstValue.adtype_fa)
				{
					NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Event OnFsDisplayed", () => $"adtype={info.adtype} -> IncreaseImpressionCount pos={info.pos}");
					IncreaseImpressionCount(info.pos);
				}
			};

			// Reset BreakAd timer on ANY FS before open (100% certainty, per your decision)
			onFSBeforeOpen_BreakReset = info =>
			{
				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Event OnFsBeforeOpen", () => $"(Break reset) adtype={info.adtype} pos={info.pos}");
				Break_OnFSBeforeOpen(info);
			};

			NetEventSystem.OnFsClosed += onFSClosedHandler;
			NetEventSystem.OnFsShowFailed += onFSOnAdShowFail;
			NetEventSystem.OnFsDisplayed += onFSDisplayedHandler;
			NetEventSystem.OnFsBeforeOpen += onFSBeforeOpen_BreakReset;

			onAdCoreInitCompletedHandler = () =>
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_fa, "Init Auto", () => "OnAdCoreInitCompleted"))
				{
					TryAutoInitializeGroups();
				}
			};
			NetEventSystem.OnAdCoreInitCompleted += onAdCoreInitCompletedHandler;

			Break_OnAwake();
		}

		private void OnDestroy()
		{
			if (onFSClosedHandler != null)
				NetEventSystem.OnFsClosed -= onFSClosedHandler;

			if (onFSOnAdShowFail != null)
				NetEventSystem.OnFsShowFailed -= onFSOnAdShowFail;

			if (onFSDisplayedHandler != null)
				NetEventSystem.OnFsDisplayed -= onFSDisplayedHandler;

			if (onFSBeforeOpen_BreakReset != null)
				NetEventSystem.OnFsBeforeOpen -= onFSBeforeOpen_BreakReset;

			if (onAdCoreInitCompletedHandler != null)
				NetEventSystem.OnAdCoreInitCompleted -= onAdCoreInitCompletedHandler;

			Break_OnDestroy();
		}

		private void Update()
		{
			Break_OnUpdate();
		}

		// =============================
		// System gates
		// =============================

		/// <summary>
		/// SystemCheck normal gate:
		/// - Uses global capping clock: AdsLogic.LastTimeFSAd (you confirmed it's updated at OnFsClosed).
		///
		/// IMPORTANT (BreakAd exception):
		/// - BreakAd countdown MUST reset at NetEventSystem.OnFsBeforeOpen (your decision).
		/// - Global LastTimeFSAd updates at OnFsClosed.
		/// => There is a small mismatch ~= "ad open duration".
		/// => To guarantee BreakAd can show exactly on its countdown, we bypass ONLY the time-capping gate
		///    when BreakAd is attempting to show its BreakPos.
		/// </summary>
		private bool SystemCheck(string pos, out string reason, bool ignoreTimeCapping)
		{
			if (IsDisable)
			{
				var isEnable = configs != null && configs.IsEnabled;
				reason = $"IsDisable. IsRemovedAd: {AdsLogic.IsRemovedAd}, IsEnable: {isEnable}";
				return false;
			}

			var posInfo = GetPosInfo(pos);
			if (posInfo == null || !posInfo.CanShow)
			{
				reason = "Pos is disable";
				return false;
			}

			// Time capping (can be bypassed for BreakAd only)
			if (!ignoreTimeCapping)
			{
				var (result, deltaTimeAds, minTime) = IsWaitEnoughTime(posInfo);
				if (!result)
				{
					reason = $"Not enough time. deltaTimeAds: {deltaTimeAds}, minTime: {minTime}";
					return false;
				}
			}

			if (IgnoreAd)
			{
				reason = "Ignore FA by Developer";
				return false;
			}

			reason = "";
			return true;
		}

		private AdSystemConfigs.ForceAdChannelConfig.ForceAdPositionConfig GetPosInfo(string pos)
		{
			if (configs == null || configs.PositionConfigs == null) return null;
			return Array.Find(configs.PositionConfigs, c => c != null && c.PositionName == pos);
		}

		private (bool result, float deltaTimeAds, float minTime) IsWaitEnoughTime(AdSystemConfigs.ForceAdChannelConfig.ForceAdPositionConfig posInfo)
		{
			var deltaTimeAds = Time.realtimeSinceStartup - AdsLogic.LastTimeFSAd;
			var minTime = CalMinTime(posInfo);

			if (deltaTimeAds < minTime) return (false, deltaTimeAds, minTime);
			return (true, deltaTimeAds, minTime);
		}

		private float CalMinTime(AdSystemConfigs.ForceAdChannelConfig.ForceAdPositionConfig structAds)
		{
			float decreaseTPI = structAds.CappingDecreasePerImpression != 0
				? structAds.CappingDecreasePerImpression
				: configs.CappingDecreasePerImpression;

			float minInterTime = structAds.MinimumCappingTime != 0
				? structAds.MinimumCappingTime
				: configs.MinimumCappingTime;

			var cappingTime = Mathf.Max(
				structAds.CappingTime - GetImpressionCount(structAds.PositionName) * decreaseTPI,
				minInterTime);

			if (IsFirstFA)
				return Mathf.Max(configs.LaunchCappingTime, cappingTime);
			else
				return cappingTime;
		}

		private const string KEY_FA_TOTAL_IMP = "fa_total_impression";

		private int GetImpressionCount(string pos)
		{
			return PlayerPrefs.GetInt($"fa_count_{pos}", 0);
		}

		private int GetTotalImpressionCount()
		{
			return PlayerPrefs.GetInt(KEY_FA_TOTAL_IMP, 0);
		}

		private void IncreaseImpressionCount(string pos)
		{
			var newCount = GetImpressionCount(pos) + 1;
			PlayerPrefs.SetInt($"fa_count_{pos}", newCount);

			if (IsFirstFA) IsFirstFA = false;

			var total = PlayerPrefs.GetInt(KEY_FA_TOTAL_IMP, 0) + 1;
			PlayerPrefs.SetInt(KEY_FA_TOTAL_IMP, total);

			NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Counter", () => $"pos={pos} newCount={newCount} total={total} IsFirstFA={IsFirstFA}");
		}

		#endregion

		#region (2) ===== PUBLIC API =====

		public void InitManually(string groupName)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_fa, "InitManually", () => $"group={groupName}"))
			{
				if (IsAutoInitGroup(groupName))
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "InitManually Rejected", () => $"group={groupName} AutoInit=true");
					return;
				}

                string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
                NetTrackingSystem.RequestSystemEntry(Channel.ForceAd, groupHint);
                if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "InitManually Blocked", () => "IsDisable=true");
                    NetTrackingSystem.RequestSystemFail(Channel.ForceAd, DisableTrackingReason, groupHint);

                    return;
				}

				GroupAdType? resolvedAdType = null;
				string resolvedIdentitySource = string.Empty;
				if (core != null && core.FA_TryGetTrackingIdentity(groupName, out var adType, out var adUnitId))
				{
					resolvedAdType = adType;
					resolvedIdentitySource = adUnitId;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "InitManually CallCore", () => $"FA_Initialize({groupName})");

				core.FA_Initialize(groupName, resolvedAdType, resolvedIdentitySource);
			}
		}

		public void ForceInitManually(string groupName)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_fa, "ForceInitManually", () => $"group={groupName}"))
			{
				string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
				NetTrackingSystem.RequestSystemEntry(Channel.ForceAd, groupHint);
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "ForceInitManually Blocked", () => "IsDisable=true");
					NetTrackingSystem.RequestSystemFail(Channel.ForceAd, DisableTrackingReason, groupHint);
					return;
				}

				GroupAdType? resolvedAdType = null;
				string resolvedIdentitySource = string.Empty;
				if (core != null && core.FA_TryGetTrackingIdentity(groupName, out var adType, out var adUnitId))
				{
					resolvedAdType = adType;
					resolvedIdentitySource = adUnitId;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "ForceInitManually CallCore", () => $"FA_ForceInitialize({groupName})");
				core.FA_ForceInitialize(groupName, resolvedAdType, resolvedIdentitySource);
			}
		}

		public bool Show(string pos, Action actionDone = null)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_fa, "Show", () => $"pos={pos}"))
			{
				var group = core.FA_GroupByPos(pos);
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				NetTrackingSystem.ShowSystemEntry(Channel.ForceAd, posHint);

				if (!SystemCheck(pos, out var reason, ignoreTimeCapping: false))
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Show Blocked", () => $"SystemCheckFail reason={reason}");
					NetTrackingSystem.ShowSystemFail(Channel.ForceAd, ResolveSystemCheckTrackingReason(pos), posHint);

					actionDone?.Invoke();
					return false;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Resolved Group", () => $"group={group}");
				GroupAdType? resolvedAdType = null;
				string resolvedIdentitySource = string.Empty;
				if (core != null && core.FA_TryGetTrackingIdentity(group, out var adType, out var adUnitId))
				{
					resolvedAdType = adType;
					resolvedIdentitySource = adUnitId;
				}

				var isShow = core.FA_ShowAd(group, pos, null, actionDone, resolvedAdType, resolvedIdentitySource);

				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Show Result", () => $"returned={isShow}");

				if (!isShow)
				{
					actionDone?.Invoke();
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Show ReturnFalse", () => "invoke actionDone immediately");
				}

				return isShow;
			}
		}

		public bool AbleToShow(string pos)
		{
			if (!SystemCheck(pos, out var _, ignoreTimeCapping: false)) return false;

			var groupName = core.FA_GroupByPos(pos);
			if (!core.FA_GetReady(groupName)) return false;

			return true;
		}

		public AdSystemConfigs.ForceAdChannelConfig.ForceAdPositionConfig[] GetListPos => configs?.PositionConfigs;

        public bool IsGroupReady(string groupName)
        {
            if (core == null || string.IsNullOrEmpty(groupName))
                return false;

            return core.FA_GetReady(groupName);
        }

		#endregion

		#region (3) ===== HELPERS =====

		private bool IsDisable
		{
			get
			{
				if (configs == null || !configs.IsEnabled) return true;
				if (AdsLogic.IsRemovedAd) return true;
				return false;
			}
		}
		private TrackingReason DisableTrackingReason => AdsLogic.IsRemovedAd ? TrackingReason.IapRemoved : TrackingReason.ConfigDisabled;

		private void TryAutoInitializeGroups()
		{
			if (IsDisable)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Init Auto Blocked", () => "IsDisable=true");
				return;
			}

			var groupNames = core?.FA_GetAutoInitGroupNames(configs) ?? Array.Empty<string>();
			if (groupNames.Length == 0)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Init Auto Skip", () => "no auto-init groups");
				return;
			}

			for (int i = 0; i < groupNames.Length; i++)
			{
				var groupName = groupNames[i];
				if (string.IsNullOrEmpty(groupName))
					continue;

				string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
				NetTrackingSystem.RequestSystemEntry(Channel.ForceAd, groupHint);
				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Init Auto CallCore", () => $"FA_Initialize({groupName})");
				core.FA_Initialize(groupName);
			}
		}

		private bool IsAutoInitGroup(string groupName)
		{
			if (string.IsNullOrEmpty(groupName))
				return false;

			var groupNames = core?.FA_GetAutoInitGroupNames(configs);
			if (groupNames == null || groupNames.Length == 0)
				return false;

			for (int i = 0; i < groupNames.Length; i++)
			{
				if (string.Equals(groupNames[i], groupName, StringComparison.Ordinal))
					return true;
			}

			return false;
		}
		private TrackingReason ResolveSystemCheckTrackingReason(string pos, bool ignoreTimeCapping = false)
		{
			if (AdsLogic.IsRemovedAd) return TrackingReason.IapRemoved;
			if (configs == null || !configs.IsEnabled) return TrackingReason.ConfigDisabled;

			var posInfo = GetPosInfo(pos);
			if (posInfo == null || !posInfo.CanShow) return TrackingReason.PositionBlocked;

			if (!ignoreTimeCapping)
			{
				var (result, _, _) = IsWaitEnoughTime(posInfo);
				if (!result) return TrackingReason.CappingBlocked;
			}

			if (IgnoreAd) return TrackingReason.Ignored;
			return TrackingReason.GateBlocked;
		}

		private TrackingReason ResolveBreakEnableTrackingReason()
		{
			if (AdsLogic.IsRemovedAd) return TrackingReason.IapRemoved;
			if (configs == null) return TrackingReason.NullData;
			if (!configs.IsEnabled) return TrackingReason.Blocked;

			var b = configs.BreakAdConfig;
			if (b == null) return TrackingReason.NullData;
			if (!b.IsEnabled) return TrackingReason.Blocked;

			var pos = Break_GetPosSafe();
			if (string.IsNullOrEmpty(pos)) return TrackingReason.NullData;

			var posInfo = GetPosInfo(pos);
			if (posInfo == null) return TrackingReason.NullData;
			if (!posInfo.CanShow) return TrackingReason.Blocked;

			return TrackingReason.Blocked;
		}

		private TrackingReason ResolveBreakAutoShowTrackingReason(string pos, bool ignoreTimeCapping = false)
		{
			if (AdsLogic.IsRemovedAd) return TrackingReason.IapRemoved;
			if (configs == null) return TrackingReason.NullData;
			if (!configs.IsEnabled) return TrackingReason.Blocked;

			var posInfo = GetPosInfo(pos);
			if (string.IsNullOrEmpty(pos) || posInfo == null) return TrackingReason.NullData;
			if (!posInfo.CanShow) return TrackingReason.Blocked;

			if (!ignoreTimeCapping)
			{
				var (result, _, _) = IsWaitEnoughTime(posInfo);
				if (!result) return TrackingReason.CappingBlocked;
			}

			if (IgnoreAd) return TrackingReason.Ignored;
			return TrackingReason.Blocked;
		}

		public string GetDebugInfo()
		{
			var sb = new System.Text.StringBuilder(1400);

			sb.AppendLine("=== ForceAdLogic (FA) Overview ===");

			sb.AppendLine("-- Configs --");
			if (configs == null)
			{
				sb.AppendLine("(null)");
			}
			else
			{
				sb.Append("isEnabled: ").AppendLine(configs.IsEnabled.ToString());
				sb.Append("launchCappingTime: ").AppendLine(configs.LaunchCappingTime.ToString("0.###"));
				sb.Append("minimumCappingTime: ").AppendLine(configs.MinimumCappingTime.ToString("0.###"));
				sb.Append("cappingDecreasePerImpression: ").AppendLine(configs.CappingDecreasePerImpression.ToString("0.###"));

				var posArr = configs.PositionConfigs;
				sb.Append("PosCount: ").AppendLine(posArr == null ? "0" : posArr.Length.ToString());
			}

			sb.AppendLine();
			sb.AppendLine("-- Runtime --");
			sb.Append("IgnoreAd: ").AppendLine(IgnoreAd.ToString());
			sb.Append("IsFirstFA: ").AppendLine(IsFirstFA.ToString());

			sb.AppendLine();
			sb.AppendLine("-- Gates --");
			sb.Append("IsDisable: ").AppendLine(IsDisable.ToString());
			sb.Append("IsRemovedAd: ").AppendLine(AdsLogic.IsRemovedAd.ToString());
			sb.Append("LastTimeFSAd: ").AppendLine(AdsLogic.LastTimeFSAd.ToString("0.###"));

			sb.AppendLine();
			sb.AppendLine("-- Groups --");
			if (core == null)
			{
				sb.AppendLine("Core: (null)");
			}
			else
			{
				string[] groups = null;
				try { groups = core.FA_GetListGroup(); } catch { }

				if (groups == null || groups.Length == 0)
				{
					sb.AppendLine("AvailableGroups: (none)");
				}
				else
				{
					sb.Append("AvailableGroups(").Append(groups.Length).AppendLine("):");
					for (int i = 0; i < groups.Length; i++)
					{
						var g = groups[i];
						if (string.IsNullOrEmpty(g)) continue;
						sb.Append(" - ").AppendLine(g);
					}
				}
			}

			sb.AppendLine();
			sb.AppendLine("-- Impression Counters (PlayerPrefs) --");
			sb.Append("TotalImpression: ").AppendLine(GetTotalImpressionCount().ToString());

			if (configs != null && configs.PositionConfigs != null && configs.PositionConfigs.Length > 0)
			{
				int enabledShow = 0;
				int disabledShow = 0;

				for (int i = 0; i < configs.PositionConfigs.Length; i++)
				{
					var p = configs.PositionConfigs[i];
					if (p == null) continue;
					if (p.CanShow) enabledShow++;
					else disabledShow++;
				}

				sb.Append("positionConfigs.canShow: ").Append(enabledShow).Append(" enabled | ").Append(disabledShow).AppendLine(" disabled");

				sb.AppendLine("positionCounts:");
				for (int i = 0; i < configs.PositionConfigs.Length; i++)
				{
					var p = configs.PositionConfigs[i];
					if (p == null) continue;

					var pos = p.PositionName ?? "";
					if (string.IsNullOrEmpty(pos)) continue;

					sb.Append(" - ").Append(pos);
					sb.Append(" | canShow=").Append(p.CanShow);
					sb.Append(" | count=").Append(GetImpressionCount(pos));
					sb.AppendLine();
				}
			}

			sb.AppendLine();
			sb.AppendLine("-- Notes --");
			sb.AppendLine("• Group debug: call GetDebugInfoGroup(groupName)");

			return sb.ToString();
		}

		public string GetDebugInfoGroup(string group)
		{
			if (core == null)
				return "ForceAd core = null (mediation not initialized yet)";

			return core.FA_GetDebugInfo(group);
		}

		public string[] GetTotalGroup()
		{
			if (core == null)
				return Array.Empty<string>();

			return core.FA_GetListGroup();
		}

		public string GetDebugInfoBreakAd()
		{
			var sb = new System.Text.StringBuilder(1400);

			sb.AppendLine("=== ForceAdLogic (BreakAd) Overview ===");

			sb.AppendLine("-- Configs (Break) --");
			if (configs == null)
			{
				sb.AppendLine("(configs null)");
			}
			else
			{
				var b = configs.BreakAdConfig;

				sb.Append("forceAd.isEnabled: ").AppendLine(configs.IsEnabled.ToString());

				sb.Append("breakAdConfig: ").AppendLine(b == null ? "(null)" : "OK");
				if (b != null)
				{
				sb.Append("breakAdConfig.isEnabled: ").AppendLine(b.IsEnabled.ToString());
					sb.Append("breakAdConfig.positionName: ").AppendLine(string.IsNullOrEmpty(b.PositionName) ? "(empty)" : b.PositionName);
					sb.Append("breakAdConfig.notificationLeadTimeSeconds: ").AppendLine(b.NotificationLeadTimeSeconds.ToString());
				}

				var pos = b != null ? b.PositionName : null;
				var posInfo = !string.IsNullOrEmpty(pos) ? GetPosInfo(pos) : null;

				sb.Append("breakPositionConfig: ").AppendLine(posInfo == null ? "(null)" : "OK");
				if (posInfo != null)
				{
					sb.Append(" - isShow: ").AppendLine(posInfo.CanShow.ToString());
					sb.Append(" - capping: ").AppendLine(posInfo.CappingTime.ToString("0.###"));
					sb.Append(" - minCappingTime(override): ").AppendLine(posInfo.MinimumCappingTime.ToString("0.###"));
					sb.Append(" - decreaseCappingPerImpr(override): ").AppendLine(posInfo.CappingDecreasePerImpression.ToString("0.###"));

					var targetTime = CalMinTime(posInfo);
					sb.Append(" - targetTime(CalMinTime): ").AppendLine(targetTime.ToString("0.###"));

					sb.Append(" - impressionCount: ").AppendLine(GetImpressionCount(pos).ToString());
				}
			}

			sb.AppendLine();
			sb.AppendLine("-- Runtime (Break) --");
			sb.Append("break_isRunning: ").AppendLine(break_isRunning.ToString());
			sb.Append("break_isAttemptingShow: ").AppendLine(break_isAttemptingShow.ToString());
			sb.Append("break_elapsed: ").AppendLine(break_elapsed.ToString("0.###"));
			sb.Append("break_notiSent: ").AppendLine(break_notiSent.ToString());
			sb.Append("break_activePos: ").AppendLine(string.IsNullOrEmpty(break_activePos) ? "(null)" : break_activePos);
			sb.Append("break_activeGroup: ").AppendLine(string.IsNullOrEmpty(break_activeGroup) ? "(null)" : break_activeGroup);

			sb.AppendLine();
			sb.AppendLine("-- Time (Break) --");
			{
				var pos = Break_GetPosSafe();
				if (string.IsNullOrEmpty(pos))
				{
					sb.AppendLine("BreakPos: (empty)");
					sb.AppendLine("break_targetTime: (n/a)");
					sb.AppendLine("break_remaining: (n/a)");
				}
				else
				{
					var posInfo = GetPosInfo(pos);
					if (posInfo == null)
					{
						sb.Append("BreakPos: ").AppendLine(pos);
						sb.AppendLine("BreakPosInfo: (null)");
						sb.AppendLine("break_targetTime: (n/a)");
						sb.AppendLine("break_remaining: (n/a)");
					}
					else
					{
						var target = CalMinTime(posInfo);
						var remaining = Mathf.Max(0f, target - break_elapsed);

						sb.Append("BreakPos: ").AppendLine(pos);
						sb.Append("break_targetTime: ").AppendLine(target.ToString("0.###"));
						sb.Append("break_remaining: ").AppendLine(remaining.ToString("0.###"));
					}
				}
			}

			sb.AppendLine();
			sb.AppendLine("-- Gates (Break) --");
			sb.Append("CoreNull: ").AppendLine((core == null).ToString());
			sb.Append("IsRemovedAd: ").AppendLine(AdsLogic.IsRemovedAd.ToString());

			if (!BreakAd_IsEnable(out var enableReason))
			{
				sb.Append("BreakAd_IsEnable: ").AppendLine("FALSE");
				sb.Append("EnableReason: ").AppendLine(enableReason);
			}
			else
			{
				sb.Append("BreakAd_IsEnable: ").AppendLine("TRUE");
			}

			sb.AppendLine();
			sb.AppendLine("-- Core Ready (Break) --");
			var breakPos = Break_GetPosSafe();
			if (string.IsNullOrEmpty(breakPos))
			{
				sb.AppendLine("BreakPos: (empty)");
			}
			else
			{
				sb.Append("BreakPos: ").AppendLine(breakPos);

				string group = null;
				try { group = core != null ? core.FA_GroupByPos(breakPos) : null; } catch { }

				sb.Append("GroupByPos: ").AppendLine(string.IsNullOrEmpty(group) ? "(null/empty)" : group);

				bool ready = false;
				try { ready = !string.IsNullOrEmpty(group) && core != null && core.FA_GetReady(group); } catch { }

				sb.Append("FA_GetReady: ").AppendLine(ready.ToString());
			}

			sb.AppendLine();
			sb.AppendLine("-- Notes --");
			sb.AppendLine("• BreakAd timer resets on NetEventSystem.OnFsBeforeOpen.");
			sb.AppendLine("• Attempt state clears on OnFsClosed / OnFsShowFailed for the BreakAd pos.");

			sb.AppendLine();
			sb.AppendLine("-- Notes (Break Events Timing) --");
			sb.AppendLine("• BreakAd_OnNotifyBeforeShow: fired in Break_OnUpdate when remaining <= notificationLeadTimeSeconds.");
			sb.AppendLine("• BreakAd_OnShown: fired inside FA_ShowAd callback (right before show pipeline).");
			sb.AppendLine("• BreakAd_OnClosed: fired on NetEventSystem.OnFsClosed for BreakPos while BreakAd isAttemptingShow==true.");
			sb.AppendLine("• BreakAd_OnShowFail: fired when Break_TryAttemptShow fails OR NetEventSystem.OnFsShowFailed for BreakPos while attempting.");

			return sb.ToString();
		}

		#endregion

		#region (4) ===== BREAK AD =====

		public event Action<string, int> BreakAd_OnNotifyBeforeShow; // (pos, secondsRemaining)
		public event Action<string, string> BreakAd_OnShowFail;       // (pos, reason)
		public event Action<string> BreakAd_OnShown;                 // (pos)
		public event Action<string> BreakAd_OnClosed;                // (pos)

		[SerializeField] private bool breakAdAutoStart;

		private bool break_isRunning;
		private float break_elapsed;
		private bool break_notiSent;

		private bool break_isAttemptingShow;
		private string break_activePos;
		private string break_activeGroup;

		private int break_lastResetFrame = -999999;

		public bool BreakAd_IsRunning => break_isRunning;

		public void StartBreakAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_fa, "Break Start", () => "StartBreakAd()"))
			{
				if (!BreakAd_IsEnable(out var reason))
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Break Start Blocked", () => reason);
					return;
				}

				break_isRunning = true;
				Break_ResetCycle("StartBreakAd", forceSameFrame: true);

				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break Start OK", () => $"pos={Break_GetPosSafe()}");
			}
		}

		public void StopBreakAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_fa, "Break Stop", () => "StopBreakAd()"))
			{
				break_isRunning = false;
				Break_ResetCycle("StopBreakAd", forceSameFrame: true);

				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break Stop OK", () => "running=false");
			}
		}

		public void BreakAd_ResetNow(string reason = "ManualReset")
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_fa, "Break ResetNow", () => $"reason={reason}"))
			{
				if (!break_isRunning)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Break ResetNow Skip", () => "not running");
					return;
				}

				Break_ResetCycle(reason, forceSameFrame: true);
				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break ResetNow Done", () => "elapsed=0, noti=false");
			}
		}

		private void Break_OnAwake()
		{
			if (breakAdAutoStart)
			{
				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break AutoStart", () => "breakAdAutoStart=true");
				StartBreakAd();
			}
		}

		private void Break_OnDestroy()
		{
			break_isRunning = false;
			break_isAttemptingShow = false;
			break_activePos = null;
			break_activeGroup = null;

			break_elapsed = 0f;
			break_notiSent = false;

			break_lastResetFrame = -999999;
		}

		// Called from NetEventSystem.OnFsBeforeOpen (ANY FS) => reset timer for 100% certainty.
		// NOTE: This is a BreakAd-local countdown reset, NOT the global capping clock.
		private void Break_OnFSBeforeOpen(AdInfo info)
		{
			if (!break_isRunning) return;

			// Any FS about to open => reset timer. (Includes break ad itself too)
			NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break Reset", () => "OnFsBeforeOpen => Break_ResetCycle");
			Break_ResetCycle("OnFsBeforeOpen");
		}

		private void Break_OnUpdate()
		{
			if (!break_isRunning) return;
			if (break_isAttemptingShow) return;

			var posInfo = Break_GetPosInfoSafe(out var pos);
			if (posInfo == null || string.IsNullOrEmpty(pos))
				return;

			if (!posInfo.CanShow)
				return;

			var targetTime = CalMinTime(posInfo);
			var notiBefore = Break_GetNotiBeforeTimeSafe();

			break_elapsed += Time.deltaTime;

			// EVENT TIMING: BreakAd_OnNotifyBeforeShow fired in Update loop, once when remaining <= NotiBeforeAdTime.
			if (!break_notiSent && notiBefore > 0)
			{
				var remaining = targetTime - break_elapsed;
				if (remaining <= notiBefore)
				{
					break_notiSent = true;

					var secRemain = Mathf.Max(0, Mathf.CeilToInt(remaining));
					BreakAd_OnNotifyBeforeShow?.Invoke(pos, secRemain);

					NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break Notify", () => $"pos={pos} secRemain={secRemain}");
				}
			}

			// EVENT TIMING: attempt show ONLY when break_elapsed >= targetTime.
			if (break_elapsed >= targetTime)
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_fa, "Break Attempt", () => $"pos={pos} target={targetTime:0.###} elapsed={break_elapsed:0.###}"))
				{
					Break_TryAttemptShow(pos);
				}
			}
		}

		private void Break_TryAttemptShow(string pos)
		{
			var group = core.FA_GroupByPos(pos);
			string posHint = NetTrackingSystem.PosTargetHint(pos);
			NetTrackingSystem.AutoShowSystemEntry(Channel.ForceAd, posHint);

			if (!BreakAd_IsEnable(out var enableReason))
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Break Attempt Blocked", () => $"EnableGateFail: {enableReason}");
				NetTrackingSystem.AutoShowSystemFail(Channel.ForceAd, ResolveBreakEnableTrackingReason(), posHint);
				Break_ResetCycle($"EnableGateFail: {enableReason}", forceSameFrame: true);
				return;
			}

			// IMPORTANT (BreakAd time-capping bypass) — per your decision.
			if (!SystemCheck(pos, out var reason, ignoreTimeCapping: true))
			{
				BreakAd_OnShowFail?.Invoke(pos, reason);

				NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Break Attempt Fail", () => $"SystemCheckFail (time gate bypassed) reason={reason}");
				NetTrackingSystem.AutoShowSystemFail(Channel.ForceAd, ResolveBreakAutoShowTrackingReason(pos, ignoreTimeCapping: true), posHint);

				Break_ResetCycle("SystemCheckFail", forceSameFrame: true);
				return;
			}

			if (string.IsNullOrEmpty(group))
			{
				var r = "GroupByPos returned null/empty";
				BreakAd_OnShowFail?.Invoke(pos, r);

				NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Break Attempt Fail", () => r);
				NetTrackingSystem.AutoShowSystemFail(Channel.ForceAd, TrackingReason.NullData, posHint);

				Break_ResetCycle("GroupNull", forceSameFrame: true);
				return;
			}

			GroupAdType? resolvedAdType = null;
			string resolvedIdentitySource = string.Empty;

			if (core != null && core.FA_TryGetTrackingIdentity(group, out var resolvedType, out var resolvedId))
			{
				resolvedAdType = resolvedType;
				resolvedIdentitySource = resolvedId;
			}

			if (!core.FA_GetReady(group))
			{
				var r = "Core not ready";
				BreakAd_OnShowFail?.Invoke(pos, r);

				NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Break Attempt Fail", () => $"group={group} {r}");
				NetTrackingSystem.AutoShowSystemFail(Channel.ForceAd, TrackingReason.NotReady, posHint);

				Break_ResetCycle("NotReady", forceSameFrame: true);
				return;
			}

			break_isAttemptingShow = true;
			break_activePos = pos;
			break_activeGroup = group;

			NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break Attempt Show", () => $"pos={pos} group={group}");

			var ok = core.FA_ShowAd(group, pos, () =>
			{
				// EVENT TIMING: BreakAd_OnShown fired inside FA_ShowAd callback.
				BreakAd_OnShown?.Invoke(pos);

				NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break OnShown", () => $"pos={pos} group={group}");
			}, null, resolvedAdType, resolvedIdentitySource);

			if (!ok)
			{
				break_isAttemptingShow = false;
				break_activePos = null;
				break_activeGroup = null;

				var r = "FA_ShowAd returned false";
				BreakAd_OnShowFail?.Invoke(pos, r);

				NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Break Attempt Fail", () => r);

				Break_ResetCycle("ShowReturnFalse", forceSameFrame: true);
				return;
			}

			NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break Attempt OK", () => "FA_ShowAd requested");
		}

		private void Break_ResetCycle(string reason, bool forceSameFrame = false)
		{
			if (!forceSameFrame && break_lastResetFrame == Time.frameCount)
				return;

			break_lastResetFrame = Time.frameCount;

			break_elapsed = 0f;
			break_notiSent = false;

			// Only clear attempt state when stopping or failing.
			// NOTE: OnFsBeforeOpen does NOT clear attempt-state; BreakAd needs it to catch close/fail events properly.
			if (reason == "StopBreakAd"
				|| reason.StartsWith("ShowFail")
				|| reason == "ShowReturnFalse"
				|| reason == "NotReady"
				|| reason == "GroupNull"
				|| reason == "SystemCheckFail"
				|| reason.StartsWith("EnableGateFail"))
			{
				break_isAttemptingShow = false;
				break_activePos = null;
				break_activeGroup = null;
			}

			NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break ResetCycle", () => $"reason={reason}");
		}

		private bool BreakAd_IsEnable(out string reason)
		{
			if (configs == null)
			{
				reason = "Configs null";
				return false;
			}

			if (!configs.IsEnabled)
			{
				reason = "ForceAd configs disabled";
				return false;
			}

			var b = configs.BreakAdConfig;
			if (b == null || !b.IsEnabled)
			{
				reason = "BreakAdConfig disabled/null";
				return false;
			}

			if (AdsLogic.IsRemovedAd)
			{
				reason = "RemovedAd";
				return false;
			}

			var pos = Break_GetPosSafe();
			if (string.IsNullOrEmpty(pos))
			{
				reason = "Break pos empty";
				return false;
			}

			var posInfo = GetPosInfo(pos);
			if (posInfo == null)
			{
				reason = $"Break position not found in positionConfigs: {pos}";
				return false;
			}

			reason = "";
			return true;
		}

		private string Break_GetPosSafe()
		{
			return configs?.BreakAdConfig?.PositionName;
		}

		private int Break_GetNotiBeforeTimeSafe()
		{
			return Mathf.Max(0, configs?.BreakAdConfig?.NotificationLeadTimeSeconds ?? 0);
		}

		private AdSystemConfigs.ForceAdChannelConfig.ForceAdPositionConfig Break_GetPosInfoSafe(out string pos)
		{
			pos = Break_GetPosSafe();
			if (string.IsNullOrEmpty(pos)) return null;

			return GetPosInfo(pos);
		}

		private void Break_OnFSClosed(AdInfo info)
		{
			if (!break_isRunning) return;
			if (!break_isAttemptingShow) return;

			if (string.IsNullOrEmpty(break_activePos)) return;
			if (info.pos != break_activePos) return;

			var pos = break_activePos;

			break_isAttemptingShow = false;
			break_activePos = null;
			break_activeGroup = null;

			Break_ResetCycle("Break_OnClosed", forceSameFrame: true);

			// EVENT TIMING: fired on NetEventSystem.OnFsClosed for BreakPos after ad closed.
			BreakAd_OnClosed?.Invoke(pos);

			NetFlowDebugSystem.Log(Layer.sys, Module.format_fa, "Break Closed", () => $"pos={pos}");
		}

		private void Break_OnFSShowFail(AdInfo info)
		{
			if (!break_isRunning) return;
			if (!break_isAttemptingShow) return;

            if (string.IsNullOrEmpty(break_activePos)) return;
			if (info.pos != break_activePos) return;

			var pos = break_activePos;

			break_isAttemptingShow = false;
			break_activePos = null;
			break_activeGroup = null;

			// EVENT TIMING: fired on NetEventSystem.OnFsShowFailed for BreakPos while attempting.
			BreakAd_OnShowFail?.Invoke(pos, "OnFsShowFailed");

			NetFlowDebugSystem.Warn(Layer.sys, Module.format_fa, "Break ShowFail Event", () => $"pos={pos}");

			Break_ResetCycle("ShowFailEvent", forceSameFrame: true);
		}

		#endregion
	}
}
