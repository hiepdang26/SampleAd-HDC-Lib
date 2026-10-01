using System.Collections.Generic;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2AndroidDeviceDiscovery
    {
        public static BuildAndroidDeviceInfo FindConnectedAndroidDevice(string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                return null;

            BuildAndroidDeviceInfo[] devices = GetConnectedAndroidDevices();
            for (int i = 0; i < devices.Length; i++)
            {
                if (string.Equals(devices[i].Serial, serial, System.StringComparison.OrdinalIgnoreCase))
                    return devices[i];
            }

            return null;
        }

        public static BuildAndroidDeviceInfo[] GetConnectedAndroidDevices()
        {
            string adbPath = BuildToolV2ExecutablePaths.GetAdbExecutablePath();
            if (!BuildToolV2ProcessRunner.TryRunProcess(adbPath, "devices -l", out string output))
                return System.Array.Empty<BuildAndroidDeviceInfo>();

            string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            var devices = new List<BuildAndroidDeviceInfo>();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("List of devices attached"))
                    continue;

                string[] tokens = line.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2)
                    continue;

                var device = new BuildAndroidDeviceInfo
                {
                    Serial = tokens[0],
                    State = tokens[1],
                };

                for (int tokenIndex = 2; tokenIndex < tokens.Length; tokenIndex++)
                {
                    string token = tokens[tokenIndex];
                    int separatorIndex = token.IndexOf(':');
                    if (separatorIndex <= 0 || separatorIndex >= token.Length - 1)
                        continue;

                    string key = token.Substring(0, separatorIndex);
                    string value = token.Substring(separatorIndex + 1);
                    switch (key)
                    {
                        case "product":
                            device.Product = value;
                            break;
                        case "model":
                            device.Model = value.Replace('_', ' ');
                            break;
                        case "device":
                            device.Device = value;
                            break;
                        case "transport_id":
                            device.TransportId = value;
                            break;
                    }
                }

                if (string.Equals(device.State, "device", System.StringComparison.OrdinalIgnoreCase))
                {
                    device.Manufacturer = GetAdbShellProperty(adbPath, device.Serial, "ro.product.manufacturer");
                    if (string.IsNullOrWhiteSpace(device.Model))
                        device.Model = GetAdbShellProperty(adbPath, device.Serial, "ro.product.model");
                    device.AndroidVersion = GetAdbShellProperty(adbPath, device.Serial, "ro.build.version.release");
                    device.SdkInt = GetAdbShellProperty(adbPath, device.Serial, "ro.build.version.sdk");
                    device.Resolution = GetAdbPhysicalSize(adbPath, device.Serial);
                    device.Dpi = GetAdbPhysicalDensity(adbPath, device.Serial);
                }

                devices.Add(device);
            }

            return devices.ToArray();
        }

        private static string GetAdbShellProperty(string adbPath, string serial, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(serial) || string.IsNullOrWhiteSpace(propertyName))
                return string.Empty;

            if (!BuildToolV2ProcessRunner.TryRunProcess(adbPath, $"-s {serial} shell getprop {propertyName}", out string output))
                return string.Empty;

            return output.Trim();
        }

        private static string GetAdbPhysicalSize(string adbPath, string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                return string.Empty;

            if (!BuildToolV2ProcessRunner.TryRunProcess(adbPath, $"-s {serial} shell wm size", out string output))
                return string.Empty;

            string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                const string prefix = "Physical size:";
                if (!line.StartsWith(prefix))
                    continue;

                return line.Substring(prefix.Length).Trim();
            }

            return output.Trim();
        }

        private static string GetAdbPhysicalDensity(string adbPath, string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                return string.Empty;

            if (BuildToolV2ProcessRunner.TryRunProcess(adbPath, $"-s {serial} shell wm density", out string output))
            {
                string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    const string prefix = "Physical density:";
                    if (!line.StartsWith(prefix))
                        continue;

                    return line.Substring(prefix.Length).Trim();
                }

                return output.Trim();
            }

            return string.Empty;
        }
    }
}
