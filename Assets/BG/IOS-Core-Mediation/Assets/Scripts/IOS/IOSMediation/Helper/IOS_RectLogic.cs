using System;
using System.Text;
using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.IOSSDK;
using BG_Library.NET.Tracking;
using UnityEngine;

namespace BG_Library.NET.Mediation.IOS
{
	public class IOS_RectLogic<T> : IOSNativeAdCallbackTarget, IOSNativeAdEventTarget
		where T : IOS_RectBaseInfo
	{
		private const float DefaultPopupX = 16f;
		private const float DefaultPopupY = 96f;
		private const float DefaultPopupWidth = 320f;
		private const float DefaultPopupHeight = 280f;

		private readonly IOS_RectGroupController<T> core;
		private readonly string idKey;
		private readonly string instanceId;

		private bool isAttached;
		private bool hasFirstLoadResolved;
		private bool hasBridgeInstance;
		private bool isPopupDisplayable;
		private bool popupLayoutUpdated;
		private float popupX;
		private float popupY;
		private float popupLayoutWidthDp;
		private float popupLayoutHeightDp;

		public bool HasBridgeInstance => hasBridgeInstance;

		public IOS_RectLogic(IOS_RectGroupController<T> core)
		{
			this.core = core;
			idKey = this.core.Id;
			instanceId = BuildInstanceId(this.core);
			popupX = DefaultPopupX;
			popupY = DefaultPopupY;
			popupLayoutWidthDp = DefaultPopupWidth;
			popupLayoutHeightDp = DefaultPopupHeight;
		}

		public void CreateAndLoad()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_rect, $"SDK.CreateAndLoad {core.GroupName}",
				() => $"adtype={core.Adtype} id={idKey} instance={instanceId}"))
			{
				if (string.IsNullOrEmpty(idKey))
				{
					core.Message(() => "CreateAndLoad fail. idKey empty", LogLevel.Warning);
					NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "id_empty=true");
					return;
				}

				ListenToAdEvents();
				isPopupDisplayable = false;

				if (ShouldBlockPostInitPopupReload())
				{
					NetFlowDebugSystem.Log(Layer.group, Module.ios_api_rect, $"SDK.RequestBlocked {core.GroupName}",
						() => $"adtype={core.Adtype} id={idKey} reason=DisablePostInitReload");
					return;
				}

				switch (core.Info)
				{
					case IOS_BNInfo bannerInfo:
						IOSNativeAdBridge.LoadBanner(
							instanceId,
							SafeCsvOrId(bannerInfo.IdsCsv),
							SafeCsvOrDefault(bannerInfo.LayoutsCsv, IOSNativeAdBridge.DefaultBannerLayoutName),
							bannerInfo.TimeReload);
						hasBridgeInstance = true;
						break;

					case IOS_PUInfo popupInfo:
						IOSNativeAdBridge.LoadPopup(
							instanceId,
							SafeCsvOrId(popupInfo.IdsCsv),
							ResolvePopupLayout(popupInfo),
							popupInfo.TimeShow,
							popupInfo.DisablePostInitReload ? 0 : popupInfo.TimeReload,
							popupX,
							popupY,
							popupLayoutWidthDp,
							popupLayoutHeightDp,
							autoClose: false,
							enableCtrOverlay: false);
						hasBridgeInstance = true;
						break;

					default:
						core.Message(() => $"CreateAndLoad fail. Unsupported info type {core.Info?.GetType().Name}", LogLevel.Warning);
						break;
				}
			}
		}

		public void ShowAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_rect, $"SDK.Show {core.GroupName}",
				() => $"adtype={core.Adtype} id={idKey} instance={instanceId} hasBridge={hasBridgeInstance}"))
			{
				if (!hasBridgeInstance)
				{
					core.Message(() => "ShowAd fail. Bridge instance not loaded", LogLevel.Warning);
					NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "bridge_instance=false");
					return;
				}

				if (core.Info is IOS_PUInfo)
				{
					if (!IOSNativeAdBridge.IsPopupDisplayable(instanceId))
					{
						IOSNativeAdBridge.NotifyPopupShowSkipped(instanceId, $"state={IOSNativeAdBridge.PopupStateFor(instanceId)}");
						return;
					}

					IOSNativeAdBridge.ShowPopup(instanceId);
					return;
				}

				IOSNativeAdBridge.ShowBanner(instanceId);
			}
		}

		public void HideAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_rect, $"SDK.Hide {core.GroupName}",
				() => $"adtype={core.Adtype} id={idKey} instance={instanceId} hasBridge={hasBridgeInstance}"))
			{
				if (!hasBridgeInstance)
				{
					core.Message(() => "HideAd fail. Bridge instance not loaded", LogLevel.Warning);
					NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "bridge_instance=false");
					return;
				}

				if (core.Info is IOS_PUInfo)
				{
					IOSNativeAdBridge.HidePopup(instanceId);
					return;
				}

				IOSNativeAdBridge.HideBanner(instanceId);
			}
		}

		public bool ExpandAd(bool enableClick = true)
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_rect, $"SDK.Expand {core.GroupName}",
				() => $"adtype={core.Adtype} id={idKey} instance={instanceId} hasBridge={hasBridgeInstance}"))
			{
				if (core.Info is not IOS_BNInfo)
					return false;

				if (!hasBridgeInstance)
				{
					core.Message(() => "ExpandAd fail. Bridge instance not loaded", LogLevel.Warning);
					NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "bridge_instance=false");
					return false;
				}

				return IOSNativeAdBridge.ExpandBanner(instanceId, enableClick);
			}
		}

		public void DestroyAd()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_rect, $"SDK.Destroy {core.GroupName}",
				() => $"adtype={core.Adtype} id={idKey} instance={instanceId} hasBridge={hasBridgeInstance}"))
			{
				if (core.Info is IOS_PUInfo)
					IOSNativeAdBridge.DestroyPopup(instanceId);
				else
					IOSNativeAdBridge.DestroyBanner(instanceId);

				IOSNativeAdBridge.Unregister(instanceId);
				isAttached = false;
				hasFirstLoadResolved = false;
				hasBridgeInstance = false;
				isPopupDisplayable = false;
			}
		}

		public void UpdatePUPosition(float xDp, float yDp, float w, float h)
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.ios_api_rect, $"SDK.UpdatePUPos {core.GroupName}",
				() => $"adtype={core.Adtype} x={xDp:0.##} y={yDp:0.##} w={w:0.##} h={h:0.##}"))
			{
				if (core.Info is not IOS_PUInfo popupInfo)
					return;

				popupLayoutUpdated = true;
				popupLayoutWidthDp = w;
				popupLayoutHeightDp = h;
				popupX = xDp;
				popupY = yDp;

				IOSNativeAdBridge.UpdatePopupPlacement(
					instanceId,
					popupX,
					popupY,
					popupLayoutWidthDp,
					popupLayoutHeightDp);
			}
		}

		public bool TryValidatePopupLayoutBeforeShow(out TrackingReason reason, out string detail)
		{
			reason = default;
			detail = string.Empty;

			if (core.Info is not IOS_PUInfo)
				return true;

			if (!popupLayoutUpdated)
			{
				reason = TrackingReason.UpdatePositionRequired;
				detail = "Popup layout has not been updated yet.";
				return false;
			}

			if (popupLayoutWidthDp <= 0f || popupLayoutHeightDp <= 0f)
			{
				reason = TrackingReason.InvalidLayoutSize;
				detail = $"Popup layout size invalid. w={popupLayoutWidthDp:0.###}, h={popupLayoutHeightDp:0.###}";
				return false;
			}

			if (!IsFinitePopupLayout(popupX, popupY, popupLayoutWidthDp, popupLayoutHeightDp))
			{
				reason = TrackingReason.InvalidLayoutSize;
				detail = $"Popup layout has invalid numeric value. x={popupX:0.###}, y={popupY:0.###}, w={popupLayoutWidthDp:0.###}, h={popupLayoutHeightDp:0.###}";
				return false;
			}

			if (!HasValidScreen(out var screenWidthDp, out var screenHeightDp))
			{
				reason = TrackingReason.InvalidLayoutSize;
				detail = "Popup screen size invalid.";
				return false;
			}

			if (!IsPopupPlacementVisible(screenWidthDp, screenHeightDp))
			{
				reason = TrackingReason.InvalidLayoutSize;
				detail = $"Popup placement outside viewport. x={popupX:0.###}, y={popupY:0.###}, w={popupLayoutWidthDp:0.###}, h={popupLayoutHeightDp:0.###}, screen=({screenWidthDp:0.###},{screenHeightDp:0.###})";
				return false;
			}

			if (!hasBridgeInstance)
			{
				reason = TrackingReason.NotReady;
				detail = "Popup bridge instance not loaded.";
				return false;
			}

			if (!isPopupDisplayable && !IOSNativeAdBridge.IsPopupDisplayable(instanceId))
			{
				reason = TrackingReason.NotReady;
				detail = $"Popup not displayable. state={IOSNativeAdBridge.PopupStateFor(instanceId)}";
				return false;
			}

			return true;
		}

		public string GetIdKeySafe() => string.IsNullOrEmpty(idKey) ? "(empty)" : idKey;

		public string GetBridgeInstanceStateShort()
		{
			if (!hasBridgeInstance) return "none";
			return core.Info is IOS_PUInfo ? "popup" : "banner";
		}

		public string GetAttachStateShort()
		{
			if (string.IsNullOrEmpty(idKey)) return "idEmpty";
			return isAttached ? "attached" : "notAttached";
		}

		public void HandleNativeCallback(string callbackName)
		{
			UnityMainThreadDispatcher.EnqueueCallback(() =>
			{
				using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_rect, $"CB.{callbackName} {core.GroupName}",
					() => $"adtype={core.Adtype} id={idKey} instance={instanceId}"))
				{
					switch (callbackName)
					{
						case IOSNativeAdCallbackNames.Loading:
							core.Message(() => "IOS native loading");
							break;

						case IOSNativeAdCallbackNames.Loaded:
							hasFirstLoadResolved = true;
							if (core.Info is IOS_PUInfo)
							{
								core.Message(() => $"IOS popup loaded. state={IOSNativeAdBridge.PopupStateFor(instanceId)}");
								break;
							}
							core.OnAdLoadedEvent(string.Empty, BG_ConstValue.mediation_ios);
							break;

						case IOSNativeAdCallbackNames.Displayable:
							hasFirstLoadResolved = true;
							isPopupDisplayable = true;
							core.OnAdLoadedEvent(string.Empty, BG_ConstValue.mediation_ios);
							break;

						case IOSNativeAdCallbackNames.Failed:
							isPopupDisplayable = false;
							HandleLoadFailed();
							if (core.IsShowing)
								core.Hide();
							break;

						case IOSNativeAdCallbackNames.Shown:
							isPopupDisplayable = false;
							core.Message(() => "IOS native shown");
							break;

						case IOSNativeAdCallbackNames.OnClosed:
							isPopupDisplayable = false;
							if (core.IsShowing)
								core.Hide();
							break;
					}
				}
			});
		}

		public void HandleNativeAdEvent(string eventName, IOSNativeAdEventData data)
		{
			UnityMainThreadDispatcher.EnqueueCallback(() =>
			{
				string adapter = string.IsNullOrEmpty(data?.mediationAdapter) ? BG_ConstValue.mediation_ios : data.mediationAdapter;
				using (NetFlowDebugSystem.FlowNew(Layer.group, Module.ios_api_rect, $"CB.{eventName} {core.GroupName}",
					() => $"adtype={core.Adtype} id={idKey} instance={instanceId} adapter={adapter} source={data?.adSourceName}"))
				{
					switch (eventName)
					{
						case IOSNativeAdCallbackNames.Impression:
							core.OnAdImpressionEvent(adapter);
							break;

						case IOSNativeAdCallbackNames.Clicked:
							core.OnAdClickedEvent(adapter);
							break;

						case IOSNativeAdCallbackNames.Paid:
							if (data == null)
								break;
							core.OnAdRevenuePaidEvent(data.Revenue, data.SafeCurrencyCode, adapter);
							break;
					}
				}
			});
		}

		private void ListenToAdEvents()
		{
			using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Attach {core.GroupName}",
				() => $"adtype={core.Adtype} id={idKey} instance={instanceId}"))
			{
				if (isAttached)
				{
					core.Message(() => "ListenToAdEvents skip. Already attached", LogLevel.Warning);
					NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "already_attached=true");
					return;
				}

				if (string.IsNullOrEmpty(idKey))
				{
					core.Message(() => "ListenToAdEvents fail. idKey empty", LogLevel.Warning);
					NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "id_empty=true");
					return;
				}

				IOSNativeAdBridge.Register(instanceId, this);
				isAttached = true;

				core.Message(() => $"ListenToAdEvents success. instance={instanceId}");
				NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Result {core.GroupName}", () => "attach_success=true");
			}
		}

		private void HandleLoadFailed()
		{
			var shouldDestroyAfterFirstFail = IsPopupFirstLoadFailDestroyCase();
			hasFirstLoadResolved = true;
			core.OnAdLoadFailedEvent(-1, "IOS native ad load failed.");

			if (shouldDestroyAfterFirstFail)
			{
				NetFlowDebugSystem.Log(Layer.group, Module.ios_api_rect, $"Popup.DestroyAfterFirstFail {core.GroupName}",
					() => $"adtype={core.Adtype} id={idKey} reason=DisablePostInitReload");
				DestroyAd();
			}
		}

		private bool IsPopupFirstLoadFailDestroyCase()
		{
			return !hasFirstLoadResolved &&
			       core.Info is IOS_PUInfo puInfo &&
			       puInfo.DisablePostInitReload;
		}

		private bool ShouldBlockPostInitPopupReload()
		{
			return hasFirstLoadResolved &&
			       core.Info is IOS_PUInfo puInfo &&
			       puInfo.DisablePostInitReload;
		}

		private string SafeCsvOrId(string csv)
		{
			if (!string.IsNullOrWhiteSpace(csv))
				return csv;

			return string.IsNullOrEmpty(idKey) ? string.Empty : idKey;
		}

		private static string SafeCsvOrDefault(string csv, string fallback)
		{
			return string.IsNullOrWhiteSpace(csv) ? fallback : csv;
		}

		private static string ResolvePopupLayout(IOS_PUInfo info)
		{
			if (!string.IsNullOrWhiteSpace(info.Layout))
				return info.Layout;

			var sourceLayouts = info.AdSourceLayouts;
			if (sourceLayouts != null)
			{
				for (int i = 0; i < sourceLayouts.Length; i++)
				{
					if (!string.IsNullOrWhiteSpace(sourceLayouts[i].Layout))
						return sourceLayouts[i].Layout;
				}
			}

			var layouts = info.Layouts;
			if (layouts != null)
			{
				for (int i = 0; i < layouts.Length; i++)
				{
					if (!string.IsNullOrWhiteSpace(layouts[i]))
						return layouts[i];
				}
			}

			return IOSNativeAdBridge.DefaultPopupLayoutName;
		}

		private static bool IsFinitePopupLayout(float xDp, float yDp, float wDp, float hDp)
		{
			return IsFinite(xDp) &&
			       IsFinite(yDp) &&
			       IsFinite(wDp) &&
			       IsFinite(hDp);
		}

		private static bool HasValidScreen(out float screenWidthDp, out float screenHeightDp)
		{
			screenWidthDp = 0f;
			screenHeightDp = 0f;

			var density = Master.GetScreenDensity();
			if (!IsFinite(density) || density <= 0f)
				return false;

			if (Screen.width <= 0 || Screen.height <= 0)
				return false;

			screenWidthDp = Screen.width / density;
			screenHeightDp = Screen.height / density;
			return IsFinite(screenWidthDp) &&
			       IsFinite(screenHeightDp) &&
			       screenWidthDp > 0f &&
			       screenHeightDp > 0f;
		}

		private bool IsPopupPlacementVisible(float screenWidthDp, float screenHeightDp)
		{
			return popupX + popupLayoutWidthDp > 0f &&
			       popupY + popupLayoutHeightDp > 0f &&
			       popupX < screenWidthDp &&
			       popupY < screenHeightDp;
		}

		private static bool IsFinite(float value)
		{
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}

		private static string BuildInstanceId(IOS_RectGroupController<T> core)
		{
			var prefix = core.Info is IOS_PUInfo ? "ios_popup" : "ios_banner";
			var raw = $"{prefix}_{core.GroupName}";
			var sb = new StringBuilder(raw.Length);

			for (int i = 0; i < raw.Length; i++)
			{
				char c = char.ToLowerInvariant(raw[i]);
				bool valid =
					(c >= 'a' && c <= 'z') ||
					(c >= '0' && c <= '9') ||
					c == '_' ||
					c == '-';

				sb.Append(valid ? c : '_');
			}

			return sb.Length == 0 ? prefix : sb.ToString();
		}
	}
}
