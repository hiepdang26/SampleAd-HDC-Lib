using UnityEngine;

namespace BG_Library.Common
{
	public static class ManualOrientationController
	{
		public enum GameOrientation
		{
			Portrait,
			Landscape
		}

		// Nhớ landscape side lần gần nhất (mặc định Left)
		private static ScreenOrientation lastLandscape = ScreenOrientation.LandscapeLeft;

		public static GameOrientation GetCurrent()
		{
			return Screen.width > Screen.height
				? GameOrientation.Landscape
				: GameOrientation.Portrait;
		}

		public static bool IsPortrait() => GetCurrent() == GameOrientation.Portrait;
		public static bool IsLandscape() => GetCurrent() == GameOrientation.Landscape;

		private static void RememberLandscapeSideIfAny()
		{
			// Trên device, Screen.orientation có thể là LandscapeLeft/Right.
			// Nếu đang landscape thì lưu lại.
			var o = Screen.orientation;
			if (o == ScreenOrientation.LandscapeLeft || o == ScreenOrientation.LandscapeRight)
			{
				lastLandscape = o;
				return;
			}

			// Fallback: nếu đang landscape nhưng orientation bị Unknown/AutoRotation,
			// giữ nguyên lastLandscape (không overwrite).
			if (Screen.width > Screen.height)
			{
				// không làm gì, chỉ tránh overwrite
			}
		}

		public static void ForcePortrait()
		{
			if (IsPortrait())
			{
				Debug.Log("[Orientation] Already Portrait → Skip");
				return;
			}

			// ✅ Nhớ side trước khi chuyển sang dọc
			RememberLandscapeSideIfAny();

			Screen.autorotateToPortrait = true;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToLandscapeLeft = false;
			Screen.autorotateToLandscapeRight = false;

			Screen.orientation = ScreenOrientation.Portrait;

			Debug.Log("[Orientation] Switched → Portrait");
		}

		public static void ForceLandscape()
		{
			if (IsLandscape())
			{
				Debug.Log("[Orientation] Already Landscape → Skip");
				return;
			}

			Screen.autorotateToPortrait = false;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToLandscapeLeft = true;
			Screen.autorotateToLandscapeRight = true;

			// ✅ Quay lại đúng side đã lưu
			Screen.orientation = lastLandscape;

			Debug.Log($"[Orientation] Switched → Landscape ({lastLandscape})");
		}

		public static void Toggle()
		{
			if (IsPortrait())
				ForceLandscape();
			else
				ForcePortrait();
		}
	}
}