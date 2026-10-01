using System;
using System.Collections.Generic;
using BG_Library.NET.IOSSDK;

namespace AdsMultiplatform.Unity
{
	public static class NativeAdCallbackNames
	{
		public const string Idle = IOSNativeAdCallbackNames.Idle;
		public const string Loading = IOSNativeAdCallbackNames.Loading;
		public const string Loaded = IOSNativeAdCallbackNames.Loaded;
		public const string Displayable = IOSNativeAdCallbackNames.Displayable;
		public const string Failed = IOSNativeAdCallbackNames.Failed;
		public const string Shown = IOSNativeAdCallbackNames.Shown;
		public const string OnClosed = IOSNativeAdCallbackNames.OnClosed;
		public const string Impression = IOSNativeAdCallbackNames.Impression;
		public const string Clicked = IOSNativeAdCallbackNames.Clicked;
		public const string Paid = IOSNativeAdCallbackNames.Paid;
	}

	public sealed class FullscreenNativeAd : IOSNativeAdCallbackTarget, IOSNativeAdEventTarget
	{
		public string InstanceId { get; }
		public bool EnableReloadAfterShow { get; set; }
		public string LastCallbackName { get; private set; }
		public bool IsLoaded { get { return LastCallbackName == NativeAdCallbackNames.Loaded; } }

		public event Action<string> CallbackReceived;
		public event Action Loading;
		public event Action Loaded;
		public event Action Displayable;
		public event Action Failed;
		public event Action Shown;
		public event Action OnClosed;
		public event Action<IOSNativeAdEventData> Impression;
		public event Action<IOSNativeAdEventData> Clicked;
		public event Action<IOSNativeAdEventData> Paid;

		public FullscreenNativeAd(
			string instanceId = NativeAdBridge.DefaultFullscreenInstanceId,
			bool enableReloadAfterShow = false)
		{
			InstanceId = NativeAdBridge.SafeInstanceId(instanceId);
			EnableReloadAfterShow = enableReloadAfterShow;
			IOSNativeAdBridge.Register(InstanceId, this);
		}

		public void Load(string[] androidAdUnitIds = null, string[] iosAdUnitIds = null)
		{
			NativeAdBridge.LoadFullscreen(InstanceId, androidAdUnitIds, iosAdUnitIds, EnableReloadAfterShow);
		}

		public void Show(string layoutName = NativeAdBridge.DefaultLayoutName, double durationSeconds = 0.0)
		{
			NativeAdBridge.ShowFullscreen(InstanceId, layoutName, durationSeconds);
		}

