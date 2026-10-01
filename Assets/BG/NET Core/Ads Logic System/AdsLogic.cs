using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using Sirenix.OdinInspector;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BG_Library.NET.AdSystem
{
	public class AdsLogic : MonoBehaviour
	{
		public static AdsLogic Ins { get; private set; }

		public static string ScreenName
		{
			get
			{
				string st = SceneManager.GetActiveScene().name;

				string normalized = st.Normalize(NormalizationForm.FormD);
				StringBuilder sb = new StringBuilder();

				foreach (char c in normalized)
				{
					if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
						sb.Append(c);
				}

				string noAccent = sb.ToString().Normalize(NormalizationForm.FormC);
				noAccent = noAccent.ToUpperInvariant();
				noAccent = Regex.Replace(noAccent, @"[^A-Z0-9]", "");

				if (noAccent.Length > 15)
					noAccent = noAccent.Substring(0, 15);

				return noAccent;
			}
		}

		public static bool IsAlwaysTrackingRev { get; private set; }
		public static bool IsTrackingBgAdImpression { get; private set; }

		public static float LastTimeFSAd { get; private set; }
		public static bool IsRemovedAd { get; private set; }
		public static bool IsCallInitMediation { get; private set; }

		[BoxGroup("AD MANAGER")] public AppLaunchSystem AL_ManagerIns;
		[BoxGroup("AD MANAGER")] public AppResumeSystem AR_ManagerIns;

		[BoxGroup("AD MANAGER")] public ForceAdSystem FA_ManagerIns;
		[BoxGroup("AD MANAGER")] public RewardedSystem RW_ManagerIns;

		[BoxGroup("AD MANAGER")] public BannerSystem BN_ManagerIns;
		[BoxGroup("AD MANAGER")] public MrecSystem Mrec_ManagerIns;

		[BoxGroup("AD MANAGER")] public PopUpSystem PU_ManagerIns;
		[BoxGroup("AD MANAGER")] public CollapSystem CL_ManagerIns;

		[BoxGroup("Assets"), SerializeField] AdCoreBase adsCoreDefault;

		public static AdSystemConfigs AdsConfigIns { get; private set; }
		public static AdCoreBase AdsCoreIns { get; private set; }

		public string AdCoreName => AdsConfigIns != null ? (AdsConfigIns.SelectedAdCoreName ?? "") : "";

		public static bool ShouldTrackChannel(Channel? channel)
		{
			var config = GetChannelConfig(channel);
			return config == null || config.TrackChannel;
		}

		private static AdSystemConfigs.BaseChannelConfig GetChannelConfig(Channel? channel)
		{
			if (!channel.HasValue || AdsConfigIns == null)
				return null;

			return channel.Value switch
			{
				Channel.AppLaunch => AdsConfigIns.AppLaunchChannel,
				Channel.AppResume => AdsConfigIns.AppResumeChannel,
				Channel.ForceAd => AdsConfigIns.ForceAdChannel,
				Channel.Rewarded => AdsConfigIns.RewardedChannel,
				Channel.Banner => AdsConfigIns.BannerChannel,
				Channel.Mrec => AdsConfigIns.MrecChannel,
				Channel.Popup => AdsConfigIns.PopupChannel,
				Channel.Collap => AdsConfigIns.CollapChannel,
				_ => null
			};
		}

		private void Awake()
		{
			Ins = this;
			IsRemovedAd = PlayerPrefs.GetInt("REMOVEADS", 0) == 1;

			NetEventSystem.OnFsClosed += info =>
			{
				UpdateLastTimeFSAd();
			};

			NetEventsBinder.Bind();

			NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "Awake", () => $"removed={IsRemovedAd}");
		}

		private void Start()
		{
			Initialize();
		}

		public static void UpdateLastTimeFSAd()
		{
			LastTimeFSAd = Time.realtimeSinceStartup;
		}

		public void Initialize()
		{
			var list = NetConfigsSO.Ins.ListAdCoreInfos;

			for (int i = 0; i < list.Length; i++)
			{
				if (list[i].InitPluginAwake)
					list[i].Core.InitPluginAtAwake();
			}

			NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "Start", () => $"initPluginAwakeCount={(list?.Length ?? 0)}");
		}

		public void InitAdsConfig(string adsConfigJson)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.adslogic, "InitAdsConfig", () => $"len={(adsConfigJson?.Length ?? 0)}"))
			{
				AdsConfigIns = JsonTool.DeserializeObject<AdSystemConfigs>(adsConfigJson);
				AdsConfigIns ??= new AdSystemConfigs();

				IsAlwaysTrackingRev = AdsConfigIns.AlwaysTrackRevenue;
				IsTrackingBgAdImpression = AdsConfigIns.TrackBgAdImpression;

				NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "InitAdsConfig Done",
					() => $"adcore={AdsConfigIns.SelectedAdCoreName} trackRev={IsAlwaysTrackingRev} trackBgImp={IsTrackingBgAdImpression}");
			}
		}

		public void InitMediation(string mediation_config)
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.adslogic, "InitMediation", () => $"len={(mediation_config?.Length ?? 0)}"))
			{
				if (IsCallInitMediation)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.adslogic, "InitMediation Skip", () => "already called");
					return;
				}

				IsCallInitMediation = true;

				if (Application.isEditor)
				{
					AdsCoreIns = adsCoreDefault;

					NetFlowDebugSystem.Warn(Layer.sys, Module.adslogic, "AdCore Choose", () => "editor -> adsCoreDefault");
				}
				else
				{
					var adCoreInfo = Array.Find(NetConfigsSO.Ins.ListAdCoreInfos, info => info.Core.AdCoreName == AdsConfigIns.SelectedAdCoreName);
					if (adCoreInfo == null || adCoreInfo.Core == null)
					{
						AdsCoreIns = adsCoreDefault;

						NetFlowDebugSystem.Warn(Layer.sys, Module.adslogic, "AdCore Choose", () => "not found -> adsCoreDefault");
					}
					else
					{
						AdsCoreIns = adCoreInfo.Core;

						NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "AdCore Choose", () => $"success -> {AdsConfigIns.SelectedAdCoreName}");
					}
				}

				// Setup managers (keep original order & logic)
				AR_ManagerIns.Setup(AdsCoreIns, AdsConfigIns.AppResumeChannel);
				AL_ManagerIns.Setup(AdsCoreIns, AdsConfigIns.AppLaunchChannel);

				FA_ManagerIns.Setup(AdsCoreIns, AdsConfigIns.ForceAdChannel);
				RW_ManagerIns.Setup(AdsCoreIns, AdsConfigIns.RewardedChannel);

				BN_ManagerIns.Setup(AdsCoreIns, AdsConfigIns.BannerChannel);
				Mrec_ManagerIns.Setup(AdsCoreIns, AdsConfigIns.MrecChannel);

				PU_ManagerIns.Setup(AdsCoreIns, AdsConfigIns.PopupChannel);
				CL_ManagerIns.Setup(AdsCoreIns, AdsConfigIns.CollapChannel);

				NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "Managers Setup", () => "done");

				NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "Mediation Init", () => "InitStats");
				AdsCoreIns.InitStats(mediation_config);

				NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "Mediation Init", () => "InitializeMediation");
				AdsCoreIns.InitializeMediation();

				NetFlowDebugSystem.Log(Layer.sys, Module.adslogic, "InitMediation Done", () => $"adcore={AdsConfigIns.SelectedAdCoreName}");
			}
		}

		public void PurchaseRemoveAds()
		{
			IsRemovedAd = true;
			PlayerPrefs.SetInt("REMOVEADS", 1);
			PlayerPrefs.Save();

			if (AdsConfigIns != null)
			{
				AdsCoreIns.BN_HideAd(BannerPlacement.FullBottom);
			}

			NetFlowDebugSystem.Warn(Layer.sys, Module.adslogic, "RemoveAds", () => "purchased=true");
		}

		public void RevertPurchaseRemoveAds()
		{
			IsRemovedAd = false;
			PlayerPrefs.SetInt("REMOVEADS", 0);
			PlayerPrefs.Save();

			NetFlowDebugSystem.Warn(Layer.sys, Module.adslogic, "RemoveAds", () => "purchased=false");
		}

		public string GetDebugInfo()
		{
			var sb = new StringBuilder();

			sb.AppendLine("=== ADS LOGIC DEBUG ===");

			sb.Append("- selectedAdCoreName  : ").AppendLine(AdsConfigIns != null ? AdsConfigIns.SelectedAdCoreName : "(null)");
			sb.Append("- alwaysTrackRevenue  : ").AppendLine(IsAlwaysTrackingRev.ToString());
			sb.Append("- trackBgAdImpression : ").AppendLine(IsTrackingBgAdImpression.ToString());
			sb.Append("- IsRemovedAd         : ").AppendLine(IsRemovedAd.ToString());
			sb.Append("- IsCallInitMediation : ").AppendLine(IsCallInitMediation.ToString());
			sb.Append("- LastTimeFSAd        : ").AppendLine(LastTimeFSAd.ToString("0.###"));

			sb.AppendLine("---");
			sb.Append("- Scene               : ").AppendLine(ScreenName ?? "(null)");

			sb.AppendLine();

			return sb.ToString();
		}
	}
}
