using System;
using System.Linq;
using BG_Library.NET.Mediation.Android;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BG_Library.NET.AndroidSDK
{
    public class RectAdInstance
    {
        public enum E_Format
        {
            Banner = 0,
            PopUp = 1,
            Collap = 2
        }

        readonly string[] ids;
        readonly string layout;
        readonly string[] layouts;
        readonly AdSourceLayout[] adSourceLayouts;
        readonly int reloadTime;
        readonly int timeCountdown;
        readonly int timeShow;
        readonly int cl_timeClose;
        readonly bool cl_enableHiddenReload;
        readonly int cl_reloadByClick;
        readonly int cl_reloadByHiddenTime;
        readonly E_Format format;

        readonly BNNAInstance bnnaInstance;
        readonly ONAPopupInstace popupInstance;
        readonly ONACollapseReloadByTimeClickInstance collapInstance;

        float pu_xDp;
        float pu_yDp;
        float pu_w;
        float pu_h;
        string loadedAdSourceId;
        string loadedAdSource;
        string loadedMediationAdapter;
        private bool isActiveCtrController;
        private float forceClickRate;

        public RectAdInstance(string[] ids, string layout, string[] layouts, AdSourceLayout[] adSourceLayouts, int timeShow, int reloadTime, bool isPU, bool autoReload = true, bool isActiveCtrController = false, float forceClickRate = 0f, int timeCountdown = 5)
        {
            this.ids = NormalizeIds(ids);
            this.layout = layout;
            this.layouts = NormalizeLayouts(layout, layouts);
            this.adSourceLayouts = adSourceLayouts;
            this.reloadTime = reloadTime;
            this.timeCountdown = Math.Max(0, timeCountdown);
            this.timeShow = timeShow;
            this.isActiveCtrController = isActiveCtrController;
            this.forceClickRate = forceClickRate;

            if (isPU)
            {
                format = E_Format.PopUp;
                popupInstance = new ONAPopupInstace(new AndroidNAConfig(this.ids, autoReload));
                popupInstance.OnONAPopupLoaded += OnPopupLoaded;
                popupInstance.OnONAPopupFailedToload += OnPopupFailedToLoad;
                popupInstance.OnONAPopupClicked += OnPopupClicked;
                popupInstance.OnONAPopupPaidImpression += OnPopupPaidImpression;
                popupInstance.OnONAPopupDisplayed += OnPopupDisplayed;
                popupInstance.OnONAPopupClosed += OnPopupClosed;
                popupInstance.OnONAPopupOpened += OnPopupOpened;
                popupInstance.OnONAPopupDisplayable += OnPopupDisplayable;
                return;
            }

            format = E_Format.Banner;
            bnnaInstance = new BNNAInstance(this.ids);
            bnnaInstance.OnBannerLoaded += OnBannerLoaded;
            bnnaInstance.OnBannerFailedToLoad += OnBannerFailedToLoad;
            bnnaInstance.OnBannerClicked += OnBannerClicked;
            bnnaInstance.OnBannerPaidImpression += OnBannerPaidImpression;
            bnnaInstance.OnBannerDisplayed += OnBannerDisplayed;
            bnnaInstance.OnBannerClosed += OnBannerClosed;
            bnnaInstance.OnBannerOpened += OnBannerOpened;
            bnnaInstance.OnBannerDisplayable += OnBannerDisplayable;
        }

        public RectAdInstance(string id, string layout, string[] layouts, int timeClose, int reloadByClick, int reloadByHiddenTime, bool enableHiddenReload = true)
        {
            this.ids = NormalizeIds(new[] { id });
            this.layout = layout;
            this.layouts = NormalizeLayouts(layout, layouts);
            cl_reloadByClick = reloadByClick;
            cl_enableHiddenReload = enableHiddenReload;
            cl_reloadByHiddenTime = reloadByHiddenTime;
            cl_timeClose = timeClose;
            format = E_Format.Collap;

            collapInstance = new ONACollapseReloadByTimeClickInstance(new AndroidNAConfig(this.ids));
            collapInstance.OnCollapseReloadLoaded += OnCollapLoaded;
            collapInstance.OnCollapseReloadFailedToLoad += OnCollapFailedToLoad;
            collapInstance.OnCollapseReloadClicked += OnCollapClicked;
            collapInstance.OnCollapseReloadPaidImpression += OnCollapPaidImpression;
            collapInstance.OnCollapseReloadDisplayed += OnCollapDisplayed;
            collapInstance.OnCollapseReloadClosed += OnCollapClosed;
            collapInstance.OnAdClickCloseButton += OnAdClickCloseButton;
            collapInstance.OnCollapseReloadOpened += OnCollapOpened;
            collapInstance.OnCollapseReloadDisplayable += OnCollapDisplayable;
        }

        public void LoadAd()
        {
            switch (format)
            {
                case E_Format.Banner:
                    bnnaInstance?.LoadAd();
                    break;
                case E_Format.PopUp:
                    popupInstance?.PreloadAll();
                    break;
                case E_Format.Collap:
                    collapInstance?.PreloadAll();
                    break;
            }
        }

        public void LoadAd(string adUnitId)
        {
            if (string.IsNullOrWhiteSpace(adUnitId))
            {
                LoadAd();
                return;
            }

            switch (format)
            {
                case E_Format.Banner:
                    bnnaInstance?.LoadAd(adUnitId);
                    break;
                case E_Format.PopUp:
                    popupInstance?.PreloadOne(adUnitId);
                    break;
                case E_Format.Collap:
                    collapInstance?.PreloadOne(adUnitId);
                    break;
            }
        }

        public void DestroyAd()
        {
            switch (format)
            {
                case E_Format.Banner:
                    bnnaInstance?.DestroyAd();
                    break;
                case E_Format.PopUp:
                    popupInstance?.Clear();
                    break;
                case E_Format.Collap:
                    collapInstance?.Clear();
                    break;
            }
        }

        public bool IsReady => format switch
        {
            E_Format.Banner => bnnaInstance != null && bnnaInstance.IsReady(),
            E_Format.PopUp => popupInstance != null && popupInstance.IsReady(),
            E_Format.Collap => collapInstance != null && collapInstance.IsReady(),
            _ => false
        };

        public string GetLatestNativeMediationAdapter()
        {
            var adapter = format switch
            {
                E_Format.Banner => bnnaInstance?.GetLatestNativeMediationAdapter(),
                E_Format.PopUp => popupInstance?.GetLatestNativeMediationAdapter(),
                E_Format.Collap => collapInstance?.GetLatestNativeMediationAdapter(),
                _ => null
            };

            return string.IsNullOrEmpty(adapter)
                ? loadedMediationAdapter ?? string.Empty
                : adapter;
        }

        public string LatestNativeMediationAdapter => GetLatestNativeMediationAdapter();

        public string GetLatestNativeAdSource()
        {
            return loadedAdSource ?? string.Empty;
        }

        public string LatestNativeAdSource => GetLatestNativeAdSource();

        public void ShowAd()
        {
            switch (format)
            {
                case E_Format.Banner:
                    bnnaInstance?.ShowAd(layouts, reloadTime, timeCountdown);
                    break;
                case E_Format.PopUp:
                    string layoutSelected = GetPuLayout();
                    popupInstance?.ShowManualClose(layoutSelected, timeShow, reloadTime, pu_xDp, pu_yDp, pu_w, pu_h);
                    break;
                case E_Format.Collap:
                    collapInstance?.ShowCollapseReloadAuto(layout, cl_timeClose, cl_reloadByHiddenTime, cl_enableHiddenReload, cl_reloadByClick);
                    break;
            }
        }

        public string GetPuLayout()
        {
            string layoutSelected = layout;
            if (adSourceLayouts != null)
            {
                foreach (var adSourceLayout in adSourceLayouts)
                {
                    if (adSourceLayout.AdSources.Contains(loadedAdSourceId))
                        return adSourceLayout.Layout;
                }
            }
            return layoutSelected;
        }

        public void HideAd()
        {
            switch (format)
            {
                case E_Format.Banner:
                    bnnaInstance?.HideAd();
                    break;
                case E_Format.PopUp:
                    popupInstance?.Hide();
                    break;
                case E_Format.Collap:
                    collapInstance?.Hide();
                    break;
            }
        }

        public bool ExpandAd(bool enableClick = true)
        {
            if (format != E_Format.Banner)
            {
                return false;
            }

            return bnnaInstance != null && bnnaInstance.ExpandAd(enableClick);
        }

        public void CloseAd()
        {
            if (format == E_Format.PopUp)
            {
                popupInstance?.Close();
                return;
            }

            HideAd();
        }

        public void StopAd()
        {
            switch (format)
            {
                case E_Format.Banner:
                    bnnaInstance?.StopAd();
                    break;
                case E_Format.Collap:
                    collapInstance?.Stop();
                    break;
            }
        }

        public void PU_UpdatePos(float xDp, float yDp, float w, float h)
        {
            if (format != E_Format.PopUp)
            {
                return;
            }

            pu_xDp = xDp;
            pu_yDp = yDp;
            pu_w = w;
            pu_h = h;
        }

        public event Action<AdInfo> OnAdLoadedEvent;
        public event Action<string, int, string> OnAdLoadFailedEvent;
        public event Action<AdInfo> OnAdClicked;
        public event Action<AdInfo, AdValue> OnPaidAdImpressionEvent;
        public event Action<AdInfo> OnAdDisplayedEvent;
        public event Action<AdInfo> OnAdClosedEvent;
        public event Action<AdInfo> OnAdClickCloseButtonEvent;
        public event Action<AdInfo> OnAdOpenedEvent;
        public event Action OnAdDisplayableEvent;

        static AdInfo ToAdInfo(NativeAdInfo src)
        {
            if (src == null)
            {
                return null;
            }

            return new AdInfo
            {
                adUnitId = src.adUnitId,
                /*headline = src.headline,
                body = src.body,
                callToAction = src.callToAction,
                advertiser = src.advertiser,
                store = src.store,
                price = src.price,*/
                mediationAdapter = src.mediationAdapter,
                responseId = src.responseId,
                adSource = src.adSource,
                adSourceId = src.adSourceId
            };
        }

        static AdValue ToAdValue(NativeAdPaidInfo src)
        {
            if (src == null)
            {
                return null;
            }

            return new AdValue
            {
                revenueMicros = src.revenueMicros,
                currencyCode = src.currencyCode
            };
        }

        void OnBannerLoaded(NativeAdInfo info)
        {
            SetLoadedAdSource(info);
            OnAdLoadedEvent?.Invoke(ToAdInfo(info));
        }

        void OnBannerFailedToLoad(string adUnit, int errorCode, string err)
        {
            OnAdLoadFailedEvent?.Invoke(adUnit, errorCode, err);
        }

        void OnBannerClicked(NativeAdInfo info) => OnAdClicked?.Invoke(ToAdInfo(info));
        void OnBannerDisplayed(NativeAdInfo info) => OnAdDisplayedEvent?.Invoke(ToAdInfo(info));
        void OnBannerClosed(NativeAdInfo info) => OnAdClosedEvent?.Invoke(ToAdInfo(info));
        void OnBannerOpened(NativeAdInfo info) => OnAdOpenedEvent?.Invoke(ToAdInfo(info));
        void OnBannerDisplayable() => OnAdDisplayableEvent?.Invoke();
        void OnBannerPaidImpression(NativeAdInfo info, NativeAdPaidInfo paid) => OnPaidAdImpressionEvent?.Invoke(ToAdInfo(info), ToAdValue(paid));

        void OnPopupLoaded(NativeAdInfo info)
        {
            SetLoadedAdSource(info);
            OnAdLoadedEvent?.Invoke(ToAdInfo(info));
        }

        void OnPopupFailedToLoad(string adUnit, int errorCode, string err)
        {
            OnAdLoadFailedEvent?.Invoke(adUnit, errorCode, err);
        }

        void OnPopupClicked(NativeAdInfo info) => OnAdClicked?.Invoke(ToAdInfo(info));
        void OnPopupDisplayed(NativeAdInfo info) => OnAdDisplayedEvent?.Invoke(ToAdInfo(info));
        void OnPopupClosed(NativeAdInfo info) => OnAdClosedEvent?.Invoke(ToAdInfo(info));
        void OnPopupOpened(NativeAdInfo info) => OnAdOpenedEvent?.Invoke(ToAdInfo(info));
        void OnPopupDisplayable() => OnAdDisplayableEvent?.Invoke();
        void OnPopupPaidImpression(NativeAdInfo info, NativeAdPaidInfo paid) => OnPaidAdImpressionEvent?.Invoke(ToAdInfo(info), ToAdValue(paid));

        void OnCollapLoaded(NativeAdInfo info)
        {
            SetLoadedAdSource(info);
            OnAdLoadedEvent?.Invoke(ToAdInfo(info));
        }

        void OnCollapFailedToLoad(string adUnit, int errorCode, string err)
        {
            OnAdLoadFailedEvent?.Invoke(adUnit, errorCode, err);
        }

        void OnCollapClicked(NativeAdInfo info) => OnAdClicked?.Invoke(ToAdInfo(info));
        void OnCollapDisplayed(NativeAdInfo info) => OnAdDisplayedEvent?.Invoke(ToAdInfo(info));
        void OnCollapClosed(NativeAdInfo info) => OnAdClosedEvent?.Invoke(ToAdInfo(info));
        void OnAdClickCloseButton(NativeAdInfo info) => OnAdClickCloseButtonEvent?.Invoke(ToAdInfo(info));
        void OnCollapOpened(NativeAdInfo info) => OnAdOpenedEvent?.Invoke(ToAdInfo(info));
        void OnCollapDisplayable() => OnAdDisplayableEvent?.Invoke();
        void OnCollapPaidImpression(NativeAdInfo info, NativeAdPaidInfo paid) => OnPaidAdImpressionEvent?.Invoke(ToAdInfo(info), ToAdValue(paid));

        void SetLoadedAdSource(NativeAdInfo info)
        {
            loadedAdSource = info?.adSource;
            loadedMediationAdapter = info?.mediationAdapter;
            loadedAdSourceId = info?.adSourceId;
        }

        static string[] NormalizeLayouts(string fallbackLayout, string[] rawLayouts)
        {
            if (rawLayouts == null || rawLayouts.Length == 0)
            {
                return string.IsNullOrWhiteSpace(fallbackLayout)
                    ? Array.Empty<string>()
                    : new[] { fallbackLayout.Trim() };
            }

            var normalized = new System.Collections.Generic.List<string>(rawLayouts.Length + 1);

            if (!string.IsNullOrWhiteSpace(fallbackLayout))
            {
                normalized.Add(fallbackLayout.Trim());
            }

            foreach (var candidate in rawLayouts)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                var trimmed = candidate.Trim();
                if (!normalized.Contains(trimmed))
                {
                    normalized.Add(trimmed);
                }
            }

            return normalized.ToArray();
        }

        static string[] NormalizeIds(string[] rawIds)
        {
            if (rawIds == null || rawIds.Length == 0)
            {
                return Array.Empty<string>();
            }

            var normalized = new System.Collections.Generic.List<string>(rawIds.Length);
            foreach (var candidate in rawIds)
            {
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                var trimmed = candidate.Trim();
                if (!normalized.Contains(trimmed))
                {
                    normalized.Add(trimmed);
                }
            }

            return normalized.ToArray();
        }
    }
}
