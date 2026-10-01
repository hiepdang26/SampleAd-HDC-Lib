using BG_Library.NET.Mediation.Admob;
using Cysharp.Threading.Tasks;
using System.Threading;
using BG_Library.NET.AdSystem;

namespace AppBootstrap.Splash
{
    internal sealed class SplashSdkInitializationService : ISplashSdkInitializationService
    {
        public async UniTask WaitUntilReadyAsync(CancellationToken cancellationToken)
        {
            SplashLogger.Log("WaitSdkInitialization start");
            await UniTask.WhenAll(
                WaitFirebaseInitialization(cancellationToken),
                WaitAbMobInitialization(cancellationToken));
            SplashLogger.Log("WaitSdkInitialization complete");
        }

        private static async UniTask WaitFirebaseInitialization(CancellationToken cancellationToken)
        {
            SplashLogger.Log("Waiting Firebase RemoteConfig fetch complete");
            await UniTask.WaitUntil(() => RemoteConfig.Ins.IsDataFetched, cancellationToken: cancellationToken);
            SplashLogger.Log("Firebase RemoteConfig fetch complete");
        }

        private static async UniTask WaitAbMobInitialization(CancellationToken cancellationToken)
        {
            SplashLogger.Log("Waiting Admob initialization complete");
            await UniTask.WaitUntil(() => Admob_MediationManager.IsInitComplete, cancellationToken: cancellationToken);
            SplashLogger.Log("Admob initialization complete");
        }
    }
}
