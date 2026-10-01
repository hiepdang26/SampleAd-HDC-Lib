using System.Collections.Generic;
using System.Text;
using BG_Library.Common;
using BG_Library.NET.Debug;
using BG_Library.NET.Mediation.Admob;
using BG_Library.NET.Mediation.Android;
using BG_Library.NET.Mediation.Max;
using BG_Library.NET.Tracking;
using GoogleMobileAds.Api;
using UnityEngine;

namespace BG_Library.NET.AdCore.MainAndroid
{
    internal sealed class MrecFallbackGroup : RectFallbackGroupBase
    {
        private enum PresetPosition
        {
            TopLeft = 0,
            Top = 1,
            TopRight = 2,
            Center = 3,
            BottomLeft = 4,
            Bottom = 5,
            BottomRight = 6,
        }

        private enum PositionMode
        {
            None,
            AdPosition,
            Anchor,
        }

        private PositionMode positionMode;
        private int lastAdPosition;
        private GameObject lastAnchorTarget;
        private Camera lastAnchorCamera;

        public MrecFallbackGroup(List<FallbackCandidate<IRectGroup>> candidates, bool useBackup)
            : base(candidates, useBackup, BG_ConstValue.adtype_mrec, GroupAdType.Mrec, "MR", "MREC Fallback", "Mrec Fallback", Channel.Mrec, _ => Module.format_mrec)
        {
        }

        protected override void OnCandidateLoaded(FallbackCandidate<IRectGroup> candidate)
        {
            ApplyStoredPosition(candidate?.Group);
        }

        protected override void OnCandidatePresenting(FallbackCandidate<IRectGroup> candidate)
        {
            ApplyStoredPosition(candidate?.Group);
        }

        protected override void AppendExtraDebug(StringBuilder sb)
        {
            sb.Append("positionMode: ").AppendLine(positionMode.ToString());
        }

        public override void Mrec_UpdatePos(int pos)
        {
            positionMode = PositionMode.AdPosition;
            lastAdPosition = pos;
            lastAnchorTarget = null;
            lastAnchorCamera = null;

            for (int i = 0; i < Candidates.Count; i++)
            {
                var candidate = Candidates[i];
                if (candidate?.Group == null || !candidate.WasInitialized || !candidate.Group.IsLoaded)
                    continue;

                ApplyPresetPosition(candidate.Group, pos);
            }
        }

        public override void Mrec_UpdatePos(GameObject targetObj, Camera camera = null)
        {
            positionMode = PositionMode.Anchor;
            lastAnchorTarget = targetObj;
            lastAnchorCamera = camera;
            base.Mrec_UpdatePos(targetObj, camera);
        }

        private void ApplyStoredPosition(IRectGroup group)
        {
            if (group == null)
                return;

            switch (positionMode)
            {
                case PositionMode.AdPosition:
                    ApplyPresetPosition(group, lastAdPosition);
                    break;
                case PositionMode.Anchor:
                    group.Mrec_UpdatePos(lastAnchorTarget, lastAnchorCamera);
                    break;
                default:
                    ApplyPresetPosition(group, (int)PresetPosition.BottomRight);
                    break;
            }
        }

        private static void ApplyPresetPosition(IRectGroup group, int pos)
        {
            var preset = NormalizePresetPosition(pos);

            switch (group)
            {
                case Admob_RectGroupController<Admob_MrecInfo>:
                    group.Mrec_UpdatePos((int)MapToAdmobPosition(preset));
                    break;

                case Max_RectGroupController<Max_MrecInfo, Max_MrecAccessAPI>:
                    group.Mrec_UpdatePos((int)MapToMaxPosition(preset));
                    break;
            }
        }

        private static PresetPosition NormalizePresetPosition(int pos)
        {
            return pos switch
            {
                (int)PresetPosition.TopLeft => PresetPosition.TopLeft,
                (int)PresetPosition.Top => PresetPosition.Top,
                (int)PresetPosition.TopRight => PresetPosition.TopRight,
                (int)PresetPosition.Center => PresetPosition.Center,
                (int)PresetPosition.BottomLeft => PresetPosition.BottomLeft,
                (int)PresetPosition.Bottom => PresetPosition.Bottom,
                (int)PresetPosition.BottomRight => PresetPosition.BottomRight,
                _ => PresetPosition.BottomRight,
            };
        }

        private static AdPosition MapToAdmobPosition(PresetPosition preset)
        {
            return preset switch
            {
                PresetPosition.TopLeft => AdPosition.TopLeft,
                PresetPosition.Top => AdPosition.Top,
                PresetPosition.TopRight => AdPosition.TopRight,
                PresetPosition.Center => AdPosition.Center,
                PresetPosition.BottomLeft => AdPosition.BottomLeft,
                PresetPosition.Bottom => AdPosition.Bottom,
                _ => AdPosition.BottomRight,
            };
        }

        private static MaxSdkBase.AdViewPosition MapToMaxPosition(PresetPosition preset)
        {
            return preset switch
            {
                PresetPosition.TopLeft => MaxSdkBase.AdViewPosition.TopLeft,
                PresetPosition.Top => MaxSdkBase.AdViewPosition.TopCenter,
                PresetPosition.TopRight => MaxSdkBase.AdViewPosition.TopRight,
                PresetPosition.Center => MaxSdkBase.AdViewPosition.Centered,
                PresetPosition.BottomLeft => MaxSdkBase.AdViewPosition.BottomLeft,
                PresetPosition.Bottom => MaxSdkBase.AdViewPosition.BottomCenter,
                _ => MaxSdkBase.AdViewPosition.BottomRight,
            };
        }
    }

    internal sealed class BannerFallbackGroup : RectFallbackGroupBase
    {
        public BannerFallbackGroup(List<FallbackCandidate<IRectGroup>> candidates, bool useBackup, BannerPlacement placement)
            : base(candidates, useBackup, BG_ConstValue.adtype_bn, GroupAdType.Banner, $"BN.{ShortPlacement(placement)}", "BN Fallback", "Banner Fallback", Channel.Banner, _ => Module.format_bn)
        {
        }

        protected override int GetCurrentWeakFailThreshold(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex >= Candidates.Count)
                return 1;

            var candidate = Candidates[currentIndex];
            if (!string.Equals(candidate?.Label, "Android", System.StringComparison.Ordinal))
                return 1;

            if (candidate.Group is Android_RectGroupController<Android_BNInfo> androidGroup)
                return androidGroup.Info.TimeReload >= 20 ? 1 : 2;

            return 1;
        }

        protected override float GetCurrentWatchdogTimeoutSeconds(int currentIndex)
        {
            if (currentIndex < 0 || currentIndex >= Candidates.Count)
                return 30f;

            var candidate = Candidates[currentIndex];
            if (candidate.Group is Android_RectGroupController<Android_BNInfo> androidGroup)
                return Mathf.Max(30f, androidGroup.Info.TimeReload + 5f);

            return 30f;
        }

        private static string ShortPlacement(BannerPlacement placement)
        {
            return placement switch
            {
                BannerPlacement.FullBottom => "FB",
                BannerPlacement.FullTop => "FT",
                BannerPlacement.TopLeft => "TL",
                BannerPlacement.TopRight => "TR",
                BannerPlacement.BottomLeft => "BL",
                BannerPlacement.BottomRight => "BR",
                _ => placement.ToString()
            };
        }
    }
}
