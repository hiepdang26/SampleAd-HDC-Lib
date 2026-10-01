using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    internal interface ISplashAdsRunner
    {
        AdShowTriggerType PendingDeferredTrigger { get; }

        UniTask RunAsync(SplashConfig config, CancellationToken cancellationToken);
        UniTask CheckAndShowDeferredAsync(AdShowTriggerType triggerType, CancellationToken cancellationToken);
        void OnEndSceneHideAd();
    }
}
