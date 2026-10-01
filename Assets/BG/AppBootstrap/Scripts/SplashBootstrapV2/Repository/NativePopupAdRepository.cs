using System;
using System.Threading;
using BG_Library.NET;
using BG_Library.NET.API;
using BG_Library.Common;
#if boostrap_ios && UNITY_IOS
using BG_Library.NET.IOSSDK;
#endif
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AppBootstrap.Splash
{
    public class NativePopupAdRepository : IAdRequester
    {
        private readonly string _adUnitId;
        private AdStatus _status;
        private string _position;
#if boostrap_ios && UNITY_IOS
        private IosPopupNativeInstance _instance;
#else
        private ONAPopupInstance _instance;
#endif
        private PULayout _layout;
        
        public string Position => _position;
        public AdStatus Status => _status;
        public Action<AdStatus> OnAdStatusChanged;
        /// <summary>Ad SDK recorded the impression (currently raised by the iOS bridge only).</summary>
        public Action<NativeAdInfo> OnAdImpression;

        public NativePopupAdRepository(string position, string adUnitId, PULayout layout)
        {
            _adUnitId = adUnitId;
            _position = position;
            _layout = layout;
#if boostrap_ios && UNITY_IOS
            _instance = new IosPopupNativeInstance(_adUnitId, _layout);
#else
            _instance = new ONAPopupInstance(new AndroidNAConfig(new string[] {_adUnitId}));
#endif
            
            _instance.OnONAPopupLoaded += OnAdLoaded;
            _instance.OnONAPopupFailedToload += OnAdLoadFailed;
            _instance.OnONAPopupPaidImpression += OnAdPaidImpression;
            _instance.OnONAPopupDisplayed += OnRectDisplayed;
            _instance.OnONAPopupClosed += OnAdClosed;
#if boostrap_ios && UNITY_IOS
            _instance.OnONAPopupImpression += OnAdImpressionRecorded;
#endif
        }

        #region CallBack

        private void OnAdLoaded(NativeAdInfo adInfo)
        {
            SplashTracking.Tracking($"5_{_position}_l_s_loaded");
            ChangeStatus(AdStatus.Loaded);
        }
        
        private void OnAdClosed(NativeAdInfo adInfo)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_closed");
            ChangeStatus(AdStatus.AdClosed);
        }

        private void OnRectDisplayed(NativeAdInfo obj)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_displayed");
            ChangeStatus(AdStatus.AdDisplayed);
        }

        private void OnAdImpressionRecorded(NativeAdInfo adInfo)
        {
            OnAdImpression?.Invoke(adInfo);
        }

        private void OnAdPaidImpression(NativeAdInfo adInfo, NativeAdPaidInfo adPaidInfo)
        {
            SplashTracking.Tracking($"5_{_position}_s_s_paid");
            ChangeStatus(AdStatus.AdPaidImpression);
        }

        private void OnAdLoadFailed(string arg1, int arg2, string arg3)
        {
            SplashTracking.Tracking($"5_{_position}_l_s_failed");
            ChangeStatus(AdStatus.LoadFailed);
        }
        
        #endregion

        #region Core

        public async UniTask<AdStatus> LoadAsync(CancellationToken ct)
        {
            ChangeStatus(AdStatus.Loading);
            _instance.PreloadAll();
            await UniTask.WaitUntil(() => _status != AdStatus.Loading, cancellationToken: ct);
            return _status;
        }

        public void Show()
        {
            var coor = _layout.CenterNormalized;
            var size = _layout.SizeInPixels;

            var density = Master.GetScreenDensity();

            var wDp = size.x / density;
            var hDp = size.y / density;

            _instance.ShowManualClose("mrec_single_manual_06",
                0,
                0,
                coor.x,
                coor.y,
                wDp,
                hDp
            );
        }

        public bool IsReady()
        {
            return _status == AdStatus.Loaded;
        }

        public void ChangeStatus(AdStatus status)
        {
            _status = status;
            OnAdStatusChanged?.Invoke(status);
        }

