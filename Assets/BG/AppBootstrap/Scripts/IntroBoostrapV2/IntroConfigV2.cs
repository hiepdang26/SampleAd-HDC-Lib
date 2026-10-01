namespace AppBootstrap.Intro
{
    [System.Serializable]
    public struct IntroConfigV2
    {
        public string nextSceneName;
        public IntroConfig[] Configs;
        public IntroGroupAd Profile;
        public IntroGroupAd Avatar;
    }

    [System.Serializable]
    public struct IntroGroupAd
    {
        public bool isEnable;
        public bool PreloadPopup;
        public string IntroBackgroundImage;
        public string PopupConfig;
        public string InterstitialConfig;
        public string NativeAfterInterstitialConfig;
        public float NativeAfterInterstitialConfigTimeOut;
    }
}