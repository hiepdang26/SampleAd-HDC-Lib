using System;
using System.Linq;
using BG_Library.Common;
using BG_Library.NET;
using BG_Library.NET.API;
using Firebase.Analytics;
using UnityEngine;

namespace AppBootstrap.Splash
{
    public class AdTracking : MonoBehaviour
    {
        private const string TotalAdPaidKey = "TotalAdPaid";
        private const string SessionPlayKey = "PlayerSessionPlay";

        [SerializeField] private string[] adGroupNameTrackers;
        
        private void Awake()
        {
            AddSessionPlay();
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            NetEventSystem.OnRectPaid += OnAdPaidEvent;
            NetEventSystem.OnFsPaid += OnAdPaidEvent;
        }

        private void OnAdPaidEvent(AdInfo adInfo, AdValueInfo adValueInfo)
        {
            if(!adGroupNameTrackers.Contains(adInfo.group)) return;
            
            var group = adInfo.group;
            var adSource = adInfo.adSource.Split('.')[^1];
            
            AdPaidImpression(group);

            var sessionPlay = GetSessionPlayCount();
            var adPaidGroupCount = GetAdPaidImpressionCount(group);
            var adPaidTotalCount = GetAdPaidImpressionCount(TotalAdPaidKey);
            
            var revenue = adValueInfo.adRevenue;
            var currencyCode = adValueInfo.currencyCode;
            
            FirebaseAnalytics.LogEvent("ad_tracking_ltv", 
                new Parameter("ad_group", group), 
                new Parameter("ad_source", adSource),
                new Parameter("session_play", sessionPlay.ToString()),
                new Parameter("paid_group_count", adPaidGroupCount.ToString()),
                new Parameter("paid_total_count", adPaidTotalCount.ToString()),
                new Parameter("revenue", revenue),
                new Parameter("currency_code", currencyCode));
            
            Debug.Log($"OnAdPaidEvent: ad_group: {group} | ad_source: {adSource} | revenue: {revenue} | currency_code: {currencyCode}");
        }

        public void AdPaidImpression(string groupName)
        {
            var adPaidCount = GetAdPaidImpressionCount(groupName);
            var totalAdPaidCount = GetAdPaidImpressionCount(TotalAdPaidKey);
            
            PlayerPrefs.SetInt(groupName, adPaidCount + 1);
            PlayerPrefs.SetInt(TotalAdPaidKey, totalAdPaidCount + 1);
            PlayerPrefs.Save();
        }

        public int GetAdPaidImpressionCount(string groupName)
        {
            int count = PlayerPrefs.GetInt(groupName, 0);
            return count;
        }

        public void AddSessionPlay()
        {
            PlayerPrefs.SetInt(SessionPlayKey, GetSessionPlayCount() + 1);
            PlayerPrefs.Save();
        }
        
        public int GetSessionPlayCount()
        {
            int count = PlayerPrefs.GetInt(SessionPlayKey, 0);
            return count;
        }
    }

    [System.Serializable]
    public class AdPaidData
    {
        public int totalAdCount;
        public AdData[] adData;
    }

    [System.Serializable]
    public class AdData
    {
        public string groupName;
        public int adCount;
    }
}