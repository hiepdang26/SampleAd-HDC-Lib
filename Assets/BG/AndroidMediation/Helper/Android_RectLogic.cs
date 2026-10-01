using BG_Library.Common;
using BG_Library.NET.AndroidSDK;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.Mediation.Android
{
    public class Android_RectLogic<T>
        where T : Android_RectBaseInfo
    {
        private readonly Android_RectGroupController<T> core;
        private RectAdInstance ad;

        private readonly string idKey;

        private bool isAttached;
        private bool hasFirstLoadResolved;
        private bool popupLayoutUpdated;
        private float popupLayoutWidthDp;
        private float popupLayoutHeightDp;

        public bool HasAdInstance => ad != null;

        public Android_RectLogic(Android_RectGroupController<T> core)
        {
            this.core = core;
            idKey = this.core.Id;
        }

        // =========================
        // SDK CALLS
        // =========================

        public void CreateAndLoad()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_rect, $"SDK.CreateAndLoad {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey} hasAd={(ad != null)}"))
            {
                if (string.IsNullOrEmpty(idKey))
                {
                    core.Message(() => "CreateAndLoad fail. idKey empty", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "id_empty=true");
                    return;
                }

                core.Message(() => "CreateAndLoad");

                CreateAdInstanceIfNeeded();
                ListenToAdEvents();

                if (ShouldBlockPostInitPopupReload())
                {
                    NetFlowDebugSystem.Log(Layer.group, Module.android_api_rect, $"SDK.RequestBlocked {core.GroupName}",
                        () => $"adtype={core.Adtype} id={idKey} reason=DisablePostInitReload");
                    return;
                }

                ad?.LoadAd();
            }
        }

        public void ShowAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_rect, $"SDK.Show {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey} hasAd={(ad != null)}"))
            {
                if (ad == null)
                {
                    core.Message(() => "ShowAd fail. RectAdInstance null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "ad_null=true");
                    return;
                }

                ad?.ShowAd();
            }
        }

        public void HideAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_rect, $"SDK.Hide {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey} hasAd={(ad != null)}"))
            {
                if (ad == null)
                {
                    core.Message(() => "HideAd fail. RectAdInstance null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "ad_null=true");
                    return;
                }

                ad?.HideAd();
            }
        }

        public bool ExpandAd(bool enableClick = true)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_rect, $"SDK.Expand {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey} hasAd={(ad != null)}"))
            {
                if (ad == null)
                {
                    core.Message(() => "ExpandAd fail. RectAdInstance null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "ad_null=true");
                    return false;
                }

                return ad.ExpandAd(enableClick);
            }
        }

        public void DestroyAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_rect, $"SDK.Destroy {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey} hasAd={(ad != null)}"))
            {
                if (ad == null)
                {
                    core.Message(() => "DestroyAd skip. RectAdInstance null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "ad_null=true");
                    return;
                }

                ad?.DestroyAd();
                ad = null;
                isAttached = false;
                hasFirstLoadResolved = false;
            }
        }

        public void UpdatePUPosition(float xDp, float yDp, float w, float h)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_rect, $"SDK.UpdatePUPos {core.GroupName}",
                () => $"adtype={core.Adtype} x={xDp:0.##} y={yDp:0.##} w={w:0.##} h={h:0.##}"))
            {
                popupLayoutUpdated = true;
                popupLayoutWidthDp = w;
                popupLayoutHeightDp = h;
                ad?.PU_UpdatePos(xDp, yDp, w, h);
            }
        }

        public bool TryValidatePopupLayoutBeforeShow(out TrackingReason reason, out string detail)
        {
            reason = default;
            detail = string.Empty;

            if (core.Info is not Android_PUInfo)
                return true;

            if (!popupLayoutUpdated)
            {
                reason = TrackingReason.UpdatePositionRequired;
                detail = "Popup layout has not been updated yet.";
                return false;
            }

            if (popupLayoutWidthDp <= 0f || popupLayoutHeightDp <= 0f)
            {
                reason = TrackingReason.InvalidLayoutSize;
                detail = $"Popup layout size invalid. w={popupLayoutWidthDp:0.###}, h={popupLayoutHeightDp:0.###}";
                return false;
            }

            return true;
        }

        // =========================
        // Debug helpers
        // =========================

        public string GetIdKeySafe() => string.IsNullOrEmpty(idKey) ? "(empty)" : idKey;

        public string GetAdInstanceStateShort() => ad == null ? "null" : ad.GetType().Name;

        public string GetAttachStateShort()
        {
            if (string.IsNullOrEmpty(idKey)) return "idEmpty";
            if (ad == null) return "adNull";
            return isAttached ? "attached" : "notAttached";
        }

        // =========================
        // Attach callbacks (ASYNC)
        // =========================

        private void ListenToAdEvents()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"Attach {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey}"))
            {
                if (isAttached)
                {
                    core.Message(() => "ListenToAdEvents skip. Already attached", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "already_attached=true");
                    return;
                }

                if (string.IsNullOrEmpty(idKey))
                {
                    core.Message(() => "ListenToAdEvents fail. idKey empty", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "id_empty=true");
                    return;
                }

                if (ad == null)
                {
                    core.Message(() => "ListenToAdEvents fail. RectAdInstance null", LogLevel.Error);
                    NetFlowDebugSystem.Error(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "ad_null=true");
                    return;
                }

                isAttached = true;

                // ===== LOADED =====
                ad.OnAdLoadedEvent += adInfo =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_rect, $"Evt.Loaded {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            var infoStr = "";
                            var source = "";

                            try { if (adInfo != null) infoStr = adInfo.GetInfo(); } catch { }
                            try { if (adInfo != null) source = adInfo.mediationAdapter; } catch { }

                            hasFirstLoadResolved = true;
                            core.OnAdLoadedEvent(infoStr, source);
                        }
                    });
                };

                // ===== LOAD FAILED =====
                ad.OnAdLoadFailedEvent += (adunit, errorCode, errorMessage) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_rect, $"Evt.LoadFailed {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            var shouldDestroyAfterFirstFail = IsPopupFirstLoadFailDestroyCase();
                            hasFirstLoadResolved = true;
                            core.OnAdLoadFailedEvent(errorCode, errorMessage);

                            if (shouldDestroyAfterFirstFail)
                            {
                                NetFlowDebugSystem.Log(Layer.group, Module.android_api_rect, $"Popup.DestroyAfterFirstFail {core.GroupName}",
                                    () => $"adtype={core.Adtype} id={idKey} reason=DisablePostInitReload");
                                DestroyAd();
                            }
                        }
                    });
                };

                // ===== CLICK CLOSE BUTTON =====
                ad.OnAdClickCloseButtonEvent += adInfo =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_rect, $"Evt.ClickClose {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            core.Hide();
                        }
                    });
                };

                // ===== CLICK =====
                ad.OnAdClicked += adInfo =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_rect, $"Evt.Clicked {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            var source = "";
                            try { if (adInfo != null) source = adInfo.mediationAdapter; } catch { }

                            core.OnAdClickedEvent(source);
                        }
                    });
                };

                // ===== REVENUE =====
                ad.OnPaidAdImpressionEvent += (adInfo, adValue) =>
                {
                    UnityMainThreadDispatcher.EnqueueCallback(() =>
                    {
                        using (NetFlowDebugSystem.FlowNew(Layer.group, Module.android_api_rect, $"Evt.Paid {core.GroupName}",
                            () => $"adtype={core.Adtype} id={idKey}"))
                        {
                            var rev = 0d;
                            try
                            {
                                if (adValue != null)
                                    rev = adValue.revenueMicros / 1000000d;
                            }
                            catch { }

                            var currency = "USD";
                            try
                            {
                                if (adValue != null && !string.IsNullOrEmpty(adValue.currencyCode))
                                    currency = adValue.currencyCode;
                            }
                            catch { }

                            var source = "";
                            try { if (adInfo != null) source = adInfo.mediationAdapter; } catch { }

                            core.OnAdRevenuePaidEvent(rev, currency, source);
                        }
                    });
                };

                core.Message(() => $"ListenToAdEvents success. id={idKey}");
                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Result {core.GroupName}", () => "attach_success=true");
            }
        }

        private void CreateAdInstanceIfNeeded()
        {
            if (ad != null)
                return;

            using (NetFlowDebugSystem.Flow(Layer.group, Module.android_api_rect, $"CreateAdInstance {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey}"))
            {
                ad = core.Info.CreateAdInstance();
                isAttached = false;
                hasFirstLoadResolved = false;

                if (ad == null)
                {
                    NetFlowDebugSystem.Error(Layer.group, Module.rect_group, $"Result {core.GroupName}",
                        () => "RectAdInstance null");
                    return;
                }

                NetFlowDebugSystem.Log(Layer.group, Module.rect_group, $"Result {core.GroupName}",
                    () => $"instance={ad.GetType().Name}");
            }
        }

        private bool IsPopupFirstLoadFailDestroyCase()
        {
            return !hasFirstLoadResolved &&
                   core.Info is Android_PUInfo puInfo &&
                   puInfo.DisablePostInitReload;
        }

        private bool ShouldBlockPostInitPopupReload()
        {
            return hasFirstLoadResolved &&
                   core.Info is Android_PUInfo puInfo &&
                   puInfo.DisablePostInitReload;
        }
    }
}
