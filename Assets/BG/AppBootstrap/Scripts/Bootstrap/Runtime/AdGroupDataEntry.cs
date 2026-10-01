using BG_Library.Common;
using UnityEngine.Serialization;

namespace AppBootstrap.Splash
{
    [System.Serializable]
    public sealed class AdGroupDataEntry
    {
        public AdGroupType adType;
        public string adGroupName;
        public AdGroupStatus adGroupStatus;
        public AdInfo adInfo;
    }
}
