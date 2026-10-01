using System;
using UnityEngine;

namespace BG_Library.NET.AdSystem
{
	[Serializable]
	public class AdSystemConfigs
	{
		// Channel-level config:
		// controls how each of the 8 ad channels behaves in product logic.
		// This is separate from AdCore configs, which map channels to real groups/units/mediations.
		[SerializeField] string selectedAdCoreName;
		[SerializeField] bool alwaysTrackRevenue;
		[SerializeField] bool trackBgAdImpression;

		[SerializeField] AppLaunchChannelConfig appLaunchChannel = new();
        [SerializeField] AppResumeChannelConfig appResumeChannel = new();

        [SerializeField] ForceAdChannelConfig forceAdChannel = new();
		[SerializeField] RewardedChannelConfig rewardedChannel = new();

		[SerializeField] BannerChannelConfig bannerChannel = new();
		[SerializeField] MrecChannelConfig mrecChannel = new();

		[SerializeField] PopupChannelConfig popupChannel = new();
		[SerializeField] CollapChannelConfig collapChannel = new();

		public string SelectedAdCoreName => selectedAdCoreName;
		public bool AlwaysTrackRevenue => alwaysTrackRevenue;
		public bool TrackBgAdImpression => trackBgAdImpression;

		public AppLaunchChannelConfig AppLaunchChannel => appLaunchChannel;
        public AppResumeChannelConfig AppResumeChannel => appResumeChannel;

        public ForceAdChannelConfig ForceAdChannel => forceAdChannel;
		public RewardedChannelConfig RewardedChannel => rewardedChannel;

		public BannerChannelConfig BannerChannel => bannerChannel;
		public MrecChannelConfig MrecChannel => mrecChannel;

		public PopupChannelConfig PopupChannel => popupChannel;
		public CollapChannelConfig CollapChannel => collapChannel;

		public abstract class BaseChannelConfig
		{
			[SerializeField] protected bool isEnabled;
			[SerializeField] protected bool trackChannel = true;
			public bool IsEnabled => isEnabled;
			public bool TrackChannel => trackChannel;
		}

        [Serializable]
        public class AppResumeChannelConfig : BaseChannelConfig
        {
            [SerializeField] bool autoInit = true;
            [SerializeField] string adUnitId;
            [SerializeField] string layoutGroup;

            public bool AutoInit => autoInit;
            public string AdUnitId => adUnitId;
            public string LayoutGroup => layoutGroup;
        }

        [Serializable]
        public class AppLaunchChannelConfig : BaseChannelConfig
        {
            [SerializeField] bool autoInit = true;
            [SerializeField] int minWaitSeconds;
            [SerializeField] int timeoutSeconds;

            public bool AutoInit => autoInit;
			public int MinWaitSeconds => minWaitSeconds;
			public int TimeoutSeconds => timeoutSeconds;
        }

        [Serializable]
		public class ForceAdChannelConfig : BaseChannelConfig
		{
			[SerializeField] private float launchCappingTime;
			[SerializeField] private float minimumCappingTime;
			[SerializeField] private float cappingDecreasePerImpression;
			[SerializeField] ForceAdPositionConfig[] positionConfigs;
			[SerializeField] BreakAdChannelConfig breakAdConfig;

			public float LaunchCappingTime => launchCappingTime;
			public float MinimumCappingTime => minimumCappingTime;
			public float CappingDecreasePerImpression => cappingDecreasePerImpression;
			public ForceAdPositionConfig[] PositionConfigs => positionConfigs;
			public BreakAdChannelConfig BreakAdConfig => breakAdConfig;

            [Serializable]
			public class ForceAdPositionConfig
			{
				[SerializeField] string positionName;
				[SerializeField] bool canShow;
				[SerializeField] bool autoInit;
				[SerializeField] float cappingTime;

				[SerializeField] private float minimumCappingTime;
				[SerializeField] private float cappingDecreasePerImpression;

				public string PositionName => positionName;
				public bool AutoInit => autoInit;
				public bool CanShow => canShow;
				public float CappingTime => cappingTime;

				public float MinimumCappingTime => minimumCappingTime;
				public float CappingDecreasePerImpression => cappingDecreasePerImpression;

				public ForceAdPositionConfig(string positionName)
				{
					this.positionName = positionName;
					canShow = true;
					cappingTime = 10;
				}

				public ForceAdPositionConfig(string positionName, bool canShow, float cappingTime)
				{
					this.positionName = positionName;
					this.canShow = canShow;
					this.cappingTime = cappingTime;
				}
			}

            [Serializable]
            public class BreakAdChannelConfig : BaseChannelConfig
            {
				[SerializeField] string positionName;
                [SerializeField] int notificationLeadTimeSeconds;
                
				public string PositionName => positionName;
                public int NotificationLeadTimeSeconds => notificationLeadTimeSeconds;
            }
        }

		[Serializable]
		public class RewardedChannelConfig : BaseChannelConfig
		{
            [SerializeField] bool autoInit = true;

            public bool AutoInit => autoInit;
		}

		[Serializable]
		public class BannerChannelConfig : BaseChannelConfig
		{
            [SerializeField] BannerSlotChannelConfig fullBottom = new();
            [SerializeField] BannerSlotChannelConfig fullTop = new();
            [SerializeField] BannerSlotChannelConfig topLeft = new();
            [SerializeField] BannerSlotChannelConfig topRight = new();
            [SerializeField] BannerSlotChannelConfig bottomLeft = new();
            [SerializeField] BannerSlotChannelConfig bottomRight = new();

            public BannerSlotChannelConfig FullBottom => fullBottom;
            public BannerSlotChannelConfig FullTop => fullTop;
            public BannerSlotChannelConfig TopLeft => topLeft;
            public BannerSlotChannelConfig TopRight => topRight;
            public BannerSlotChannelConfig BottomLeft => bottomLeft;
            public BannerSlotChannelConfig BottomRight => bottomRight;

            [Serializable]
            public class BannerSlotChannelConfig
            {
                [SerializeField] bool isEnabled;
                [SerializeField] bool autoInit = true;
                [SerializeField] bool autoShowOnLoad;

                public bool IsEnabled => isEnabled;
                public bool AutoInit => autoInit;
                public bool AutoShowOnLoad => autoShowOnLoad;
            }
		}

		[Serializable]
		public class MrecChannelConfig : BaseChannelConfig
		{
            [SerializeField] bool autoInit = true;

            public bool AutoInit => autoInit;
		}

		[Serializable]
		public class PopupChannelConfig : BaseChannelConfig
		{
			[SerializeField] PopupPositionConfig[] positionConfigs;
			public PopupPositionConfig[] PositionConfigs => positionConfigs;

			[Serializable]
			public class PopupPositionConfig
			{
				[SerializeField] bool isEnabled;
				[SerializeField] bool autoInit;
				[SerializeField] string positionName;

				public bool IsEnabled => isEnabled;
				public bool AutoInit => autoInit;
				public string PositionName => positionName;

				public PopupPositionConfig(string positionName)
				{
					this.positionName = positionName;
				}
			}
		}

        [Serializable]
        public class CollapChannelConfig : BaseChannelConfig
        {
            [SerializeField] bool autoInit = true;

            public bool AutoInit => autoInit;

            public CollapChannelConfig()
            {
            }
        }
    }
}
