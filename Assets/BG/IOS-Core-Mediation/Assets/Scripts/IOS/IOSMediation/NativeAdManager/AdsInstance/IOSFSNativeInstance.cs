using System;
using System.Collections.Generic;
using BG_Library.Common;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.IOSSDK;
using BG_Library.NET.Mediation.Android;

namespace BG_Library.NET.AndroidSDK
{
	public class IOSFSNativeInstance : IFSInstance, IOSNativeAdCallbackTarget, IOSNativeAdEventTarget
	{
		private static int counter;

		private readonly string id;
		private readonly string[] ids;
		private readonly LayoutGroupConfig layoutGroup;
		private readonly string orientation;
		private readonly LayoutGroupPicker layoutGroupPicker;
		private readonly string alias;

		private bool isReady;
		private bool isDisposed;
		private bool showRequested;

		public IOSFSNativeInstance(string[] ids, LayoutGroupConfig layoutGroup, string orientation = null)
		{
			this.ids = SanitizeIds(ids);
			id = this.ids.Length > 0 ? this.ids[0] : string.Empty;
			this.layoutGroup = layoutGroup;
			this.orientation = string.IsNullOrEmpty(orientation) ? Master.GetOrientationString() : orientation;
			layoutGroupPicker = new LayoutGroupPicker(layoutGroup);
			alias = "ios_fullscreen_" + (++counter);

			IOSNativeAdBridge.Register(alias, this);
		}

		public event Action<AdInfo> OnAdLoadedEvent;
		public event Action<string, int, string> OnAdLoadFailedEvent;
		public event Action<AdInfo> OnAdDisplayedEvent;
		public event Action<AdInfo> OnAdClicked;
		public event Action<AdInfo, AdValue> OnPaidAdImpressionEvent;
		/// <summary>Raised when the Google Mobile Ads SDK records the impression (nativeAdDidRecordImpression).</summary>
		public event Action<AdInfo> OnAdImpressionEvent;
		public event Action<AdInfo> OnAdHiddenEvent;
		public event Action<AdInfo, int, string> OnAdShowFailedEvent;

		public void LoadAd()
		{
			if (isDisposed)
				return;

			isReady = false;
			showRequested = false;
			IOSNativeAdBridge.LoadFullscreen(alias, false, ids);
		}

		public void ShowAd()
		{
			if (isDisposed)
				throw new ObjectDisposedException(nameof(IOSFSNativeInstance));

			if (!isReady)
				throw new InvalidOperationException("IOS fullscreen native ad is not ready.");

			if (!layoutGroupPicker.TryGetLayout(string.Empty, out var layoutConfig))
				throw new NullReferenceException("Cant get layout config");

			showRequested = true;
			isReady = false;
			ShowLayout(layoutConfig);
		}

		public void DestroyAd()
		{
			if (isDisposed)
				return;

			isDisposed = true;
			isReady = false;
			showRequested = false;
			IOSNativeAdBridge.DestroyFullscreen(alias);
			IOSNativeAdBridge.Unregister(alias);
		}

		public bool IsReady() => !isDisposed && isReady;

		public void HandleNativeCallback(string callbackName)
		{
			string state = string.IsNullOrEmpty(callbackName) ? string.Empty : callbackName.Trim();
			var info = CreateAdInfo();

			switch (state)
			{
				case IOSNativeAdCallbackNames.Loading:
					isReady = false;
					break;

				case IOSNativeAdCallbackNames.Loaded:
					isReady = true;
					showRequested = false;
					OnAdLoadedEvent?.Invoke(info);
					break;

				case IOSNativeAdCallbackNames.Failed:
					isReady = false;
					if (showRequested)
					{
						showRequested = false;
						OnAdShowFailedEvent?.Invoke(info, -1, "IOS fullscreen native show failed.");
					}
					else
					{
						OnAdLoadFailedEvent?.Invoke(id, -1, "IOS fullscreen native load failed.");
					}
					break;

				case IOSNativeAdCallbackNames.Shown:
					isReady = false;
					showRequested = false;
					OnAdDisplayedEvent?.Invoke(info);
					break;

				case IOSNativeAdCallbackNames.OnClosed:
					isReady = false;
					showRequested = false;
					OnAdHiddenEvent?.Invoke(info);
					break;
			}
		}

		public void HandleNativeAdEvent(string eventName, IOSNativeAdEventData data)
		{
			if (isDisposed)
				return;

			var info = CreateAdInfo(data);
			switch (eventName)
			{
				case IOSNativeAdCallbackNames.Impression:
					OnAdImpressionEvent?.Invoke(info);
					break;

				case IOSNativeAdCallbackNames.Clicked:
					OnAdClicked?.Invoke(info);
					break;

				case IOSNativeAdCallbackNames.Paid:
					OnPaidAdImpressionEvent?.Invoke(info, CreateAdValue(data));
					break;
			}
		}