#if boostrap_ios && UNITY_IOS
        private sealed class IosPopupNativeInstance : IOSNativeAdCallbackTarget, IOSNativeAdEventTarget
        {
            private const string LayoutName = "mrec_single_manual_06";

            private static int counter;

            private readonly string adUnitId;
            private readonly PULayout layout;
            private readonly string alias;

            private bool isReady;
            private bool showRequested;

            public event Action<NativeAdInfo> OnONAPopupLoaded;
            public event Action<NativeAdInfo> OnONAPopupDisplayed;
            public event Action<NativeAdInfo> OnONAPopupClosed;
            public event Action<string, int, string> OnONAPopupFailedToload;
            public event Action<NativeAdInfo, NativeAdPaidInfo> OnONAPopupPaidImpression;
            public event Action<NativeAdInfo> OnONAPopupImpression;

            public IosPopupNativeInstance(string adUnitId, PULayout layout)
            {
                this.adUnitId = adUnitId;
                this.layout = layout;
                alias = "ios_popup_direct_" + (++counter);
                IOSNativeAdBridge.Register(alias, this);
            }

            public void PreloadAll()
            {
                isReady = false;
                showRequested = false;
                GetPopupPlacement(layout, out var xDp, out var yDp, out var widthDp, out var heightDp);
                IOSNativeAdBridge.LoadPopup(
                    alias,
                    adUnitId,
                    LayoutName,
                    0,
                    0,
                    xDp,
                    yDp,
                    widthDp,
                    heightDp,
                    autoClose: false,
                    enableCtrOverlay: false);
            }

            public void ShowManualClose(string layoutName, int timeShowSeconds, int timeReload, float x, float y, float widthDp, float heightDp)
            {
                ResolvePopupPlacement(x, y, widthDp, heightDp, out var xDp, out var yDp);
                IOSNativeAdBridge.UpdatePopupPlacement(alias, xDp, yDp, widthDp, heightDp);

                if (!IOSNativeAdBridge.IsPopupDisplayable(alias))
                {
                    IOSNativeAdBridge.NotifyPopupShowSkipped(alias, $"state={IOSNativeAdBridge.PopupStateFor(alias)}");
                    return;
                }

                showRequested = true;
                isReady = false;
                IOSNativeAdBridge.ShowPopup(alias);
            }

            public bool IsReady()
            {
                return isReady && IOSNativeAdBridge.IsPopupDisplayable(alias);
            }

            public void HandleNativeCallback(string callbackName)
            {
                var state = string.IsNullOrEmpty(callbackName) ? string.Empty : callbackName.Trim();
                var info = CreateAdInfo();

                switch (state)
                {
                    case IOSNativeAdCallbackNames.Loading:
                        isReady = false;
                        break;

                    case IOSNativeAdCallbackNames.Displayable:
                        isReady = true;
                        showRequested = false;
                        OnONAPopupLoaded?.Invoke(info);
                        break;

                    case IOSNativeAdCallbackNames.Failed:
                        isReady = false;
                        if (showRequested)
                        {
                            showRequested = false;
                            OnONAPopupClosed?.Invoke(info);
                        }
                        else
                        {
                            OnONAPopupFailedToload?.Invoke(adUnitId, -1, "IOS popup native load failed.");
                        }
                        break;

                    case IOSNativeAdCallbackNames.Shown:
                        isReady = false;
                        showRequested = false;
                        OnONAPopupDisplayed?.Invoke(info);
                        break;

                    case IOSNativeAdCallbackNames.OnClosed:
                        isReady = false;
                        showRequested = false;
                        OnONAPopupClosed?.Invoke(info);
                        break;
                }
            }

            public void HandleNativeAdEvent(string eventName, IOSNativeAdEventData data)
            {
                if (data == null)
                    return;

                var info = CreateAdInfo();
                if (!string.IsNullOrEmpty(data.adUnitId)) info.adUnitId = data.adUnitId;
                if (!string.IsNullOrEmpty(data.mediationAdapter)) info.mediationAdapter = data.mediationAdapter;
                if (!string.IsNullOrEmpty(data.responseId)) info.responseId = data.responseId;
                if (!string.IsNullOrEmpty(data.adSourceName)) info.adSource = data.adSourceName;
                info.adSourceId = data.adSourceId ?? string.Empty;

                switch (eventName)
                {
                    case IOSNativeAdCallbackNames.Impression:
                        OnONAPopupImpression?.Invoke(info);
                        break;

                    case IOSNativeAdCallbackNames.Paid:
                        info.revenueMicros = data.valueMicros;
                        info.currencyCode = data.SafeCurrencyCode;
                        info.precisionType = data.precision;
                        OnONAPopupPaidImpression?.Invoke(info, new NativeAdPaidInfo
                        {
                            revenueMicros = data.valueMicros,
                            currencyCode = data.SafeCurrencyCode
                        });
                        break;
                }
            }

            private NativeAdInfo CreateAdInfo()
            {
                return new NativeAdInfo
                {
                    adUnitId = adUnitId,
                    mediationAdapter = BG_ConstValue.mediation_ios,
                    responseId = alias,
                    adSource = BG_ConstValue.mediation_ios,
                    adSourceId = string.Empty
                };
            }

            private static void GetPopupPlacement(PULayout layout, out float xDp, out float yDp, out float widthDp, out float heightDp)
            {
                if (layout == null)
                {
                    xDp = 0f;
                    yDp = 0f;
                    widthDp = 0f;
                    heightDp = 0f;
                    return;
                }

                var coor = layout.CenterNormalized;
                var size = layout.SizeInPixels;
                var density = Master.GetScreenDensity();
                if (density <= 0f)
                    density = 1f;

                widthDp = size.x / density;
                heightDp = size.y / density;
                ResolvePopupPlacement(coor.x, coor.y, widthDp, heightDp, out xDp, out yDp);
            }

            private static void ResolvePopupPlacement(float x, float y, float widthDp, float heightDp, out float xDp, out float yDp)
            {
                if (x >= 0f && x <= 1f && y >= 0f && y <= 1f)
                {
                    var density = Master.GetScreenDensity();
                    if (density <= 0f)
                        density = 1f;

                    var screenWidthDp = Screen.width / density;
                    var screenHeightDp = Screen.height / density;
                    var rawX = x * screenWidthDp - widthDp * 0.5f;
                    var rawY = (1f - y) * screenHeightDp - heightDp * 0.5f;
                    xDp = Mathf.Clamp(rawX, 0f, Mathf.Max(0f, screenWidthDp - widthDp));
                    yDp = Mathf.Clamp(rawY, 0f, Mathf.Max(0f, screenHeightDp - heightDp));
                    return;
                }

                xDp = x;
                yDp = y;
            }
        }
#endif
        #endregion

    }
}
