namespace AppBootstrap.Splash
{
    internal sealed class DeferredAppLauncherState
    {
        public AdShowTriggerType TriggerType;
        public float DelayShowSeconds;
        public ISplashAdHandler Handler;
        public AdGroupEntry GroupEntry;
        public SplashAdExecutionResult Result;
    }
}
