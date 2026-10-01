using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    internal interface ISplashSdkInitializationService
    {
        UniTask WaitUntilReadyAsync(CancellationToken cancellationToken);
    }
}
