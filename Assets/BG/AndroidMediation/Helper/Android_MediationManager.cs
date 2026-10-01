using BG_Library.Common;
using BG_Library.NET.Debug;
using System;

namespace BG_Library.NET.Mediation.Android
{
	[System.Serializable]
	public class Android_MediationManager
	{
		private const string TITLE_RW = "Android_RWGroup";
		private const string TITLE_FA = "Android_FAGroup";
		private const string TITLE_BN = "Android_BNGroup";
		private const string TITLE_PU = "Android_PUGroup";
		private const string TITLE_CL = "Android_CLGroup";

		public const string GroupName_Rw = "android_rewarded";
		public const string GroupName_Bn = "android_banner";
		public const string GroupName_Cl = "android_collap";

		public Android_FSGroupController<Android_RWInfo> RW_Group { get; private set; }
		public Android_FSGroupController<Android_FAInfo>[] FA_Groups { get; private set; }

		public Android_RectGroupController<Android_BNInfo> BN_Group { get; private set; }
		public Android_RectGroupController<Android_PUInfo>[] PU_Group { get; private set; }
		public Android_RectGroupController<Android_CLInfo> CL_Group { get; private set; }

		public Android_MediationManager(IAndroid_Configs configs)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, "Constructor", () => "create groups");

			// ================= RW =================
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_RW,
				() => $"create groupName={GroupName_Rw} mediation={BG_ConstValue.mediation_android} format={BG_ConstValue.adtype_rw}");

			RW_Group = new(
				info: configs.GetAndroidRWInfo(),
				adType: BG_ConstValue.adtype_rw,
				groupName: GroupName_Rw,
				mediation: BG_ConstValue.mediation_android,
				budget: 0);

			// ================= FA =================
			FA_Init(configs);

			// ================= BN =================
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_BN,
				() => $"create groupName={GroupName_Bn} mediation={BG_ConstValue.mediation_android} format={BG_ConstValue.adtype_bn}");

			BN_Group = new(
				info: configs.GetAndroidBNInfo(),
				adType: BG_ConstValue.adtype_bn,
				groupName: GroupName_Bn,
				mediation: BG_ConstValue.mediation_android);

			// ================= CL =================
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_CL,
				() => $"create groupName={GroupName_Cl} mediation={BG_ConstValue.mediation_android} format={BG_ConstValue.adtype_cl}");

			CL_Group = new(
				info: configs.GetAndroidCLInfo(),
				adType: BG_ConstValue.adtype_cl,
				groupName: GroupName_Cl,
				mediation: BG_ConstValue.mediation_android
			);

			// ================= PU =================
			PU_Init(configs);
		}

		void FA_Init(IAndroid_Configs configs)
		{
			var fa_Infos = configs.GetAndroidFAInfo();
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_FA,
				() => "init FA groups");

			if (fa_Infos == null || fa_Infos.Length == 0)
			{
				FA_Groups = Array.Empty<Android_FSGroupController<Android_FAInfo>>();
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_android, TITLE_FA,
					() => "infos empty -> FA_Groups=Empty");

				return;
			}

			FA_Groups = new Android_FSGroupController<Android_FAInfo>[fa_Infos.Length];

			for (int i = 0; i < FA_Groups.Length; i++)
			{
				var info = fa_Infos[i];

				NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_FA,
					() => $"create[{i}] groupName={info.GroupName} budget={info.MaxShowCount}",
					details: d =>
					{
						d.AddKV("group", info.GroupName);
						d.AddKV("format", BG_ConstValue.adtype_fa);
						d.AddKV("mediation", BG_ConstValue.mediation_android);
						d.AddKV("id", info.Id);
					});

				FA_Groups[i] = new(
					info: info,
					adType: BG_ConstValue.adtype_fa,
					groupName: info.GroupName,
					mediation: BG_ConstValue.mediation_android,
					budget: info.MaxShowCount);
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_FA,
				() => $"done count={FA_Groups.Length}");
		}

		public IFSGroup FA_GetGroup(string groupName)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, "FA_GetGroup",
				() => $"request groupName={groupName}");

			if (FA_Groups == null || FA_Groups.Length == 0)
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_android, "FA_GetGroup",
					() => "FA_Groups empty -> return null");
				return null;
			}

			if (string.IsNullOrEmpty(groupName))
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_android, "FA_GetGroup",
					() => "groupName null/empty -> return null");
				return null;
			}

			for (int i = 0; i < FA_Groups.Length; i++)
			{
				var c = FA_Groups[i];
				if (c == null) continue;

				if (string.Equals(c.GroupName, groupName, StringComparison.Ordinal))
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, "FA_GetGroup",
						() => $"hit index={i} groupName={groupName}");
					return c;
				}
			}

			NetFlowDebugSystem.Warn(Layer.adcore, Module.med_android, "FA_GetGroup",
				() => $"miss groupName={groupName} -> return null");

			return null;
		}

		void PU_Init(IAndroid_Configs configs)
		{
			var pu_Infos = configs.GetAndroidPUInfo();
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_PU,
				() => "init PU groups");

			if (pu_Infos == null || pu_Infos.Length == 0)
			{
				PU_Group = Array.Empty<Android_RectGroupController<Android_PUInfo>>();
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_android, TITLE_PU,
					() => "infos empty -> PU_Group=Empty");

				return;
			}

			PU_Group = new Android_RectGroupController<Android_PUInfo>[pu_Infos.Length];

			for (int i = 0; i < PU_Group.Length; i++)
			{
				var info = pu_Infos[i];

				NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_PU,
					() => $"create[{i}] groupName={info.GroupName}",
					details: d =>
					{
						d.AddKV("format", BG_ConstValue.adtype_pu);
						d.AddKV("mediation", BG_ConstValue.mediation_android);
					});

				PU_Group[i] = new(
					info: info,
					adType: BG_ConstValue.adtype_pu,
					groupName: info.GroupName,
					mediation: BG_ConstValue.mediation_android);
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, TITLE_PU,
				() => $"done count={PU_Group.Length}");
		}

		public IRectGroup PU_GetGroup(string groupName)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, "PU_GetGroup",
				() => $"request groupName={groupName}");

			if (PU_Group == null || PU_Group.Length == 0)
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_android, "PU_GetGroup",
					() => "PU_Group empty -> return null");
				return null;
			}

			if (string.IsNullOrEmpty(groupName))
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_android, "PU_GetGroup",
					() => "groupName null/empty -> return null");
				return null;
			}

			for (int i = 0; i < PU_Group.Length; i++)
			{
				var c = PU_Group[i];
				if (c == null) continue;

				if (string.Equals(c.GroupName, groupName, StringComparison.Ordinal))
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.med_android, "PU_GetGroup",
						() => $"hit index={i} groupName={groupName}");
					return c;
				}
			}

			NetFlowDebugSystem.Warn(Layer.adcore, Module.med_android, "PU_GetGroup",
				() => $"miss groupName={groupName} -> return null");

			return null;
		}
	}
}
