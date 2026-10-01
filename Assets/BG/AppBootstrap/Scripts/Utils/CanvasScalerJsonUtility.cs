using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AppBootstrap.Splash
{
    [Serializable]
    public sealed class CanvasScalerData
    {
        // Keep only Scale With Screen Size fields to minimize payload size.
        [FormerlySerializedAs("referenceResolution")] public Vector2 rr;
        [FormerlySerializedAs("screenMatchMode")] public CanvasScaler.ScreenMatchMode sm;
        [FormerlySerializedAs("matchWidthOrHeight")] public float mw;
        [FormerlySerializedAs("referencePixelsPerUnit")] public float rp;
    }

    public static class CanvasScalerJsonUtility
    {
        public static string ExportToJson(CanvasScaler target, bool prettyPrint = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var data = ExportData(target);
            return JsonUtility.ToJson(data, prettyPrint);
        }

        public static CanvasScalerData ExportData(CanvasScaler target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            EnsureScaleWithScreenSize(target);

            return new CanvasScalerData
            {
                rr = target.referenceResolution,
                sm = target.screenMatchMode,
                mw = target.matchWidthOrHeight,
                rp = target.referencePixelsPerUnit
            };
        }

        public static void ApplyFromJson(CanvasScaler target, string json)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("CanvasScaler json is empty.", nameof(json));
            }

            var data = JsonUtility.FromJson<CanvasScalerData>(json);
            if (data == null)
            {
                throw new InvalidOperationException("Failed to parse CanvasScaler json.");
            }

            ApplyData(target, data);
        }

        public static void ApplyData(CanvasScaler target, CanvasScalerData data)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            target.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            target.referenceResolution = data.rr;
            target.screenMatchMode = data.sm;
            target.matchWidthOrHeight = data.mw;
            target.referencePixelsPerUnit = data.rp;
        }

        private static void EnsureScaleWithScreenSize(CanvasScaler target)
        {
            if (target.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                throw new InvalidOperationException(
                    $"CanvasScaler '{target.name}' must use {CanvasScaler.ScaleMode.ScaleWithScreenSize}.");
            }
        }
    }
}
