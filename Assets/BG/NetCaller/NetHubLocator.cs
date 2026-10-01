using UnityEngine;

namespace BG_Library.NET.API
{
	public static class NetHubLocator
	{
		private static NetEventsHub _cached;
		public static NetEventsHub Hub => GetHub();

		public static NetEventsHub GetHub()
		{
			if (_cached != null) return _cached;

			_cached = Resources.Load<NetEventsHub>("NetEventsHub");

			if (_cached == null)
			{
                UnityEngine.Debug.LogWarning(
					$"[NetHubLocator] NetEventsHub not found at Resources.");
			}

			return _cached;
		}

		public static void ResetCache() => _cached = null;
	}
}