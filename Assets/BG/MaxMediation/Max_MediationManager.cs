using System;
using BG_Library.Common;
using BG_Library.NET.Debug;

namespace BG_Library.NET.Mediation.Max
{
	public class Max_MediationManager
	{
		// Titles should be GROUP names (not format names) - per your rule
		private const string TITLE_AO = "Max_AOGroup";
		private const string TITLE_FA = "Max_FAGroup";
		private const string TITLE_RW = "Max_RWGroup";
		private const string TITLE_BN = "Max_BNGroup";
		private const string TITLE_MREC = "Max_MrecGroup";
		private const string TITLE_INIT = "InitSDK";

		public const string GroupName_Ao = "max_appopen";
		public const string GroupName_Fa = "max_interstitial";
		public const string GroupName_Rw = "max_rewarded";
		public const string GroupName_Bn = "max_banner";
		public const string GroupName_Mrec = "max_mrec";

		public static bool IsCallInit { get; private set; }
		public static bool IsInitComplete { get; private set; }

		public Max_FSGroupController<Max_AOInfo, Max_AOAccessAPI> AO_Group { get; private set; }
		public Max_FSGroupController<Max_FAInfo, Max_FAAccessAPI> FA_Group { get; private set; }
		public Max_FSGroupController<Max_RWInfo, Max_RWAccessAPI> RW_Group { get; private set; }

		public Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> BN_FullBottomGroup { get; private set; }
		public Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> BN_FullTopGroup { get; private set; }
		public Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> BN_TopLeftGroup { get; private set; }
		public Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> BN_TopRightGroup { get; private set; }
		public Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> BN_BottomLeftGroup { get; private set; }
		public Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> BN_BottomRightGroup { get; private set; }
		public Max_RectGroupController<Max_MrecInfo, Max_MrecAccessAPI> Mrec_Group { get; private set; }

		public Max_MediationManager(IMax_Configs configs)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, "Constructor", () => "create groups",
				details: d =>
				{
				d.AddKV("aoGroup", GroupName_Ao);
				d.AddKV("faGroup", GroupName_Fa);
				d.AddKV("rwGroup", GroupName_Rw);
				d.AddKV("bnGroup", "max_banner_*");
				d.AddKV("mrecGroup", GroupName_Mrec);
				});

