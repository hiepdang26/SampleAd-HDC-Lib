#if UNITY_IOS
using BG_Library.NET.AdCore.MainIOS;
using UnityEditor;
using UnityEditor.Callbacks;

namespace AdsMultiplatform.Unity.Editor
{
	public static class IosAdMobPostprocess
	{
		[PostProcessBuild(101)]
		public static void ApplyKMPBuildSettings(BuildTarget target, string buildPath)
		{
			IOSKMPPostprocess.ApplyKMPBuildSettings(target, buildPath);
		}
	}
}
#endif
