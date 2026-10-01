using BG_Library.NET.Mediation.Base;

namespace BG_Library.NET.Mediation.Max
{
    public enum MaxRectPlacement
    {
        Mrec = 0,
        FullBottom = 1,
        FullTop = 2,
        TopLeft = 3,
        TopRight = 4,
        BottomLeft = 5,
        BottomRight = 6
    }

    public class Max_FAInfo : InfoBase
    {
        public Max_FAInfo(string id) : base(id)
        {
        }
    }

    public class Max_AOInfo : InfoBase
    {
        public Max_AOInfo(string id) : base(id)
        {
        }
    }

    public class Max_RWInfo : InfoBase
    {
        public Max_RWInfo(string id) : base(id)
        {
        }
    }

	public class Max_BNInfo : InfoBase
	{
        public MaxRectPlacement Placement { get; }

		public Max_BNInfo(string id, MaxRectPlacement placement) : base(id)
		{
            Placement = placement;
		}
	}

	public class Max_MrecInfo : InfoBase
	{
		public Max_MrecInfo(string id) : base(id)
		{
		}

        public MaxRectPlacement Placement => MaxRectPlacement.Mrec;
	}
}
