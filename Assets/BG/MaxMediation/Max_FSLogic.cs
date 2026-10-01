using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;
using System;
using System.Collections.Generic;

namespace BG_Library.NET.Mediation.Max
{
    public sealed class Max_FSLogic<T, API>
        where T : InfoBase
        where API : IMax_FSAccessAPI, new()
    {
        private static readonly HashSet<string> attachedIds = new HashSet<string>();

        private readonly Max_FSGroupController<T, API> core;
        private readonly IMax_FSAccessAPI api;

        private readonly string idKey;

        public bool IsAttached =>
            !string.IsNullOrEmpty(idKey) && attachedIds.Contains(idKey);

        public Max_FSLogic(Max_FSGroupController<T, API> core, IMax_FSAccessAPI api)
        {
            this.core = core;
            this.api = api;
            idKey = this.core.Id;
        }

        public void Attach_Once()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.fs_group, $"Attach.Once {core.GroupName}",
                       () => $"adtype={core.Adtype} id={idKey}"))
            {
                // ✅ global idempotent by AdUnitId
                if (string.IsNullOrEmpty(idKey))
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Attach.Fail {core.GroupName}", () => "reason=idKey_empty");
                    return;
                }

                if (!attachedIds.Add(idKey))
                {
                    NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, $"Attach.Skip {core.GroupName}", () => $"reason=already_attached id={idKey}");
                    return;
                }

                // Subscribe (MaxSdkCallbacks are global => callback async => FlowNew inside dispatcher)
                api.SubLoaded((adUnitId, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"CB.Loaded {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={adUnitId} net={(adInfo != null ? adInfo.NetworkName : "")}"))
                        {
                            core.OnAdLoadedEvent(adInfo != null ? adInfo.ToString() : "", adInfo != null ? adInfo.NetworkName : "");
                        }
                    });
                });

                api.SubLoadFailed((adUnitId, errorInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"CB.LoadFailed {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={adUnitId} errNull={(errorInfo == null)}"))
                        {
                            // build errMsg safe (edge-case)
                            var errMsg = errorInfo != null ? errorInfo.ToString() : "LoadFailed (error null)";
                            core.OnAdLoadFailedEvent((int)errorInfo.Code, errMsg);
                        }
                    });
                });

                api.SubDisplayed((adUnitId, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"CB.Displayed {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={adUnitId} net={(adInfo != null ? adInfo.NetworkName : "")}"))
                        {
                            core.OnAdDisplayedEvent(adInfo != null ? adInfo.ToString() : "");
                        }
                    });
                });

                api.SubClicked((adUnitId, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"CB.Clicked {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={adUnitId} net={(adInfo != null ? adInfo.NetworkName : "")}"))
                        {
                            core.OnAdClickedEvent(adInfo != null ? adInfo.NetworkName : "");
                        }
                    });
                });

                api.SubRevenuePaid((adUnitId, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"CB.Paid {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={adUnitId} net={(adInfo != null ? adInfo.NetworkName : "")}"))
                        {
                            var rev = 0d;
                            try { if (adInfo != null) rev = adInfo.Revenue; } catch { }
                            core.OnAdRevenuePaidEvent(rev, "USD", adInfo != null ? adInfo.NetworkName : "");
                        }
                    });
                });

                api.SubHidden((adUnitId, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"CB.Hidden {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={adUnitId} net={(adInfo != null ? adInfo.NetworkName : "")}"))
                        {
                            core.OnAdHiddenEvent(adInfo != null ? adInfo.NetworkName : "");
                        }
                    });
                });

                api.SubDisplayFailed((adUnitId, errorInfo, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"CB.DisplayFailed {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={adUnitId} errNull={(errorInfo == null)}"))
                        {
                            var errMsg = errorInfo != null ? errorInfo.ToString() : "ShowFailed (error null)";
                            var errorCode = errorInfo != null ? (int)errorInfo.Code : int.MinValue;
                            core.OnAdDisplayFailedEvent(adInfo != null ? adInfo.ToString() : "", errMsg, errorCode: errorCode);
                        }
                    });
                });

                api.SubReceivedReward((adUnitId, reward, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.fs_group, $"CB.Reward {core.GroupName}",
                                   () => $"adtype={core.Adtype} id={adUnitId} net={(adInfo != null ? adInfo.NetworkName : "")}"))
                        {
                            core.OnAdReceivedRewardEvent(adInfo != null ? adInfo.ToString() : "");
                        }
                    });
                });

                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, $"Attach.OK {core.GroupName}", () => $"id={idKey}");
            }
        }

        public bool IsMine(string adUnitId) =>
            !string.IsNullOrEmpty(idKey) && string.Equals(idKey, adUnitId, StringComparison.Ordinal);
    }
}
