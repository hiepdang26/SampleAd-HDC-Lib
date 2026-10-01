using System;
using UnityEngine.Serialization;

namespace AppBootstrap.Splash
{
    [Serializable]
    internal sealed class AdAppLauncherEntry
    {
        public float loadSeconds;
        public float delayShowSeconds;

        public AdGroupType type;
        public AdShowTriggerType showTriggerType;
        public string groupName;
        public string adsPosition;

        public bool isBackup;
        public float loadBackupSeconds;
        public string groupNameBackup;
        public string adsPositionBackup;
    }
}
