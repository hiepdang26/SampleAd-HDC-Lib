using System;
using System.Collections.Generic;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.Mediation.IOS;

namespace BG_Library.NET.AdCore.MainIOS
{
	internal sealed class IOSConfigsAdapter : IIOS_Configs
	{
		private readonly Configs configs;

		public IOSConfigsAdapter(Configs configs)
		{
			this.configs = configs;
		}

		public IOS_FAInfo[] GetIOSFAInfo()
		{
			if (configs?.ForceAdGroups == null || configs.ForceAdGroups.Length == 0)
				return Array.Empty<IOS_FAInfo>();

			var list = new List<IOS_FAInfo>();

			for (int i = 0; i < configs.ForceAdGroups.Length; i++)
			{
				var group = configs.ForceAdGroups[i];
				if (group == null) continue;

				var platform = group.AndroidUnit;
				if (string.IsNullOrEmpty(platform.Id)) continue;
				if (string.IsNullOrEmpty(group.GroupName)) continue;

				list.Add(new IOS_FAInfo(
					id: platform.Id,
					layoutGroup: ResolveLayoutGroup(platform.LayoutGroupName),
					groupName: group.GroupName,
					maxShowCount: group.MaxShowCount,
					mediationPriority: group.MediationPriority,
					androidInterstitials: platform.AndroidInterstitials,
					disablePostInitReload: group.DisablePostInitReload));
			}

			return list.ToArray();
		}

		public IOS_RWInfo GetIOSRWInfo()
		{
			if (configs?.RewardedUnit == null)
				return new IOS_RWInfo("", null);

			var platform = configs.RewardedUnit.AndroidUnit;
			if (string.IsNullOrEmpty(platform.Id))
				return new IOS_RWInfo("", null);

			return new IOS_RWInfo(
				id: platform.Id,
				layoutGroup: ResolveLayoutGroup(platform.LayoutGroupName));
		}

		public IOS_BNInfo GetIOSBNInfo()
		{
			if (configs?.BannerUnit == null)
				return new IOS_BNInfo("", Array.Empty<string>(), Array.Empty<string>(), 0);

			var slot = configs.BannerUnit.FullBottom;
			var platform = slot?.AndroidUnit ?? default;
			var primaryId = string.IsNullOrEmpty(platform.Id) ? FirstNonEmpty(platform.Ids) : platform.Id;

			if (string.IsNullOrEmpty(primaryId))
				return new IOS_BNInfo("", Array.Empty<string>(), Array.Empty<string>(), 0);

			return new IOS_BNInfo(
				id: primaryId,
				ids: platform.Ids,
				layouts: platform.Layouts,
				timeReload: platform.ReloadTime);
		}

		public IOS_PUInfo[] GetIOSPUInfo()
		{
			if (configs?.PopupGroups == null || configs.PopupGroups.Length == 0)
				return Array.Empty<IOS_PUInfo>();

			var list = new List<IOS_PUInfo>(configs.PopupGroups.Length);

			for (int i = 0; i < configs.PopupGroups.Length; i++)
			{
				var group = configs.PopupGroups[i];
				if (group == null) continue;

				var platform = group.AndroidUnit;
				if (string.IsNullOrEmpty(platform.Id)) continue;
				if (string.IsNullOrEmpty(group.GroupName)) continue;

				list.Add(new IOS_PUInfo(
					id: platform.Id,
					layout: platform.Layout,
					layouts: Array.Empty<string>(),
					adSourceLayouts: platform.AdSourceLayouts,
					timeShow: platform.TimeShow,
					timeReload: platform.ReloadTime,
					groupName: group.GroupName,
					disablePostInitReload: group.DisablePostInitReload));
			}

			return list.ToArray();
		}

		private LayoutGroupConfig ResolveLayoutGroup(string groupName)
		{
			if (configs == null || string.IsNullOrEmpty(groupName))
				return null;

			try
			{
				return configs.GetLayoutGroupConfigByGroupName(groupName);
			}
			catch
			{
				return null;
			}
		}

		private static string FirstNonEmpty(string[] values)
		{
			if (values == null)
				return string.Empty;

			for (int i = 0; i < values.Length; i++)
			{
				if (!string.IsNullOrEmpty(values[i]))
					return values[i];
			}

			return string.Empty;
		}
	}
}
