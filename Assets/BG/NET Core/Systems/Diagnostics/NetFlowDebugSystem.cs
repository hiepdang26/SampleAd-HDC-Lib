using BG_Library.NET.AdSystem;
using System;
using BG_Library.Common;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BG_Library.NET.Tracking;

namespace BG_Library.NET.Debug
{
    public enum DebugPreset
    {
        QuickDebug = 0,
        NormalDebug = 1,
        FullDebug = 2,
        Off = 3,
    }

    public enum Layer
    {
        sys,     // => remote configs, adsystem
        adcore,  // Adcore, mediation manager
        group,   // All Group
        tracking
    }

    public enum Module
    {
        // Core
        adcore,
        rc,
        adslogic,

        // Formats
        format_fa,
        format_rw,
        format_al,
        format_ar,
        format_bn,
        format_mrec,
        format_pu,
        format_cl,

        // groups
        rect_group,
        fs_group,

        // med
        med_admob,
        med_max,
        med_android,

        // api_admob
        admob_api_ao,
        admob_api_fa,
        admob_api_rw,
        admob_api_bn,

        // api_max
        max_api_ao,
        max_api_fa,
        max_api_rw,
        max_api_bn,
        max_api_mrec,

        // api_android
        android_api_fs,
        android_api_rect,

        // tracking
        track_system,
        track_adcore,
        track_group,
        track_revenue,

        // med/api ios
        med_ios,
        ios_api_fs,
        ios_api_rect
    }

    public static class NetFlowDebugSystem
    {
        private const int MaxHistoryEntries = 512;
        private const int FirstLogPerFlowKey = int.MinValue;

        internal readonly struct PresetState
        {
            public PresetState(
                bool enabled,
                bool verbose,
                bool onlyFirstLogPerLayerModuleInFlow,
                bool enableLayerSys,
                bool enableLayerAdcore,
                bool enableLayerGroup,
                bool enableLayerTracking,
                bool enableTrackingEventLog,
                bool enableTrackingRevenueLog,
                bool enableTrackingParamDetails,
                bool enableTrackingFirebase,
                bool enableFormatFilter,
                bool enableFormatFA,
                bool enableFormatRW,
                bool enableFormatAL,
                bool enableFormatAR,
                bool enableFormatBN,
                bool enableFormatMrec,
                bool enableFormatPU,
                bool enableFormatCL)
            {
                Enabled = enabled;
                Verbose = verbose;
                OnlyFirstLogPerLayerModuleInFlow = onlyFirstLogPerLayerModuleInFlow;
                EnableLayerSys = enableLayerSys;
                EnableLayerAdcore = enableLayerAdcore;
                EnableLayerGroup = enableLayerGroup;
                EnableLayerTracking = enableLayerTracking;
                EnableTrackingEventLog = enableTrackingEventLog;
                EnableTrackingRevenueLog = enableTrackingRevenueLog;
                EnableTrackingParamDetails = enableTrackingParamDetails;
                EnableTrackingFirebase = enableTrackingFirebase;
                EnableFormatFilter = enableFormatFilter;
                EnableFormatFA = enableFormatFA;
                EnableFormatRW = enableFormatRW;
                EnableFormatAL = enableFormatAL;
                EnableFormatAR = enableFormatAR;
                EnableFormatBN = enableFormatBN;
                EnableFormatMrec = enableFormatMrec;
                EnableFormatPU = enableFormatPU;
                EnableFormatCL = enableFormatCL;
            }

            public bool Enabled { get; }
            public bool Verbose { get; }
            public bool OnlyFirstLogPerLayerModuleInFlow { get; }
            public bool EnableLayerSys { get; }
            public bool EnableLayerAdcore { get; }
            public bool EnableLayerGroup { get; }
            public bool EnableLayerTracking { get; }
            public bool EnableTrackingEventLog { get; }
            public bool EnableTrackingRevenueLog { get; }
            public bool EnableTrackingParamDetails { get; }
            public bool EnableTrackingFirebase { get; }
            public bool EnableFormatFilter { get; }
            public bool EnableFormatFA { get; }
            public bool EnableFormatRW { get; }
            public bool EnableFormatAL { get; }
            public bool EnableFormatAR { get; }
            public bool EnableFormatBN { get; }
            public bool EnableFormatMrec { get; }
            public bool EnableFormatPU { get; }
            public bool EnableFormatCL { get; }
        }

