using UnityEngine;

public class FSNACallback : AdNativeCallbackProxyBase
{
    public event System.Action<NativeAdInfo> OnFSNALoaded;
    public event System.Action<NativeAdInfo> OnFSNADisplayed;
    public event System.Action<NativeAdInfo> OnFSNAOpened;
    public event System.Action<NativeAdInfo> OnFSNAClosed;
    public event System.Action<NativeAdInfo> OnFSNAClicked;
    public event System.Action<string, int, string> OnFSNAFailedToLoad;
    public event System.Action<NativeAdInfo, NativeAdPaidInfo> OnFSNAPaidImpression;
    public event System.Action OnFSNADisplayable;

    public FSNACallback() : base("com.blackgems.aar.features.fullscreen.FullscreenNativeAdCallback", "FSNACallback") { }

    protected override bool HandleInvoke(string methodName, object[] args)
    {
        switch (methodName)
        {
            case nameof(onFSNALoaded): onFSNALoaded(AsJavaObject(args[0])); return true;
            case nameof(onFSNADisplayed): onFSNADisplayed(AsJavaObject(args[0])); return true;
            case nameof(onFSNAOpened): onFSNAOpened(AsJavaObject(args[0])); return true;
            case nameof(onFSNAClosed): onFSNAClosed(AsJavaObject(args[0])); return true;
            case nameof(onFSNAClicked): onFSNAClicked(AsJavaObject(args[0])); return true;
            case nameof(onFSNAFailedToLoad): onFSNAFailedToLoad(AsString(args[0]), AsInt(args[1]), AsString(args[2])); return true;
            case nameof(onFSNAPaidImpression): onFSNAPaidImpression(AsJavaObject(args[0]), AsJavaObject(args[1])); return true;
            case nameof(onFSNADisplayable): onFSNADisplayable(); return true;
            default: return false;
        }
    }

    public void onFSNALoaded(AndroidJavaObject ad)
    {
        Debug.Log("[FSNACallback] onFSNALoaded");
        OnFSNALoaded?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onFSNADisplayed(AndroidJavaObject ad)
    {
        Debug.Log("[FSNACallback] onFSNADisplayed");
        OnFSNADisplayed?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onFSNAOpened(AndroidJavaObject ad)
    {
        Debug.Log("[FSNACallback] onFSNAOpened");
        OnFSNAOpened?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onFSNAClosed(AndroidJavaObject ad)
    {
        Debug.Log("[FSNACallback] onFSNAClosed");
        OnFSNAClosed?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onFSNAClicked(AndroidJavaObject ad)
    {
        Debug.Log("[FSNACallback] onFSNAClicked");
        OnFSNAClicked?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onFSNAFailedToLoad(string adUnit, int errorCode, string error)
    {
        Debug.Log($"[FSNACallback] onFSNAFailedToLoad {adUnit} {errorCode} {error}");
        OnFSNAFailedToLoad?.Invoke(adUnit, errorCode, error);
    }

    public void onFSNAPaidImpression(AndroidJavaObject ad, AndroidJavaObject paidInfo)
    {
        Debug.Log("[FSNACallback] onFSNAPaidImpression");
        OnFSNAPaidImpression?.Invoke(ParseNativeAdInfo(ad), ParsePaidInfo(paidInfo));
    }

    public void onFSNADisplayable()
    {
        Debug.Log("[FSNACallback] onFSNADisplayable");
        OnFSNADisplayable?.Invoke();
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
            adSourceId = ad.Call<string>("getAdSourceId"),
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
