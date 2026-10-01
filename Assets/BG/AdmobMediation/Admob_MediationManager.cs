using BG_Library.Common;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GoogleMobileAds.Common;
using UnityEngine;

namespace BG_Library.NET.Mediation.Admob
{
	public class Admob_MediationManager
	{
		// Titles should be GROUP names (not format names) - per your rule
		private const string TITLE_AO = "Admob_AOGroup";
		private const string TITLE_RW = "Admob_RWGroup";
		private const string TITLE_FA = "Admob_FAGroup";
		private const string TITLE_BN = "Admob_BNGroup";
		private const string TITLE_MREC = "Admob_MrecGroup";
		private const string TITLE_INIT = "InitSDK";

		public const string GroupName_Ao = "admob_appopen";
		public const string GroupName_Rw = "admob_rewarded";
		public const string GroupName_Mrec = "admob_mrec";

		public Admob_FSGroupController<Admob_AOInfo, Admob_AOAccessAPI> AO_Group { get; private set; }
		public Admob_FSGroupController<Admob_RWInfo, Admob_RWAccessAPI> RW_Group { get; private set; }
		public Admob_FSGroupController<Admob_FAInfo, Admob_FAAccessAPI>[] FA_Groups { get; private set; }

		public Admob_RectGroupController<Admob_BNInfo> BN_FullBottomGroup { get; private set; }
		public Admob_RectGroupController<Admob_BNInfo> BN_FullTopGroup { get; private set; }
		public Admob_RectGroupController<Admob_BNInfo> BN_TopLeftGroup { get; private set; }
		public Admob_RectGroupController<Admob_BNInfo> BN_TopRightGroup { get; private set; }
		public Admob_RectGroupController<Admob_BNInfo> BN_BottomLeftGroup { get; private set; }
		public Admob_RectGroupController<Admob_BNInfo> BN_BottomRightGroup { get; private set; }
		public Admob_RectGroupController<Admob_MrecInfo> Mrec_Group { get; private set; }

		public Admob_MediationManager(IAdmob_Configs configs)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, "Constructor", () => "create groups",
				details: d =>
				{
				d.AddKV("aoGroup", GroupName_Ao);
				d.AddKV("rwGroup", GroupName_Rw);
					d.AddKV("bnGroup", "admob_banner_full_bottom");
					d.AddKV("mrecGroup", GroupName_Mrec);
				});

