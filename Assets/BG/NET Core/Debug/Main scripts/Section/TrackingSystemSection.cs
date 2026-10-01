using System;
using Sirenix.OdinInspector;
using BG_Library.NET;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using BG_Library.NET.Tracking;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BG_Library.DEBUG
{
    [DisallowMultipleComponent]
    public sealed class TrackingSystemSection : MonoBehaviour
    {
        private enum TrackingViewerMode
        {
            Sequential,
            Count
        }

        private enum TrackingViewerFilterMode
        {
            AllTracking,
            RequestFlow,
            ActionFlow
        }

        private enum TrackingViewerActionToken
        {
            All,
            Show,
            Hide,
            Activate
        }

        private readonly struct TrackingHistoryRecord
        {
            public TrackingHistoryRecord(long sequence, float elapsedSeconds, string eventName, string explanation, Channel? channel,
                TrackingHistoryAction action, TrackingHistoryScope scope)
            {
                Sequence = sequence;
                ElapsedSeconds = elapsedSeconds;
                EventName = eventName ?? string.Empty;
                Explanation = explanation ?? string.Empty;
                Channel = channel;
                Action = action;
                Scope = scope;
            }

            public long Sequence { get; }
            public float ElapsedSeconds { get; }
            public string EventName { get; }
            public string Explanation { get; }
            public Channel? Channel { get; }
            public TrackingHistoryAction Action { get; }
            public TrackingHistoryScope Scope { get; }
        }

        private sealed class TrackingRequestIdOption
        {
            public string Id;
            public readonly HashSet<string> RequestPatterns = new HashSet<string>(StringComparer.Ordinal);
        }

        private const int MaxHistoryEntries = 512;
        private static readonly List<TrackingHistoryRecord> HistoryRecords = new List<TrackingHistoryRecord>(MaxHistoryEntries);
        private static long historyVersion;
        private static bool historyHookBound;
        private static TrackingSystemSection activeInstance;

        [SerializeField] private Text titleText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text summaryText;

        [Header("Tracking config")]
        [SerializeField] private Toggle trackingEventToggle;
        [SerializeField] private Toggle trackingRevenueToggle;
        [SerializeField] private Toggle trackingDetailsToggle;
        [SerializeField] private Toggle trackingFirebaseToggle;

        [Header("Tracking tools")]
        [SerializeField] private Button overviewSequentialButton;
        [SerializeField] private Button overviewCountButton;
        [SerializeField] private Button clearHistoryButton;
        [SerializeField] private DebugDetailViewer detailViewer;
        [SerializeField] private DebugOptionPicker optionPicker;

        [Header("Legend")]
        [SerializeField] private Text ruleLegendText;

        private bool showVietnameseTrackingDescription;
        private bool trackingViewerOpen;
        private TrackingViewerMode trackingViewerMode = TrackingViewerMode.Sequential;
        private TrackingViewerFilterMode trackingFilterMode = TrackingViewerFilterMode.AllTracking;
        private TrackingViewerActionToken trackingActionFilter = TrackingViewerActionToken.All;
        private readonly List<string> detectedTrackingPoses = new List<string>();
        private readonly List<Channel> channelFilterOptions = new List<Channel>();
        private readonly Dictionary<Channel, List<TrackingRequestIdOption>> detectedRequestIdsByChannel = new Dictionary<Channel, List<TrackingRequestIdOption>>();
        private string selectedTrackingPos = string.Empty;
        private Channel? selectedTrackingChannel;
        private string selectedRequestId = string.Empty;

        private void Awake()
        {
            activeInstance = this;
            EnsureHistoryHookBound();
            ApplyStaticText();
            BindActions();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (activeInstance == this)
                activeInstance = null;

            trackingViewerOpen = false;
            ReleaseHistoryHook();
        }

        public void Refresh()
        {
            var state = GetCurrentState();
            bool trackingFirebaseEnabled = NetConfigsSO.Ins != null && NetConfigsSO.Ins.Tracking_SendToFirebase;

            SetToggle(trackingEventToggle, state.EnableTrackingEventLog);
            SetToggle(trackingRevenueToggle, state.EnableTrackingRevenueLog);
            SetToggle(trackingDetailsToggle, state.EnableTrackingParamDetails);
            SetToggle(trackingFirebaseToggle, trackingFirebaseEnabled);
            SetToggleInteractable(trackingEventToggle, false);
            SetToggleInteractable(trackingRevenueToggle, false);
            SetToggleInteractable(trackingDetailsToggle, false);
            SetToggleInteractable(trackingFirebaseToggle, false);

            if (summaryText != null)
            {
                summaryText.text =
                    $"Event: {(state.EnableTrackingEventLog ? "ON" : "OFF")} | Revenue: {(state.EnableTrackingRevenueLog ? "ON" : "OFF")} | Details: {(state.EnableTrackingParamDetails ? "ON" : "OFF")}\n" +
                    $"{BuildTrackingFirebaseSummary(trackingFirebaseEnabled)}\n" +
                    BuildTrackingHistorySummaryText();
            }
        }

        private void ApplyStaticText()
        {
            if (titleText != null)
                titleText.text = "Tracking system";

            if (hintText != null)
                hintText.text = "This section shows preset-based tracking debug status, history overview, and the tracking rule guide.";

            if (ruleLegendText != null)
                ruleLegendText.text = NetTrackingEventExplainer.NormalizeDisplayText(NetTrackingEventExplainer.GetLegendText());
        }

        private void BindActions()
        {
            ClearToggleListeners(trackingEventToggle);
            ClearToggleListeners(trackingRevenueToggle);
            ClearToggleListeners(trackingDetailsToggle);
            ClearToggleListeners(trackingFirebaseToggle);

            BindButton(overviewSequentialButton, OpenTrackingOverviewSequential);
            BindButton(overviewCountButton, OpenTrackingOverviewCount);
            BindButton(clearHistoryButton, () =>
            {
                ClearTrackingHistory();
                Refresh();
            });
        }

        private NetFlowDebugSystem.PresetState GetCurrentState()
        {
            DebugPreset preset = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.Debug_Preset : NetFlowDebugSystem.CurrentPreset;
            return NetFlowDebugSystem.GetPresetState(preset);
        }

        private void OpenTrackingOverviewSequential()
        {
            trackingViewerMode = TrackingViewerMode.Sequential;
            OpenTrackingViewer();
        }

        private void OpenTrackingOverviewCount()
        {
            trackingViewerMode = TrackingViewerMode.Count;
            OpenTrackingViewer();
        }

        private void OpenTrackingViewer()
        {
            if (detailViewer == null)
                detailViewer = transform.root.GetComponentInChildren<DebugDetailViewer>(true);
            if (optionPicker == null)
                optionPicker = transform.root.GetComponentInChildren<DebugOptionPicker>(true);

            trackingViewerOpen = true;
            RebuildTrackingFilterOptions();
            detailViewer?.OpenTrackingViewer(
                trackingViewerMode == TrackingViewerMode.Sequential ? "Tracking Overview | Sequential" : "Tracking Overview | Count",
                BuildTrackingViewerBody,
                showVietnameseTrackingDescription,
                OnTrackingDescriptionChanged,
                OnTrackingViewerClosed);
            RefreshTrackingViewerChrome();
        }

        private void OnTrackingDescriptionChanged(bool value)
        {
            showVietnameseTrackingDescription = value;
        }

        private void OnTrackingViewerClosed()
        {
            trackingViewerOpen = false;
        }

        private void BindButton(Button button, Action action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private static void ClearToggleListeners(Toggle toggle)
        {
            if (toggle == null)
                return;

            toggle.onValueChanged.RemoveAllListeners();
        }

        private static string BuildTrackingFirebaseSummary(bool trackingFirebaseEnabled)
        {
            return trackingFirebaseEnabled ? "Firebase: ON (release AAB only)" : "Firebase: OFF";
        }

        private static void SetToggleInteractable(Toggle toggle, bool interactable)
        {
            if (toggle != null)
                toggle.interactable = interactable;
        }

        private string BuildTrackingViewerBody(bool includeVietnameseDescription)
        {
            return trackingViewerMode == TrackingViewerMode.Sequential
                ? BuildTrackingHistorySequentialText(includeVietnameseDescription)
                : BuildTrackingHistoryCountText(includeVietnameseDescription);
        }

        private void CopyTrackingViewerToClipboard()
        {
            string value = BuildTrackingViewerBody(showVietnameseTrackingDescription);
            GUIUtility.systemCopyBuffer = value ?? string.Empty;
            detailViewer?.ShowTrackingCopyFeedback();
        }

        private void RefreshTrackingViewerChrome()
        {
            if (detailViewer == null || !trackingViewerOpen)
                return;

            detailViewer.ConfigureTrackingFilters(
                BuildTrackingFilterModeId(),
                "All Tracking",
                "Request Flow",
                "Action Flow",
                BuildTrackingPrimaryLabel(),
                BuildTrackingSecondaryLabel(),
                trackingFilterMode != TrackingViewerFilterMode.AllTracking,
                trackingFilterMode == TrackingViewerFilterMode.RequestFlow || trackingFilterMode == TrackingViewerFilterMode.ActionFlow,
                SelectAllTrackingMode,
                SelectRequestFlowMode,
                SelectActionFlowMode,
                OpenTrackingPrimaryPicker,
                OnTrackingSecondaryPressed,
                ResetTrackingFilters,
                CopyTrackingViewerToClipboard);
            detailViewer.RefreshTrackingViewer();
        }

        private string BuildTrackingFilterModeId()
        {
            return trackingFilterMode switch
            {
                TrackingViewerFilterMode.RequestFlow => "request",
                TrackingViewerFilterMode.ActionFlow => "action",
                _ => "all"
            };
        }

        private string BuildTrackingPrimaryLabel()
        {
            if (trackingFilterMode == TrackingViewerFilterMode.AllTracking)
                return string.Empty;

            if (trackingFilterMode == TrackingViewerFilterMode.RequestFlow)
                return $"Channel: {(selectedTrackingChannel.HasValue ? FormatChannelToken(selectedTrackingChannel.Value) : "All")}";

            return $"Pos: {FormatOptionLabel(selectedTrackingPos)}";
        }

        private string BuildTrackingSecondaryLabel()
        {
            if (trackingFilterMode == TrackingViewerFilterMode.AllTracking)
                return string.Empty;

            if (trackingFilterMode == TrackingViewerFilterMode.RequestFlow)
                return $"ID: {FormatOptionLabel(selectedRequestId)}";

            return $"Action: {FormatTrackingAction(trackingActionFilter)}";
        }

        private void SelectAllTrackingMode()
        {
            trackingFilterMode = TrackingViewerFilterMode.AllTracking;
            RefreshTrackingViewerChrome();
        }

        private void SelectRequestFlowMode()
        {
            trackingFilterMode = TrackingViewerFilterMode.RequestFlow;
            RefreshTrackingViewerChrome();
        }

        private void SelectActionFlowMode()
        {
            trackingFilterMode = TrackingViewerFilterMode.ActionFlow;
            RefreshTrackingViewerChrome();
        }

        private void ResetTrackingFilters()
        {
            trackingFilterMode = TrackingViewerFilterMode.AllTracking;
            trackingActionFilter = TrackingViewerActionToken.All;
            selectedTrackingPos = string.Empty;
            selectedTrackingChannel = null;
            selectedRequestId = string.Empty;
            RebuildTrackingFilterOptions();
            RefreshTrackingViewerChrome();
        }

        private void RebuildTrackingFilterOptions()
        {
            detectedTrackingPoses.Clear();
            detectedRequestIdsByChannel.Clear();
            channelFilterOptions.Clear();

            var seenPoses = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < HistoryRecords.Count; i++)
            {
                string eventName = HistoryRecords[i].EventName;

                if (TryExtractTrackingTarget(eventName, out _, out var pos) && seenPoses.Add(pos))
                    detectedTrackingPoses.Add(pos);

                if (TryExtractRequestSeedApiInfo(eventName, out var channel, out var id))
                {
                    if (!detectedRequestIdsByChannel.TryGetValue(channel.Value, out var items))
                    {
                        items = new List<TrackingRequestIdOption>();
                        detectedRequestIdsByChannel[channel.Value] = items;
                    }

                    var option = items.Find(item => string.Equals(item.Id, id, StringComparison.Ordinal));
                    if (option == null)
                    {
                        option = new TrackingRequestIdOption { Id = id };
                        items.Add(option);
                    }

                    option.RequestPatterns.Add($"_rq_{id}_");
                }
            }

            foreach (var pair in detectedRequestIdsByChannel)
                pair.Value.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

            AddDefaultChannelOptions();

            detectedTrackingPoses.Sort(StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(selectedTrackingPos) && !detectedTrackingPoses.Contains(selectedTrackingPos))
                selectedTrackingPos = string.Empty;

            if (selectedTrackingChannel.HasValue && !channelFilterOptions.Contains(selectedTrackingChannel.Value))
            {
                selectedTrackingChannel = null;
                selectedRequestId = string.Empty;
            }

            var idOptions = GetSelectedRequestIdOptions();
            if (!string.IsNullOrEmpty(selectedRequestId) && !ContainsRequestId(idOptions, selectedRequestId))
                selectedRequestId = string.Empty;
        }

        private void AddDefaultChannelOptions()
        {
            AddChannelOption(Channel.AppLaunch);
            AddChannelOption(Channel.AppResume);
            AddChannelOption(Channel.Rewarded);
            AddChannelOption(Channel.ForceAd);
            AddChannelOption(Channel.Popup);
            AddChannelOption(Channel.Banner);
            AddChannelOption(Channel.Mrec);
            AddChannelOption(Channel.Collap);
        }

        private void AddChannelOption(Channel channel)
        {
            if (!channelFilterOptions.Contains(channel))
                channelFilterOptions.Add(channel);
        }

        private void OpenTrackingPrimaryPicker()
        {
            if (trackingFilterMode == TrackingViewerFilterMode.AllTracking || optionPicker == null)
                return;

            if (trackingFilterMode == TrackingViewerFilterMode.RequestFlow)
            {
                var channelOptions = new List<string>(channelFilterOptions.Count + 1) { "All" };
                for (int i = 0; i < channelFilterOptions.Count; i++)
                    channelOptions.Add(FormatChannelToken(channelFilterOptions[i]));

                optionPicker.Open(
                    "Select Channel",
                    channelOptions,
                    selectedTrackingChannel.HasValue ? FormatChannelToken(selectedTrackingChannel.Value) : "All",
                    option =>
                    {
                        selectedTrackingChannel = TryParseChannelOption(option, out var channel) ? channel : (Channel?)null;
                        selectedRequestId = string.Empty;
                        RefreshTrackingViewerChrome();
                    },
                    "Choose one request channel to filter tracking.");
                return;
            }

            var options = new List<string>(detectedTrackingPoses.Count + 1) { "All" };
            options.AddRange(detectedTrackingPoses);

            optionPicker.Open(
                "Select Pos",
                options,
                FormatOptionLabel(selectedTrackingPos),
                option =>
                {
                    selectedTrackingPos = string.Equals(option, "All", StringComparison.Ordinal) ? string.Empty : option;
                    RefreshTrackingViewerChrome();
                },
                "Choose one pos to filter action tracking.");
        }

        private void OnTrackingSecondaryPressed()
        {
            if (trackingFilterMode == TrackingViewerFilterMode.AllTracking)
                return;

            if (trackingFilterMode == TrackingViewerFilterMode.ActionFlow)
            {
                int count = Enum.GetValues(typeof(TrackingViewerActionToken)).Length;
                trackingActionFilter = (TrackingViewerActionToken)(((int)trackingActionFilter + 1) % count);
                RefreshTrackingViewerChrome();
                return;
            }

            if (optionPicker == null)
                return;

            var idOptions = GetSelectedRequestIdOptions();
            var options = new List<string>(idOptions.Count + 1) { "All" };
            for (int i = 0; i < idOptions.Count; i++)
                options.Add(idOptions[i].Id);

            optionPicker.Open(
                "Select ID",
                options,
                FormatOptionLabel(selectedRequestId),
                option =>
                {
                    selectedRequestId = string.Equals(option, "All", StringComparison.Ordinal) ? string.Empty : option;
                    RefreshTrackingViewerChrome();
                },
                "Choose one group ID to filter request tracking.");
        }

        private List<TrackingRequestIdOption> GetSelectedRequestIdOptions()
        {
            if (!selectedTrackingChannel.HasValue)
            {
                var merged = new Dictionary<string, TrackingRequestIdOption>(StringComparer.Ordinal);
                foreach (var pair in detectedRequestIdsByChannel)
                {
                    var sourceItems = pair.Value;
                    for (int i = 0; i < sourceItems.Count; i++)
                    {
                        var source = sourceItems[i];
                        if (!merged.TryGetValue(source.Id, out var target))
                        {
                            target = new TrackingRequestIdOption { Id = source.Id };
                            merged[source.Id] = target;
                        }

                        foreach (var pattern in source.RequestPatterns)
                            target.RequestPatterns.Add(pattern);
                    }
                }

                var mergedItems = new List<TrackingRequestIdOption>(merged.Values);
                mergedItems.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
                return mergedItems;
            }

            return detectedRequestIdsByChannel.TryGetValue(selectedTrackingChannel.Value, out var items)
                ? items
                : new List<TrackingRequestIdOption>();
        }

        private bool MatchesActiveTrackingFilter(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return false;

            if (trackingFilterMode == TrackingViewerFilterMode.AllTracking)
                return true;

            if (trackingFilterMode == TrackingViewerFilterMode.ActionFlow)
            {
                if (!TryExtractTrackingTarget(eventName, out var actionToken, out var targetToken))
                    return false;

                if (!string.IsNullOrEmpty(selectedTrackingPos) &&
                    !string.Equals(targetToken, selectedTrackingPos, StringComparison.Ordinal))
                    return false;

                return trackingActionFilter switch
                {
                    TrackingViewerActionToken.Show => string.Equals(actionToken, "sh", StringComparison.Ordinal),
                    TrackingViewerActionToken.Hide => string.Equals(actionToken, "hid", StringComparison.Ordinal),
                    TrackingViewerActionToken.Activate => string.Equals(actionToken, "act", StringComparison.Ordinal),
                    _ => true
                };
            }

            if (!IsRequestFlowEvent(eventName))
                return false;

            if (!selectedTrackingChannel.HasValue)
                return true;

            string requestSeedPattern = BuildRequestChannelSeedPattern(selectedTrackingChannel.Value);
            if (eventName.Contains(requestSeedPattern, StringComparison.Ordinal))
                return true;

            var idOptions = GetSelectedRequestIdOptions();
            TrackingRequestIdOption selectedOption = string.IsNullOrEmpty(selectedRequestId)
                ? null
                : idOptions.Find(item => string.Equals(item.Id, selectedRequestId, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(selectedRequestId))
                return MatchesAnyRequestPatterns(idOptions, eventName);

            return MatchesRequestPatterns(selectedOption, eventName);
        }

        private static bool MatchesAnyRequestPatterns(List<TrackingRequestIdOption> options, string eventName)
        {
            for (int i = 0; i < options.Count; i++)
            {
                if (MatchesRequestPatterns(options[i], eventName))
                    return true;
            }

            return false;
        }

        private static bool MatchesRequestPatterns(TrackingRequestIdOption option, string eventName)
        {
            if (option == null)
                return false;

            foreach (var pattern in option.RequestPatterns)
            {
                if (eventName.Contains(pattern, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static string FormatTrackingAction(TrackingViewerActionToken action)
        {
            return action switch
            {
                TrackingViewerActionToken.Show => "Show",
                TrackingViewerActionToken.Hide => "Hide",
                TrackingViewerActionToken.Activate => "Activate",
                _ => "All"
            };
        }

        private static string FormatOptionLabel(string value)
        {
            return string.IsNullOrEmpty(value) ? "All" : value;
        }

        private static string FormatChannelToken(Channel channel)
        {
            return channel switch
            {
                Channel.AppLaunch => "AL",
                Channel.AppResume => "AR",
                Channel.ForceAd => "FA",
                Channel.Rewarded => "RW",
                Channel.Banner => "BN",
                Channel.Mrec => "MR",
                Channel.Popup => "PU",
                Channel.Collap => "CL",
                _ => "??"
            };
        }

        private static string ChannelToken(Channel channel)
        {
            return channel switch
            {
                Channel.AppLaunch => "al",
                Channel.AppResume => "ar",
                Channel.ForceAd => "fa",
                Channel.Rewarded => "rw",
                Channel.Banner => "bn",
                Channel.Mrec => "mr",
                Channel.Popup => "pu",
                Channel.Collap => "cl",
                _ => "xx"
            };
        }

        private static bool TryParseChannelOption(string value, out Channel channel)
        {
            if (string.IsNullOrEmpty(value) || string.Equals(value, "All", StringComparison.Ordinal))
            {
                channel = default;
                return false;
            }

            return TryParseChannelToken(value.ToLowerInvariant(), out channel);
        }

        private static bool ContainsRequestId(List<TrackingRequestIdOption> options, string id)
        {
            if (string.IsNullOrEmpty(id))
                return true;

            for (int i = 0; i < options.Count; i++)
            {
                if (string.Equals(options[i].Id, id, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool TryExtractTrackingTarget(string eventName, out string actionToken, out string targetToken)
        {
            actionToken = string.Empty;
            targetToken = string.Empty;

            if (string.IsNullOrEmpty(eventName))
                return false;

            string[] tokens = eventName.Split('_');
            if (tokens.Length < 4 || !string.Equals(tokens[0], "ad", StringComparison.Ordinal))
                return false;

            if (TryExtractNewShowCallbackTarget(tokens, out actionToken, out targetToken) ||
                TryExtractNewLayerActionTarget(tokens, out actionToken, out targetToken))
            {
                return !string.IsNullOrEmpty(targetToken);
            }

            return false;
        }

        private static bool IsRequestFlowEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return false;

            return eventName.Contains("_rq_", StringComparison.Ordinal);
        }

        private static bool TryExtractNewShowCallbackTarget(string[] tokens, out string actionToken, out string targetToken)
        {
            actionToken = string.Empty;
            targetToken = string.Empty;

            if (tokens.Length < 6 || tokens[1] != "evt" || tokens[3] != "sh")
                return false;

            actionToken = "sh";
            targetToken = tokens[5];
            return true;
        }

        private static bool TryExtractNewLayerActionTarget(string[] tokens, out string actionToken, out string targetToken)
        {
            actionToken = string.Empty;
            targetToken = string.Empty;

            if (tokens.Length < 4 || !IsLayerToken(tokens[1]))
                return false;

            if (tokens[1] == "gr")
            {
                int actionIndex = 2;
                int targetIndex = 4;
                if (tokens.Length >= 6 && TryParseChannelToken(tokens[2], out _) && IsPosActionToken(tokens[3]))
                {
                    actionIndex = 3;
                    targetIndex = 5;
                }

                if (tokens.Length <= targetIndex || !IsPosActionToken(tokens[actionIndex]))
                    return false;

                actionToken = tokens[actionIndex];
                targetToken = tokens[targetIndex];
                return targetToken != "n" && targetToken != "api";
            }

            if (tokens.Length >= 5 && TryParseChannelToken(tokens[2], out _) && IsPosActionToken(tokens[3]))
            {
                actionToken = tokens[3];
                targetToken = tokens[4];
                return targetToken != "n" && targetToken != "api";
            }

            return false;
        }

        private static bool TryExtractRequestSeedApiInfo(string eventName, out Channel? channel, out string id)
        {
            channel = null;
            id = string.Empty;

            if (string.IsNullOrEmpty(eventName) ||
                !eventName.Contains("_api", StringComparison.Ordinal))
            {
                return false;
            }

            string[] tokens = eventName.Split('_');
            if (tokens.Length < 6 || !IsLayerToken(tokens[1]))
                return false;

            if (!TryParseChannelToken(tokens[2], out var parsedChannel) || tokens[3] != "rq")
                return false;

            if (!IsGroupIdToken(tokens[4]))
                return false;

            channel = parsedChannel;
            id = tokens[4];
            return true;
        }

        private static bool IsLayerToken(string token)
        {
            return token == "sy" || token == "ac" || token == "atsy" || token == "gr";
        }

        private static bool IsPosActionToken(string token)
        {
            return token == "sh" || token == "hid" || token == "act";
        }

        private static string BuildRequestChannelSeedPattern(Channel channel)
        {
            return $"_{ChannelToken(channel)}_rq_";
        }

        private static bool IsGroupIdToken(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length != 7)
                return false;

            string prefix = token.Substring(0, 2);
            if (prefix != "ao" && prefix != "rw" && prefix != "fa" && prefix != "pu" &&
                prefix != "bn" && prefix != "mr" && prefix != "cl")
                return false;

            for (int i = 2; i < token.Length; i++)
            {
                if (!char.IsLetterOrDigit(token[i]))
                    return false;
            }

            return true;
        }

        private static bool TryParseChannelToken(string token, out Channel channel)
        {
            switch (token)
            {
                case "al": channel = Channel.AppLaunch; return true;
                case "ar": channel = Channel.AppResume; return true;
                case "fa": channel = Channel.ForceAd; return true;
                case "rw": channel = Channel.Rewarded; return true;
                case "bn": channel = Channel.Banner; return true;
                case "mr": channel = Channel.Mrec; return true;
                case "pu": channel = Channel.Popup; return true;
                case "cl": channel = Channel.Collap; return true;
                default:
                    channel = default;
                    return false;
            }
        }

        private string BuildTrackingHistorySummaryText()
        {
            var all = GetFilteredHistorySnapshot();
            string latest = all.Count == 0 ? "-" : all[all.Count - 1].EventName;
            long total = historyVersion;
            int stored = HistoryRecords.Count;
            return $"Total: {total} | Stored: {stored} | Latest: {latest}";
        }

        private string BuildTrackingHistorySequentialText(bool includeVietnameseDescription)
        {
            var all = GetFilteredHistorySnapshot();
            
            if (all.Count == 0)
                return "Không có tracking event nào khớp bộ lọc hiện tại.";

            var builder = new StringBuilder(all.Count * 80);
            builder.AppendLine($"Total tracked events: {historyVersion}");
            builder.AppendLine($"Stored events: {all.Count}");
            builder.AppendLine();

            for (int i = 0; i < all.Count; i++)
            {
                var entry = all[i];
                var explanation = NetTrackingEventExplainer.Explain(entry.EventName);
                builder.Append('[');
                builder.Append(FormatTrackingElapsedTime(entry.ElapsedSeconds));
                builder.Append("] ");
                builder.Append(entry.EventName);
                if (includeVietnameseDescription)
                {
                    builder.Append(" => ");
                    builder.Append(explanation.Explanation);
                }
                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private string BuildTrackingHistoryCountText(bool includeVietnameseDescription)
        {
            var all = GetFilteredHistorySnapshot();
            
            if (all.Count == 0)
                return "Không có tracking event nào khớp bộ lọc hiện tại.";

            var countByEvent = new Dictionary<string, int>();
            for (int i = 0; i < all.Count; i++)
            {
                string eventName = all[i].EventName;
                if (countByEvent.TryGetValue(eventName, out var count))
                    countByEvent[eventName] = count + 1;
                else
                    countByEvent[eventName] = 1;
            }

            var pairs = new List<KeyValuePair<string, int>>(countByEvent);
            pairs.Sort((a, b) =>
            {
                int byCount = b.Value.CompareTo(a.Value);
                return byCount != 0 ? byCount : string.CompareOrdinal(a.Key, b.Key);
            });

            var builder = new StringBuilder(pairs.Count * 64);
            builder.AppendLine($"Total tracked events: {historyVersion}");
            builder.AppendLine($"Stored events: {all.Count}");
            builder.AppendLine($"Unique event names: {pairs.Count}");
            builder.AppendLine();

            for (int i = 0; i < pairs.Count; i++)
            {
                var pair = pairs[i];
                builder.Append(i + 1);
                builder.Append(". ");
                builder.Append(pair.Key);
                builder.Append(" | count=");
                builder.Append(pair.Value);
                if (includeVietnameseDescription)
                {
                    builder.Append(" | ");
                    builder.Append(NetTrackingEventExplainer.Explain(pair.Key).Explanation);
                }
                builder.AppendLine();
            }

            return builder.ToString().TrimEnd();
        }

        private List<TrackingHistoryRecord> GetFilteredHistorySnapshot()
        {
            var snapshot = new List<TrackingHistoryRecord>(HistoryRecords.Count);
            for (int i = 0; i < HistoryRecords.Count; i++)
            {
                var entry = HistoryRecords[i];
                if (MatchesActiveTrackingFilter(entry.EventName))
                    snapshot.Add(entry);
            }

            return snapshot;
        }

        private static void EnsureHistoryHookBound()
        {
            if (historyHookBound)
                return;

            NetTrackingSystem.OnTrackedRawEvent += HandleTrackedRawEvent;
            historyHookBound = true;
        }

        private static void ReleaseHistoryHook()
        {
            if (!historyHookBound)
                return;

            NetTrackingSystem.OnTrackedRawEvent -= HandleTrackedRawEvent;
            historyHookBound = false;
        }

        private static void HandleTrackedRawEvent(string eventName)
        {
            historyVersion++;
            var explanation = NetTrackingEventExplainer.Explain(eventName);
            HistoryRecords.Add(new TrackingHistoryRecord(
                historyVersion,
                Time.realtimeSinceStartup,
                eventName,
                explanation.Explanation,
                explanation.Channel,
                explanation.Action,
                explanation.Scope));

            if (HistoryRecords.Count > MaxHistoryEntries)
                HistoryRecords.RemoveAt(0);

            activeInstance?.HandleTrackingHistoryChanged();
        }

        private static void ClearTrackingHistory()
        {
            HistoryRecords.Clear();
            historyVersion = 0;
        }

        private static string FormatTrackingElapsedTime(float elapsedSeconds)
        {
            if (elapsedSeconds < 0f)
                elapsedSeconds = 0f;

            var elapsed = TimeSpan.FromSeconds(Mathf.FloorToInt(elapsedSeconds));
            int totalHours = Mathf.FloorToInt((float)elapsed.TotalHours);
            return $"{totalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
        }

        private void HandleTrackingHistoryChanged()
        {
            if (!trackingViewerOpen)
                return;

            RebuildTrackingFilterOptions();
            RefreshTrackingViewerChrome();
        }

        private static void SetToggle(Toggle toggle, bool value)
        {
            if (toggle != null)
                toggle.isOn = value;
        }
    }
}
