using System;
using System.Collections.Generic;
using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.AdCore.MainAndroid
{
    internal sealed class FsFallbackGroup : SequentialFallbackGroupBase<IFSGroup>, IFSGroup
    {
        private readonly GroupAdType trackingAdType;

        public FsFallbackGroup(
            List<FallbackCandidate<IFSGroup>> candidates,
            bool useBackup,
            string adtype,
            GroupAdType trackingAdType,
            string debugPrefix,
            Channel defaultTrackingChannel,
            Func<Channel?, Module> debugModuleResolver)
            : base(candidates, useBackup, adtype, debugPrefix, defaultTrackingChannel, debugModuleResolver)
        {
            this.trackingAdType = trackingAdType;
        }

        public string Id
        {
            get
            {
                var best = GetBestReadyCandidate() ?? GetBestInitializedCandidate() ?? GetPrimaryCandidate();
                return best?.Group?.Id ?? "";
            }
        }

        public string Adtype => trackingAdType switch
        {
            GroupAdType.Rewarded => BG_ConstValue.adtype_rw,
            GroupAdType.AppOpen => BG_ConstValue.adtype_ao,
            _ => ""
        };

        public GroupAdType TrackingAdType => trackingAdType;

        public string TrackingIdentitySource
        {
            get
            {
                var best = GetBestReadyCandidate() ?? GetBestInitializedCandidate() ?? GetPrimaryCandidate();
                return best?.Group?.TrackingIdentitySource ?? "";
            }
        }

        public void Initialize()
        {
            EnsureInitialized(group => group.Initialize());
        }

        public bool ForceInitialize()
        {
            var best = GetBestReadyCandidate() ?? GetBestInitializedCandidate() ?? GetPrimaryCandidate();
            if (best?.Group == null)
            {
                NetFlowDebugSystem.Warn(Layer.adcore, DebugModule, $"{DebugPrefix}.ForceInitBlocked",
                    () => $"reason=no_candidate candidates={Candidates.Count} useBackup={UseBackup}");
                return false;
            }

            NetFlowDebugSystem.Log(Layer.adcore, DebugModule, $"{DebugPrefix}.ForceInitPick",
                () => $"pick={best.Label} mediation={best.Mediation} id={best.Group.Id}");

            return best.Group.ForceInitialize();
        }

        public bool GetReady() => GetBestReadyCandidate() != null;

        public bool Show(string pos, Action OnBeforeAdShow = null, Action OnAdShowComplete = null)
        {
            var best = GetBestReadyCandidate();
            if (best == null)
            {
                NetFlowDebugSystem.Warn(Layer.adcore, DebugModule, $"{DebugPrefix}.ShowBlocked",
                    () => $"reason=no_ready pos={pos} candidates={Candidates.Count} useBackup={UseBackup}");

                if (UseBackup)
                {
                    NetTrackingSystem.ShowGroupFail(trackingAdType, pos, TrackingReason.NotReady, trackingChannel);
                    return false;
                }

                var single = GetBestInitializedCandidate() ?? GetPrimaryCandidate();
                if (single?.Group != null)
                    return single.Group.Show(pos, OnBeforeAdShow, OnAdShowComplete);

                return false;
            }

            NetFlowDebugSystem.Log(Layer.adcore, DebugModule, $"{DebugPrefix}.ShowPick",
                () => $"pick={best.Label} mediation={best.Mediation} id={best.Group.Id} pos={pos}");

            return best.Group.Show(pos, OnBeforeAdShow, OnAdShowComplete);
        }

        public string GetDebugInfo()
            => BuildDebugInfo($"{trackingAdType} Fallback", group =>
            {
                bool ready = false;
                try { ready = group != null && group.GetReady(); } catch { }
                return $"ready={ready}";
            });

        protected override void SubscribeCore()
        {
            NetEventSystem.OnFsLoadFailed += HandleFsLoadFailed;
            NetEventSystem.OnFsShowFailed += HandleFsShowFailed;
        }

        protected override void UnsubscribeCore()
        {
            NetEventSystem.OnFsLoadFailed -= HandleFsLoadFailed;
            NetEventSystem.OnFsShowFailed -= HandleFsShowFailed;
        }

        private void HandleFsLoadFailed(AdInfo info, string error)
        {
            var index = FindCandidateIndex(info);
            if (index < 0)
                return;

            NetFlowDebugSystem.Warn(Layer.adcore, DebugModule, $"{DebugPrefix}.LoadFail",
                () => $"idx={index} label={Candidates[index].Label} err={error}");

            if (index == highestInitializedIndex)
                InitializeNextCandidate("load_fail", group => group.Initialize());
        }

        private void HandleFsShowFailed(AdInfo info)
        {
            var index = FindCandidateIndex(info);
            if (index < 0)
                return;

            NetFlowDebugSystem.Warn(Layer.adcore, DebugModule, $"{DebugPrefix}.ShowFail",
                () => $"idx={index} label={Candidates[index].Label} pos={info.pos}");

            if (index == highestInitializedIndex)
                InitializeNextCandidate("show_fail", group => group.Initialize());
        }

        private FallbackCandidate<IFSGroup> GetBestReadyCandidate()
            => GetFirstCandidateMatching(group => group.GetReady(), "ReadyCheckError");
    }
}
