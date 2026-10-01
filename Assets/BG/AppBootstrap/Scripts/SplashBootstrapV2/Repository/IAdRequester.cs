using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    public interface IAdRequester
    {
        UniTask<AdStatus> LoadAsync(CancellationToken ct);
        bool IsReady();
    }
}