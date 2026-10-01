using BG_Library.Common;
using BG_Library.NET.Debug;
using Firebase.Extensions;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CountryRegionCheck;
using UnityEngine;

namespace BG_Library.NET.AdSystem
{
	public class RemoteConfig : MonoBehaviour
	{
		public static RemoteConfig Ins;

		public static Action OnAllJsonsComplete;
		public static Action OnAdsConfigApplied;
		public static Action OnMediationConfigApplied;
		public static Action OnCustomConfigsApplied;
		public static Action OnFirebaseInitialized;

		[BoxGroup("Status"), SerializeField, ReadOnly] private bool isDataFetched = false;
		[BoxGroup("Status"), SerializeField, ReadOnly] private bool isFirebaseInitialized = false;
		[BoxGroup("Status"), SerializeField, ReadOnly] private bool isRefrectedProperties = false;
		[BoxGroup("Status"), SerializeField, ReadOnly] private bool isFetchTimeOut = false;

		[BoxGroup("Status"), SerializeField, ReadOnly]
		private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

		[BoxGroup("Fetch data")] public string ads_config;
		[BoxGroup("Fetch data")] public string mediation_config;
		[BoxGroup("Custom RC (Runtime cache)"), SerializeField]
		private NetConfigsSO.CustomRemoteConfigs[] customRemoteConfigsRuntime;
		[SerializeField] private CountryRegionChecker countryRegionChecker;

		[SerializeField] private bool enableCheckCountry = true;
		
		public bool IsDataFetched => isDataFetched;
		public bool IsFirebaseInitialized => isFirebaseInitialized;
		public bool IsRefrectedProperties => isRefrectedProperties;

		public const string adConfigSt = "ads_config";
		public const string devicesSt = "devices";

		private Coroutine waitTimeOutCorou;
		private RegionCheckFinalResult regionCheckResult;
		private DeviceDebug deviceDebug;

		internal class DeviceDebug
		{
			public string[] debugDevices = new string[0];
		}

		private void Awake()
		{
			Ins = this;
		}

		private void Start()
		{
			countryRegionChecker.Check(OnCheckDone);
			deviceDebug = new DeviceDebug();
			InitFirebase();
		}

		private void OnCheckDone(RegionCheckFinalResult result)
		{
			regionCheckResult = result;
		}

		/// <summary>
		/// Get custom remote config content by key (runtime applied, fallback already handled).
		/// </summary>
		public string GetCustomRemoteConfigs(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Custom Get Rejected", () => "key is NULL/EMPTY");
				return "";
			}

			if (customRemoteConfigsRuntime == null || customRemoteConfigsRuntime.Length == 0)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Custom Get Miss", () => $"runtime empty key={key}");
				return "";
			}

			for (int i = 0; i < customRemoteConfigsRuntime.Length; i++)
			{
				var item = customRemoteConfigsRuntime[i];
				if (item == null) continue;

				if (item.Key == key)
				{
					if (string.IsNullOrEmpty(item.Content))
					{
						NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Custom Get Empty", () => $"key={key}");
						return "";
					}

					NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Custom Get Hit", () => $"key={key} len={item.Content.Length}");
					return item.Content;
				}
			}

			NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Custom Get Miss", () => $"key={key}");
			return "";
		}

		private void InitFirebase()
		{
			NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Init Start", () => "Firebase");

			Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
			{
				dependencyStatus = task.Result;

				if (dependencyStatus == Firebase.DependencyStatus.Available)
				{
					isFirebaseInitialized = true;
					NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Firebase Ready", () => $"status={dependencyStatus}");
					OnFirebaseInitialized?.Invoke();
					InitializeRemoteConfig();
				}
				else
				{
					NetFlowDebugSystem.Error(Layer.sys, Module.rc, "Firebase Fail", () => $"status={dependencyStatus}");

					// fallback để không bị kẹt luồng
					RefrectProperties();
				}
			});
		}

		private void InitializeRemoteConfig()
		{
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.rc, "Defaults Build", () => "Start"))
			{
				// 1) Base config (cache > SO)
				string st0 = PlayerPrefs.GetString(adConfigSt, NetConfigsSO.Ins.AdsConfigsDefault);
				if (string.IsNullOrEmpty(st0)) st0 = NetConfigsSO.Ins.AdsConfigsDefault;

				Dictionary<string, object> defaults = new Dictionary<string, object>()
				{
					{ adConfigSt, st0 }
				};

				NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Defaults Base", () => $"key={adConfigSt} len={st0?.Length ?? 0}",
					d => d.AddKV("defaultsCount", defaults.Count));

				// 2) Per-core defaults
				var coreList = NetConfigsSO.Ins.ListAdCoreInfos;
				if (coreList == null || coreList.Length == 0)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Defaults Core Skip", () => "ListAdCoreInfos empty");
				}
				else
				{
					int add = 0, skipNull = 0, skipEmpty = 0, dup = 0;

					for (int i = 0; i < coreList.Length; i++)
					{
						var infos = coreList[i];
						if (infos == null || infos.Core == null)
						{
							skipNull++;
							continue;
						}

						string key = infos.Core.AdCoreName;
						if (string.IsNullOrEmpty(key))
						{
							skipEmpty++;
							continue;
						}

						string value = PlayerPrefs.GetString(key, infos.DefaultConfigs);
						if (string.IsNullOrEmpty(value)) value = infos.DefaultConfigs;

						if (defaults.ContainsKey(key))
						{
							dup++;
							continue;
						}

						defaults.Add(key, value);
						add++;
					}

					// ✅ Debug mới: summary “kĩ nhưng gọn”
					NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Defaults Core Done",
						() => $"add={add} skipNull={skipNull} skipEmpty={skipEmpty} dup={dup}",
						d => d.AddKV("listCount", coreList.Length).AddKV("defaultsCount", defaults.Count));

					if (dup > 0)
						NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Defaults Core Warn", () => $"dup={dup}");
				}

				// 3) Custom RC defaults
				BuildCustomDefaults(defaults);

				// 4) Set defaults
				try
				{
					var task = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
					task.ContinueWithOnMainThread(t =>
					{
						if (t.IsFaulted)
						{
							NetFlowDebugSystem.Error(Layer.sys, Module.rc, "Defaults Set Fail", () => "SetDefaultsAsync faulted");
						}
						else
						{
							NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Defaults Set Done", () => $"count={defaults.Count}");
						}
					});
				}
				catch (Exception e)
				{
					NetFlowDebugSystem.Error(Layer.sys, Module.rc, "Defaults Set Throw", () => e.GetType().Name);
				}

				StartCoroutine(FetchDataAsync());
			}
		}

		private void BuildCustomDefaults(Dictionary<string, object> defaults)
		{
			var customList = NetConfigsSO.Ins.ListCustomRemoteConfigs;

			if (customList == null || customList.Length == 0)
			{
				NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Defaults Custom Skip", () => "ListCustomRemoteConfigs empty",
					d => d.AddKV("defaultsCount", defaults.Count));
				return;
			}

			HashSet<string> seen = new HashSet<string>();

			int add = 0, skipNull = 0, skipEmpty = 0, dup = 0, existed = 0;

			for (int i = 0; i < customList.Length; i++)
			{
				var src = customList[i];
				if (src == null)
				{
					skipNull++;
					continue;
				}

				string key = src.Key;
				string soDefault = src.Content;

				if (string.IsNullOrEmpty(key))
				{
					skipEmpty++;
					continue;
				}

				if (!seen.Add(key))
				{
					dup++;
					continue;
				}

				string cached = PlayerPrefs.GetString(key, soDefault);
				if (string.IsNullOrEmpty(cached)) cached = soDefault;

				if (defaults.ContainsKey(key))
				{
					existed++;
					continue;
				}

				defaults.Add(key, cached);
				add++;
			}

			// ✅ Debug mới: summary “kĩ nhưng gọn”
			NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Defaults Custom Done",
				() => $"add={add} skipNull={skipNull} skipEmpty={skipEmpty} dup={dup} existed={existed}",
				d => d.AddKV("listCount", customList.Length).AddKV("defaultsCount", defaults.Count));

			if (dup > 0 || existed > 0)
				NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Defaults Custom Warn", () => $"dup={dup} existed={existed}");
		}

		private IEnumerator FetchDataAsync()
		{
			yield return new WaitUntil(() => regionCheckResult != null);
			
			isFetchTimeOut = false;

			NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Fetch Start", () => "FetchAsync");
			
			Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);
			fetchTask.ContinueWithOnMainThread(FetchComplete);

			if (waitTimeOutCorou != null) StopCoroutine(waitTimeOutCorou);
			waitTimeOutCorou = StartCoroutine(WaitTimeOutFetch(10f));
		}

		private void FetchComplete(Task fetchTask)
		{
			if (isFetchTimeOut)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Fetch Ignored", () => "timed out already");
				return;
			}

			if (waitTimeOutCorou != null)
			{
				StopCoroutine(waitTimeOutCorou);
				waitTimeOutCorou = null;
			}

			if (fetchTask.IsCanceled)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Fetch Canceled", () => "fallback apply");
				RefrectProperties();
				return;
			}

			if (fetchTask.IsFaulted)
			{
				NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Fetch Faulted", () => "fallback apply");
				RefrectProperties();
				return;
			}

			if (fetchTask.IsCompletedSuccessfully)
			{
				NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Fetch Done", () => "completed");
			}

			var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;

			NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Fetch Status", () => $"{info.LastFetchStatus}");

			switch (info.LastFetchStatus)
			{
				case Firebase.RemoteConfig.LastFetchStatus.Success:
					Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
						.ActivateAsync()
						.ContinueWithOnMainThread(_ =>
						{
							NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Activate Done", () => "apply");
							RefrectProperties();
						});

					break;

				case Firebase.RemoteConfig.LastFetchStatus.Failure:
					switch (info.LastFetchFailureReason)
					{
						case Firebase.RemoteConfig.FetchFailureReason.Error:
							NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Fetch Fail", () => "reason=Error");
							break;

						case Firebase.RemoteConfig.FetchFailureReason.Throttled:
							NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Fetch Fail", () => "reason=Throttled");
							break;
					}

					RefrectProperties();
					break;

				case Firebase.RemoteConfig.LastFetchStatus.Pending:
					NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Fetch Pending", () => "fallback apply");
					RefrectProperties();
					break;

				default:
					RefrectProperties();
					break;
			}
		}

		private IEnumerator WaitTimeOutFetch(float timeoutSeconds)
		{
			float t = 0f;

			while (true)
			{
				t += Time.deltaTime;

				if (t > timeoutSeconds)
				{
					NetFlowDebugSystem.Warn(Layer.sys, Module.rc, "Fetch Timeout", () => $"{timeoutSeconds:0.#}s -> apply");

					isFetchTimeOut = true;
					RefrectProperties();
					yield break;
				}

				yield return null;
			}
		}

		private void RefrectProperties()
		{
			if (isRefrectedProperties) return;
			isRefrectedProperties = true;

			using (NetFlowDebugSystem.Flow(Layer.sys, Module.rc, "Apply Start", () => "RefrectProperties"))
			{
				var rc = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
				
				// Get Debug Config
				var debugRaw = rc.GetValue(devicesSt).StringValue;
				if(!string.IsNullOrEmpty(debugRaw)) deviceDebug = JsonUtility.FromJson<DeviceDebug>(debugRaw);

				// ====== 1) ADS CONFIG (REAL) ======
				string adsSource = "rc";
				if (Application.isEditor || NetConfigsSO.Ins.SetupConfigNETType == InitNetType.Default)
				{
					ads_config = NetConfigsSO.Ins.AdsConfigsDefault;
					adsSource = "so";
				}
				else
				{
					ads_config = rc.GetValue(adConfigSt).StringValue;
					if (string.IsNullOrEmpty(ads_config))
					{
						ads_config = PlayerPrefs.GetString(adConfigSt, NetConfigsSO.Ins.AdsConfigsDefault);
						adsSource = string.IsNullOrEmpty(ads_config) ? "so" : "cache";

						if (string.IsNullOrEmpty(ads_config))
							ads_config = NetConfigsSO.Ins.AdsConfigsDefault;
					}
				}

				if (IsCountryCheck() && enableCheckCountry)
				{
					ads_config = NetConfigsSO.Ins.AdsConfigsCountry;
				}
				
				NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Real Ads", () => $"src={adsSource} len={ads_config?.Length ?? 0}");
				AdsLogic.Ins.InitAdsConfig(ads_config);
				SaveAdsCacheSnapshot();
				OnAdsConfigApplied?.Invoke();

				// ====== 2) MEDIATION CONFIG (REAL) ======
				string mediationKey = AdsLogic.Ins.AdCoreName;
				string medSource = "rc";

				if (Application.isEditor || NetConfigsSO.Ins.SetupConfigNETType == InitNetType.Default)
				{
					mediation_config = "";
					var list = NetConfigsSO.Ins.ListAdCoreInfos;

					for (int i = 0; i < list.Length; i++)
					{
						var info = list[i];
						if (info == null || info.Core == null) continue;

						if (info.Core.AdCoreName == mediationKey)
						{
							mediation_config = info.DefaultConfigs;
							break;
						}
					}

					medSource = "so";
				}
				else
				{
					mediation_config = rc.GetValue(mediationKey).StringValue;

					if (string.IsNullOrEmpty(mediation_config))
					{
						// cache
						mediation_config = PlayerPrefs.GetString(mediationKey, "");
						medSource = string.IsNullOrEmpty(mediation_config) ? "so" : "cache";

						// SO fallback
						if (string.IsNullOrEmpty(mediation_config))
						{
							var list = NetConfigsSO.Ins.ListAdCoreInfos;
							for (int i = 0; i < list.Length; i++)
							{
								var info = list[i];
								if (info == null || info.Core == null) continue;

								if (info.Core.AdCoreName == mediationKey)
								{
									mediation_config = info.DefaultConfigs;
									break;
								}
							}
						}
					}
				}

				if (IsCountryCheck() && enableCheckCountry)
				{
					var list = NetConfigsSO.Ins.ListAdCoreInfos;

					for (int i = 0; i < list.Length; i++)
					{
						var info = list[i];
						if (info == null || info.Core == null) continue;

						if (info.Core.AdCoreName == mediationKey)
						{
							mediation_config = info.DefaultConfigsCountry;
							break;
						}
					}

					medSource = "so";
				}
				
				NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Real Med", () => $"adCoreKey={mediationKey} src={medSource} len={mediation_config?.Length ?? 0}");
				AdsLogic.Ins.InitMediation(mediation_config);
				SaveMediationCacheSnapshot();
				OnMediationConfigApplied?.Invoke();

				// ====== 3) CUSTOM REMOTE CONFIG (REAL) ======
				ApplyCustomRemoteConfigs(rc);
				SaveCustomCacheSnapshots();
				OnCustomConfigsApplied?.Invoke();

				// ====== Complete ======
				OnAllJsonsComplete?.Invoke();

				SaveData();
				isDataFetched = true;

				NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Apply Done",
					() => $"adsLen={ads_config?.Length ?? 0} medLen={mediation_config?.Length ?? 0} customCount={(customRemoteConfigsRuntime?.Length ?? 0)}");
			}
		}

		private bool IsCountryCheck()
		{
			var hasResult = regionCheckResult != null;
			var isCountryTarget = regionCheckResult.isTargetCountry;
			return hasResult && isCountryTarget && !IsDeviceDebug()/* && NetConfigsSO.Ins.Debug_Preset == DebugPreset.Off*/;
		}

		private bool IsDeviceDebug()
		{
			var deviceId = AndroidDeviceIdUtils.GetAndroidId();
			UnityEngine.Debug.Log($"AndroidDeviceIdUtils - {deviceId}");
			var isDeviceDebug = deviceDebug.debugDevices.Contains(deviceId);
#if UNITY_EDITOR
			return true;
#else 
			return isDeviceDebug;
#endif
		}

		private void ApplyCustomRemoteConfigs(Firebase.RemoteConfig.FirebaseRemoteConfig rc)
		{
			var soList = NetConfigsSO.Ins.ListCustomRemoteConfigs;

			if (soList == null || soList.Length == 0)
			{
				NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Real Custom Skip", () => "so list empty");

				customRemoteConfigsRuntime = Array.Empty<NetConfigsSO.CustomRemoteConfigs>();
				return;
			}

			var runtime = new List<NetConfigsSO.CustomRemoteConfigs>(soList.Length);

			int fromRc = 0, fromCache = 0, fromSo = 0;

			for (int i = 0; i < soList.Length; i++)
			{
				var src = soList[i];
				if (src == null || string.IsNullOrEmpty(src.Key))
					continue;

				if (Application.isEditor || NetConfigsSO.Ins.SetupConfigNETType == InitNetType.Default)
				{
					runtime.Add(new NetConfigsSO.CustomRemoteConfigs(src.Key, src.Content));
					fromSo++;
				}
				else
				{
					string rcValue = rc.GetValue(src.Key).StringValue;

					if (string.IsNullOrEmpty(rcValue))
					{
						string cached = PlayerPrefs.GetString(src.Key, src.Content);
						if (string.IsNullOrEmpty(cached)) cached = src.Content;

						runtime.Add(new NetConfigsSO.CustomRemoteConfigs(src.Key, cached));
						fromCache++;
					}
					else
					{
						runtime.Add(new NetConfigsSO.CustomRemoteConfigs(src.Key, rcValue));
						fromRc++;
					}
				}
			}

			if (IsCountryCheck() && enableCheckCountry)
			{
				runtime.Clear();
				for (int i = 0; i < soList.Length; i++)
				{
					var src = soList[i];
					if (src == null || string.IsNullOrEmpty(src.Key))
						continue;
					runtime.Add(new NetConfigsSO.CustomRemoteConfigs(src.Key, src.ContentCountry));
				}
			}

			customRemoteConfigsRuntime = runtime.ToArray();

			NetFlowDebugSystem.Log(Layer.sys, Module.rc, "Real Custom", () => $"count={customRemoteConfigsRuntime.Length}",
				d => d.AddKV("fromRc", fromRc).AddKV("fromCache", fromCache).AddKV("fromSo", fromSo));
		}

		private void SaveData()
		{
			SaveAdsCacheSnapshot();
			SaveMediationCacheSnapshot();
			SaveCustomCacheSnapshots();
			PlayerPrefs.Save();
		}

		private void SaveAdsCacheSnapshot()
		{
			PlayerPrefs.SetString(adConfigSt, ads_config ?? "");
		}

		private void SaveMediationCacheSnapshot()
		{
			string mediationKey = AdsLogic.Ins.AdCoreName;
			if (string.IsNullOrEmpty(mediationKey))
				return;

			PlayerPrefs.SetString(mediationKey, mediation_config ?? "");
		}

		private void SaveCustomCacheSnapshots()
		{
			if (customRemoteConfigsRuntime == null || customRemoteConfigsRuntime.Length == 0)
				return;

			for (int i = 0; i < customRemoteConfigsRuntime.Length; i++)
			{
				var item = customRemoteConfigsRuntime[i];
				if (item == null || string.IsNullOrEmpty(item.Key))
					continue;

				PlayerPrefs.SetString(item.Key, item.Content ?? "");
			}
		}
	}
}
