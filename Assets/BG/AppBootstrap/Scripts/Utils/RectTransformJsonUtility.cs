using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace AppBootstrap.Splash
{
    [Serializable]
    public sealed class RectTransformData
    {
        // Keep only the fields required to rebuild the layout at runtime.
        [FormerlySerializedAs("anchorMin")] public Vector2 am;
        [FormerlySerializedAs("anchorMax")] public Vector2 ax;
        [FormerlySerializedAs("anchoredPosition")] public Vector2 ap;
        [FormerlySerializedAs("sizeDelta")] public Vector2 sd;
        [FormerlySerializedAs("pivot")] public Vector2 pv;
    }

    public static class RectTransformJsonUtility
    {
        public static string ExportToJson(RectTransform target, bool prettyPrint = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var data = ExportData(target);
            return JsonUtility.ToJson(data, prettyPrint);
        }

        public static RectTransformData ExportData(RectTransform target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return new RectTransformData
            {
                am = target.anchorMin,
                ax = target.anchorMax,
                ap = target.anchoredPosition,
                sd = target.sizeDelta,
                pv = target.pivot
            };
        }

        public static void ApplyFromJson(RectTransform target, string json)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("RectTransform json is empty.", nameof(json));
            }

            var data = JsonUtility.FromJson<RectTransformData>(json);
            if (data == null)
            {
                throw new InvalidOperationException("Failed to parse RectTransform json.");
            }

            ApplyData(target, data);
        }

        public static void ApplyData(RectTransform target, RectTransformData data)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            target.anchorMin = data.am;
            target.anchorMax = data.ax;
            target.pivot = data.pv;
            target.sizeDelta = data.sd;
            target.anchoredPosition = data.ap;
        }
    }
}
