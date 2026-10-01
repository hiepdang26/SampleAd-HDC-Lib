namespace AppBootstrap.Splash
{
    public enum AdGroupStatus
    {
        NotCalledLoadAds = 0,
        WaitingLoadAds = 1,
        LoadAdsFailed = 2,
        AdsReady = 3,
        TimeOut = 4,
        AdsComplete = 5,
        AdsShowing = 6
    }
}
