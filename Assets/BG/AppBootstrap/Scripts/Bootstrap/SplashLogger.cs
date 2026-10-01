using UnityEngine;

namespace AppBootstrap.Splash
{
    internal static class SplashLogger
    {
        private const string Prefix = "[Splash]";

        public static void Log(string message)
        {
            Debug.Log($"{FormatRuntime()} {Prefix} {message}");
        }

        public static void Warn(string message)
        {
            Debug.LogWarning($"{FormatRuntime()} {Prefix} {message}");
        }

        public static void Error(string message)
        {
            Debug.LogError($"{FormatRuntime()} {Prefix} {message}");
        }

        private static string FormatRuntime()
        {
            return $"[t={Time.realtimeSinceStartup:0.000}s]";
        }
    }
}
