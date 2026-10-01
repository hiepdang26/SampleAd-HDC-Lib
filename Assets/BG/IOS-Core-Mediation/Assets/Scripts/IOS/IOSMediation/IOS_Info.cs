using System;
using System.Collections.Generic;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Mediation.Android;
using BG_Library.NET.Mediation.Base;

namespace BG_Library.NET.Mediation.IOS
{
	public abstract class IOS_FSInfo : InfoBase
	{
		protected const string TestNativeAdUnitId = "ca-app-pub-3940256099942544/3986624511";

		private readonly LayoutGroupConfig layoutGroup;
		private readonly AndroidInterstitials androidInterstitials;

		protected IOS_FSInfo(string id, LayoutGroupConfig layoutGroup, AndroidInterstitials androidInterstitials = default) : base(id)
		{
			this.layoutGroup = layoutGroup;
			this.androidInterstitials = androidInterstitials;
		}

		public LayoutGroupConfig LayoutGroup => layoutGroup;
		public AndroidInterstitials AndroidInterstitials => androidInterstitials;
		protected virtual string TestAdUnitId => TestNativeAdUnitId;

		public override string Id
		{
			get
			{
				if (NetConfigsSO.Ins.Admob_TestId)
					return TestAdUnitId;

				return id;
			}
		}

		public virtual bool DisablePostInitReload => false;
		public abstract bool IsRewarded { get; }
	}

	public class IOS_RWInfo : IOS_FSInfo
	{
		public IOS_RWInfo(string id, LayoutGroupConfig layoutGroup) : base(id, layoutGroup)
		{
		}

		public override bool IsRewarded => true;
	}

	public class IOS_FAInfo : IOS_FSInfo
	{
		private const string TestInterstitialAdUnitId = "ca-app-pub-3940256099942544/4411468910";

		private readonly string groupName;
		private readonly int maxShowCount;
		private readonly E_MediationPriority mediationPriority;
		private readonly bool disablePostInitReload;

		public IOS_FAInfo(
			string id,
			LayoutGroupConfig layoutGroup,
			string groupName,
			int maxShowCount,
			E_MediationPriority mediationPriority = E_MediationPriority.Android,
			AndroidInterstitials androidInterstitials = default,
			bool disablePostInitReload = false)
			: base(id, layoutGroup, androidInterstitials)
		{
			this.groupName = groupName;
			this.maxShowCount = maxShowCount;
			this.mediationPriority = mediationPriority;
			this.disablePostInitReload = disablePostInitReload;
		}

		public string GroupName => groupName;
		public int MaxShowCount => maxShowCount;
		public E_MediationPriority MediationPriority => mediationPriority;
		protected override string TestAdUnitId =>
			MediationPriority == E_MediationPriority.Android ? TestNativeAdUnitId : TestInterstitialAdUnitId;
		public override bool DisablePostInitReload => disablePostInitReload;
		public override bool IsRewarded => false;
	}

	public abstract class IOS_RectBaseInfo : InfoBase
	{
		private const string TestNativeAdUnitId = "ca-app-pub-3940256099942544/3986624511";

		protected readonly string layout;
		protected readonly string[] layouts;

		protected IOS_RectBaseInfo(string id, string layout, string[] layouts) : base(id)
		{
			this.layout = layout;
			this.layouts = layouts ?? Array.Empty<string>();
		}

		public override string Id
		{
			get
			{
				if (NetConfigsSO.Ins.Admob_TestId)
					return TestNativeAdUnitId;

				return id;
			}
		}

		public string Layout => layout;
		public string[] Layouts => layouts;
		public virtual bool DisablePostInitReload => false;

		protected static void AddUnique(List<string> target, string value)
		{
			if (target == null || string.IsNullOrWhiteSpace(value))
				return;

			var trimmed = value.Trim();
			if (!target.Contains(trimmed))
				target.Add(trimmed);
		}

		protected static string JoinCsv(IEnumerable<string> values)
		{
			if (values == null)
				return string.Empty;

			var list = new List<string>();
			foreach (var value in values)
				AddUnique(list, value);

			return list.Count == 0 ? string.Empty : string.Join(",", list);
		}
	}

	public class IOS_BNInfo : IOS_RectBaseInfo
	{
		private readonly int timeReload;
		private readonly List<string> ids;

		public IOS_BNInfo(string id, string[] ids, string[] layouts, int timeReload) : base(id, null, layouts)
		{
			this.timeReload = timeReload;
			this.ids = new List<string>();
			AddUnique(this.ids, id);

			if (ids == null)
				return;

			for (int i = 0; i < ids.Length; i++)
				AddUnique(this.ids, ids[i]);
		}

		public int TimeReload => timeReload;
		public string IdsCsv => JoinCsv(ids);
		public string LayoutsCsv => JoinCsv(layouts);
	}

	public class IOS_PUInfo : IOS_RectBaseInfo
	{
		private readonly string groupName;
		private readonly int timeShow;
		private readonly int timeReload;
		private readonly bool disablePostInitReload;
		private readonly AdSourceLayout[] adSourceLayouts;

		public IOS_PUInfo(
			string id,
			string layout,
			string[] layouts,
			AdSourceLayout[] adSourceLayouts,
			int timeShow,
			int timeReload,
			string groupName,
			bool disablePostInitReload = false)
			: base(id, layout, layouts)
		{
			this.groupName = groupName;
			this.timeShow = timeShow;
			this.timeReload = timeReload;
			this.disablePostInitReload = disablePostInitReload;
			this.adSourceLayouts = adSourceLayouts ?? Array.Empty<AdSourceLayout>();
		}

		public string GroupName => groupName;
		public int TimeShow => timeShow;
		public int TimeReload => timeReload;
		public override bool DisablePostInitReload => disablePostInitReload;
		public AdSourceLayout[] AdSourceLayouts => adSourceLayouts;
		public string IdsCsv => Id;
	}
}
