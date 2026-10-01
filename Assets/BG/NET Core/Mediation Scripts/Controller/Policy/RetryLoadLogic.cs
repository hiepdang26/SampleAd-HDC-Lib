using BG_Library.Common;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;
using BG_Library.NET.Tracking;
using System;
using System.Collections;
using UnityEngine;

namespace BG_Library.NET.Mediation
{
    public sealed class RetryLoadLogic<T>
        where T : InfoBase
    {
        private const int RETRY_CAP = 6;
        private const float RETRY_DELAY_CAP = 64f; // 2^6 = 64s

        private Coroutine retryCo;

        public int Attempt { get; private set; }
        public bool IsWaiting { get; private set; }

        private readonly FS_GroupControllerBase<T> core;

        public RetryLoadLogic(FS_GroupControllerBase<T> core)
        {
            this.core = core;

            Attempt = 0;
            IsWaiting = false;
            retryCo = null;
        }

        public void Stop()
        {
            NetFlowDebugSystem.Log(Layer.group, Module.fs_group, "FS_RetryLoadLogic", ()=> "Reset");
            Attempt = 0;
            Cancel();
        }

        private void Cancel()
        {
            IsWaiting = false;

            var host = AdsLogic.Ins;
            if (host != null && retryCo != null)
            {
                host.StopCoroutine(retryCo);
            }

            retryCo = null;
        }

        public void Schedule()
        {
            if (!core.CanRetryGate()) 
            {
                NetTrackingSystem.RequestGroupFail(core.GetTrackingAdType, core.TrackingIdentitySource, GroupRequestContext.Retry, TrackingReason.GateBlocked, core.CurrentTrackingChannel, retryAttempt: Attempt + 1);
                return;
            }

            IsWaiting = true;

            Attempt++;
            var exp = Math.Min(RETRY_CAP, Attempt);

            var delay = 1f * (1 << exp);
            if (delay > RETRY_DELAY_CAP)
                delay = RETRY_DELAY_CAP;

            NetFlowDebugSystem.Log(Layer.group, Module.fs_group, "FS_RetryLoadLogic", 
                () => $"Schedule | attempt={Attempt} delay={delay:0}s cap={RETRY_DELAY_CAP:0}s");

            Cancel(); // cancel previous co (if any) but keep IsWaiting true
            IsWaiting = true;

            var host = AdsLogic.Ins;
            if (host == null)
            {
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, "FS_RetryLoadLogic",
                    () => $"FS_RetryLoadLogic: cannot start (AdsLogic.Ins null)");
                NetTrackingSystem.RequestGroupFail(core.GetTrackingAdType, core.TrackingIdentitySource, GroupRequestContext.Retry, TrackingReason.HostBlocked, core.CurrentTrackingChannel, retryAttempt: Attempt);

                IsWaiting = false;
                return;
            }

            retryCo = host.StartCoroutine(CO_Retry(delay));
        }

        private IEnumerator CO_Retry(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            retryCo = null;
            IsWaiting = false;

            core.N_Load("retry");
        }

        public string GetDebugInfo()
        {
            return $"RetryLoadLogic | Attempt={Attempt} | IsWaiting={IsWaiting}";
        }
    }
}