        private readonly struct FlowHistoryEntry
        {
            public FlowHistoryEntry(long sequence, string renderedText, string countKey, Channel? channel)
            {
                Sequence = sequence;
                RenderedText = renderedText ?? "";
                CountKey = countKey ?? "";
                Channel = channel;
            }

            public long Sequence { get; }
            public string RenderedText { get; }
            public string CountKey { get; }
            public Channel? Channel { get; }
        }

        public static DebugPreset CurrentPreset { get; private set; } = DebugPreset.Off;
        private static bool enabled = true;
        private static bool verbose;
        private static bool onlyFirstLogPerLayerModuleInFlow;
        private static bool OnlyFirstLogPerFlow = false;
        private static bool enableLayerSys = true;
        private static bool enableLayerAdcore = true;
        private static bool enableLayerGroup = true;
        private static bool enableLayerTracking = true;
        private static bool enableTrackingEventLog = true;
        private static bool enableTrackingRevenueLog = true;
        private static bool enableTrackingParamDetails;
        private static bool enableTrackingFirebase;
        private static bool enableFormatFilter;
        private static bool enableFormatFA = true;
        private static bool enableFormatRW = true;
        private static bool enableFormatAL = true;
        private static bool enableFormatAR = true;
        private static bool enableFormatBN = true;
        private static bool enableFormatMREC = true;
        private static bool enableFormatPU = true;
        private static bool enableFormatCL = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void InitConfigs()
        {
            ReloadFromConfig();
        }

        public static void ReloadFromConfig()
        {
            var configs = NetConfigsSO.Ins;
            if (configs == null)
                return;

            CurrentPreset = NormalizePreset(configs.Debug_Preset);
            ApplyRuntimeState(GetPresetState(CurrentPreset));
        }

        internal static DebugPreset NormalizePreset(DebugPreset preset)
        {
            return (int)preset switch
            {
                0 => DebugPreset.QuickDebug,
                1 => DebugPreset.NormalDebug,
                2 => DebugPreset.FullDebug,
                3 => DebugPreset.Off,
                4 => DebugPreset.NormalDebug,
                5 => DebugPreset.FullDebug,
                _ => DebugPreset.Off,
            };
        }

        internal static PresetState GetPresetState(DebugPreset preset)
        {
            preset = NormalizePreset(preset);
            return preset switch
            {
                DebugPreset.QuickDebug => new PresetState(
                    true, false, false,
                    true, true, true, true,
                    true, true, false, false,
                    false,
                    true, true, true, true, true, true, true, true),
                DebugPreset.NormalDebug => new PresetState(
                    true, true, false,
                    true, true, false, true,
                    true, true, true, false,
                    false,
                    true, true, true, true, true, true, true, true),
                DebugPreset.FullDebug => new PresetState(
                    true, true, false,
                    true, true, true, true,
                    true, true, true, true,
                    false,
                    true, true, true, true, true, true, true, true),
                _ => new PresetState(
                    false, false, false,
                    false, false, false, false,
                    false, false, false, false,
                    false,
                    true, true, true, true, true, true, true, true),
            };
        }

        public static bool IsEnabledForPreset(DebugPreset preset)
        {
            return GetPresetState(preset).Enabled;
        }

        internal static bool Verbose => verbose;
        internal static bool EnableTrackingParamDetails => enableTrackingParamDetails;

        public static bool ShouldSendTrackingToFirebase(DebugPreset preset)
        {
            PresetState state = GetPresetState(preset);
            if (!state.Enabled)
                return true;

            return state.EnableTrackingFirebase;
        }

