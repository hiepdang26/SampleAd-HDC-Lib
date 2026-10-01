using BG_Library.NET.AdSystem;
using BG_Library.NET.API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppBootstrap.Splash
{
    internal sealed class RemoteConfigProvider : IConfigProvider
    {
        private const string AdsInfoConfigKey = "ads_info_config";

        public T LoadRemoteConfig<T>(string configKey)
        {
            SplashLogger.Log($"UpdateRemoteSettings start key={configKey}");
            string rawData = RemoteConfig.Ins.GetCustomRemoteConfigs(configKey);
            SplashLogger.Log($"Load {configKey} raw data {rawData}");
            return JsonUtility.FromJson<T>(rawData);
        }

        public IntroConfig LoadIntro(string configKey)
        {
            return LoadRemoteConfig<IntroConfig>(configKey);
        }

        public AdInfoConfig LoadAdInfo()
        {
            return LoadRemoteConfig<AdInfoConfig>(AdsInfoConfigKey);
        }

        public AdGroupEntry ResolveAdEntry(string adGroupName, AdInfoConfig adInfoConfig)
        {
            if (adInfoConfig == null || string.IsNullOrWhiteSpace(adGroupName))
            {
                return null;
            }

            return adInfoConfig.GetAdGroupEntry(adGroupName);
        }
    }
}