			// AO
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_AO, () => $"create groupName={GroupName_Ao} mediation={BG_ConstValue.mediation_admob} format={BG_ConstValue.adtype_ao}");
			AO_Group = new(
				info: configs.GetAdmobAOInfo(),
				format: BG_ConstValue.adtype_ao,
				groupName: GroupName_Ao,
				mediation: BG_ConstValue.mediation_admob,
				budget: 0);

			// RW
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_RW, () => $"create groupName={GroupName_Rw} mediation={BG_ConstValue.mediation_admob} format={BG_ConstValue.adtype_rw}");
			RW_Group = new(
				info: configs.GetAdmobRWInfo(),
				format: BG_ConstValue.adtype_rw,
				groupName: GroupName_Rw,
				mediation: BG_ConstValue.mediation_admob,
				budget: 0);

			// FA
			FA_Init(configs);

			// BN
            BN_Init(configs);

			// MREC
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_MREC, () => $"create groupName={GroupName_Mrec} mediation={BG_ConstValue.mediation_admob} format={BG_ConstValue.adtype_mrec}");
			Mrec_Group = new(
				info: configs.GetAdmobMrecId(),
				format: BG_ConstValue.adtype_mrec,
				groupName: GroupName_Mrec,
				mediation: BG_ConstValue.mediation_admob);
		}

		void FA_Init(IAdmob_Configs configs)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_FA, () => "init",
				details: d => d.Add("build FA_Groups[] from configs.GetAdmobFAInfo()"));

			var fa_Infos = configs.GetAdmobFAInfo();
			if (fa_Infos == null || fa_Infos.Length == 0)
			{
				FA_Groups = Array.Empty<Admob_FSGroupController<Admob_FAInfo, Admob_FAAccessAPI>>();
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, TITLE_FA, () => "infos empty -> FA_Groups=Empty");
				return;
			}

			FA_Groups = new Admob_FSGroupController<Admob_FAInfo, Admob_FAAccessAPI>[fa_Infos.Length];

			for (int i = 0; i < fa_Infos.Length; i++)
			{
				var info = fa_Infos[i];

				// show each group creation in verbose mode only
				NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_FA, () => $"create[{i}] groupName={info.GroupName} budget={info.MaxShowCount}",
					details: d =>
					{
						d.AddKV("group", info.GroupName);
						d.AddKV("format", BG_ConstValue.adtype_fa);
						d.AddKV("mediation", BG_ConstValue.mediation_admob);
						d.AddKV("preload", info.PreloadAd);
						d.AddKV("buffer", info.AdBufferSize);
						d.AddKV("unit", info.Id);
					});

				FA_Groups[i] = new(
					info: info,
					format: BG_ConstValue.adtype_fa,
					groupName: info.GroupName,
					mediation: BG_ConstValue.mediation_admob,
					budget: info.MaxShowCount);
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_FA, () => $"done count={FA_Groups.Length}");
		}

        void BN_Init(IAdmob_Configs configs)
        {
            var infos = configs.GetAdmobBNInfos();

            BN_FullBottomGroup = CreateBannerGroup(infos, AdmobRectPlacement.FullBottom);
            BN_FullTopGroup = CreateBannerGroup(infos, AdmobRectPlacement.FullTop);
            BN_TopLeftGroup = CreateBannerGroup(infos, AdmobRectPlacement.TopLeft);
            BN_TopRightGroup = CreateBannerGroup(infos, AdmobRectPlacement.TopRight);
            BN_BottomLeftGroup = CreateBannerGroup(infos, AdmobRectPlacement.BottomLeft);
            BN_BottomRightGroup = CreateBannerGroup(infos, AdmobRectPlacement.BottomRight);

        }

        public Admob_RectGroupController<Admob_BNInfo> GetBNGroup(BannerPlacement placement)
        {
            return GetBNGroup(ToAdmobRectPlacement(placement));
        }

        private Admob_RectGroupController<Admob_BNInfo> GetBNGroup(AdmobRectPlacement placement)
        {
            return placement switch
            {
                AdmobRectPlacement.FullBottom => BN_FullBottomGroup,
                AdmobRectPlacement.FullTop => BN_FullTopGroup,
                AdmobRectPlacement.TopLeft => BN_TopLeftGroup,
                AdmobRectPlacement.TopRight => BN_TopRightGroup,
                AdmobRectPlacement.BottomLeft => BN_BottomLeftGroup,
                AdmobRectPlacement.BottomRight => BN_BottomRightGroup,
                _ => null
            };
        }

        private Admob_RectGroupController<Admob_BNInfo> CreateBannerGroup(Admob_BNInfo[] infos, AdmobRectPlacement placement)
        {
            var info = FindBannerInfo(infos, placement) ?? new Admob_BNInfo("", placement);
            var groupName = GetBannerGroupName(placement);

            NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_BN,
                () => $"create groupName={groupName} placement={placement} mediation={BG_ConstValue.mediation_admob} format={BG_ConstValue.adtype_bn}");

            return new(
                info: info,
                format: BG_ConstValue.adtype_bn,
                groupName: groupName,
                mediation: BG_ConstValue.mediation_admob);
        }

        private static Admob_BNInfo FindBannerInfo(Admob_BNInfo[] infos, AdmobRectPlacement placement)
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

        private static string GetBannerGroupName(AdmobRectPlacement placement)
        {
            return placement switch
            {
                AdmobRectPlacement.FullBottom => "admob_banner_full_bottom",
                AdmobRectPlacement.FullTop => "admob_banner_full_top",
                AdmobRectPlacement.TopLeft => "admob_banner_top_left",
                AdmobRectPlacement.TopRight => "admob_banner_top_right",
                AdmobRectPlacement.BottomLeft => "admob_banner_bottom_left",
                AdmobRectPlacement.BottomRight => "admob_banner_bottom_right",
                _ => "admob_banner_full_bottom"
            };
        }

        private static AdmobRectPlacement ToAdmobRectPlacement(BannerPlacement placement)
        {
            return placement switch
            {
                BannerPlacement.FullBottom => AdmobRectPlacement.FullBottom,
                BannerPlacement.FullTop => AdmobRectPlacement.FullTop,
                BannerPlacement.TopLeft => AdmobRectPlacement.TopLeft,
                BannerPlacement.TopRight => AdmobRectPlacement.TopRight,
                BannerPlacement.BottomLeft => AdmobRectPlacement.BottomLeft,
                BannerPlacement.BottomRight => AdmobRectPlacement.BottomRight,
                _ => AdmobRectPlacement.FullBottom
            };
        }

		public IFSGroup FA_GetGroup(string groupName)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, "FA_GetGroup", () => $"request groupName={groupName}");

			if (FA_Groups == null || FA_Groups.Length == 0)
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, "FA_GetGroup", () => "FA_Groups empty -> return null");
				return null;
			}

			if (string.IsNullOrEmpty(groupName))
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, "FA_GetGroup", () => "groupName null/empty -> return null");
				return null;
			}

			for (int i = 0; i < FA_Groups.Length; i++)
			{
				var c = FA_Groups[i];
				if (c == null) continue;

				if (string.Equals(c.GroupName, groupName, StringComparison.Ordinal))
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, "FA_GetGroup", () => $"hit index={i} groupName={groupName}");
					return c;
				}
			}

			NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, "FA_GetGroup", () => $"miss groupName={groupName} -> return null");
			return null;
		}

		#region Init SDK

		static GoogleMobileAdsConsentController _consentController;
		public static bool IsCallInit { get; private set; }
		public static bool IsInitComplete { get; private set; }
		public static bool CanRequestAds => _consentController != null && _consentController.CanRequestAds;

		// The Google Mobile Ads Unity plugin needs to be run only once.
		private static bool? _isInitialized;

		public static string GetDeviceId()
		{
			List<string> candidates = GetAutoTestDeviceIds(includeSimulator: false);
			return candidates.Count > 0 ? candidates[0] : "unknown";
		}

		public static List<string> GetAutoTestDeviceIds(bool includeSimulator = true)
		{
			var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

#if UNITY_ANDROID && !UNITY_EDITOR
			try
			{
				using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
					AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");

					using (var secure = new AndroidJavaClass("android.provider.Settings$Secure"))
					{
						string androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");
						AddHashedCandidates(candidates, androidId);
					}
				}
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogWarning($"[AdMob] Failed to auto-detect Android test device IDs: {e.Message}");
			}
