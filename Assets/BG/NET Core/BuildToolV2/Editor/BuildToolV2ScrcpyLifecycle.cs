using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2ScrcpyLifecycle
    {
        private static readonly Dictionary<string, Process> RunningScrcpyProcesses = new Dictionary<string, Process>(System.StringComparer.OrdinalIgnoreCase);
        private static ScheduledScrcpyLaunch pendingScrcpyLaunch;
        private static bool isScrcpyLaunchUpdateHookRegistered;

        public static bool IsRunning(string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                return false;

            if (!RunningScrcpyProcesses.TryGetValue(serial, out Process process))
                return false;

            if (process == null)
            {
                RunningScrcpyProcesses.Remove(serial);
                return false;
            }

            try
            {
                if (process.HasExited)
                {
                    RunningScrcpyProcesses.Remove(serial);
                    process.Dispose();
                    return false;
                }

                return true;
            }
            catch
            {
                RunningScrcpyProcesses.Remove(serial);
                return false;
            }
        }

        public static bool IsLaunchScheduled(string serial)
        {
            if (pendingScrcpyLaunch == null || string.IsNullOrWhiteSpace(serial))
                return false;

            return string.Equals(pendingScrcpyLaunch.DeviceSerial, serial, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool Stop(string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                return false;

            if (!RunningScrcpyProcesses.TryGetValue(serial, out Process process) || process == null)
                return false;

            try
            {
                if (!process.HasExited)
                    process.Kill();
            }
            catch
            {
                return false;
            }
            finally
            {
                RunningScrcpyProcesses.Remove(serial);
                process.Dispose();
            }

            return true;
        }

        public static bool Launch(BuildAndroidDeviceInfo device, BuildToolScrcpySettingsV2 settings, bool restartIfRunning, out string message)
        {
            message = string.Empty;

            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
            {
                message = "No Android device is selected.";
                return false;
            }

            string scrcpyPath = BuildToolV2ExecutablePaths.GetScrcpyExecutablePath(settings);
            if (string.IsNullOrWhiteSpace(scrcpyPath))
            {
                message = "scrcpy.exe was not found. Set a custom path or add it to PATH.";
                return false;
            }

            if (restartIfRunning)
                Stop(device.Serial);
            else if (IsRunning(device.Serial))
            {
                message = "scrcpy is already running for the selected device.";
                return true;
            }

            string arguments = BuildToolV2ScrcpyArguments.Build(device.Serial, settings ?? new BuildToolScrcpySettingsV2());
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = scrcpyPath,
                        Arguments = arguments,
                        UseShellExecute = true,
                        CreateNoWindow = false,
                    },
                    EnableRaisingEvents = true,
                };

                process.Exited += (_, __) =>
                {
                    RunningScrcpyProcesses.Remove(device.Serial);
                    process.Dispose();
                };

                if (!process.Start())
                {
                    message = "scrcpy failed to start.";
                    process.Dispose();
                    return false;
                }

                RunningScrcpyProcesses[device.Serial] = process;
                message = $"scrcpy started for {device.DisplayName}.";
                return true;
            }
            catch (System.Exception exception)
            {
                message = $"scrcpy failed: {exception.Message}";
                return false;
            }
        }

        public static void ScheduleLaunch(BuildAndroidDeviceInfo device, BuildToolScrcpySettingsV2 settings, bool restartIfRunning)
        {
            if (device == null || string.IsNullOrWhiteSpace(device.Serial) || settings == null)
                return;

            CancelScheduledLaunch();

            pendingScrcpyLaunch = new ScheduledScrcpyLaunch
            {
                DeviceSerial = device.Serial,
                Settings = settings.Clone(),
                RestartIfRunning = restartIfRunning,
                LaunchAtTime = EditorApplication.timeSinceStartup + Mathf.Max(0, settings.startupDelayMs) / 1000d,
            };

            EnsureLaunchUpdateHook();
        }

        private static void EnsureLaunchUpdateHook()
        {
            if (isScrcpyLaunchUpdateHookRegistered)
                return;

            EditorApplication.update += TickScheduledLaunch;
            isScrcpyLaunchUpdateHookRegistered = true;
        }

        private static void CancelScheduledLaunch()
        {
            pendingScrcpyLaunch = null;
            if (!isScrcpyLaunchUpdateHookRegistered)
                return;

            EditorApplication.update -= TickScheduledLaunch;
            isScrcpyLaunchUpdateHookRegistered = false;
        }

        private static void TickScheduledLaunch()
        {
            if (pendingScrcpyLaunch == null)
            {
                CancelScheduledLaunch();
                return;
            }

            if (EditorApplication.timeSinceStartup < pendingScrcpyLaunch.LaunchAtTime)
                return;

            ScheduledScrcpyLaunch launch = pendingScrcpyLaunch;
            CancelScheduledLaunch();

            BuildAndroidDeviceInfo device = BuildToolV2AndroidDeviceDiscovery.FindConnectedAndroidDevice(launch.DeviceSerial)
                ?? new BuildAndroidDeviceInfo { Serial = launch.DeviceSerial };
            if (!Launch(device, launch.Settings, launch.RestartIfRunning, out string message) && !string.IsNullOrWhiteSpace(message))
                UnityEngine.Debug.LogWarning("[BuildToolV2] " + message);
        }
    }
}
