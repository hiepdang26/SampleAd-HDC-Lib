using System;
using System.Collections.Generic;
using System.Text;
using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.AdCore.MainAndroid
{
    internal abstract class SequentialFallbackGroupBase<TGroup> : IDisposable
        where TGroup : class, IGroup
    {
        private readonly string adtype;
        private readonly string debugPrefix;
        private readonly Channel defaultTrackingChannel;
        private readonly Func<Channel?, Module> debugModuleResolver;
        private bool subscribed;

        protected readonly List<FallbackCandidate<TGroup>> candidates;
        protected readonly bool useBackup;
        protected Channel? trackingChannel;
        protected bool isInit;
        protected int highestInitializedIndex = -1;

        protected SequentialFallbackGroupBase(
            List<FallbackCandidate<TGroup>> candidates,
            bool useBackup,
            string adtype,
            string debugPrefix,
            Channel defaultTrackingChannel,
            Func<Channel?, Module> debugModuleResolver)
        {
            this.candidates = candidates ?? new List<FallbackCandidate<TGroup>>();
            this.useBackup = useBackup;
            this.adtype = adtype;
            this.debugPrefix = debugPrefix;
            this.defaultTrackingChannel = defaultTrackingChannel;
            this.debugModuleResolver = debugModuleResolver;

            SubscribeIfNeeded();
        }

        protected Module DebugModule => debugModuleResolver?.Invoke(trackingChannel) ?? Module.adcore;
        protected Channel TrackingChannelOrDefault => trackingChannel ?? defaultTrackingChannel;
        protected string DebugPrefix => debugPrefix;
        protected IReadOnlyList<FallbackCandidate<TGroup>> Candidates => candidates;
        protected bool UseBackup => useBackup;

        public void Dispose()
        {
            if (!subscribed)
                return;

            subscribed = false;
            UnsubscribeCore();
            OnDisposed();
        }

        protected void SubscribeIfNeeded()
        {
            if (subscribed)
                return;

            subscribed = true;
            SubscribeCore();
        }

        protected abstract void SubscribeCore();
        protected abstract void UnsubscribeCore();
        protected virtual void OnDisposed() { }

        public void SetTrackingChannel(Channel channel)
        {
            trackingChannel = channel;

            for (int i = 0; i < candidates.Count; i++)
                candidates[i].Group?.SetTrackingChannel(channel);
        }

        protected void EnsureInitialized(Action<TGroup> initializeGroup, Action<FallbackCandidate<TGroup>> afterInitialize = null)
        {
            if (isInit)
                return;

            isInit = true;
            InitializeNextCandidate("init", initializeGroup, afterInitialize);
        }

        protected void InitializeNextCandidate(string source, Action<TGroup> initializeGroup, Action<FallbackCandidate<TGroup>> afterInitialize = null)
        {
            for (int i = highestInitializedIndex + 1; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate.WasInitialized || candidate.Group == null)
                    continue;

                candidate.WasInitialized = true;
                highestInitializedIndex = i;
                candidate.Group.SetTrackingChannel(TrackingChannelOrDefault);

                NetFlowDebugSystem.Log(Layer.adcore, DebugModule, $"{debugPrefix}.InitNext",
                    () => $"source={source} idx={i} label={candidate.Label} mediation={candidate.Mediation} id={candidate.Group.Id}");

                initializeGroup?.Invoke(candidate.Group);
                afterInitialize?.Invoke(candidate);
                return;
            }

            NetFlowDebugSystem.Log(Layer.adcore, DebugModule, $"{debugPrefix}.InitNext",
                () => $"source={source} idx=end no_more_candidates");
        }

        protected int FindCandidateIndex(AdInfo info)
        {
            if (info.adtype != adtype)
                return -1;

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (!string.Equals(candidate.Group?.Id, info.id, StringComparison.Ordinal))
                    continue;

                if (!string.Equals(candidate.Mediation, info.mediation, StringComparison.Ordinal))
                    continue;

                return i;
            }

            return -1;
        }

        protected FallbackCandidate<TGroup> GetPrimaryCandidate()
            => candidates.Count > 0 ? candidates[0] : null;

        protected FallbackCandidate<TGroup> GetBestInitializedCandidate()
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i].WasInitialized)
                    return candidates[i];
            }

            return null;
        }

        protected FallbackCandidate<TGroup> GetFirstCandidateMatching(Func<TGroup, bool> predicate, string errorAction)
        {
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                try
                {
                    if (candidate.Group != null && predicate(candidate.Group))
                        return candidate;
                }
                catch (Exception ex)
                {
                    NetFlowDebugSystem.Warn(Layer.adcore, DebugModule, $"{debugPrefix}.{errorAction}",
                        () => $"idx={i} label={candidate.Label} err={ex.Message}");
                }
            }

            return null;
        }

        protected string BuildDebugInfo(string title, Func<TGroup, string> statusFactory)
        {
            var sb = new StringBuilder(768);
            sb.Append("=== ").Append(title).AppendLine(" ===");
            sb.Append("isInit: ").AppendLine(isInit.ToString());
            sb.Append("highestInitializedIndex: ").AppendLine(highestInitializedIndex.ToString());
            sb.Append("useBackup: ").AppendLine(useBackup.ToString());
            sb.Append("trackingChannel: ").AppendLine(trackingChannel?.ToString() ?? "(null)");

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                string status = "";
                try { status = statusFactory?.Invoke(candidate.Group) ?? ""; } catch { }

                sb.Append(i == 0 ? "* " : "- ");
                sb.Append(candidate.Label)
                    .Append(" | mediation=").Append(candidate.Mediation)
                    .Append(" | id=").Append(candidate.Group?.Id ?? "")
                    .Append(" | init=").Append(candidate.WasInitialized);

                if (!string.IsNullOrEmpty(status))
                    sb.Append(" | ").Append(status);

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
