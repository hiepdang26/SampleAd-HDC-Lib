using UnityEngine;

namespace AppBootstrap.Splash
{
    public static class FirstSessionData
    {
        public static bool IsFirstOpen;
        
        public static void CheckAndCacheFirstOpenData()
        {
            IsFirstOpen = PlayerPrefs.GetInt("user_first_open", 0) == 0;
        }

        public static void EndFirstOpenSession()
        {
            PlayerPrefs.SetInt("user_first_open", 1);
            PlayerPrefs.Save();
        }
    }
}