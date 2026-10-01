using System.Threading;
using Cysharp.Threading.Tasks;

namespace AppBootstrap.Splash
{
    internal interface IAdStorage
    {
        void Initialize();

        AdGroupStatus GetStatus(AdGroupType groupType, string groupName);
        void SetStatus(AdGroupType groupType, string groupName, AdGroupStatus status);
        
        AdGroupDataEntry GetAdGroupDataEntry(AdGroupType groupType, string groupName); 

        UniTask<AdGroupStatus> LoadAdsAsync(ISplashAdHandler handler, AdGroupType groupType, string groupName,
            float timeoutSeconds, CancellationToken cancellationToken);
    }
}
