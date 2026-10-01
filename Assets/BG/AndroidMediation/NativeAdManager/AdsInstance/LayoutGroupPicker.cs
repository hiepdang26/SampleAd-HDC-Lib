using System.Collections.Generic;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.Mediation.Android;

namespace BG_Library.NET.AndroidSDK
{
   public class LayoutGroupPicker
    {
        private readonly LayoutGroupConfig _config;

        private readonly Dictionary<int, List<LayoutConfig>> _bags = new Dictionary<int, List<LayoutConfig>>();

        public LayoutGroupPicker(LayoutGroupConfig config) => _config = config;
        public LayoutGroupConfig LayoutGroup => _config;

        public bool TryGetLayout(string adSourceId, out LayoutConfig layout)
        {
            layout = default;
            if (_config == null) return false;

            int groupIndex = ResolveGroupIndex(adSourceId);
            LayoutConfig[] source = groupIndex >= 0
                ? _config.AdSourceGroups[groupIndex].Layouts
                : _config.Layouts;

            if (source.Length == 0) return false;

            if (!_bags.TryGetValue(groupIndex, out var bag) || bag.Count == 0)
            {
                bag = new List<LayoutConfig>(source);
                _bags[groupIndex] = bag;
            }

            int i = UnityEngine.Random.Range(0, bag.Count);
            layout = bag[i];
            bag.RemoveAt(i);
            return true;
        }

        public void Reset() => _bags.Clear();

        private int ResolveGroupIndex(string adSourceId)
        {
            if (string.IsNullOrEmpty(adSourceId)) return -1;

            var groups = _config.AdSourceGroups;
            for (int g = 0; g < groups.Length; g++)
            {
                var grp = groups[g];
                if (grp == null || grp.Layouts.Length == 0) continue;

                var ids = grp.AdSourceIds;
                for (int k = 0; k < ids.Length; k++)
                    if (ids[k] == adSourceId) return g;
            }
            return -1;
        }
    }
}