		private void ShowLayout(LayoutConfig layoutConfig)
		{
			var layoutNames = new[] { layoutConfig.Layout };
			var durationSeconds = layoutConfig.LayoutTime;
			var delaySeconds = layoutConfig.Delay;
			var timeUpCSeconds = layoutConfig.TimeUpC;
			var assetVisibility = layoutGroup?.ResolveAssetVisibility(layoutConfig.AssetConfigName);

			if (LayoutNamesContain(layoutNames, "prgs_cls"))
			{
				ShowWithOptions(
					IOSNativeAdBridge.FullscreenOverlayProgressClsMode,
					layoutNames,
					IOSNativeAdBridge.FullscreenProgressCls01,
					durationSeconds,
					delaySeconds,
					timeUpCSeconds,
					layoutConfig,
					assetVisibility);
				return;
			}

			if (LayoutNamesContain(layoutNames, "prgs"))
			{
				ShowWithOptions(
					IOSNativeAdBridge.FullscreenOverlayProgressMode,
					layoutNames,
					IOSNativeAdBridge.FullscreenProgress01,
					durationSeconds,
					delaySeconds,
					timeUpCSeconds,
					layoutConfig,
					assetVisibility);
				return;
			}

			if (LayoutNamesContain(layoutNames, "cls"))
			{
				ShowWithOptions(
					IOSNativeAdBridge.FullscreenOverlayClsMode,
					layoutNames,
					IOSNativeAdBridge.FullscreenCls01,
					durationSeconds,
					delaySeconds,
					timeUpCSeconds,
					layoutConfig,
					assetVisibility);
				return;
			}

			if (LayoutNamesContain(layoutNames, "nav"))
			{
				ShowWithOptions(
					IOSNativeAdBridge.FullscreenOverlayNavMode,
					layoutNames,
					IOSNativeAdBridge.FullscreenNav01,
					durationSeconds,
					delaySeconds,
					timeUpCSeconds,
					layoutConfig,
					assetVisibility);
				return;
			}

			IOSNativeAdBridge.ShowFullscreen(alias, layoutConfig.Layout, durationSeconds);
		}

		private void ShowWithOptions(
			string mode,
			string[] layoutNames,
			string fallbackLayoutName,
			double durationSeconds,
			double delaySeconds,
			int timeUpCSeconds,
			LayoutConfig layoutConfig,
			global::NativeAssetVisibilityOptions assetVisibility)
		{
			IOSNativeAdBridge.ShowFullscreenWithOptions(
				alias,
				mode,
				layoutNames,
				fallbackLayoutName,
				durationSeconds,
				orientation,
				false,
				layoutConfig.PauseGameplay,
				!layoutConfig.DisableAdComeback,
				layoutConfig.ShowTCD,
				delaySeconds,
				timeUpCSeconds,
				id,
				assetVisibility);
		}

		private AdInfo CreateAdInfo()
		{
			return new AdInfo
			{
				adUnitId = id,
				mediationAdapter = BG_ConstValue.mediation_ios,
				responseId = alias,
				adSource = BG_ConstValue.mediation_ios,
				adSourceId = string.Empty
			};
		}

		private AdInfo CreateAdInfo(IOSNativeAdEventData data)
		{
			return new AdInfo
			{
				adUnitId = string.IsNullOrEmpty(data.adUnitId) ? id : data.adUnitId,
				mediationAdapter = string.IsNullOrEmpty(data.mediationAdapter) ? BG_ConstValue.mediation_ios : data.mediationAdapter,
				responseId = string.IsNullOrEmpty(data.responseId) ? alias : data.responseId,
				adSource = string.IsNullOrEmpty(data.adSourceName) ? BG_ConstValue.mediation_ios : data.adSourceName,
				adSourceId = data.adSourceId ?? string.Empty
			};
		}

		private static AdValue CreateAdValue(IOSNativeAdEventData data)
		{
			return new AdValue
			{
				revenueMicros = data.valueMicros,
				currencyCode = data.SafeCurrencyCode,
				precisionType = data.precision
			};
		}

		private static bool LayoutNamesContain(string[] layoutNames, string token)
		{
			if (layoutNames == null || string.IsNullOrEmpty(token))
				return false;

			for (int i = 0; i < layoutNames.Length; i++)
			{
				var layoutName = layoutNames[i];
				if (!string.IsNullOrEmpty(layoutName) &&
				    layoutName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
					return true;
			}

			return false;
		}

		private static string[] SanitizeIds(string[] source)
		{
			if (source == null || source.Length == 0)
				return Array.Empty<string>();

			var result = new List<string>(source.Length);
			for (int i = 0; i < source.Length; i++)
			{
				string value = source[i];
				if (string.IsNullOrWhiteSpace(value))
					continue;

				string trimmed = value.Trim();
				if (!result.Contains(trimmed))
					result.Add(trimmed);
			}

			return result.ToArray();
		}
	}
}
