using System.Collections.Generic;
using System.Text;

namespace BG_Library.BuildToolV2
{
    internal sealed class BuildAndroidDeviceInfo
    {
        public string Serial;
        public string State;
        public string Product;
        public string Model;
        public string Device;
        public string TransportId;
        public string Manufacturer;
        public string AndroidVersion;
        public string SdkInt;
        public string Resolution;
        public string Dpi;
        public string NetworkSource;
        public string NetworkInterfaceName;
        public string NetworkLocalIp;
        public string NetworkVpn;
        public string NetworkPublicIp;
        public string NetworkLocation;
        public string NetworkProvider;
        public string NetworkServiceType;
        public string NetworkCountry;
        public string NetworkRegion;
        public string NetworkCity;
        public string NetworkSharedBy;
        public bool IsUsingSharedComputerNetwork;

        public string DisplayName
        {
            get
            {
                string manufacturer = string.IsNullOrWhiteSpace(Manufacturer) ? string.Empty : Manufacturer.Trim();
                string model = string.IsNullOrWhiteSpace(Model) ? "(unknown model)" : Model.Trim();
                string serial = string.IsNullOrWhiteSpace(Serial) ? "(no serial)" : Serial.Trim();
                string label = string.IsNullOrWhiteSpace(manufacturer) ? model : $"{manufacturer} {model}";
                return $"{label} [{serial}]";
            }
        }

        public string Summary
        {
            get
            {
                var builder = new StringBuilder();
                builder.Append(DisplayName);
                if (!string.IsNullOrWhiteSpace(AndroidVersion))
                    builder.Append(" | android=").Append(AndroidVersion);
                if (!string.IsNullOrWhiteSpace(SdkInt))
                    builder.Append(" (SDK ").Append(SdkInt).Append(')');
                if (!string.IsNullOrWhiteSpace(Resolution))
                    builder.Append(" | res=").Append(Resolution);
                if (!string.IsNullOrWhiteSpace(Dpi))
                    builder.Append(" | dpi=").Append(Dpi);
                return builder.ToString();
            }
        }
    }

    [System.Serializable]
    internal sealed class PublicNetworkInfo
    {
        public string ip = string.Empty;
        public string ip_decimal = string.Empty;
        public string country = string.Empty;
        public string country_iso = string.Empty;
        public bool country_eu = false;
        public string city = string.Empty;
        public string region = string.Empty;
        public double latitude = 0d;
        public double longitude = 0d;
        public string time_zone = string.Empty;
        public string asn = string.Empty;
        public string asn_org = string.Empty;

        public string RegionDisplay => region;
        public string PublicIp => ip;
        public string Provider => !string.IsNullOrWhiteSpace(asn_org) ? asn_org : asn;
        public string ServiceType => !string.IsNullOrWhiteSpace(asn) ? asn : string.Empty;

        public string LocationSummary
        {
            get
            {
                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(city))
                    parts.Add(city.Trim());
                if (!string.IsNullOrWhiteSpace(RegionDisplay))
                    parts.Add(RegionDisplay.Trim());
                if (!string.IsNullOrWhiteSpace(country))
                    parts.Add(country.Trim());

                return parts.Count == 0 ? string.Empty : string.Join(", ", parts);
            }
        }
    }

    internal sealed class ScheduledScrcpyLaunch
    {
        public string DeviceSerial;
        public BuildToolScrcpySettingsV2 Settings;
        public bool RestartIfRunning;
        public double LaunchAtTime;
    }
}
