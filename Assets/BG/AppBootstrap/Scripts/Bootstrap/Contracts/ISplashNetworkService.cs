using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    internal interface ISplashNetworkService
    {
        NetworkDetected.Status GetNetworkStatus();
        UniTask WaitUntilInternetAvailableAsync(Action<NetworkDetected.Status> onCompleteTrackingInternet, CancellationToken cancellationToken);
    }
}
