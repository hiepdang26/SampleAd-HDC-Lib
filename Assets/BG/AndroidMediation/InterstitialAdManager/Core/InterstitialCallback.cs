using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

public class InterstitialCallback : AdNativeCallbackProxyBase
{
// Native callback từ AAR: ad interstitial đã load xong và được đưa vào buffer/cache.
    public event System.Action<InterstitialInfo> OnInterstitialLoaded;

// Native callback từ AAR: ad đã được show lên full screen.
    public event System.Action<InterstitialInfo> OnInterstitialDisplayed;

// Native callback từ AAR: ad được "opened".
// Lưu ý code Android hiện tại đang bắn callback này khi user click ad.
    public event System.Action<InterstitialInfo> OnInterstitialOpened;

// Native callback từ AAR: ad full screen đã đóng/dismiss.
    public event System.Action<InterstitialInfo> OnInterstitialClosed;

// Native callback từ AAR: user click vào ad.
    public event System.Action<InterstitialInfo> OnInterstitialClicked;

// Native callback từ AAR: load ad fail, ad chưa ready, hoặc show fail.
// Trả về adUnit, errorCode, error message.
    public event System.Action<string, int, string> OnInterstitialFailedToLoad;

// Native callback từ AAR: có paid impression / revenue event.
    public event System.Action<InterstitialInfo, InterstitialPaidInfo> OnInterstitialPaidImpression;

// Native callback từ AAR: ad đã sẵn sàng để show.
// Trong Android hiện tại callback này được gọi ngay sau Loaded.
    public event System.Action OnInterstitialDisplayable;


// Nhóm dưới đây là callback compat cho code kiểu GoogleMobileAds Unity.
// Không phải callback native mới từ AAR.

// Compat của OnInterstitialFailedToLoad, convert sang GoogleMobileAds.Api.LoadAdError.
    public event System.Action<LoadAdError> OnAdLoadFailedCompat;

// Compat của OnInterstitialPaidImpression, convert InterstitialPaidInfo sang GoogleMobileAds.Api.AdValue.
    public event System.Action<AdValue> OnAdPaid;

// Compat của OnInterstitialClicked.
    public event System.Action OnAdClicked;

// Compat của OnInterstitialOpened.
// Lưu ý vì OnInterstitialOpened hiện đang bắn khi click, callback này cũng sẽ chạy theo click.
    public event System.Action OnAdFullScreenContentOpened;

// Compat của OnInterstitialClosed.
    public event System.Action OnAdFullScreenContentClosed;

// Compat error dạng GoogleMobileAds.Api.AdError.
// Hiện tại đang được gọi từ onInterstitialFailedToLoad, nên tên này hơi lệch:
// nó không chỉ là "failed to show fullscreen", mà cả load fail/not ready cũng vào đây.
    public event System.Action<AdError> OnAdFullScreenContentFailed;

    public InterstitialCallback() : base("com.blackgems.aar.features.interstitial.InterstitialAdCallback",
        "InterstitialCallback")
    {
    }

    protected override bool HandleInvoke(string methodName, object[] args)
    {
        switch (methodName)
        {
            case nameof(onInterstitialLoaded):
                onInterstitialLoaded(AsJavaObject(args[0]));
                return true;
            case nameof(onInterstitialDisplayed):
                onInterstitialDisplayed(AsJavaObject(args[0]));
                return true;
            case nameof(onInterstitialOpened):
                onInterstitialOpened(AsJavaObject(args[0]));
                return true;
            case nameof(onInterstitialClosed):
                onInterstitialClosed(AsJavaObject(args[0]));
                return true;
            case nameof(onInterstitialClicked):
                onInterstitialClicked(AsJavaObject(args[0]));
                return true;
            case nameof(onInterstitialFailedToLoad):
                onInterstitialFailedToLoad(AsString(args[0]), AsInt(args[1]), AsString(args[2]));
                return true;
            case nameof(onInterstitialPaidImpression):
                onInterstitialPaidImpression(AsJavaObject(args[0]), AsJavaObject(args[1]));
                return true;
            case nameof(onInterstitialDisplayable):
                onInterstitialDisplayable();
                return true;
            default: return false;
        }
    }

    public void onInterstitialLoaded(AndroidJavaObject ad)
    {
        Debug.Log("[InterstitialCallback] onInterstitialLoaded");
        OnInterstitialLoaded?.Invoke(ParseInterstitialInfo(ad));
    }

    public void onInterstitialDisplayed(AndroidJavaObject ad)
    {
        Debug.Log("[InterstitialCallback] onInterstitialDisplayed");
        OnInterstitialDisplayed?.Invoke(ParseInterstitialInfo(ad));
    }

