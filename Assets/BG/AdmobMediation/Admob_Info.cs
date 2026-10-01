using BG_Library.NET.AdSystem;
using BG_Library.NET.Mediation.Base;

namespace BG_Library.NET.Mediation.Admob
{
    public enum AdmobRectPlacement
    {
        Mrec = 0,
        FullBottom = 1,
        FullTop = 2,
        TopLeft = 3,
        TopRight = 4,
        BottomLeft = 5,
        BottomRight = 6
    }

    public class Admob_FSInfo : InfoBase
    {
        private readonly bool preloadAd;
        private readonly int adBufferSize;

        public Admob_FSInfo(string id, bool preloadAd, int adBufferSize) : base(id)
        {
            this.preloadAd = preloadAd;
            this.adBufferSize = adBufferSize;
        }

        public bool PreloadAd => preloadAd;
        public int AdBufferSize => adBufferSize;
        public virtual bool DisablePostInitReload => false;
    }

    public class Admob_AOInfo : Admob_FSInfo
    {
        public Admob_AOInfo(string id, bool preloadAd, int adBufferSize) : base(id, preloadAd, adBufferSize)
        {
        }

        public override string Id
        {
            get
            {
                if (NetConfigsSO.Ins.Admob_TestId)
                {
                    return "ca-app-pub-3940256099942544/9257395921";
                }
                else
                {
                    return id;
                }
            }
        }
    }

    public class Admob_RWInfo : Admob_FSInfo
    {
        public Admob_RWInfo(string id, bool preloadAd, int adBufferSize) : base(id, preloadAd, adBufferSize)
        {
        }

        public override string Id
        {
            get
            {
                if (NetConfigsSO.Ins.Admob_TestId)
                {
                    return "ca-app-pub-3940256099942544/5224354917";
                }
                else
                {
                    return id;
                }
            }
        }
    }

    public class Admob_FAInfo : Admob_FSInfo
    {
        private readonly string groupName;
        private readonly int maxShowCount; // 0 = unlimited (default)
        private readonly bool disablePostInitReload;

        public Admob_FAInfo(string id, bool preloadAd, int adBufferSize, string groupName, int maxShowCount,
            bool disablePostInitReload = false)
        : base(id, preloadAd, adBufferSize)
        {
            this.groupName = groupName;
            this.maxShowCount = maxShowCount;
            this.disablePostInitReload = disablePostInitReload;
        }

        public override string Id
        {
            get
            {
                if (NetConfigsSO.Ins.Admob_TestId)
                {
                    return "ca-app-pub-3940256099942544/1033173712";
                }
                else
                {
                    return id;
                }
            }
        }

        public string GroupName => groupName;
        public int MaxShowCount => maxShowCount;
        public override bool DisablePostInitReload => disablePostInitReload;
    }

    public abstract class Admob_RectInfo : InfoBase
    {
        public Admob_RectInfo(string id) : base(id)
        {
        }

        public abstract AdmobRectPlacement Placement { get; }
    }

	public class Admob_BNInfo : Admob_RectInfo
    {
        private readonly AdmobRectPlacement placement;

		public Admob_BNInfo(string id, AdmobRectPlacement placement) : base(id)
		{
            this.placement = placement;
		}

		public override string Id
		{
			get
			{
				if (NetConfigsSO.Ins.Admob_TestId)
				{
                    return placement switch
                    {
                        AdmobRectPlacement.FullBottom or AdmobRectPlacement.FullTop
                            => "ca-app-pub-3940256099942544/9214589741",
                        _ => "ca-app-pub-3940256099942544/6300978111"
                    };
				}
				else
				{
					return id;
				}
			}
		}

        public override AdmobRectPlacement Placement => placement;
	}

	public class Admob_MrecInfo : Admob_RectInfo
    {
		public Admob_MrecInfo(string id) : base(id)
		{
		}

		public override string Id
		{
			get
			{
				if (NetConfigsSO.Ins.Admob_TestId)
				{
					return "ca-app-pub-3940256099942544/6300978111";
				}
				else
				{
					return id;
				}
			}
		}

        public override AdmobRectPlacement Placement => AdmobRectPlacement.Mrec;
    }
}
