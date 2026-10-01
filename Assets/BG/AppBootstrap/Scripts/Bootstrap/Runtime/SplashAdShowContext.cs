using BG_Library.NET.API;

namespace AppBootstrap.Splash
{
    internal readonly struct SplashAdShowContext
    {
        public SplashAdShowContext(PULayout puLayoutTemplate)
        {
            PuLayoutTemplate = puLayoutTemplate;
        }

        public PULayout PuLayoutTemplate { get; }
    }
}
