using System.IO;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2ExecutablePaths
    {
        private static string cachedAdbExecutablePath;
        private static string cachedScrcpyExecutablePath;
        private static bool hasCachedAdbExecutablePath;
        private static bool hasCachedScrcpyExecutablePath;

        public static void ClearCaches()
        {
            cachedAdbExecutablePath = null;
            cachedScrcpyExecutablePath = null;
            hasCachedAdbExecutablePath = false;
            hasCachedScrcpyExecutablePath = false;
        }

        public static string GetAdbExecutablePath()
        {
            if (!hasCachedAdbExecutablePath)
            {
                cachedAdbExecutablePath = TryResolveExecutableWithWhere("adb.exe");
                hasCachedAdbExecutablePath = true;
            }

            return string.IsNullOrWhiteSpace(cachedAdbExecutablePath) ? "adb" : cachedAdbExecutablePath;
        }

        public static string GetScrcpyExecutablePath(BuildToolScrcpySettingsV2 settings = null)
        {
            string customPath = settings?.customExecutablePath?.Trim();
            if (!string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
                return customPath;

            if (!hasCachedScrcpyExecutablePath)
            {
                cachedScrcpyExecutablePath = TryResolveExecutableWithWhere("scrcpy.exe");
                hasCachedScrcpyExecutablePath = true;
            }

            return cachedScrcpyExecutablePath ?? string.Empty;
        }

        public static bool HasScrcpyExecutable(BuildToolScrcpySettingsV2 settings = null)
        {
            return !string.IsNullOrWhiteSpace(GetScrcpyExecutablePath(settings));
        }

        public static string GetNetworkShareToolPath(BuildToolScrcpySettingsV2 settings = null)
        {
            string scrcpyPath = GetScrcpyExecutablePath(settings);
            if (!string.IsNullOrWhiteSpace(scrcpyPath))
            {
                string scrcpyDirectory = Path.GetDirectoryName(scrcpyPath);
                if (!string.IsNullOrWhiteSpace(scrcpyDirectory) && Directory.Exists(scrcpyDirectory))
                {
                    string[] candidateNames =
                    {
                        "gnirehtet-run.cmd",
                        "gnirehtet.cmd",
                        "gnirehtet.bat",
                        "gnirehtet.exe",
                    };

                    for (int i = 0; i < candidateNames.Length; i++)
                    {
                        string candidatePath = Path.Combine(scrcpyDirectory, candidateNames[i]);
                        if (File.Exists(candidatePath))
                            return candidatePath;
                    }
                }
            }

            string[] pathCandidates =
            {
                TryResolveExecutableWithWhere("gnirehtet-run.cmd"),
                TryResolveExecutableWithWhere("gnirehtet.exe"),
                TryResolveExecutableWithWhere("gnirehtet.cmd"),
                TryResolveExecutableWithWhere("gnirehtet.bat"),
            };

            for (int i = 0; i < pathCandidates.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(pathCandidates[i]))
                    return pathCandidates[i];
            }

            return string.Empty;
        }

        public static void InvalidateScrcpyExecutableCache()
        {
            cachedScrcpyExecutablePath = null;
            hasCachedScrcpyExecutablePath = false;
        }

        private static string TryResolveExecutableWithWhere(string executableName)
        {
            if (!BuildToolV2Utilities.TryRunProcess("where.exe", executableName, out string output))
                return string.Empty;

            string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            return lines.Length == 0 ? string.Empty : lines[0].Trim();
        }
    }
}
