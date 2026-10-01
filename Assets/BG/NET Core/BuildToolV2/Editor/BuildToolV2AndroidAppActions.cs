using System.IO;
using UnityEditor;
using UnityEditor.Build;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2AndroidAppActions
    {
        public static string GetApplicationIdentifier(BuildTarget target)
        {
            NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target));
            return PlayerSettings.GetApplicationIdentifier(namedBuildTarget) ?? string.Empty;
        }

        public static bool InstallBuildArtifact(BuildAndroidDeviceInfo device, string buildOutputPath, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(buildOutputPath) || !File.Exists(buildOutputPath))
            {
                message = "Build artifact was not found.";
                return false;
            }

            string extension = Path.GetExtension(buildOutputPath)?.ToLowerInvariant() ?? string.Empty;
            if (extension == ".aab")
            {
                message = "AAB cannot be installed directly by this tool. Build APK or use bundletool/apks workflow.";
                return false;
            }

            if (extension != ".apk")
            {
                message = $"Unsupported build artifact: {extension}";
                return false;
            }

            string adbPath = BuildToolV2ExecutablePaths.GetAdbExecutablePath();
            string escapedPath = $"\"{buildOutputPath}\"";
            if (!BuildToolV2Utilities.TryRunProcess(adbPath, $"-s {device.Serial} install -r {escapedPath}", out string output))
            {
                message = string.IsNullOrWhiteSpace(output) ? "APK install failed." : output.Trim();
                return false;
            }

            message = string.IsNullOrWhiteSpace(output) ? "APK installed successfully." : output.Trim();
            return true;
        }

        public static bool UninstallApplication(BuildAndroidDeviceInfo device, string packageName, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(packageName))
            {
                message = "Package name is empty.";
                return false;
            }

            string adbPath = BuildToolV2ExecutablePaths.GetAdbExecutablePath();
            if (!BuildToolV2Utilities.TryRunProcess(adbPath, $"-s {device.Serial} uninstall {packageName}", out string output))
            {
                message = string.IsNullOrWhiteSpace(output) ? $"Failed to uninstall {packageName}." : output.Trim();
                return false;
            }

            message = string.IsNullOrWhiteSpace(output) ? $"Uninstalled {packageName}." : output.Trim();
            return true;
        }

        public static bool LaunchApplication(BuildAndroidDeviceInfo device, string packageName, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(packageName))
            {
                message = "Package name is empty.";
                return false;
            }

            string adbPath = BuildToolV2ExecutablePaths.GetAdbExecutablePath();
            if (!BuildToolV2Utilities.TryRunProcess(adbPath, $"-s {device.Serial} shell monkey -p {packageName} -c android.intent.category.LAUNCHER 1", out string output))
            {
                message = string.IsNullOrWhiteSpace(output) ? $"Failed to launch {packageName}." : output.Trim();
                return false;
            }

            message = $"Launch command sent for {packageName}.";
            return true;
        }

        public static bool ForceStopApplication(BuildAndroidDeviceInfo device, string packageName, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(packageName))
            {
                message = "Package name is empty.";
                return false;
            }

            string adbPath = BuildToolV2ExecutablePaths.GetAdbExecutablePath();
            if (!BuildToolV2Utilities.TryRunProcess(adbPath, $"-s {device.Serial} shell am force-stop {packageName}", out string output))
            {
                message = string.IsNullOrWhiteSpace(output) ? $"Failed to stop {packageName}." : output.Trim();
                return false;
            }

            message = string.IsNullOrWhiteSpace(output) ? $"Stopped {packageName}." : output.Trim();
            return true;
        }

        public static bool ClearApplicationData(BuildAndroidDeviceInfo device, string packageName, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(packageName))
            {
                message = "Package name is empty.";
                return false;
            }

            string adbPath = BuildToolV2ExecutablePaths.GetAdbExecutablePath();
            if (!BuildToolV2Utilities.TryRunProcess(adbPath, $"-s {device.Serial} shell pm clear {packageName}", out string output))
            {
                message = string.IsNullOrWhiteSpace(output) ? $"Failed to clear data for {packageName}." : output.Trim();
                return false;
            }

            message = string.IsNullOrWhiteSpace(output) ? $"Cleared data for {packageName}." : output.Trim();
            return true;
        }

        public static bool TurnDeviceScreenOff(BuildAndroidDeviceInfo device, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            string adbPath = BuildToolV2ExecutablePaths.GetAdbExecutablePath();
            if (!BuildToolV2Utilities.TryRunProcess(adbPath, $"-s {device.Serial} shell input keyevent 26", out string output))
            {
                message = string.IsNullOrWhiteSpace(output) ? "Failed to turn the device screen off." : output.Trim();
                return false;
            }

            message = "Power button command sent to the device.";
            return true;
        }
    }
}