		public void ShowCls(
			string layoutName = NativeAdBridge.FullscreenCls01,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			ShowOverlayCls(
				layoutName,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowCls(
			string[] layoutNames,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			ShowOverlayCls(
				layoutNames,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowOverlayCls(
			string layoutName = NativeAdBridge.FullscreenCls01,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			NativeAdBridge.ShowFullscreenCls(
				InstanceId,
				layoutName,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowOverlayCls(
			string[] layoutNames,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			NativeAdBridge.ShowFullscreenCls(
				InstanceId,
				layoutNames,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowNav(
			string layoutName = NativeAdBridge.FullscreenNav01,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			ShowOverlayNav(
				layoutName,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowNav(
			string[] layoutNames,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			ShowOverlayNav(
				layoutNames,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowOverlayNav(
			string layoutName = NativeAdBridge.FullscreenNav01,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			NativeAdBridge.ShowFullscreenNav(
				InstanceId,
				layoutName,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowOverlayNav(
			string[] layoutNames,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			NativeAdBridge.ShowFullscreenNav(
				InstanceId,
				layoutNames,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowProgress(
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			NativeAdBridge.ShowFullscreenProgress(
				InstanceId,
				NativeAdBridge.FullscreenProgress01,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void ShowProgressCls(
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			NativeAdBridge.ShowFullscreenProgressCls(
				InstanceId,
				NativeAdBridge.FullscreenProgressCls01,
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public void Hide()
		{
			NativeAdBridge.HideFullscreen(InstanceId);
		}

		public void Destroy()
		{
			NativeAdBridge.DestroyFullscreen(InstanceId);
			Dispose();
		}

		public void Dispose()
		{
			IOSNativeAdBridge.Unregister(InstanceId);
		}

		void IOSNativeAdCallbackTarget.HandleNativeCallback(string callbackName)
		{
			string normalizedCallbackName = NormalizeCallbackName(callbackName);
			LastCallbackName = normalizedCallbackName;
			CallbackReceived?.Invoke(normalizedCallbackName);

			switch (normalizedCallbackName)
			{
				case NativeAdCallbackNames.Loading:
					Loading?.Invoke();
					break;
				case NativeAdCallbackNames.Loaded:
					Loaded?.Invoke();
					break;
				case NativeAdCallbackNames.Displayable:
					Displayable?.Invoke();
					break;
				case NativeAdCallbackNames.Failed:
					Failed?.Invoke();
					break;
				case NativeAdCallbackNames.Shown:
					Shown?.Invoke();
					break;
				case NativeAdCallbackNames.OnClosed:
					OnClosed?.Invoke();
					break;
			}
		}

		void IOSNativeAdEventTarget.HandleNativeAdEvent(string eventName, IOSNativeAdEventData data)
		{
			switch (eventName)
			{
				case NativeAdCallbackNames.Impression:
					Impression?.Invoke(data);
					break;
				case NativeAdCallbackNames.Clicked:
					Clicked?.Invoke(data);
					break;
				case NativeAdCallbackNames.Paid:
					Paid?.Invoke(data);
					break;
			}
		}

		private static string NormalizeCallbackName(string callbackName)
		{
			return string.IsNullOrEmpty(callbackName) ? string.Empty : callbackName.Trim();
		}
	}

	public static class NativeAdBridge
	{
		public const string DefaultFullscreenInstanceId = IOSNativeAdBridge.DefaultFullscreenInstanceId;
		public const string DefaultLayoutName = IOSNativeAdBridge.DefaultLayoutName;
		public const string FullscreenOverlayClsMode = IOSNativeAdBridge.FullscreenOverlayClsMode;
		public const string FullscreenOverlayNavMode = IOSNativeAdBridge.FullscreenOverlayNavMode;
		public const string FullscreenOverlayProgressMode = IOSNativeAdBridge.FullscreenOverlayProgressMode;
		public const string FullscreenOverlayProgressClsMode = IOSNativeAdBridge.FullscreenOverlayProgressClsMode;
		public const string FullscreenCls01 = IOSNativeAdBridge.FullscreenCls01;
		public const string FullscreenCls02 = IOSNativeAdBridge.FullscreenCls02;
		public const string FullscreenCls03 = IOSNativeAdBridge.FullscreenCls03;
		public const string FullscreenCls03Left = IOSNativeAdBridge.FullscreenCls03Left;
		public const string FullscreenCls03Right = IOSNativeAdBridge.FullscreenCls03Right;
		public const string FullscreenNav01 = IOSNativeAdBridge.FullscreenNav01;
		public const string FullscreenNav02 = IOSNativeAdBridge.FullscreenNav02;
		public const string FullscreenNav03 = IOSNativeAdBridge.FullscreenNav03;
		public const string FullscreenNav01Left = IOSNativeAdBridge.FullscreenNav01Left;
		public const string FullscreenNav01Right = IOSNativeAdBridge.FullscreenNav01Right;
		public const string FullscreenNav02Left = IOSNativeAdBridge.FullscreenNav02Left;
		public const string FullscreenNav02Right = IOSNativeAdBridge.FullscreenNav02Right;
		public const string FullscreenNav03Left = IOSNativeAdBridge.FullscreenNav03Left;
		public const string FullscreenNav03Right = IOSNativeAdBridge.FullscreenNav03Right;
		public const string FullscreenProgress01 = IOSNativeAdBridge.FullscreenProgress01;
		public const string FullscreenProgressCls01 = IOSNativeAdBridge.FullscreenProgressCls01;
		public const string DefaultBannerInstanceId = IOSNativeAdBridge.DefaultBannerInstanceId;
		public const string DefaultPopupInstanceId = IOSNativeAdBridge.DefaultPopupInstanceId;
		public const string DefaultInterstitialInstanceId = IOSNativeAdBridge.DefaultInterstitialInstanceId;
		public const string DefaultBannerLayoutName = IOSNativeAdBridge.DefaultBannerLayoutName;
		public const string DefaultPopupLayoutName = IOSNativeAdBridge.DefaultPopupLayoutName;

		private const string IosTestAdUnitId = "ca-app-pub-3940256099942544/3986624511";
		private const string IosTestInterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";
		private static readonly Dictionary<string, InterstitialLoadConfig> InterstitialLoadConfigs =
			new Dictionary<string, InterstitialLoadConfig>();

		public static FullscreenNativeAd CreateFullscreen(
			string instanceId = DefaultFullscreenInstanceId,
			bool enableReloadAfterShow = false)
		{
			return new FullscreenNativeAd(instanceId, enableReloadAfterShow);
		}

		public static void Load(string androidAdUnitId = null, string iosAdUnitId = null)
		{
			LoadFullscreen(
				DefaultFullscreenInstanceId,
				ToArrayOrNull(androidAdUnitId),
				ToArrayOrNull(iosAdUnitId),
				false);
		}

		public static void Show(string androidAdUnitId = null, string iosAdUnitId = null)
		{
			ShowFullscreen(DefaultFullscreenInstanceId, DefaultLayoutName, 0.0);
		}

		public static void Hide()
		{
			HideFullscreen(DefaultFullscreenInstanceId);
		}

		public static void LoadFullscreen(
			string instanceId,
			string[] androidAdUnitIds = null,
			string[] iosAdUnitIds = null,
			bool enableReloadAfterShow = false)
		{
			IOSNativeAdBridge.LoadFullscreen(
				SafeInstanceId(instanceId),
				enableReloadAfterShow,
				SelectIosIds(androidAdUnitIds, iosAdUnitIds));
		}

		public static void ShowFullscreen(
			string instanceId,
			string layoutName = DefaultLayoutName,
			double durationSeconds = 0.0)
		{
			IOSNativeAdBridge.ShowFullscreen(SafeInstanceId(instanceId), layoutName, durationSeconds);
		}

		public static void ShowFullscreenWithOptions(
			string instanceId,
			string mode,
			string[] layoutNames,
			string fallbackLayoutName,
			double durationSeconds,
			string orientation,
			bool autoClose,
			bool pauseGameplay,
			bool enableAdComeback,
			bool showTCD,
			double delaySeconds,
			int timeUpCSeconds,
			string fallbackAdUnitId,
			global::NativeAssetVisibilityOptions assetVisibility = null)
		{
			IOSNativeAdBridge.ShowFullscreenWithOptions(
				SafeInstanceId(instanceId),
				mode,
				layoutNames,
				fallbackLayoutName,
				durationSeconds,
				orientation,
				autoClose,
				pauseGameplay,
				enableAdComeback,
				showTCD,
				delaySeconds,
				timeUpCSeconds,
				FirstNonEmpty(fallbackAdUnitId, IosTestAdUnitId),
				assetVisibility);
		}

		public static void ShowFullscreenCls(
			string instanceId,
			string layoutName = FullscreenCls01,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			ShowFullscreenCls(
				instanceId,
				new[] { layoutName },
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public static void ShowFullscreenCls(
			string instanceId,
			string[] layoutNames,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0,
			global::NativeAssetVisibilityOptions assetVisibility = null)
		{
			ShowFullscreenWithOptions(
				instanceId,
				FullscreenOverlayClsMode,
				layoutNames,
				FullscreenCls01,
				durationSeconds,
				orientation,
				false,
				pauseGameplay,
				enableAdComeback,
				showTCD,
				delaySeconds,
				timeUpCSeconds,
				IosTestAdUnitId,
				assetVisibility);
		}

		public static void ShowFullscreenNav(
			string instanceId,
			string layoutName = FullscreenNav01,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			ShowFullscreenNav(
				instanceId,
				new[] { layoutName },
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public static void ShowFullscreenNav(
			string instanceId,
			string[] layoutNames,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0,
			global::NativeAssetVisibilityOptions assetVisibility = null)
		{
			ShowFullscreenWithOptions(
				instanceId,
				FullscreenOverlayNavMode,
				layoutNames,
				FullscreenNav01,
				durationSeconds,
				orientation,
				false,
				pauseGameplay,
				enableAdComeback,
				showTCD,
				delaySeconds,
				timeUpCSeconds,
				IosTestAdUnitId,
				assetVisibility);
		}

		public static void ShowFullscreenProgress(
			string instanceId,
			string layoutName = FullscreenProgress01,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			ShowFullscreenProgress(
				instanceId,
				new[] { layoutName },
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public static void ShowFullscreenProgress(
			string instanceId,
			string[] layoutNames,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0,
			global::NativeAssetVisibilityOptions assetVisibility = null)
		{
			ShowFullscreenWithOptions(
				instanceId,
				FullscreenOverlayProgressMode,
				layoutNames,
				FullscreenProgress01,
				durationSeconds,
				orientation,
				false,
				pauseGameplay,
				enableAdComeback,
				showTCD,
				delaySeconds,
				timeUpCSeconds,
				IosTestAdUnitId,
				assetVisibility);
		}

		public static void ShowFullscreenProgressCls(
			string instanceId,
			string layoutName = FullscreenProgressCls01,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0)
		{
			ShowFullscreenProgressCls(
				instanceId,
				new[] { layoutName },
				durationSeconds,
				orientation,
				showTCD,
				pauseGameplay,
				enableAdComeback,
				delaySeconds,
				timeUpCSeconds);
		}

		public static void ShowFullscreenProgressCls(
			string instanceId,
			string[] layoutNames,
			double durationSeconds = 3.0,
			string orientation = "auto",
			bool showTCD = true,
			bool pauseGameplay = false,
			bool enableAdComeback = true,
			double delaySeconds = 0.0,
			int timeUpCSeconds = 0,
			global::NativeAssetVisibilityOptions assetVisibility = null)
		{
			ShowFullscreenWithOptions(
				instanceId,
				FullscreenOverlayProgressClsMode,
				layoutNames,
				FullscreenProgressCls01,
				durationSeconds,
				orientation,
				false,
				pauseGameplay,
				enableAdComeback,
				showTCD,
				delaySeconds,
				timeUpCSeconds,
				IosTestAdUnitId,
				assetVisibility);
		}

		public static void HideFullscreen(string instanceId)
		{
			IOSNativeAdBridge.HideFullscreen(SafeInstanceId(instanceId));
		}

		public static void DestroyFullscreen(string instanceId)
		{
			IOSNativeAdBridge.DestroyFullscreen(SafeInstanceId(instanceId));
		}

		public static bool CreateInterstitial(
			string instanceId = DefaultInterstitialInstanceId,
			string[] androidAdUnitIds = null,
			string[] iosAdUnitIds = null,
			int preloadBufferSize = 1,
			bool autoReload = true)
		{
			string safeInstanceId = SafeInterstitialInstanceId(instanceId);
			string adUnitIdsCsv = JoinAdUnitIds(SelectIosIds(androidAdUnitIds, iosAdUnitIds), IosTestInterstitialAdUnitId);
			InterstitialLoadConfigs[safeInstanceId] = new InterstitialLoadConfig(adUnitIdsCsv, preloadBufferSize, autoReload);
			return IOSNativeAdBridge.CreateInterstitial(safeInstanceId, adUnitIdsCsv, preloadBufferSize, autoReload);
		}

		public static bool LoadInterstitial(
			string instanceId = DefaultInterstitialInstanceId,
			int bufferSize = 1)
		{
			string safeInstanceId = SafeInterstitialInstanceId(instanceId);
			InterstitialLoadConfig config;
			if (!InterstitialLoadConfigs.TryGetValue(safeInstanceId, out config))
				config = new InterstitialLoadConfig(IosTestInterstitialAdUnitId, bufferSize, true);

			return IOSNativeAdBridge.LoadInterstitial(
				safeInstanceId,
				config.AdUnitIdsCsv,
				bufferSize < 1 ? config.PreloadBufferSize : bufferSize,
				config.AutoReload);
		}

		public static bool ShowInterstitial(
			string instanceId = DefaultInterstitialInstanceId,
			bool immersiveMode = true)
		{
			return IOSNativeAdBridge.ShowInterstitial(SafeInterstitialInstanceId(instanceId), immersiveMode);
		}

		public static bool DestroyInterstitial(string instanceId = DefaultInterstitialInstanceId)
		{
			string safeInstanceId = SafeInterstitialInstanceId(instanceId);
			InterstitialLoadConfigs.Remove(safeInstanceId);
			return IOSNativeAdBridge.DestroyInterstitial(safeInstanceId);
		}

		public static bool IsInterstitialReady(string instanceId = DefaultInterstitialInstanceId)
		{
			return IOSNativeAdBridge.IsInterstitialReady(SafeInterstitialInstanceId(instanceId));
		}

		public static void LoadBanner(
			string instanceId = DefaultBannerInstanceId,
			string adUnitIdsCsv = null,
			string layoutNamesCsv = DefaultBannerLayoutName,
			int timeReloadSeconds = 0,
			int timeCountdownSeconds = 5,
			int timeCollapseSeconds = 5)
		{
			IOSNativeAdBridge.LoadBanner(
				instanceId,
				FirstNonEmpty(adUnitIdsCsv, IosTestAdUnitId),
				layoutNamesCsv,
				timeReloadSeconds,
				timeCountdownSeconds,
				timeCollapseSeconds);
		}

		public static void ShowBanner(string instanceId = DefaultBannerInstanceId)
		{
			IOSNativeAdBridge.ShowBanner(instanceId);
		}

		public static bool ExpandBanner(string instanceId = DefaultBannerInstanceId, bool enableClick = true)
		{
			return IOSNativeAdBridge.ExpandBanner(instanceId, enableClick);
		}

		public static void CollapseBanner(string instanceId = DefaultBannerInstanceId)
		{
			IOSNativeAdBridge.CollapseBanner(instanceId);
		}

		public static void HideBanner(string instanceId = DefaultBannerInstanceId)
		{
			IOSNativeAdBridge.HideBanner(instanceId);
		}

		public static void DestroyBanner(string instanceId = DefaultBannerInstanceId)
		{
			IOSNativeAdBridge.DestroyBanner(instanceId);
		}

		public static void LoadPopup(
			string instanceId = DefaultPopupInstanceId,
			string adUnitIdsCsv = null,
			string layoutName = DefaultPopupLayoutName,
			int timeShowSeconds = 0,
			int timeReloadSeconds = 0,
			float xDp = 0f,
			float yDp = 0f,
			float adWidthDp = 300f,
			float adHeightDp = 250f,
			bool autoClose = false,
			bool enableCtrOverlay = false)
		{
			IOSNativeAdBridge.LoadPopup(
				instanceId,
				FirstNonEmpty(adUnitIdsCsv, IosTestAdUnitId),
				layoutName,
				timeShowSeconds,
				timeReloadSeconds,
				xDp,
				yDp,
				adWidthDp,
				adHeightDp,
				autoClose,
				enableCtrOverlay);
		}

		public static void UpdatePopupPlacement(
			string instanceId = DefaultPopupInstanceId,
			float xDp = 0f,
			float yDp = 0f,
			float adWidthDp = 300f,
			float adHeightDp = 250f)
		{
			IOSNativeAdBridge.UpdatePopupPlacement(instanceId, xDp, yDp, adWidthDp, adHeightDp);
		}

		public static void ShowPopup(string instanceId = DefaultPopupInstanceId)
		{
			IOSNativeAdBridge.ShowPopup(instanceId);
		}

		public static void ClosePopup(string instanceId = DefaultPopupInstanceId)
		{
			IOSNativeAdBridge.ClosePopup(instanceId);
		}

		public static void HidePopup(string instanceId = DefaultPopupInstanceId)
		{
			IOSNativeAdBridge.HidePopup(instanceId);
		}

		public static void StopPopup(string instanceId = DefaultPopupInstanceId)
		{
			IOSNativeAdBridge.StopPopup(instanceId);
		}

		public static void DestroyPopup(string instanceId = DefaultPopupInstanceId)
		{
			IOSNativeAdBridge.DestroyPopup(instanceId);
		}

		public static bool IsPopupReady(string instanceId = DefaultPopupInstanceId)
		{
			return IOSNativeAdBridge.IsPopupReady(instanceId);
		}

		public static bool IsPopupDisplayable(string instanceId = DefaultPopupInstanceId)
		{
			return IOSNativeAdBridge.IsPopupDisplayable(instanceId);
		}

		public static string PopupStateFor(string instanceId = DefaultPopupInstanceId)
		{
			return IOSNativeAdBridge.PopupStateFor(instanceId);
		}

		public static string SafeInstanceId(string instanceId)
		{
			return IOSNativeAdBridge.SafeInstanceId(instanceId);
		}

		private static string SafeInterstitialInstanceId(string instanceId)
		{
			return string.IsNullOrEmpty(instanceId) ? DefaultInterstitialInstanceId : instanceId.Trim();
		}

		private static string[] SelectIosIds(string[] androidAdUnitIds, string[] iosAdUnitIds)
		{
			return HasAnyValue(iosAdUnitIds) ? iosAdUnitIds : androidAdUnitIds;
		}

		private static string[] ToArrayOrNull(string value)
		{
			return string.IsNullOrWhiteSpace(value) ? null : new[] { value.Trim() };
		}

		private static bool HasAnyValue(string[] values)
		{
			if (values == null)
				return false;

			for (int i = 0; i < values.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(values[i]))
					return true;
			}

			return false;
		}

		private static string JoinAdUnitIds(string[] adUnitIds, string fallbackAdUnitId)
		{
			if (!HasAnyValue(adUnitIds))
				return fallbackAdUnitId;

			var normalized = new List<string>();
			for (int i = 0; i < adUnitIds.Length; i++)
			{
				string value = adUnitIds[i];
				if (string.IsNullOrWhiteSpace(value))
					continue;

				string trimmed = value.Trim();
				if (!normalized.Contains(trimmed))
					normalized.Add(trimmed);
			}

			return normalized.Count == 0 ? fallbackAdUnitId : string.Join(",", normalized);
		}

		private static string FirstNonEmpty(string value, string fallback)
		{
			return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
		}

		private struct InterstitialLoadConfig
		{
			public readonly string AdUnitIdsCsv;
			public readonly int PreloadBufferSize;
			public readonly bool AutoReload;

			public InterstitialLoadConfig(string adUnitIdsCsv, int preloadBufferSize, bool autoReload)
			{
				AdUnitIdsCsv = FirstNonEmpty(adUnitIdsCsv, IosTestInterstitialAdUnitId);
				PreloadBufferSize = preloadBufferSize < 1 ? 1 : preloadBufferSize;
				AutoReload = autoReload;
			}
		}
	}
}
