using UnityEngine;

public class ONACollapseReloadByTimeClickCallback : AdNativeCallbackProxyBase
{
    public event System.Action<NativeAdInfo> OnONACollapseReloadLoaded;
    public event System.Action<NativeAdInfo> OnONACollapseReloadDisplayed;
    public event System.Action<NativeAdInfo> OnONACollapseReloadOpened;
    public event System.Action<NativeAdInfo> OnONACollapseReloadClosed;
    public event System.Action<NativeAdInfo> OnONACollapseReloadClickCloseButton;
    public event System.Action<NativeAdInfo> OnONACollapseReloadClicked;
    public event System.Action<string, int, string> OnONACollapseReloadFailedToLoad;
    public event System.Action<NativeAdInfo, NativeAdPaidInfo> OnONACollapseReloadPaidImpression;
    public event System.Action OnONACollapseReloadDisplayable;

    public ONACollapseReloadByTimeClickCallback() : base("com.blackgems.aar.features.collapse.OverlayCollapseNativeAdCallback", "ONACollapseReloadCallback") { }

    protected override bool HandleInvoke(string methodName, object[] args)
    {
        switch (methodName)
        {
            case nameof(onONACollapseLoaded): onONACollapseLoaded(AsJavaObject(args[0])); return true;
            case nameof(onONACollapseDisplayed): onONACollapseDisplayed(AsJavaObject(args[0])); return true;
            case nameof(onONACollapseOpened): onONACollapseOpened(AsJavaObject(args[0])); return true;
            case nameof(onONACollapseClosed): onONACollapseClosed(AsJavaObject(args[0])); return true;
            case nameof(onONACollapseClickCloseButton): onONACollapseClickCloseButton(AsJavaObject(args[0])); return true;
            case nameof(onONACollapseClicked): onONACollapseClicked(AsJavaObject(args[0])); return true;
            case nameof(onONACollapseFailedToLoad): onONACollapseFailedToLoad(AsString(args[0]), AsInt(args[1]), AsString(args[2])); return true;
            case nameof(onONACollapsePaidImpression): onONACollapsePaidImpression(AsJavaObject(args[0]), AsJavaObject(args[1])); return true;
            case nameof(onONACollapseDisplayable): onONACollapseDisplayable(); return true;
            default: return false;
        }
    }

    public void onONACollapseLoaded(AndroidJavaObject ad)
    {
        Debug.Log("[ONACollapseReloadCallback] onONACollapseLoaded");
        OnONACollapseReloadLoaded?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONACollapseDisplayed(AndroidJavaObject ad)
    {
        Debug.Log("[ONACollapseReloadCallback] onONACollapseDisplayed");
        OnONACollapseReloadDisplayed?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONACollapseOpened(AndroidJavaObject ad)
    {
        Debug.Log("[ONACollapseReloadCallback] onONACollapseOpened");
        OnONACollapseReloadOpened?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONACollapseClosed(AndroidJavaObject ad)
    {
        Debug.Log("[ONACollapseReloadCallback] onONACollapseClosed");
        OnONACollapseReloadClosed?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONACollapseClickCloseButton(AndroidJavaObject ad)
    {
        Debug.Log("[ONACollapseReloadCallback] onONACollapseClickCloseButton");
        OnONACollapseReloadClickCloseButton?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONACollapseClicked(AndroidJavaObject ad)
    {
        Debug.Log("[ONACollapseReloadCallback] onONACollapseClicked");
        OnONACollapseReloadClicked?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onONACollapseFailedToLoad(string adUnit, int errorCode, string error)
    {
        Debug.Log($"[ONACollapseReloadCallback] onONACollapseFailedToLoad {adUnit} {errorCode} {error}");
        OnONACollapseReloadFailedToLoad?.Invoke(adUnit, errorCode, error);
    }

    public void onONACollapsePaidImpression(AndroidJavaObject ad, AndroidJavaObject paidInfo)
    {
        Debug.Log("[ONACollapseReloadCallback] onONACollapsePaidImpression");
        OnONACollapseReloadPaidImpression?.Invoke(ParseNativeAdInfo(ad), ParsePaidInfo(paidInfo));
    }

    public void onONACollapseDisplayable()
    {
        Debug.Log("[ONACollapseReloadCallback] onONACollapseDisplayable");
        OnONACollapseReloadDisplayable?.Invoke();
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
