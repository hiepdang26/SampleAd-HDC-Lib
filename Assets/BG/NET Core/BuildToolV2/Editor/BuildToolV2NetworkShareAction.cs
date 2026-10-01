using System.Diagnostics;
using System.IO;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2NetworkShareAction
    {
        public static bool ShareDeviceNetwork(BuildAndroidDeviceInfo device, BuildToolScrcpySettingsV2 settings, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            string shareToolPath = BuildToolV2ExecutablePaths.GetNetworkShareToolPath(settings);
            if (string.IsNullOrWhiteSpace(shareToolPath))
            {
                message = "No reverse-tether tool was found near scrcpy or in PATH. Install gnirehtet next to scrcpy to use Share Network.";
                return false;
            }

            string executablePath = ResolveGnirehtetExecutablePath(shareToolPath);
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                message = "A reverse-tether launcher was found, but gnirehtet.exe could not be resolved next to it. Put gnirehtet.exe beside scrcpy.exe.";
                return false;
            }

            StopLocalGnirehtetRelay();
            TryForceStopGnirehtetClient(device);

            if (!TryStartGnirehtetInTerminal(executablePath, device.Serial, out string errorMessage))
            {
                message = $"Share Network failed: {errorMessage}";
                return false;
            }

            message = $"Share Network terminal started for {device.DisplayName}. If the phone asks for VPN permission, allow it, then refresh Network.";
            return true;
        }

        public static bool StopSharedDeviceNetwork(BuildAndroidDeviceInfo device, BuildToolScrcpySettingsV2 settings, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            string shareToolPath = BuildToolV2ExecutablePaths.GetNetworkShareToolPath(settings);
            if (string.IsNullOrWhiteSpace(shareToolPath))
            {
                message = "No reverse-tether tool was found near scrcpy or in PATH. Install gnirehtet next to scrcpy to stop shared network.";
                return false;
            }

            string executablePath = ResolveGnirehtetExecutablePath(shareToolPath);
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                message = "A reverse-tether launcher was found, but gnirehtet.exe could not be resolved next to it. Put gnirehtet.exe beside scrcpy.exe.";
                return false;
            }

            bool stopWithSerial = TryRunGnirehtetCommand(executablePath, $"stop {device.Serial}", out string output);
            bool stopWithoutSerial = stopWithSerial || TryRunGnirehtetCommand(executablePath, "stop", out output);
            bool clientStopped = TryForceStopGnirehtetClient(device);
            bool relayStopped = StopLocalGnirehtetRelay();

            if (!stopWithoutSerial && !clientStopped && !relayStopped)
            {
                string details = string.IsNullOrWhiteSpace(output) ? "Unknown error." : output.Trim();
                message = $"Stop Network failed: {details}";
                return false;
            }

            message = $"Stop Network command sent for {device.DisplayName}. Wait a moment, then refresh Network.";
            return true;
        }

        private static string ResolveGnirehtetExecutablePath(string shareToolPath)
        {
            if (string.IsNullOrWhiteSpace(shareToolPath))
                return string.Empty;

            if (shareToolPath.EndsWith(".exe", System.StringComparison.OrdinalIgnoreCase) && File.Exists(shareToolPath))
                return shareToolPath;

            string directory = Path.GetDirectoryName(shareToolPath);
            if (string.IsNullOrWhiteSpace(directory))
                return string.Empty;

            string executablePath = Path.Combine(directory, "gnirehtet.exe");
            return File.Exists(executablePath) ? executablePath : string.Empty;
        }

        private static bool TryStartGnirehtetInTerminal(string executablePath, string serial, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                string workingDirectory = Path.GetDirectoryName(executablePath);
                string arguments = $"/c \"\"{executablePath}\" run {serial}\"";

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                });
                return true;
            }
            catch (System.Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
        }

        private static bool TryRunGnirehtetCommand(string executablePath, string arguments, out string output)
        {
            output = string.Empty;

            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    WorkingDirectory = Path.GetDirectoryName(executablePath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                process.Start();
                string stdOut = process.StandardOutput.ReadToEnd();
                string stdErr = process.StandardError.ReadToEnd();
                process.WaitForExit(5000);

                output = string.IsNullOrWhiteSpace(stdOut) ? stdErr : stdOut;
                return process.ExitCode == 0;
            }
            catch (System.Exception exception)
            {
                output = exception.Message;
                return false;
            }
        }

        private static bool TryForceStopGnirehtetClient(BuildAndroidDeviceInfo device)
        {
            string adbPath = BuildToolV2ExecutablePaths.GetAdbExecutablePath();
            return BuildToolV2ProcessRunner.TryRunAdbShell(
                adbPath,
                device.Serial,
                "am force-stop com.genymobile.gnirehtet",
                out _);
        }

        private static bool StopLocalGnirehtetRelay()
        {
            bool stoppedAny = false;

            try
            {
                Process[] processes = Process.GetProcessesByName("gnirehtet");
                for (int i = 0; i < processes.Length; i++)
                {
                    try
                    {
                        if (processes[i].HasExited)
                            continue;

                        processes[i].Kill();
                        processes[i].WaitForExit(2000);
                        stoppedAny = true;
                    }
                    catch
                    {
                    }
                    finally
                    {
                        processes[i].Dispose();
                    }
                }
            }
            catch
            {
            }

            return stoppedAny;
        }

    }
}
