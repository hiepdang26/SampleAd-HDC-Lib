using System;

namespace BG_Library.NET
{
    public static partial class NetEventSystem
    {
        public static Action<bool> OnIapRemovedAd;

        public static Action OnAdCoreInitCompleted;
    }
}
