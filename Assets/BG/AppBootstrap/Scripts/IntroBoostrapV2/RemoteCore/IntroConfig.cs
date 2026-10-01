using AppBootstrap.Splash;

namespace AppBootstrap.Intro
{
    [System.Serializable]
    public class IntroConfig
    {
        public IntroType IntroType { get; set; }
        public bool EnableCompleteIntro { get; set; }
        public AdConfig AdConfig { get; set; }
        public float TimeDelayPreload { get; set; }
        public float IntroInterval { get; set; }
        public string IntroDescription { get; set; }
        public string IntroBackgroundImage { get; set; }
        public string IntroCoverImage { get; set; }
        public float DelayShowContinue { get; set; }
    }
}