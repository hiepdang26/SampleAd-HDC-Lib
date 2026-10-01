#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace BG_Library.NET.AdCore.MainIOS
{
    public static class IOSPodfileDeploymentTargetPostprocess
    {
        private const string RequiredIosDeploymentTarget = "15.0";
        private const string LogTag = "[ios-podfile]";

        // EDM4U generates the Podfile at priority 40 and runs pod install at 50.
        [PostProcessBuild(45)]
        public static void ApplyDeploymentTarget(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS)
                return;

            string podfilePath = Path.Combine(buildPath, "Podfile");
            if (!File.Exists(podfilePath))
            {
                UnityEngine.Debug.LogWarning($"{LogTag} Podfile was not found at {podfilePath}; skip deployment target patch.");
                return;
            }

            string deploymentTarget = ResolveDeploymentTarget();
            string[] lines = File.ReadAllLines(podfilePath);
            var updatedLines = new List<string>(lines.Length + 1);
            bool replacedPlatform = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (!replacedPlatform && line.TrimStart().StartsWith("platform :ios,", StringComparison.Ordinal))
                {
                    updatedLines.Add($"platform :ios, '{deploymentTarget}'");
                    replacedPlatform = true;
                    continue;
                }

                updatedLines.Add(line);
            }

            if (!replacedPlatform)
                updatedLines.Insert(0, $"platform :ios, '{deploymentTarget}'");

            File.WriteAllLines(podfilePath, updatedLines.ToArray());
            UnityEngine.Debug.Log($"{LogTag} Set Podfile iOS deployment target to {deploymentTarget}.");
        }

        private static string ResolveDeploymentTarget()
        {
            string configured = PlayerSettings.iOS.targetOSVersionString;
            return IsAtLeast(configured, RequiredIosDeploymentTarget)
                ? configured.Trim()
                : RequiredIosDeploymentTarget;
        }

        private static bool IsAtLeast(string value, string minimum)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!Version.TryParse(NormalizeVersion(value), out Version parsed))
                return false;

            return Version.TryParse(NormalizeVersion(minimum), out Version min) && parsed.CompareTo(min) >= 0;
        }

        private static string NormalizeVersion(string value)
        {
            string trimmed = value.Trim();
            return trimmed.Contains(".") ? trimmed : $"{trimmed}.0";
        }
    }
}
#endif
