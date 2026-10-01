using System;
using System.Collections.Generic;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.AdSystem;
using BG_Library.NET.AndroidSDK;
using BG_Library.NET.Mediation.Base;

namespace BG_Library.NET.Mediation.Android
{
    #region FS

    public abstract class Android_FSInfo : InfoBase
    {
        private readonly LayoutGroupConfig layoutGroup;
        private readonly int[] adTimes;
        private readonly AndroidInterstitials androidInterstitials;
        
        public Android_FSInfo(string id, AndroidInterstitials androidInterstitials, LayoutGroupConfig layoutGroup) : base(id)
        {
            this.layoutGroup = layoutGroup;
            this.androidInterstitials = androidInterstitials;
        }

        public LayoutGroupConfig LayoutGroup => layoutGroup;
        public int[] AdTimes => adTimes;
        public AndroidInterstitials AndroidInterstitials => androidInterstitials;
        
        public virtual bool DisablePostInitReload => false;
        public abstract bool IsRewarded { get; }
    }
    
    public class Android_RWInfo : Android_FSInfo
    {
        public Android_RWInfo(string id, LayoutGroupConfig layoutGroup/*, bool pauseGameplay, bool enableAdComeback = true,
            bool showTCD = true*/)
        : base(id, default, layoutGroup/*, pauseGameplay, enableAdComeback, showTCD*/)
        {
        }

        public override string Id
        {
            get
            {
                if (NetConfigsSO.Ins.Admob_TestId)
                {
                    return "ca-app-pub-3940256099942544/1044960115";
                }
                else
                {
                    return id;
                }
            }
        }

        public override bool IsRewarded => true;
    }

    public class Android_FAInfo : Android_FSInfo
    {
        private readonly string groupName;
        private readonly int maxShowCount;
        private readonly bool disablePostInitReload;
        private readonly AndroidInterstitials androidInterstitials;

        public Android_FAInfo(string id, LayoutGroupConfig layoutGroup, string groupName, int maxShowCount, AndroidInterstitials androidInterstitials, bool disablePostInitReload = false)
        : base(id, androidInterstitials, layoutGroup)
        {
            this.groupName = groupName;
            this.maxShowCount = maxShowCount;
            this.disablePostInitReload = disablePostInitReload;
            this.androidInterstitials = androidInterstitials;
        }

        public override string Id
        {
            get
            {
                if (NetConfigsSO.Ins.Admob_TestId) return "ca-app-pub-3940256099942544/1044960115";
                return id;
            }
        }

        public string GroupName => groupName;
        public int MaxShowCount => maxShowCount;
        public AndroidInterstitials AndroidInterstitials => androidInterstitials;
        public override bool DisablePostInitReload => disablePostInitReload;

        public override bool IsRewarded => false;
    }

    #endregion

    #region RectAd

    public abstract class Android_RectBaseInfo : InfoBase
    {
        protected readonly string layout;
        protected readonly string[] layouts;

        protected Android_RectBaseInfo(string id, string layout, string[] layouts) : base(id)
        {
            this.layout = layout;
            this.layouts = layouts ?? Array.Empty<string>();
        }

        public override string Id
        {
            get
            {
                if (NetConfigsSO.Ins.Admob_TestId) return "ca-app-pub-3940256099942544/2247696110";
                return id;
            }
        }

        public string Layout => layout;
        public string[] Layouts => layouts;
        public virtual bool DisablePostInitReload => false;

        public abstract RectAdInstance CreateAdInstance();
    }

    public class Android_BNInfo : Android_RectBaseInfo
    {
        private readonly int timeReload;
        private readonly List<string> Ids;

        public Android_BNInfo(string id, string[] ids, string[] layouts, int timeReload) : base(id, null, layouts)
        {
            this.timeReload = timeReload;
            this.Ids = new List<string>();
            this.Ids.Add(id);
            this.Ids.AddRange(ids);
        }

        public int TimeReload => timeReload;

        public override RectAdInstance CreateAdInstance()
            => new(Ids.ToArray(), null, layouts, Array.Empty<AdSourceLayout>(), 0, timeReload, false);
    }

    public class Android_PUInfo : Android_RectBaseInfo
    {
        private readonly string groupName;
		private readonly int timeShow;
		private readonly int timeReload;
        private readonly bool disablePostInitReload;
        private readonly AdSourceLayout[] adSourceLayouts;

		public Android_PUInfo(string id, string layout, string[] layouts, AdSourceLayout[] adSourceLayouts, int timeShow, int timeReload, string groupName, bool disablePostInitReload = false) : base(id, layout, layouts)
        {
            this.groupName = groupName;
			this.timeShow = timeShow;
			this.timeReload = timeReload;
            this.adSourceLayouts = adSourceLayouts;
            this.disablePostInitReload = disablePostInitReload;
		}

        public string GroupName => groupName;
		public int TimeShow => timeShow;
		public int TimeReload => timeReload;
        public override bool DisablePostInitReload => disablePostInitReload;

		public override RectAdInstance CreateAdInstance()
			 => new(new string[] { Id }, layout, layouts, adSourceLayouts, timeShow, disablePostInitReload ? int.MaxValue : timeReload, true, !disablePostInitReload);
	}

    public class Android_CLInfo : Android_RectBaseInfo
    {
        private readonly int timeClose;
        private readonly int reloadByClick;
        private readonly int reloadByHiddenTime;
        private readonly bool enableHiddenReload;

        public Android_CLInfo(string id, string layout, string[] layouts, int timeClose, int reloadByClick, int reloadByHiddenTime,
            bool enableHiddenReload) : base(id, layout, layouts)
        {
            this.timeClose = timeClose;
            this.reloadByClick = reloadByClick;
            this.reloadByHiddenTime = reloadByHiddenTime;
            this.enableHiddenReload = enableHiddenReload;
        }

        public int ReloadByClick => reloadByClick;
        public int ReloadByHiddenTime => reloadByHiddenTime;

        public override RectAdInstance CreateAdInstance()
            => new(Id, layout, layouts, timeClose, reloadByClick, reloadByHiddenTime, enableHiddenReload);
    }

    #endregion
}
