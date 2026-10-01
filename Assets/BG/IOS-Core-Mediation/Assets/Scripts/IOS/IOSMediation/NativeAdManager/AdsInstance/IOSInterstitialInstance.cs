using System;
using System.Collections.Generic;
using BG_Library.Common;
using BG_Library.NET.IOSSDK;

namespace BG_Library.NET.AndroidSDK
{
	public class IOSInterstitialInstance : IFSInstance, IOSNativeAdCallbackTarget, IOSNativeAdEventTarget
	{
		private static int counter;

		private readonly string id;
		private readonly string[] ids;
		private readonly string adUnitIdsCsv;
		private readonly string alias;
		private readonly int preloadBufferSize;
		private readonly bool autoReload;

		private bool isReady;
		private bool isDisposed;
		private bool showRequested;
		private bool suppressNextFailedLoad;

		public IOSInterstitialInstance(string[] ids, int preloadBufferSize = 1, bool autoReload = true)
		{
			this.ids = SanitizeIds(ids);
			id = this.ids.Length > 0 ? this.ids[0] : string.Empty;
			adUnitIdsCsv = this.ids.Length > 0 ? string.Join(",", this.ids) : string.Empty;
			this.preloadBufferSize = preloadBufferSize < 1 ? 1 : preloadBufferSize;
			this.autoReload = autoReload;
			alias = "ios_interstitial_" + (++counter);

			IOSNativeAdBridge.Register(alias, this);
			IOSNativeAdBridge.CreateInterstitial(alias, adUnitIdsCsv, this.preloadBufferSize, this.autoReload);
		}

		public event Action<AdInfo> OnAdLoadedEvent;
		public event Action<string, int, string> OnAdLoadFailedEvent;
		public event Action<AdInfo> OnAdDisplayedEvent;
		public event Action<AdInfo> OnAdClicked;
		public event Action<AdInfo, AdValue> OnPaidAdImpressionEvent;
		/// <summary>Raised when the Google Mobile Ads SDK records the impression (adDidRecordImpression).</summary>
		public event Action<AdInfo> OnAdImpressionEvent;
		public event Action<AdInfo> OnAdHiddenEvent;
		public event Action<AdInfo, int, string> OnAdShowFailedEvent;

		public void LoadAd()
		{
			if (isDisposed)
				return;

			isReady = false;
			showRequested = false;
			suppressNextFailedLoad = false;

			bool started = IOSNativeAdBridge.LoadInterstitial(alias, adUnitIdsCsv, preloadBufferSize, autoReload);
#if UNITY_IOS && !UNITY_EDITOR
			if (!started)
				OnAdLoadFailedEvent?.Invoke(id, -1, "IOS interstitial load failed.");
#endif
		}

		public void ShowAd()
		{
			if (isDisposed)
				throw new ObjectDisposedException(nameof(IOSInterstitialInstance));

#if UNITY_IOS && !UNITY_EDITOR
			if (!isReady && IOSNativeAdBridge.IsInterstitialReady(alias))
				isReady = true;
#endif

			if (!isReady)
				throw new InvalidOperationException("IOS interstitial ad is not ready.");

			showRequested = true;
			isReady = false;

			bool shown = IOSNativeAdBridge.ShowInterstitial(alias);
			if (!shown)
			{
				showRequested = false;
				suppressNextFailedLoad = true;
				OnAdShowFailedEvent?.Invoke(CreateAdInfo(), -1, "IOS interstitial show failed.");
			}
		}

		public void DestroyAd()
		{
			if (isDisposed)
				return;

			isDisposed = true;
			isReady = false;
			showRequested = false;
			suppressNextFailedLoad = false;
			IOSNativeAdBridge.DestroyInterstitial(alias);
			IOSNativeAdBridge.Unregister(alias);
		}

		public bool IsReady()
		{
			if (isDisposed)
				return false;

#if UNITY_IOS && !UNITY_EDITOR
			if (IOSNativeAdBridge.IsInterstitialReady(alias))
				isReady = true;
#endif
			return isReady;
		}

		public void HandleNativeCallback(string callbackName)
		{
			string state = string.IsNullOrEmpty(callbackName) ? string.Empty : callbackName.Trim();
			var info = CreateAdInfo();

			switch (state)
			{
				case IOSNativeAdCallbackNames.Loading:
					isReady = false;
					suppressNextFailedLoad = false;
					break;

				case IOSNativeAdCallbackNames.Loaded:
					isReady = true;
					showRequested = false;
					suppressNextFailedLoad = false;
					OnAdLoadedEvent?.Invoke(info);
					break;

				case IOSNativeAdCallbackNames.Failed:
					isReady = false;
					if (showRequested)
					{
						showRequested = false;
						OnAdShowFailedEvent?.Invoke(info, -1, "IOS interstitial show failed.");
					}
					else
					{
						if (suppressNextFailedLoad)
						{
							suppressNextFailedLoad = false;
						}
						else
						{
							OnAdLoadFailedEvent?.Invoke(id, -1, "IOS interstitial load failed.");
						}
					}
					break;

				case IOSNativeAdCallbackNames.Shown:
					isReady = false;
					showRequested = false;
					suppressNextFailedLoad = false;
					OnAdDisplayedEvent?.Invoke(info);
					break;

				case IOSNativeAdCallbackNames.OnClosed:
					isReady = false;
					showRequested = false;
					suppressNextFailedLoad = false;
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
