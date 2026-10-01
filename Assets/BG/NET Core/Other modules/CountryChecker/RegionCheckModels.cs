using System;
using System.Collections.Generic;

namespace CountryRegionCheck
{
    public enum RegionCheckVerdict
    {
        TargetCountry,
        NotTargetCountry,
        Unknown,
        Failed
    }

    public enum RegionSignalType
    {
        SystemLanguage,
        Culture,
        Timezone,
        AndroidSimCountry,
        AndroidNetworkCountry
    }

    [Serializable]
    public class RegionSignal
    {
        public RegionSignalType type;
        public bool isTargetCountry;
        public bool isSuspicious;
        public string value;
        public string reason;

        public RegionSignal(RegionSignalType type, bool isTargetCountry, string value, string reason, bool isSuspicious = false)
        {
            this.type = type;
            this.isTargetCountry = isTargetCountry;
            this.value = value;
            this.reason = reason;
            this.isSuspicious = isSuspicious;
        }
    }

    [Serializable]
    public class RegionCheckFinalResult
    {
        public RegionCheckVerdict verdict;
        public bool isTargetCountry;
        public float elapsedMs;
        public string finalReason;
        public List<RegionSignal> signals = new List<RegionSignal>();
    }

    [Serializable]
    public class CountryRegionConfig
    {
        public string targetCountryIso2 = "vn";
        public string[] targetCulturePrefixes = { "vi" };
        public string[] targetTimezoneKeywords = { "ho_chi_minh", "saigon" };
        public double[] targetUtcOffsets = { 7.0 };
        public bool matchTimezoneByUtcOffset = true;
        public bool matchTimezoneByKeyword = true;
        public bool matchSystemLanguageVietnameseWhenTargetVn = true;
    }
}