    public void onInterstitialOpened(AndroidJavaObject ad)
    {
        Debug.Log("[InterstitialCallback] onInterstitialOpened");
        OnInterstitialOpened?.Invoke(ParseInterstitialInfo(ad));
        OnAdFullScreenContentOpened?.Invoke();
    }

    public void onInterstitialClosed(AndroidJavaObject ad)
    {
        Debug.Log("[InterstitialCallback] onInterstitialClosed");
        OnInterstitialClosed?.Invoke(ParseInterstitialInfo(ad));
        OnAdFullScreenContentClosed?.Invoke();
    }

    public void onInterstitialClicked(AndroidJavaObject ad)
    {
        Debug.Log("[InterstitialCallback] onInterstitialClicked");
        OnInterstitialClicked?.Invoke(ParseInterstitialInfo(ad));
        OnAdClicked?.Invoke();
    }

    public void onInterstitialFailedToLoad(string adUnit, int errorCode, string error)
    {
        Debug.Log($"[InterstitialCallback] onInterstitialFailedToLoad {adUnit} {errorCode} {error}");
        OnInterstitialFailedToLoad?.Invoke(adUnit, errorCode, error);
        var loadError = new LoadAdError(new InterstitialLoadAdErrorClient(errorCode, error, adUnit));
        OnAdLoadFailedCompat?.Invoke(loadError);
        OnAdFullScreenContentFailed?.Invoke(loadError);
    }

    public void onInterstitialPaidImpression(AndroidJavaObject ad, AndroidJavaObject paidInfo)
    {
        Debug.Log("[InterstitialCallback] onInterstitialPaidImpression");
        var parsedPaidInfo = ParsePaidInfo(paidInfo);
        OnInterstitialPaidImpression?.Invoke(ParseInterstitialInfo(ad), parsedPaidInfo);
        OnAdPaid?.Invoke(ToGoogleAdValue(parsedPaidInfo));
    }

    public void onInterstitialDisplayable()
    {
        Debug.Log("[InterstitialCallback] onInterstitialDisplayable");
        OnInterstitialDisplayable?.Invoke();
    }

    private static InterstitialInfo ParseInterstitialInfo(AndroidJavaObject ad)
    {
        if (ad == null) return new InterstitialInfo();
        return new InterstitialInfo
        {
            adUnitId = ad.Call<string>("getAdUnitId"),
            mediationAdapter = ad.Call<string>("getMediationAdapter"),
            responseId = ad.Call<string>("getResponseId"),
            adSource = ad.Call<string>("getAdSource")
        };
    }

    private static InterstitialPaidInfo ParsePaidInfo(AndroidJavaObject paidInfo)
    {
        if (paidInfo == null) return new InterstitialPaidInfo();
        return new InterstitialPaidInfo
        {
            adUnitId = paidInfo.Call<string>("getAdUnitId"),
            revenueMicros = paidInfo.Call<long>("getRevenueMicros"),
            currencyCode = paidInfo.Call<string>("getCurrencyCode"),
            mediationAdapter = paidInfo.Call<string>("getMediationAdapter"),
            adSource = paidInfo.Call<string>("getAdSource"),
            responseId = paidInfo.Call<string>("getResponseId")
        };
    }

    private static AdValue ToGoogleAdValue(InterstitialPaidInfo paidInfo)
    {
        if (paidInfo == null)
        {
            return null;
        }

        return new AdValue
        {
            Value = paidInfo.revenueMicros,
            CurrencyCode = paidInfo.currencyCode,
            Precision = AdValue.PrecisionType.Precise
        };
    }

    private class InterstitialAdErrorClient : IAdErrorClient
    {
        private readonly int code;
        private readonly string message;
        private readonly string domain;
        private readonly IAdErrorClient cause;

        public InterstitialAdErrorClient(int code, string message, string domain, IAdErrorClient cause = null)
        {
            this.code = code;
            this.message = message ?? string.Empty;
            this.domain = string.IsNullOrWhiteSpace(domain) ? "android_interstitial" : domain;
            this.cause = cause;
        }

        public int GetCode() => code;
        public string GetDomain() => domain;
        public string GetMessage() => message;
        public IAdErrorClient GetCause() => cause;
    }

    private sealed class InterstitialLoadAdErrorClient : InterstitialAdErrorClient, ILoadAdErrorClient
    {
        public InterstitialLoadAdErrorClient(int code, string message, string domain)
            : base(code, message, domain)
        {
        }

        public IResponseInfoClient GetResponseInfoClient() => null;
    }
}