using System;
using GoogleAdError = GoogleMobileAds.Api.AdError;
using GoogleAdValue = GoogleMobileAds.Api.AdValue;
using GoogleLoadAdError = GoogleMobileAds.Api.LoadAdError;

namespace BG_Library.NET.AndroidSDK
{
    public class InterstitialsInstance
    {
        private readonly InterstitialAdInstance interstitial;
        private readonly bool immersiveMode;
        private readonly int preloadBufferSize;

        public InterstitialsInstance(
            string id,
            bool autoReload = false,
            int preloadBufferSize = 1,
            bool immersiveMode = false)
            : this(
                string.IsNullOrWhiteSpace(id) ? Array.Empty<string>() : new[] { id },
                autoReload,
                preloadBufferSize,
                immersiveMode)
        {
        }

        public InterstitialsInstance(
            string[] ids,
            bool autoReload = false,
            int preloadBufferSize = 1,
            bool immersiveMode = false)
        {
            this.immersiveMode = immersiveMode;
            this.preloadBufferSize = Math.Max(1, preloadBufferSize);

            interstitial = new InterstitialAdInstance(
                new InterstitialAdInstance.InterstitialConfig(NormalizeIds(ids), autoReload, this.preloadBufferSize));
            interstitial.OnInterstitialLoaded += info => OnAdLoadedEvent?.Invoke(Map(info));
            interstitial.OnInterstitialFailedToLoad += (adUnit, errorCode, msg) => OnAdLoadFailedEvent?.Invoke(adUnit, errorCode, msg);
            interstitial.OnInterstitialDisplayed += info => OnAdDisplayedEvent?.Invoke(Map(info));
            interstitial.OnInterstitialClicked += info => OnAdClicked?.Invoke(Map(info));
            interstitial.OnInterstitialPaidImpression += (info, paid) => OnPaidAdImpressionEvent?.Invoke(Map(info), Map(paid));
            interstitial.OnInterstitialClosed += info => OnAdHiddenEvent?.Invoke(Map(info));
            interstitial.OnInterstitialOpened += info => OnAdOpenedEvent?.Invoke(Map(info));
            interstitial.OnInterstitialDisplayable += () => OnAdDisplayableEvent?.Invoke();
        }

        public string Alias => interstitial?.Alias;
        public bool IsReady => interstitial?.IsReady() ?? false;

        public void LoadAd()
        {
            interstitial?.Load(preloadBufferSize);
        }

        public void LoadAd(int customPreloadBufferSize)
        {
            interstitial?.Load(customPreloadBufferSize);
        }

        public void ShowAd()
        {
            interstitial?.Show(immersiveMode);
        }

        public void ShowAd(bool customImmersiveMode)
        {
            interstitial?.Show(customImmersiveMode);
        }

        public void DestroyAd()
        {
            interstitial?.Clear();
        }

        public event Action<AdInfo> OnAdLoadedEvent;
        public event Action<string, int, string> OnAdLoadFailedEvent;
        public event Action<AdInfo> OnAdDisplayedEvent;
        public event Action<AdInfo> OnAdClicked;
        public event Action<AdInfo, AdValue> OnPaidAdImpressionEvent;
        public event Action<AdInfo> OnAdHiddenEvent;
        public event Action<AdInfo> OnAdOpenedEvent;
        public event Action OnAdDisplayableEvent;
        public event Action<GoogleLoadAdError> OnAdLoadFailedCompat
        {
            add => interstitial.OnAdLoadFailedCompat += value;
            remove => interstitial.OnAdLoadFailedCompat -= value;
        }
        public event Action<GoogleAdValue> OnAdPaid
        {
            add => interstitial.OnAdPaid += value;
            remove => interstitial.OnAdPaid -= value;
        }
        public event Action OnAdClickedCompat
        {
            add => interstitial.OnAdClicked += value;
            remove => interstitial.OnAdClicked -= value;
        }
        public event Action OnAdFullScreenContentOpened
        {
            add => interstitial.OnAdFullScreenContentOpened += value;
            remove => interstitial.OnAdFullScreenContentOpened -= value;
        }
        public event Action OnAdFullScreenContentClosed
        {
            add => interstitial.OnAdFullScreenContentClosed += value;
            remove => interstitial.OnAdFullScreenContentClosed -= value;
        }
        public event Action<GoogleAdError> OnAdFullScreenContentFailed
        {
            add => interstitial.OnAdFullScreenContentFailed += value;
            remove => interstitial.OnAdFullScreenContentFailed -= value;
        }

        private static AdInfo Map(InterstitialInfo src)
        {
            if (src == null)
            {
                return null;
            }

            return new AdInfo
            {
                adUnitId = src.adUnitId,
                mediationAdapter = src.mediationAdapter,
                responseId = src.responseId,
                adSource = src.adSource
            };
        }

        private static AdValue Map(InterstitialPaidInfo src)
        {
            if (src == null)
            {
                return null;
            }

            return new AdValue
            {
                revenueMicros = src.revenueMicros,
                currencyCode = src.currencyCode,
                precisionType = 0
            };
        }

        private static string[] NormalizeIds(string[] rawIds)
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
