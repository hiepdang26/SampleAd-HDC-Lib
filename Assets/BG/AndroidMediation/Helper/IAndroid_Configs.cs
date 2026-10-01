using System;
using BG_Library.NET.AndroidSDK;
using UnityEngine;
using UnityEngine.Serialization;

namespace BG_Library.NET.Mediation.Android
{
    public interface IAndroid_Configs
    {
        Android_FAInfo[] GetAndroidFAInfo();
        Android_RWInfo GetAndroidRWInfo();
        Android_BNInfo GetAndroidBNInfo();
        Android_PUInfo[] GetAndroidPUInfo();
        Android_CLInfo GetAndroidCLInfo();
    }

	[Serializable]
	public struct LayoutConfig : IEquatable<LayoutConfig>
	{
		[SerializeField] string layout;
		[SerializeField] string assetConfigName;
		[SerializeField] float layoutTime;
		[SerializeField] float delay;
		[SerializeField] int timeUpC;
		
		[SerializeField] bool pauseGameplay;
		[SerializeField] bool showTCD;
		[SerializeField] bool disableAdComeback;
		
		public string Layout => layout;
		public string AssetConfigName => assetConfigName;
		public float LayoutTime => layoutTime;
		public float Delay => delay;
		public int TimeUpC => timeUpC;
		
		public bool PauseGameplay => pauseGameplay;
		public bool ShowTCD => showTCD;
		public bool DisableAdComeback => disableAdComeback;

		private LayoutConfig(
			string layout,
			string assetConfigName,
			float layoutTime,
			float delay,
			int timeUpC,
			bool pauseGameplay,
			bool showTCD,
			bool disableAdComeback)
		{
			this.layout = layout;
			this.assetConfigName = assetConfigName;
			this.layoutTime = layoutTime;
			this.delay = delay;
			this.timeUpC = timeUpC;
			this.pauseGameplay = pauseGameplay;
			this.showTCD = showTCD;
			this.disableAdComeback = disableAdComeback;
		}

		public bool Equals(LayoutConfig other)
		{
			return layout == other.layout && assetConfigName == other.assetConfigName && layoutTime.Equals(other.layoutTime) && delay.Equals(other.delay) && timeUpC == other.timeUpC && pauseGameplay == other.pauseGameplay && showTCD == other.showTCD && disableAdComeback == other.disableAdComeback;
		}

		public override bool Equals(object obj)
		{
			return obj is LayoutConfig other && Equals(other);
		}

		public override int GetHashCode()
		{
			var hashCode = new HashCode();
			hashCode.Add(layout);
			hashCode.Add(assetConfigName);
			hashCode.Add(layoutTime);
			hashCode.Add(delay);
			hashCode.Add(timeUpC);
			hashCode.Add(pauseGameplay);
			hashCode.Add(showTCD);
			hashCode.Add(disableAdComeback);
			return hashCode.ToHashCode();
		}
	}

	[Serializable]
	public struct ForceAdAndroidUnit
	{
		[SerializeField] string id;
		[SerializeField] string layoutGroupName;
		[SerializeField] AndroidInterstitials androidInterstitials;
		/*[SerializeField] bool pauseGameplay;
		[SerializeField] bool showTCD;
        [SerializeField] bool disableAdComeback;*/

		public readonly string Id => id;
		public readonly string LayoutGroupName => layoutGroupName;
		public AndroidInterstitials AndroidInterstitials => androidInterstitials;
		/*public readonly bool PauseGameplay => pauseGameplay;
		public readonly bool ShowTCD => showTCD;
        public readonly bool EnableAdComeback => !disableAdComeback;*/
	}

	[Serializable]
	public struct AndroidInterstitials
	{
		[SerializeField] bool switchToInterstitialAndroid;
		[SerializeField] bool isPreloadAd;
		[SerializeField] int bufferSize;
		
		[SerializeField] bool useNativeAfterInterstitial;
		[SerializeField] string nativeAfterInterstitialId;
		[SerializeField] string nativeAfterInterstitialLayout;
		
		public bool SwitchToInterstitialAndroid => switchToInterstitialAndroid;
		public bool IsPreloadAd => isPreloadAd;
		public int BufferSize => bufferSize;
		
		public bool UseNativeAfterInterstitial => useNativeAfterInterstitial;
		public string NativeAfterInterstitialId => nativeAfterInterstitialId;
		public string NativeAfterInterstitialLayout => nativeAfterInterstitialLayout;
	}

	[Serializable]
	public struct BannerAndroidUnit
	{
		[SerializeField] string id;
		[SerializeField] string[] ids;
		[SerializeField] string[] layouts;
		[SerializeField] int reloadTime;

		public readonly string Id => id;
		public readonly string[] Ids => ids ?? Array.Empty<string>();
		public readonly string[] Layouts => layouts ?? Array.Empty<string>();
		public readonly int ReloadTime => reloadTime;
	}

	[Serializable]
	public struct RectAndroidUnit
	{
		[SerializeField] string id;
		[SerializeField] string layout;
		[SerializeField] AdSourceLayout[] adSourceLayouts;
		[SerializeField] int timeShow;
		[SerializeField] int reloadTime;
		
		public readonly string Id => id;
		public readonly string Layout => layout;
		public AdSourceLayout[] AdSourceLayouts => adSourceLayouts ?? Array.Empty<AdSourceLayout>();
		public readonly int TimeShow => timeShow;
		public readonly int ReloadTime => reloadTime;
	}

	[System.Serializable]
	public struct AdSourceLayout
	{
		[SerializeField] string[] adSources;
		[SerializeField] string layout;
		
		public string[] AdSources => adSources ?? Array.Empty<string>();
		public string Layout => layout;
	}

	[Serializable]
	public struct CLAndroidUnit
	{
		[SerializeField] string id;
		[SerializeField] string layout;
		[SerializeField] string[] layouts;
		[SerializeField] int timeClose;
		[SerializeField] int reloadByClick;
		[SerializeField] int reloadByHiddenTime;
		[SerializeField] bool enableHiddenReload;

		public readonly string Id => id;
        public readonly string Layout => layout;
		public readonly string[] Layouts => layouts ?? Array.Empty<string>();
		public readonly int TimeClose => timeClose;
        public readonly int ReloadByClick => reloadByClick;
        public readonly int ReloadByHiddenTime => reloadByHiddenTime;
		public readonly bool EnableHiddenReload => enableHiddenReload;
    }
}