#endif

			AddHashedCandidates(candidates, SystemInfo.deviceUniqueIdentifier);
			AddRawCandidates(candidates, SystemInfo.deviceUniqueIdentifier);

			if (includeSimulator)
				AddRawCandidates(candidates, AdRequest.TestDeviceSimulator);

			return candidates.Where(id => !string.IsNullOrWhiteSpace(id) && !string.Equals(id, "unknown", StringComparison.OrdinalIgnoreCase)).ToList();
		}

		public static List<string> GetAutoConsentTestDeviceHashedIds()
		{
			var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

#if UNITY_ANDROID && !UNITY_EDITOR
			try
			{
				using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
					AndroidJavaObject contentResolver = activity.Call<AndroidJavaObject>("getContentResolver");

					using (var secure = new AndroidJavaClass("android.provider.Settings$Secure"))
					{
						string androidId = secure.CallStatic<string>("getString", contentResolver, "android_id");
						AddHashedCandidates(candidates, androidId);
					}
				}
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogWarning($"[AdMob] Failed to auto-detect UMP test device hashes: {e.Message}");
			}
#endif

			AddHashedCandidates(candidates, SystemInfo.deviceUniqueIdentifier);
			return candidates.Where(id => !string.IsNullOrWhiteSpace(id) && !string.Equals(id, "unknown", StringComparison.OrdinalIgnoreCase)).ToList();
		}

		private static void AddRawCandidates(HashSet<string> candidates, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return;

			string trimmed = value.Trim();
			candidates.Add(trimmed);
			candidates.Add(trimmed.ToLowerInvariant());
			candidates.Add(trimmed.ToUpperInvariant());
		}

		private static void AddHashedCandidates(HashSet<string> candidates, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return;

			string trimmed = value.Trim();
			candidates.Add(ComputeMd5Hex(trimmed, false));
			candidates.Add(ComputeMd5Hex(trimmed, true));
		}

		private static string ComputeMd5Hex(string value, bool upperCase)
		{
			using var md5 = MD5.Create();
			byte[] bytes = Encoding.ASCII.GetBytes(value ?? string.Empty);
			byte[] hash = md5.ComputeHash(bytes);

			var builder = new StringBuilder(hash.Length * 2);
			string format = upperCase ? "X2" : "x2";
			for (int i = 0; i < hash.Length; i++)
				builder.Append(hash[i].ToString(format));

			return builder.ToString();
		}

		public static void InitMediation(Action onComplete = null)
		{
			if (IsInitComplete || IsCallInit)
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => $"skip (IsInitComplete={IsInitComplete} IsCallInit={IsCallInit})");
				return;
			}

			IsCallInit = true;

			using (NetFlowDebugSystem.Flow(Layer.adcore, Module.med_admob, TITLE_INIT, () => "InitMediation"))
			{
				_consentController = new GoogleMobileAdsConsentController();

				// iOS pause behavior
				MobileAds.SetiOSAppPauseOnBackground(true);
				MobileAdsEventExecutor.Initialize();

				// Setup test device config (if needed)
				SetupTestDeviceIfNeeded();

				// ✅ GatherConsent trước, rồi mới init SDK nếu can request ads
				InitializeGoogleMobileAdsConsent(() =>
				{
					MobileAdsEventExecutor.ExecuteInUpdate(() =>
					{
						bool canRequest = _consentController != null && _consentController.CanRequestAds;
						NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => $"Consent done -> CanRequestAds={canRequest}");

						if (canRequest)
						{
							RequestIosAppTrackingAuthorizationIfConsentAllowsPersonalizedAds(() =>
							{
								InitializeGoogleMobileAds(() =>
								{
									IsInitComplete = true;

									NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "complete (SDK initialized)");

									onComplete?.Invoke();
								});
							});
						}
						else
						{
							// Không thể request ads -> kết thúc flow init mediation.
							// Không hỏi ATT khi consent chưa/không cho phép request ads.
							IsInitComplete = true;

							NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, TITLE_INIT, () => "complete (CannotRequestAds, skip ATT)");

							onComplete?.Invoke();
						}
					});
				});
			}
		}

		static void RequestIosAppTrackingAuthorizationIfConsentAllowsPersonalizedAds(Action onComplete)
		{
#if UNITY_IOS && !UNITY_EDITOR
			if (!HasConsentForAtt())
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, TITLE_INIT, () => "ATT skip (personalized consent not granted)");
				onComplete?.Invoke();
				return;
			}

			int status = IOSAppTrackingTransparencyBridge.GetAuthorizationStatus();
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => $"ATT status before request={status}");

			const int notDetermined = 0;
			if (status != notDetermined)
			{
				onComplete?.Invoke();
				return;
			}

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "ATT request...");
			IOSAppTrackingTransparencyBridge.RequestAuthorization(attStatus =>
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => $"ATT completed status={attStatus}");
				MobileAdsEventExecutor.ExecuteInUpdate(() => onComplete?.Invoke());
			});