        public static string DescribeLayersVi(DebugPreset preset)
        {
            PresetState state = GetPresetState(preset);
            string text = string.Empty;
            AppendLayer(ref text, state.EnableLayerSys, "sys");
            AppendLayer(ref text, state.EnableLayerAdcore, "adcore");
            AppendLayer(ref text, state.EnableLayerGroup, "group");
            AppendLayer(ref text, state.EnableLayerTracking, "tracking");
            return string.IsNullOrEmpty(text) ? "none" : text;
        }

        public static string DescribeTrackingVi(DebugPreset preset)
        {
            PresetState state = GetPresetState(preset);
            if (!state.Enabled)
                return "Tracking log: OFF";

            string trackingText = (!state.EnableTrackingEventLog && !state.EnableTrackingRevenueLog)
                ? "tracking logs off"
                : state.EnableTrackingParamDetails ? "event + revenue + param detail" : "event + revenue";

            return trackingText;
        }

        private static void ApplyRuntimeState(PresetState state)
        {
            enabled = state.Enabled;
            verbose = state.Verbose;
            onlyFirstLogPerLayerModuleInFlow = state.OnlyFirstLogPerLayerModuleInFlow;
            OnlyFirstLogPerFlow = CurrentPreset == DebugPreset.QuickDebug;

            enableLayerSys = state.EnableLayerSys;
            enableLayerAdcore = state.EnableLayerAdcore;
            enableLayerGroup = state.EnableLayerGroup;
            enableLayerTracking = state.EnableLayerTracking;

            enableTrackingEventLog = state.EnableTrackingEventLog;
            enableTrackingRevenueLog = state.EnableTrackingRevenueLog;
            enableTrackingParamDetails = state.EnableTrackingParamDetails;
            enableTrackingFirebase = state.EnableTrackingFirebase;

            enableFormatFilter = state.EnableFormatFilter;
            enableFormatFA = state.EnableFormatFA;
            enableFormatRW = state.EnableFormatRW;
            enableFormatAL = state.EnableFormatAL;
            enableFormatAR = state.EnableFormatAR;
            enableFormatBN = state.EnableFormatBN;
            enableFormatMREC = state.EnableFormatMrec;
            enableFormatPU = state.EnableFormatPU;
            enableFormatCL = state.EnableFormatCL;
        }

        private static void AppendLayer(ref string text, bool layerEnabled, string label)
        {
            if (!layerEnabled)
                return;

            if (text.Length > 0)
                text += " | ";

            text += label;
        }

        public static string DescribePresetVi(DebugPreset preset)
        {
            return preset switch
            {
                DebugPreset.QuickDebug => "Quick Debug: moi flow chi giu 1 debug dau tien, khong in param chi tiet. Hop de xem duong di nhanh ma it nhieu.",
                DebugPreset.NormalDebug => "Normal Debug: bat day du log he thong, adcore va tracking, co param chi tiet nhung bo tang group de de doc hon.",
                DebugPreset.FullDebug => "Full Debug: bat toan bo layer, tracking, param chi tiet va moi phan debug can de soi sau.",
                DebugPreset.Off => "Off: tat het debug cua NET va dong thoi tat ca Debug.unityLogger.logEnabled theo preset nay.",
                _ => "Preset khong xac dinh."
            };
        }

        private static int flowSeed = 0;
        private static long historySequence = 0;

        [ThreadStatic] private static Stack<int> flowStack;
        private static Stack<int> FlowStack => flowStack ??= new Stack<int>(8);

        // Per-flow caches (to support filters/once-per-module)
        private static readonly Dictionary<int, Module> FlowFormat = new Dictionary<int, Module>(64);
        private static readonly Dictionary<int, HashSet<int>> FlowLoggedKeys = new Dictionary<int, HashSet<int>>(64);
        private static readonly List<FlowHistoryEntry> SequentialHistory = new List<FlowHistoryEntry>(MaxHistoryEntries);
        private static readonly Dictionary<string, int> HistoryCountByLine = new Dictionary<string, int>(StringComparer.Ordinal);
        private static string lastHistoryLine = "";

        // =========================================================
        // Public API (ENUM)
        // =========================================================

