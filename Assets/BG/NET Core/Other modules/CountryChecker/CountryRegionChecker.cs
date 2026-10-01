using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CountryRegionCheck
{
    public class CountryRegionChecker : MonoBehaviour
    {
        [Header("Target Country")]
        [SerializeField] private string targetCountryIso2 = "vn";

        [Header("Locale / Timezone")]
        [SerializeField] private bool matchTimezoneByUtcOffset = true;
        [SerializeField] private bool matchTimezoneByKeyword = true;
        [SerializeField] private bool matchSystemLanguageVietnameseWhenTargetVn = true;

        [Header("Behavior")]
        [SerializeField] private bool earlyReturnWhenTargetCountryFound = false;
        [SerializeField] private bool verboseLog = true;

        private bool _isChecking;

        public void Check(Action<RegionCheckFinalResult> onDone)
        {
            if (_isChecking)
            {
                Log("Checker is already running.");
                return;
            }
            StartCoroutine(CheckRoutine(onDone));
        }

        private IEnumerator CheckRoutine(Action<RegionCheckFinalResult> onDone)
        {
            _isChecking = true;
            float startTime = Time.realtimeSinceStartup;
            CountryRegionConfig config = BuildConfig();

            RegionCheckFinalResult finalResult = new RegionCheckFinalResult
            {
                verdict = RegionCheckVerdict.Unknown,
                isTargetCountry = false,
                finalReason = "No target country signal found yet."
            };

            Log($"Check started. Target country ISO2={config.targetCountryIso2}");

            List<RegionSignal> localSignals = LocalRegionSignalUtils.CollectLocalSignals(config, verboseLog);
            finalResult.signals.AddRange(localSignals);

            RegionSignal localTargetSignal = FindFirstTargetCountrySignal(localSignals);
            if (localTargetSignal != null && earlyReturnWhenTargetCountryFound)
            {
                CompleteAsTargetCountry(finalResult, localTargetSignal.reason, startTime, onDone);
                yield break;
            }

            RegionSignal anyTargetSignal = FindFirstTargetCountrySignal(finalResult.signals);
            if (anyTargetSignal != null)
            {
                CompleteAsTargetCountry(finalResult, anyTargetSignal.reason, startTime, onDone);
                yield break;
            }

            finalResult.elapsedMs = GetElapsedMs(startTime);
            finalResult.verdict = RegionCheckVerdict.Unknown;
            finalResult.finalReason = "No target country signal found.";
            _isChecking = false;
            onDone?.Invoke(finalResult);
        }

        private CountryRegionConfig BuildConfig()
        {
            string normalizedIso = string.IsNullOrEmpty(targetCountryIso2) ? "vn" : targetCountryIso2.Trim().ToLowerInvariant();
            CountryRegionConfig config = new CountryRegionConfig
            {
                targetCountryIso2 = normalizedIso,
                matchTimezoneByUtcOffset = matchTimezoneByUtcOffset,
                matchTimezoneByKeyword = matchTimezoneByKeyword,
                matchSystemLanguageVietnameseWhenTargetVn = matchSystemLanguageVietnameseWhenTargetVn
            };
            if (normalizedIso == "vn")
            {
                config.targetCulturePrefixes = new[] { "vi" };
                config.targetTimezoneKeywords = new[] { "ho_chi_minh", "saigon" };
                config.targetUtcOffsets = new[] { 7.0 };
            }
            return config;
        }

        private RegionSignal FindFirstTargetCountrySignal(List<RegionSignal> signals)
        {
            foreach (RegionSignal signal in signals)
                if (signal != null && signal.isTargetCountry) return signal;
            return null;
        }

        private void CompleteAsTargetCountry(RegionCheckFinalResult result, string reason, float startTime, Action<RegionCheckFinalResult> onDone)
        {
            result.verdict = RegionCheckVerdict.TargetCountry;
            result.isTargetCountry = true;
            result.elapsedMs = GetElapsedMs(startTime);
            result.finalReason = reason;
            _isChecking = false;
            Log($"Check result: TargetCountry | {reason} | {result.elapsedMs:0}ms");
            onDone?.Invoke(result);
        }

        private float GetElapsedMs(float startTime) => (Time.realtimeSinceStartup - startTime) * 1000f;

        private void Log(string message)
        {
            if (verboseLog) Debug.Log("[CountryRegionCheck] " + message);
        }
    }
}