			// AO
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, TITLE_AO, () =>
				$"create groupName={GroupName_Ao} mediation={BG_ConstValue.mediation_max} format={BG_ConstValue.adtype_ao}");
			AO_Group = new(
				info: configs.GetMaxAOInfo(),
				format: BG_ConstValue.adtype_ao,
				groupName: GroupName_Ao,
				mediation: BG_ConstValue.mediation_max);

			// FA
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, TITLE_FA, () =>
				$"create groupName={GroupName_Fa} mediation={BG_ConstValue.mediation_max} format={BG_ConstValue.adtype_fa}");
			FA_Group = new(
				info: configs.GetMaxFAInfo(),
				format: BG_ConstValue.adtype_fa,
				groupName: GroupName_Fa,
				mediation: BG_ConstValue.mediation_max);

			// RW
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, TITLE_RW, () =>
				$"create groupName={GroupName_Rw} mediation={BG_ConstValue.mediation_max} format={BG_ConstValue.adtype_rw}");
			RW_Group = new(
				info: configs.GetMaxRWInfo(),
				format: BG_ConstValue.adtype_rw,
				groupName: GroupName_Rw,
				mediation: BG_ConstValue.mediation_max);

			// BN
			BN_Init(configs);

			// MREC
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, TITLE_MREC, () =>
				$"create groupName={GroupName_Mrec} mediation={BG_ConstValue.mediation_max} format={BG_ConstValue.adtype_mrec}");
			Mrec_Group = new(
				info: configs.GetMaxMrecId(),
				format: BG_ConstValue.adtype_mrec,
				groupName: GroupName_Mrec,
				mediation: BG_ConstValue.mediation_max);
		}

		public static void InitMediation(Action onComplete = null)
		{
			if (IsInitComplete || IsCallInit)
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, TITLE_INIT, () => $"skip (IsInitComplete={IsInitComplete} IsCallInit={IsCallInit})");
				return;
			}

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.med_max, TITLE_INIT, () => "InitMediation"))
			{
				IsCallInit = true;

				// NOTE: This callback can happen later => may appear as a different flow in logs (expected).
				MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
				{
					IsInitComplete = true;

					NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, TITLE_INIT, () => "complete (SDK initialized)");

					onComplete?.Invoke();
				};

				NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, TITLE_INIT, () => "MaxSdk.InitializeSdk()");

				MaxSdk.InitializeSdk();
			}
		}

		void BN_Init(IMax_Configs configs)
		{
			var infos = configs.GetMaxBNInfos();

			BN_FullBottomGroup = CreateBannerGroup(infos, MaxRectPlacement.FullBottom);
			BN_FullTopGroup = CreateBannerGroup(infos, MaxRectPlacement.FullTop);
			BN_TopLeftGroup = CreateBannerGroup(infos, MaxRectPlacement.TopLeft);
			BN_TopRightGroup = CreateBannerGroup(infos, MaxRectPlacement.TopRight);
			BN_BottomLeftGroup = CreateBannerGroup(infos, MaxRectPlacement.BottomLeft);
			BN_BottomRightGroup = CreateBannerGroup(infos, MaxRectPlacement.BottomRight);
		}

		public Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> GetBNGroup(BannerPlacement placement)
		{
			return GetBNGroup(ToMaxRectPlacement(placement));
		}

		private Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> GetBNGroup(MaxRectPlacement placement)
		{
			return placement switch
			{
				MaxRectPlacement.FullBottom => BN_FullBottomGroup,
				MaxRectPlacement.FullTop => BN_FullTopGroup,
				MaxRectPlacement.TopLeft => BN_TopLeftGroup,
				MaxRectPlacement.TopRight => BN_TopRightGroup,
				MaxRectPlacement.BottomLeft => BN_BottomLeftGroup,
				MaxRectPlacement.BottomRight => BN_BottomRightGroup,
				_ => null
			};
		}

		private Max_RectGroupController<Max_BNInfo, Max_BNAccessAPI> CreateBannerGroup(Max_BNInfo[] infos, MaxRectPlacement placement)
		{
			var info = FindBannerInfo(infos, placement) ?? new Max_BNInfo("", placement);
			var groupName = GetBannerGroupName(placement);

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_max, TITLE_BN, () =>
				$"create groupName={groupName} placement={placement} mediation={BG_ConstValue.mediation_max} format={BG_ConstValue.adtype_bn}");

			return new(
				info: info,
				format: BG_ConstValue.adtype_bn,
				groupName: groupName,
				mediation: BG_ConstValue.mediation_max);
		}

		private static Max_BNInfo FindBannerInfo(Max_BNInfo[] infos, MaxRectPlacement placement)
		{
			if (infos == null || infos.Length == 0)
				return null;

			for (int i = 0; i < infos.Length; i++)
			{
				var info = infos[i];
				if (info != null && info.Placement == placement)
					return info;
			}

			return null;
		}

		private static string GetBannerGroupName(MaxRectPlacement placement)
		{
			return placement switch
			{
				MaxRectPlacement.FullBottom => "max_banner_full_bottom",
				MaxRectPlacement.FullTop => "max_banner_full_top",
				MaxRectPlacement.TopLeft => "max_banner_top_left",
				MaxRectPlacement.TopRight => "max_banner_top_right",
				MaxRectPlacement.BottomLeft => "max_banner_bottom_left",
				MaxRectPlacement.BottomRight => "max_banner_bottom_right",
				_ => "max_banner_full_bottom"
			};
		}

		private static MaxRectPlacement ToMaxRectPlacement(BannerPlacement placement)
		{
			return placement switch
			{
				BannerPlacement.FullBottom => MaxRectPlacement.FullBottom,
				BannerPlacement.FullTop => MaxRectPlacement.FullTop,
				BannerPlacement.TopLeft => MaxRectPlacement.TopLeft,
				BannerPlacement.TopRight => MaxRectPlacement.TopRight,
				BannerPlacement.BottomLeft => MaxRectPlacement.BottomLeft,
				BannerPlacement.BottomRight => MaxRectPlacement.BottomRight,
				_ => MaxRectPlacement.FullBottom
			};
		}
	}
}