        public static IDisposable Flow(Layer layer, Module module, string title, Func<string> messageFactory = null)
        {
            if (!enabled) return DummyScope.Instance;

            bool createdNew = false;
            int flowId;

            // JOIN if already inside a flow.
            if (CurrentFlowId == 0)
            {
                flowId = ++flowSeed;
                FlowStack.Push(flowId);
                createdNew = true;

                // Detect format ONLY if flow begins at sys + format_*
                if (layer == Layer.sys && IsFormatModule(module))
                {
                    lock (FlowFormat) FlowFormat[flowId] = module;
                }
            }
            else
            {
                flowId = CurrentFlowId;
            }

            Log(layer, module, title, messageFactory ?? (() => "Start"));

            // Only the scope that CREATED the flow should pop it.
            return createdNew
                ? new FlowScope(popOnDispose: true, flowId)
                : new FlowScope(popOnDispose: false, flowId);
        }

        public static IDisposable FlowNew(Layer layer, Module module, string title, Func<string> messageFactory = null)
        {
            if (!enabled) return DummyScope.Instance;

            int flowId = ++flowSeed;
            FlowStack.Push(flowId);

            // Detect format ONLY if flow begins at sys + format_*
            if (layer == Layer.sys && IsFormatModule(module))
            {
                lock (FlowFormat) FlowFormat[flowId] = module;
            }

            Log(layer, module, title, messageFactory ?? (() => "Start"));
            return new FlowScope(popOnDispose: true, flowId);
        }

        public static void Log(Layer layer, Module module, string title, Func<string> messageFactory,
            Action<Detail> details = null, LogType unityType = LogType.Log)
        {
            if (!enabled) return;

            int flowId = CurrentFlowId;

            // (2) layer enable
            if (!IsLayerEnabled(layer)) return;

            // (4) format filter (skip if flow has no format, or format is disabled)
            if (!PassFormatFilter(flowId)) return;

            if (!PassFlowLogLimit(flowId, layer, module)) return;

            // (3) only-first per (layer,module) in this flow
            WriteLog(layer.ToString(), module.ToString(), title, messageFactory, details, unityType);
        }

        public static void Warn(Layer layer, Module module, string title, Func<string> messageFactory,
            Action<Detail> details = null)
            => Log(layer, module, title, messageFactory, details, LogType.Warning);

        public static void Error(Layer layer, Module module, string title, Func<string> messageFactory,
            Action<Detail> details = null)
            => Log(layer, module, title, messageFactory, details, LogType.Error);

        public static void LogTracking(Module module, string eventName, bool isRevenue = false, Action<Detail> details = null)
        {
            if (!enabled) return;
            if (!IsLayerEnabled(Layer.tracking)) return;
            if (isRevenue)
            {
                if (!enableTrackingRevenueLog) return;
            }
            else
            {
                if (!enableTrackingEventLog) return;
            }

            int flowId = CurrentFlowId;
            if (!PassFormatFilter(flowId)) return;
            if (!PassFlowLogLimit(flowId, Layer.tracking, module)) return;

            WriteLog(Layer.tracking.ToString(), module.ToString(), isRevenue ? "Revenue" : "Event",
                () => eventName, details, LogType.Log, enableTrackingParamDetails);
        }

        public static bool Guard(Layer layer, Module module, string titleIfBlocked, bool conditionPass,
            Func<string> blockedMessageFactory, Action<Detail> details = null)
        {
            if (conditionPass) return true;
            Warn(layer, module, titleIfBlocked, blockedMessageFactory, details);
            return false;
        }

        // =========================================================
        // History
        // =========================================================

        public static string GetHistorySequentialText(Channel? filterChannel = null)
        {
            var filtered = FilterHistory(filterChannel);
            if (filtered.Count == 0)
                return "No debug flow log matches the current filter.";

            var builder = new StringBuilder(filtered.Count * 96);
            builder.AppendLine($"Total flow logs: {historySequence}");
            builder.AppendLine($"Matched logs: {filtered.Count}/{SequentialHistory.Count}");
            builder.AppendLine($"Filter channel: {DescribeChannelFilter(filterChannel)}");
            builder.AppendLine();

            for (int i = 0; i < filtered.Count; i++)
            {
                var entry = filtered[i];
                builder.Append(entry.Sequence.ToString("0000"));
                builder.Append(". ");
                builder.AppendLine(entry.RenderedText);
            }

            return builder.ToString().TrimEnd();
        }

