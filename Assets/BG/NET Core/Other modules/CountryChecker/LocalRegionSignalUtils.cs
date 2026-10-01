using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace CountryRegionCheck
{
    public static class LocalRegionSignalUtils
    {
        public static List<RegionSignal> CollectLocalSignals(CountryRegionConfig config, bool verboseLog)
        {
            List<RegionSignal> signals = new List<RegionSignal>
            {
                CheckSystemLanguage(config),
                CheckCulture(config),
                CheckTimezone(config),
                CheckAndroidSimCountry(config),
                CheckAndroidNetworkCountry(config)
            };
            if (verboseLog)
            {
                foreach (RegionSignal signal in signals)
                    Debug.Log($"[CountryRegionCheck][Local] {signal.type}: isTarget={signal.isTargetCountry}, suspicious={signal.isSuspicious}, value={signal.value}, reason={signal.reason}");
            }
            return signals;
        }

        public static RegionSignal CheckSystemLanguage(CountryRegionConfig config)
        {
            bool isTarget = config.targetCountryIso2 == "vn" && config.matchSystemLanguageVietnameseWhenTargetVn && Application.systemLanguage == SystemLanguage.Vietnamese;
            return new RegionSignal(RegionSignalType.SystemLanguage, isTarget, Application.systemLanguage.ToString(), isTarget ? "System language matches target country." : "System language does not match target country.");
        }

        public static RegionSignal CheckCulture(CountryRegionConfig config)
        {
            string culture = CultureInfo.CurrentCulture.Name;
            string uiCulture = CultureInfo.CurrentUICulture.Name;
            bool isTarget = false;
            if (config.targetCulturePrefixes != null)
            {
                foreach (string prefix in config.targetCulturePrefixes)
                {
                    if (string.IsNullOrEmpty(prefix)) continue;
                    string p = prefix.ToLowerInvariant();
                    if (culture.ToLowerInvariant().StartsWith(p) || uiCulture.ToLowerInvariant().StartsWith(p)) { isTarget = true; break; }
                }
            }
            return new RegionSignal(RegionSignalType.Culture, isTarget, $"Culture={culture}, UICulture={uiCulture}", isTarget ? "Culture or UI culture matches target country." : "Culture and UI culture do not match target country.");
        }

        public static RegionSignal CheckTimezone(CountryRegionConfig config)
        {
            TimeZoneInfo timezone = TimeZoneInfo.Local;
            string id = timezone.Id.ToLowerInvariant();
            double offset = timezone.BaseUtcOffset.TotalHours;
            bool matchKeyword = false;
            bool matchOffset = false;
            if (config.matchTimezoneByKeyword && config.targetTimezoneKeywords != null)
            {
                foreach (string keyword in config.targetTimezoneKeywords)
                    if (!string.IsNullOrEmpty(keyword) && id.Contains(keyword.ToLowerInvariant())) { matchKeyword = true; break; }
            }
            if (config.matchTimezoneByUtcOffset && config.targetUtcOffsets != null)
            {
                foreach (double targetOffset in config.targetUtcOffsets)
                    if (Math.Abs(offset - targetOffset) < 0.01) { matchOffset = true; break; }
            }
            bool isTarget = matchKeyword || matchOffset;
            return new RegionSignal(RegionSignalType.Timezone, isTarget, $"{timezone.Id}, UTC offset={offset}", isTarget ? "Timezone matches target country config." : "Timezone does not match target country config.");
        }

        public static RegionSignal CheckAndroidSimCountry(CountryRegionConfig config)
        {
            string simCountry = AndroidTelephonyUtils.GetSimCountryIso();
            bool isTarget = simCountry == config.targetCountryIso2;
            return new RegionSignal(RegionSignalType.AndroidSimCountry, isTarget, simCountry, isTarget ? "Android SIM country matches target country." : $"Android SIM country does not match target country. Value={simCountry}");
        }

        public static RegionSignal CheckAndroidNetworkCountry(CountryRegionConfig config)
        {
            string networkCountry = AndroidTelephonyUtils.GetNetworkCountryIso();
            bool isTarget = networkCountry == config.targetCountryIso2;
            return new RegionSignal(RegionSignalType.AndroidNetworkCountry, isTarget, networkCountry, isTarget ? "Android network country matches target country." : $"Android network country does not match target country. Value={networkCountry}");
        }
    }
}