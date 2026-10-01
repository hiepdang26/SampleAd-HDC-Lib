using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

public sealed class NetworkMonitor : MonoBehaviour
{
    public struct NetStatus
    {
        public readonly NetworkReachability Reachability => Application.internetReachability;
        public bool hasLink;         // OS says reachable (wifi/cellular)
        public bool hasInternet;     // Verified by web request
        public long latencyMs;
        public string error;
        public DateTime utcTime;

        public readonly bool CheckNetwork => Reachability != NetworkReachability.NotReachable && hasInternet;
    }

    [Header("Config")]
    [Min(1f)] public float intervalSeconds = 10f;
    [Min(1)] public int timeoutSeconds = 3;

    [Header("State (read-only)")]
    [SerializeField] private bool hasLink;
    [SerializeField] private bool hasInternet;
    [SerializeField] private long latencyMs;
    [SerializeField] private string lastError;

    public static NetStatus Current { get; private set; }

    public event Action<NetStatus> OnStatusChanged;
    public event Action<NetStatus> OnStatusUpdated;

    // Endpoints designed for connectivity checks
    // - generate_204 is great for detecting captive portal (expects 204, empty body)
    // - add fallback in case some domains are blocked
    private static readonly string[] Urls =
    {
        "https://clients3.google.com/generate_204",
        "https://www.google.com/generate_204",
        "https://connectivitycheck.gstatic.com/generate_204",
        "https://www.cloudflare.com/cdn-cgi/trace"
    };

    private Coroutine _co;

    private void OnEnable()
    {
        _co = StartCoroutine(CoLoop());
    }

    private void OnDisable()
    {
        if (_co != null) StopCoroutine(_co);
        _co = null;
    }

    private IEnumerator CoLoop()
    {
        // Run immediately once
        yield return CoCheckAndNotify();

        var wait = new WaitForSeconds(intervalSeconds);
        while (true)
        {
            yield return wait;
            yield return CoCheckAndNotify();
        }
    }

    private IEnumerator CoCheckAndNotify()
    {
        var newStatus = new NetStatus
        {
            utcTime = DateTime.UtcNow
        };

        newStatus.hasLink = newStatus.Reachability != NetworkReachability.NotReachable;

        // If OS says no link, we skip web check (fast + no waste)
        if (!newStatus.hasLink)
        {
            newStatus.hasInternet = false;
            newStatus.latencyMs = 0;
            newStatus.error = "NotReachable";
            ApplyAndNotify(newStatus);
            yield break;
        }

        // Verify real internet
        yield return CoVerifyInternet(newStatus, s =>
        {
            newStatus.hasInternet = s.hasInternet;
            newStatus.latencyMs = s.latencyMs;
            newStatus.error = s.error;
        });

        ApplyAndNotify(newStatus);
    }

    private IEnumerator CoVerifyInternet(NetStatus baseStatus, Action<NetStatus> result)
    {
        // Try multiple endpoints until one confirms
        for (int i = 0; i < Urls.Length; i++)
        {
            var url = Urls[i];

            var sw = Stopwatch.StartNew();
            using (var req = UnityWebRequest.Head(url))
            {
                // Note: timeout is ignored on some platforms (e.g., older WebGL),
                // but it works on most mobile/desktop.
                req.timeout = timeoutSeconds;

                yield return req.SendWebRequest();
                sw.Stop();

                var s = baseStatus;
                s.latencyMs = sw.ElapsedMilliseconds;

                if (req.result == UnityWebRequest.Result.Success)
                {
                    // Heuristic:
                    // - For generate_204 endpoints: responseCode should be 204
                    // - For cloudflare trace: typically 200 with small body; accept 200 as "internet ok"
                    var code = req.responseCode;

                    bool ok =
                        (url.Contains("generate_204") && code == 204) ||
                        (!url.Contains("generate_204") && (code >= 200 && code < 400));

                    if (ok)
                    {
                        s.hasInternet = true;
                        s.error = null;
                        result(s);
                        yield break;
                    }

                    // Captive portal often returns 200 instead of 204 for generate_204
                    s.hasInternet = false;
                    s.error = $"UnexpectedCode:{code}";
                    // continue to next endpoint
                }
                else
                {
                    s.hasInternet = false;
                    s.error = string.IsNullOrEmpty(req.error) ? req.result.ToString() : req.error;
                    // continue to next endpoint
                }
            }
        }

        // All endpoints failed
        var fail = baseStatus;
        fail.hasInternet = false;
        fail.latencyMs = 0;
        fail.error = "AllEndpointsFailed";
        result(fail);
    }

    private void ApplyAndNotify(NetStatus newStatus)
    {
        bool changed = (newStatus.hasLink != hasLink) || (newStatus.hasInternet != hasInternet);

        hasLink = newStatus.hasLink;
        hasInternet = newStatus.hasInternet;
        latencyMs = newStatus.latencyMs;
        lastError = newStatus.error;

        Current = newStatus;

        OnStatusUpdated?.Invoke(Current);
        if (changed) OnStatusChanged?.Invoke(Current);
    }
}