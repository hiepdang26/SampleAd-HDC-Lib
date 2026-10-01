using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace AppBootstrap.Splash
{
    public sealed class SplashNetworkService : ISplashNetworkService
    {
        private readonly NetworkDetected _networkDetected;
        private NetworkDetected.Status _networkStatus;

        public SplashNetworkService(NetworkDetected networkDetected)
        {
            _networkDetected = networkDetected;
        }

        public NetworkDetected.Status GetNetworkStatus()
        {
            return _networkStatus;
        }

        public async UniTask WaitUntilInternetAvailableAsync(Action<NetworkDetected.Status> onCompleteTrackingInternet, CancellationToken cancellationToken)
        {
            //SplashLogger.Log("WaitNetworkConnection start");

            _networkStatus = await _networkDetected.CheckAsync(cancellationToken);
            //SplashLogger.Log($"Network status link={_networkStatus.HasLink} internet={_networkStatus.HasInternet} latencyMs={_networkStatus.LatencyMs} error={_networkStatus.Error}");

            if (_networkStatus.IsInternetAvailable)
            {
                //SplashLogger.Log("Internet connection ready");
            }
            else
            {
                string detail = _networkStatus.HasLink ? "Network detected but internet is not reachable yet" : "No network detected";
                //SplashLogger.Warn($"Internet not ready: {detail}");
            }
            
            onCompleteTrackingInternet?.Invoke(_networkStatus);
        }
    }
}
