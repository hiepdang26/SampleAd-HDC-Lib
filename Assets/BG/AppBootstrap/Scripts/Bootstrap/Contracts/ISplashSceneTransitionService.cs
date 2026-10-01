using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    internal interface ISplashSceneTransitionService
    {
        UniTask<bool> LoadNextSceneAsync(string sceneName, CancellationToken cancellationToken);
    }
}
