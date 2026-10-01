using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;
using System;
using UnityEngine;

namespace BG_Library.NET.Mediation
{
    [Serializable]
    public sealed class StopLogic<T>
        where T : InfoBase
    {
        private readonly FS_GroupControllerBase<T> core;

        public bool IsStopped { get; private set; }
        public int RemainingShows { get; private set; }

        public StopLogic(FS_GroupControllerBase<T> core)
        {
            this.core = core;
            IsStopped = false;

            RemainingShows = this.core.Budget <= 0 ?
                int.MaxValue : this.core.Budget;
        }

        public bool ShouldIgnore()
        {
            return !core.IsInit || IsStopped || RemainingShows <= 0;
        }

        /// <summary>
        /// Stops the flow (idempotent).
        /// - onBestEffortCleanup: destroy/clear if available (caller decides).
        /// - logWarning: optional logger
        /// </summary>
        public void Stop()
        {
            if (!core.IsInit)
            {
                NetFlowDebugSystem.Warn(Layer.group, Module.fs_group, "FS_StopLogic", () => "Stop failed. Init not yet");
                return;
            }
            if (IsStopped)
            {
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, "FS_StopLogic", () => "Stop failed. Stop already");
                return;
            }

            IsStopped = true;

            try { core.DoBestEffortCleanup(); } catch { }
            NetFlowDebugSystem.Log(Layer.group, Module.fs_group, "FS_StopLogic", () => "Stop");
        }

        /// <summary>
        /// Tiêu thụ lượt show ad, nếu hết sẽ trả về false và tự động Stop luồng
        /// </summary>
        /// <returns></returns>
        public bool ConsumeOneShowBudget()
        {
            if (this.core.Budget <= 0) return true; // unlimited
            RemainingShows = Mathf.Max(0, RemainingShows - 1);

            if (RemainingShows <= 0)
            {
                NetFlowDebugSystem.Log(Layer.group, Module.fs_group, "FS_StopLogic", () => "RemainingShows = 0. Auto call stop");
                Stop();

                return false;
            }

            return true;
        }

        public string GetDebugInfo()
        {
            bool shouldIgnore = false;
            try { shouldIgnore = ShouldIgnore(); } catch { }
            return $"StopLogic | IsStopped={IsStopped} | RemainingShows={RemainingShows} | ShouldIgnore={shouldIgnore}";
        }
    }
}