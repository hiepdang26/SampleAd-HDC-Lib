using BG_Library.Common;
using BG_Library.NET.API;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using System;
using UnityEngine;

namespace BG_Library.NET.AdSystem
{
	public class PopUpSystem : MonoBehaviour
	{
		private AdCoreBase core;
		private AdSystemConfigs.PopupChannelConfig configs;
		private Action onAdCoreInitCompletedHandler;

		public void Setup(AdCoreBase _core, AdSystemConfigs.PopupChannelConfig _configs)
		{
			core = _core;
			configs = _configs;

			NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "Setup",
				() => $"core={(core != null)} enable={(configs != null && configs.IsEnabled)} posCount={(configs?.PositionConfigs?.Length ?? 0)}");
		}

		#region (1) ===== LOGIC =====

		private void Awake()
		{
			onAdCoreInitCompletedHandler = () =>
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_pu, "Init Auto", () => "OnAdCoreInitCompleted"))
				{
					TryAutoInitializeGroups();
				}
			};

			NetEventSystem.OnAdCoreInitCompleted += onAdCoreInitCompletedHandler;
		}

		private void OnDestroy()
		{
			if (onAdCoreInitCompletedHandler != null)
				NetEventSystem.OnAdCoreInitCompleted -= onAdCoreInitCompletedHandler;
		}

		private bool SystemCheck(string pos, out string reason)
		{
			if (IsDisable)
			{
				var isEnable = configs != null && configs.IsEnabled;
				reason = $"IsDisable. IsRemovedAd: {AdsLogic.IsRemovedAd}, IsEnable: {isEnable}";
				return false;
			}

			var posInfo = GetPosInfo(pos);
			if (posInfo == null || !posInfo.IsEnabled)
			{
				reason = "Pos is disable";
				return false;
			}

			reason = "";
			return true;
		}

		private AdSystemConfigs.PopupChannelConfig.PopupPositionConfig GetPosInfo(string pos)
		{
			if (configs == null || configs.PositionConfigs == null) return null;
			return Array.Find(configs.PositionConfigs, c => c != null && c.PositionName == pos);
		}

		#endregion

		#region (2) ===== PUBLIC API =====

		public void InitManually(string groupName)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_pu, "InitManually", () => $"group={groupName}"))
			{
				if (IsAutoInitGroup(groupName))
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "InitManually Rejected", () => $"group={groupName} AutoInit=true");
					return;
				}

				string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
				NetTrackingSystem.RequestSystemEntry(Channel.Popup, groupHint);
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "InitManually Blocked", () => "IsDisable=true");
					NetTrackingSystem.RequestSystemFail(Channel.Popup, DisableTrackingReason, groupHint);

					return;
				}

				GroupAdType? resolvedAdType = null;
				string resolvedIdentitySource = string.Empty;
				if (core != null && core.PU_TryGetTrackingIdentity(groupName, out var adType, out var adUnitId))
				{
					resolvedAdType = adType;
					resolvedIdentitySource = adUnitId;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "InitManually CallCore", () => $"PU_Initialize({groupName})");
				core.PU_Initialize(groupName, resolvedAdType, resolvedIdentitySource);
			}
		}

		public void ForceInitManually(string groupName)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_pu, "ForceInitManually", () => $"group={groupName}"))
			{
				string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
				NetTrackingSystem.RequestSystemEntry(Channel.Popup, groupHint);
				if (IsDisable)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "ForceInitManually Blocked", () => "IsDisable=true");
					NetTrackingSystem.RequestSystemFail(Channel.Popup, DisableTrackingReason, groupHint);
					return;
				}

				GroupAdType? resolvedAdType = null;
				string resolvedIdentitySource = string.Empty;
				if (core != null && core.PU_TryGetTrackingIdentity(groupName, out var adType, out var adUnitId))
				{
					resolvedAdType = adType;
					resolvedIdentitySource = adUnitId;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "ForceInitManually CallCore", () => $"PU_ForceInitialize({groupName})");
				core.PU_ForceInitialize(groupName, resolvedAdType, resolvedIdentitySource);
			}
		}

		public void Show(string pos)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_pu, "Show", () => $"pos={pos}"))
			{
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				var group = core.PU_GroupByPos(pos);
				NetTrackingSystem.ShowSystemEntry(Channel.Popup, posHint);

				if (!SystemCheck(pos, out var reason))
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "Show Blocked", () => $"reason={reason}");
					NetTrackingSystem.ShowSystemFail(Channel.Popup, ResolveSystemCheckTrackingReason(pos), posHint);
					return;
				}

				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "Resolved Group", () => $"group={group}");
				GroupAdType? resolvedAdType = null;
				string resolvedIdentitySource = string.Empty;
				if (core != null && core.PU_TryGetTrackingIdentity(group, out var adType, out var adUnitId))
				{
					resolvedAdType = adType;
					resolvedIdentitySource = adUnitId;
				}

				core.PU_ShowAd(group, pos, resolvedAdType, resolvedIdentitySource);
				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "Show CallCore", () => "PU_ShowAd(group,pos)");
			}
		}

		public void Hide(string pos)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_pu, "Hide", () => $"pos={pos}"))
			{
				// ? Hide không ch?n b?i IsDisable (d? luôn d?n UI du?c)
				string posHint = NetTrackingSystem.PosTargetHint(pos);
				var group = core.PU_GroupByPos(pos);
				NetTrackingSystem.HideSystemEntry(Channel.Popup, posHint);

				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "Resolved Group", () => $"group={group}");
				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "Hide CallCore", () => "PU_HideAd(group)");

				core.PU_HideAd(group, pos);
			}
		}

		public void UpdatePos(string pos, PULayout layout)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_pu, "UpdatePos", () => $"pos={pos} layout={(layout != null)}"))
			{
				if (!SystemCheck(pos, out var reason))
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "UpdatePos Blocked", () => $"reason={reason}");
					return;
				}

				if (layout == null)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "UpdatePos Blocked", () => "reason=layout_null");
					return;
				}

				var group = core.PU_GroupByPos(pos);
				var coor = layout.CenterNormalized;
				var size = layout.SizeInPixels;

				var density = Master.GetScreenDensity();
				if (density <= 0f)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "UpdatePos Blocked", () => $"reason=invalid_density value={density:0.###}");
					return;
				}

				var wDp = size.x / density;
				var hDp = size.y / density;

				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "Resolved Group", () => $"group={group}");
				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "Layout",
					() => $"center=({coor.x:0.###},{coor.y:0.###}) sizePx=({size.x:0.###},{size.y:0.###}) density={density:0.###} sizeDp=({wDp:0.###},{hDp:0.###})");

				core.PU_UpdatePos(
					group,
					coor.x,
					coor.y,
					wDp,
					hDp
				);

				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "UpdatePos CallCore", () => "PU_UpdatePos(group,x,y,wDp,hDp)");
			}
		}

		public bool AbleToShow(string pos)
		{
			if (!SystemCheck(pos, out var reason))
				{
					return false;
				}

				var groupName = core.PU_GroupByPos(pos);
				var ready = core.PU_GetReady(groupName);

				if (!ready) return false;
				return true;
		}

        public bool IsGroupReady(string groupName)
        {
            if (core == null || string.IsNullOrEmpty(groupName))
                return false;

            return core.PU_GetReady(groupName);
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

		private void TryAutoInitializeGroups()
		{
			if (IsDisable)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "Init Auto Blocked", () => "IsDisable=true");
				return;
			}

			var groupNames = core?.PU_GetAutoInitGroupNames(configs) ?? Array.Empty<string>();
			if (groupNames.Length == 0)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.format_pu, "Init Auto Skip", () => "no auto-init groups");
				return;
			}

			for (int i = 0; i < groupNames.Length; i++)
			{
				var groupName = groupNames[i];
				if (string.IsNullOrEmpty(groupName))
					continue;

				string groupHint = NetTrackingSystem.GroupTargetHint(groupName);
				NetTrackingSystem.RequestSystemEntry(Channel.Popup, groupHint);
				NetFlowDebugSystem.Log(Layer.sys, Module.format_pu, "Init Auto CallCore", () => $"PU_Initialize({groupName})");
				core.PU_Initialize(groupName);
			}
		}

		private bool IsAutoInitGroup(string groupName)
		{
			if (string.IsNullOrEmpty(groupName))
				return false;

			var groupNames = core?.PU_GetAutoInitGroupNames(configs);
			if (groupNames == null || groupNames.Length == 0)
				return false;

			for (int i = 0; i < groupNames.Length; i++)
			{
				if (string.Equals(groupNames[i], groupName, StringComparison.Ordinal))
					return true;
			}

			return false;
		}
		private TrackingReason ResolveSystemCheckTrackingReason(string pos)
		{
			if (AdsLogic.IsRemovedAd) return TrackingReason.IapRemoved;
			if (configs == null || !configs.IsEnabled) return TrackingReason.ConfigDisabled;

			var posInfo = GetPosInfo(pos);
			if (posInfo == null || !posInfo.IsEnabled) return TrackingReason.PositionBlocked;

			return TrackingReason.GateBlocked;
		}

		public string GetDebugInfo()
		{
			var sb = new System.Text.StringBuilder(1200);

			sb.AppendLine("=== PopUpLogic (PU) Overview ===");

			sb.AppendLine("-- Configs --");
			if (configs == null)
			{
				sb.AppendLine("(null)");
			}
			else
			{
				sb.Append("isEnabled: ").AppendLine(configs.IsEnabled.ToString());

				var posInfos = configs.PositionConfigs;
				var count = posInfos != null ? posInfos.Length : 0;
				sb.Append("PosCount: ").AppendLine(count.ToString());

				if (posInfos == null || posInfos.Length == 0)
				{
					sb.AppendLine("positionConfigs: (empty)");
				}
				else
				{
					sb.AppendLine("positionConfigs:");
					for (int i = 0; i < posInfos.Length; i++)
					{
						var p = posInfos[i];
						if (p == null) continue;

						sb.Append(" - ").Append(p.PositionName ?? "");
						sb.Append(" | isEnabled=").Append(p.IsEnabled);
						sb.AppendLine();
					}
				}
			}

			sb.AppendLine();
			sb.AppendLine("-- Runtime --");
			sb.Append("IsDisable: ").AppendLine(IsDisable.ToString());
			sb.Append("IsRemovedAd: ").AppendLine(AdsLogic.IsRemovedAd.ToString());
			sb.Append("coreNull: ").AppendLine((core == null).ToString());

			sb.AppendLine();
			sb.AppendLine("-- Groups (from AdsCore) --");
			if (core == null)
			{
				sb.AppendLine("core: (null)");
			}
			else
			{
				string[] groups = null;
				try { groups = core.PU_GetListGroup(); } catch { }

				if (groups == null || groups.Length == 0)
				{
					sb.AppendLine("PU_GetListGroup(): (empty)");
				}
				else
				{
					sb.Append("PU_GetListGroup(): ").AppendLine(groups.Length.ToString());
					for (int i = 0; i < groups.Length; i++)
					{
						var g = groups[i];
						if (string.IsNullOrEmpty(g)) continue;

						bool ready = false;
						try { ready = core.PU_GetReady(g); } catch { }

						sb.Append(" - ").Append(g);
						sb.Append(" | Ready=").Append(ready);
						sb.AppendLine();
					}
				}
			}

			sb.AppendLine();
			sb.AppendLine("-- Notes --");
			sb.AppendLine("• Group debug is shown in separate UI via core.PU_GetDebugInfo(group) (if you expose it).");
			sb.AppendLine("• AbleToShow(pos) requires: config enabled + pos enabled + group ready.");

			return sb.ToString();
		}

		public string GetDebugInfoGroup(string group)
		{
			if (core == null)
				return "Popup core = null (mediation not initialized yet)";

			return core.PU_GetDebugInfo(group);
		}

		public string[] GetTotalGroup()
		{
			if (core == null)
				return Array.Empty<string>();

			return core.PU_GetListGroup();
		}

		#endregion
	}
}

