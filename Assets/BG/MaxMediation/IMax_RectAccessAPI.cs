using BG_Library.NET.Debug;
using System;
using UnityEngine;

namespace BG_Library.NET.Mediation.Max
{
    public interface IMax_RectAccessAPI
    {
        void ShowAd(string id);
        void HideAd(string id);
        void RequestAd(string id);
        void DestroyAd(string id);
        void UpdatePosition(string id, float x, float y);
        void UpdatePosition(string id, MaxSdkBase.AdViewPosition pos);
        Vector2 GetSize(string id);

        void SubLoaded(Action<string, MaxSdkBase.AdInfo> h);
        void SubLoadFailed(Action<string, MaxSdkBase.ErrorInfo> h);
        void SubClicked(Action<string, MaxSdkBase.AdInfo> h);
        void SubRevenuePaid(Action<string, MaxSdkBase.AdInfo> h);
    }

    public sealed class Max_BNAccessAPI : IMax_RectAccessAPI
    {
        public void RequestAd(string id, MaxRectPlacement placement)
        {
            var position = placement switch
            {
                MaxRectPlacement.FullBottom => MaxSdk.AdViewPosition.BottomCenter,
                MaxRectPlacement.FullTop => MaxSdk.AdViewPosition.TopCenter,
                MaxRectPlacement.TopLeft => MaxSdk.AdViewPosition.TopLeft,
                MaxRectPlacement.TopRight => MaxSdk.AdViewPosition.TopRight,
                MaxRectPlacement.BottomLeft => MaxSdk.AdViewPosition.BottomLeft,
                MaxRectPlacement.BottomRight => MaxSdk.AdViewPosition.BottomRight,
                _ => MaxSdk.AdViewPosition.BottomCenter
            };

            NetFlowDebugSystem.Log(Layer.group, Module.max_api_bn, "Call",
                () => $"MaxSdk.CreateBanner id={id} placement={placement} pos={position}");

            var cfg = new MaxSdk.AdViewConfiguration(position);
            MaxSdk.CreateBanner(id, cfg);

            NetFlowDebugSystem.Log(Layer.group, Module.max_api_bn, "Call",
                () => $"MaxSdk.HideBanner id={id} (hide immediately after create)");
            MaxSdk.HideBanner(id);

            NetFlowDebugSystem.Log(Layer.group, Module.max_api_bn, "Call",
                () => $"MaxSdk.SetBannerBackgroundColor id={id}");

            MaxSdk.SetBannerBackgroundColor(id, Color.white);
        }

        public void ShowAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_bn, "Call",
                () => $"MaxSdk.ShowBanner id={id}");
            MaxSdk.ShowBanner(id);
        }

        public void HideAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_bn, "Call",
                () => $"MaxSdk.HideBanner id={id}");
            MaxSdk.HideBanner(id);
        }

        public void RequestAd(string id)
        {
            RequestAd(id, MaxRectPlacement.FullBottom);
        }

        public void DestroyAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_bn, "Call",
                () => $"MaxSdk.DestroyBanner id={id}");
            MaxSdk.DestroyBanner(id);
        }

        public void UpdatePosition(string id, float x, float y)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_bn, "Call",
                () => $"MaxSdk.UpdateBannerPosition id={id} x={x:0.##} y={y:0.##}");
            MaxSdk.UpdateBannerPosition(id, x, y);
        }

        public void UpdatePosition(string id, MaxSdkBase.AdViewPosition pos)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_bn, "Call",
                () => $"MaxSdk.UpdateBannerPosition id={id} preset={pos}");
            MaxSdk.UpdateBannerPosition(id, pos);
        }

        public Vector2 GetSize(string id)
        {
            return MaxSdk.GetBannerLayout(id).size;
        }

        public void SubLoaded(Action<string, MaxSdkBase.AdInfo> h)
            => MaxSdkCallbacks.Banner.OnAdLoadedEvent += h;

        public void SubLoadFailed(Action<string, MaxSdkBase.ErrorInfo> h)
            => MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += h;

        public void SubClicked(Action<string, MaxSdkBase.AdInfo> h)
            => MaxSdkCallbacks.Banner.OnAdClickedEvent += h;

        public void SubRevenuePaid(Action<string, MaxSdkBase.AdInfo> h)
            => MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += h;
    }

    public sealed class Max_MrecAccessAPI : IMax_RectAccessAPI
    {
        public void ShowAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_mrec, "Call",
                () => $"MaxSdk.ShowMRec id={id}");
            MaxSdk.ShowMRec(id);
        }

        public void HideAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_mrec, "Call",
                () => $"MaxSdk.HideMRec id={id}");
            MaxSdk.HideMRec(id);
        }

        public void RequestAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_mrec, "Call",
                () => $"MaxSdk.CreateMRec id={id}");

            var cfg = new MaxSdk.AdViewConfiguration(MaxSdk.AdViewPosition.BottomCenter);
            MaxSdk.CreateMRec(id, cfg);

            NetFlowDebugSystem.Log(Layer.group, Module.max_api_mrec, "Call",
                () => $"MaxSdk.HideMRec id={id} (hide immediately after create)");
            MaxSdk.HideMRec(id);
        }

        public void DestroyAd(string id)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_mrec, "Call",
                () => $"MaxSdk.DestroyMRec id={id}");
            MaxSdk.DestroyMRec(id);
        }

        public void UpdatePosition(string id, float x, float y)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_mrec, "Call",
                () => $"MaxSdk.UpdateMRecPosition id={id} x={x:0.##} y={y:0.##}");
            MaxSdk.UpdateMRecPosition(id, x, y);
        }

        public void UpdatePosition(string id, MaxSdkBase.AdViewPosition pos)
        {
            NetFlowDebugSystem.Log(Layer.group, Module.max_api_mrec, "Call",
                () => $"MaxSdk.UpdateMRecPosition id={id} preset={pos}");
            MaxSdk.UpdateMRecPosition(id, pos);
        }

        public Vector2 GetSize(string id)
        {
            float density = MaxSdkUtils.GetScreenDensity();
            return new Vector2(300 * density, 250 * density);
        }

        public void SubLoaded(Action<string, MaxSdkBase.AdInfo> h)
            => MaxSdkCallbacks.MRec.OnAdLoadedEvent += h;

        public void SubLoadFailed(Action<string, MaxSdkBase.ErrorInfo> h)
            => MaxSdkCallbacks.MRec.OnAdLoadFailedEvent += h;

        public void SubClicked(Action<string, MaxSdkBase.AdInfo> h)
            => MaxSdkCallbacks.MRec.OnAdClickedEvent += h;

        public void SubRevenuePaid(Action<string, MaxSdkBase.AdInfo> h)
            => MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += h;
    }
}
