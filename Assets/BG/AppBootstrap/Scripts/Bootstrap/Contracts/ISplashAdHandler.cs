using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    internal interface ISplashAdHandler
    {
        AdGroupType GroupType { get; }

        void Initialize(string groupName);

        bool IsReady(string groupName);

        void AdShow(AdGroupEntry groupEntry, AdInfoConfig adInfoConfig, IAdStorage adStorage, SplashAdExecutionResult result, SplashAdShowContext context, CancellationToken cancellationToken);
        UniTask WaitToCompleteHandlerAsync(AdGroupEntry groupEntry, IAdStorage adStorage, CancellationToken cancellationToken);
    }
}
