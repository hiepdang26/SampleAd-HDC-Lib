using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using System;
using System.Collections;
using UnityEngine;

namespace BG_Library.NET.AdSystem
{
    public class AppLaunchSystem : MonoBehaviour
    {
        [Header("Runtime Flags")]
        public bool IgnoreAd;

        private AdCoreBase core;
        private AdSystemConfigs.AppLaunchChannelConfig configs;

        public void Setup(AdCoreBase _core, AdSystemConfigs.AppLaunchChannelConfig _configs)
        {
            core = _core;
            configs = _configs;

            NetFlowDebugSystem.Log(Layer.sys, Module.format_al, "Setup"
                , () => $"core={(core != null)} enable={(configs != null && configs.IsEnabled)}");
        }

        #region (1) ===== LOGIC =====

        private float launchTime;
        private bool isLaunchClockStarted;
        private bool isInitCalled;
        private bool isTimedOut;
        private bool enterTracked;
        private bool enterFailTracked;
        private bool waitingAdCloseCallback;
        private float adCloseFallbackStartTime;
        private bool timeMinReachedLogged;

        private Action onMediationCompletedHandler;

        private bool isBeforeShowCalled;
        private bool isCompleteCalled;
        private const float AdCloseCallbackFallbackSeconds = 15f;

        // Launch clock follows gameplay timeScale so timeMin/timeout pause when timeScale = 0.
        private static float GetLaunchClockTime() => Time.time;
        // Keep close callback fallback independent from gameplay timeScale to avoid hanging forever.
        private static float GetCloseFallbackClockTime() => Time.unscaledTime;

        private void StartLaunchClockIfNeeded(string reason)
        {
            if (isLaunchClockStarted)
                return;

            isLaunchClockStarted = true;
            launchTime = GetLaunchClockTime();

            NetFlowDebugSystem.Log(Layer.sys, Module.format_al, "ALClock Start",
                () => $"reason={reason} start={launchTime:0.###} timeScale={Time.timeScale:0.###}");
        }

        private float GetLaunchElapsed()
        {
            if (!isLaunchClockStarted)
                return 0f;

            return GetLaunchClockTime() - launchTime;
        }

        private void TryLogTimeMinReached()
        {
            if (!isLaunchClockStarted || timeMinReachedLogged)
                return;

            float timeMin = GetTimeMinSeconds();
            if (timeMin > 0f && GetLaunchElapsed() < timeMin)
                return;

            timeMinReachedLogged = true;
            NetFlowDebugSystem.Log(Layer.sys, Module.format_al, "ALClock MinReached",
                () => $"elapsed={GetLaunchElapsed():0.###} min={timeMin:0.###} timeScale={Time.timeScale:0.###}");
        }

        private float GetTimeMinSeconds()
        {
            // Rule update: TimeMin default = 5 when = 0
            if (configs == null) return 5f;

            float timeMin = configs.MinWaitSeconds;
            if (timeMin <= 0f) return 5f;

            return timeMin;
        }

        private float GetEffectiveTimeoutSeconds()
        {
            float timeMin = GetTimeMinSeconds();

            if (configs == null)
                return timeMin + 5f;

            float timeout = configs.TimeoutSeconds;

            if (timeout <= 0f) return timeMin + 5f;
            if (timeout < timeMin) return timeMin + 5f;

            return timeout;
        }

        private void TryCallBeforeShowOnce(string reason)
        {
            if (isBeforeShowCalled) return;

            if (!IsTimeMinReached()) return;

            isBeforeShowCalled = true;

            NetFlowDebugSystem.Log(Layer.sys, Module.format_al, "OnAdLaunchBeforeShow",
                () => $"invoke once, reason={reason}");

            OnAdLaunchBeforeShow?.Invoke();
        }

