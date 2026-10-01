using BG_Library.NET.Mediation.Admob;
using BG_Library.NET.AndroidSDK;
using BG_Library.NET.Mediation.Android;
using BG_Library.NET.Mediation.Max;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BG_Library.NET.AdCore.MainAndroid
{
    [System.Serializable]
    public class Configs : IAdmob_Configs, IAndroid_Configs, IMax_Configs
    {
        [SerializeField] ComebackChannelInfo comebackChannel;

        [SerializeField] AssetConfig[] assetConfigs = { new AssetConfig("default") };
        [SerializeField] ForceAdLayoutConfig forceAdLayoutConfig;
        [SerializeField] ForceAdGroupConfig[] forceAdGroups;
        [SerializeField] ForceAdMaxUnitConfig forceAdMaxUnit;
		[SerializeField] RewardedUnitConfig rewardedUnit;
        [SerializeField] AppOpenUnitConfig appOpenUnit;

		[SerializeField] BannerUnitConfig bannerUnit;
        [SerializeField] MrecUnitConfig mrecUnit;

        [SerializeField] PopupGroupConfig[] popupGroups;
		[SerializeField] CollapUnitConfig collapUnit;

		public ComebackChannelInfo ComebackChannel => comebackChannel;
        public AssetConfig[] AssetConfigs => assetConfigs ?? Array.Empty<AssetConfig>();

		public ForceAdGroupConfig[] ForceAdGroups => forceAdGroups;
        public ForceAdMaxUnitConfig ForceAdMaxUnit => forceAdMaxUnit;
		public RewardedUnitConfig RewardedUnit => rewardedUnit;
		public AppOpenUnitConfig AppOpenUnit => appOpenUnit;

		public BannerUnitConfig BannerUnit => bannerUnit;
		public MrecUnitConfig MrecUnit => mrecUnit;

        public PopupGroupConfig[] PopupGroups => popupGroups;
		public CollapUnitConfig CollapUnit => collapUnit;

        /// <summary>
        /// Tạo map: groupName -> mediationPriority (lấy từ từng ForceAdGroupConfig group)
        /// Nếu groupName trùng nhau ở nhiều group: mặc định GIỮ group đầu tiên.
        /// </summary>
        public Dictionary<string, E_MediationPriority> GetFAMediationPriorityMap()
        {
            var result = new Dictionary<string, E_MediationPriority>(StringComparer.Ordinal);

            if (forceAdGroups == null || forceAdGroups.Length == 0)
                return result;

            for (int i = 0; i < forceAdGroups.Length; i++)
            {
                var group = forceAdGroups[i];
                if (group == null) continue;

                var groupName = group.GroupName;
                if (string.IsNullOrEmpty(groupName)) continue;

                // ✅ ưu tiên group đầu tiên (không override)
                if (!result.ContainsKey(groupName))
                    result.Add(groupName, group.MediationPriority);
            }

            return result;
        }

        public string GetFAGroupByPos(string pos)
        {
            if (string.IsNullOrEmpty(pos)) return string.Empty;
            if (forceAdGroups == null) return string.Empty;

            for (int i = 0; i < forceAdGroups.Length; i++)
            {
                var group = forceAdGroups[i];
                if (group == null) continue;

                var posArr = group.PositionNames;
                if (posArr == null || posArr.Length == 0) continue;

                for (int j = 0; j < posArr.Length; j++)
                {
                    if (string.Equals(posArr[j], pos))
                        return group.GroupName ?? string.Empty;
                }
            }

            return string.Empty;
        }

        public string GetPUGroupByPos(string pos)
        {
            if (string.IsNullOrEmpty(pos)) return string.Empty;
            if (popupGroups == null) return string.Empty;

            for (int i = 0; i < popupGroups.Length; i++)
            {
                var group = popupGroups[i];
                if (group == null) continue;

                var posArr = group.PositionNames;
                if (posArr == null || posArr.Length == 0) continue;

                for (int j = 0; j < posArr.Length; j++)
                {
                    if (string.Equals(posArr[j], pos))
                        return group.GroupName ?? string.Empty;
                }
            }

            return string.Empty;
        }

        #region Admob Interface

        public Admob_FAInfo[] GetAdmobFAInfo()
        {
            if (forceAdGroups == null || forceAdGroups.Length == 0)
                return Array.Empty<Admob_FAInfo>();

            var list = new List<Admob_FAInfo>();

            for (int i = 0; i < forceAdGroups.Length; i++)
            {
                var group = forceAdGroups[i];
                if (group == null) continue;
                
                var admob = group.AdmobUnit;
                if (string.IsNullOrEmpty(admob.Id)) continue;
				if (string.IsNullOrEmpty(group.GroupName)) continue;

				list.Add(new(
                    id: admob.Id,
                    preloadAd: admob.PreloadAd,
                    adBufferSize: admob.AdBufferSize,
                    groupName: group.GroupName,
                    maxShowCount: group.MaxShowCount,
                    disablePostInitReload: !admob.PreloadAd && group.DisablePostInitReload
                ));
            }

            return list.ToArray();
        }

        public Admob_RWInfo GetAdmobRWInfo()
        {
            if (rewardedUnit == null)
                return new Admob_RWInfo("", false, 0);

            var admob = rewardedUnit.AdmobUnit;
            if (string.IsNullOrEmpty(admob.Id))
                return new Admob_RWInfo("", false, 0);

            return new Admob_RWInfo(
                id: admob.Id,
                preloadAd: admob.PreloadAd,
                adBufferSize: admob.AdBufferSize
            );
        }

        public Admob_AOInfo GetAdmobAOInfo()
        {
            if (appOpenUnit == null)
                return new Admob_AOInfo("", false, 0);

            var admob = appOpenUnit.AdmobUnit;
            if (string.IsNullOrEmpty(admob.Id))
                return new Admob_AOInfo("", false, 0);

            return new Admob_AOInfo(
                id: admob.Id,
                preloadAd: admob.PreloadAd,
                adBufferSize: admob.AdBufferSize
            );
        }

        public Admob_BNInfo GetAdmobBNId()
        {
            if (bannerUnit == null || bannerUnit.FullBottom == null)
                return new Admob_BNInfo("", AdmobRectPlacement.FullBottom);

            var admob = bannerUnit.FullBottom.AdmobUnit;
            if (string.IsNullOrEmpty(admob.Id))
                return new Admob_BNInfo("", AdmobRectPlacement.FullBottom);

            return new Admob_BNInfo(admob.Id, AdmobRectPlacement.FullBottom);
        }

        public Admob_BNInfo[] GetAdmobBNInfos()
        {
            if (bannerUnit == null)
                return CreateEmptyBannerInfos();

            return new[]
            {
                CreateBannerInfo(AdmobRectPlacement.FullBottom, bannerUnit.FullBottom?.AdmobUnit.Id),
                CreateBannerInfo(AdmobRectPlacement.FullTop, bannerUnit.FullTop?.AdmobUnit.Id),
                CreateBannerInfo(AdmobRectPlacement.TopLeft, bannerUnit.TopLeft?.AdmobUnit.Id),
                CreateBannerInfo(AdmobRectPlacement.TopRight, bannerUnit.TopRight?.AdmobUnit.Id),
                CreateBannerInfo(AdmobRectPlacement.BottomLeft, bannerUnit.BottomLeft?.AdmobUnit.Id),
                CreateBannerInfo(AdmobRectPlacement.BottomRight, bannerUnit.BottomRight?.AdmobUnit.Id),
            };
        }

        private static Admob_BNInfo[] CreateEmptyBannerInfos()
        {
            return new[]
            {
                new Admob_BNInfo("", AdmobRectPlacement.FullBottom),
                new Admob_BNInfo("", AdmobRectPlacement.FullTop),
                new Admob_BNInfo("", AdmobRectPlacement.TopLeft),
                new Admob_BNInfo("", AdmobRectPlacement.TopRight),
                new Admob_BNInfo("", AdmobRectPlacement.BottomLeft),
                new Admob_BNInfo("", AdmobRectPlacement.BottomRight),
            };
        }

        private static Admob_BNInfo CreateBannerInfo(AdmobRectPlacement placement, string primaryId)
        {
            var id = primaryId ?? "";
            return new Admob_BNInfo(id, placement);
        }

        public Admob_MrecInfo GetAdmobMrecId()
        {
            if (mrecUnit == null)
                return new Admob_MrecInfo("");

            var admob = mrecUnit.AdmobUnit;
            if (string.IsNullOrEmpty(admob.Id))
                return new Admob_MrecInfo("");

            return new Admob_MrecInfo(admob.Id);
        }

        #endregion

        #region Max Interface

        public Max_FAInfo GetMaxFAInfo()
        {
            if (forceAdMaxUnit == null || string.IsNullOrEmpty(forceAdMaxUnit.MaxUnit))
                return new Max_FAInfo("");

            return new Max_FAInfo(forceAdMaxUnit.MaxUnit);
        }

        public Max_RWInfo GetMaxRWInfo()
        {
            if (rewardedUnit == null || string.IsNullOrEmpty(rewardedUnit.MaxUnit))
                return new Max_RWInfo("");

            return new Max_RWInfo(rewardedUnit.MaxUnit);
        }

        public Max_AOInfo GetMaxAOInfo()
        {
            if (appOpenUnit == null || string.IsNullOrEmpty(appOpenUnit.MaxUnit))
                return new Max_AOInfo("");

            return new Max_AOInfo(appOpenUnit.MaxUnit);
        }

        public Max_BNInfo[] GetMaxBNInfos()
        {
            if (bannerUnit == null)
                return CreateEmptyMaxBannerInfos();

            return new[]
            {
                CreateMaxBannerInfo(MaxRectPlacement.FullBottom, bannerUnit.FullBottom?.MaxUnit),
                CreateMaxBannerInfo(MaxRectPlacement.FullTop, bannerUnit.FullTop?.MaxUnit),
                CreateMaxBannerInfo(MaxRectPlacement.TopLeft, bannerUnit.TopLeft?.MaxUnit),
                CreateMaxBannerInfo(MaxRectPlacement.TopRight, bannerUnit.TopRight?.MaxUnit),
                CreateMaxBannerInfo(MaxRectPlacement.BottomLeft, bannerUnit.BottomLeft?.MaxUnit),
                CreateMaxBannerInfo(MaxRectPlacement.BottomRight, bannerUnit.BottomRight?.MaxUnit),
            };
        }

        private static Max_BNInfo[] CreateEmptyMaxBannerInfos()
        {
            return new[]
            {
                new Max_BNInfo("", MaxRectPlacement.FullBottom),
                new Max_BNInfo("", MaxRectPlacement.FullTop),
                new Max_BNInfo("", MaxRectPlacement.TopLeft),
                new Max_BNInfo("", MaxRectPlacement.TopRight),
                new Max_BNInfo("", MaxRectPlacement.BottomLeft),
                new Max_BNInfo("", MaxRectPlacement.BottomRight),
            };
        }

        private static Max_BNInfo CreateMaxBannerInfo(MaxRectPlacement placement, string id)
        {
            return new Max_BNInfo(id ?? "", placement);
        }

        public Max_MrecInfo GetMaxMrecId()
        {
            if (mrecUnit == null || string.IsNullOrEmpty(mrecUnit.MaxUnit))
                return new Max_MrecInfo("");

            return new Max_MrecInfo(mrecUnit.MaxUnit);
        }

        #endregion

        #region Android Interface

        public Android_FAInfo[] GetAndroidFAInfo()
        {
            if (forceAdGroups == null || forceAdGroups.Length == 0)
                return Array.Empty<Android_FAInfo>();

            var list = new List<Android_FAInfo>();

            for (int i = 0; i < forceAdGroups.Length; i++)
            {
                var group = forceAdGroups[i];
                if (group == null) continue;

                var android = group.AndroidUnit;

                if (string.IsNullOrEmpty(android.Id)) continue;

                var layoutGroup = GetLayoutGroupConfigByGroupName(android.LayoutGroupName);

                list.Add(new(
                    id: android.Id,
                    layoutGroup: layoutGroup,
                    /*pauseGameplay: android.PauseGameplay,*/
                    groupName: group.GroupName,
                    maxShowCount: group.MaxShowCount,
                    androidInterstitials: group.AndroidUnit.AndroidInterstitials,
                    disablePostInitReload: group.DisablePostInitReload
                    /*enableAdComeback: android.EnableAdComeback,
                    showTCD: android.ShowTCD*/
                ));
            }

            return list.ToArray();
        }

        public Android_RWInfo GetAndroidRWInfo()
        {
            if (rewardedUnit == null)
                return new Android_RWInfo("", null);

            var android = rewardedUnit.AndroidUnit;

            if (string.IsNullOrEmpty(android.Id))
                return new Android_RWInfo("", null);

            var layoutGroup = GetLayoutGroupConfigByGroupName(android.LayoutGroupName);

            return new Android_RWInfo(
                id: android.Id,
                layoutGroup: layoutGroup/*,
                pauseGameplay: android.PauseGameplay,
                enableAdComeback: android.EnableAdComeback,
                showTCD: android.ShowTCD*/
            );
        }

        public Android_BNInfo GetAndroidBNInfo()
        {
            if (bannerUnit == null)
                return new Android_BNInfo("", Array.Empty<string>(), Array.Empty<string>(), 0);

            var slot = bannerUnit.FullBottom;
            var android = slot?.AndroidUnit ?? default;

            if (android.Ids.Length == 0) 
                return new Android_BNInfo("", Array.Empty<string>(), Array.Empty<string>(), 0);

            return new Android_BNInfo(
                id: android.Id,
                ids: android.Ids,
                layouts: android.Layouts,
                timeReload: android.ReloadTime
            );
        }


        public Android_PUInfo[] GetAndroidPUInfo()
		{
			if (popupGroups == null || popupGroups.Length == 0)
				return Array.Empty<Android_PUInfo>();

			var list = new List<Android_PUInfo>(popupGroups.Length);

			for (int i = 0; i < popupGroups.Length; i++)
			{
				var group = popupGroups[i];
				if (group == null) continue;

				var android = group.AndroidUnit;
				if (string.IsNullOrEmpty(android.Id)) continue;
				if (string.IsNullOrEmpty(group.GroupName)) continue;

				list.Add(new Android_PUInfo(
					id: android.Id,
					layout: android.Layout,
                    layouts: Array.Empty<string>(),
                    adSourceLayouts: android.AdSourceLayouts,
                    timeShow: android.TimeShow,
					timeReload: android.ReloadTime,
					groupName: group.GroupName,
                    disablePostInitReload: group.DisablePostInitReload
				));
			}

			return list.ToArray();
		}

        public Android_CLInfo GetAndroidCLInfo()
        {
            if (collapUnit == null)
                return new Android_CLInfo("", "", Array.Empty<string>(), 0, 0, 0, false);

            var android = collapUnit.AndroidUnit;

            if (string.IsNullOrEmpty(android.Id))
                return new Android_CLInfo("", "", Array.Empty<string>(), 0, 0, 0, false);

            return new Android_CLInfo(
                id: android.Id,
                layout: android.Layout,
                layouts: android.Layouts,
                timeClose: android.TimeClose,
                reloadByClick: android.ReloadByClick,
                reloadByHiddenTime: android.ReloadByHiddenTime,
                enableHiddenReload: android.EnableHiddenReload
            );
        }

        public LayoutGroupConfig GetLayoutGroupConfigByGroupName(string groupName)
        {
            for (var i = 0; i < forceAdLayoutConfig.LayoutGroups.Length; i++)
            {
                var layoutGroup = forceAdLayoutConfig.LayoutGroups[i];
                if (layoutGroup == null) continue;
                if (layoutGroup.GroupName != groupName) continue;
                return layoutGroup.WithAssetConfigs(AssetConfigs);
            }
            return null;
        }

        #endregion
    }

    [Serializable]
    public class AssetsConfig
    {
        [SerializeField] AssetClickConfig[] configs = { new AssetClickConfig("default") };

        public AssetClickConfig[] Configs => configs ?? Array.Empty<AssetClickConfig>();

        public NativeClickAssetOptions Resolve(string configName)
        {
            if (string.IsNullOrEmpty(configName))
                return null;

            var allConfigs = Configs;
            for (var i = 0; i < allConfigs.Length; i++)
            {
                var config = allConfigs[i];
                if (config == null) continue;
                if (config.ConfigName != configName) continue;
                return config.ClickAssets;
            }

            return null;
        }
    }

    [Serializable]
    public class AssetClickConfig
    {
        [SerializeField] string configName;
        [SerializeField] bool enableCLCTA = true;
        [SerializeField] bool enableCLHeadline = true;
        [SerializeField] bool enableCLBody = true;
        [SerializeField] bool enableCLDescription = true;
        [SerializeField] bool enableCLIcon = true;
        [SerializeField] bool enableCLAdvertiser = true;
        [SerializeField] bool enableCLMedia = true;
        [SerializeField] bool enableCLMediaImage = true;
        [SerializeField] bool enableCLMediaVideo = true;

        public AssetClickConfig()
        {
        }

        public AssetClickConfig(string configName)
        {
            this.configName = configName;
        }

        public string ConfigName => configName;
        public global::NativeClickAssetOptions ClickAssets => new global::NativeClickAssetOptions(
            enableCLCTA,
            enableCLHeadline,
            enableCLBody,
            enableCLDescription,
            enableCLIcon,
            enableCLAdvertiser,
            enableCLMedia,
            enableCLMedia && enableCLMediaImage,
            enableCLMedia && enableCLMediaVideo);
    }
    
    [Serializable]
    public class ForceAdLayoutConfig
    {
        [SerializeField] LayoutGroupConfig[] layoutGroups;
        public LayoutGroupConfig[] LayoutGroups => layoutGroups ?? Array.Empty<LayoutGroupConfig>();
    }

    [Serializable]
    public class LayoutGroupConfig
    {
        [SerializeField] string groupName;
        [SerializeField] LayoutConfig[] layouts;
        [SerializeField] AdSourceGroup[] adSourceGroups;
        [NonSerialized] AssetConfig[] assetConfigs;
        
        private LayoutGroupConfig(string groupName, LayoutConfig[] layouts, AdSourceGroup[] adSourceGroups)
        {
            this.groupName = groupName;
            this.layouts = layouts;
            this.adSourceGroups = adSourceGroups;
        }
        
        public string GroupName => groupName;
        public LayoutConfig[] Layouts => layouts ?? Array.Empty<LayoutConfig>();
        public AdSourceGroup[] AdSourceGroups => adSourceGroups ?? Array.Empty<AdSourceGroup>();

        internal LayoutGroupConfig WithAssetConfigs(AssetConfig[] assetConfigs)
        {
            this.assetConfigs = assetConfigs;
            return this;
        }

        public NativeAssetVisibilityOptions ResolveAssetVisibility(string assetConfigName)
        {
            return AssetConfig.Resolve(assetConfigs, assetConfigName);
        }
    }

    [Serializable]
    public class AssetConfig
    {
        [SerializeField] string assetConfigName;
        [SerializeField] bool show_cta = true;
        [SerializeField] bool show_headline = true;
        [SerializeField] bool show_body = true;
        [SerializeField] bool show_description = true;
        [SerializeField] bool show_icon = true;
        [SerializeField] bool show_advertiser = true;
        [SerializeField] bool show_media = true;
        [SerializeField] bool show_media_image = true;
        [SerializeField] bool show_media_video = true;
        [SerializeField] bool show_star_rating = true;
        [SerializeField] bool show_store = true;
        [SerializeField] bool show_price = true;

        public AssetConfig()
        {
        }

        public AssetConfig(string assetConfigName)
        {
            this.assetConfigName = assetConfigName;
        }

        public string AssetConfigName => assetConfigName;
        public NativeAssetVisibilityOptions VisibilityOptions => new NativeAssetVisibilityOptions(
            show_cta,
            show_headline,
            show_body,
            show_description,
            show_icon,
            show_advertiser,
            show_media,
            show_media && show_media_image,
            show_media && show_media_video,
            show_star_rating,
            show_store,
            show_price);

        public static NativeAssetVisibilityOptions Resolve(AssetConfig[] configs, string assetConfigName)
        {
            if (string.IsNullOrEmpty(assetConfigName))
                return null;

            if (configs == null)
                return null;

            for (var i = 0; i < configs.Length; i++)
            {
                var config = configs[i];
                if (config == null) continue;
                if (!string.Equals(config.AssetConfigName, assetConfigName, StringComparison.Ordinal)) continue;
                return config.VisibilityOptions;
            }

            return null;
        }
    }

    [Serializable]
    public class AdSourceGroup
    {
        [SerializeField] string[] adSourceIds;
        [SerializeField] LayoutConfig[] layouts;
        
        private AdSourceGroup(string[] adSourceIds, LayoutConfig[] layouts)
        {
            this.adSourceIds = adSourceIds;
            this.layouts = layouts;
        }
        
        public string[] AdSourceIds => adSourceIds ?? Array.Empty<string>();
        public LayoutConfig[] Layouts => layouts ?? Array.Empty<LayoutConfig>();

    }

    [Serializable]
    public class ForceAdMaxUnitConfig
    {
        [SerializeField] string maxUnit;

        public string MaxUnit => maxUnit;
    }

	[Serializable]
    public class ComebackChannelInfo
    {
        [SerializeField] E_ComebackAdType launchAdType;
		[SerializeField] string launchForceAdGroupName;

		[SerializeField] E_ComebackAdType resumeAdType;
		[SerializeField] string resumeForceAdGroupName;


		public E_ComebackAdType LaunchAdType => launchAdType;
		public string LaunchForceAdGroupName => launchForceAdGroupName;

		public E_ComebackAdType ResumeAdType => resumeAdType;
		public string ResumeForceAdGroupName => resumeForceAdGroupName;

	}

    [Serializable]
    public class ForceAdGroupConfig
    {
        [SerializeField] E_MediationPriority mediationPriority;
        [SerializeField] bool useBackup = false;
        [SerializeField] string groupName;
        [SerializeField] string[] positionNames;
        [SerializeField] int maxShowCount;
        [SerializeField] bool disablePostInitReload;

        [SerializeField] FSAdmobUnit admobUnit;
        [SerializeField] ForceAdAndroidUnit androidUnit;

        public E_MediationPriority MediationPriority => mediationPriority;
        public bool UseBackup => useBackup;
        public string GroupName => groupName;
        public string[] PositionNames => positionNames;
        public int MaxShowCount => maxShowCount;
        public bool DisablePostInitReload => disablePostInitReload;

        public FSAdmobUnit AdmobUnit => admobUnit;
        public ForceAdAndroidUnit AndroidUnit => androidUnit;

        public bool IsUseMax => mediationPriority == E_MediationPriority.Max;
        public bool IsUseAdmob => MediationPriority == E_MediationPriority.Admob;
        public bool IsUseAndroid => mediationPriority == E_MediationPriority.Android;
    }

    [Serializable]
    public class RewardedUnitConfig
    {
        [SerializeField] E_MediationPriority mediationPriority;
        [SerializeField] bool useBackup = false;
        [SerializeField] FSAdmobUnit admobUnit;
        [SerializeField] ForceAdAndroidUnit androidUnit;
        [SerializeField] string maxUnit;

        public E_MediationPriority MediationPriority => mediationPriority;
        public bool UseBackup => useBackup;
        public FSAdmobUnit AdmobUnit => admobUnit;
        public ForceAdAndroidUnit AndroidUnit => androidUnit;
        public string MaxUnit => maxUnit;
    }

    [Serializable]
    public class AppOpenUnitConfig
    {
        [SerializeField] E_AdmobMaxMediationPriority mediationPriority;
        [SerializeField] bool useBackup = false;
		[SerializeField] FSAdmobUnit admobUnit;
        [SerializeField] string maxUnit;

        public E_AdmobMaxMediationPriority MediationPriority => mediationPriority;
        public bool UseBackup => useBackup;
		public FSAdmobUnit AdmobUnit => admobUnit;
        public string MaxUnit => maxUnit;
	}

    [Serializable]
    public class BannerUnitConfig
    {
        [SerializeField] BannerFullBottomSlotUnitConfig fullBottom = new();
        [SerializeField] BannerSlotUnitConfig fullTop = new();
        [SerializeField] BannerSlotUnitConfig topLeft = new();
        [SerializeField] BannerSlotUnitConfig topRight = new();
        [SerializeField] BannerSlotUnitConfig bottomLeft = new();
        [SerializeField] BannerSlotUnitConfig bottomRight = new();

        public BannerFullBottomSlotUnitConfig FullBottom => fullBottom;
        public BannerSlotUnitConfig FullTop => fullTop;
        public BannerSlotUnitConfig TopLeft => topLeft;
        public BannerSlotUnitConfig TopRight => topRight;
        public BannerSlotUnitConfig BottomLeft => bottomLeft;
        public BannerSlotUnitConfig BottomRight => bottomRight;
    }

    [Serializable]
    public class BannerSlotUnitConfig
    {
        [SerializeField] E_AdmobMaxMediationPriority mediationPriority;
        [SerializeField] bool useBackup = false;
        [SerializeField] BNAdmobUnit admobUnit;
        [SerializeField] string maxUnit;

        public E_AdmobMaxMediationPriority MediationPriority => mediationPriority;
        public bool UseBackup => useBackup;
        public BNAdmobUnit AdmobUnit => admobUnit;
        public string MaxUnit => maxUnit;
    }

    [Serializable]
    public class BannerFullBottomSlotUnitConfig
    {
        [SerializeField] E_MediationPriority mediationPriority;
        [SerializeField] bool useBackup = false;
        [SerializeField] BNAdmobUnit admobUnit;
        [SerializeField] string maxUnit;
        [SerializeField] BannerAndroidUnit androidUnit;

        public E_MediationPriority MediationPriority => mediationPriority;
        public bool UseBackup => useBackup;
        public BNAdmobUnit AdmobUnit => admobUnit;
        public string MaxUnit => maxUnit;
        public BannerAndroidUnit AndroidUnit => androidUnit;
    }


    [Serializable]
    public class MrecUnitConfig
    {
        [SerializeField] E_AdmobMaxMediationPriority mediationPriority;
		[SerializeField] bool useBackup = false;
		[SerializeField] BNAdmobUnit admobUnit;
        [SerializeField] string maxUnit;

        public E_AdmobMaxMediationPriority MediationPriority => mediationPriority;
		public bool UseBackup => useBackup;
		public BNAdmobUnit AdmobUnit => admobUnit;
        public string MaxUnit => maxUnit;
	}

    [Serializable]
    public class PopupGroupConfig
    {
		[SerializeField] string groupName;
		[SerializeField] string[] positionNames;
        [SerializeField] bool disablePostInitReload;
		[SerializeField] RectAndroidUnit androidUnit;

		public string GroupName => groupName;
		public string[] PositionNames => positionNames;
        public bool DisablePostInitReload => disablePostInitReload;
		public RectAndroidUnit AndroidUnit => androidUnit;
    }

    [Serializable]
    public class CollapUnitConfig
    {
		[SerializeField] CLAndroidUnit androidUnit;

		public CLAndroidUnit AndroidUnit => androidUnit;
	}

	public enum E_MediationPriority
    {
        Admob = 0,
        Android = 1,
        Max = 2
    }

    public enum E_AdmobMaxMediationPriority
    {
        Admob = 0,
        Max = 1
    }

    public enum E_ComebackAdType
    {
        FA = 0,
        AO = 1,
    }
}