#else
			onComplete?.Invoke();
#endif
		}

		static bool HasConsentForAtt()
		{
			string purposeConsents = PlayerPrefs.GetString("IABTCF_PurposeConsents", string.Empty);
			if (string.IsNullOrEmpty(purposeConsents))
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "TCF purpose consents empty -> allow ATT");
				return true;
			}

			bool storageConsent = HasTcfPurposeConsent(purposeConsents, 1);
			bool personalizedAdsConsent =
				HasTcfPurposeConsent(purposeConsents, 3) &&
				HasTcfPurposeConsent(purposeConsents, 4);

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT,
				() => $"TCF purpose consents p1={storageConsent} p3p4={personalizedAdsConsent} raw={purposeConsents}");

			return storageConsent && personalizedAdsConsent;
		}

		static bool HasTcfPurposeConsent(string purposeConsents, int purposeId)
		{
			int index = purposeId - 1;
			return !string.IsNullOrEmpty(purposeConsents) &&
				index >= 0 &&
				index < purposeConsents.Length &&
				purposeConsents[index] == '1';
		}

		static void SetupTestDeviceIfNeeded()
		{
			if (!NetConfigsSO.Ins.Admob_TestDevice)
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "SetupTestDevice skip (Admob_TestDevice=false)");
				return;
			}

			List<string> testDeviceIds = GetAutoTestDeviceIds();

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "SetupTestDevice",
				details: d =>
				{
					d.AddKV("count", testDeviceIds.Count);
					d.AddKV("ids", testDeviceIds.Count == 0 ? "(none)" : string.Join(", ", testDeviceIds));
					d.AddKV("simulator", AdRequest.TestDeviceSimulator);
					d.AddKV("rating", MaxAdContentRating.T);
				});

			if (testDeviceIds.Count == 0)
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, TITLE_INIT, () => "SetupTestDevice skip (no auto test device id candidate)");
				return;
			}

			var requestConfig = new RequestConfiguration()
			{
				TestDeviceIds = testDeviceIds,
				MaxAdContentRating = MaxAdContentRating.T,
				TagForChildDirectedTreatment = TagForChildDirectedTreatment.Unspecified,
				TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.False
			};

			MobileAds.SetRequestConfiguration(requestConfig);
		}

		/// <summary>
		/// Ensures that privacy and consent information is up to date.
		/// </summary>
		static void InitializeGoogleMobileAdsConsent(Action onConsentDone)
		{
			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "GatherConsent...");

			_consentController.GatherConsent((string error) =>
			{
				if (!string.IsNullOrEmpty(error))
				{
					NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, TITLE_INIT, () => "GatherConsent error", details: d => d.AddKV("error", error));
				}
				else
				{
					NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => $"Consent updated: {ConsentInformation.ConsentStatus}");
				}

				onConsentDone?.Invoke();
			});
		}

		static void InitializeGoogleMobileAds(Action onInitialized)
		{
			// ✅ init rồi
			if (_isInitialized == true)
			{
				NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "MobileAds.Initialize skip (already initialized)");
				onInitialized?.Invoke();
				return;
			}

			// ✅ đang init
			if (_isInitialized == false)
			{
				NetFlowDebugSystem.Warn(Layer.adcore, Module.med_admob, TITLE_INIT, () => "MobileAds.Initialize already in progress");
				return;
			}

			_isInitialized = false;

			NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "MobileAds.Initialize...");

			MobileAds.Initialize((InitializationStatus initstatus) =>
			{
				if (initstatus == null)
				{
					NetFlowDebugSystem.Error(Layer.adcore, Module.med_admob, TITLE_INIT, () => "MobileAds init failed (initstatus null)");

					_isInitialized = null;

					// ✅ tránh bị kẹt init
					IsCallInit = false;
					return;
				}

				// Log adapter status (nếu dùng mediation)
				var adapterStatusMap = initstatus.getAdapterStatusMap();
				int adapterCount = 0;

				if (adapterStatusMap != null)
				{
					foreach (var item in adapterStatusMap)
					{
						adapterCount++;

						// Flow log only when Verbose to avoid spam
						if (NetFlowDebugSystem.Verbose)
						{
							NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => "Adapter",
								details: d =>
								{
									d.AddKV("name", item.Key);
									d.AddKV("state", item.Value.InitializationState);
								});
						}
					}
				}

				NetFlowDebugSystem.Log(Layer.adcore, Module.med_admob, TITLE_INIT, () => $"MobileAds initialization complete (adapters={adapterCount})");

				_isInitialized = true;
				onInitialized?.Invoke();
			});
		}

		#endregion
	}
}
