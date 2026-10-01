using System;
using UnityEngine;

namespace AppBootstrap.Splash
{
    [Serializable]
    internal sealed class SplashConfig
    {
        public float seconds;
        public string nextSceneName;
        public string firstOpenNextSceneName;
        public bool isAdsLoadAsync;
        public AdAppLauncherEntry appLauncherEntry;
        public string[] adEntryLoadings;
    }
}
