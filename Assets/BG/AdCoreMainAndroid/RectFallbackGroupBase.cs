using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BG_Library.Common;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using UnityEngine;

namespace BG_Library.NET.AdCore.MainAndroid
{
    internal abstract class RectFallbackGroupBase : SequentialFallbackGroupBase<IRectGroup>, IRectGroup
    {
        private enum VisibleCommandMode
        {
            None,
            Activate,
            Show,
        }

        protected sealed class CandidateRuntime
        {
            public bool HasEverLoaded;
            public bool IsRebuilding;
            public bool AwaitingRecovery;
            public bool CurrentWatchArmed;
            public int RefreshFailStreak;
            public int RecoveryFailCount;
            public float ShowActiveDurationSeconds;
            public float ActiveShownStartedAt = float.NegativeInfinity;
            public float LastCurrentWatchSignalAt = float.NegativeInfinity;
            public float RecoveryStartedAt = float.NegativeInfinity;
            public float LastRecoveryProgressAt = float.NegativeInfinity;
        }

        private const float RecoveryTimeoutBaseSeconds = 20f;
        private const float RecoveryTimeoutStepSeconds = 5f;
        private const float RecoveryTimeoutMaxSeconds = 40f;
        private const float RecoveryWatchdogTickSeconds = 1f;
        private const float SwapableShownDurationSeconds = 10f;

        private readonly string debugTitle;
        private readonly string ownerTag;
        private readonly string rectAdtype;
        private readonly GroupAdType rectTrackingAdType;
        private readonly CandidateRuntime[] runtimeStates;

        private FallbackCandidate<IRectGroup> lastPresentedCandidate;
        private Coroutine recoveryWatchdogCoroutine;
        private int currentCandidateIndex = -1;
        private bool isViewActive;
        private string lastVisiblePos = "";
        private VisibleCommandMode lastVisibleCommandMode = VisibleCommandMode.None;

        protected RectFallbackGroupBase(
            List<FallbackCandidate<IRectGroup>> candidates,
            bool useBackup,
            string adtype,
            GroupAdType trackingAdType,
            string ownerTag,
            string debugPrefix,
            string debugTitle,
            Channel defaultTrackingChannel,
            Func<Channel?, Module> debugModuleResolver)
            : base(candidates, useBackup, adtype, debugPrefix, defaultTrackingChannel, debugModuleResolver)
        {
            this.ownerTag = ownerTag;
            this.debugTitle = debugTitle;
            rectAdtype = adtype;
            rectTrackingAdType = trackingAdType;
            runtimeStates = new CandidateRuntime[candidates?.Count ?? 0];
            for (int i = 0; i < runtimeStates.Length; i++)
                runtimeStates[i] = new CandidateRuntime();

            EnsureRecoveryWatchdogStarted();
        }

        public string Id
        {
            get
            {
                var best = GetCurrentCandidate() ?? GetBestLoadedCandidate() ?? GetBestInitializedCandidate() ?? GetPrimaryCandidate();
                return best?.Group?.Id ?? "";
            }
        }

        public string Adtype => rectAdtype;
        public GroupAdType TrackingAdType => rectTrackingAdType;

        public string TrackingIdentitySource
        {
            get
            {
                var best = GetCurrentCandidate() ?? GetBestLoadedCandidate() ?? GetBestInitializedCandidate() ?? GetPrimaryCandidate();
                return best?.Group?.TrackingIdentitySource ?? "";
            }
        }

        public bool IsLoaded => GetBestLoadedCandidate() != null;

        public void Initialize()
        {
            EnsureInitialized(group => group.Initialize(), candidate =>
            {
                OnCandidateInitialized(candidate);
                BeginRecoveryWatch(GetCandidateIndex(candidate), "init", rebuilding: true);
            });
        }

        public bool Rebuild()
        {
            int index = currentCandidateIndex >= 0 ? currentCandidateIndex : GetFirstUsableCandidateIndex();
            if (index < 0)
                index = 0;

            return TryRebuildCandidate(index, "wrapper_rebuild");
        }

