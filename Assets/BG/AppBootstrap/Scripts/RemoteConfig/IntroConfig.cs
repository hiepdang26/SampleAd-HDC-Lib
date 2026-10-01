using System;
using UnityEngine;

namespace AppBootstrap.Splash
{
    [Serializable]
    internal sealed class IntroConfig
    {
        public string nextSceneName;
        public IntroEntry[] IntroEntries;
        
        public static IntroConfig RawDataToSplashConfig(string rawData)
        {
            return JsonUtility.FromJson<IntroConfig>(rawData);
        }
    }
}
