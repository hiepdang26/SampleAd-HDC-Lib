using System;
using UnityEngine;

namespace BG_Library.NET.Mediation.Admob
{
    public interface IAdmob_Configs
    {
        Admob_FAInfo[] GetAdmobFAInfo();
        Admob_RWInfo GetAdmobRWInfo();
        Admob_AOInfo GetAdmobAOInfo();

        Admob_BNInfo GetAdmobBNId();
        Admob_BNInfo[] GetAdmobBNInfos();
        Admob_MrecInfo GetAdmobMrecId();
    }

	[Serializable]
	public struct FSAdmobUnit
	{
		[SerializeField] private string id;
		[SerializeField] private bool preloadAd;
		[SerializeField] private int adBufferSize;

		public readonly string Id => id;
		public readonly bool PreloadAd => preloadAd;
		public readonly int AdBufferSize => adBufferSize;
	}

	[Serializable]
	public struct BNAdmobUnit
	{
		[SerializeField] private string id;

		public readonly string Id => id;
	}
}
