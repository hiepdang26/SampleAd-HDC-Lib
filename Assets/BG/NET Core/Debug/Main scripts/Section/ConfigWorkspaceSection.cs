using System;
using System.Collections.Generic;
using System.Text;
using BG_Library.Common;
using BG_Library.NET;
using BG_Library.NET.AdSystem;
using BG_Library.NET.AdCore.MainAndroid;
using BG_Library.NET.Tracking;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
#endif

namespace BG_Library.DEBUG
{
    [DisallowMultipleComponent]
    public sealed class ConfigWorkspaceSection : MonoBehaviour
    {
        private enum ConfigViewerMode
        {
            Remote = 0,
            Cache = 1,
            Real = 2,
        }

        [SerializeField] private Text titleText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Button openViewerButton;
        [SerializeField] private Button trackingPreviewButton;

        [SerializeField] private DebugDetailViewer detailViewer;
        private bool readinessEventsBound;
        private bool hasReceivedAllJsonsComplete;
        private bool hasAdsRemoteValue;
        private bool hasAdsRealValue;
        private bool hasMediationRemoteValue;
        private bool hasMediationRealValue;

        private readonly Dictionary<string, string> mediationRemoteByCore = new Dictionary<string, string>();
        private readonly Dictionary<string, string> mediationPrefsSnapshotByCore = new Dictionary<string, string>();
        private readonly Dictionary<string, string> mediationFinalByCore = new Dictionary<string, string>();
        private readonly Dictionary<string, string> customRcRemoteCache = new Dictionary<string, string>();
        private readonly Dictionary<string, string> customRcPlayerPrefsCache = new Dictionary<string, string>();
        private readonly Dictionary<string, string> customRcFinalCache = new Dictionary<string, string>();

        private string adsRemoteCache = string.Empty;
        private string adsPrefsSnapshot = string.Empty;
        private string adsFinalCache = string.Empty;
        private string cachedLocalOverviewText = string.Empty;

        private ConfigViewerMode currentViewerMode = ConfigViewerMode.Remote;
        private string currentViewerSection = "overview";
        private string currentViewerCustomKey = string.Empty;
        private readonly Dictionary<string, HashSet<string>> collapsedTopLevelKeysByViewer = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        private readonly HashSet<string> collapseInitializedViewerKeys = new HashSet<string>(StringComparer.Ordinal);
        private const string ToggleAllViewerOptionKey = "__all__";

        private void Awake()
        {
            ApplyStaticText();
            BindActions();
            BindReadinessEvents();
        }

        private void Start()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            UnbindReadinessEvents();
        }

        public void Refresh()
        {
            RefreshConfigCaches();
            RefreshSummary();
            RefreshViewer();
        }

        public void OpenConfigsViewer()
        {
            detailViewer.ShowConfigViewer();
            RefreshViewer();
        }

        public void OpenTrackingPreview()
        {
            if (detailViewer == null)
                detailViewer = transform.root.GetComponentInChildren<DebugDetailViewer>(true);

            detailViewer?.Open("Tracking Preview | Real", BuildTrackingPreviewText());
        }

        private void ApplyStaticText()
        {
            titleText.text = "Configs workspace";
            hintText.text = "Use Full Viewer for JSON inspection, or Tracking Preview to see how the current Real config will be formatted into tracking tokens.";
            detailViewer.SetCustomKeysHeaderText("Custom keys");
        }

        private void BindActions()
        {
            openViewerButton.onClick.RemoveAllListeners();
            openViewerButton.onClick.AddListener(OpenConfigsViewer);
            trackingPreviewButton?.onClick.RemoveAllListeners();
            trackingPreviewButton?.onClick.AddListener(OpenTrackingPreview);
            detailViewer.BindConfigActions(
                () => SetViewerMode(ConfigViewerMode.Remote),
                () => SetViewerMode(ConfigViewerMode.Cache),
                () => SetViewerMode(ConfigViewerMode.Real),
                () => SetViewerSection("overview"),
                () => SetViewerSection("ads"),
                () => SetViewerSection("mediation"),
                () => SetViewerSection("custom"));
        }

        private void RefreshSummary()
        {
            string adCoreName = ResolveCurrentAdCoreName();
            int adCoreCount = NetConfigsSO.Ins?.ListAdCoreInfos?.Length ?? 0;
            int customCount = NetConfigsSO.Ins?.ListCustomRemoteConfigs?.Length ?? 0;
            string mode = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.SetupConfigNETType.ToString() : "Unknown";

            string remoteState;
            if (RemoteConfig.Ins == null)
            {
                remoteState = "Remote: instance missing";
            }
            else
            {
                remoteState =
                    $"Remote: firebase={(RemoteConfig.Ins.IsFirebaseInitialized ? "ON" : "OFF")} " +
                    $"fetched={(RemoteConfig.Ins.IsDataFetched ? "ON" : "OFF")} " +
                    $"applied={(RemoteConfig.Ins.IsRefrectedProperties ? "ON" : "OFF")}";
            }

            summaryText.text =
                $"Selected AdCore: {adCoreName}\n" +
                $"Setup mode: {mode} | AdCores: {adCoreCount} | Custom RC keys: {customCount}\n" +
                remoteState + "\n" +
                "Open Full Viewer for JSON, or Tracking Preview to inspect final tracking tokens from current Real config.";
        }

        private string BuildTrackingPreviewText()
        {
            var sb = new StringBuilder(4096);
            var adsLogic = AdsLogic.Ins;
            var core = AdsLogic.AdsCoreIns;
            var adsConfig = AdsLogic.AdsConfigIns;
            string[] forcePositions = GetForceAdPositions(adsConfig);
            string[] popupPositions = GetPopupPositions(adsConfig);

            sb.AppendLine("=== TRACKING PREVIEW | REAL CONFIG ===");
            sb.AppendLine("Preview này đọc từ runtime config đang được apply thật trong game.");
            sb.Append("Selected AdCore: ").AppendLine(adsLogic != null ? (adsLogic.AdCoreName ?? "(empty)") : "(missing)");
            sb.Append("Ads config ready: ").AppendLine(adsConfig != null ? "Yes" : "No");
            sb.Append("AdCore ready: ").AppendLine(core != null ? "Yes" : "No");

            AppendTrackingPreviewTopNotes(sb, forcePositions, popupPositions);

            if (RemoteConfig.Ins != null)
            {
                sb.Append("Real ads len: ").AppendLine((RemoteConfig.Ins.ads_config?.Length ?? 0).ToString());
                sb.Append("Real mediation len: ").AppendLine((RemoteConfig.Ins.mediation_config?.Length ?? 0).ToString());
            }

            if (adsConfig == null || core == null)
            {
                sb.AppendLine();
                sb.AppendLine("Tracking preview chưa sẵn sàng. Hãy chờ RemoteConfig + AdsLogic apply xong rồi mở lại.");
                return sb.ToString();
            }

            AdConfigsValidator.ValidationReport mainAndroidIdReport = TryBuildMainAndroidIdReport(adsConfig, core);

            AppendSingleChannelPreview(sb, "AppLaunch", adsConfig.AppLaunchChannel != null && adsConfig.AppLaunchChannel.IsEnabled, core.AL_TryGetTrackingIdentity,
                mainAndroidIdReport, ResolvePreviewIdQuery(core, "AppLaunch"));
            AppendSingleChannelPreview(sb, "AppResume", adsConfig.AppResumeChannel != null && adsConfig.AppResumeChannel.IsEnabled, core.AR_TryGetTrackingIdentity,
                mainAndroidIdReport, ResolvePreviewIdQuery(core, "AppResume"));
            AppendSingleChannelPreview(sb, "Rewarded", adsConfig.RewardedChannel != null && adsConfig.RewardedChannel.IsEnabled, core.RW_TryGetTrackingIdentity,
                mainAndroidIdReport, new PreviewIdQuery("RW", "Rewarded"));
            AppendSingleChannelPreview(sb, "Banner", adsConfig.BannerChannel != null && adsConfig.BannerChannel.IsEnabled,
                (out GroupAdType adType, out string identitySource) =>
                    core.BN_TryGetTrackingIdentity(BannerPlacement.FullBottom, out adType, out identitySource),
                mainAndroidIdReport, new PreviewIdQuery("BN", "FullBottom"));
            AppendSingleChannelPreview(sb, "Mrec", adsConfig.MrecChannel != null && adsConfig.MrecChannel.IsEnabled, core.Mrec_TryGetTrackingIdentity,
                mainAndroidIdReport, new PreviewIdQuery("MREC", "Mrec"));
            AppendSingleChannelPreview(sb, "Collap", adsConfig.CollapChannel != null && adsConfig.CollapChannel.IsEnabled, core.CL_TryGetTrackingIdentity,
                mainAndroidIdReport, new PreviewIdQuery("CL", "Collap"));

            AppendForceAdPreview(sb, core, forcePositions, mainAndroidIdReport);
            AppendPopupPreview(sb, core, popupPositions, mainAndroidIdReport);
            return sb.ToString();
        }

