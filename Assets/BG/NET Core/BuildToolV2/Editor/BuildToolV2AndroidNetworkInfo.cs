using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2AndroidNetworkInfo
    {
        public static void RefreshAndroidDeviceNetworkInfo(BuildAndroidDeviceInfo device)
        {
            if (device == null || string.IsNullOrWhiteSpace(device.Serial))
                return;

            Clear(device);
            Populate(BuildToolV2ExecutablePaths.GetAdbExecutablePath(), device);
        }

        public static string GetLocalIpAddress()
        {
            try
            {
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (netInterface.OperationalStatus != OperationalStatus.Up)
                        continue;

                    foreach (UnicastIPAddressInformation address in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                            return address.Address.ToString();
                    }
                }
            }
            catch
            {
            }

            return "Not Available";
        }

        private static void Populate(string adbPath, BuildAndroidDeviceInfo device)
        {
            string defaultInterface = GetDefaultInterface(adbPath, device.Serial);
            bool isGnirehtetClientRunning = IsGnirehtetClientRunning(adbPath, device.Serial);
            bool isVpnInterface = IsLikelyVpnInterface(defaultInterface);
            bool isVpnEnabled = isVpnInterface || IsVpnActive(adbPath, device.Serial);
            bool isUsingSharedComputerNetwork = isGnirehtetClientRunning && isVpnEnabled;

            device.NetworkInterfaceName = string.IsNullOrWhiteSpace(defaultInterface) ? "(unknown)" : defaultInterface;
            device.NetworkLocalIp = GetInterfaceIpv4Address(adbPath, device.Serial, defaultInterface);
            device.NetworkVpn = isVpnEnabled ? "On" : "Off";
            device.IsUsingSharedComputerNetwork = isUsingSharedComputerNetwork;
            device.NetworkSharedBy = isUsingSharedComputerNetwork ? "gnirehtet / reverse tether" : string.Empty;
            device.NetworkSource = DescribeSource(defaultInterface, isUsingSharedComputerNetwork, isVpnEnabled);

            PublicNetworkInfo publicInfo = isUsingSharedComputerNetwork
                ? GetComputerPublicNetworkInfo()
                : GetDevicePublicNetworkInfo(adbPath, device.Serial);

            if (publicInfo == null)
                return;

            device.NetworkPublicIp = publicInfo.PublicIp ?? string.Empty;
            device.NetworkLocation = publicInfo.LocationSummary;
            device.NetworkProvider = publicInfo.Provider ?? string.Empty;
            device.NetworkServiceType = publicInfo.ServiceType ?? string.Empty;
            device.NetworkCountry = publicInfo.country ?? string.Empty;
            device.NetworkRegion = publicInfo.RegionDisplay ?? string.Empty;
            device.NetworkCity = publicInfo.city ?? string.Empty;
        }

        private static void Clear(BuildAndroidDeviceInfo device)
        {
            device.NetworkSource = string.Empty;
            device.NetworkInterfaceName = string.Empty;
            device.NetworkLocalIp = string.Empty;
            device.NetworkVpn = string.Empty;
            device.NetworkPublicIp = string.Empty;
            device.NetworkLocation = string.Empty;
            device.NetworkProvider = string.Empty;
            device.NetworkServiceType = string.Empty;
            device.NetworkCountry = string.Empty;
            device.NetworkRegion = string.Empty;
            device.NetworkCity = string.Empty;
            device.NetworkSharedBy = string.Empty;
            device.IsUsingSharedComputerNetwork = false;
        }

        private static string GetDefaultInterface(string adbPath, string serial)
        {
            if (!BuildToolV2ProcessRunner.TryRunAdbShell(adbPath, serial, "ip route", out string output))
                return string.Empty;

            string[] lines = output.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!line.StartsWith("default"))
                    continue;

                Match match = Regex.Match(line, @"\bdev\s+([^\s]+)");
                if (match.Success)
                    return match.Groups[1].Value.Trim();
            }

            return string.Empty;
        }

        private static string GetInterfaceIpv4Address(string adbPath, string serial, string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
                return string.Empty;

            if (!BuildToolV2ProcessRunner.TryRunAdbShell(adbPath, serial, $"ip -f inet addr show {interfaceName}", out string output))
                return string.Empty;

            Match match = Regex.Match(output, @"\binet\s+(\d+\.\d+\.\d+\.\d+)");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private static bool IsVpnActive(string adbPath, string serial)
        {
            if (!BuildToolV2ProcessRunner.TryRunAdbShell(adbPath, serial, "ip addr show", out string output))
                return false;

            return Regex.IsMatch(output, @"^\d+:\s+(tun|ppp|tap)[^:]*:", RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        private static bool IsGnirehtetClientRunning(string adbPath, string serial)
        {
            const string packageName = "com.genymobile.gnirehtet";

            if (BuildToolV2ProcessRunner.TryRunAdbShell(adbPath, serial, $"pidof {packageName}", out string pidOutput)
                && !string.IsNullOrWhiteSpace(pidOutput))
            {
                return true;
            }

            if (BuildToolV2ProcessRunner.TryRunAdbShell(adbPath, serial, $"ps -A | grep {packageName}", out string processOutput)
                && processOutput.IndexOf(packageName, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (BuildToolV2ProcessRunner.TryRunAdbShell(adbPath, serial, "dumpsys connectivity", out string connectivityOutput)
                && connectivityOutput.IndexOf(packageName, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static bool IsLikelyVpnInterface(string interfaceName)
        {
            if (string.IsNullOrWhiteSpace(interfaceName))
                return false;

            return interfaceName.StartsWith("tun", System.StringComparison.OrdinalIgnoreCase)
                || interfaceName.StartsWith("ppp", System.StringComparison.OrdinalIgnoreCase)
                || interfaceName.StartsWith("tap", System.StringComparison.OrdinalIgnoreCase);
        }

        private static string DescribeSource(string interfaceName, bool isUsingSharedComputerNetwork, bool isVpnEnabled)
        {
            if (isUsingSharedComputerNetwork)
                return "Shared from PC (USB)";

            if (isVpnEnabled)
                return "Device network (VPN)";

            interfaceName ??= string.Empty;

            if (interfaceName.StartsWith("wlan", System.StringComparison.OrdinalIgnoreCase))
                return "Wi-Fi on device";
            if (interfaceName.StartsWith("rmnet", System.StringComparison.OrdinalIgnoreCase)
                || interfaceName.StartsWith("ccmni", System.StringComparison.OrdinalIgnoreCase)
                || interfaceName.StartsWith("pdp", System.StringComparison.OrdinalIgnoreCase))
                return "Mobile data on device";
            if (interfaceName.StartsWith("eth", System.StringComparison.OrdinalIgnoreCase))
                return "Ethernet on device";

            return string.IsNullOrWhiteSpace(interfaceName) ? "Unknown" : $"Device network ({interfaceName})";
        }

        private static PublicNetworkInfo GetDevicePublicNetworkInfo(string adbPath, string serial)
        {
            string[] commands =
            {
                "toybox wget -qO- https://ipconfig.io/json",
                "wget -qO- https://ipconfig.io/json",
                "curl -s https://ipconfig.io/json",
            };

            for (int i = 0; i < commands.Length; i++)
            {
                if (!BuildToolV2ProcessRunner.TryRunAdbShell(adbPath, serial, commands[i], out string output))
                    continue;

                PublicNetworkInfo info = ParsePublicNetworkInfo(output);
                if (info != null && (!string.IsNullOrWhiteSpace(info.PublicIp) || !string.IsNullOrWhiteSpace(info.LocationSummary)))
                    return info;
            }

            return null;
        }

        private static PublicNetworkInfo GetComputerPublicNetworkInfo()
        {
            return TryDownloadPublicNetworkInfo("https://ipconfig.io/json");
        }

        private static PublicNetworkInfo TryDownloadPublicNetworkInfo(string url)
        {
            try
            {
                using var client = new HttpClient { Timeout = System.TimeSpan.FromSeconds(2.5) };
                string json = client.GetStringAsync(url).GetAwaiter().GetResult();
                return ParsePublicNetworkInfo(json);
            }
            catch
            {
                return null;
            }
        }

        private static PublicNetworkInfo ParsePublicNetworkInfo(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonUtility.FromJson<PublicNetworkInfo>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
