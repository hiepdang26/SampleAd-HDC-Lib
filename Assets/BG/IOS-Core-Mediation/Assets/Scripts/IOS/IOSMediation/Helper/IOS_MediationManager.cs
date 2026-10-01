using System;
using BG_Library.Common;
using BG_Library.NET;
using BG_Library.NET.Debug;

namespace BG_Library.NET.Mediation.IOS
{
	[Serializable]
	public class IOS_MediationManager
	{
		private const string TITLE_RW = "IOS_RWGroup";
		private const string TITLE_FA = "IOS_FAGroup";
		private const string TITLE_BN = "IOS_BNGroup";
		private const string TITLE_PU = "IOS_PUGroup";

		public const string GroupName_Rw = "ios_rewarded";
		public const string GroupName_Bn = "ios_banner";

		public IOS_FSGroupController<IOS_RWInfo> RW_Group { get; private set; }
		public IOS_FSGroupController<IOS_FAInfo>[] FA_Groups { get; private set; }
		public IOS_RectGroupController<IOS_BNInfo> BN_Group { get; private set; }
		public IOS_RectGroupController<IOS_PUInfo>[] PU_Group { get; private set; }

		public IOS_MediationManager(IIOS_Configs configs)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, "Constructor", () => "create groups");

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, TITLE_RW,
				() => $"create groupName={GroupName_Rw} mediation={BG_ConstValue.mediation_ios} format={BG_ConstValue.adtype_rw}");

			RW_Group = new(
				info: configs.GetIOSRWInfo(),
				adType: BG_ConstValue.adtype_rw,
				groupName: GroupName_Rw,
				mediation: BG_ConstValue.mediation_ios,
				budget: 0);

			FA_Init(configs);

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, TITLE_BN,
				() => $"create groupName={GroupName_Bn} mediation={BG_ConstValue.mediation_ios} format={BG_ConstValue.adtype_bn}");

			BN_Group = new(
				info: configs.GetIOSBNInfo(),
				adType: BG_ConstValue.adtype_bn,
				groupName: GroupName_Bn,
				mediation: BG_ConstValue.mediation_ios);

			PU_Init(configs);
		}

		private void FA_Init(IIOS_Configs configs)
		{
			var faInfos = configs.GetIOSFAInfo();
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, TITLE_FA, () => "init FA groups");

			if (faInfos == null || faInfos.Length == 0)
			{
				FA_Groups = Array.Empty<IOS_FSGroupController<IOS_FAInfo>>();
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_ios, TITLE_FA, () => "infos empty -> FA_Groups=Empty");
				return;
			}

			FA_Groups = new IOS_FSGroupController<IOS_FAInfo>[faInfos.Length];

			for (int i = 0; i < FA_Groups.Length; i++)
			{
				var info = faInfos[i];

				NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, TITLE_FA,
					() => $"create[{i}] groupName={info.GroupName} budget={info.MaxShowCount}",
					details: d =>
					{
						d.AddKV("group", info.GroupName);
						d.AddKV("format", BG_ConstValue.adtype_fa);
						d.AddKV("mediation", BG_ConstValue.mediation_ios);
						d.AddKV("id", info.Id);
						d.AddKV("layoutGroup", info.LayoutGroup?.GroupName ?? "");
						d.AddKV("mediationPriority", $"{info.MediationPriority}({(int)info.MediationPriority})");
						d.AddKV("switchToInterstitialAndroid", info.AndroidInterstitials.SwitchToInterstitialAndroid);
					});

				FA_Groups[i] = new(
					info: info,
					adType: BG_ConstValue.adtype_fa,
					groupName: info.GroupName,
					mediation: BG_ConstValue.mediation_ios,
					budget: info.MaxShowCount);
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, TITLE_FA, () => $"done count={FA_Groups.Length}");
		}

		public IFSGroup FA_GetGroup(string groupName)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, "FA_GetGroup", () => $"request groupName={groupName}");

			if (FA_Groups == null || FA_Groups.Length == 0)
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_ios, "FA_GetGroup", () => "FA_Groups empty -> return null");
				return null;
			}

			if (string.IsNullOrEmpty(groupName))
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_ios, "FA_GetGroup", () => "groupName null/empty -> return null");
				return null;
			}

			for (int i = 0; i < FA_Groups.Length; i++)
			{
				var c = FA_Groups[i];
				if (c == null) continue;

				if (string.Equals(c.GroupName, groupName, StringComparison.Ordinal))
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, "FA_GetGroup", () => $"hit index={i} groupName={groupName}");
					return c;
				}
			}

			NetFlowDebugSystem.Warn(Layer.adcore, Module.med_ios, "FA_GetGroup", () => $"miss groupName={groupName} -> return null");
			return null;
		}

		private void PU_Init(IIOS_Configs configs)
		{
			var puInfos = configs.GetIOSPUInfo();
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, TITLE_PU, () => "init PU groups");

			if (puInfos == null || puInfos.Length == 0)
			{
				PU_Group = Array.Empty<IOS_RectGroupController<IOS_PUInfo>>();
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_ios, TITLE_PU, () => "infos empty -> PU_Group=Empty");
				return;
			}

			PU_Group = new IOS_RectGroupController<IOS_PUInfo>[puInfos.Length];

			for (int i = 0; i < PU_Group.Length; i++)
			{
				var info = puInfos[i];

				NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, TITLE_PU,
					() => $"create[{i}] groupName={info.GroupName}",
					details: d =>
					{
						d.AddKV("format", BG_ConstValue.adtype_pu);
						d.AddKV("mediation", BG_ConstValue.mediation_ios);
						d.AddKV("id", info.Id);
					});

				PU_Group[i] = new(
					info: info,
					adType: BG_ConstValue.adtype_pu,
					groupName: info.GroupName,
					mediation: BG_ConstValue.mediation_ios);
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, TITLE_PU, () => $"done count={PU_Group.Length}");
		}

		public IRectGroup PU_GetGroup(string groupName)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, "PU_GetGroup", () => $"request groupName={groupName}");

			if (PU_Group == null || PU_Group.Length == 0)
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_ios, "PU_GetGroup", () => "PU_Group empty -> return null");
				return null;
			}

			if (string.IsNullOrEmpty(groupName))
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_ios, "PU_GetGroup", () => "groupName null/empty -> return null");
				return null;
			}

			for (int i = 0; i < PU_Group.Length; i++)
			{
				var c = PU_Group[i];
				if (c == null) continue;

				if (string.Equals(c.GroupName, groupName, StringComparison.Ordinal))
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.med_ios, "PU_GetGroup", () => $"hit index={i} groupName={groupName}");
					return c;
				}
			}

			NetFlowDebugSystem.Warn(Layer.adcore, Module.med_ios, "PU_GetGroup", () => $"miss groupName={groupName} -> return null");
			return null;
		}
	}
}
