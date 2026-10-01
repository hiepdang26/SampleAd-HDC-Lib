using System.Text;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2ScrcpyArguments
    {
        public static string Build(string serial, BuildToolScrcpySettingsV2 settings)
        {
            var builder = new StringBuilder();
            builder.Append("--serial ").Append(serial).Append(' ');

            ResolvePreset(settings, out int maxFps, out int maxSize, out int bitrateMbps);

            if (true)
                builder.Append("--always-on-top ");
            if (maxSize > 0)
                builder.Append("--max-size ").Append(maxSize).Append(' ');
            if (maxFps > 0)
                builder.Append("--max-fps ").Append(maxFps).Append(' ');
            if (bitrateMbps > 0)
                builder.Append("--video-bit-rate ").Append(bitrateMbps).Append("M ");
            if (settings.turnScreenOff)
                builder.Append("--turn-screen-off ");
            if (settings.stayAwake)
                builder.Append("--stay-awake ");
            builder.Append("--no-audio ");

            return builder.ToString().TrimEnd();
        }

        public static string GetPresetDescription(BuildToolScrcpyPresetV2 preset)
        {
            return preset switch
            {
                BuildToolScrcpyPresetV2.LowLatency => "Low Latency: prioritizes responsiveness. Uses 60 FPS, max size 960, bitrate 6 Mbps. Good for gameplay, input, and ad flow testing.",
                BuildToolScrcpyPresetV2.HighQuality => "High Quality: prioritizes sharper image quality. Uses 60 FPS, max size 1600, bitrate 12 Mbps. Good for UI, icon, layout, and demo capture.",
                _ => "Balanced: balances smoothness and clarity. Uses 45 FPS, max size 1280, bitrate 8 Mbps. This is the default preset for most daily build/test sessions.",
            };
        }

        private static void ResolvePreset(BuildToolScrcpySettingsV2 settings, out int maxFps, out int maxSize, out int bitrateMbps)
        {
            BuildToolScrcpyPresetV2 preset = settings?.preset ?? BuildToolScrcpyPresetV2.Balanced;
            switch (preset)
            {
                case BuildToolScrcpyPresetV2.LowLatency:
                    maxFps = 60;
                    maxSize = 960;
                    bitrateMbps = 6;
                    break;
                case BuildToolScrcpyPresetV2.HighQuality:
                    maxFps = 60;
                    maxSize = 1600;
                    bitrateMbps = 12;
                    break;
                default:
                    maxFps = 45;
                    maxSize = 1280;
                    bitrateMbps = 8;
                    break;
            }
        }
    }
}