        public static string GetHistoryCountText(Channel? filterChannel = null)
        {
            var filtered = FilterHistory(filterChannel);
            if (filtered.Count == 0)
                return "No debug flow log matches the current filter.";

            var countByLine = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < filtered.Count; i++)
            {
                string key = filtered[i].CountKey;
                if (countByLine.TryGetValue(key, out var count))
                    countByLine[key] = count + 1;
                else
                    countByLine[key] = 1;
            }

            var pairs = new List<KeyValuePair<string, int>>(countByLine);
            pairs.Sort((a, b) =>
            {
                int byCount = b.Value.CompareTo(a.Value);
                return byCount != 0 ? byCount : string.CompareOrdinal(a.Key, b.Key);
            });

            var builder = new StringBuilder(pairs.Count * 72);
            builder.AppendLine($"Total flow logs: {historySequence}");
            builder.AppendLine($"Matched logs: {filtered.Count}/{SequentialHistory.Count}");
            builder.AppendLine($"Unique lines: {pairs.Count}");
            builder.AppendLine($"Filter channel: {DescribeChannelFilter(filterChannel)}");
            builder.AppendLine();

            for (int i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                builder.Append(i + 1);
                builder.Append(". ");
                builder.Append(pair.Key);
                builder.Append(" | count=");
                builder.Append(pair.Value);
                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        public static string GetHistorySummaryText(Channel? filterChannel = null)
        {
            var filtered = FilterHistory(filterChannel);
            string latest = filtered.Count == 0 ? "-" : FirstLine(filtered[filtered.Count - 1].RenderedText);
            return $"Total: {historySequence} | Matched: {filtered.Count} | Stored: {SequentialHistory.Count} | Latest: {latest}";
        }

        public static void ClearHistory()
        {
            SequentialHistory.Clear();
            HistoryCountByLine.Clear();
            historySequence = 0;
            lastHistoryLine = "";
        }

        // =========================================================
        // Internals
        // =========================================================

        private static int CurrentFlowId => (flowStack == null || flowStack.Count == 0) ? 0 : flowStack.Peek();

        private static bool IsLayerEnabled(Layer layer)
        {
            switch (layer)
            {
                case Layer.sys: return enableLayerSys;
                case Layer.adcore: return enableLayerAdcore;
                case Layer.group: return enableLayerGroup;
                case Layer.tracking: return enableLayerTracking;
                default: return true;
            }
        }

        private static bool IsFormatModule(Module module)
        {
            switch (module)
            {
                case Module.format_fa:
                case Module.format_rw:
                case Module.format_al:
                case Module.format_ar:
                case Module.format_bn:
                case Module.format_mrec:
                case Module.format_pu:
                case Module.format_cl:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsFormatEnabled(Module formatModule)
        {
            switch (formatModule)
            {
                case Module.format_fa: return enableFormatFA;
                case Module.format_rw: return enableFormatRW;
                case Module.format_al: return enableFormatAL;
                case Module.format_ar: return enableFormatAR;
                case Module.format_bn: return enableFormatBN;
                case Module.format_mrec: return enableFormatMREC;
                case Module.format_pu: return enableFormatPU;
                case Module.format_cl: return enableFormatCL;
                default: return false;
            }
        }

        /// <summary>
        /// If filter enabled:
        /// - flow MUST have a detected format (sys + format_*) else SKIP (per your decision).
        /// - and that format must be enabled.
        /// </summary>
        private static bool PassFormatFilter(int flowId)
        {
            if (!enableFormatFilter) return true;

            if (flowId <= 0) return false; // no flow => treat as "no format" => skip

            Module fmt;
            lock (FlowFormat)
            {
                if (!FlowFormat.TryGetValue(flowId, out fmt))
                    return false; // no format => skip
            }

            return IsFormatEnabled(fmt);
        }

        private static bool PassFlowLogLimit(int flowId, Layer layer, Module module)
        {
            if (flowId <= 0)
                return true;

            if (OnlyFirstLogPerFlow)
                return MarkFirstLayerModuleLog(flowId, FirstLogPerFlowKey);

            if (onlyFirstLogPerLayerModuleInFlow)
            {
                int key = MakeLayerModuleKey(layer, module);
                return MarkFirstLayerModuleLog(flowId, key);
            }

            return true;
        }

        private static int MakeLayerModuleKey(Layer layer, Module module)
        {
            // pack into int: low 16 bits = module, high 16 bits = layer
            return (((int)layer & 0xFFFF) << 16) | ((int)module & 0xFFFF);
        }

        /// <summary>
        /// returns true if this is the FIRST time this (layer,module) logs in this flow
        /// </summary>
        private static bool MarkFirstLayerModuleLog(int flowId, int key)
        {
            lock (FlowLoggedKeys)
            {
                if (!FlowLoggedKeys.TryGetValue(flowId, out var set) || set == null)
                {
                    set = new HashSet<int>();
                    FlowLoggedKeys[flowId] = set;
                }

                return set.Add(key);
            }
        }

        private static void CleanupFlowCaches(int flowId)
        {
            lock (FlowFormat) FlowFormat.Remove(flowId);
            lock (FlowLoggedKeys) FlowLoggedKeys.Remove(flowId);
        }

        private static bool TryParse<TEnum>(string s, out TEnum e) where TEnum : struct
        {
            if (string.IsNullOrEmpty(s))
            {
                e = default;
                return false;
            }

            return Enum.TryParse(s, ignoreCase: true, out e);
        }

        private static List<FlowHistoryEntry> FilterHistory(Channel? filterChannel)
        {
            var result = new List<FlowHistoryEntry>(SequentialHistory.Count);
            for (int i = 0; i < SequentialHistory.Count; i++)
            {
                var entry = SequentialHistory[i];
                if (filterChannel != null && entry.Channel != filterChannel)
                    continue;

                result.Add(entry);
            }

            return result;
        }

        private static void RecordHistory(string renderedText, string countKey, Channel? channel)
        {
            historySequence++;
            lastHistoryLine = renderedText ?? "";

            var entry = new FlowHistoryEntry(historySequence, lastHistoryLine, countKey, channel);
            SequentialHistory.Add(entry);
            if (SequentialHistory.Count > MaxHistoryEntries)
                SequentialHistory.RemoveAt(0);

            if (HistoryCountByLine.TryGetValue(countKey, out var count))
                HistoryCountByLine[countKey] = count + 1;
            else
                HistoryCountByLine[countKey] = 1;
        }

        private static string BuildCountKey(string layer, string module, string title, string msg)
        {
            var builder = new StringBuilder(96);
            builder.Append('[').Append(layer ?? "").Append(']')
                .Append('[').Append(module ?? "").Append(']')
                .Append(' ')
                .Append(title ?? "");

            if (!string.IsNullOrEmpty(msg))
                builder.Append(' ').Append(msg);

            return builder.ToString();
        }

        private static Channel? ResolveHistoryChannel(int flowId, string moduleToken)
        {
            if (TryParse(moduleToken, out Module directModule))
            {
                var directChannel = MapFormatModuleToChannel(directModule);
                if (directChannel.HasValue)
                    return directChannel;
            }

            if (flowId > 0)
            {
                lock (FlowFormat)
                {
                    if (FlowFormat.TryGetValue(flowId, out var formatModule))
                        return MapFormatModuleToChannel(formatModule);
                }
            }

            return null;
        }

        private static Channel? MapFormatModuleToChannel(Module module)
        {
            return module switch
            {
                Module.format_al => Channel.AppLaunch,
                Module.format_ar => Channel.AppResume,
                Module.format_fa => Channel.ForceAd,
                Module.format_rw => Channel.Rewarded,
                Module.format_bn => Channel.Banner,
                Module.format_mrec => Channel.Mrec,
                Module.format_pu => Channel.Popup,
                Module.format_cl => Channel.Collap,
                _ => null
            };
        }

        private static string DescribeChannelFilter(Channel? channel)
        {
            if (!channel.HasValue)
                return "All";

            return channel.Value switch
            {
                Channel.AppLaunch => "AppLaunch",
                Channel.AppResume => "AppResume",
                Channel.ForceAd => "ForceAd",
                Channel.Rewarded => "Rewarded",
                Channel.Banner => "Banner",
                Channel.Mrec => "Mrec",
                Channel.Popup => "Popup",
                Channel.Collap => "Collap",
                _ => channel.Value.ToString()
            };
        }

        private static string FirstLine(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "-";

            int index = value.IndexOf('\n');
            return index >= 0 ? value.Substring(0, index) : value;
        }

        private static void WriteLog(string layer, string module, string title, Func<string> messageFactory,
            Action<Detail> details, LogType unityType, bool forceDetails = false)
        {
            if (!enabled) return;

            string msg = messageFactory != null ? messageFactory() : "";
            int flowId = CurrentFlowId;
            Channel? channel = ResolveHistoryChannel(flowId, module);

            var sb = new StringBuilder(160);
            sb.Append('[').Append(layer ?? "").Append(']')
              .Append('[').Append(module ?? "").Append(']')
              .Append(' ')
              .Append(title ?? "");

            if (!string.IsNullOrEmpty(msg))
                sb.Append(' ').Append(msg);

            if (flowId > 0)
                sb.Append(" #F").Append(flowId);

            if (details != null && (verbose || forceDetails))
            {
                var d = Detail.Rent();
                try
                {
                    details(d);
                    if (d.Count > 0)
                    {
                        sb.AppendLine();
                        for (int i = 0; i < d.Count; i++)
                            sb.Append("- ").AppendLine(d[i]);
                    }
                }
                finally
                {
                    d.Return();
                }
            }

            string renderedText = sb.ToString();
            RecordHistory(renderedText, BuildCountKey(layer, module, title, msg), channel);

            switch (unityType)
            {
                case LogType.Warning: UnityEngine.Debug.LogWarning(renderedText); break;
                case LogType.Error:
                case LogType.Exception: UnityEngine.Debug.LogError(renderedText); break;
                default: UnityEngine.Debug.Log(renderedText); break;
            }
        }

        // =========================================================
        // Scopes
        // =========================================================

        private sealed class FlowScope : IDisposable
        {
            private bool disposed;
            private readonly bool popOnDispose;
            private readonly int flowId;

            public FlowScope(bool popOnDispose, int flowId)
            {
                this.popOnDispose = popOnDispose;
                this.flowId = flowId;
            }

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;

                if (!enabled) return;

                // Only pop if this scope created a new flow id.
                if (popOnDispose && flowStack != null && flowStack.Count > 0)
                {
                    // pop current
                    flowStack.Pop();

                    // cleanup caches for that flow
                    CleanupFlowCaches(flowId);
                }
            }
        }

        private sealed class DummyScope : IDisposable
        {
            public static readonly DummyScope Instance = new DummyScope();
            public void Dispose() { }
        }

        // =========================================================
        // Details
        // =========================================================

        public sealed class Detail
        {
            private static readonly Stack<Detail> pool = new Stack<Detail>(16);
            private readonly List<string> lines = new List<string>(8);

            public int Count => lines.Count;
            public string this[int index] => lines[index];

            public Detail Add(string line)
            {
                if (!string.IsNullOrEmpty(line)) lines.Add(line);
                return this;
            }

            public Detail AddKV(string k, object v)
            {
                lines.Add($"{k}={v}");
                return this;
            }

            public static Detail Rent()
            {
                lock (pool)
                {
                    if (pool.Count > 0)
                    {
                        var d = pool.Pop();
                        d.lines.Clear();
                        return d;
                    }
                }
                return new Detail();
            }

            public void Return()
            {
                lines.Clear();
                lock (pool)
                {
                    pool.Push(this);
                }
            }
        }
    }
}