        public bool ActivateView(string pos = "")
        {
            isViewActive = true;
            lastVisibleCommandMode = VisibleCommandMode.Activate;
            lastVisiblePos = pos ?? "";

            if (TryPresentCurrentCandidate("activate_resume"))
                return true;

            int candidateIndex = ChooseBestInitialCandidateIndex();
            if (candidateIndex >= 0)
                return PresentCandidate(candidateIndex, VisibleCommandMode.Activate, "activate_first");

            EnsureInitialized(group => group.Initialize(), candidate =>
            {
                OnCandidateInitialized(candidate);
                BeginRecoveryWatch(GetCandidateIndex(candidate), "activate_init", rebuilding: true);
            });

            LogRectFallback("AW",
                () => $"pos={lastVisiblePos} current=none candidates={Candidates.Count}");
            return false;
        }

        public void Show(string pos = "")
        {
            int candidateIndex = currentCandidateIndex >= 0 && IsCandidateUsable(currentCandidateIndex)
                ? currentCandidateIndex
                : ChooseBestInitialCandidateIndex();

            if (candidateIndex < 0)
            {
                WarnRectFallback("SB",
                    () => $"reason=no_candidate pos={pos} candidates={Candidates.Count}");
                return;
            }

            lastVisiblePos = pos ?? "";
            lastVisibleCommandMode = VisibleCommandMode.Show;
            isViewActive = true;
            PresentCandidate(candidateIndex, VisibleCommandMode.Show, "show");
        }

        public void Hide()
        {
            isViewActive = false;

            var current = GetCurrentCandidate() ?? lastPresentedCandidate;
            if (current?.Group == null)
            {
                WarnRectFallback("HB",
                    () => $"reason=no_candidate candidates={Candidates.Count}");
                return;
            }

            StopCurrentWatch(currentCandidateIndex, "hide");
            PauseShowActiveDuration(currentCandidateIndex, "hide");

            LogRectFallback("HD",
                () => $"pick={current.Label} mediation={current.Mediation} id={current.Group.Id}");

            current.Group.Hide();
        }

        public bool Expand(bool enableClick = true)
        {
            var current = GetCurrentCandidate() ?? lastPresentedCandidate;
            if (current?.Group == null)
            {
                WarnRectFallback("EB",
                    () => $"reason=no_candidate candidates={Candidates.Count}");
                return false;
            }

            LogRectFallback("EX",
                () => $"pick={current.Label} mediation={current.Mediation} id={current.Group.Id} enableClick={enableClick}");

            return current.Group.Expand(enableClick);
        }

        public virtual Vector2 Mrec_GetSize()
        {
            var best = GetCurrentCandidate() ?? lastPresentedCandidate ?? GetBestLoadedCandidate() ?? GetBestInitializedCandidate() ?? GetPrimaryCandidate();
            return best?.Group?.Mrec_GetSize() ?? Vector2.zero;
        }

        public virtual void Mrec_UpdatePos(int pos)
        {
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (!IsCandidateUsable(i))
                    continue;

                Candidates[i].Group?.Mrec_UpdatePos(pos);
            }
        }