        private void TryCallCompleteOnce(string reason)
        {
            if (isCompleteCalled) return;

            if (!IsTimeMinReached()) return;

            TryCallBeforeShowOnce($"complete_guard:{reason}");

            waitingAdCloseCallback = false;
            isCompleteCalled = true;

            NetFlowDebugSystem.Log(Layer.sys, Module.format_al, "OnAdLaunchComplete",
                () => $"invoke once, reason={reason}");

            OnAdLaunchComplete?.Invoke();
        }

        private void TrackAutoShowEntry()
        {
            if (enterTracked)
                return;

            enterTracked = true;
            NetTrackingSystem.AutoShowSystemEntry(Channel.AppLaunch, NetTrackingSystem.PosTargetHint("app_launch"));
        }

        private void TrackAutoShowFail(TrackingReason reason)
        {
            if (enterFailTracked)
                return;

            TrackAutoShowEntry();
            enterFailTracked = true;
            NetTrackingSystem.AutoShowSystemFail(Channel.AppLaunch, reason, NetTrackingSystem.PosTargetHint("app_launch"));
        }

        private void StartAdCloseCallbackFallback()
        {
            waitingAdCloseCallback = true;
            adCloseFallbackStartTime = GetCloseFallbackClockTime();
        }

        private AutoShowAttemptResult TryStartAutoShow()
        {
            using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_al,
                "AutoShow Commit", () => "place=app_launch"))
            {
                if (!isTimedOut && IsTimeoutReached())
                {
                    isTimedOut = true;

                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_al,
                        "AutoShow Blocked", () => "timeout reached before call core");
                    TrackAutoShowFail(TrackingReason.Timeout);
                    TryCallCompleteOnce("timeout_before_call_core");
                    return AutoShowAttemptResult.Completed;
                }

