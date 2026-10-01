using BG_Library.NET.API;
using Firebase.Analytics;

namespace AppBootstrap.Splash
{
    public static class SplashTracking
    {
        public static string first_open_tracking_prefix = FirstSessionData.IsFirstOpen ? "f" : "n";

        public static void Tracking(string value)
        {
            FirebaseAnalytics.LogEvent($"{first_open_tracking_prefix}_{value}");
            FirebaseAnalytics.LogEvent($"a_{value}");
        }
    }
}