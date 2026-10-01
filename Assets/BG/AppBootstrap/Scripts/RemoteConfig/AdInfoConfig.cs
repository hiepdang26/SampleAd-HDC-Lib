using System;
using UnityEngine;

namespace AppBootstrap.Splash
{
    [Serializable]
    internal sealed class AdInfoConfig
    {
        public AdLayoutConfig[] adLayouts;
        public AdGroupEntry[] adEntrys;

        public AdGroupEntry GetAdGroupEntry(string adGroupName)
        {
            if (string.IsNullOrWhiteSpace(adGroupName) || adEntrys == null || adEntrys.Length == 0)
            {
                return null;
            }

            foreach (var entry in adEntrys)
            {
                if (entry == null || !string.Equals(entry.groupName, adGroupName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // entry.adLayoutConfig = GetAdLayoutConfig(entry.layoutId);
                return entry;
            }
            return null;
        }

        public AdLayoutConfig GetAdLayoutConfig(string layoutId)
        {
            if (string.IsNullOrWhiteSpace(layoutId) || adLayouts == null || adLayouts.Length == 0)
            {
                return null;
            }

            foreach (var layout in adLayouts)
            {
                if (layout == null || !string.Equals(layout.layoutId, layoutId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                return layout;
            }

            return null;
        }
    }
}
