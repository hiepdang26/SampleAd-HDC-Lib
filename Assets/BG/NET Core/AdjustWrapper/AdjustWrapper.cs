using BG_Library.NET.AdSystem;
using System;
using System.Reflection;

namespace BG_Library.NET
{
    public static class AdjustWrapper
    {
	    /// <summary>
	    /// Track Ad Revenue 
	    /// </summary>
	    /// <param name="platform"></param>
	    /// <param name="adRev"></param>
	    /// <param name="currencyCode"></param>
	    /// <param name="adSource"></param>
	    /// <param name="adsUnitName"></param>
	    /// <param name="adsPlacment"></param>
        public static void TrackAdRevenue(string platform, double adRev, string currencyCode
	        , string adSource, string adsUnitName, string adsPlacment)
        {
	        Type adjustAdRevenueType = Type.GetType(GetAdjustAdRevenueClass());
	        if(adjustAdRevenueType == null) return;
	        
	        object adjustAdRevenueInstance = Activator.CreateInstance(adjustAdRevenueType, platform);
	        
	        MethodInfo revenueMethod = adjustAdRevenueType.GetMethod(GetAdjustSetRevenueMethod());
	        revenueMethod.Invoke(adjustAdRevenueInstance, new object[] { adRev, currencyCode });

	        SetMetadata(adjustAdRevenueType, adjustAdRevenueInstance, GetAdjustAdRevenueNetworkField(), adSource);
	        SetMetadata(adjustAdRevenueType, adjustAdRevenueInstance, GetAdjustAdRevenueUnitField(), adsUnitName);
	        SetMetadata(adjustAdRevenueType, adjustAdRevenueInstance, GetAdjustAdRevenuePlacementField(), adsPlacment);

	        Type adjustType = Type.GetType(GetAdjustClass());
	        MethodInfo trackAdRevenueMethod = adjustType.GetMethod(GetAdjustTrackAdRevenueMethod(), new Type[] { adjustAdRevenueType });
	        trackAdRevenueMethod.Invoke(null, new object[] { adjustAdRevenueInstance });
        }

        /// <summary>
        /// Track IAP revenue
        /// </summary>
        /// <param name="localizedPrice"></param>
        /// <param name="isoCurrencyCode"></param>
        public static void TrackIAPRevenue(float localizedPrice, string isoCurrencyCode)
	    {
		    Type adjustEventType = Type.GetType(GetAdjustEventClass());
		    if(adjustEventType == null) return;
		        
		    object adjustEventInstance = Activator.CreateInstance(adjustEventType, NetConfigsSO.Ins.AdjustEventIAP);
		    
		    MethodInfo revenueMethod = adjustEventType.GetMethod(GetAdjustSetRevenueMethod());
		    revenueMethod.Invoke(adjustEventInstance, new object[] { localizedPrice, isoCurrencyCode });
		    
		    Type adjustType = Type.GetType(GetAdjustClass());
		    
		    MethodInfo trackEventMethod = adjustType.GetMethod(GetAdjustTrackEventMethod());
		    trackEventMethod.Invoke(null, new object[] { adjustEventInstance });
	    }

	    private static void SetMetadata(Type type, object instance, string key, string value)
	    {
		    PropertyInfo property = type.GetProperty(key);
		    if (property != null)
		    {
			    property.SetValue(instance, value);
		    }
		    else
		    {
			    FieldInfo fieldInfo = type.GetField(key, BindingFlags.Instance | BindingFlags.NonPublic);
			    fieldInfo.SetValue(instance, value);
		    }
	    }
        
	    private static string GetAdjustClass()
	    {
		    AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
		    if (adjustVersion == AdjustVersion.Adjust_v5)
			    return "AdjustSdk.Adjust";
		    return "com.adjust.sdk.Adjust";
	    }
	    
	    private static string GetAdjustEventClass()
	    {
			AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
			if (adjustVersion == AdjustVersion.Adjust_v5)
			    return "AdjustSdk.AdjustEvent";
		    return "com.adjust.sdk.AdjustEvent";
	    }

        private static string GetAdjustAdRevenueClass()
        {
			AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
			if (adjustVersion == AdjustVersion.Adjust_v5)
		        return "AdjustSdk.AdjustAdRevenue";
	        return "com.adjust.sdk.AdjustAdRevenue";
        }

        private static string GetAdjustSetRevenueMethod()
        {
			AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
			if (adjustVersion == AdjustVersion.Adjust_v5)
		        return "SetRevenue";
	        return "setRevenue";
        }
        
        private static string GetAdjustAdRevenueNetworkField()
        {
			AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
			if (adjustVersion == AdjustVersion.Adjust_v5)
		        return "AdRevenueNetwork";
	        return "adRevenueNetwork";
        }
        
        private static string GetAdjustAdRevenueUnitField()
        {
			AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
			if (adjustVersion == AdjustVersion.Adjust_v5)
		        return "AdRevenueUnit";
	        return "adRevenueUnit";
        }
        
        private static string GetAdjustAdRevenuePlacementField()
        {
			AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
			if (adjustVersion == AdjustVersion.Adjust_v5)
		        return "AdRevenuePlacement";
	        return "adRevenuePlacement";
        }
        
        private static string GetAdjustTrackAdRevenueMethod()
        {
			AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
			if (adjustVersion == AdjustVersion.Adjust_v5)
		        return "TrackAdRevenue";
	        return "trackAdRevenue";
        }
        
        private static string GetAdjustTrackEventMethod()
        {
			AdjustVersion adjustVersion = AdjustVersion.Adjust_v5;
			if (adjustVersion == AdjustVersion.Adjust_v5)
		        return "TrackEvent";
	        return "trackEvent";
        }
    }
}