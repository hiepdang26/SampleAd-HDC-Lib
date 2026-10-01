namespace AppBootstrap.Splash
{
    [System.Serializable]
    internal sealed class SplashConfigV2
    {
        public float loadingTime;
        
        public string nativeSplashGroup;
        public float nativeSplashShowTime;
        public float nativeSplashLoadTimeOut;
        public bool enableAdmobLayout;
        
        public string interSplashAdUnitId;
        public string interSplashBackupAdUnitId;
        public bool waitNativeSplashLoad;

        public int interSplashPreload;
        public int interSplashPreloadWaitCount;
        
        public float interLoadAdTimeout;
        public float delayShowInterstitials;

        public string nativeAfterInterId;
        public string nativeAfterLayoutGroup;
        
        public string firstOpenNextScene;
        public string nextScene;
    }
}