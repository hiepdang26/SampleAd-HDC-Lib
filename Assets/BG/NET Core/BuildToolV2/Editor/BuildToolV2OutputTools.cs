using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2OutputTools
    {
        public static string GetBuildDisplayName(BuildProfileV2 profile, NetConfigsSO netConfigs)
        {
            if (profile == null)
                return BuildNameFromProduct(PlayerSettings.bundleVersion);

            var nameParts = new List<string>(8)
            {
                GetBuildProductCode(profile.ProductName),
                GetBuildPresetToken(profile.Preset),
                SanitizeBuildNameToken(profile.ResolveVersion()),
                GetAndroidVersionCodeToken(profile.VersionCode),
                profile.BuildTarget == BuildTarget.Android && profile.BuildAppBundle ? "AAB" : "APK",
            };

            if (profile.BuildHack)
                nameParts.Add("hack");
            if (profile.IsReleaseBuildWithDebugWarning)
                nameParts.Add("releaseDebug");
            if (profile.HasManagedSourceMismatches())
                nameParts.Add("custom");
            if (profile.DevelopmentBuild)
                nameParts.Add("dev");
            else if (profile.Preset != BuildProfilePresetV2.TestDebug
                && (profile.UsesDebugMode || (netConfigs != null && NetFlowDebugSystem.IsEnabledForPreset(netConfigs.Debug_Preset))))
                nameParts.Add("debug");

            return string.Join("_", nameParts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        public static void OpenFolderInExplorer(string directory)
        {
            OpenFolder(directory);
        }

        public static void OpenAndSelectPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            string fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                OpenFolder(fullPath);
                return;
            }

            if (!File.Exists(fullPath))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{fullPath}\"",
                    UseShellExecute = true,
                });
            }
            catch
            {
                string directory = Path.GetDirectoryName(fullPath);
                OpenFolder(directory);
            }
        }

        public static string ChooseOutputDirectory(BuildProfileV2 profile)
        {
            string initialDirectory = profile != null && !string.IsNullOrWhiteSpace(profile.OutputDirectory)
                ? profile.OutputDirectory
                : (Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath);

            string selectedDirectory = EditorUtility.SaveFolderPanel("Choose Build Output Folder", initialDirectory, string.Empty);
            if (string.IsNullOrWhiteSpace(selectedDirectory))
                return string.Empty;

            profile?.SetOutputDirectory(selectedDirectory);
            return selectedDirectory;
        }

        public static string GetOutputPath(BuildProfileV2 profile, NetConfigsSO netConfigs)
        {
            string outputDirectory = profile.OutputDirectory;
            if (string.IsNullOrWhiteSpace(outputDirectory) || !Directory.Exists(outputDirectory))
            {
                outputDirectory = ChooseOutputDirectory(profile);
                if (string.IsNullOrWhiteSpace(outputDirectory))
                    return string.Empty;
            }

            string buildName = GetBuildDisplayName(profile, netConfigs);
            if (profile.BuildTarget == BuildTarget.iOS)
                return Path.Combine(outputDirectory, buildName);

            string extension = profile.BuildTarget == BuildTarget.Android && profile.BuildAppBundle ? ".aab" : ".apk";
            return Path.Combine(outputDirectory, buildName + extension);
        }

        public static string BuildNameFromProduct(string version)
        {
            return $"{GetBuildProductCode()}_{SanitizeBuildNameToken(version)}";
        }

        private static string GetBuildProductCode()
        {
            return GetBuildProductCode(PlayerSettings.productName);
        }

        private static string GetBuildProductCode(string productName)
        {
            string normalizedProductName = string.IsNullOrWhiteSpace(productName) ? "APP" : productName;
            string lettersOnly = Regex.Replace(normalizedProductName, "[^a-zA-Z]", string.Empty);

            var upperBuilder = new StringBuilder();
            for (int i = 0; i < lettersOnly.Length; i++)
            {
                if (char.IsUpper(lettersOnly[i]))
                    upperBuilder.Append(lettersOnly[i]);
            }

            return upperBuilder.Length > 0
                ? upperBuilder.ToString()
                : normalizedProductName.Substring(0, Mathf.Min(3, normalizedProductName.Length)).ToUpperInvariant();
        }

        private static string GetBuildPresetToken(BuildProfilePresetV2 preset)
        {
            return preset switch
            {
                BuildProfilePresetV2.TestDebug => "testDebug",
                BuildProfilePresetV2.TestRelease => "testRelease",
                BuildProfilePresetV2.Release => "release",
                _ => preset.ToString(),
            };
        }

        private static string GetAndroidVersionCodeToken(int versionCode)
        {
            return Mathf.Max(1, versionCode).ToString();
        }

        private static string SanitizeBuildNameToken(string value)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return "unknown";

            return Regex.Replace(normalized, @"[^a-zA-Z0-9._-]+", "-");
        }

        private static void OpenFolder(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = directory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch
            {
                // Best effort only.
            }
        }
    }
}