        private static void AppendTrackingPreviewTopNotes(StringBuilder sb, string[] forcePositions, string[] popupPositions)
        {
            var allPositions = new List<string>();
            if (forcePositions != null && forcePositions.Length > 0)
                allPositions.AddRange(forcePositions);
            if (popupPositions != null && popupPositions.Length > 0)
                allPositions.AddRange(popupPositions);

            List<string> duplicateRawPositions = FindDuplicateRawPositions(allPositions);
            List<string> duplicateTokenNotes = FindDuplicatePosTokens(allPositions);

            if (duplicateRawPositions.Count == 0 && duplicateTokenNotes.Count == 0)
            {
                sb.AppendLine("Status: OK");
                sb.AppendLine("No duplicate pos detected in current Real config.");
                return;
            }

            if (duplicateRawPositions.Count > 0)
            {
                sb.AppendLine("[ERROR] Duplicate pos raw:");
                for (int i = 0; i < duplicateRawPositions.Count; i++)
                    sb.Append(" - ").AppendLine(duplicateRawPositions[i]);
            }

            if (duplicateTokenNotes.Count > 0)
            {
                sb.AppendLine("[ERROR] Duplicate pos token after tracking format:");
                for (int i = 0; i < duplicateTokenNotes.Count; i++)
                    sb.Append(" - ").AppendLine(duplicateTokenNotes[i]);
            }

            sb.AppendLine();
        }

        private static void AppendSingleChannelPreview(
            StringBuilder sb,
            string title,
            bool isEnabled,
            TryResolveNoArgTrackingIdentity resolver,
            AdConfigsValidator.ValidationReport idReport,
            PreviewIdQuery idQuery)
        {
            sb.AppendLine();
            sb.Append('[').Append(title).AppendLine("]");

            if (!isEnabled)
            {
                sb.AppendLine("tracking: (disabled)");
                return;
            }

            if (resolver == null || !resolver(out var adType, out var identitySource) || string.IsNullOrEmpty(identitySource))
            {
                sb.AppendLine("tracking: (missing)");
                return;
            }

            AppendConfiguredIdsPreview(sb, idReport, idQuery, adType);
            AppendIdentityPreview(sb, "", adType, identitySource);
        }

        private static void AppendForceAdPreview(StringBuilder sb, AdCoreBase core, string[] positions, AdConfigsValidator.ValidationReport idReport)
        {
            sb.AppendLine();
            sb.AppendLine("[ForceAd]");

            string[] groups = DistinctNonEmpty(AdsLogic.Ins != null && AdsLogic.Ins.FA_ManagerIns != null
                ? AdsLogic.Ins.FA_ManagerIns.GetTotalGroup()
                : Array.Empty<string>());
            if (groups.Length == 0)
            {
                sb.AppendLine("(empty)");
                return;
            }

            for (int i = 0; i < groups.Length; i++)
            {
                string groupName = groups[i];
                if (!core.FA_TryGetTrackingIdentity(groupName, out var adType, out var identitySource))
                {
                    sb.Append("- ").Append(groupName).AppendLine(" => tracking identity missing");
                    continue;
                }

                sb.AppendLine();
                sb.Append("group raw: ").AppendLine(groupName);
                sb.Append("group token: ").AppendLine(NetTrackingSystem.ConvertTrackingTextToken(groupName));
                AppendConfiguredIdsPreview(sb, idReport, new PreviewIdQuery("FA", groupName), adType);
                AppendIdentityPreview(sb, groupName, adType, identitySource);
                AppendMappedPositions(sb, positions, groupName, core.FA_GroupByPos);
            }
        }

        private static void AppendPopupPreview(StringBuilder sb, AdCoreBase core, string[] positions, AdConfigsValidator.ValidationReport idReport)
        {
            sb.AppendLine();
            sb.AppendLine("[Popup]");

            string[] groups = DistinctNonEmpty(AdsLogic.Ins != null && AdsLogic.Ins.PU_ManagerIns != null
                ? AdsLogic.Ins.PU_ManagerIns.GetTotalGroup()
                : Array.Empty<string>());
            if (groups.Length == 0)
            {
                sb.AppendLine("(empty)");
                return;
            }

            for (int i = 0; i < groups.Length; i++)
            {
                string groupName = groups[i];
                if (!core.PU_TryGetTrackingIdentity(groupName, out var adType, out var identitySource))
                {
                    sb.Append("- ").Append(groupName).AppendLine(" => tracking identity missing");
                    continue;
                }

                sb.AppendLine();
                sb.Append("group raw: ").AppendLine(groupName);
                sb.Append("group token: ").AppendLine(NetTrackingSystem.ConvertTrackingTextToken(groupName));
                AppendConfiguredIdsPreview(sb, idReport, new PreviewIdQuery("PU", groupName), adType);
                AppendIdentityPreview(sb, groupName, adType, identitySource);
                AppendMappedPositions(sb, positions, groupName, core.PU_GroupByPos);
            }
        }

        private static void AppendConfiguredIdsPreview(StringBuilder sb, AdConfigsValidator.ValidationReport idReport, PreviewIdQuery query, GroupAdType adType)
        {
            if (idReport == null || string.IsNullOrEmpty(query.Format) || string.IsNullOrEmpty(query.Name))
                return;

            var entries = new List<AdConfigsValidator.IdEntry>();
            for (int i = 0; i < idReport.Ids.Count; i++)
            {
                var item = idReport.Ids[i];
                if (item == null)
                    continue;

                if (!string.Equals(item.Format, query.Format, StringComparison.Ordinal))
                    continue;

                if (!string.Equals(item.Name, query.Name, StringComparison.Ordinal))
                    continue;

                entries.Add(item);
            }

            if (entries.Count == 0)
                return;

            sb.AppendLine("ids:");
            for (int i = 0; i < entries.Count; i++)
            {
                var item = entries[i];
                sb.Append(" - ");
                if (IsPriorityIdEntry(item))
                    sb.Append("[P] ");

                sb.Append(item.Mediation)
                    .Append(" => ")
                    .Append(NetTrackingSystem.ConvertTrackingIdToken(item.Id, adType, includeAdTypePrefix: true));

                sb.AppendLine();
            }
        }

