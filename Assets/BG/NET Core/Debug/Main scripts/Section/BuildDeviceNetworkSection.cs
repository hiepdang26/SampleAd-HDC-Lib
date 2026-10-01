using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace BG_Library.DEBUG
{
    [DisallowMultipleComponent]
    public sealed class BuildDeviceNetworkSection : MonoBehaviour
    {
        private const string IpLookupUrl = "https://ipwho.is/";
        private const float ProbeCooldownSeconds = 60f;

        [Serializable]
        private sealed class IpWhoIsConnection
        {
            public string isp = string.Empty;
            public string org = string.Empty;
            public string domain = string.Empty;
        }

        [Serializable]
        private sealed class IpWhoIsResponse
        {
            public bool success = false;
            public string ip = string.Empty;
            public string type = string.Empty;
            public string city = string.Empty;
            public string region = string.Empty;
            public string country = string.Empty;
            public string country_code = string.Empty;
            public float latitude = 0f;
            public float longitude = 0f;
            public string timezone = string.Empty;
            public IpWhoIsConnection connection = new IpWhoIsConnection();
            public string message = string.Empty;
        }

        [SerializeField] private Text titleText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Button openViewerButton;
        [SerializeField] private DebugDetailViewer detailViewer;

        private IpWhoIsResponse publicInfo;
        private string probeError;
        private bool isProbeRunning;
        private float lastProbeRealtime = -999f;
        private DateTime? lastProbeUtc;

        private void Awake()
        {
            ApplyStaticText();
            BindActions();
        }

        private void OnEnable()
        {
            TryBindNetworkMonitor();
        }

        private void OnDisable()
        {
            TryUnbindNetworkMonitor();
        }

        private void Start()
        {
            Refresh();
            TriggerProbeIfNeeded(true);
        }

        public void Refresh()
        {
            summaryText.text = BuildSummaryText();
        }

        private void ApplyStaticText()
        {
            if (titleText != null)
                titleText.text = "Build / device / network";

            if (hintText != null)
                hintText.text = "Quick view for the current build, device, and public network location used for Firebase country conditions.";
        }

        private void BindActions()
        {
            if (openViewerButton == null)
                return;

            openViewerButton.onClick.RemoveAllListeners();
            openViewerButton.onClick.AddListener(OpenViewer);
        }

        private void OpenViewer()
        {
            TriggerProbeIfNeeded(false);
            if (detailViewer != null)
                detailViewer.Open("Build | Device | Network", BuildViewerText());
        }

        private void TryBindNetworkMonitor()
        {
            var monitor = FindFirstObjectByType<NetworkMonitor>();
            if (monitor == null)
                return;

            monitor.OnStatusUpdated -= OnNetworkStatusUpdated;
            monitor.OnStatusUpdated += OnNetworkStatusUpdated;
        }

        private void TryUnbindNetworkMonitor()
        {
            var monitor = FindFirstObjectByType<NetworkMonitor>();
            if (monitor == null)
                return;

            monitor.OnStatusUpdated -= OnNetworkStatusUpdated;
        }

        private void OnNetworkStatusUpdated(NetworkMonitor.NetStatus status)
        {
            Refresh();
            if (status.hasInternet)
                TriggerProbeIfNeeded(false);
        }

        private void TriggerProbeIfNeeded(bool force)
        {
            if (!isActiveAndEnabled || isProbeRunning)
                return;

            var networkStatus = NetworkMonitor.Current;
            if (!networkStatus.hasInternet && !force)
                return;

            if (!force && Time.realtimeSinceStartup - lastProbeRealtime < ProbeCooldownSeconds)
                return;

            StartCoroutine(CoProbePublicNetworkInfo());
        }

        private IEnumerator CoProbePublicNetworkInfo()
        {
            isProbeRunning = true;
            probeError = null;
            Refresh();

            using (var request = UnityWebRequest.Get(IpLookupUrl))
            {
                request.timeout = 6;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    probeError = string.IsNullOrEmpty(request.error) ? "Probe failed." : request.error;
                    publicInfo = null;
                }
                else
                {
                    TryApplyPublicInfo(request.downloadHandler.text);
                }
            }

            lastProbeRealtime = Time.realtimeSinceStartup;
            lastProbeUtc = DateTime.UtcNow;
            isProbeRunning = false;
            Refresh();
        }

        private void TryApplyPublicInfo(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                publicInfo = null;
                probeError = "Empty response.";
                return;
            }

            try
            {
                var parsed = JsonUtility.FromJson<IpWhoIsResponse>(json);
                if (parsed == null)
                {
                    publicInfo = null;
                    probeError = "Invalid response.";
                    return;
                }

                if (!parsed.success)
                {
                    publicInfo = null;
                    probeError = string.IsNullOrEmpty(parsed.message) ? "Lookup failed." : parsed.message;
                    return;
                }

                publicInfo = parsed;
                probeError = null;
            }
            catch (Exception exception)
            {
                publicInfo = null;
                probeError = exception.Message;
            }
        }

        private string BuildSummaryText()
        {
            var networkStatus = NetworkMonitor.Current;
            string publicLine;

            if (isProbeRunning)
            {
                publicLine = "Public: probing...";
            }
            else if (publicInfo != null)
            {
                publicLine =
                    $"Public: {ValueOrDash(publicInfo.ip)} | {BuildLocationLine()} | {BuildProviderLine()}";
            }
            else if (!string.IsNullOrEmpty(probeError))
            {
                publicLine = $"Public: unavailable ({probeError})";
            }
            else
            {
                publicLine = "Public: waiting for first probe...";
            }

            return
                $"Version: {Application.version} | Platform: {Application.platform}\n" +
                $"Device: {SystemInfo.deviceModel} | OS: {SystemInfo.operatingSystem}\n" +
                $"Network: {Application.internetReachability} | {BuildNetworkSummaryLine(networkStatus)}\n" +
                $"{publicLine}";
        }

        private string BuildViewerText()
        {
            var networkStatus = NetworkMonitor.Current;
            string batteryLevel = SystemInfo.batteryLevel < 0f ? "-" : $"{SystemInfo.batteryLevel * 100f:0}%";

            return CombineSections(
                "Build",
                $"App version: {Application.version}\n" +
                $"Unity version: {Application.unityVersion}\n" +
                $"Platform: {Application.platform}\n" +
                $"Identifier: {Application.identifier}\n" +
                $"Language: {Application.systemLanguage}",
                "Device",
                $"Device name: {SystemInfo.deviceName}\n" +
                $"Device model: {SystemInfo.deviceModel}\n" +
                $"Device type: {SystemInfo.deviceType}\n" +
                $"Operating system: {SystemInfo.operatingSystem}\n" +
                $"CPU: {SystemInfo.processorType} | count={SystemInfo.processorCount}\n" +
                $"System memory: {SystemInfo.systemMemorySize} MB\n" +
                $"GPU: {SystemInfo.graphicsDeviceName}\n" +
                $"GPU memory: {SystemInfo.graphicsMemorySize} MB\n" +
                $"Screen: {Screen.width}x{Screen.height} @ {Screen.dpi:0.#} dpi\n" +
                $"Battery: {SystemInfo.batteryStatus} | {batteryLevel}",
                "Network",
                $"Reachability: {Application.internetReachability}\n" +
                $"Has link: {networkStatus.hasLink}\n" +
                $"Has internet: {networkStatus.hasInternet}\n" +
                $"CheckNetwork: {networkStatus.CheckNetwork}\n" +
                $"Latency: {networkStatus.latencyMs} ms\n" +
                $"Monitor error: {ValueOrDash(networkStatus.error)}\n" +
                $"Monitor UTC time: {networkStatus.utcTime:O}",
                "Public Network",
                BuildPublicNetworkDetails());
        }

        private string BuildPublicNetworkDetails()
        {
            if (publicInfo == null)
            {
                var state = isProbeRunning ? "Probing..." : "Unavailable";
                return
                    $"State: {state}\n" +
                    $"Probe error: {ValueOrDash(probeError)}\n" +
                    $"Last update: {FormatUtc(lastProbeUtc)}";
            }

            string latLon = publicInfo.latitude != 0f || publicInfo.longitude != 0f
                ? $"{publicInfo.latitude:0.####}, {publicInfo.longitude:0.####}"
                : "-";

            return
                $"Public IP: {ValueOrDash(publicInfo.ip)}\n" +
                $"Country: {ValueOrDash(publicInfo.country)} ({ValueOrDash(publicInfo.country_code)})\n" +
                $"Region: {ValueOrDash(publicInfo.region)}\n" +
                $"City: {ValueOrDash(publicInfo.city)}\n" +
                $"Provider: {BuildProviderLine()}\n" +
                $"Service: {ValueOrDash(publicInfo.type)}\n" +
                $"Lat/Lon: {latLon}\n" +
                $"Timezone: {ValueOrDash(publicInfo.timezone)}\n" +
                $"Last update: {FormatUtc(lastProbeUtc)}\n" +
                $"Probe error: {ValueOrDash(probeError)}";
        }

        private string BuildProviderLine()
        {
            if (publicInfo == null)
                return "-";

            if (publicInfo.connection != null)
            {
                if (!string.IsNullOrEmpty(publicInfo.connection.isp))
                    return publicInfo.connection.isp;

                if (!string.IsNullOrEmpty(publicInfo.connection.org))
                    return publicInfo.connection.org;
            }

            return "-";
        }

        private string BuildLocationLine()
        {
            if (publicInfo == null)
                return "-";

            if (!string.IsNullOrEmpty(publicInfo.city) && !string.IsNullOrEmpty(publicInfo.country))
                return $"{publicInfo.city}, {publicInfo.country}";

            if (!string.IsNullOrEmpty(publicInfo.region) && !string.IsNullOrEmpty(publicInfo.country))
                return $"{publicInfo.region}, {publicInfo.country}";

            return ValueOrDash(publicInfo.country);
        }

        private static string BuildNetworkSummaryLine(NetworkMonitor.NetStatus networkStatus)
        {
            return $"link={(networkStatus.hasLink ? "on" : "off")} internet={(networkStatus.hasInternet ? "on" : "off")} latency={networkStatus.latencyMs}ms";
        }

        private static string ValueOrDash(string value)
        {
            return string.IsNullOrEmpty(value) ? "-" : value;
        }

        private static string FormatUtc(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("O") : "-";
        }

        private static string CombineSections(params string[] blocks)
        {
            if (blocks == null || blocks.Length == 0)
                return string.Empty;

            var text = string.Empty;
            for (int i = 0; i + 1 < blocks.Length; i += 2)
            {
                if (text.Length > 0)
                    text += "\n\n";

                text += $"[{blocks[i]}]\n{blocks[i + 1]}";
            }

            return text;
        }
    }
}
