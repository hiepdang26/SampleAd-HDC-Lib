using UnityEngine;

public class ONAPopupCallback : AdNativeCallbackProxyBase
{
    public event System.Action<NativeAdInfo> OnONAPopupLoaded;
    public event System.Action<NativeAdInfo> OnONAPopupDisplayed;
    public event System.Action<NativeAdInfo> OnONAPopupOpened;
    public event System.Action<NativeAdInfo> OnONAPopupClosed;
    public event System.Action<NativeAdInfo> OnONAPopupClicked;
    public event System.Action<string, int, string> OnONAPopupFailedToLoad;
    public event System.Action<NativeAdInfo, NativeAdPaidInfo> OnONAPopupPaidImpression;
    public event System.Action OnONAPopupDisplayable;

    public ONAPopupCallback() : base("com.blackgems.aar.features.popup.OverlayPopupNativeAdCallback", "ONAPopupCallback") { }

    protected override bool HandleInvoke(string methodName, object[] args)
    {
        switch (methodName)
        {
            case nameof(onONAPopupLoaded): onONAPopupLoaded(AsJavaObject(args[0])); return true;
            case nameof(onONAPopupDisplayed): onONAPopupDisplayed(AsJavaObject(args[0])); return true;
            case nameof(onONAPopupOpened): onONAPopupOpened(AsJavaObject(args[0])); return true;
            case nameof(onONAPopupClosed): onONAPopupClosed(AsJavaObject(args[0])); return true;
            case nameof(onONAPopupClicked): onONAPopupClicked(AsJavaObject(args[0])); return true;
            case nameof(onONAPopupFailedToLoad): onONAPopupFailedToLoad(AsString(args[0]), AsInt(args[1]), AsString(args[2])); return true;
            case nameof(onONAPopupPaidImpression): onONAPopupPaidImpression(AsJavaObject(args[0]), AsJavaObject(args[1])); return true;
            case nameof(onONAPopupDisplayable): onONAPopupDisplayable(); return true;
            default: return false;
        }
    }

    public void onONAPopupLoaded(AndroidJavaObject ad)
    {
        Debug.Log("[ONAPopupCallback] onONAPopupLoaded");
        OnONAPopupLoaded?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONAPopupDisplayed(AndroidJavaObject ad)
    {
        Debug.Log("[ONAPopupCallback] onONAPopupDisplayed");
        OnONAPopupDisplayed?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONAPopupOpened(AndroidJavaObject ad)
    {
        Debug.Log("[ONAPopupCallback] onONAPopupOpened");
        OnONAPopupOpened?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONAPopupClosed(AndroidJavaObject ad)
    {
        Debug.Log("[ONAPopupCallback] onONAPopupClosed");
        OnONAPopupClosed?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONAPopupClicked(AndroidJavaObject ad)
    {
        Debug.Log("[ONAPopupCallback] onONAPopupClicked");
        OnONAPopupClicked?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONAPopupFailedToLoad(string adUnit, int errorCode, string error)
    {
        Debug.Log($"[ONAPopupCallback] onONAPopupFailedToLoad {adUnit} {errorCode} {error}");
        OnONAPopupFailedToLoad?.Invoke(adUnit, errorCode, error);
    }

    public void onONAPopupPaidImpression(AndroidJavaObject ad, AndroidJavaObject paidInfo)
    {
        Debug.Log("[ONAPopupCallback] onONAPopupPaidImpression");
        OnONAPopupPaidImpression?.Invoke(ParseNativeAdInfo(ad), ParsePaidInfo(paidInfo));
    }

    public void onONAPopupDisplayable()
    {
        Debug.Log("[ONAPopupCallback] onONAPopupDisplayable");
        OnONAPopupDisplayable?.Invoke();
    }

    private NativeAdInfo ParseNativeAdInfo(AndroidJavaObject ad)
    {
        if (ad == null) return new NativeAdInfo();
        return new NativeAdInfo
        {
            adUnitId = ad.Call<string>("getAdUnitId"),
            headline = ad.Call<string>("getHeadline"),
            body = ad.Call<string>("getBody"),
            callToAction = ad.Call<string>("getCallToAction"),
            advertiser = ad.Call<string>("getAdvertiser"),
            store = ad.Call<string>("getStore"),
            price = ad.Call<string>("getPrice"),
            mediationAdapter = ad.Call<string>("getMediationAdapter"),
            responseId = ad.Call<string>("getResponseId"),
            adSource = ad.Call<string>("getAdSource"),
            adSourceId = ad.Call<string>("getAdSourceId")
        };
    }

    private NativeAdPaidInfo ParsePaidInfo(AndroidJavaObject paidInfo)
    {
        if (paidInfo == null) return new NativeAdPaidInfo();
        return new NativeAdPaidInfo
        {
            revenueMicros = paidInfo.Call<long>("getRevenueMicros"),
            currencyCode = paidInfo.Call<string>("getCurrencyCode"),
        };
    }
}
