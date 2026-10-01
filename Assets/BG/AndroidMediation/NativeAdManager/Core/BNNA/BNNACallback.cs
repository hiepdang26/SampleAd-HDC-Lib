using System;
using UnityEngine;

public abstract class AdNativeCallbackProxyBase : AndroidJavaProxy
{
    private readonly string logTag;

    protected AdNativeCallbackProxyBase(string javaInterface, string logTag) : base(javaInterface)
    {
        this.logTag = logTag;
    }

    public override AndroidJavaObject Invoke(string methodName, object[] args)
    {
        Debug.Log($"[{logTag}] Invoke {methodName}");

        try
        {
            if (HandleInvoke(methodName, args ?? Array.Empty<object>()))
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{logTag}] Invoke {methodName} failed: {ex}");
            return null;
        }

        return base.Invoke(methodName, args);
    }

    protected abstract bool HandleInvoke(string methodName, object[] args);

    protected static AndroidJavaObject AsJavaObject(object value)
    {
        return value as AndroidJavaObject;
    }

    protected static string AsString(object value)
    {
        return value as string ?? value?.ToString();
    }

    protected static int AsInt(object value)
    {
        if (value is int intValue) return intValue;
        if (value is AndroidJavaObject javaObject) return javaObject.Call<int>("intValue");
        return value != null ? Convert.ToInt32(value) : 0;
    }
}

public class BNNACallback : AdNativeCallbackProxyBase
{
    public event System.Action<NativeAdInfo> OnBNNALoaded;
    public event System.Action<NativeAdInfo> OnBNNADisplayed;
    public event System.Action<NativeAdInfo> OnBNNAOpened;
    public event System.Action<NativeAdInfo> OnBNNAClosed;
    public event System.Action<NativeAdInfo> OnBNNAClicked;
    public event System.Action<string, int, string> OnBNNAFailedToLoad;
    public event System.Action<NativeAdInfo, NativeAdPaidInfo> OnBNNAPaidImpression;
    public event System.Action OnBNNADisplayable;

    public BNNACallback() : base("com.blackgems.aar.features.banner.BannerNativeAdCallback", "BNNACallback") { }

    protected override bool HandleInvoke(string methodName, object[] args)
    {
        switch (methodName)
        {
            case nameof(onBNNALoaded): onBNNALoaded(AsJavaObject(args[0])); return true;
            case nameof(onBNNADisplayed): onBNNADisplayed(AsJavaObject(args[0])); return true;
            case nameof(onBNNAOpened): onBNNAOpened(AsJavaObject(args[0])); return true;
            case nameof(onBNNAClosed): onBNNAClosed(AsJavaObject(args[0])); return true;
            case nameof(onBNNAClicked): onBNNAClicked(AsJavaObject(args[0])); return true;
            case nameof(onBNNAFailedToLoad): onBNNAFailedToLoad(AsString(args[0]), AsInt(args[1]), AsString(args[2])); return true;
            case nameof(onBNNAPaidImpression): onBNNAPaidImpression(AsJavaObject(args[0]), AsJavaObject(args[1])); return true;
            case nameof(onBNNADisplayable): onBNNADisplayable(); return true;
            default: return false;
        }
    }

    public void onBNNALoaded(AndroidJavaObject ad)
    {
        Debug.Log("[BNNACallback] onBNNALoaded");
        OnBNNALoaded?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onBNNADisplayed(AndroidJavaObject ad)
    {
        Debug.Log("[BNNACallback] onBNNADisplayed");
        OnBNNADisplayed?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onBNNAOpened(AndroidJavaObject ad)
    {
        Debug.Log("[BNNACallback] onBNNAOpened");
        OnBNNAOpened?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onBNNAClosed(AndroidJavaObject ad)
    {
        Debug.Log("[BNNACallback] onBNNAClosed");
        OnBNNAClosed?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onBNNAClicked(AndroidJavaObject ad)
    {
        Debug.Log("[BNNACallback] onBNNAClicked");
        OnBNNAClicked?.Invoke(ParseNativeAdInfo(ad));
    }

    public void onBNNAFailedToLoad(string adUnit, int errorCode, string error)
    {
        Debug.Log($"[BNNACallback] onBNNAFailedToLoad {adUnit} {errorCode} {error}");
        OnBNNAFailedToLoad?.Invoke(adUnit, errorCode, error);
    }

    public void onBNNAPaidImpression(AndroidJavaObject ad, AndroidJavaObject paidInfo)
    {
        Debug.Log("[BNNACallback] onBNNAPaidImpression");
        OnBNNAPaidImpression?.Invoke(ParseNativeAdInfo(ad), ParsePaidInfo(paidInfo));
    }

    public void onBNNADisplayable()
    {
        Debug.Log("[BNNACallback] onBNNADisplayable");
        OnBNNADisplayable?.Invoke();
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
