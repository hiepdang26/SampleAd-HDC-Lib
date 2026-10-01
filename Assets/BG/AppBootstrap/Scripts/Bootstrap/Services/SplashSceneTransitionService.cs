using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace AppBootstrap.Splash
{
    internal sealed class SplashSceneTransitionService : ISplashSceneTransitionService
    {
        public async UniTask<bool> LoadNextSceneAsync(string sceneName, CancellationToken cancellationToken)
        {
            SplashLogger.Log($"Loading next scene sceneName={sceneName}");
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                SplashLogger.Warn($"Failed to create load operation for next scene sceneName={sceneName}");
                return false;
            }

            await operation.ToUniTask(cancellationToken: cancellationToken);
            SplashLogger.Log($"Next scene loaded sceneName={sceneName}");
            return true;
        }
    }
}
