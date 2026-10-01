using System;
using UnityEngine.Serialization;

namespace AppBootstrap.Splash
{
    [Serializable]
    internal sealed class AdGroupEntry
    {
        public float loadSeconds;
        public float showDurationSeconds;
        public bool isAdCompleteAutoHidden;
        public bool onlyLoadOnFirstOpen;
        public bool autoShowOnLoaded;

        public AdGroupType type;
        public string groupName;
        public string adsPosition;

        public bool isBackup;
        public float loadBackupSeconds;
        public string groupNameBackup;
        public string adsPositionBackup;

        public string defaultLayoutId;
        public LayoutAdSource[] layouts;
        
        //public AdLayoutConfig adLayoutConfig;
    }

    [System.Serializable]
    internal class LayoutAdSource
    {
        public string adSourceId;
        public string layoutId;
    }
}
