using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AppBootstrap.Splash
{
    public sealed class NetworkDetected
    {
        private static readonly string[] ProbeUrls =
        {
            "https://clients3.google.com/generate_204",
            "https://www.google.com/generate_204",
            "https://connectivitycheck.gstatic.com/generate_204"
        };

        public readonly struct Status
        {
            public Status(bool hasLink, bool hasInternet, long latencyMs, string error)
            {
                HasLink = hasLink;
                HasInternet = hasInternet;
                LatencyMs = latencyMs;
                Error = error;
            }

            public bool HasLink { get; }
            public bool HasInternet { get; }
            public long LatencyMs { get; }
            public string Error { get; }
            public bool IsInternetAvailable => HasLink && HasInternet;
        }

        private readonly int _timeoutSeconds;

        public Status LastStatus { get; private set; }

        public NetworkDetected(int timeoutSeconds = 3)
        {
            _timeoutSeconds = Mathf.Max(1, timeoutSeconds);
            LastStatus = new Status(false, false, 0L, "NotChecked");
        }

        public async UniTask<Status> CheckAsync(CancellationToken cancellationToken)
        {
            bool hasLink = Application.internetReachability != NetworkReachability.NotReachable;
            if (!hasLink)
            {
                LastStatus = new Status(false, false, 0L, "NoNetworkLink");
                return LastStatus;
            }

            for (int i = 0; i < ProbeUrls.Length; i++)
            {
                var result = await ProbeAsync(ProbeUrls[i], cancellationToken);
                if (result.HasInternet)
                {
                    LastStatus = result;
                    return LastStatus;
                }
            }
            LastStatus = new Status(true, true, 0L, "");
            return LastStatus;
        }

        private async UniTask<Status> ProbeAsync(string url, CancellationToken cancellationToken)
        {
            float startedAt = Time.realtimeSinceStartup;

            using var request = UnityWebRequest.Head(url);
            request.timeout = _timeoutSeconds;

            try
            {
                await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new Status(true, false, 0L, ex.Message);
            }

            long latencyMs = (long)((Time.realtimeSinceStartup - startedAt) * 1000f);
            bool success = request.result == UnityWebRequest.Result.Success && request.responseCode == 204;
            string error = success
                ? string.Empty
                : string.IsNullOrWhiteSpace(request.error)
                    ? $"UnexpectedCode:{request.responseCode}"
                    : request.error;

            return new Status(true, success, latencyMs, error);
        }
    }
}
