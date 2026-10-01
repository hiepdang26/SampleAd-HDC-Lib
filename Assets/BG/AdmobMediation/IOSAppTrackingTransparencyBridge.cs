using System;
using System.Runtime.InteropServices;
using AOT;

namespace BG_Library.NET.Mediation.Admob
{
	internal static class IOSAppTrackingTransparencyBridge
	{
		private static Action<int> _authorizationCallback;

#if UNITY_IOS && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern int BG_ATT_GetAuthorizationStatus();

		[DllImport("__Internal")]
		private static extern void BG_ATT_RequestAuthorization(AuthorizationCallback callback);
#endif

		private delegate void AuthorizationCallback(int status);

		public static int GetAuthorizationStatus()
		{
#if UNITY_IOS && !UNITY_EDITOR
			return BG_ATT_GetAuthorizationStatus();
#else
			return -1;
#endif
		}

		public static void RequestAuthorization(Action<int> callback)
		{
#if UNITY_IOS && !UNITY_EDITOR
			_authorizationCallback = callback;
			BG_ATT_RequestAuthorization(HandleAuthorizationCallback);
#else
			callback?.Invoke(-1);
#endif
		}

		[MonoPInvokeCallback(typeof(AuthorizationCallback))]
		private static void HandleAuthorizationCallback(int status)
		{
			Action<int> callback = _authorizationCallback;
			_authorizationCallback = null;
			callback?.Invoke(status);
		}
	}
}
