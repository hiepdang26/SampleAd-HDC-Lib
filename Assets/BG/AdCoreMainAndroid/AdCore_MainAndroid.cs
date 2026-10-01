using BG_Library.Common;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Admob;
using BG_Library.NET.Mediation.Android;
using BG_Library.NET.Mediation.Max;
using BG_Library.NET.Tracking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.NET.AdCore.MainAndroid
{
	[CreateAssetMenu(fileName = "AdCore SO", menuName = "BG_Library/NET/AdCore/AdCoreMainAndroid")]
	public class AdCore_MainAndroid : AdCoreBase
	{
		private Admob_MediationManager mediation1;
		private Android_MediationManager mediation2;
		private Max_MediationManager mediation3;
		private Configs configs;
        private bool requireMaxMediation;

		// Caches
		private Dictionary<string, IFSGroup> faGroupCache;
		private Dictionary<string, IRectGroup> puGroupCache;
		private Dictionary<BannerPlacement, IRectGroup> bnGroupCache;
		private FsFallbackGroup rwGroup;
		private FsFallbackGroup aoGroup;
		private MrecFallbackGroup mrecGroup;

		public Configs ConfigsIns => configs;

		public override string AdCoreName => "adcore_main_android";

		public override void InitPluginAtAwake()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "InitPluginAtAwake", () => $"core={AdCoreName}"))
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Call", () => "Admob_MediationManager.InitMediation()");
				Admob_MediationManager.InitMediation();
			}
		}

		public override void InitStats(string configSt)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "InitStats", () => $"core={AdCoreName} len={(configSt?.Length ?? 0)}"))
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Deserialize", () => "Configs from json");
				configs = JsonTool.DeserializeObject<Configs>(configSt);

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Create", () => "Admob_MediationManager(configs)");
				mediation1 = new Admob_MediationManager(configs);

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Create", () => "Android_MediationManager(configs)");
				mediation2 = new Android_MediationManager(configs);

                NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Create", () => "Max_MediationManager(configs)");
				mediation3 = new Max_MediationManager(configs);
				requireMaxMediation = false;
				rwGroup?.Dispose();
				rwGroup = null;
				aoGroup?.Dispose();
				aoGroup = null;
				mrecGroup?.Dispose();
				mrecGroup = null;
				DisposeGroupCache(faGroupCache);
				DisposeGroupCache(puGroupCache);
				DisposeGroupCache(bnGroupCache);
				alGroup = null;
				arGroup = null;

				// Init caches (InitStats is called once, but safe and clear)
				faGroupCache = new Dictionary<string, IFSGroup>(System.StringComparer.Ordinal);
				puGroupCache = new Dictionary<string, IRectGroup>(System.StringComparer.Ordinal);
				bnGroupCache = new Dictionary<BannerPlacement, IRectGroup>();

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () =>
					$"configs={(configs != null)} admob={(mediation1 != null)} android={(mediation2 != null)} max={(mediation3 != null)} " +
					$"faCache={(faGroupCache != null)} puCache={(puGroupCache != null)}");
			}
		}

		public override void InitializeMediation()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "InitializeMediation", () => $"core={AdCoreName}"))
			{
                requireMaxMediation = HasAnyMaxMediationConfigured();

				// Nếu chưa được call Init từ trước
				if (!Admob_MediationManager.IsCallInit)
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Call", () => "Admob_MediationManager.InitMediation()");
					Admob_MediationManager.InitMediation();
				}
				else
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Skip", () => "Admob init already called");
				}

                if (requireMaxMediation)
                {
                    if (!Max_MediationManager.IsCallInit)
                    {
                        NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Call", () => "Max_MediationManager.InitMediation()");
                        Max_MediationManager.InitMediation();
                    }
                    else
                    {
                        NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Skip", () => "Max init already called");
                    }
                }
                else
                {
                    NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Skip", () => "Max mediation not required by current configs");
                }

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "StartCoroutine", () => "WaitPluginAllDone()");
				AdsLogic.Ins.StartCoroutine(WaitPluginAllDone());
			}
		}

		private IEnumerator WaitPluginAllDone()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "WaitPluginAllDone", () => $"core={AdCoreName}"))
			{
				yield return new WaitUntil(() =>
                    Admob_MediationManager.IsInitComplete &&
                    (!requireMaxMediation || Max_MediationManager.IsInitComplete));

                NetEventSystem.OnAdCoreInitCompleted?.Invoke();
                NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Emit", () => "OnAdCoreInitCompleted");
			}
		}

		private static void DisposeGroupCache<TKey, TValue>(Dictionary<TKey, TValue> cache)
		{
			if (cache == null)
				return;

			foreach (var pair in cache)
			{
				if (pair.Value is IDisposable disposable)
					disposable.Dispose();
			}
		}

        public override string DebugRecheckLogic()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "DebugRecheckLogic", () => $"core={AdCoreName}"))
			{
				var report = AdConfigsValidator.Validate(AdsLogic.AdsConfigIns, configs);
				var st = report.ToDebugString();

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"len={(st?.Length ?? 0)}");
				return st;
			}
		}

		#region FA

		protected override IFSGroup FA_GetGroup(string groupName)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "FA_GetGroup", () => $"groupName={(groupName ?? "(null)")}"))
			{
				if (string.IsNullOrEmpty(groupName))
				{
					NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Fail", () => "GroupName NullOrEmpty");
					return null;
				}

				// Cache
				if (faGroupCache != null && faGroupCache.TryGetValue(groupName, out var cached) && cached != null)
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "CacheHit", () => $"groupName={groupName}");
					return cached;
				}

				var groupConfig = FindFAGroupConfig(groupName);
				if (groupConfig == null)
				{
					NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Fail", () => $"Group config not found: {groupName}");
					return null;
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Priority", () => $"{groupConfig.MediationPriority} backup={groupConfig.UseBackup}");

				var candidates = BuildForceAdFallbackCandidates(groupConfig);
				IFSGroup group = candidates.Count > 0
					? new FsFallbackGroup(candidates, groupConfig.UseBackup, BG_ConstValue.adtype_fa, GroupAdType.ForceAd, "FA Fallback", Channel.ForceAd, _ => Module.format_fa)
					: null;

				if (group != null && faGroupCache != null)
				{
					faGroupCache[groupName] = group;
					NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "CacheStore", () => $"groupName={groupName}");
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"group={(group != null)} candidates={candidates.Count}");
				return group;
			}
		}

		private ForceAdGroupConfig FindFAGroupConfig(string groupName)
		{
			if (configs?.ForceAdGroups == null || string.IsNullOrEmpty(groupName))
				return null;

			for (int i = 0; i < configs.ForceAdGroups.Length; i++)
			{
				var group = configs.ForceAdGroups[i];
				if (group == null)
					continue;

				if (string.Equals(group.GroupName, groupName, System.StringComparison.Ordinal))
					return group;
			}

			return null;
		}

		private List<FallbackCandidate<IFSGroup>> BuildForceAdFallbackCandidates(ForceAdGroupConfig groupConfig)
		{
			var result = new List<FallbackCandidate<IFSGroup>>(3);
			if (groupConfig == null)
				return result;

			var orderedPriorities = BuildRewardedPriorityOrder(groupConfig.MediationPriority, groupConfig.UseBackup);
			string groupName = groupConfig.GroupName ?? "";

			for (int i = 0; i < orderedPriorities.Count; i++)
			{
				var priority = orderedPriorities[i];
				switch (priority)
				{
					case E_MediationPriority.Admob:
						if (TryGetAdmob("BuildForceAdFallbackCandidates", out var admob))
						{
							var group = admob.FA_GetGroup(groupName);
							if (!string.IsNullOrEmpty(group?.Id))
								result.Add(new FallbackCandidate<IFSGroup>("Admob", BG_ConstValue.mediation_admob, group));
						}
						break;

					case E_MediationPriority.Max:
						var maxGroup = CreateForceAdMaxGroup(groupName);
						if (!string.IsNullOrEmpty(maxGroup?.Id))
							result.Add(new FallbackCandidate<IFSGroup>("Max", BG_ConstValue.mediation_max, maxGroup));
						break;

					case E_MediationPriority.Android:
						if (TryGetAndroid("BuildForceAdFallbackCandidates", out var android))
						{
							var group = android.FA_GetGroup(groupName);
							if (!string.IsNullOrEmpty(group?.Id))
								result.Add(new FallbackCandidate<IFSGroup>("Android", BG_ConstValue.mediation_android, group));
						}
						break;
				}
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "FA BuildCandidates",
				() => $"group={groupName} priority={groupConfig.MediationPriority} useBackup={groupConfig.UseBackup} count={result.Count}");
			return result;
		}

		private IFSGroup CreateForceAdMaxGroup(string groupName)
		{
			var maxUnit = configs?.ForceAdMaxUnit?.MaxUnit;
			if (string.IsNullOrEmpty(maxUnit))
				return null;

			return new Max_FSGroupController<Max_FAInfo, Max_FAAccessAPI>(
				info: new Max_FAInfo(maxUnit),
				format: BG_ConstValue.adtype_fa,
				groupName: groupName ?? Max_MediationManager.GroupName_Fa,
				mediation: BG_ConstValue.mediation_max);
		}

		public override string FA_GroupByPos(string pos)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "FA_GroupByPos", () => $"pos={(pos ?? "(null)")}"))
			{
				if (string.IsNullOrEmpty(pos))
				{
					NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Fail", () => "Pos NullOrEmpty");
					return null;
				}

				var g = configs.GetFAGroupByPos(pos);
				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"group={(g ?? "(null)")}");
				return g;
			}
		}

		public override string[] FA_GetListGroup()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "FA_GetListGroup", () => ""))
			{
				var groupNames = new string[configs.ForceAdGroups.Length];
				for (int i = 0; i < groupNames.Length; i++)
				{
					groupNames[i] = configs.ForceAdGroups[i].GroupName;
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"count={(groupNames?.Length ?? 0)}");
				return groupNames;
			}
		}

		public override string[] FA_GetAutoInitGroupNames(AdSystemConfigs.ForceAdChannelConfig channelConfig)
		{
			if (channelConfig?.PositionConfigs == null || channelConfig.PositionConfigs.Length == 0 || configs == null)
				return Array.Empty<string>();

			var result = new List<string>();
			var seen = new HashSet<string>(StringComparer.Ordinal);

			for (int i = 0; i < channelConfig.PositionConfigs.Length; i++)
			{
				var posConfig = channelConfig.PositionConfigs[i];
				if (posConfig == null || !posConfig.AutoInit || !posConfig.CanShow || string.IsNullOrEmpty(posConfig.PositionName))
					continue;

				var groupName = configs.GetFAGroupByPos(posConfig.PositionName);
				if (string.IsNullOrEmpty(groupName) || !seen.Add(groupName))
					continue;

				result.Add(groupName);
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "FA AutoInitGroups",
				() => $"count={result.Count}");
			return result.ToArray();
		}

		#endregion

		#region RW

		private void RW_InitGroup()
		{
			if (rwGroup != null) return;

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "RW_InitGroup", () => $"core={AdCoreName}"))
			{
				var candidates = BuildRewardedFallbackCandidates();
				rwGroup = candidates.Count > 0
					? new FsFallbackGroup(candidates, configs.RewardedUnit.UseBackup, BG_ConstValue.adtype_rw, GroupAdType.Rewarded, "RW Fallback", Channel.Rewarded, _ => Module.format_rw)
					: null;

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"rwGroup={(rwGroup != null)} candidates={candidates.Count}");
			}
		}

		protected override IFSGroup RW_GetGroup()
		{
			RW_InitGroup();
			return rwGroup;
		}

		#endregion

		private List<FallbackCandidate<IFSGroup>> BuildRewardedFallbackCandidates()
		{
			var result = new List<FallbackCandidate<IFSGroup>>(3);
			var rewardedConfig = configs?.RewardedUnit;
			var orderedPriorities = BuildRewardedPriorityOrder(
				rewardedConfig?.MediationPriority ?? E_MediationPriority.Admob,
				rewardedConfig?.UseBackup ?? false);

			for (int i = 0; i < orderedPriorities.Count; i++)
			{
				var priority = orderedPriorities[i];
				switch (priority)
				{
					case E_MediationPriority.Admob:
						if (TryGetAdmob("BuildRewardedFallbackCandidates", out var admob) && !string.IsNullOrEmpty(admob.RW_Group?.Id))
							result.Add(new FallbackCandidate<IFSGroup>("Admob", BG_ConstValue.mediation_admob, admob.RW_Group));
						break;

					case E_MediationPriority.Max:
						if (TryGetMax("BuildRewardedFallbackCandidates", out var max) && !string.IsNullOrEmpty(max.RW_Group?.Id))
							result.Add(new FallbackCandidate<IFSGroup>("Max", BG_ConstValue.mediation_max, max.RW_Group));
						break;

					case E_MediationPriority.Android:
						if (TryGetAndroid("BuildRewardedFallbackCandidates", out var android) && !string.IsNullOrEmpty(android.RW_Group?.Id))
							result.Add(new FallbackCandidate<IFSGroup>("Android", BG_ConstValue.mediation_android, android.RW_Group));
						break;
				}
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "RW BuildCandidates",
				() => $"priority={configs?.RewardedUnit?.MediationPriority} useBackup={configs?.RewardedUnit?.UseBackup} count={result.Count}");
			return result;
		}

		private static List<E_MediationPriority> BuildRewardedPriorityOrder(E_MediationPriority priority, bool useBackup)
		{
			var ordered = new List<E_MediationPriority>(3) { priority };
			if (!useBackup)
				return ordered;

			var canonical = new[] { E_MediationPriority.Admob, E_MediationPriority.Max, E_MediationPriority.Android };

			for (int i = 0; i < canonical.Length; i++)
			{
				if (!ordered.Contains(canonical[i]))
					ordered.Add(canonical[i]);
			}

			return ordered;
		}

		#region AL

		private IFSGroup alGroup;

		private void AL_InitGroup()
		{
			if (alGroup != null) return;

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "AL_InitGroup", () => $"core={AdCoreName} launchAdType={configs?.ComebackChannel?.LaunchAdType}"))
			{
				if (configs == null)
				{
					NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Fail", () => "configs null (InitStats not called?)");
					return;
				}

				switch (configs.ComebackChannel.LaunchAdType)
				{
					case E_ComebackAdType.FA:
						string launchGroupName = configs.ComebackChannel.LaunchForceAdGroupName;
						NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Resolve", () => $"FA group={(string.IsNullOrWhiteSpace(launchGroupName) ? "(empty)" : launchGroupName)}");
						if (string.IsNullOrWhiteSpace(launchGroupName))
						{
							NetFlowDebugSystem.Warn(Layer.adcore, Module.adcore, "Config", () => "LaunchForceAdGroupName empty -> AL group unresolved");
							break;
						}

						alGroup = FA_GetGroup(launchGroupName);
						break;

					case E_ComebackAdType.AO:
						AL_SetFromAO();
						break;

					default:
						break;
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"alGroup={(alGroup != null)}");
			}
		}

		private void AL_SetFromAO()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "AL_SetFromAO", () => $"core={AdCoreName}"))
			{
				alGroup = AO_GetSharedFallbackGroup();
			}
		}

		protected override IFSGroup AL_GetGroup()
		{
			AL_InitGroup();
			return alGroup;
		}

		#endregion

		#region AR

		private IFSGroup arGroup;

		private void AR_InitGroup()
		{
			if (arGroup != null) return;

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "AR_InitGroup", () => $"core={AdCoreName} resumeAdType={configs?.ComebackChannel?.ResumeAdType}"))
			{
				if (configs == null)
				{
					NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Fail", () => "configs null (InitStats not called?)");
					return;
				}

				switch (configs.ComebackChannel.ResumeAdType)
				{
					case E_ComebackAdType.FA:
						string resumeGroupName = configs.ComebackChannel.ResumeForceAdGroupName;
						NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Resolve", () => $"FA group={(string.IsNullOrWhiteSpace(resumeGroupName) ? "(empty)" : resumeGroupName)}");
						if (string.IsNullOrWhiteSpace(resumeGroupName))
						{
							NetFlowDebugSystem.Warn(Layer.adcore, Module.adcore, "Config", () => "ResumeForceAdGroupName empty -> AR group unresolved");
							break;
						}

						arGroup = FA_GetGroup(resumeGroupName);
						break;

					case E_ComebackAdType.AO:
						AR_SetFromAO();
						break;

					default:
						break;
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"arGroup={(arGroup != null)}");
			}
		}

		private void AR_SetFromAO()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "AR_SetFromAO", () => $"core={AdCoreName}"))
			{
				arGroup = AO_GetSharedFallbackGroup();
			}
		}

		protected override IFSGroup AR_GetGroup()
		{
			AR_InitGroup();
			return arGroup;
		}

		private FsFallbackGroup AO_GetSharedFallbackGroup()
		{
			if (aoGroup != null)
				return aoGroup;

			var candidates = BuildAppOpenFallbackCandidates();
			aoGroup = candidates.Count > 0
				? new FsFallbackGroup(
					candidates,
					configs.AppOpenUnit.UseBackup,
					BG_ConstValue.adtype_ao,
					GroupAdType.AppOpen,
					"AO Fallback",
					Channel.AppLaunch,
					channel => channel == Channel.AppResume ? Module.format_ar : Module.format_al)
				: null;

			NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "AO SharedFallback",
				() => $"aoGroup={(aoGroup != null)} candidates={candidates.Count}");
			return aoGroup;
		}

		private List<FallbackCandidate<IFSGroup>> BuildAppOpenFallbackCandidates()
		{
			var result = new List<FallbackCandidate<IFSGroup>>(2);
			var appOpenConfig = configs?.AppOpenUnit;
			var orderedPriorities = BuildAppOpenPriorityOrder(
				appOpenConfig?.MediationPriority ?? E_AdmobMaxMediationPriority.Admob,
				appOpenConfig?.UseBackup ?? false);

			for (int i = 0; i < orderedPriorities.Count; i++)
			{
				var priority = orderedPriorities[i];
				switch (priority)
				{
					case E_AdmobMaxMediationPriority.Admob:
						if (TryGetAdmob("BuildAppOpenFallbackCandidates", out var admob) && !string.IsNullOrEmpty(admob.AO_Group?.Id))
							result.Add(new FallbackCandidate<IFSGroup>("Admob", BG_ConstValue.mediation_admob, admob.AO_Group));
						break;

					case E_AdmobMaxMediationPriority.Max:
						if (TryGetMax("BuildAppOpenFallbackCandidates", out var max) && !string.IsNullOrEmpty(max.AO_Group?.Id))
							result.Add(new FallbackCandidate<IFSGroup>("Max", BG_ConstValue.mediation_max, max.AO_Group));
						break;
				}
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "AO BuildCandidates",
				() => $"priority={configs?.AppOpenUnit?.MediationPriority} useBackup={configs?.AppOpenUnit?.UseBackup} count={result.Count}");
			return result;
		}

		private static List<E_AdmobMaxMediationPriority> BuildAppOpenPriorityOrder(E_AdmobMaxMediationPriority priority, bool useBackup)
		{
			var ordered = new List<E_AdmobMaxMediationPriority>(2) { priority };
			if (!useBackup)
				return ordered;

			var canonical = new[] { E_AdmobMaxMediationPriority.Admob, E_AdmobMaxMediationPriority.Max };

			for (int i = 0; i < canonical.Length; i++)
			{
				if (!ordered.Contains(canonical[i]))
					ordered.Add(canonical[i]);
			}

			return ordered;
		}

		#endregion

		#region BN

		protected override IRectGroup BN_GetGroup(BannerPlacement placement)
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "BN_GetGroup", () => $"placement={placement}"))
			{
				if (bnGroupCache != null && bnGroupCache.TryGetValue(placement, out var cached) && cached != null)
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "CacheHit", () => $"placement={placement}");
					return cached;
				}

				var useBackup = GetBannerUseBackup(placement);
				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "BN Mediation",
					() => $"{placement} => {GetBannerPriorityLabel(placement)} backup={useBackup}");

				var candidates = BuildBannerFallbackCandidates(placement);
				IRectGroup group = candidates.Count > 0 ? new BannerFallbackGroup(candidates, useBackup, placement) : null;

				if (group != null && bnGroupCache != null)
				{
					bnGroupCache[placement] = group;
					NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "CacheStore", () => $"placement={placement}");
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"group={(group != null)}");
				return group;
			}
		}

		private BannerSlotUnitConfig GetBannerSlotConfig(BannerPlacement placement)
		{
			var unit = configs?.BannerUnit;
			if (unit == null)
				return null;

			return placement switch
			{
				BannerPlacement.FullTop => unit.FullTop,
				BannerPlacement.TopLeft => unit.TopLeft,
				BannerPlacement.TopRight => unit.TopRight,
				BannerPlacement.BottomLeft => unit.BottomLeft,
				BannerPlacement.BottomRight => unit.BottomRight,
				_ => null
			};
		}

		private BannerFullBottomSlotUnitConfig GetBannerFullBottomSlotConfig()
		{
			return configs?.BannerUnit?.FullBottom;
		}

		private bool GetBannerUseBackup(BannerPlacement placement)
		{
			if (placement == BannerPlacement.FullBottom)
			{
				var slot = GetBannerFullBottomSlotConfig();
				if (slot == null)
					return false;

				return slot.UseBackup;
			}

			return GetBannerSlotConfig(placement)?.UseBackup ?? false;
		}

		private string GetBannerPriorityLabel(BannerPlacement placement)
		{
			if (placement == BannerPlacement.FullBottom)
				return (GetBannerFullBottomSlotConfig()?.MediationPriority ?? E_MediationPriority.Admob).ToString();

			return (GetBannerSlotConfig(placement)?.MediationPriority ?? E_AdmobMaxMediationPriority.Admob).ToString();
		}

		private List<FallbackCandidate<IRectGroup>> BuildBannerFallbackCandidates(BannerPlacement placement)
		{
			var result = new List<FallbackCandidate<IRectGroup>>(3);

			if (placement == BannerPlacement.FullBottom)
			{
				var slot = GetBannerFullBottomSlotConfig();
				var useBackup = GetBannerUseBackup(placement);
				var orderedPriorities = BuildBannerPriorityOrder(
					slot?.MediationPriority ?? E_MediationPriority.Admob,
					useBackup,
					placement);

				for (int i = 0; i < orderedPriorities.Count; i++)
				{
					switch (orderedPriorities[i])
					{
						case E_MediationPriority.Admob:
							if (TryGetAdmob("BuildBannerFallbackCandidates", out var admob))
							{
								var group = admob.GetBNGroup(placement);
								if (!string.IsNullOrEmpty(group?.Id))
									result.Add(new FallbackCandidate<IRectGroup>("Admob", BG_ConstValue.mediation_admob, group));
							}
							break;

						case E_MediationPriority.Android:
							if (TryGetAndroid("BuildBannerFallbackCandidates", out var android))
							{
								var group = android.BN_Group;
								if (!string.IsNullOrEmpty(group?.Id))
									result.Add(new FallbackCandidate<IRectGroup>("Android", BG_ConstValue.mediation_android, group));
							}
							break;

						case E_MediationPriority.Max:
							if (TryGetMax("BuildBannerFallbackCandidates", out var max))
							{
								var group = max.GetBNGroup(placement);
								if (!string.IsNullOrEmpty(group?.Id))
									result.Add(new FallbackCandidate<IRectGroup>("Max", BG_ConstValue.mediation_max, group));
							}
							break;
					}
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "BN BuildCandidates",
					() => $"placement={placement} priority={slot?.MediationPriority} useBackup={useBackup} count={result.Count}");
				return result;
			}

			var regularSlot = GetBannerSlotConfig(placement);
			var orderedRegularPriorities = BuildBannerPriorityOrder(
				regularSlot?.MediationPriority ?? E_AdmobMaxMediationPriority.Admob,
				regularSlot?.UseBackup ?? false);

			for (int i = 0; i < orderedRegularPriorities.Count; i++)
			{
				switch (orderedRegularPriorities[i])
				{
					case E_AdmobMaxMediationPriority.Admob:
						if (TryGetAdmob("BuildBannerFallbackCandidates", out var admob))
						{
							var group = admob.GetBNGroup(placement);
							if (!string.IsNullOrEmpty(group?.Id))
								result.Add(new FallbackCandidate<IRectGroup>("Admob", BG_ConstValue.mediation_admob, group));
						}
						break;

					case E_AdmobMaxMediationPriority.Max:
						if (TryGetMax("BuildBannerFallbackCandidates", out var max))
						{
							var group = max.GetBNGroup(placement);
							if (!string.IsNullOrEmpty(group?.Id))
								result.Add(new FallbackCandidate<IRectGroup>("Max", BG_ConstValue.mediation_max, group));
						}
						break;
				}
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "BN BuildCandidates",
				() => $"placement={placement} priority={regularSlot?.MediationPriority} useBackup={regularSlot?.UseBackup} count={result.Count}");
			return result;
		}

		private static List<E_AdmobMaxMediationPriority> BuildBannerPriorityOrder(E_AdmobMaxMediationPriority priority, bool useBackup)
		{
			var ordered = new List<E_AdmobMaxMediationPriority>(2) { priority };
			if (!useBackup)
				return ordered;

			var canonical = new[] { E_AdmobMaxMediationPriority.Admob, E_AdmobMaxMediationPriority.Max };

			for (int i = 0; i < canonical.Length; i++)
			{
				if (!ordered.Contains(canonical[i]))
					ordered.Add(canonical[i]);
			}

			return ordered;
		}

		private static List<E_MediationPriority> BuildBannerPriorityOrder(E_MediationPriority priority, bool useBackup, BannerPlacement placement)
		{
			var ordered = new List<E_MediationPriority>(3) { priority };
			if (!useBackup)
				return ordered;

			var canonical = placement == BannerPlacement.FullBottom
				? new[] { E_MediationPriority.Admob, E_MediationPriority.Max, E_MediationPriority.Android }
				: new[] { E_MediationPriority.Admob, E_MediationPriority.Max };

			for (int i = 0; i < canonical.Length; i++)
			{
				if (!ordered.Contains(canonical[i]))
					ordered.Add(canonical[i]);
			}

			return ordered;
		}

		#endregion

		#region MREC

		private void Mrec_InitGroup()
		{
			if (mrecGroup != null) return;

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "Mrec_InitGroup", () => $"med={configs?.MrecUnit?.MediationPriority}"))
			{
				var candidates = BuildMrecFallbackCandidates();
				mrecGroup = candidates.Count > 0 ? new MrecFallbackGroup(candidates, configs.MrecUnit.UseBackup) : null;

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"mrecGroup={(mrecGroup != null)}");
			}
		}

		protected override IRectGroup Mrec_GetGroup()
		{
			Mrec_InitGroup();
			return mrecGroup;
		}

		private List<FallbackCandidate<IRectGroup>> BuildMrecFallbackCandidates()
		{
			var result = new List<FallbackCandidate<IRectGroup>>(2);
			var mrecConfig = configs?.MrecUnit;
			var orderedPriorities = BuildMrecPriorityOrder(
				mrecConfig?.MediationPriority ?? E_AdmobMaxMediationPriority.Admob,
				mrecConfig?.UseBackup ?? false);

			for (int i = 0; i < orderedPriorities.Count; i++)
			{
				var priority = orderedPriorities[i];
				switch (priority)
				{
					case E_AdmobMaxMediationPriority.Admob:
						if (TryGetAdmob("BuildMrecFallbackCandidates", out var admob) && !string.IsNullOrEmpty(admob.Mrec_Group?.Id))
							result.Add(new FallbackCandidate<IRectGroup>("Admob", BG_ConstValue.mediation_admob, admob.Mrec_Group));
						break;

					case E_AdmobMaxMediationPriority.Max:
						if (TryGetMax("BuildMrecFallbackCandidates", out var max) && !string.IsNullOrEmpty(max.Mrec_Group?.Id))
							result.Add(new FallbackCandidate<IRectGroup>("Max", BG_ConstValue.mediation_max, max.Mrec_Group));
						break;
				}
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "MREC BuildCandidates",
				() => $"priority={configs?.MrecUnit?.MediationPriority} useBackup={configs?.MrecUnit?.UseBackup} count={result.Count}");
			return result;
		}

		private static List<E_AdmobMaxMediationPriority> BuildMrecPriorityOrder(E_AdmobMaxMediationPriority priority, bool useBackup)
		{
			var ordered = new List<E_AdmobMaxMediationPriority>(2) { priority };
			if (!useBackup)
				return ordered;

			var canonical = new[] { E_AdmobMaxMediationPriority.Admob, E_AdmobMaxMediationPriority.Max };

			for (int i = 0; i < canonical.Length; i++)
			{
				if (!ordered.Contains(canonical[i]))
					ordered.Add(canonical[i]);
			}

			return ordered;
		}

		#endregion

		#region Popup

		protected override IRectGroup PU_GetGroup(string groupName)
		{
			if (string.IsNullOrEmpty(groupName))
				{
					return null;
				}

				if (puGroupCache != null && puGroupCache.TryGetValue(groupName, out var cached) && cached != null)
				{
					return cached;
				}

				if (!TryGetAndroid("PU_GetGroup", out var android)) return null;

				var group = android.PU_GetGroup(groupName);
				if (group != null && puGroupCache != null)
				{
					puGroupCache[groupName] = group;
				}

				return group;
		}

		public override string PU_GroupByPos(string pos)
		{
			if (string.IsNullOrEmpty(pos))
				{
					NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Fail", () => "Pos NullOrEmpty");
					return null;
				}

				var g = configs.GetPUGroupByPos(pos);
				return g;
		}

		public override string[] PU_GetListGroup()
		{
			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "PU_GetListGroup", () => ""))
			{
				var groupNames = new string[configs.PopupGroups.Length];
				for (int i = 0; i < groupNames.Length; i++)
				{
					groupNames[i] = configs.PopupGroups[i].GroupName;
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"count={(groupNames?.Length ?? 0)}");
				return groupNames;
			}
		}

		public override string[] PU_GetAutoInitGroupNames(AdSystemConfigs.PopupChannelConfig channelConfig)
		{
			if (channelConfig?.PositionConfigs == null || channelConfig.PositionConfigs.Length == 0 || configs == null)
				return Array.Empty<string>();

			var result = new List<string>();
			var seen = new HashSet<string>(StringComparer.Ordinal);

			for (int i = 0; i < channelConfig.PositionConfigs.Length; i++)
			{
				var posConfig = channelConfig.PositionConfigs[i];
				if (posConfig == null || !posConfig.AutoInit || !posConfig.IsEnabled || string.IsNullOrEmpty(posConfig.PositionName))
					continue;

				var groupName = configs.GetPUGroupByPos(posConfig.PositionName);
				if (string.IsNullOrEmpty(groupName) || !seen.Add(groupName))
					continue;

				result.Add(groupName);
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "PU AutoInitGroups",
				() => $"count={result.Count}");
			return result.ToArray();
		}

		#endregion

		#region Collap

		private IRectGroup clGroup;

		private void CL_InitGroup()
		{
			if (clGroup != null) return;

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "CL_InitGroup", () => "Android only"))
			{
				if (!TryGetAndroid("CL_InitGroup", out var android)) return;
				clGroup = android.CL_Group;

				NetFlowDebugSystem.Log(Layer.adcore, Module.adcore, "Result", () => $"clGroup={(clGroup != null)}");
			}
		}

		protected override IRectGroup CL_GetGroup()
		{
			CL_InitGroup();
			return clGroup;
		}

		#endregion

		#region (3) ===== HELPERS =====

		private bool TryGetAdmob(string context, out Admob_MediationManager admob)
		{
			admob = mediation1;
			if (admob != null) return true;

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "ResolveAdmobFail", () => $"core={AdCoreName} ctx={context}"))
			{
				NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Result", () => "Admob mediation null");
			}

			return false;
		}

		private bool TryGetAndroid(string context, out Android_MediationManager android)
		{
			android = mediation2;
			if (android != null) return true;

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "ResolveAndroidFail", () => $"core={AdCoreName} ctx={context}"))
			{
				NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Result", () => "Android mediation null");
			}

			return false;
		}

        private bool TryGetMax(string context, out Max_MediationManager max)
        {
            max = mediation3;
            if (max != null) return true;

            using (NetFlowDebugSystem.Flow(Layer.adcore, Module.adcore, "ResolveMaxFail", () => $"core={AdCoreName} ctx={context}"))
            {
                NetFlowDebugSystem.Error(Layer.adcore, Module.adcore, "Result", () => "Max mediation null");
            }

            return false;
        }

        private bool HasAnyMaxMediationConfigured()
        {
            if (configs == null)
                return false;

            if (configs.ForceAdGroups != null)
            {
                for (int i = 0; i < configs.ForceAdGroups.Length; i++)
                {
                    var group = configs.ForceAdGroups[i];
                    if (group == null)
                        continue;

                    bool forceAdUsesMax =
                        group.MediationPriority == E_MediationPriority.Max ||
                        (group.UseBackup && !string.IsNullOrEmpty(configs.ForceAdMaxUnit?.MaxUnit));

                    if (forceAdUsesMax && !string.IsNullOrEmpty(configs.ForceAdMaxUnit?.MaxUnit))
                        return true;
                }
            }

            if (configs.RewardedUnit != null)
            {
                bool rewardedUsesMax =
                    configs.RewardedUnit.MediationPriority == E_MediationPriority.Max ||
                    (configs.RewardedUnit.UseBackup && !string.IsNullOrEmpty(configs.RewardedUnit.MaxUnit));

                if (rewardedUsesMax && !string.IsNullOrEmpty(configs.RewardedUnit.MaxUnit))
                    return true;
            }

            if (configs.AppOpenUnit != null)
            {
                bool appOpenUsesMax =
                    configs.AppOpenUnit.MediationPriority == E_AdmobMaxMediationPriority.Max ||
                    (configs.AppOpenUnit.UseBackup && !string.IsNullOrEmpty(configs.AppOpenUnit.MaxUnit));

                if (appOpenUsesMax && !string.IsNullOrEmpty(configs.AppOpenUnit.MaxUnit))
                    return true;
            }

            if (configs.MrecUnit != null)
            {
                bool mrecUsesMax =
                    configs.MrecUnit.MediationPriority == E_AdmobMaxMediationPriority.Max ||
                    (configs.MrecUnit.UseBackup && !string.IsNullOrEmpty(configs.MrecUnit.MaxUnit));

                if (mrecUsesMax && !string.IsNullOrEmpty(configs.MrecUnit.MaxUnit))
                    return true;
            }

			if (configs.BannerUnit != null)
			{
				if (BannerSlotUsesMax(configs.BannerUnit.FullBottom))
					return true;
				if (BannerSlotUsesMax(configs.BannerUnit.FullTop))
					return true;
				if (BannerSlotUsesMax(configs.BannerUnit.TopLeft))
					return true;
				if (BannerSlotUsesMax(configs.BannerUnit.TopRight))
					return true;
				if (BannerSlotUsesMax(configs.BannerUnit.BottomLeft))
					return true;
				if (BannerSlotUsesMax(configs.BannerUnit.BottomRight))
					return true;
			}

			return false;
		}

		private static bool BannerSlotUsesMax(BannerSlotUnitConfig slot)
		{
			if (slot == null)
				return false;

			bool bannerUsesMax =
				slot.MediationPriority == E_AdmobMaxMediationPriority.Max ||
				(slot.UseBackup && !string.IsNullOrEmpty(slot.MaxUnit));

			return bannerUsesMax && !string.IsNullOrEmpty(slot.MaxUnit);
		}

		private static bool BannerSlotUsesMax(BannerFullBottomSlotUnitConfig slot)
		{
			if (slot == null)
				return false;

			bool bannerUsesMax =
				slot.MediationPriority == E_MediationPriority.Max ||
				(slot.UseBackup && !string.IsNullOrEmpty(slot.MaxUnit));

			return bannerUsesMax && !string.IsNullOrEmpty(slot.MaxUnit);
		}

		#endregion
	}
}
