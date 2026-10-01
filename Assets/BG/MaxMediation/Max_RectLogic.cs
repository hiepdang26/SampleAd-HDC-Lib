using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Base;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BG_Library.NET.Mediation.Max
{
    public class Max_RectLogic<T, API>
        where T : InfoBase
        where API : IMax_RectAccessAPI, new()
    {
        private static readonly HashSet<string> attachedIds = new();

        private readonly Max_RectGroupController<T, API> core;
        private readonly IMax_RectAccessAPI api;

        private readonly string idKey;

        public Max_RectLogic(Max_RectGroupController<T, API> core, IMax_RectAccessAPI api)
        {
            this.core = core;
            this.api = api;
            idKey = this.core.Id;
        }

        // =========================
        // Debug helpers
        // =========================

        public float GetDensity()
        {
            float d = 1f;
            try { d = MaxSdkUtils.GetScreenDensity(); } catch { }
            return d > 0f ? d : 1f;
        }

        public string GetSafeAreaShort()
        {
            var r = Screen.safeArea;
            return $"x={r.x:0.#} y={r.y:0.#} w={r.width:0.#} h={r.height:0.#}";
        }

        public string GetAttachStateShort()
        {
            if (string.IsNullOrEmpty(idKey)) return "idEmpty";
            return attachedIds.Contains(idKey) ? "attached" : "notAttached";
        }

        public Vector2 GetSizeDpFromWorld(Vector2 sizeWorldPx)
        {
            var density = GetDensity();
            return new Vector2(sizeWorldPx.x / density, sizeWorldPx.y / density);
        }

        // =========================
        // Attach callbacks (ASYNC)
        // =========================

        public void Attach_Once()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Attach {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey}"))
            {
                if (string.IsNullOrEmpty(idKey))
                {
                    core.Message(() => "Attach failed. idKey empty", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "id_empty=true");
                    return;
                }

                if (!attachedIds.Add(idKey))
                {
                    core.Message(() => $"Skip attach. Already attached id={idKey}", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "already_attached=true");
                    return;
                }

                // ===== LOADED =====
                api.SubLoaded((adUnitId, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.Loaded {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            core.OnAdLoadedEvent(
                                adInfo?.ToString() ?? "",
                                adInfo?.NetworkName ?? ""
                            );
                        }
                    });
                });

                // ===== LOAD FAILED =====
                api.SubLoadFailed((adUnitId, errorInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.LoadFailed {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            core.OnAdLoadFailedEvent((int)errorInfo.Code, errorInfo?.ToString() ?? "");
                        }
                    });
                });

                // ===== CLICKED =====
                api.SubClicked((adUnitId, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.Clicked {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            core.OnAdClickedEvent(adInfo?.NetworkName ?? "");
                        }
                    });
                });

                // ===== REVENUE =====
                api.SubRevenuePaid((adUnitId, adInfo) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        if (!IsMine(adUnitId)) return;

                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.rect_group, $"Evt.Paid {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            var rev = 0d;
                            try { if (adInfo != null) rev = adInfo.Revenue; } catch { }

                            core.OnAdRevenuePaidEvent(
                                rev,
                                "USD",
                                adInfo?.NetworkName ?? ""
                            );
                        }
                    });
                });

                core.Message(() => $"Attach callbacks done id={idKey}");
                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Result {core.GroupName}", () => "attach_success=true");
            }
        }

        // =========================
        // Update Position (REAL SDK)
        // =========================

        public void UpdatePos(GameObject targetObj, Camera camera = null)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"UpdatePos(anchor) {core.GroupName}",
                () => $"adtype={core.Adtype} init={core.IsInit} loaded={core.IsLoaded}"))
            {
                if (!core.IsLoaded || !core.IsInit)
                {
                    core.Message(() => "UpdatePos fail. Not init or not ready", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}",
                        () => $"init={core.IsInit} loaded={core.IsLoaded}");
                    return;
                }

                if (targetObj == null)
                {
                    core.Message(() => "UpdatePos fail. Target null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "target_null=true");
                    return;
                }

                var screenPos = camera != null
                    ? (Vector2)camera.WorldToScreenPoint(targetObj.transform.position)
                    : RectTransformUtility.WorldToScreenPoint(null, targetObj.transform.position);

                SetupScreenPos(screenPos);
            }
        }

        public void UpdatePos(int adViewPosition)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"UpdatePos(preset) {core.GroupName}",
                () => $"adtype={core.Adtype} init={core.IsInit} loaded={core.IsLoaded} pos={(MaxSdkBase.AdViewPosition)adViewPosition}"))
            {
                if (!core.IsLoaded || !core.IsInit)
                {
                    core.Message(() => "UpdatePos fail. Not init or not ready", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}",
                        () => $"init={core.IsInit} loaded={core.IsLoaded}");
                    return;
                }

                SetupScreenPos(adViewPosition);
            }
        }

        private void SetupScreenPos(Vector2 screenPos)
        {
            var vt = GetPos(screenPos);

            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {core.GroupName}",
                () => $"MaxSdk.UpdatePosition id={idKey} x={vt.x:0.##} y={vt.y:0.##}");

            api.UpdatePosition(idKey, vt.x, vt.y);
        }

        private void SetupScreenPos(int screenPos)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Call {core.GroupName}",
                () => $"MaxSdk.UpdatePosition id={idKey} preset={(MaxSdkBase.AdViewPosition)screenPos}");

            api.UpdatePosition(idKey, (MaxSdkBase.AdViewPosition)screenPos);
        }

        // =========================
        // SafeArea + clamp (pixel -> dp)
        // =========================

        private Vector2 GetPos(Vector2 screenPos)
        {
            float density = GetDensity();
            Rect safeArea = Screen.safeArea;

            float xPx = screenPos.x - safeArea.x;

            float topInset = Screen.height - safeArea.height - safeArea.y;
            float yPx = (Screen.height - screenPos.y) - topInset;

            xPx = Mathf.Clamp(xPx, 0f, safeArea.width);
            yPx = Mathf.Clamp(yPx, 0f, safeArea.height);

            float xDp = xPx / density - 150f;
            float yDp = yPx / density - 125f;

            float maxXDp = safeArea.width / density - 300f;
            float maxYDp = safeArea.height / density - 250f;

            xDp = Mathf.Clamp(xDp, 0f, Mathf.Max(0f, maxXDp));
            yDp = Mathf.Clamp(yDp, 0f, Mathf.Max(0f, maxYDp));

            return new Vector2(xDp, yDp);
        }

        public bool IsMine(string adUnitId)
            => !string.IsNullOrEmpty(idKey)
               && string.Equals(idKey, adUnitId, StringComparison.Ordinal);

    }
}