        private static void AppendIdentityPreview(StringBuilder sb, string groupName, GroupAdType adType, string identitySource)
        {
            sb.AppendLine("resolved tracking:");
            sb.Append("group id: ").AppendLine(NetTrackingSystem.DebugPreviewShortId(adType, identitySource));

            if (!string.IsNullOrEmpty(groupName))
                sb.Append("request flow token: ").AppendLine(NetTrackingSystem.DebugPreviewRequestIdentity(adType, identitySource));
        }

        private static void AppendMappedPositions(StringBuilder sb, string[] positions, string groupName, Func<string, string> resolveGroup)
        {
            var mapped = new List<string>();
            if (positions != null && resolveGroup != null)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    string pos = positions[i];
                    if (string.IsNullOrEmpty(pos))
                        continue;

                    if (string.Equals(resolveGroup(pos), groupName, StringComparison.Ordinal))
                        mapped.Add(pos);
                }
            }

            if (mapped.Count == 0)
            {
                sb.AppendLine("positions: (none)");
                return;
            }

            sb.AppendLine("positions:");
            for (int i = 0; i < mapped.Count; i++)
            {
                string pos = mapped[i];
                sb.Append(" - ").Append(pos)
                    .Append(" => ")
                    .AppendLine(NetTrackingSystem.ConvertTrackingTextToken(pos));
            }
        }

        private static string[] GetForceAdPositions(AdSystemConfigs adsConfig)
        {
            var configs = adsConfig != null ? adsConfig.ForceAdChannel?.PositionConfigs : null;
            if (configs == null || configs.Length == 0)
                return Array.Empty<string>();

            var values = new List<string>(configs.Length);
            for (int i = 0; i < configs.Length; i++)
            {
                var item = configs[i];
                if (item != null && !string.IsNullOrEmpty(item.PositionName))
                    values.Add(item.PositionName);
            }

            return DistinctNonEmpty(values);
        }

        private static string[] GetPopupPositions(AdSystemConfigs adsConfig)
        {
            var configs = adsConfig != null ? adsConfig.PopupChannel?.PositionConfigs : null;
            if (configs == null || configs.Length == 0)
                return Array.Empty<string>();

            var values = new List<string>(configs.Length);
            for (int i = 0; i < configs.Length; i++)
            {
                var item = configs[i];
                if (item != null && !string.IsNullOrEmpty(item.PositionName))
                    values.Add(item.PositionName);
            }

            return DistinctNonEmpty(values);
        }

        private static string[] DistinctNonEmpty(IEnumerable<string> values)
        {
            if (values == null)
                return Array.Empty<string>();

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<string>();
            foreach (var value in values)
            {
                if (string.IsNullOrEmpty(value) || !seen.Add(value))
                    continue;

                result.Add(value);
            }

            result.Sort(StringComparer.Ordinal);
            return result.ToArray();
        }

        private static List<string> FindDuplicateRawPositions(IEnumerable<string> positions)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            if (positions != null)
            {
                foreach (var pos in positions)
                {
                    if (string.IsNullOrEmpty(pos))
                        continue;

                    counts[pos] = counts.TryGetValue(pos, out var count) ? count + 1 : 1;
                }
            }

            var duplicates = new List<string>();
            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                    duplicates.Add($"{pair.Key} (x{pair.Value})");
            }

            duplicates.Sort(StringComparer.Ordinal);
            return duplicates;
        }

        private static List<string> FindDuplicatePosTokens(IEnumerable<string> positions)
        {
            var rawByToken = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            if (positions != null)
            {
                foreach (var pos in positions)
                {
                    if (string.IsNullOrEmpty(pos))
                        continue;

                    string token = NetTrackingSystem.ConvertTrackingTextToken(pos);
                    if (!rawByToken.TryGetValue(token, out var list))
                    {
                        list = new List<string>();
                        rawByToken[token] = list;
                    }

                    if (!list.Contains(pos))
                        list.Add(pos);
                }
            }

            var duplicates = new List<string>();
            foreach (var pair in rawByToken)
            {
                if (pair.Value.Count > 1)
                {
                    pair.Value.Sort(StringComparer.Ordinal);
                    duplicates.Add($"{pair.Key} <= {string.Join(", ", pair.Value)}");
                }
            }

            duplicates.Sort(StringComparer.Ordinal);
            return duplicates;
        }

        private void RefreshViewer()
        {
            if (!detailViewer.ModalRoot.gameObject.activeSelf)
                return;

            string[] keys = GetCustomConfigKeys();
            if (string.IsNullOrEmpty(currentViewerCustomKey) && keys.Length > 0)
                currentViewerCustomKey = keys[0];

            string viewerBody = BuildCurrentViewerBodyText();
            BuildViewerOptions(viewerBody, out bool showOptionKeys, out string optionKeysHeader, out string[] optionIds, out string[] optionLabels, out string selectedOptionKey);

            detailViewer.RenderConfigViewer(
                $"Configs Viewer | {GetViewerSectionTitle()} | {GetViewerModeTitle()}",
                viewerBody,
                showOptionKeys,
                optionKeysHeader,
                optionIds,
                optionLabels,
                GetViewerModeId(currentViewerMode),
                currentViewerSection,
                selectedOptionKey,
                HandleViewerOptionSelected);
        }

        private string BuildOverviewViewerText()
        {
            string netConfigsOverview = cachedLocalOverviewText;
            string realReview = BuildRealConfigReviewText();
            return $"[Configs SO Summary]\n{netConfigsOverview}\n\n[Real Config Review]\n{realReview}";
        }

        private string GetViewerSectionTitle()
        {
            switch (currentViewerSection)
            {
                case "overview": return "Overview";
                case "ads": return "Ads";
                case "mediation": return "Mediation";
                case "custom": return $"Custom ({(string.IsNullOrEmpty(currentViewerCustomKey) ? "-" : currentViewerCustomKey)})";
                default: return "Unknown";
            }
        }

        private string GetViewerModeTitle()
        {
            switch (currentViewerMode)
            {
                case ConfigViewerMode.Remote: return "RC";
                case ConfigViewerMode.Cache: return "Cache";
                case ConfigViewerMode.Real: return "Real";
                default: return currentViewerMode.ToString();
            }
        }

        private string BuildCurrentViewerBodyText()
        {
            string rawText;
            switch (currentViewerSection)
            {
                case "overview":
                    rawText = BuildOverviewViewerText();
                    break;
                case "ads":
                    rawText = GetAdsConfigViewText(currentViewerMode);
                    break;
                case "mediation":
                    rawText = GetMediationConfigViewText(currentViewerMode);
                    break;
                case "custom":
                    rawText = GetCustomConfigViewText(currentViewerMode, currentViewerCustomKey);
                    break;
                default:
                    rawText = "(UNKNOWN SECTION)";
                    break;
            }

            if (!TryGetCurrentCollapsedTopLevelKeys(out var collapsedKeys, out string viewerStateKey))
                return rawText;

            if (!TryParseTopLevelJsonSections(rawText, out var sections) || sections.Count == 0)
                return rawText;

            EnsureDefaultCollapsedState(viewerStateKey, sections, collapsedKeys);
            return BuildCollapsedJsonText(rawText, sections, collapsedKeys);
        }

        private static string GetViewerModeId(ConfigViewerMode mode)
        {
            switch (mode)
            {
                case ConfigViewerMode.Remote: return "remote";
                case ConfigViewerMode.Cache: return "cache";
                case ConfigViewerMode.Real: return "real";
                default: return string.Empty;
            }
        }

        private void SetViewerMode(ConfigViewerMode mode)
        {
            currentViewerMode = mode;
            RefreshViewer();
        }

        private void SetViewerSection(string section)
        {
            currentViewerSection = section;
            RefreshViewer();
        }

        private void HandleCustomKeySelected(string key)
        {
            currentViewerCustomKey = key;
            RefreshViewer();
        }

        private void HandleViewerOptionSelected(string optionKey)
        {
            if (currentViewerSection == "custom")
            {
                HandleCustomKeySelected(optionKey);
                return;
            }

            if (!TryGetCurrentCollapsedTopLevelKeys(out var collapsedKeys, out _))
                return;

            string rawText = currentViewerSection switch
            {
                "ads" => GetAdsConfigViewText(currentViewerMode),
                "mediation" => GetMediationConfigViewText(currentViewerMode),
                _ => string.Empty
            };

            if (!TryParseTopLevelJsonSections(rawText, out var sections) || sections.Count == 0)
                return;

            if (string.Equals(optionKey, ToggleAllViewerOptionKey, StringComparison.Ordinal))
            {
                bool allCollapsed = true;
                for (int i = 0; i < sections.Count; i++)
                {
                    if (!collapsedKeys.Contains(sections[i].Key))
                    {
                        allCollapsed = false;
                        break;
                    }
                }

                collapsedKeys.Clear();
                if (!allCollapsed)
                {
                    for (int i = 0; i < sections.Count; i++)
                        collapsedKeys.Add(sections[i].Key);
                }
            }
            else
            {
                if (!collapsedKeys.Add(optionKey))
                    collapsedKeys.Remove(optionKey);
            }

            RefreshViewer();
        }

        private void BuildViewerOptions(string currentBody, out bool showOptionKeys, out string optionKeysHeader, out string[] optionIds, out string[] optionLabels, out string selectedOptionKey)
        {
            showOptionKeys = false;
            optionKeysHeader = string.Empty;
            optionIds = Array.Empty<string>();
            optionLabels = Array.Empty<string>();
            selectedOptionKey = string.Empty;

            if (currentViewerSection == "custom")
            {
                string[] keys = GetCustomConfigKeys();
                showOptionKeys = keys.Length > 0;
                optionKeysHeader = "Custom keys";
                optionIds = keys;
                optionLabels = keys;
                selectedOptionKey = currentViewerCustomKey;
                return;
            }

            if (!TryGetCurrentCollapsedTopLevelKeys(out var collapsedKeys, out string viewerStateKey))
                return;

            if (!TryParseTopLevelJsonSections(currentBody, out var sections) || sections.Count == 0)
                return;

            EnsureDefaultCollapsedState(viewerStateKey, sections, collapsedKeys);

            showOptionKeys = true;
            optionKeysHeader = "Top level";

            var ids = new List<string>(sections.Count + 1);
            var labels = new List<string>(sections.Count + 1);
            bool allCollapsed = true;
            for (int i = 0; i < sections.Count; i++)
            {
                if (!collapsedKeys.Contains(sections[i].Key))
                {
                    allCollapsed = false;
                    break;
                }
            }

            ids.Add(ToggleAllViewerOptionKey);
            labels.Add(allCollapsed ? "Expand All" : "Collapse All");

            for (int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];
                bool isCollapsed = collapsedKeys.Contains(section.Key);
                ids.Add(section.Key);
                labels.Add($"{(isCollapsed ? "+" : "-")} {section.Key}");
            }

            optionIds = ids.ToArray();
            optionLabels = labels.ToArray();
        }

        private bool TryGetCurrentCollapsedTopLevelKeys(out HashSet<string> collapsedKeys, out string viewerStateKey)
        {
            collapsedKeys = null;
            viewerStateKey = GetCurrentViewerCollapseStateKey();
            if (string.IsNullOrEmpty(viewerStateKey))
                return false;

            if (!collapsedTopLevelKeysByViewer.TryGetValue(viewerStateKey, out collapsedKeys))
            {
                collapsedKeys = new HashSet<string>(StringComparer.Ordinal);
                collapsedTopLevelKeysByViewer[viewerStateKey] = collapsedKeys;
            }

            return true;
        }

        private string GetCurrentViewerCollapseStateKey()
        {
            switch (currentViewerSection)
            {
                case "ads":
                    return $"ads:{GetViewerModeId(currentViewerMode)}";
                case "mediation":
                    return $"mediation:{ResolveCurrentAdCoreKey()}:{GetViewerModeId(currentViewerMode)}";
                default:
                    return string.Empty;
            }
        }

        private void EnsureDefaultCollapsedState(string viewerStateKey, List<TopLevelJsonSection> sections, HashSet<string> collapsedKeys)
        {
            if (string.IsNullOrEmpty(viewerStateKey) || sections == null || sections.Count == 0 || collapsedKeys == null)
                return;

            if (collapseInitializedViewerKeys.Contains(viewerStateKey))
                return;

            collapsedKeys.Clear();
            for (int i = 0; i < sections.Count; i++)
                collapsedKeys.Add(sections[i].Key);

            collapseInitializedViewerKeys.Add(viewerStateKey);
        }

        private void BindReadinessEvents()
        {
            if (readinessEventsBound)
                return;

            RemoteConfig.OnAllJsonsComplete -= HandleRemoteConfigsReady;
            RemoteConfig.OnAllJsonsComplete += HandleRemoteConfigsReady;
            RemoteConfig.OnAdsConfigApplied -= HandleAdsConfigApplied;
            RemoteConfig.OnAdsConfigApplied += HandleAdsConfigApplied;
            RemoteConfig.OnMediationConfigApplied -= HandleMediationConfigApplied;
            RemoteConfig.OnMediationConfigApplied += HandleMediationConfigApplied;
            RemoteConfig.OnCustomConfigsApplied -= HandleCustomConfigsApplied;
            RemoteConfig.OnCustomConfigsApplied += HandleCustomConfigsApplied;

            NetEventSystem.OnAdCoreInitCompleted -= HandleAdCoreReady;
            NetEventSystem.OnAdCoreInitCompleted += HandleAdCoreReady;
            readinessEventsBound = true;
        }

        private void UnbindReadinessEvents()
        {
            if (!readinessEventsBound)
                return;

            RemoteConfig.OnAllJsonsComplete -= HandleRemoteConfigsReady;
            RemoteConfig.OnAdsConfigApplied -= HandleAdsConfigApplied;
            RemoteConfig.OnMediationConfigApplied -= HandleMediationConfigApplied;
            RemoteConfig.OnCustomConfigsApplied -= HandleCustomConfigsApplied;
            NetEventSystem.OnAdCoreInitCompleted -= HandleAdCoreReady;
            readinessEventsBound = false;
        }

        private void HandleRemoteConfigsReady()
        {
            if (!Application.isPlaying)
                return;

            hasReceivedAllJsonsComplete = true;
            Refresh();
        }

        private void HandleAdsConfigApplied()
        {
            if (!Application.isPlaying)
                return;

            Refresh();
        }

        private void HandleMediationConfigApplied()
        {
            if (!Application.isPlaying)
                return;

            Refresh();
        }

        private void HandleCustomConfigsApplied()
        {
            if (!Application.isPlaying)
                return;

            Refresh();
        }

        private void HandleAdCoreReady()
        {
            if (!Application.isPlaying)
                return;

            Refresh();
        }

        private void RefreshConfigCaches()
        {
            BuildAdsPrefsCache();
            BuildAllMediationPrefsCache();
            BuildCustomPrefsCache();
            BuildLocalOverviewCache();
            SyncProgressiveRemoteCaches();
            SyncRemoteCachesIfReady();
        }

        private void SyncProgressiveRemoteCaches()
        {
            if (RemoteConfig.Ins == null)
                return;

            TryBuildAdsCachesProgressive();
            TryBuildMediationCachesProgressive();
        }

        private void SyncRemoteCachesIfReady()
        {
            if (RemoteConfig.Ins == null || !RemoteConfig.Ins.IsDataFetched)
                return;

            hasReceivedAllJsonsComplete = true;

            var rc = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance;
            BuildAdsRemoteAndFinalCache(rc);
            BuildMediationRemoteAndFinalCache(rc);
            BuildCustomRemoteAndFinalCaches(rc);
        }

        private void BuildLocalOverviewCache()
        {
            cachedLocalOverviewText = NetConfigsSO.Ins != null
                ? BuildConfigsSoSummaryText()
                : "(EMPTY) NetConfigsSO missing";
        }

        private string BuildConfigsSoSummaryText()
        {
            var so = NetConfigsSO.Ins;
            if (so == null)
                return "(missing)";

            var sb = new StringBuilder(1024);
            sb.Append("DebugPreset: ").AppendLine(so.Debug_Preset.ToString());
            sb.Append("BuildHack: ").AppendLine(so.Build_Hack.ToString());
            sb.Append("Admob_TestDevice: ").AppendLine(so.Admob_TestDevice.ToString());
            sb.Append("Admob_TestId: ").AppendLine(so.Admob_TestId.ToString());
            sb.Append("Tracking_SendToFirebase: ").AppendLine(so.Tracking_SendToFirebase.ToString());
            sb.Append("DebugUI_KeepLandscape: ").AppendLine(so.DebugUI_KeepLandscape.ToString());
            sb.Append("SetupConfigNETType: ").AppendLine(so.SetupConfigNETType.ToString());
            sb.Append("AdsConfigsDefault len: ").AppendLine((so.AdsConfigsDefault?.Length ?? 0).ToString());
            sb.Append("CustomRemoteConfigs count: ").AppendLine((so.ListCustomRemoteConfigs?.Length ?? 0).ToString());
            sb.Append("AdCoreInfos count: ").AppendLine((so.ListAdCoreInfos?.Length ?? 0).ToString());
            return sb.ToString().TrimEnd();
        }

        private string BuildRealConfigReviewText()
        {
            var sb = new StringBuilder(4096);
            string selectedAdCoreKey = ResolveCurrentAdCoreKey();
            string selectedAdCoreName = ResolveCurrentAdCoreName();
            bool realReady = TryGetRealOverviewState(out var adsReal, out var mediationReal, out var waitingReason);

            sb.Append("Selected AdCore: ").AppendLine(string.IsNullOrEmpty(selectedAdCoreName) ? "(empty)" : selectedAdCoreName);
            sb.Append("AdsLogic.AdsConfigIns: ").AppendLine(AdsLogic.AdsConfigIns != null ? "READY" : "WAIT");
            sb.Append("AdsLogic.AdsCoreIns: ").AppendLine(AdsLogic.AdsCoreIns != null ? "READY" : "WAIT");
            sb.Append("Remote Refrected: ").AppendLine(RemoteConfig.Ins != null && RemoteConfig.Ins.IsRefrectedProperties ? "YES" : "NO");
            sb.Append("Real Ready: ").AppendLine(realReady ? "YES" : "NO");

            if (!realReady)
            {
                sb.Append("Waiting: ").AppendLine(waitingReason);
                return sb.ToString().TrimEnd();
            }

            string adsDefault = JsonTool.FormatJson(NetConfigsSO.Ins != null ? (NetConfigsSO.Ins.AdsConfigsDefault ?? string.Empty) : string.Empty);
            string mediationDefault = GetCurrentDefaultMediationConfigText(selectedAdCoreKey);
            bool adsMatchesDefault = JsonEqualsLoose(adsDefault, adsReal);
            bool mediationMatchesDefault = JsonEqualsLoose(mediationDefault, mediationReal);
            string defaultSelectedAdCore = TryGetSelectedAdCoreNameFromJson(adsDefault);
            string realSelectedAdCore = TryGetSelectedAdCoreNameFromJson(adsReal);

            sb.AppendLine();
            sb.AppendLine("-- Real Sources --");
            sb.Append("Ads real len: ").AppendLine((adsReal?.Length ?? 0).ToString());
            sb.Append("Mediation real len: ").AppendLine((mediationReal?.Length ?? 0).ToString());
            sb.Append("Default ads len: ").AppendLine((adsDefault?.Length ?? 0).ToString());
            sb.Append("Default mediation len: ").AppendLine((mediationDefault?.Length ?? 0).ToString());
            sb.Append("Default selectedAdCoreName: ").AppendLine(string.IsNullOrEmpty(defaultSelectedAdCore) ? "(empty)" : defaultSelectedAdCore);
            sb.Append("Real selectedAdCoreName: ").AppendLine(string.IsNullOrEmpty(realSelectedAdCore) ? "(empty)" : realSelectedAdCore);

            sb.AppendLine();
            sb.AppendLine("-- Compare With Defaults --");
            sb.Append("Ads real == default: ").AppendLine(adsMatchesDefault ? "YES" : "NO");
            sb.Append("Mediation real == default: ").AppendLine(mediationMatchesDefault ? "YES" : "NO");
            sb.Append("Real selectedAdCore == current runtime: ").AppendLine(
                string.Equals(realSelectedAdCore, selectedAdCoreKey, StringComparison.Ordinal) ? "YES" : "NO");

            var errors = new List<string>();
            var warnings = new List<string>();

            if (string.IsNullOrEmpty(adsDefault))
                warnings.Add("Default ads config is empty in Configs SO.");
            if (string.IsNullOrEmpty(mediationDefault))
                warnings.Add($"Default mediation config is empty for selected adcore '{selectedAdCoreKey}'.");
            if (string.IsNullOrEmpty(realSelectedAdCore))
                errors.Add("Real ads config missing selectedAdCoreName.");
            else if (!string.Equals(realSelectedAdCore, selectedAdCoreKey, StringComparison.Ordinal))
                errors.Add($"Real selectedAdCoreName='{realSelectedAdCore}' but runtime current adcore='{selectedAdCoreKey}'.");
            if (!string.IsNullOrEmpty(defaultSelectedAdCore) &&
                !string.Equals(defaultSelectedAdCore, realSelectedAdCore, StringComparison.Ordinal))
                warnings.Add($"Default selectedAdCoreName='{defaultSelectedAdCore}' differs from Real='{realSelectedAdCore}'.");
            if (!adsMatchesDefault)
                warnings.Add("Real ads config differs from Configs SO default.");
            if (!mediationMatchesDefault)
                warnings.Add("Real mediation config differs from Configs SO default for the selected adcore.");

            sb.AppendLine();
            sb.AppendLine("-- Report --");
            if (errors.Count == 0 && warnings.Count == 0)
            {
                sb.AppendLine("Status: OK");
                sb.AppendLine("No overview-level mismatch detected.");
            }
            else
            {
                if (errors.Count > 0)
                {
                    sb.AppendLine("[ERROR]");
                    for (int i = 0; i < errors.Count; i++)
                        sb.Append(" - ").AppendLine(errors[i]);
                }

                if (warnings.Count > 0)
                {
                    sb.AppendLine("[WARNING]");
                    for (int i = 0; i < warnings.Count; i++)
                        sb.Append(" - ").AppendLine(warnings[i]);
                }
            }

            string validationReport = BuildMediationOverviewText();
            if (!string.IsNullOrEmpty(validationReport))
            {
                sb.AppendLine();
                sb.AppendLine("-- AdCore Validation --");
                sb.AppendLine(validationReport.TrimEnd());
            }

            return sb.ToString().TrimEnd();
        }

        private bool TryGetRealOverviewState(out string adsReal, out string mediationReal, out string waitingReason)
        {
            adsReal = string.Empty;
            mediationReal = string.Empty;
            waitingReason = string.Empty;

            if (AdsLogic.AdsConfigIns == null)
            {
                waitingReason = "AdsLogic.AdsConfigIns not ready.";
                return false;
            }

            if (AdsLogic.AdsCoreIns == null)
            {
                waitingReason = "AdsLogic.AdsCoreIns not ready.";
                return false;
            }

            if (RemoteConfig.Ins == null || !RemoteConfig.Ins.IsRefrectedProperties)
            {
                waitingReason = "RemoteConfig has not finished applying real values.";
                return false;
            }

            adsReal = adsFinalCache ?? string.Empty;
            if (string.IsNullOrEmpty(adsReal))
            {
                waitingReason = "Real ads config is empty.";
                return false;
            }

            string currentKey = ResolveCurrentAdCoreKey();
            if (string.IsNullOrEmpty(currentKey))
            {
                waitingReason = "Current adcore key is empty.";
                return false;
            }

            if (!mediationFinalByCore.TryGetValue(currentKey, out mediationReal))
                mediationReal = string.Empty;

            if (string.IsNullOrEmpty(mediationReal))
            {
                waitingReason = $"Real mediation config is empty for core '{currentKey}'.";
                return false;
            }

            return true;
        }

        private static string GetCurrentDefaultMediationConfigText(string adCoreKey)
        {
            if (string.IsNullOrEmpty(adCoreKey) || NetConfigsSO.Ins == null)
                return string.Empty;

            var list = NetConfigsSO.Ins.ListAdCoreInfos;
            if (list == null || list.Length == 0)
                return string.Empty;

            for (int i = 0; i < list.Length; i++)
            {
                var info = list[i];
                if (info?.Core == null)
                    continue;

                if (!string.Equals(info.Core.AdCoreName, adCoreKey, StringComparison.Ordinal))
                    continue;

                return JsonTool.FormatJson(info.DefaultConfigs ?? string.Empty);
            }

            return string.Empty;
        }

        private static string TryGetSelectedAdCoreNameFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            try
            {
                var config = JsonTool.DeserializeObject<AdSystemConfigs>(json);
                return config?.SelectedAdCoreName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool JsonEqualsLoose(string left, string right)
        {
            return string.Equals(
                NormalizeJsonForCompare(left),
                NormalizeJsonForCompare(right),
                StringComparison.Ordinal);
        }

        private static string NormalizeJsonForCompare(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return string.Empty;

            return json.Replace("\r", "").Trim();
        }

        private string[] GetCustomConfigKeys()
        {
            var list = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.ListCustomRemoteConfigs : null;
            if (list == null || list.Length == 0)
                return Array.Empty<string>();

            List<string> output = new List<string>(list.Length);
            for (int i = 0; i < list.Length; i++)
            {
                var item = list[i];
                if (item == null || string.IsNullOrEmpty(item.Key))
                    continue;

                output.Add(item.Key);
            }

            return output.ToArray();
        }

        private string GetAdsConfigViewText(ConfigViewerMode mode)
        {
            switch (mode)
            {
                case ConfigViewerMode.Remote:
                    return !string.IsNullOrEmpty(adsRemoteCache)
                        ? adsRemoteCache
                        : hasAdsRemoteValue || hasReceivedAllJsonsComplete
                            ? "(EMPTY) ADS REMOTE: rc.GetValue('ads_config') is EMPTY (missing key or empty content)"
                            : "(EMPTY) ADS REMOTE: not determined yet";

                case ConfigViewerMode.Cache:
                    return EnsureNotEmpty(
                        adsPrefsSnapshot,
                        "ADS CACHE: PlayerPrefs('ads_config') empty at snapshot time");

                case ConfigViewerMode.Real:
                    return EnsureNotEmpty(
                        adsFinalCache,
                        hasAdsRealValue || hasReceivedAllJsonsComplete
                            ? "ADS REAL: RemoteConfig.Ins.ads_config empty (should be rare; check RefrectProperties fallback path)"
                            : "ADS REAL: not determined yet");

                default:
                    return string.Empty;
            }
        }

        private string GetMediationConfigViewText(ConfigViewerMode mode)
        {
            string coreKey = ResolveCurrentAdCoreKey();

            switch (mode)
            {
                case ConfigViewerMode.Remote:
                    if (string.IsNullOrEmpty(coreKey))
                        return "(EMPTY) MEDIATION REMOTE: AdsLogic.Ins.selectedAdCoreName is null/empty";
                    if (mediationRemoteByCore.TryGetValue(coreKey, out var remote) && !string.IsNullOrEmpty(remote))
                        return remote;
                    return hasMediationRemoteValue || hasReceivedAllJsonsComplete
                        ? $"(EMPTY) MEDIATION REMOTE: rc.GetValue('{coreKey}') is EMPTY (missing key or empty content)"
                        : "(EMPTY) MEDIATION REMOTE: not determined yet";

                case ConfigViewerMode.Cache:
                    if (string.IsNullOrEmpty(coreKey))
                        return "(EMPTY) MEDIATION CACHE: AdsLogic.Ins.selectedAdCoreName is null/empty";
                    if (mediationPrefsSnapshotByCore.TryGetValue(coreKey, out var cache) && !string.IsNullOrEmpty(cache))
                        return cache;
                    return $"(EMPTY) MEDIATION CACHE: PlayerPrefs('{coreKey}') empty at snapshot time (expected if this core never saved before)";

                case ConfigViewerMode.Real:
                    if (string.IsNullOrEmpty(coreKey))
                        return "(EMPTY) MEDIATION REAL: AdsLogic.Ins.selectedAdCoreName is null/empty";
                    if (mediationFinalByCore.TryGetValue(coreKey, out var real) && !string.IsNullOrEmpty(real))
                        return real;
                    return hasMediationRealValue || hasReceivedAllJsonsComplete
                        ? $"(EMPTY) MEDIATION REAL: RemoteConfig.Ins.mediation_config empty for core='{coreKey}' (check RefrectProperties fallback path)"
                        : "(EMPTY) MEDIATION REAL: not determined yet";

                default:
                    return string.Empty;
            }
        }

        private string GetCustomConfigViewText(ConfigViewerMode mode, string key)
        {
            if (string.IsNullOrEmpty(key))
                return "(EMPTY) CUSTOM: key is NULL/EMPTY";

            switch (mode)
            {
                case ConfigViewerMode.Remote:
                    if (customRcRemoteCache.TryGetValue(key, out var remote) && !string.IsNullOrEmpty(remote))
                        return remote;
                    return hasReceivedAllJsonsComplete
                        ? $"(EMPTY) CUSTOM REMOTE: rc.GetValue('{key}') is EMPTY (missing key or empty content)"
                        : "(EMPTY) CUSTOM REMOTE: waiting OnAllJsonsComplete (not received yet)";

                case ConfigViewerMode.Cache:
                    if (customRcPlayerPrefsCache.TryGetValue(key, out var cache) && !string.IsNullOrEmpty(cache))
                        return cache;
                    return $"(EMPTY) CUSTOM CACHE: PlayerPrefs('{key}') empty at snapshot time";

                case ConfigViewerMode.Real:
                    if (customRcFinalCache.TryGetValue(key, out var real) && !string.IsNullOrEmpty(real))
                        return real;
                    return hasReceivedAllJsonsComplete
                        ? $"(EMPTY) CUSTOM REAL: runtime cache empty for key='{key}' (check ApplyCustomRemoteConfigs fallback path)"
                        : "(EMPTY) CUSTOM REAL: waiting OnAllJsonsComplete (not received yet)";

                default:
                    return string.Empty;
            }
        }

        private void BuildAdsPrefsCache()
        {
            string value = PlayerPrefs.GetString(RemoteConfig.adConfigSt, "");
            adsPrefsSnapshot = JsonTool.FormatJson(value);
        }

        private void BuildAllMediationPrefsCache()
        {
            mediationPrefsSnapshotByCore.Clear();

            var list = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.ListAdCoreInfos : null;
            if (list == null || list.Length == 0)
                return;

            for (int i = 0; i < list.Length; i++)
            {
                var info = list[i];
                if (info == null || info.Core == null || string.IsNullOrEmpty(info.Core.AdCoreName))
                    continue;

                string key = info.Core.AdCoreName;
                string value = PlayerPrefs.GetString(key, "");
                mediationPrefsSnapshotByCore[key] = JsonTool.FormatJson(value);
            }
        }

        private void BuildCustomPrefsCache()
        {
            customRcPlayerPrefsCache.Clear();

            var list = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.ListCustomRemoteConfigs : null;
            if (list == null || list.Length == 0)
                return;

            for (int i = 0; i < list.Length; i++)
            {
                var item = list[i];
                if (item == null || string.IsNullOrEmpty(item.Key))
                    continue;

                string soDefault = item.Content ?? string.Empty;
                string cacheValue = PlayerPrefs.GetString(item.Key, soDefault);
                if (string.IsNullOrEmpty(cacheValue))
                    cacheValue = soDefault;

                customRcPlayerPrefsCache[item.Key] = JsonTool.FormatJson(cacheValue);
            }
        }

        private void BuildAdsRemoteAndFinalCache(Firebase.RemoteConfig.FirebaseRemoteConfig rc)
        {
            string remoteValue = rc.GetValue(RemoteConfig.adConfigSt).StringValue;
            adsRemoteCache = JsonTool.FormatJson(remoteValue ?? string.Empty);
            adsFinalCache = JsonTool.FormatJson(RemoteConfig.Ins != null ? (RemoteConfig.Ins.ads_config ?? string.Empty) : string.Empty);
            hasAdsRemoteValue = true;
            hasAdsRealValue = true;
        }

        private void BuildMediationRemoteAndFinalCache(Firebase.RemoteConfig.FirebaseRemoteConfig rc)
        {
            mediationRemoteByCore.Clear();
            mediationFinalByCore.Clear();

            var list = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.ListAdCoreInfos : null;
            if (list != null)
            {
                for (int i = 0; i < list.Length; i++)
                {
                    var info = list[i];
                    if (info == null || info.Core == null || string.IsNullOrEmpty(info.Core.AdCoreName))
                        continue;

                    string key = info.Core.AdCoreName;
                    string remoteValue = rc.GetValue(key).StringValue;
                    mediationRemoteByCore[key] = JsonTool.FormatJson(remoteValue ?? string.Empty);
                }
            }

            string currentKey = ResolveCurrentAdCoreKey();
            string finalValue = RemoteConfig.Ins != null ? (RemoteConfig.Ins.mediation_config ?? string.Empty) : string.Empty;
            if (!string.IsNullOrEmpty(currentKey))
                mediationFinalByCore[currentKey] = JsonTool.FormatJson(finalValue);
            hasMediationRemoteValue = true;
            hasMediationRealValue = true;
        }

        private void TryBuildAdsCachesProgressive()
        {
            string runtimeAds = RemoteConfig.Ins != null ? RemoteConfig.Ins.ads_config : string.Empty;
            if (!string.IsNullOrEmpty(runtimeAds))
            {
                adsFinalCache = JsonTool.FormatJson(runtimeAds);
                hasAdsRealValue = true;
            }

            if (RemoteConfig.Ins == null || !RemoteConfig.Ins.IsFirebaseInitialized)
                return;

            try
            {
                string remoteValue = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteConfig.adConfigSt).StringValue;
                adsRemoteCache = JsonTool.FormatJson(remoteValue ?? string.Empty);
                hasAdsRemoteValue = true;
            }
            catch
            {
            }
        }

        private void TryBuildMediationCachesProgressive()
        {
            string coreKey = ResolveCurrentAdCoreKey();
            if (string.IsNullOrEmpty(coreKey))
                return;

            string runtimeMediation = RemoteConfig.Ins != null ? RemoteConfig.Ins.mediation_config : string.Empty;
            mediationFinalByCore[coreKey] = JsonTool.FormatJson(runtimeMediation ?? string.Empty);
            if (!string.IsNullOrEmpty(runtimeMediation))
                hasMediationRealValue = true;

            if (RemoteConfig.Ins == null || !RemoteConfig.Ins.IsFirebaseInitialized)
                return;

            try
            {
                string remoteValue = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(coreKey).StringValue;
                mediationRemoteByCore[coreKey] = JsonTool.FormatJson(remoteValue ?? string.Empty);
                hasMediationRemoteValue = true;
            }
            catch
            {
            }
        }

        private void BuildCustomRemoteAndFinalCaches(Firebase.RemoteConfig.FirebaseRemoteConfig rc)
        {
            customRcRemoteCache.Clear();
            customRcFinalCache.Clear();

            var list = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.ListCustomRemoteConfigs : null;
            if (list == null || list.Length == 0)
                return;

            for (int i = 0; i < list.Length; i++)
            {
                var item = list[i];
                if (item == null || string.IsNullOrEmpty(item.Key))
                    continue;

                string remoteValue = rc.GetValue(item.Key).StringValue;
                customRcRemoteCache[item.Key] = JsonTool.FormatJson(remoteValue ?? string.Empty);

                string finalValue = RemoteConfig.Ins != null ? (RemoteConfig.Ins.GetCustomRemoteConfigs(item.Key) ?? string.Empty) : string.Empty;
                customRcFinalCache[item.Key] = JsonTool.FormatJson(finalValue);
            }
        }

        private static string ResolveCurrentAdCoreName()
        {
            if (AdsLogic.Ins != null && !string.IsNullOrEmpty(AdsLogic.Ins.AdCoreName))
                return AdsLogic.Ins.AdCoreName;

            var list = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.ListAdCoreInfos : null;
            if (list == null)
                return "(none)";

            for (int i = 0; i < list.Length; i++)
            {
                var info = list[i];
                if (info?.Core == null || string.IsNullOrEmpty(info.Core.AdCoreName))
                    continue;

                return info.Core.AdCoreName;
            }

            return "(none)";
        }

        private static string ResolveCurrentAdCoreKey()
        {
            if (AdsLogic.Ins != null && !string.IsNullOrEmpty(AdsLogic.Ins.AdCoreName))
                return AdsLogic.Ins.AdCoreName;

            var list = NetConfigsSO.Ins != null ? NetConfigsSO.Ins.ListAdCoreInfos : null;
            if (list == null || list.Length == 0)
                return string.Empty;

            for (int i = 0; i < list.Length; i++)
            {
                var info = list[i];
                if (info?.Core != null && !string.IsNullOrEmpty(info.Core.AdCoreName))
                    return info.Core.AdCoreName;
            }

            return string.Empty;
        }

        private static string BuildMediationOverviewText()
        {
            if (AdsLogic.AdsCoreIns == null)
                return "Wait AdCore init done";

            try
            {
                return AdsLogic.AdsCoreIns.DebugRecheckLogic() ?? string.Empty;
            }
            catch (Exception ex)
            {
                return $"[debug read failed] {ex.Message}";
            }
        }

        private static AdConfigsValidator.ValidationReport TryBuildMainAndroidIdReport(AdSystemConfigs adsConfig, AdCoreBase core)
        {
            if (adsConfig == null || core is not AdCore_MainAndroid mainAndroid || mainAndroid.ConfigsIns == null)
                return null;

            try
            {
                return AdConfigsValidator.Validate(adsConfig, mainAndroid.ConfigsIns);
            }
            catch
            {
                return null;
            }
        }

        private static PreviewIdQuery ResolvePreviewIdQuery(AdCoreBase core, string channelTitle)
        {
            if (core is not AdCore_MainAndroid mainAndroid || mainAndroid.ConfigsIns == null)
                return default;

            if (string.Equals(channelTitle, "AppLaunch", StringComparison.Ordinal))
            {
                if (mainAndroid.ConfigsIns.ComebackChannel?.LaunchAdType == E_ComebackAdType.AO)
                    return new PreviewIdQuery("AO", "AppOpen");

                if (mainAndroid.ConfigsIns.ComebackChannel?.LaunchAdType == E_ComebackAdType.FA)
                    return new PreviewIdQuery("FA", mainAndroid.ConfigsIns.ComebackChannel.LaunchForceAdGroupName);
            }

            if (string.Equals(channelTitle, "AppResume", StringComparison.Ordinal))
            {
                if (mainAndroid.ConfigsIns.ComebackChannel?.ResumeAdType == E_ComebackAdType.AO)
                    return new PreviewIdQuery("AO", "AppOpen");

                if (mainAndroid.ConfigsIns.ComebackChannel?.ResumeAdType == E_ComebackAdType.FA)
                    return new PreviewIdQuery("FA", mainAndroid.ConfigsIns.ComebackChannel.ResumeForceAdGroupName);
            }

            return default;
        }

        private static bool IsPriorityIdEntry(AdConfigsValidator.IdEntry item)
        {
            if (item == null || string.IsNullOrEmpty(item.Note) || string.IsNullOrEmpty(item.Mediation))
                return false;

            return item.Note.IndexOf($"priority={item.Mediation}", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string EnsureNotEmpty(string value, string reasonIfEmpty)
        {
            return string.IsNullOrEmpty(value) ? $"(EMPTY) {reasonIfEmpty}" : value;
        }

        private static string BuildCollapsedJsonText(string json, List<TopLevelJsonSection> sections, ISet<string> collapsedKeys)
        {
            if (string.IsNullOrEmpty(json) || sections == null || sections.Count == 0 || collapsedKeys == null || collapsedKeys.Count == 0)
                return json;

            string[] lines = json.Replace("\r", "").Split('\n');
            var sb = new StringBuilder(json.Length);
            int lineIndex = 0;

            for (int sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
            {
                TopLevelJsonSection section = sections[sectionIndex];
                if (lineIndex > section.StartLine)
                    continue;

                while (lineIndex < section.StartLine && lineIndex < lines.Length)
                {
                    AppendLine(sb, lines[lineIndex]);
                    lineIndex++;
                }

                if (lineIndex >= lines.Length)
                    break;

                if (!collapsedKeys.Contains(section.Key))
                    continue;

                string indent = GetLineIndent(lines[section.StartLine]);
                string placeholder = section.ContainerKind == '[' ? "[...]" : "{...}";
                string trailing = section.HasTrailingComma ? "," : string.Empty;
                AppendLine(sb, $"{indent}\"{section.Key}\": {placeholder}{trailing}");
                lineIndex = section.EndLine + 1;
            }

            while (lineIndex < lines.Length)
            {
                AppendLine(sb, lines[lineIndex]);
                lineIndex++;
            }

            return sb.ToString().TrimEnd('\n');
        }

        private static bool TryParseTopLevelJsonSections(string json, out List<TopLevelJsonSection> sections)
        {
            sections = new List<TopLevelJsonSection>();
            if (string.IsNullOrWhiteSpace(json))
                return false;

            string[] lines = json.Replace("\r", "").Split('\n');
            if (lines.Length < 3)
                return false;

            int[] depthBefore = new int[lines.Length];
            int[] depthAfter = new int[lines.Length];
            int depth = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                depthBefore[i] = depth;
                ScanJsonDepth(lines[i], ref depth);
                depthAfter[i] = depth;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                if (depthBefore[i] != 1)
                    continue;

                if (!TryParseTopLevelContainerLine(lines[i], out string key, out char containerKind))
                    continue;

                int endLine = FindTopLevelSectionEndLine(i, depthAfter);
                if (endLine < i)
                    continue;

                bool hasTrailingComma = lines[endLine].TrimEnd().EndsWith(",", StringComparison.Ordinal);
                sections.Add(new TopLevelJsonSection(key, i, endLine, containerKind, hasTrailingComma));
            }

            return sections.Count > 0;
        }

        private static int FindTopLevelSectionEndLine(int startLine, int[] depthAfter)
        {
            for (int i = startLine; i < depthAfter.Length; i++)
            {
                if (depthAfter[i] == 1)
                    return i;
            }

            return -1;
        }

        private static bool TryParseTopLevelContainerLine(string line, out string key, out char containerKind)
        {
            key = string.Empty;
            containerKind = '\0';

            if (string.IsNullOrWhiteSpace(line))
                return false;

            string trimmed = line.TrimStart();
            if (!trimmed.StartsWith("\"", StringComparison.Ordinal))
                return false;

            int keyEnd = FindClosingQuote(trimmed, 1);
            if (keyEnd <= 1)
                return false;

            int colonIndex = FindColonOutsideString(trimmed, keyEnd + 1);
            if (colonIndex < 0)
                return false;

            string valuePart = trimmed.Substring(colonIndex + 1).TrimStart();
            if (valuePart.StartsWith("{", StringComparison.Ordinal))
                containerKind = '{';
            else if (valuePart.StartsWith("[", StringComparison.Ordinal))
                containerKind = '[';
            else
                return false;

            key = trimmed.Substring(1, keyEnd - 1);
            return !string.IsNullOrEmpty(key);
        }

        private static int FindClosingQuote(string text, int startIndex)
        {
            bool escaped = false;
            for (int i = startIndex; i < text.Length; i++)
            {
                char c = text[i];
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (c == '"')
                    return i;
            }

            return -1;
        }

        private static int FindColonOutsideString(string text, int startIndex)
        {
            bool inString = false;
            bool escaped = false;
            for (int i = Math.Max(0, startIndex); i < text.Length; i++)
            {
                char c = text[i];
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (!inString && c == ':')
                    return i;
            }

            return -1;
        }

        private static void ScanJsonDepth(string line, ref int depth)
        {
            bool inString = false;
            bool escaped = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (c == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                    continue;
                }

                if (inString)
                    continue;

                if (c == '{' || c == '[')
                    depth++;
                else if ((c == '}' || c == ']') && depth > 0)
                    depth--;
            }
        }

        private static string GetLineIndent(string line)
        {
            if (string.IsNullOrEmpty(line))
                return string.Empty;

            int count = 0;
            while (count < line.Length && char.IsWhiteSpace(line[count]))
                count++;

            return count == 0 ? string.Empty : line.Substring(0, count);
        }

        private static void AppendLine(StringBuilder sb, string line)
        {
            sb.Append(line);
            sb.Append('\n');
        }

        private readonly struct PreviewIdQuery
        {
            public readonly string Format;
            public readonly string Name;

            public PreviewIdQuery(string format, string name)
            {
                Format = format ?? string.Empty;
                Name = name ?? string.Empty;
            }
        }

        private readonly struct TopLevelJsonSection
        {
            public readonly string Key;
            public readonly int StartLine;
            public readonly int EndLine;
            public readonly char ContainerKind;
            public readonly bool HasTrailingComma;

            public TopLevelJsonSection(string key, int startLine, int endLine, char containerKind, bool hasTrailingComma)
            {
                Key = key ?? string.Empty;
                StartLine = startLine;
                EndLine = endLine;
                ContainerKind = containerKind;
                HasTrailingComma = hasTrailingComma;
            }
        }

        private delegate bool TryResolveNoArgTrackingIdentity(out GroupAdType adType, out string identitySource);
    }
}
