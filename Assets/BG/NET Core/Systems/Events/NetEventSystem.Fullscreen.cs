using System;
using BG_Library.Common;

namespace BG_Library.NET
{
    public static partial class NetEventSystem
    {
        public static Action<AdInfo> OnFsRequest;
        public static Action<AdInfo> OnFsLoaded;
        public static Action<AdInfo, string> OnFsLoadFailed;
        public static Action<AdInfo> OnFsBeforeOpen;
        public static Action<AdInfo> OnFsDisplayed;
        public static Action<AdInfo> OnFsClosed;
        public static Action<AdInfo> OnFsClicked;
        /// <summary>Ad SDK recorded the impression. Revenue tracking stays on <see cref="OnFsPaid"/>.</summary>
        public static Action<AdInfo> OnFsImpression;
        public static Action<AdInfo, AdValueInfo> OnFsPaid;
        public static Action<AdInfo> OnFsRewarded;
        public static Action<AdInfo> OnFsShowFailed;
    }
}
