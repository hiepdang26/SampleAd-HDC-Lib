using System;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    public enum SdkPresetMediationTagV2
    {
        AdMob,
        MAX,
        Firebase,
        Adjust,
    }

    public enum SdkPresetCategoryTagV2
    {
        CoreSDK,
        MediationAdapter,
        FirebaseModule,
        Environment,
    }

    public enum SdkPresetPlatformV2
    {
        Unity,
        Android,
        IOS,
    }

    [Serializable]
    public sealed class SdkVersionPresetEntryV2
    {
        /// <summary>Legacy field; ignored by the validator and report writer. Hidden in the custom inspector.</summary>
        public bool enabled = true;
        public SdkPresetMediationTagV2 mediationTag;
        public SdkPresetCategoryTagV2 categoryTag;
        public string item = string.Empty;
        public SdkPresetPlatformV2 platform;
        public string expectedVersion = string.Empty;
    }

    [CreateAssetMenu(fileName = "SDK Version Preset", menuName = "BG_Library/Build/SDK Version Preset")]
    public sealed class SdkVersionPresetV2 : ScriptableObject
    {
        [SerializeField] private string presetCode = string.Empty;
        [SerializeField] [TextArea(2, 5)] private string notes = string.Empty;
        [SerializeField] private List<SdkVersionPresetEntryV2> entries = new List<SdkVersionPresetEntryV2>();

        public string PresetCode => string.IsNullOrWhiteSpace(presetCode) ? name : presetCode.Trim();
        public string Notes => notes ?? string.Empty;
        public IReadOnlyList<SdkVersionPresetEntryV2> Entries => entries;
        public int EntryCount => entries == null ? 0 : entries.Count;
    }

    internal readonly struct SdkVersionPresetKeyV2 : IEquatable<SdkVersionPresetKeyV2>
    {
        public readonly SdkPresetMediationTagV2 MediationTag;
        public readonly SdkPresetCategoryTagV2 CategoryTag;
        public readonly string Item;
        public readonly SdkPresetPlatformV2 Platform;

        public SdkVersionPresetKeyV2(
            SdkPresetMediationTagV2 mediationTag,
            SdkPresetCategoryTagV2 categoryTag,
            string item,
            SdkPresetPlatformV2 platform)
        {
            MediationTag = mediationTag;
            CategoryTag = categoryTag;
            Item = item?.Trim() ?? string.Empty;
            Platform = platform;
        }

        public bool Equals(SdkVersionPresetKeyV2 other)
        {
            return MediationTag == other.MediationTag
                && CategoryTag == other.CategoryTag
                && Platform == other.Platform
                && string.Equals(Item, other.Item, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return obj is SdkVersionPresetKeyV2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)MediationTag;
                hash = (hash * 397) ^ (int)CategoryTag;
                hash = (hash * 397) ^ (int)Platform;
                hash = (hash * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Item ?? string.Empty);
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{MediationTag} / {CategoryTag} / {Item} / {Platform}";
        }
    }

    internal sealed class SdkVersionInspectionEntryV2
    {
        public SdkVersionPresetKeyV2 Key;
        public string ActualVersion = string.Empty;
    }

    internal sealed class CanonicalInspectionItemV2
    {
        private readonly List<SdkPresetPlatformV2> platforms = new List<SdkPresetPlatformV2>(3);
        private readonly Dictionary<SdkPresetPlatformV2, string> actualVersionsByPlatform = new Dictionary<SdkPresetPlatformV2, string>(3);

        public CanonicalInspectionItemV2(string item, SdkPresetCategoryTagV2 category)
        {
            Item = item ?? string.Empty;
            Category = category;
        }

        public string Item { get; }
        public SdkPresetCategoryTagV2 Category { get; }
        public IReadOnlyList<SdkPresetPlatformV2> Platforms => platforms;

        public void AddPlatform(SdkPresetPlatformV2 platform, string actualVersion)
        {
            if (platforms.Contains(platform))
            {
                actualVersionsByPlatform[platform] = actualVersion ?? string.Empty;
                return;
            }

            platforms.Add(platform);
            actualVersionsByPlatform[platform] = actualVersion ?? string.Empty;
        }

        public bool HasPlatform(SdkPresetPlatformV2 platform)
        {
            return actualVersionsByPlatform.ContainsKey(platform);
        }

        public string GetActualVersion(SdkPresetPlatformV2 platform)
        {
            return actualVersionsByPlatform.TryGetValue(platform, out string version) ? version : string.Empty;
        }
    }

    internal sealed class SdkVersionPresetComparisonEntryV2
    {
        public SdkVersionPresetKeyV2 Key;
        public string ExpectedVersion = string.Empty;
        public string ActualVersion = string.Empty;
        public bool ExistsInPreset;
        public bool ExistsInProject;
        public bool IsMatch;
        public bool IsExtraInProject => ExistsInProject && !ExistsInPreset;
    }
}
