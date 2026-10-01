using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2IconTools
    {
        private static readonly Dictionary<BuildTarget, Texture2D> CachedPrimaryIcons = new Dictionary<BuildTarget, Texture2D>();

        public static void ClearCaches()
        {
            CachedPrimaryIcons.Clear();
        }

        public static Texture2D GetPrimaryAppIcon(BuildTarget target)
        {
            if (CachedPrimaryIcons.TryGetValue(target, out Texture2D cachedIcon))
                return cachedIcon;

            Texture2D icon = GetFirstNonNullIcon(GetIconsForGroup(BuildTargetGroup.Unknown));
            if (icon != null)
            {
                CachedPrimaryIcons[target] = icon;
                return icon;
            }

            icon = GetFirstNonNullIcon(GetAppIcons(target));
            if (icon != null)
            {
                CachedPrimaryIcons[target] = icon;
                return icon;
            }

            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (!IsSupportedIconTargetGroup(targetGroup))
                    continue;

                icon = GetFirstNonNullIcon(GetIconsForGroup(targetGroup));
                if (icon != null)
                {
                    CachedPrimaryIcons[target] = icon;
                    return icon;
                }
            }

            CachedPrimaryIcons[target] = null;
            return null;
        }

        public static bool TrySetDefaultAppIcon(Texture2D icon, out string errorMessage)
        {
            errorMessage = null;
            if (icon == null)
            {
                errorMessage = "Please choose an icon asset first.";
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(icon);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                errorMessage = "The selected icon must be an asset inside the project.";
                return false;
            }

            SetPrimaryIconsForTargetGroup(BuildTargetGroup.Unknown, icon);
            AssetDatabase.SaveAssets();
            ClearCaches();
            return true;
        }

        public static bool IsAdaptiveIconApplied(Texture2D sourceIcon, out string statusMessage)
        {
            statusMessage = "No source icon selected for comparison.";
            if (sourceIcon == null)
                return false;

            string sourceAssetPath = AssetDatabase.GetAssetPath(sourceIcon)?.Replace("\\", "/") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(sourceAssetPath))
            {
                statusMessage = "The current source icon is not a valid project asset.";
                return false;
            }

            string sourceDirectory = Path.GetDirectoryName(sourceAssetPath)?.Replace("\\", "/");
            string sourceName = Path.GetFileNameWithoutExtension(sourceAssetPath);
            if (string.IsNullOrWhiteSpace(sourceDirectory) || string.IsNullOrWhiteSpace(sourceName))
            {
                statusMessage = "Could not read the source icon path for adaptive icon validation.";
                return false;
            }

            string expectedInsidePath = $"{sourceDirectory}/{sourceName}_inside.png";
            string expectedForegroundPath = $"{sourceDirectory}/{sourceName}_foreground.png";
            Texture2D currentPrimaryIcon = GetPrimaryAppIcon(BuildTarget.Android);
            string currentPrimaryIconPath = AssetDatabase.GetAssetPath(currentPrimaryIcon)?.Replace("\\", "/") ?? string.Empty;
            int legacyMatched = CountMatchingAndroidPrimaryIcons(AndroidPlatformIconKind.Legacy, sourceAssetPath, out int legacyTotal);
            int roundMatched = CountMatchingAndroidPrimaryIcons(AndroidPlatformIconKind.Round, sourceAssetPath, out int roundTotal);

            bool adaptiveMatched = false;
            bool adaptiveReady = false;
            if (TryGetCurrentAdaptiveTextures(out Texture2D currentInsideTexture, out Texture2D currentForegroundTexture))
            {
                string currentInsidePath = AssetDatabase.GetAssetPath(currentInsideTexture)?.Replace("\\", "/") ?? string.Empty;
                string currentForegroundPath = AssetDatabase.GetAssetPath(currentForegroundTexture)?.Replace("\\", "/") ?? string.Empty;
                adaptiveReady = true;
                adaptiveMatched = string.Equals(currentInsidePath, expectedInsidePath, System.StringComparison.OrdinalIgnoreCase)
                    && string.Equals(currentForegroundPath, expectedForegroundPath, System.StringComparison.OrdinalIgnoreCase);
            }

            bool primaryMatches = string.Equals(currentPrimaryIconPath, sourceAssetPath, System.StringComparison.OrdinalIgnoreCase);
            bool legacyReady = legacyTotal == 0 || legacyMatched == legacyTotal;
            bool roundReady = roundTotal == 0 || roundMatched == roundTotal;
            bool matched = primaryMatches && legacyReady && roundReady && adaptiveReady && adaptiveMatched;

            var statusBuilder = new StringBuilder();
            if (matched)
            {
                statusBuilder.Append("Player Settings icon slots already match this default icon. ");
                statusBuilder.Append("Adaptive, Legacy, and Round icons are ready.");
            }
            else
            {
                if (!primaryMatches)
                    statusBuilder.Append("Default icon preview does not match the current Player Settings source. ");

                if (legacyTotal > 0)
                    statusBuilder.Append($"Legacy icons: {legacyMatched}/{legacyTotal}. ");

                if (roundTotal > 0)
                    statusBuilder.Append($"Round icons: {roundMatched}/{roundTotal}. ");

                if (!adaptiveReady)
                    statusBuilder.Append("Adaptive icons are missing. ");
                else if (!adaptiveMatched)
                    statusBuilder.Append("Adaptive icons do not match the current default icon. ");

                statusBuilder.Append("Generate & Apply Adaptive to sync Android icon slots.");
            }

            statusMessage = statusBuilder.ToString().Trim();
            return matched;
        }

        public static Texture2D CreateAdaptiveInsetPreviewTexture(Texture2D sourceIcon, float safeAreaScale = 0.75f)
        {
            return CreateScaledCenteredTexture(sourceIcon, safeAreaScale, Color.clear);
        }

        public static Texture2D CreateAdaptiveAppliedPreviewTexture(Texture2D sourceIcon, float safeAreaScale = 0.75f)
        {
            if (TryGetCurrentAdaptiveTextures(out Texture2D currentInsideTexture, out Texture2D currentForegroundTexture))
                return CreateCombinedTexture(currentInsideTexture, currentForegroundTexture, new Color(0.16f, 0.16f, 0.16f, 1f));

            return CreateScaledCenteredTexture(sourceIcon, safeAreaScale, new Color(0.16f, 0.16f, 0.16f, 1f));
        }

        public static bool TryGenerateAndApplyAdaptiveIcon(Texture2D sourceIcon, float safeAreaScale, out string errorMessage)
        {
            errorMessage = null;
            if (sourceIcon == null)
            {
                errorMessage = "Please choose a source icon first.";
                return false;
            }

            string sourceAssetPath = AssetDatabase.GetAssetPath(sourceIcon);
            if (string.IsNullOrWhiteSpace(sourceAssetPath))
            {
                errorMessage = "The selected icon must be an asset inside the project.";
                return false;
            }

            string sourceDirectory = Path.GetDirectoryName(sourceAssetPath)?.Replace("\\", "/");
            string sourceName = Path.GetFileNameWithoutExtension(sourceAssetPath);
            if (string.IsNullOrWhiteSpace(sourceDirectory) || string.IsNullOrWhiteSpace(sourceName))
            {
                errorMessage = "Could not resolve the source icon path.";
                return false;
            }

            string insideAssetPath = $"{sourceDirectory}/{sourceName}_inside.png";
            string foregroundAssetPath = $"{sourceDirectory}/{sourceName}_foreground.png";

            Texture2D insidePreview = CreateAdaptiveInsetPreviewTexture(sourceIcon, safeAreaScale);
            Texture2D foregroundPreview = CreateTransparentTexture(sourceIcon.width, sourceIcon.height);

            try
            {
                SaveTextureAsPng(insidePreview, insideAssetPath);
                SaveTextureAsPng(foregroundPreview, foregroundAssetPath);

                AssetDatabase.ImportAsset(insideAssetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset(foregroundAssetPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();

                Texture2D insideAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(insideAssetPath);
                Texture2D foregroundAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(foregroundAssetPath);
                if (insideAsset == null || foregroundAsset == null)
                {
                    errorMessage = "Generated adaptive icon assets could not be reloaded from the project.";
                    return false;
                }

                ApplyIconsToPlayerSettings(sourceIcon, insideAsset, foregroundAsset);
                AssetDatabase.SaveAssets();
                BuildToolV2Utilities.ClearEditorCaches();
                return true;
            }
            finally
            {
                if (insidePreview != null)
                    Object.DestroyImmediate(insidePreview);
                if (foregroundPreview != null)
                    Object.DestroyImmediate(foregroundPreview);
            }
        }

        private static Texture2D[] GetAppIcons(BuildTarget target)
        {
            if (target == BuildTarget.Android)
            {
                Texture2D[] platformIcons = GetAndroidPrimaryIcons(AndroidPlatformIconKind.Legacy);
                if (GetFirstNonNullIcon(platformIcons) != null)
                    return platformIcons;

                platformIcons = GetAndroidPrimaryIcons(AndroidPlatformIconKind.Round);
                if (GetFirstNonNullIcon(platformIcons) != null)
                    return platformIcons;
            }

            BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            Texture2D[] targetGroupIcons = GetIconsForGroup(targetGroup);
            if (GetFirstNonNullIcon(targetGroupIcons) != null)
                return targetGroupIcons;

            if (target == BuildTarget.Android)
            {
                Texture2D[] roundIcons = GetAndroidPrimaryIcons(AndroidPlatformIconKind.Round);
                if (GetFirstNonNullIcon(roundIcons) != null)
                    return roundIcons;
            }

            return targetGroupIcons;
        }

        private static void ApplyIconsToPlayerSettings(Texture2D primaryIcon, Texture2D adaptiveInsideIcon, Texture2D adaptiveForegroundIcon)
        {
            SetPrimaryIconsForTargetGroup(BuildTargetGroup.Unknown, primaryIcon);
            SetPrimaryIconsForTargetGroup(BuildPipeline.GetBuildTargetGroup(BuildTarget.Android), primaryIcon);
            SetAndroidPrimaryPlatformIcons(AndroidPlatformIconKind.Legacy, primaryIcon);
            SetAndroidPrimaryPlatformIcons(AndroidPlatformIconKind.Round, primaryIcon);

            NamedBuildTarget namedBuildTarget = NamedBuildTarget.Android;
            PlatformIconKind platformIconKind = AndroidPlatformIconKind.Adaptive;
            PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(namedBuildTarget, platformIconKind);
            if (icons == null || icons.Length == 0)
                return;

            Texture2D[] adaptiveTextures = { adaptiveInsideIcon, adaptiveForegroundIcon };
            for (int i = 0; i < icons.Length; i++)
                icons[i].SetTextures(adaptiveTextures);

            PlayerSettings.SetPlatformIcons(namedBuildTarget, platformIconKind, icons);
        }

        private static Texture2D CreateTransparentTexture(int width, int height)
        {
            return CreateSolidTexture(width, height, Color.clear);
        }

        private static Texture2D CreateSolidTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = "GeneratedIconPreview",
            };

            Color[] colors = Enumerable.Repeat(color, width * height).ToArray();
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateScaledCenteredTexture(Texture2D sourceIcon, float safeAreaScale, Color backgroundColor)
        {
            if (sourceIcon == null)
                return null;

            int width = Mathf.Max(1, sourceIcon.width);
            int height = Mathf.Max(1, sourceIcon.height);
            float clampedScale = Mathf.Clamp(safeAreaScale, 0.1f, 1f);

            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            GL.Clear(true, true, backgroundColor);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, width, height, 0);

            float drawWidth = width * clampedScale;
            float drawHeight = height * clampedScale;
            Rect drawRect = new Rect(
                (width - drawWidth) * 0.5f,
                (height - drawHeight) * 0.5f,
                drawWidth,
                drawHeight);
            Graphics.DrawTexture(drawRect, sourceIcon);

            GL.PopMatrix();

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = "AdaptiveIconPreview",
            };
            texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            texture.Apply();

            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);
            return texture;
        }

        private static void SaveTextureAsPng(Texture2D texture, string assetPath)
        {
            string fullPath = Path.GetFullPath(assetPath);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        }

        private static Texture2D CreateCombinedTexture(Texture2D baseTexture, Texture2D overlayTexture, Color backgroundColor)
        {
            Texture2D referenceTexture = baseTexture != null ? baseTexture : overlayTexture;
            if (referenceTexture == null)
                return null;

            int width = Mathf.Max(1, referenceTexture.width);
            int height = Mathf.Max(1, referenceTexture.height);

            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = renderTexture;

            GL.Clear(true, true, backgroundColor);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, width, height, 0);

            Rect drawRect = new Rect(0f, 0f, width, height);
            if (baseTexture != null)
                Graphics.DrawTexture(drawRect, baseTexture);
            if (overlayTexture != null)
                Graphics.DrawTexture(drawRect, overlayTexture);

            GL.PopMatrix();

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = "AppliedAdaptiveIconPreview",
            };
            texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            texture.Apply();

            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);
            return texture;
        }

        private static bool TryGetCurrentAdaptiveTextures(out Texture2D insideTexture, out Texture2D foregroundTexture)
        {
            insideTexture = null;
            foregroundTexture = null;

            PlatformIcon[] icons = GetAndroidPlatformIcons(AndroidPlatformIconKind.Adaptive);
            if (icons == null || icons.Length == 0)
                return false;

            for (int i = 0; i < icons.Length; i++)
            {
                Texture2D[] textures = icons[i].GetTextures();
                if (textures == null || textures.Length < 2)
                    continue;

                if (textures[0] == null || textures[1] == null)
                    continue;

                insideTexture = textures[0];
                foregroundTexture = textures[1];
                return true;
            }

            return false;
        }

        private static Texture2D[] GetAndroidPrimaryIcons(PlatformIconKind platformIconKind)
        {
            PlatformIcon[] icons = GetAndroidPlatformIcons(platformIconKind);
            if (icons == null || icons.Length == 0)
                return System.Array.Empty<Texture2D>();

            var textures = new Texture2D[icons.Length];
            for (int i = 0; i < icons.Length; i++)
                textures[i] = GetFirstNonNullTexture(icons[i]);
            return textures;
        }

        private static PlatformIcon[] GetAndroidPlatformIcons(PlatformIconKind platformIconKind)
        {
            try
            {
                return PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, platformIconKind);
            }
            catch (System.ArgumentException)
            {
                return System.Array.Empty<PlatformIcon>();
            }
        }

        private static Texture2D GetFirstNonNullTexture(PlatformIcon icon)
        {
            if (icon == null)
                return null;

            Texture2D[] textures = icon.GetTextures();
            if (textures == null || textures.Length == 0)
                return null;

            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] != null)
                    return textures[i];
            }

            return null;
        }

        private static int CountMatchingAndroidPrimaryIcons(PlatformIconKind platformIconKind, string expectedAssetPath, out int total)
        {
            total = 0;
            int matched = 0;
            PlatformIcon[] icons = GetAndroidPlatformIcons(platformIconKind);
            if (icons == null || icons.Length == 0)
                return 0;

            total = icons.Length;
            for (int i = 0; i < icons.Length; i++)
            {
                Texture2D texture = GetFirstNonNullTexture(icons[i]);
                if (texture == null)
                    continue;

                string assetPath = AssetDatabase.GetAssetPath(texture)?.Replace("\\", "/") ?? string.Empty;
                if (string.Equals(assetPath, expectedAssetPath, System.StringComparison.OrdinalIgnoreCase))
                    matched++;
            }

            return matched;
        }

        private static void SetAndroidPrimaryPlatformIcons(PlatformIconKind platformIconKind, Texture2D primaryIcon)
        {
            if (primaryIcon == null)
                return;

            PlatformIcon[] icons = GetAndroidPlatformIcons(platformIconKind);
            if (icons == null || icons.Length == 0)
                return;

            Texture2D[] textures = { primaryIcon };
            for (int i = 0; i < icons.Length; i++)
                icons[i].SetTextures(textures);

            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, platformIconKind, icons);
        }

        private static void SetPrimaryIconsForTargetGroup(BuildTargetGroup targetGroup, Texture2D primaryIcon)
        {
            if (primaryIcon == null || !IsSupportedIconTargetGroup(targetGroup))
                return;

#pragma warning disable CS0618
            Texture2D[] currentIcons = GetIconsForGroup(targetGroup);
            int iconCount = currentIcons != null && currentIcons.Length > 0 ? currentIcons.Length : 1;
            var icons = new Texture2D[iconCount];
            for (int i = 0; i < iconCount; i++)
                icons[i] = primaryIcon;
            PlayerSettings.SetIconsForTargetGroup(targetGroup, icons);
#pragma warning restore CS0618
        }

        private static Texture2D[] GetIconsForGroup(BuildTargetGroup targetGroup)
        {
#pragma warning disable CS0618
            try
            {
                return PlayerSettings.GetIconsForTargetGroup(targetGroup);
            }
            catch (System.ArgumentException)
            {
                return System.Array.Empty<Texture2D>();
            }
#pragma warning restore CS0618
        }

        private static bool IsSupportedIconTargetGroup(BuildTargetGroup targetGroup)
        {
            if (targetGroup == BuildTargetGroup.Unknown)
                return true;

            try
            {
                NamedBuildTarget.FromBuildTargetGroup(targetGroup);
                return true;
            }
            catch (System.ArgumentException)
            {
                return false;
            }
        }

        private static Texture2D GetFirstNonNullIcon(Texture2D[] icons)
        {
            if (icons == null || icons.Length == 0)
                return null;

            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != null)
                    return icons[i];
            }

            return null;
        }
    }
}
