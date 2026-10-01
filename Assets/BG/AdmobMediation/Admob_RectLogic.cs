// =======================
// Admob_RectLogic.cs
// =======================
using BG_Library.Common;
using BG_Library.NET.Debug;
using GoogleMobileAds.Api;
using UnityEngine;

namespace BG_Library.NET.Mediation.Admob
{
    public class Admob_RectLogic<T>
        where T : Admob_RectInfo
    {
        private readonly Admob_RectGroupController<T> core;

        private BannerView adView;
        private readonly string idKey;
        private readonly AdmobRectPlacement placement;

        public bool HasView => adView != null;
        public AdmobRectPlacement Placement => placement;

        public Admob_RectLogic(Admob_RectGroupController<T> core)
        {
            this.core = core;
            idKey = this.core.Id;
            placement = core.Info.Placement;
        }

        public void CreateAndLoad()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"SDK.CreateAndLoad {core.GroupName}",
                () => $"adtype={core.Adtype} id={idKey}"))
            {
                if (adView != null)
                {
                    core.Message(() => "CreateAndLoad skip. BannerView already created", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "view_exists=true");
                    return;
                }

                if (string.IsNullOrEmpty(idKey))
                {
                    core.Message(() => "CreateAndLoad fail. idKey empty", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "id_empty=true");
                    return;
                }

                core.Message(() => "CreateAndLoad");

                var adSize = ResolveAdSize();
                var adPosition = ResolveAdPosition();

                NetFlowDebugSystem.Log(Layer.group, Module.admob_api_bn, $"Call {core.GroupName}",
                    () => $"new BannerView placement={placement} pos={adPosition}");
                adView = new BannerView(idKey, adSize, adPosition);

                // create => default show => Hide ngay
                NetFlowDebugSystem.Log(Layer.group, Module.admob_api_bn, $"Call {core.GroupName}", () => "BannerView.Hide()");
                adView.Hide();

                ListenToBnAdEvents(adView);

                var req = new AdRequest();

                // REAL SDK CALL
                NetFlowDebugSystem.Log(Layer.group, Module.admob_api_bn, $"Call {core.GroupName}", () => "BannerView.LoadAd()");
                adView.LoadAd(req);
            }
        }

        public void ShowAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"SDK.Show {core.GroupName}",
                () => $"adtype={core.Adtype} hasView={(adView != null)}"))
            {
                if (adView == null)
                {
                    core.Message(() => "ShowAd fail. View null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "view_null=true");
                    return;
                }

                // REAL SDK CALL
                NetFlowDebugSystem.Log(Layer.group, Module.admob_api_bn, $"Call {core.GroupName}", () => "BannerView.Show()");
                adView.Show();
            }
        }

        public void HideAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"SDK.Hide {core.GroupName}",
                () => $"adtype={core.Adtype} hasView={(adView != null)}"))
            {
                if (adView == null)
                {
                    core.Message(() => "HideAd fail. View null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "view_null=true");
                    return;
                }

                // REAL SDK CALL
                NetFlowDebugSystem.Log(Layer.group, Module.admob_api_bn, $"Call {core.GroupName}", () => "BannerView.Hide()");
                adView.Hide();
            }
        }

        public void DestroyAd()
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"SDK.Destroy {core.GroupName}",
                () => $"adtype={core.Adtype} hasView={(adView != null)}"))
            {
                if (adView == null)
                {
                    core.Message(() => "DestroyAd skip. View null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "view_null=true");
                    return;
                }

                core.Message(() => "DestroyAd");
                try
                {
                    // REAL SDK CALL
                    NetFlowDebugSystem.Log(Layer.group, Module.admob_api_bn, $"Call {core.GroupName}", () => "BannerView.Destroy()");
                    adView.Destroy();
                }
                catch
                {
                    // tránh throw làm crash debug build
                }
                finally
                {
                    adView = null;
                }
            }
        }

        // ===== Size helpers =====

        public Vector2 GetSizeDp()
        {
            if (adView == null) return Vector2.zero;

            float density = Master.GetScreenDensity();
            if (density <= 0f) density = 1f;

            return new Vector2(adView.GetWidthInPixels() / density, adView.GetHeightInPixels() / density);
        }

        public Vector2 GetSizeWorld()
        {
            if (adView == null) return Vector2.zero;

            float density = Master.GetScreenDensity();
            if (density <= 0f) density = 1f;

            return new Vector2(adView.GetWidthInPixels() * density, adView.GetHeightInPixels() * density);
        }

        // ===== UpdatePos =====

        public void UpdatePos(GameObject targetObj, Camera camera = null)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"SDK.UpdatePos(anchor) {core.GroupName}",
                () => $"adtype={core.Adtype} loaded={core.IsLoaded} hasView={(adView != null)} target={(targetObj != null)} camera={(camera != null)}"))
            {
                if (adView == null || !core.IsLoaded)
                {
                    core.Message(() => "UpdatePos fail. View null or Ad not ready", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "view_null_or_not_loaded=true");
                    return;
                }

                if (targetObj == null)
                {
                    core.Message(() => "UpdatePos fail. Target object null", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "target_null=true");
                    return;
                }

                var screenPos = camera != null
                    ? (Vector2)camera.WorldToScreenPoint(targetObj.transform.position)
                    : RectTransformUtility.WorldToScreenPoint(null, targetObj.transform.position);

                var dp = ConvertScreenPosToDpTopLeft(screenPos);

                /*dp = ClampDpInSafeArea(dp, GetAdSizeDpFallback());*/

                // REAL SDK CALL
                NetFlowDebugSystem.Log(Layer.group, Module.admob_api_bn, $"Call {core.GroupName}",
                    () => $"BannerView.SetPosition(xDp={(int)dp.x}, yDp={(int)dp.y})");
                adView.SetPosition((int)dp.x, (int)dp.y);

                core.Message(() => $"UpdatePos | dp={dp}", LogLevel.Info);
            }
        }

        public void UpdatePos(int adPosition)
        {
            using (NetFlowDebugSystem.Flow(Layer.group, Module.rect_group, $"SDK.UpdatePos(preset) {core.GroupName}",
                () => $"adtype={core.Adtype} loaded={core.IsLoaded} hasView={(adView != null)} pos={(AdPosition)adPosition}"))
            {
                if (adView == null || !core.IsLoaded)
                {
                    core.Message(() => "UpdatePos fail. View null or Ad not ready", LogLevel.Warning);
                    NetFlowDebugSystem.Warn(Layer.group, Module.rect_group, $"Guard {core.GroupName}", () => "view_null_or_not_loaded=true");
                    return;
                }

                // REAL SDK CALL
                NetFlowDebugSystem.Log(Layer.group, Module.admob_api_bn, $"Call {core.GroupName}", () => $"BannerView.SetPosition({(AdPosition)adPosition})");
                adView.SetPosition((AdPosition)adPosition);

                core.Message(() => $"UpdatePos | adPosition={(AdPosition)adPosition}", LogLevel.Info);
            }
        }

        private Vector2 ConvertScreenPosToDpTopLeft(Vector2 screenPos)
        {
            float density = Master.GetScreenDensity();
            if (density <= 0f) density = 1f;

            float xPx = screenPos.x;
            float yPxTopLeft = Screen.height - screenPos.y;

            if (placement == AdmobRectPlacement.Mrec)
            {
                float xDp = xPx / density - 150f;
                float yDp = yPxTopLeft / density - 125f;
                return new Vector2(xDp, yDp);
            }

            return new Vector2(xPx / density, yPxTopLeft / density);
        }

        private Vector2 ClampDpInSafeArea(Vector2 posDpTopLeft, Vector2 adSizeDp)
        {
            float density = Master.GetScreenDensity();
            if (density <= 0f) density = 1f;

            var safe = Screen.safeArea;

            float safeLeftDp = safe.xMin / density;
            float safeRightDp = safe.xMax / density;
            float safeTopDp = (Screen.height - safe.yMin) / density;
            float safeBottomDp = (Screen.height - safe.yMax) / density;

            float minX = safeLeftDp;
            float maxX = safeRightDp - adSizeDp.x;

            float minY = safeBottomDp;
            float maxY = safeTopDp - adSizeDp.y;

            float x = Mathf.Clamp(posDpTopLeft.x, minX, maxX);
            float y = Mathf.Clamp(posDpTopLeft.y, minY, maxY);

            return new Vector2(x, y);
        }

        private Vector2 GetAdSizeDpFallback()
        {
            return placement switch
            {
                AdmobRectPlacement.Mrec => new Vector2(300f, 250f),
                AdmobRectPlacement.TopLeft or
                AdmobRectPlacement.TopRight or
                AdmobRectPlacement.BottomLeft or
                AdmobRectPlacement.BottomRight => new Vector2(320f, 50f),
                _ => Vector2.zero
            };
        }

        private AdSize ResolveAdSize()
        {
            return placement switch
            {
                AdmobRectPlacement.Mrec => AdSize.MediumRectangle,
                AdmobRectPlacement.FullBottom or AdmobRectPlacement.FullTop
                    => AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(GetScreenWidthDp()),
                _ => AdSize.Banner
            };
        }

        private static int GetScreenWidthDp()
        {
            float density = Screen.dpi <= 0f ? 160f : Screen.dpi;
            int widthDp = Mathf.RoundToInt(Screen.width / (density / 160f));
            return widthDp > 0 ? widthDp : 320;
        }

        private AdPosition ResolveAdPosition()
        {
            return placement switch
            {
                AdmobRectPlacement.Mrec => AdPosition.BottomRight,
                AdmobRectPlacement.FullBottom => AdPosition.Bottom,
                AdmobRectPlacement.FullTop => AdPosition.Top,
                AdmobRectPlacement.TopLeft => AdPosition.TopLeft,
                AdmobRectPlacement.TopRight => AdPosition.TopRight,
                AdmobRectPlacement.BottomLeft => AdPosition.BottomLeft,
                AdmobRectPlacement.BottomRight => AdPosition.BottomRight,
                _ => AdPosition.Bottom
            };
        }

        // ===== Debug helpers =====

        public string GetResponseInfoShort()
        {
            if (adView == null) return "(null view)";

            var resp = adView?.GetResponseInfo();
            if (resp == null) return "(null response)";

            string adapter = "";
            string responseId = "";

            try { adapter = resp.GetMediationAdapterClassName(); } catch { }
            try { responseId = resp.GetResponseId(); } catch { }

            adapter = Trunc(adapter, 40);
            responseId = Trunc(responseId, 24);

            return $"adapter={adapter} respId={responseId}";
        }

        private static string Trunc(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (max <= 0) return "";
            return s.Length <= max ? s : s.Substring(0, max);
        }

        // ===== Event wiring =====

        private void ListenToBnAdEvents(BannerView ad)
        {
            ad.OnBannerAdLoaded += () =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    // SDK callback => FlowNew
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.admob_api_bn, $"Evt.Loaded {core.GroupName}",
                        () => $"adtype={core.Adtype} id={idKey}"))
                    {
                        var resp = adView?.GetResponseInfo();
                        var loadedInfo = resp != null ? Trunc(resp.ToString(), 180) : "";
                        var adSource = "";

                        try { adSource = resp != null ? resp.GetMediationAdapterClassName() : ""; } catch { }
                        // adView?.Hide();
                        core.OnAdLoadedEvent(loadedInfo, adSource);
                    }
                });
            };

            ad.OnBannerAdLoadFailed += error =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.admob_api_bn, $"Evt.LoadFailed {core.GroupName}",
                        () => $"adtype={core.Adtype} id={idKey}"))
                    {
                        int code;
                        try { code = error != null ? error.GetCode() : int.MinValue; }
                        catch { code = int.MinValue; } // NextGen GMA: getCode() signature mismatch
                        core.OnAdLoadFailedEvent(code, error != null ? error.ToString() : "AdLoadFailed_ErrorNull");
                    }
                });
            };

            ad.OnAdPaid += adValue =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.admob_api_bn, $"Evt.Paid {core.GroupName}",
                        () => $"adtype={core.Adtype} id={idKey}"))
                    {
                        var resp = adView?.GetResponseInfo();
                        var adSource = "";
                        try { adSource = resp != null ? resp.GetMediationAdapterClassName() : ""; } catch { }

                        var rev = 0d;
                        try
                        {
                            if (adValue != null) rev = adValue.Value / 1000000d;
                        }
                        catch { }

                        var currency = adValue != null ? adValue.CurrencyCode : "USD";
                        core.OnAdRevenuePaidEvent(rev, currency, adSource);
                    }
                });
            };

            ad.OnAdClicked += () =>
            {
                UnityMainThreadDispatcher.EnqueueCallback(() =>
                {
                    using (NetFlowDebugSystem.FlowNew(Layer.group, Module.admob_api_bn, $"Evt.Clicked {core.GroupName}",
                        () => $"adtype={core.Adtype} id={idKey}"))
                    {
                        var resp = adView?.GetResponseInfo();
                        var adSource = "";
                        try { adSource = resp != null ? resp.GetMediationAdapterClassName() : ""; } catch { }

                        core.OnAdClickedEvent(adSource);
                    }
                });
            };
        }
    }
}
