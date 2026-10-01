namespace BG_Library.Common
{
    public struct AdInfo
    {
        public string id;
        public string adtype;
        public string mediation;
        public string adSource;

        public string group;
        public string pos;

        public int loadTime;
        public string extra;
    }

    public struct AdValueInfo
    {
        public double adRevenue;
        public string currencyCode;
    }

    public static class BG_ConstValue
    {
        public const string mediation_max = "applovin_max_sdk";
        public const string mediation_admob = "admob_sdk";
        public const string mediation_android = "android_sdk";
        public const string mediation_ios = "ios_sdk";

        public const string adtype_fa = "Interstitial";
        public const string adtype_rw = "Rewarded";
        public const string adtype_ao = "AppOpen";

        public const string adtype_bn = "Banner";
        public const string adtype_mrec = "Mrec";

        public const string adtype_pu = "PopUp";
        public const string adtype_cl = "Collap";
    }
}