                if (IsDisable || IgnoreAd)
                {
                    var reason = IsDisable ? DisableTrackingReason : TrackingReason.Ignored;

                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_al,
                        "AutoShow Blocked",
                        () => $"IsDisable={IsDisable} IgnoreAd={IgnoreAd}");
                    TrackAutoShowFail(reason);
                    TryCallCompleteOnce("show_blocked_disable_or_ignore");
                    return AutoShowAttemptResult.Completed;
                }

                if (!core.AL_GetReady())
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_al,
                        "AutoShow Recheck", () => "not ready -> continue waiting");
                    return AutoShowAttemptResult.ContinueWaiting;
                }

                NetFlowDebugSystem.Log(Layer.sys, Module.format_al,
                    "AutoShow CallCore", () => "AL_ShowAd");

                TryCallBeforeShowOnce("before_show_core");

                bool result = core.AL_ShowAd("app_launch",
                    null,
                    () => TryCallCompleteOnce("ad_closed_callback"));

                if (!result)
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_al,
                        "AutoShow ReturnFalse", () => "AL_ShowAd returned false");

                    TryCallCompleteOnce("al_showad_return_false");
                    return AutoShowAttemptResult.Completed;
                }

                StartAdCloseCallbackFallback();

                NetFlowDebugSystem.Log(Layer.sys, Module.format_al,
                    "AutoShow Return", () => $"result={result}");

                return AutoShowAttemptResult.Started;
            }
        }

        private void Awake()
        {
			onMediationCompletedHandler = () =>
			{
				using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_al, "Init Auto", () => "OnAdCoreInitCompleted"))
				{
                    if (!ShouldAutoInit)
                    {
                        NetFlowDebugSystem.Warn(Layer.sys, Module.format_al, "Init Auto Skip", () => "AutoInit=false");
                        return;
                    }
                    isInitCalled = true;

                    NetTrackingSystem.RequestSystemEntry(Channel.AppLaunch);
                    if (IsDisable)
                    {
                        NetFlowDebugSystem.Warn(Layer.sys, Module.format_al, "Init Auto Blocked", () => $"IsDisable=true");
                        NetTrackingSystem.RequestSystemFail(Channel.AppLaunch, DisableTrackingReason);

                        return;
                    }

                    StartLaunchClockIfNeeded("adcore_init_completed");

                    NetFlowDebugSystem.Log(Layer.sys, Module.format_al, "Init Auto Done", () => "AL_Initialize");
                    core.AL_Initialize();
                }
            };

            NetEventSystem.OnAdCoreInitCompleted += onMediationCompletedHandler;

            NetFlowDebugSystem.Log(Layer.sys, Module.format_al, "Awake",
                () => $"clockStarted={isLaunchClockStarted} timeScale={Time.timeScale:0.###}");
        }

        private void OnDestroy()
        {
            if (onMediationCompletedHandler != null)
                NetEventSystem.OnAdCoreInitCompleted -= onMediationCompletedHandler;

            waitingAdCloseCallback = false;
        }

        private void Update()
        {
            if (!waitingAdCloseCallback || isCompleteCalled)
                return;

            if (GetCloseFallbackClockTime() - adCloseFallbackStartTime < AdCloseCallbackFallbackSeconds)
                return;

            NetFlowDebugSystem.Warn(Layer.sys, Module.format_al,
                "Close Callback Timeout", () => $"fallback after {AdCloseCallbackFallbackSeconds:0.###}s");

            TryCallCompleteOnce("ad_close_callback_timeout_fallback");
        }

        private IEnumerator Start()
        {
            while (true)
            {
                TrackAutoShowEntry();
                TryLogTimeMinReached();

                if (!isTimedOut && IsTimeoutReached())
                {
                    isTimedOut = true;

                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_al,
                        "ALClock Timeout", () => $"elapsed={GetLaunchElapsed():0.###} timeout={GetEffectiveTimeoutSeconds():0.###} timeScale={Time.timeScale:0.###}");
                }

                if (!IsTimeMinReached())
                {
                    yield return null;
                    continue;
                }

                if (isTimedOut)
                {
                    TrackAutoShowFail(TrackingReason.Timeout);
                    TryCallCompleteOnce("timeout_fallback");
                    yield break;
                }

                if (IsDisable || IgnoreAd)
                {
                    TrackAutoShowFail(IsDisable ? DisableTrackingReason : TrackingReason.Ignored);

                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_al,
                        "AutoShow Blocked",
                        () => $"IsDisable={IsDisable} IgnoreAd={IgnoreAd}");

                    TryCallCompleteOnce("deterministic_no_show_disable_or_ignore");
                    yield break;
                }

                if (core.AL_GetReady())
                {
                    NetFlowDebugSystem.Log(Layer.sys, Module.format_al,
                        "AutoShow Ready", () => "call core.AL_ShowAd()");

                    var attempt = TryStartAutoShow();
                    if (attempt == AutoShowAttemptResult.ContinueWaiting)
                    {
                        yield return null;
                        continue;
                    }

                    yield break;
                }

                yield return null;
            }
        }

        private bool IsTimeoutReached()
        {
            if (!isLaunchClockStarted)
                return false;

            float timeout = GetEffectiveTimeoutSeconds();

            if (timeout <= 0f) return false;

            float elapsed = GetLaunchElapsed();
            return elapsed >= timeout;
        }

        private bool IsTimeMinReached()
        {
            if (!isLaunchClockStarted)
                return false;

            float timeMin = GetTimeMinSeconds();

            if (timeMin <= 0f) return true;

            float elapsed = GetLaunchElapsed();
            return elapsed >= timeMin;
        }

        #endregion

        #region (2) ===== PUBLIC API =====

        public event Action OnAdLaunchComplete;
        public event Action OnAdLaunchBeforeShow;

        public void InitManually()
        {
			using (NetFlowDebugSystem.Flow(Layer.sys, Module.format_al, "InitManually", () => "call"))
			{
                if (ShouldAutoInit)
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_al, "InitManually Rejected", () => "AutoInit=true");
                    return;
                }

                isInitCalled = true;
                StartLaunchClockIfNeeded("manual_init");
                NetTrackingSystem.RequestSystemEntry(Channel.AppLaunch);
                if (IsDisable)
                {
                    NetFlowDebugSystem.Warn(Layer.sys, Module.format_al, "Init Auto Blocked", () => $"IsDisable={IsDisable}");
                    NetTrackingSystem.RequestSystemFail(Channel.AppLaunch, DisableTrackingReason);

                    return;
                }

                NetFlowDebugSystem.Log(Layer.sys, Module.format_al, "InitManually CallCore", () => "AL_Initialize");
                core.AL_Initialize();
            }
        }

        public bool AbleToShow
        {
            get
            {
                if (IsDisable) return false;
                if (IgnoreAd) return false;

                if (isTimedOut || IsTimeoutReached()) return false;

                if (!IsTimeMinReached()) return false;

                if (!core.AL_GetReady()) return false;

                return true;
            }
        }

        #endregion

        #region (3) ===== HELPERS =====

        private bool IsDisable => AdsLogic.IsRemovedAd || configs == null || !configs.IsEnabled;
        private bool ShouldAutoInit => configs != null && configs.AutoInit;
        private TrackingReason DisableTrackingReason => AdsLogic.IsRemovedAd ? TrackingReason.IapRemoved : TrackingReason.ConfigDisabled;
        private enum AutoShowAttemptResult
        {
            ContinueWaiting,
            Started,
            Completed
        }

        public string GetDebugInfo()
        {
            var sb = new System.Text.StringBuilder(900);

            sb.AppendLine("=== AppLaunchLogic (AL) Overview ===");

            sb.AppendLine();
            sb.AppendLine("-- Configs --");
            if (configs == null)
            {
                sb.AppendLine("(null)");
            }
            else
            {
                sb.Append("isEnabled: ").AppendLine(configs.IsEnabled.ToString());
                sb.Append("timeoutSeconds: ").AppendLine(configs.TimeoutSeconds.ToString("0.###"));
                sb.Append("minWaitSeconds: ").AppendLine(configs.MinWaitSeconds.ToString("0.###"));
            }

            sb.AppendLine();
            sb.AppendLine("-- Runtime --");
            sb.Append("IgnoreAd: ").AppendLine(IgnoreAd.ToString());
            sb.Append("clockStarted: ").AppendLine(isLaunchClockStarted.ToString());
            sb.Append("launchTime: ").AppendLine(launchTime.ToString("0.###"));
            sb.Append("elapsed: ").AppendLine(GetLaunchElapsed().ToString("0.###"));
            sb.Append("timeScale: ").AppendLine(Time.timeScale.ToString("0.###"));

            sb.Append("isInitCalled: ").AppendLine(isInitCalled.ToString());
            sb.Append("isTimedOut: ").AppendLine(isTimedOut.ToString());

            sb.AppendLine();
            sb.AppendLine("-- Gates --");
            sb.Append("IsDisable: ").AppendLine(IsDisable.ToString());

            bool timeoutReached = false;
            try { timeoutReached = IsTimeoutReached(); } catch { }
            sb.Append("IsTimeoutReached(): ").AppendLine(timeoutReached.ToString());

            bool timeMinReached = true;
            try { timeMinReached = IsTimeMinReached(); } catch { }
            sb.Append("IsTimeMinReached(): ").AppendLine(timeMinReached.ToString());

            bool ready = false;
            try { ready = (core != null && core.AL_GetReady()); } catch { }
            sb.Append("AL_GetReady(): ").AppendLine(ready.ToString());

            bool able = false;
            try { able = AbleToShow; } catch { }
            sb.Append("AbleToShow: ").AppendLine(able.ToString());

            sb.AppendLine();
            sb.AppendLine("-- Notes --");
            sb.AppendLine("• Group debug: call GetDebugGroup()");

            return sb.ToString();
        }

        public string GetDebugGroup()
        {
            if (core == null)
            {
                return "AdCore = null (debug disabled)";
            }

            return core.AL_GetDebugInfo();
        }

        #endregion
    }
}