        public virtual void Mrec_UpdatePos(GameObject targetObj, Camera camera = null)
        {
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (!IsCandidateUsable(i))
                    continue;

                Candidates[i].Group?.Mrec_UpdatePos(targetObj, camera);
            }
        }

        public virtual void Pu_UpdatePos(float xDp, float yDp, float w, float h)
        {
            for (int i = 0; i < Candidates.Count; i++)
                Candidates[i].Group?.Pu_UpdatePos(xDp, yDp, w, h);
        }

        public string GetDebugInfo()
        {
            string baseInfo = BuildDebugInfo(debugTitle, group =>
            {
                bool loaded = false;
                try { loaded = group != null && group.IsLoaded; } catch { }
                return $"loaded={loaded}";
            });

            var sb = new StringBuilder(baseInfo);
            sb.Append("isViewActive: ").AppendLine(isViewActive.ToString());
            sb.Append("lastVisibleMode: ").AppendLine(lastVisibleCommandMode.ToString());
            sb.Append("lastVisiblePos: ").AppendLine(lastVisiblePos ?? "");
            sb.Append("currentCandidateIndex: ").AppendLine(currentCandidateIndex.ToString());

            for (int i = 0; i < runtimeStates.Length; i++)
            {
                var state = runtimeStates[i];
                sb.Append("[state ").Append(i).Append("] ")
                    .Append("loadedEver=").Append(state.HasEverLoaded)
                    .Append(" swapable=").Append(IsCandidateSwapable(i))
                    .Append(" shown=").Append(GetShownActiveDuration(i).ToString("0.0"))
                    .Append(" rebuilding=").Append(state.IsRebuilding)
                    .Append(" awaiting=").Append(state.AwaitingRecovery)
                    .Append(" currentWatch=").Append(state.CurrentWatchArmed)
                    .Append(" failStreak=").Append(state.RefreshFailStreak)
                    .Append(" recoveryFail=").Append(state.RecoveryFailCount)
                    .Append(" timeout=").Append(GetRecoveryTimeoutSeconds(state).ToString("0"))
                    .Append(" currentTo=").Append(GetCurrentWatchdogTimeoutSeconds(i).ToString("0"))
                    .Append(" recoveryAge=").Append(GetRecoveryAgeText(state))
                    .AppendLine();
            }

            AppendExtraDebug(sb);
            return sb.ToString();
        }

        protected override void SubscribeCore()
        {
            NetEventSystem.OnRectLoaded += HandleRectLoaded;
            NetEventSystem.OnRectLoadFailed += HandleRectLoadFailed;
            NetEventSystem.OnRectPaid += HandleRectPaid;
        }

        protected override void UnsubscribeCore()
        {
            NetEventSystem.OnRectLoaded -= HandleRectLoaded;
            NetEventSystem.OnRectLoadFailed -= HandleRectLoadFailed;
            NetEventSystem.OnRectPaid -= HandleRectPaid;
        }

        protected override void OnDisposed()
        {
            if (recoveryWatchdogCoroutine == null || AdsLogic.Ins == null)
                return;

            AdsLogic.Ins.StopCoroutine(recoveryWatchdogCoroutine);
            recoveryWatchdogCoroutine = null;
        }

        protected virtual bool EnableCurrentWatchdog => true;
        protected virtual int GetCurrentWeakFailThreshold(int currentIndex) => 1;
        protected virtual float GetCurrentWatchdogTimeoutSeconds(int currentIndex) => 30f;
        protected virtual void OnCandidateInitialized(FallbackCandidate<IRectGroup> candidate) { }
        protected virtual void OnCandidateLoaded(FallbackCandidate<IRectGroup> candidate) { }
        protected virtual void OnCandidatePresenting(FallbackCandidate<IRectGroup> candidate) { }
        protected virtual void AppendExtraDebug(StringBuilder sb) { }

        private void LogRectFallback(string title, Func<string> messageFactory)
            => LogRectFallback(title, -1, messageFactory);

        private void LogRectFallback(string title, int targetIndex, Func<string> messageFactory)
            => NetFlowDebugSystem.Log(Layer.adcore, DebugModule, $"RectFB {title}",
                () => BuildRectFallbackMessage(targetIndex, messageFactory));

        private void WarnRectFallback(string title, Func<string> messageFactory)
            => WarnRectFallback(title, -1, messageFactory);

        private void WarnRectFallback(string title, int targetIndex, Func<string> messageFactory)
            => NetFlowDebugSystem.Warn(Layer.adcore, DebugModule, $"RectFB {title}",
                () => BuildRectFallbackMessage(targetIndex, messageFactory));

        private string BuildRectFallbackMessage(int targetIndex, Func<string> messageFactory)
        {
            string message = messageFactory != null ? messageFactory() : "";
            string current = DescribeCandidate(currentCandidateIndex);
            string target = targetIndex >= 0 ? $" t={DescribeCandidate(targetIndex)}" : "";
            string pos = string.IsNullOrEmpty(lastVisiblePos) ? "" : $" p={lastVisiblePos}";
            return string.IsNullOrEmpty(message)
                ? $"o={ownerTag} c={current}{target} a={(isViewActive ? 1 : 0)}{pos}"
                : $"o={ownerTag} c={current}{target} a={(isViewActive ? 1 : 0)}{pos} {message}";
        }

        private string DescribeCandidate(int index)
        {
            if (index < 0 || index >= Candidates.Count)
                return "none";

            var candidate = Candidates[index];
            if (candidate == null)
                return "none";

            string label = candidate.Label switch
            {
                "Admob" => "A",
                "Max" => "M",
                "Android" => "D",
                _ => candidate.Label
            };
            return $"{label}{index}";
        }

        private void HandleRectLoaded(AdInfo info)
        {
            int index = FindCandidateIndex(info);
            if (index < 0)
                return;

            var state = runtimeStates[index];
            state.HasEverLoaded = true;
            state.RefreshFailStreak = 0;
            state.RecoveryFailCount = 0;
            state.ShowActiveDurationSeconds = 0f;
            state.ActiveShownStartedAt = float.NegativeInfinity;
            EndRecoveryWatch(index, "loaded");
            TouchCurrentWatch(index, "loaded");
            OnCandidateLoaded(Candidates[index]);

            LogRectFallback("Ld", index,
                () => $"idx={index} label={Candidates[index].Label} current={currentCandidateIndex} active={isViewActive} shown={GetShownActiveDuration(index):0.0}");

            if (index == currentCandidateIndex && isViewActive)
                StartShowActiveDuration(index, "loaded");

            if (!isViewActive)
                return;

            if (currentCandidateIndex < 0 || !IsCandidateUsable(currentCandidateIndex))
            {
                int firstCandidateIndex = ChooseBestInitialCandidateIndex();
                if (firstCandidateIndex >= 0)
                    PresentCandidate(firstCandidateIndex, ResolvePresentationMode(), "loaded_first_active");
                return;
            }

            if (index != currentCandidateIndex)
                TrySwapCurrentWithBackup("backup_loaded");
        }

        private void HandleRectLoadFailed(AdInfo info, int errorCode, string error)
        {
            int index = FindCandidateIndex(info);
            if (index < 0)
                return;

            var state = runtimeStates[index];

            if (!state.HasEverLoaded)
            {
                state.RecoveryFailCount++;
                TouchRecoveryProgress(index, "initial_fail");

                WarnRectFallback("IF", index,
                    () => $"idx={index} label={Candidates[index].Label} err={error} rf={state.RecoveryFailCount} to={GetRecoveryTimeoutSeconds(state):0}");

                if (currentCandidateIndex >= 0 && isViewActive && index != currentCandidateIndex)
                {
                    PingBackupCandidateAfter(currentCandidateIndex, index, "backup_initial_fail");
                }
                else if (index == highestInitializedIndex)
                {
                    InitializeNextCandidate("load_fail", group => group.Initialize(), candidate =>
                    {
                        OnCandidateInitialized(candidate);
                        BeginRecoveryWatch(GetCandidateIndex(candidate), "load_fail_init_next", rebuilding: true);
                    });
                }

                return;
            }

            state.RefreshFailStreak++;
            TouchCurrentWatch(index, "refresh_fail");
            if (index != currentCandidateIndex || !isViewActive)
            {
                state.RecoveryFailCount++;
                TouchRecoveryProgress(index, "refresh_fail");
                if (currentCandidateIndex >= 0 && isViewActive && index != currentCandidateIndex)
                    PingBackupCandidateAfter(currentCandidateIndex, index, "backup_refresh_fail");
            }

            WarnRectFallback("RF", index,
                () => $"idx={index} label={Candidates[index].Label} streak={state.RefreshFailStreak} err={error} rf={state.RecoveryFailCount} to={GetRecoveryTimeoutSeconds(state):0}");

            if (index != currentCandidateIndex || !isViewActive)
                return;

            PingBackupCandidate(index, "current_refresh_fail");
            TrySwapCurrentWithBackup("current_refresh_fail");
        }

        private void HandleRectPaid(AdInfo info, AdValueInfo _)
        {
            int index = FindCandidateIndex(info);
            if (index < 0)
                return;

            LogRectFallback("Pd", index,
                () => $"idx={index} label={Candidates[index].Label} current={currentCandidateIndex}");
        }

        private bool TryPresentCurrentCandidate(string source)
        {
            if (currentCandidateIndex < 0 || !IsCandidateUsable(currentCandidateIndex))
                return false;

            return PresentCandidate(currentCandidateIndex, ResolvePresentationMode(), source);
        }

        private bool PresentCandidate(int index, VisibleCommandMode mode, string source)
        {
            if (index < 0 || index >= Candidates.Count)
                return false;

            var candidate = Candidates[index];
            if (candidate?.Group == null)
                return false;

            candidate.Group.SetTrackingChannel(TrackingChannelOrDefault);
            OnCandidatePresenting(candidate);

            bool ok;
            switch (mode)
            {
                case VisibleCommandMode.Show:
                    if (!candidate.Group.IsLoaded)
                    {
                        WarnRectFallback("PB", index,
                            () => $"source={source} idx={index} reason=not_loaded");
                        return false;
                    }

                    LogRectFallback("PR", index,
                        () => $"source={source} mode=show idx={index} label={candidate.Label} mediation={candidate.Mediation} pos={lastVisiblePos}");
                    candidate.Group.Show(lastVisiblePos);
                    ok = true;
                    break;

                default:
                    LogRectFallback("PR", index,
                        () => $"source={source} mode=activate idx={index} label={candidate.Label} mediation={candidate.Mediation} pos={lastVisiblePos}");
                    ok = candidate.Group.ActivateView(lastVisiblePos);
                    break;
            }

            if (!ok)
                return false;

            if (currentCandidateIndex >= 0 && currentCandidateIndex != index)
                PauseShowActiveDuration(currentCandidateIndex, "present_switch");

            currentCandidateIndex = index;
            lastPresentedCandidate = candidate;
            StartShowActiveDuration(index, source);
            TouchCurrentWatch(index, source);
            return true;
        }

        private void PingBackupCandidate(int currentIndex, string source)
        {
            int backupIndex = GetPreferredBackupCandidateIndex(currentIndex);
            if (backupIndex < 0)
            {
                LogRectFallback("PG",
                    () => $"source={source} action=no_backup");
                return;
            }

            var state = runtimeStates[backupIndex];

            if (state.IsRebuilding)
            {
                LogRectFallback("PG", backupIndex,
                    () => $"source={source} idx={backupIndex} action=ignore_rebuilding");
                return;
            }

            if (IsCandidateSwapable(backupIndex))
            {
                LogRectFallback("PG", backupIndex,
                    () => $"source={source} idx={backupIndex} action=notify_ready");
                return;
            }

            if (state.AwaitingRecovery)
            {
                LogRectFallback("PG", backupIndex,
                    () => $"source={source} idx={backupIndex} action=ignore_waiting");
                return;
            }

            TryRebuildCandidate(backupIndex, source);
        }

        private void PingBackupCandidateAfter(int currentIndex, int failedBackupIndex, string source)
        {
            int backupIndex = GetNextBackupCandidateIndex(currentIndex, failedBackupIndex);
            if (backupIndex < 0)
            {
                LogRectFallback("PG",
                    () => $"source={source} after={DescribeCandidate(failedBackupIndex)} action=no_next_backup");
                return;
            }

            var state = runtimeStates[backupIndex];

            if (state.IsRebuilding)
            {
                LogRectFallback("PG", backupIndex,
                    () => $"source={source} idx={backupIndex} action=ignore_rebuilding");
                return;
            }

            if (IsCandidateSwapable(backupIndex))
            {
                LogRectFallback("PG", backupIndex,
                    () => $"source={source} idx={backupIndex} action=notify_ready");
                return;
            }

            if (state.AwaitingRecovery)
            {
                LogRectFallback("PG", backupIndex,
                    () => $"source={source} idx={backupIndex} action=ignore_waiting");
                return;
            }

            TryRebuildCandidate(backupIndex, source);
        }

        private bool TrySwapCurrentWithBackup(string source)
        {
            if (!isViewActive)
                return false;

            if (currentCandidateIndex < 0 || !IsCandidateUsable(currentCandidateIndex))
            {
                int initialCandidateIndex = ChooseBestInitialCandidateIndex();
                return initialCandidateIndex >= 0 && PresentCandidate(initialCandidateIndex, ResolvePresentationMode(), $"{source}_no_current");
            }

            int backupIndex = GetPreferredReplacementCandidateIndex(currentCandidateIndex);
            if (backupIndex < 0 || !ShouldSwapCurrentWith(backupIndex))
                return false;

            int previousIndex = currentCandidateIndex;
            var previousCandidate = Candidates[previousIndex];

            LogRectFallback("SW", backupIndex,
                () => $"source={source} from={previousCandidate.Label} to={Candidates[backupIndex].Label} streak={runtimeStates[previousIndex].RefreshFailStreak}");

            StopCurrentWatch(previousIndex, "swapped_down");
            PauseShowActiveDuration(previousIndex, "swapped_down");
            previousCandidate.Group.Hide();

            if (PresentCandidate(backupIndex, ResolvePresentationMode(), $"{source}_swap"))
                return true;

            currentCandidateIndex = previousIndex;
            PresentCandidate(previousIndex, ResolvePresentationMode(), $"{source}_restore");
            return false;
        }

        private bool TryRebuildCandidate(int index, string source)
        {
            if (index < 0 || index >= Candidates.Count)
                return false;

            var candidate = Candidates[index];
            if (candidate?.Group == null)
                return false;

            var state = runtimeStates[index];
            state.HasEverLoaded = false;
            state.RefreshFailStreak = 0;
            state.ShowActiveDurationSeconds = 0f;
            state.ActiveShownStartedAt = float.NegativeInfinity;

            candidate.WasInitialized = true;
            if (index > highestInitializedIndex)
                highestInitializedIndex = index;

            candidate.Group.SetTrackingChannel(TrackingChannelOrDefault);
            OnCandidateInitialized(candidate);
            BeginRecoveryWatch(index, source, rebuilding: true);

            LogRectFallback("RB", index,
                () => $"source={source} idx={index} label={candidate.Label} mediation={candidate.Mediation}");

            bool ok = candidate.Group.Rebuild();
            if (!ok)
            {
                state.RecoveryFailCount++;
                EndRecoveryWatch(index, "rebuild_failed");
                WarnRectFallback("RBX", index,
                    () => $"source={source} idx={index} label={candidate.Label} rf={state.RecoveryFailCount} to={GetRecoveryTimeoutSeconds(state):0}");
            }

            return ok;
        }

        private bool ShouldSwapCurrentWith(int backupIndex)
        {
            if (!IsCandidateSwapable(backupIndex))
                return false;

            if (currentCandidateIndex < 0 || !IsCandidateUsable(currentCandidateIndex))
                return true;

            if (IsPriorityCandidate(backupIndex))
                return true;

            return runtimeStates[currentCandidateIndex].RefreshFailStreak >= GetCurrentWeakFailThreshold(currentCandidateIndex);
        }

        private int ChooseBestInitialCandidateIndex()
        {
            int swapableIndex = GetFirstSwapableCandidateIndex(excludeIndex: -1);
            if (swapableIndex >= 0)
                return swapableIndex;

            return GetFirstUsableCandidateIndex();
        }

        private int GetFirstSwapableCandidateIndex(int excludeIndex)
        {
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (i == excludeIndex)
                    continue;

                if (IsCandidateSwapable(i))
                    return i;
            }

            return -1;
        }

        private int GetFirstUsableCandidateIndex()
        {
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (IsCandidateUsable(i))
                    return i;
            }

            return -1;
        }

        private int GetPreferredReplacementCandidateIndex(int currentIndex)
            => GetFirstSwapableCandidateIndex(currentIndex);

        private bool IsCandidateUsable(int index)
        {
            if (index < 0 || index >= Candidates.Count)
                return false;

            var candidate = Candidates[index];
            return candidate?.Group != null && candidate.WasInitialized && candidate.Group.IsLoaded;
        }

        private bool IsCandidateSwapable(int index)
        {
            if (!IsCandidateUsable(index))
                return false;

            return GetShownActiveDuration(index) < SwapableShownDurationSeconds;
        }

        private VisibleCommandMode ResolvePresentationMode()
            => lastVisibleCommandMode == VisibleCommandMode.Show ? VisibleCommandMode.Show : VisibleCommandMode.Activate;

        private FallbackCandidate<IRectGroup> GetCurrentCandidate()
        {
            if (currentCandidateIndex < 0 || currentCandidateIndex >= Candidates.Count)
                return null;

            return Candidates[currentCandidateIndex];
        }

        private FallbackCandidate<IRectGroup> GetBestLoadedCandidate()
            => GetFirstCandidateMatching(group => group.IsLoaded, "LoadedCheckError");

        private void EnsureRecoveryWatchdogStarted()
        {
            if (recoveryWatchdogCoroutine != null || AdsLogic.Ins == null)
                return;

            recoveryWatchdogCoroutine = AdsLogic.Ins.StartCoroutine(CoRecoveryWatchdog());
        }

        private IEnumerator CoRecoveryWatchdog()
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(RecoveryWatchdogTickSeconds);

                for (int i = 0; i < runtimeStates.Length; i++)
                {
                    if (EnableCurrentWatchdog &&
                        i == currentCandidateIndex &&
                        isViewActive &&
                        runtimeStates[i].CurrentWatchArmed &&
                        !float.IsNegativeInfinity(runtimeStates[i].LastCurrentWatchSignalAt) &&
                        Time.unscaledTime - runtimeStates[i].LastCurrentWatchSignalAt >= GetCurrentWatchdogTimeoutSeconds(i))
                    {
                        HandleCurrentWatchdogTimeout(i);
                    }

                    if (!runtimeStates[i].AwaitingRecovery)
                        continue;

                    if (Time.unscaledTime - runtimeStates[i].LastRecoveryProgressAt < GetRecoveryTimeoutSeconds(runtimeStates[i]))
                        continue;

                    HandleRecoveryTimeout(i);
                }
            }
        }

        private void HandleRecoveryTimeout(int index)
        {
            if (index < 0 || index >= runtimeStates.Length || !runtimeStates[index].AwaitingRecovery)
                return;

            float timeoutSeconds = GetRecoveryTimeoutSeconds(runtimeStates[index]);
            WarnRectFallback("TO", index,
                () => $"idx={index} label={Candidates[index].Label} current={currentCandidateIndex} active={isViewActive} to={timeoutSeconds:0} rf={runtimeStates[index].RecoveryFailCount} shown={GetShownActiveDuration(index):0.0}");

            if (index == currentCandidateIndex && isViewActive)
            {
                EndRecoveryWatch(index, "timeout_ignore_visible_current");
                return;
            }

            if (IsCandidateSwapable(index))
            {
                EndRecoveryWatch(index, "timeout_ignore_swapable_hidden");
                return;
            }

            TryRebuildCandidate(index, "recovery_timeout");
        }

        private void BeginRecoveryWatch(int index, string source, bool rebuilding)
        {
            if (index < 0 || index >= runtimeStates.Length)
                return;

            EnsureRecoveryWatchdogStarted();

            var state = runtimeStates[index];
            state.AwaitingRecovery = true;
            state.IsRebuilding = rebuilding;
            state.RecoveryStartedAt = Time.unscaledTime;
            state.LastRecoveryProgressAt = Time.unscaledTime;

            LogRectFallback("WS", index,
                () => $"source={source} idx={index} rebuilding={rebuilding} to={GetRecoveryTimeoutSeconds(state):0} rf={state.RecoveryFailCount}");
        }

        private void TouchRecoveryProgress(int index, string source)
        {
            if (index < 0 || index >= runtimeStates.Length)
                return;

            var state = runtimeStates[index];
            if (!state.AwaitingRecovery)
                return;

            state.LastRecoveryProgressAt = Time.unscaledTime;
            LogRectFallback("WT", index,
                () => $"source={source} idx={index} to={GetRecoveryTimeoutSeconds(state):0} rf={state.RecoveryFailCount}");
        }

        private void EndRecoveryWatch(int index, string source)
        {
            if (index < 0 || index >= runtimeStates.Length)
                return;

            var state = runtimeStates[index];
            if (!state.AwaitingRecovery && !state.IsRebuilding)
                return;

            state.AwaitingRecovery = false;
            state.IsRebuilding = false;
            state.RecoveryStartedAt = float.NegativeInfinity;
            state.LastRecoveryProgressAt = float.NegativeInfinity;

            LogRectFallback("WD", index,
                () => $"source={source} idx={index}");
        }

        private void TouchCurrentWatch(int index, string source)
        {
            if (!EnableCurrentWatchdog ||
                index < 0 ||
                index >= runtimeStates.Length ||
                index != currentCandidateIndex ||
                !isViewActive ||
                !IsCandidateUsable(index))
                return;

            var state = runtimeStates[index];
            state.CurrentWatchArmed = true;
            state.LastCurrentWatchSignalAt = Time.unscaledTime;

            LogRectFallback("CW", index,
                () => $"source={source} to={GetCurrentWatchdogTimeoutSeconds(index):0}");
        }

        private void StopCurrentWatch(int index, string source)
        {
            if (index < 0 || index >= runtimeStates.Length)
                return;

            var state = runtimeStates[index];
            if (!state.CurrentWatchArmed && float.IsNegativeInfinity(state.LastCurrentWatchSignalAt))
                return;

            state.CurrentWatchArmed = false;
            state.LastCurrentWatchSignalAt = float.NegativeInfinity;

            LogRectFallback("CX", index,
                () => $"source={source}");
        }

        private void HandleCurrentWatchdogTimeout(int index)
        {
            if (index < 0 ||
                index >= runtimeStates.Length ||
                index != currentCandidateIndex ||
                !isViewActive ||
                !IsCandidateUsable(index))
                return;

            var state = runtimeStates[index];
            state.LastCurrentWatchSignalAt = Time.unscaledTime;
            state.RefreshFailStreak++;

            WarnRectFallback("CT", index,
                () => $"idx={index} label={Candidates[index].Label} streak={state.RefreshFailStreak} to={GetCurrentWatchdogTimeoutSeconds(index):0}");

            PingBackupCandidate(index, "current_watchdog_timeout");
            TrySwapCurrentWithBackup("current_watchdog_timeout");
        }

        private void StartShowActiveDuration(int index, string source)
        {
            if (index < 0 || index >= runtimeStates.Length || !isViewActive || !IsCandidateUsable(index))
                return;

            var state = runtimeStates[index];
            if (!float.IsNegativeInfinity(state.ActiveShownStartedAt))
                return;

            state.ActiveShownStartedAt = Time.unscaledTime;
            LogRectFallback("SD", index,
                () => $"source={source} shown={GetShownActiveDuration(index):0.0}");
        }

        private void PauseShowActiveDuration(int index, string source)
        {
            if (index < 0 || index >= runtimeStates.Length)
                return;

            var state = runtimeStates[index];
            if (float.IsNegativeInfinity(state.ActiveShownStartedAt))
                return;

            state.ShowActiveDurationSeconds += Mathf.Max(0f, Time.unscaledTime - state.ActiveShownStartedAt);
            state.ActiveShownStartedAt = float.NegativeInfinity;

            LogRectFallback("SX", index,
                () => $"source={source} shown={GetShownActiveDuration(index):0.0}");
        }

        private float GetShownActiveDuration(int index)
        {
            if (index < 0 || index >= runtimeStates.Length)
                return 0f;

            var state = runtimeStates[index];
            float shown = Mathf.Max(0f, state.ShowActiveDurationSeconds);
            if (!float.IsNegativeInfinity(state.ActiveShownStartedAt))
                shown += Mathf.Max(0f, Time.unscaledTime - state.ActiveShownStartedAt);

            return shown;
        }

        private int GetCandidateIndex(FallbackCandidate<IRectGroup> candidate)
        {
            if (candidate == null)
                return -1;

            for (int i = 0; i < Candidates.Count; i++)
            {
                if (ReferenceEquals(Candidates[i], candidate))
                    return i;
            }

            return -1;
        }

        private static string GetRecoveryAgeText(CandidateRuntime state)
        {
            if (state == null || !state.AwaitingRecovery || float.IsNegativeInfinity(state.LastRecoveryProgressAt))
                return "-";

            return $"{(Time.unscaledTime - state.LastRecoveryProgressAt):0.0}s";
        }

        private static float GetRecoveryTimeoutSeconds(CandidateRuntime state)
        {
            if (state == null)
                return RecoveryTimeoutBaseSeconds;

            float timeout = RecoveryTimeoutBaseSeconds + (state.RecoveryFailCount * RecoveryTimeoutStepSeconds);
            return Mathf.Min(timeout, RecoveryTimeoutMaxSeconds);
        }

        private bool IsPriorityCandidate(int index) => index == 0;

        private int GetPreferredBackupCandidateIndex(int currentIndex)
        {
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (i != currentIndex && Candidates[i].Group != null)
                    return i;
            }

            return -1;
        }

        private int GetNextBackupCandidateIndex(int currentIndex, int afterIndex)
        {
            bool seenAfter = false;
            for (int i = 0; i < Candidates.Count; i++)
            {
                if (i == currentIndex || Candidates[i].Group == null)
                    continue;

                if (!seenAfter)
                {
                    if (i == afterIndex)
                        seenAfter = true;

                    continue;
                }

                return i;
            }

            return -1;
        }
    }
}
