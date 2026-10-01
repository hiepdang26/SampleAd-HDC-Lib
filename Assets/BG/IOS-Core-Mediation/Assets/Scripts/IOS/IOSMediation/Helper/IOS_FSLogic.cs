using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.AndroidSDK;
using BG_Library.NET.Debug;

namespace BG_Library.NET.Mediation.IOS
{
	public sealed class IOS_FSLogic<T>
		where T : IOS_FSInfo
	{
		private readonly IOS_FSGroupController<T> core;
		private IFSInstance ad;

		private readonly string idKey;
		private const string BridgeLogTag = "[ios-bridge]";

		public bool HasAdInstance => ad != null;

		public IOS_FSLogic(IOS_FSGroupController<T> core)
		{
			this.core = core;
			idKey = this.core.Id;
			ad = CreateAdInstance();

			NetFlowDebugSystem.Log(Layer.group, Module.ios_api_fs, $"Create.Instance {core.GroupName}",
				() => $"adtype={core.Adtype} id={idKey} layoutNames={FormatLayoutNames(core.LayoutGroup)} instance={AdInstanceName()}");
		}

		public void RequestAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_fs, $"SDK.Load {core.GroupName}",
				       () => $"adtype={core.Adtype} id={idKey} call={AdInstanceName()}.LoadAd()"))
			{
				ad?.LoadAd();
			}
		}

		public bool GetAdReady() => ad != null && ad.IsReady();

		public void Show()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_fs, $"SDK.Show {core.GroupName}",
				       () => $"adtype={core.Adtype} id={idKey} call={AdInstanceName()}.ShowAd()"))
			{
				try
				{
					ad?.ShowAd();
				}
				catch (System.Exception ex)
				{
					core.OnAdDisplayFailedEvent("", ex.Message, errorCode: -1);
				}
			}
		}

		public void DestroyAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_fs, $"SDK.Destroy {core.GroupName}",
				       () => $"adtype={core.Adtype} id={idKey} call={AdInstanceName()}.DestroyAd()"))
			{
				ad?.DestroyAd();
				ad = null;
			}
		}

		public void AttachForAdInstance()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Attach {core.GroupName}",
				       () => $"adtype={core.Adtype} id={idKey}"))
			{
				if (string.IsNullOrEmpty(idKey))
				{
					NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Attach.Fail {core.GroupName}", () => "reason=idKey_empty");
					return;
				}

				if (ad == null)
				{
					NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Attach.Fail {core.GroupName}", () => "reason=ad_null");
					return;
				}

				ad.OnAdLoadedEvent += adInfo =>
				{
					UnityMainThreadDispatcher.EnqueueCallback(() =>
					{
						using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.Loaded {core.GroupName}",
							       () => $"adtype={core.Adtype} id={idKey} adapter={SafeAdapter(adInfo)}"))
						{
							core.OnAdLoadedEvent(SafeInfo(adInfo), SafeAdapter(adInfo));
						}
					});
				};

				ad.OnAdLoadFailedEvent += (adunit, errorCode, errorMessage) =>
				{
					UnityMainThreadDispatcher.EnqueueCallback(() =>
					{
						using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.LoadFailed {core.GroupName}",
							       () => $"adtype={core.Adtype} id={idKey} errNull={(errorMessage == null)}"))
						{
							core.OnAdLoadFailedEvent(errorCode, errorMessage ?? "IOS LoadFailed (null)");
						}
					});
				};

				ad.OnAdDisplayedEvent += adInfo =>
				{
					UnityMainThreadDispatcher.EnqueueCallback(() =>
					{
						using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.Displayed {core.GroupName}",
							       () => $"adtype={core.Adtype} id={idKey} adapter={SafeAdapter(adInfo)}"))
						{
							core.OnAdDisplayedEvent(SafeAdapter(adInfo));
						}
					});
				};

				ad.OnAdClicked += adInfo =>
				{
					UnityMainThreadDispatcher.EnqueueCallback(() =>
					{
						using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.Clicked {core.GroupName}",
							       () => $"adtype={core.Adtype} id={idKey} adapter={SafeAdapter(adInfo)}"))
						{
							core.OnAdClickedEvent(SafeAdapter(adInfo));
						}
					});
				};

				ad.OnPaidAdImpressionEvent += (adInfo, adValue) =>
				{
					UnityMainThreadDispatcher.EnqueueCallback(() =>
					{
						using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.Paid {core.GroupName}",
							       () => $"adtype={core.Adtype} id={idKey} adapter={SafeAdapter(adInfo)}"))
						{
							var rev = 0d;
							var currency = "USD";

							try
							{
								if (adValue != null)
								{
									rev = adValue.revenueMicros / 1000000d;
									currency = adValue.currencyCode;
								}
							}
							catch { }

							core.OnAdRevenuePaidEvent(rev, currency, SafeAdapter(adInfo));
						}
					});
				};

				ad.OnAdHiddenEvent += adInfo =>
				{
					UnityMainThreadDispatcher.EnqueueCallback(() =>
					{
						using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.Hidden {core.GroupName}",
							       () => $"adtype={core.Adtype} id={idKey} adapter={SafeAdapter(adInfo)}"))
						{
							var wasAwaitingTerminalCallback = core.IsAwaitingTerminalShowCallback;
							core.OnAdHiddenEvent(SafeAdapter(adInfo));

							if (wasAwaitingTerminalCallback && core.Info.IsRewarded)
								core.OnAdReceivedRewardEvent(SafeInfo(adInfo));
						}
					});
				};

				if (ad is IOSFSNativeInstance iosAd)
				{
					iosAd.OnAdImpressionEvent += HandleImpression;
					iosAd.OnAdShowFailedEvent += (adInfo, errorCode, errorMessage) =>
					{
						UnityMainThreadDispatcher.EnqueueCallback(() =>
						{
							using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.ShowFailed {core.GroupName}",
								       () => $"adtype={core.Adtype} id={idKey} err={errorMessage}"))
							{
								core.OnAdDisplayFailedEvent(SafeInfo(adInfo), errorMessage ?? "IOS ShowFailed (null)", errorCode: errorCode);
							}
						});
					};
				}

				if (ad is IOSInterstitialInstance interstitialAd)
				{
					interstitialAd.OnAdImpressionEvent += HandleImpression;
					interstitialAd.OnAdShowFailedEvent += (adInfo, errorCode, errorMessage) =>
					{
						UnityMainThreadDispatcher.EnqueueCallback(() =>
						{
							using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.ShowFailed {core.GroupName}",
								       () => $"adtype={core.Adtype} id={idKey} err={errorMessage}"))
							{
								core.OnAdDisplayFailedEvent(SafeInfo(adInfo), errorMessage ?? "IOS ShowFailed (null)", errorCode: errorCode);
							}
						});
					};
				}

				NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Attach.OK {core.GroupName}", () => $"id={idKey}");
			}
		}

		private void HandleImpression(AdInfo adInfo)
		{
			UnityMainThreadDispatcher.EnqueueCallback(() =>
			{
				using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_fs, $"CB.Impression {core.GroupName}",
					       () => $"adtype={core.Adtype} id={idKey} adapter={SafeAdapter(adInfo)} source={adInfo?.adSource}"))
				{
					core.OnAdImpressionEvent(SafeAdapter(adInfo));
				}
			});
		}

		private IFSInstance CreateAdInstance()
		{
			if (core.Info != null && core.Info.IsRewarded)
			{
				BridgeLog($"select instance group={core.GroupName} route=native_fullscreen reason=rewarded_legacy id={idKey}");
				NetFlowDebugSystem.Log(Layer.group, Module.ios_api_fs, $"Create.Route {core.GroupName}",
					() => "route=native_fullscreen reason=rewarded_legacy");
				return new IOSFSNativeInstance(new[] { idKey }, core.LayoutGroup);
			}

			var interstitials = core.Info?.AndroidInterstitials ?? default;
			if (core.Info is IOS_FAInfo faInfo && faInfo.MediationPriority == E_MediationPriority.Android)
			{
				BridgeLog(
					$"select instance group={core.GroupName} route=native_fullscreen mediationPriority={FormatPriority(faInfo.MediationPriority)} id={idKey} layoutGroup={core.LayoutGroup?.GroupName ?? ""}");
				NetFlowDebugSystem.Log(Layer.group, Module.ios_api_fs, $"Create.Route {core.GroupName}",
					() => $"route=native_fullscreen mediationPriority={FormatPriority(faInfo.MediationPriority)} layoutGroup={core.LayoutGroup?.GroupName ?? ""}");
				return new IOSFSNativeInstance(new[] { idKey }, core.LayoutGroup);
			}

			int bufferSize = interstitials.IsPreloadAd && interstitials.BufferSize > 0 ? interstitials.BufferSize : 1;
			string priorityLog = core.Info is IOS_FAInfo interInfo ? FormatPriority(interInfo.MediationPriority) : "unknown";
			BridgeLog(
				$"select instance group={core.GroupName} route=interstitial mediationPriority={priorityLog} id={idKey} preload={interstitials.IsPreloadAd} buffer={bufferSize}");
			NetFlowDebugSystem.Log(Layer.group, Module.ios_api_fs, $"Create.Route {core.GroupName}",
				() => $"route=interstitial mediationPriority={priorityLog} preload={interstitials.IsPreloadAd} buffer={bufferSize}");
			return new IOSInterstitialInstance(new[] { idKey }, bufferSize);
		}

		private static void BridgeLog(string message)
		{
			UnityEngine.Debug.Log($"{BridgeLogTag} {message}");
		}

		private static string FormatPriority(E_MediationPriority priority)
		{
			return $"{priority}({(int)priority})";
		}

		private string AdInstanceName()
		{
			return ad == null ? "null" : ad.GetType().Name;
		}

		private static string FormatLayoutNames(LayoutGroupConfig layoutGroup)
		{
			var layouts = layoutGroup?.Layouts;
			if (layouts == null || layouts.Length == 0)
				return "(empty)";

			var names = new string[layouts.Length];
			for (int i = 0; i < layouts.Length; i++)
				names[i] = layouts[i].Layout;

			return $"[{string.Join(",", names)}]";
		}

		private static string SafeInfo(AdInfo adInfo)
		{
			try { return adInfo != null ? adInfo.GetInfo() : ""; }
			catch { return ""; }
		}

		private static string SafeAdapter(AdInfo adInfo)
		{
			try { return adInfo != null ? (adInfo.mediationAdapter ?? "") : ""; }
			catch { return ""; }
		}
	}
}
