using System;
using System.Collections.Generic;
using BG_Library.NET;
using BG_Library.NET.API;
using BG_Library.NET.AdSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BG_Library.DEBUG
{
    [DisallowMultipleComponent]
    public sealed class AdSystemWorkspaceSection : MonoBehaviour
    {
        [SerializeField] private RectTransform sectionRoot;
        [SerializeField] private Text hintText;
        [SerializeField] private Text summaryText;
        [SerializeField] private Button refreshWorkspaceButton;

        [Header("Channel buttons")]
        [SerializeField] private Button openAlButton;
        [SerializeField] private Button openArButton;
        [SerializeField] private Button openRwButton;
        [SerializeField] private Button openFaButton;
        [SerializeField] private Button openBnButton;
        [SerializeField] private Button openMrecButton;
        [SerializeField] private Button openClButton;
        [SerializeField] private Button openPuButton;

        [Header("Detail")]
        [SerializeField] private Text detailTitleText;
        [SerializeField] private Text detailSelectionText;
        [SerializeField] private Button selectGroupButton;
        [SerializeField] private Button selectPositionButton;
        [SerializeField] private Button refreshDetailButton;
        [SerializeField] private Button initButton;
        [SerializeField] private Button showButton;
        [SerializeField] private Button hideButton;
        [SerializeField] private Button utilityPrimaryButton;
        [SerializeField] private Button utilitySecondaryButton;
        [SerializeField] private Text detailText;

        [Header("Picker")]
        [SerializeField] private DebugOptionPicker optionPicker;
        [SerializeField] private DebugDetailViewer detailViewer;

        [Header("Popup Preview")]
        [SerializeField] private PULayout puLayout;

        private sealed class WorkspaceInfo
        {
            public string Key { get; set; }
            public string Title { get; set; }
            public bool HasPanel { get; set; }
            public bool HasLogic { get; set; }
            public string[] PublicApiNames { get; set; }
            public string[] UtilityApiNames { get; set; }
            public bool CanIgnore { get; set; }
            public bool CanSelectGroup { get; set; }
            public bool CanSelectPosition { get; set; }
            public bool CanInit { get; set; }
            public bool CanShow { get; set; }
            public bool CanHide { get; set; }
            public bool CanUpdatePosition { get; set; }
            public bool CanRefreshGroup { get; set; }

            public string ToActionSummary()
            {
                if (PublicApiNames != null && PublicApiNames.Length > 0)
                {
                    string summary = string.Join(", ", PublicApiNames);
                    if (UtilityApiNames != null && UtilityApiNames.Length > 0)
                        summary += $" | Utility: {string.Join(", ", UtilityApiNames)}";

                    return summary;
                }

                var actions = new List<string>(7);
                if (CanIgnore) actions.Add("ignore");
                if (CanSelectGroup) actions.Add("select group");
                if (CanSelectPosition) actions.Add("select pos");
                if (CanInit) actions.Add("init");
                if (CanShow) actions.Add("show/activate");
                if (CanHide) actions.Add("hide");
                if (CanUpdatePosition) actions.Add("update pos");
                if (CanRefreshGroup) actions.Add("refresh group");
                return actions.Count == 0 ? "-" : string.Join(", ", actions);
            }
        }

        private readonly Dictionary<string, Button> channelButtons = new Dictionary<string, Button>(8);

        private AppLaunchSystem alLogic;
        private AppResumeSystem arLogic;
        private RewardedSystem rwLogic;
        private BannerSystem bnLogic;
        private MrecSystem mrecLogic;
        private CollapSystem clLogic;
        private ForceAdSystem faLogic;
        private PopUpSystem puLogic;
        private bool readinessEventsBound;
        private static readonly string[] MrecPositionOptions =
        {
            "TopLeft",
            "Top",
            "TopRight",
            "Center",
            "BottomLeft",
            "Bottom",
            "BottomRight",
        };
        private static readonly string[] BannerPlacementOptions =
        {
            "FullBottom",
            "FullTop",
            "TopLeft",
            "TopRight",
            "BottomLeft",
            "BottomRight",
        };

        private string selectedAdSystemKey = "FA";
        private string selectedAdSystemGroup = "";
        private string selectedAdSystemPosition = "";

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

        private void OnDisable()
        {
            UpdatePuLayoutVisibility();
        }

        private void OnDestroy()
        {
            UpdatePuLayoutVisibility();
            UnbindReadinessEvents();
        }

        public void Refresh()
        {
            ResolveLogicInstances();
            ApplyStaticText();
            RefreshSummary();
            RefreshDetail();
        }

        private void ApplyStaticText()
        {
            if (hintText != null)
                hintText.text = "Select a channel below, then use this workspace to call public NetCallerAPI methods directly.";
        }

        private void BindActions()
        {
            channelButtons.Clear();
            BindChannelButton(openAlButton, "AL");
            BindChannelButton(openArButton, "AR");
            BindChannelButton(openRwButton, "RW");
            BindChannelButton(openFaButton, "FA");
            BindChannelButton(openBnButton, "BN");
            BindChannelButton(openMrecButton, "MREC");
            BindChannelButton(openClButton, "CL");
            BindChannelButton(openPuButton, "PU");

            BindButton(refreshWorkspaceButton, Refresh);
            BindButton(selectGroupButton, () => OpenPicker(true));
            BindButton(selectPositionButton, () => OpenPicker(false));
            BindButton(refreshDetailButton, HandleRefreshDetail);
            BindButton(initButton, InvokeInit);
            BindButton(showButton, InvokeShow);
            BindButton(hideButton, InvokeHide);
            BindButton(utilityPrimaryButton, InvokePrimaryUtility);
            BindButton(utilitySecondaryButton, InvokeSecondaryUtility);
        }

        private void BindChannelButton(Button button, string key)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                selectedAdSystemKey = key;
                NormalizeContext();
                RefreshDetail();
            });
            channelButtons[key] = button;
        }

        private static void BindButton(Button button, Action action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void RefreshSummary()
        {
            var infos = GetWorkspaceInfos();

            var lines = new string[infos.Length];
            for (int i = 0; i < infos.Length; i++)
                lines[i] = BuildSummaryLine(infos[i]);

            summaryText.text = string.Join("\n", lines);
        }

        private void RefreshDetail()
        {
            NormalizeContext();
            UpdateChannelHighlights();
            UpdateHeader();
            UpdateActionButtons();
            UpdateDetailText();
            UpdatePuLayoutVisibility();
        }

        private void HandleRefreshDetail()
        {
            if (selectedAdSystemKey == "FA")
            {
                detailViewer.Open("ForceAd | BreakAd Debug", SafeInvoke(() => faLogic?.GetDebugInfoBreakAd()));
                return;
            }

            RefreshDetail();
        }

        private void NormalizeContext()
        {
            if (string.IsNullOrEmpty(selectedAdSystemKey))
                selectedAdSystemKey = "FA";

            var groups = GetSelectedGroups();
            if (groups.Length == 0)
            {
                selectedAdSystemGroup = string.Empty;
            }
            else if (string.IsNullOrEmpty(selectedAdSystemGroup) || Array.IndexOf(groups, selectedAdSystemGroup) < 0)
            {
                selectedAdSystemGroup = groups[0];
            }

            var positions = GetSelectedPositions();
            if (positions.Length == 0)
            {
                selectedAdSystemPosition = string.Empty;
            }
            else if (string.IsNullOrEmpty(selectedAdSystemPosition) || Array.IndexOf(positions, selectedAdSystemPosition) < 0)
            {
                selectedAdSystemPosition = positions[0];
            }
        }

        private void UpdateChannelHighlights()
        {
            foreach (var item in channelButtons)
            {
                if (item.Value == null)
                    continue;

                var image = item.Value.GetComponent<Image>();
                if (image == null)
                    continue;

                image.color = item.Key == selectedAdSystemKey
                    ? new Color(0.86f, 0.53f, 0.08f, 1f)
                    : new Color(0.31f, 0.31f, 0.31f, 1f);
            }
        }

        private void UpdateHeader()
        {
            if (detailTitleText != null)
                detailTitleText.text = $"Selected channel: {selectedAdSystemKey}";

            if (detailSelectionText == null)
                return;

            var info = FindWorkspaceInfo(selectedAdSystemKey);
            var groups = GetSelectedGroups();
            string title = info != null ? info.Title : selectedAdSystemKey;
            string positionLabel = selectedAdSystemKey == "BN" ? "Placement" : "Position";
            string positionText = string.IsNullOrEmpty(selectedAdSystemPosition) ? "-" : selectedAdSystemPosition;
            string selectionLine = groups.Length > 0
                ? $"Group: {selectedAdSystemGroup} | {positionLabel}: {positionText}"
                : $"{positionLabel}: {positionText}";

            detailSelectionText.text =
                $"Channel: {selectedAdSystemKey} | {title}\n" +
                $"{selectionLine}\n" +
                "The actions below call public NetCallerAPI methods directly.";
        }

        private void UpdateActionButtons()
        {
            var groups = GetSelectedGroups();
            var positions = GetSelectedPositions();

            SetButtonVisible(selectGroupButton, groups.Length > 0);
            SetButtonLabel(selectGroupButton, groups.Length > 0 ? $"Group: {selectedAdSystemGroup}" : "Group: -");

            string positionPrefix = selectedAdSystemKey == "BN" ? "Placement" : "Position";
            SetButtonVisible(selectPositionButton, positions.Length > 0);
            SetButtonLabel(selectPositionButton, positions.Length > 0 ? $"{positionPrefix}: {selectedAdSystemPosition}" : $"{positionPrefix}: -");

            SetButtonVisible(refreshDetailButton, true);
            SetButtonLabel(refreshDetailButton, selectedAdSystemKey == "FA" ? "BreakAd Debug" : "Refresh Detail");

            bool canInit = selectedAdSystemKey is "AL" or "AR" or "RW" or "BN" or "MREC" or "CL" or "FA" or "PU";
            bool canShow = selectedAdSystemKey is "RW" or "BN" or "MREC" or "CL" or "FA" or "PU";
            bool canHide = selectedAdSystemKey is "BN" or "MREC" or "CL" or "PU";

            SetButtonVisible(initButton, canInit);
            SetButtonVisible(showButton, canShow);
            SetButtonVisible(hideButton, canHide);
            SetButtonLabel(initButton, "Init");
            SetButtonLabel(showButton, selectedAdSystemKey is "BN" or "MREC" ? "Activate" : "Show");
            SetButtonLabel(hideButton, "Hide");

            string utilityPrimary = "";
            string utilitySecondary = "";
            switch (selectedAdSystemKey)
            {
                case "FA":
                    utilityPrimary = "Start BreakAd";
                    utilitySecondary = "Stop BreakAd";
                    break;
                case "MREC":
                    utilityPrimary = "UpdatePos";
                    utilitySecondary = "GetSize";
                    break;
                case "PU":
                    utilityPrimary = "UpdatePos";
                    break;
            }

            SetButtonVisible(utilityPrimaryButton, !string.IsNullOrEmpty(utilityPrimary));
            SetButtonVisible(utilitySecondaryButton, !string.IsNullOrEmpty(utilitySecondary));
            SetButtonLabel(utilityPrimaryButton, utilityPrimary);
            SetButtonLabel(utilitySecondaryButton, utilitySecondary);
        }

        private void UpdateDetailText()
        {
            if (detailText != null)
                detailText.text = BuildDetailText();
        }

        private string BuildDetailText()
        {
            if ((selectedAdSystemKey == "FA" || selectedAdSystemKey == "PU") && AdsLogic.AdsCoreIns == null)
            {
                return "Waiting for adcore initialization...\n\nThe selected ad system detail will be available after InitMediation completes.";
            }

            switch (selectedAdSystemKey)
            {
                case "AL":
                    return CombineDebugSections("System", SafeInvoke(() => alLogic?.GetDebugInfo()), "Group", SafeInvoke(() => alLogic?.GetDebugGroup()));
                case "AR":
                    return CombineDebugSections("System", SafeInvoke(() => arLogic?.GetDebugInfo()), "Group", SafeInvoke(() => arLogic?.GetDebugGroup()));
                case "RW":
                    return CombineDebugSections("System", SafeInvoke(() => rwLogic?.GetDebugInfo()), "Group", SafeInvoke(() => rwLogic?.GetDebugGroup()));
                case "BN":
                    return CombineDebugSections(
                        "System", SafeInvoke(() => bnLogic?.GetDebugInfo()),
                        "Selected Placement", SafeInvoke(() => bnLogic?.GetDebugGroup(ParseBannerPlacement(selectedAdSystemPosition))));
                case "MREC":
                    return CombineDebugSections("System", SafeInvoke(() => mrecLogic?.GetDebugInfo()), "Group", SafeInvoke(() => mrecLogic?.GetDebugGroup()));
                case "CL":
                    return CombineDebugSections("System", SafeInvoke(() => clLogic?.GetDebugInfo()), "Group", SafeInvoke(() => clLogic?.GetDebugGroup()));
                case "FA":
                    return CombineDebugSections(
                        "System", SafeInvoke(() => faLogic?.GetDebugInfo()),
                        "Selected Group", string.IsNullOrEmpty(selectedAdSystemGroup) ? "(no group selected)" : SafeInvoke(() => faLogic?.GetDebugInfoGroup(selectedAdSystemGroup)));
                case "PU":
                    return CombineDebugSections(
                        "System", SafeInvoke(() => puLogic?.GetDebugInfo()),
                        "Selected Group", string.IsNullOrEmpty(selectedAdSystemGroup) ? "(no group selected)" : SafeInvoke(() => puLogic?.GetDebugInfoGroup(selectedAdSystemGroup)));
                default:
                    return "No channel selected.";
            }
        }

        private void OpenPicker(bool showGroupPicker)
        {
            if (optionPicker == null)
                return;

            bool isGroupPicker = showGroupPicker;
            string[] options = isGroupPicker ? GetSelectedGroups() : GetSelectedPositions();
            string selectedValue = isGroupPicker ? selectedAdSystemGroup : selectedAdSystemPosition;
            string pickerLabel = selectedAdSystemKey == "BN" ? "Placement" : "Position";
            string title = isGroupPicker ? $"Select Group - {selectedAdSystemKey}" : $"Select {pickerLabel} - {selectedAdSystemKey}";
            string subtitle = isGroupPicker
                ? "Choose a group to filter the current channel."
                : selectedAdSystemKey == "BN"
                    ? "Choose a banner placement to continue."
                    : "Choose a position to continue.";

            optionPicker.Open(title, options, selectedValue, option =>
            {
                if (isGroupPicker)
                {
                    selectedAdSystemGroup = option;
                    selectedAdSystemPosition = string.Empty;
                    NormalizeContext();
                }
                else
                {
                    selectedAdSystemPosition = option;
                }

                RefreshDetail();
            }, subtitle);
        }

        private void InvokeInit()
        {
            switch (selectedAdSystemKey)
            {
                case "AL": BG_Library.NET.API.NetCallerAPI.AL_InitManually(); break;
                case "AR": BG_Library.NET.API.NetCallerAPI.AR_InitManually(); break;
                case "RW": BG_Library.NET.API.NetCallerAPI.RW_InitManually(); break;
                case "BN": BG_Library.NET.API.NetCallerAPI.BN_InitManually(ParseNetCallerBannerPlacement(selectedAdSystemPosition)); break;
                case "MREC": BG_Library.NET.API.NetCallerAPI.Mrec_InitManually(); break;
                case "CL": BG_Library.NET.API.NetCallerAPI.CL_InitManually(); break;
                case "FA":
                    if (!string.IsNullOrEmpty(selectedAdSystemGroup))
                        BG_Library.NET.API.NetCallerAPI.FA_InitManually(selectedAdSystemGroup);
                    break;
                case "PU":
                    if (!string.IsNullOrEmpty(selectedAdSystemGroup))
                        BG_Library.NET.API.NetCallerAPI.PU_InitManually(selectedAdSystemGroup);
                    break;
            }

            RefreshDetail();
        }

        private void InvokeShow()
        {
            switch (selectedAdSystemKey)
            {
                case "RW":
                    BG_Library.NET.API.NetCallerAPI.RW_Show(string.IsNullOrEmpty(selectedAdSystemPosition) ? "debug_rw" : selectedAdSystemPosition, null);
                    break;
                case "BN":
                    BG_Library.NET.API.NetCallerAPI.BN_ActivateView(ParseNetCallerBannerPlacement(selectedAdSystemPosition));
                    break;
                case "MREC":
                    BG_Library.NET.API.NetCallerAPI.Mrec_ActivateView();
                    break;
                case "CL":
                    BG_Library.NET.API.NetCallerAPI.CL_Show();
                    break;
                case "FA":
                    if (!string.IsNullOrEmpty(selectedAdSystemPosition))
                        BG_Library.NET.API.NetCallerAPI.FA_Show(selectedAdSystemPosition, null);
                    break;
                case "PU":
                    if (!string.IsNullOrEmpty(selectedAdSystemPosition))
                        BG_Library.NET.API.NetCallerAPI.PU_Show(selectedAdSystemPosition);
                    break;
            }

            RefreshDetail();
        }

        private void InvokeHide()
        {
            switch (selectedAdSystemKey)
            {
                case "BN": BG_Library.NET.API.NetCallerAPI.BN_Hide(ParseNetCallerBannerPlacement(selectedAdSystemPosition)); break;
                case "MREC": BG_Library.NET.API.NetCallerAPI.Mrec_Hide(); break;
                case "CL": BG_Library.NET.API.NetCallerAPI.CL_Hide(); break;
                case "PU":
                    if (!string.IsNullOrEmpty(selectedAdSystemPosition))
                        BG_Library.NET.API.NetCallerAPI.PU_Hide(selectedAdSystemPosition);
                    break;
            }

            RefreshDetail();
        }

        private void InvokePrimaryUtility()
        {
            switch (selectedAdSystemKey)
            {
                case "FA":
                    BG_Library.NET.API.NetCallerAPI.FA_StartBreakAd();
                    break;
                case "MREC":
                    if (TryParseMrecPosition(selectedAdSystemPosition, out var mrecPos))
                        BG_Library.NET.API.NetCallerAPI.Mrec_UpdatePos(mrecPos);
                    break;
                case "PU":
                    if (!string.IsNullOrEmpty(selectedAdSystemPosition) && puLayout != null)
                        BG_Library.NET.API.NetCallerAPI.PU_UpdatePos(selectedAdSystemPosition, puLayout);
                    break;
            }

            RefreshDetail();
        }

        private void InvokeSecondaryUtility()
        {
            switch (selectedAdSystemKey)
            {
                case "FA":
                    BG_Library.NET.API.NetCallerAPI.FA_StopBreakAd();
                    break;
                case "MREC":
                    if (detailText != null)
                        detailText.text = BuildDetailText() + $"\n\n--- Mrec Size ---\n{BG_Library.NET.API.NetCallerAPI.Mrec_GetSize}";
                    return;
            }

            RefreshDetail();
        }

        private string[] GetSelectedGroups()
        {
            switch (selectedAdSystemKey)
            {
                case "FA": return DistinctNonEmpty(faLogic?.GetTotalGroup());
                case "PU": return DistinctNonEmpty(puLogic?.GetTotalGroup());
                default: return Array.Empty<string>();
            }
        }

        private string[] GetSelectedPositions()
        {
            switch (selectedAdSystemKey)
            {
                case "FA":
                    return FilterPositionsByGroup(GetForceAdPositions(), selectedAdSystemGroup, pos => AdsLogic.AdsCoreIns != null ? AdsLogic.AdsCoreIns.FA_GroupByPos(pos) : "");
                case "PU":
                    return FilterPositionsByGroup(GetPopupPositions(), selectedAdSystemGroup, pos => AdsLogic.AdsCoreIns != null ? AdsLogic.AdsCoreIns.PU_GroupByPos(pos) : "");
                case "RW":
                    return new[] { "debug_rw" };
                case "BN":
                    return BannerPlacementOptions;
                case "MREC":
                    return MrecPositionOptions;
                default:
                    return Array.Empty<string>();
            }
        }

        private static BG_Library.NET.BannerPlacement ParseBannerPlacement(string value)
        {
            if (Enum.TryParse(value, out BG_Library.NET.BannerPlacement placement))
                return placement;

            return BG_Library.NET.BannerPlacement.FullBottom;
        }

        private static NetCallerBannerPlacement ParseNetCallerBannerPlacement(string value)
        {
            if (Enum.TryParse(value, out NetCallerBannerPlacement placement))
                return placement;

            return NetCallerBannerPlacement.FullBottom;
        }

        private static bool TryParseMrecPosition(string value, out int pos)
        {
            pos = 6;

            if (string.IsNullOrEmpty(value))
                return false;

            switch (value)
            {
                case "TopLeft":
                    pos = 0;
                    return true;
                case "Top":
                    pos = 1;
                    return true;
                case "TopRight":
                    pos = 2;
                    return true;
                case "Center":
                    pos = 3;
                    return true;
                case "BottomLeft":
                    pos = 4;
                    return true;
                case "Bottom":
                    pos = 5;
                    return true;
                case "BottomRight":
                    pos = 6;
                    return true;
                default:
                    return int.TryParse(value, out pos);
            }
        }

        private string[] GetForceAdPositions()
        {
            var configs = AdsLogic.AdsConfigIns?.ForceAdChannel?.PositionConfigs;
            if (configs == null || configs.Length == 0)
                return Array.Empty<string>();

            var values = new List<string>(configs.Length);
            for (int i = 0; i < configs.Length; i++)
            {
                if (configs[i] != null && !string.IsNullOrEmpty(configs[i].PositionName))
                    values.Add(configs[i].PositionName);
            }

            return DistinctNonEmpty(values);
        }

        private string[] GetPopupPositions()
        {
            var configs = AdsLogic.AdsConfigIns?.PopupChannel?.PositionConfigs;
            if (configs == null || configs.Length == 0)
                return Array.Empty<string>();

            var values = new List<string>(configs.Length);
            for (int i = 0; i < configs.Length; i++)
            {
                if (configs[i] != null && !string.IsNullOrEmpty(configs[i].PositionName))
                    values.Add(configs[i].PositionName);
            }

            return DistinctNonEmpty(values);
        }

        private static string[] FilterPositionsByGroup(string[] positions, string selectedGroup, Func<string, string> resolver)
        {
            if (positions == null || positions.Length == 0)
                return Array.Empty<string>();

            if (string.IsNullOrEmpty(selectedGroup))
                return DistinctNonEmpty(positions);

            var values = new List<string>(positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                if (string.IsNullOrEmpty(position))
                    continue;

                if (string.Equals(resolver != null ? resolver(position) : "", selectedGroup, StringComparison.Ordinal))
                    values.Add(position);
            }

            return DistinctNonEmpty(values);
        }

        private WorkspaceInfo FindWorkspaceInfo(string key)
        {
            var infos = GetWorkspaceInfos();

            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i] != null && string.Equals(infos[i].Key, key, StringComparison.Ordinal))
                    return infos[i];
            }

            return null;
        }

        private string BuildSummaryLine(WorkspaceInfo info)
        {
            if (info == null)
                return "- missing info -";

            return $"{info.Key} | {info.Title} | panel {(info.HasPanel ? "ON" : "missing")} | logic {(info.HasLogic ? "ON" : "missing")} | API: {info.ToActionSummary()}";
        }

        private WorkspaceInfo[] GetWorkspaceInfos()
        {
            return new[]
            {
                BuildSingleWorkspaceInfo("AL", "AppLaunch", alLogic, canIgnore: true, canInit: true, canShow: false, canHide: false, canUpdatePos: false, canRefreshGroup: true,
                    null,
                    "AL_InitManually()"),
                BuildSingleWorkspaceInfo("AR", "AppResume", arLogic, canIgnore: true, canInit: true, canShow: false, canHide: false, canUpdatePos: false, canRefreshGroup: true,
                    null,
                    "AR_InitManually()"),
                BuildSingleWorkspaceInfo("RW", "Rewarded", rwLogic, canIgnore: true, canInit: true, canShow: true, canHide: false, canUpdatePos: false, canRefreshGroup: true,
                    null,
                    "RW_InitManually()", "RW_Show(pos)"),
                BuildSingleWorkspaceInfo("BN", "Banner", bnLogic, canIgnore: false, canInit: true, canShow: true, canHide: true, canUpdatePos: false, canRefreshGroup: true,
                    null,
                    "BN_InitManually(placement)", "BN_ActivateView(placement)", "BN_Hide(placement)"),
                BuildSingleWorkspaceInfo("MREC", "Mrec", mrecLogic, canIgnore: false, canInit: true, canShow: true, canHide: true, canUpdatePos: true, canRefreshGroup: true,
                    new[] { "Mrec_UpdatePos(pos)", "Mrec_UpdatePos(target)", "Mrec_GetSize" },
                    "Mrec_InitManually()", "Mrec_ActivateView()", "Mrec_Hide()"),
                BuildSingleWorkspaceInfo("CL", "Collap", clLogic, canIgnore: false, canInit: true, canShow: true, canHide: true, canUpdatePos: false, canRefreshGroup: true,
                    null,
                    "CL_InitManually()", "CL_Show()", "CL_Hide()"),
                BuildMultiWorkspaceInfo("FA", "ForceAd", faLogic, canIgnore: true, canHide: false, canUpdatePos: false,
                    new[] { "FA_StartBreakAd()", "FA_StopBreakAd()" },
                    "FA_InitManually(group)", "FA_Show(pos)"),
                BuildMultiWorkspaceInfo("PU", "Popup", puLogic, canIgnore: false, canHide: true, canUpdatePos: true,
                    new[] { "PU_UpdatePos(pos, layout)" },
                    "PU_InitManually(group)", "PU_Show(pos)", "PU_Hide(pos)"),
            };
        }

        private void ResolveLogicInstances()
        {
            alLogic ??= FindFirstObjectByType<AppLaunchSystem>();
            arLogic ??= FindFirstObjectByType<AppResumeSystem>();
            rwLogic ??= FindFirstObjectByType<RewardedSystem>();
            bnLogic ??= FindFirstObjectByType<BannerSystem>();
            mrecLogic ??= FindFirstObjectByType<MrecSystem>();
            clLogic ??= FindFirstObjectByType<CollapSystem>();
            faLogic ??= FindFirstObjectByType<ForceAdSystem>();
            puLogic ??= FindFirstObjectByType<PopUpSystem>();
        }

        private void UpdatePuLayoutVisibility()
        {
            if (puLayout == null)
                return;

            bool shouldShow = false;

            if (isActiveAndEnabled)
            {
                bool rootActive = sectionRoot != null
                    ? sectionRoot.gameObject.activeInHierarchy
                    : gameObject.activeInHierarchy;

                shouldShow = rootActive && string.Equals(selectedAdSystemKey, "PU", StringComparison.Ordinal);
            }

            if (puLayout.gameObject.activeSelf != shouldShow)
                puLayout.gameObject.SetActive(shouldShow);
        }

        private void BindReadinessEvents()
        {
            if (readinessEventsBound)
                return;

            NetEventSystem.OnAdCoreInitCompleted -= HandleAdCoreReady;
            NetEventSystem.OnAdCoreInitCompleted += HandleAdCoreReady;
            readinessEventsBound = true;
        }

        private void UnbindReadinessEvents()
        {
            if (!readinessEventsBound)
                return;

            NetEventSystem.OnAdCoreInitCompleted -= HandleAdCoreReady;
            readinessEventsBound = false;
        }

        private void HandleAdCoreReady()
        {
            if (!Application.isPlaying)
                return;

            Refresh();
        }

        private static WorkspaceInfo BuildSingleWorkspaceInfo(
            string key,
            string title,
            UnityEngine.Object logic,
            bool canIgnore,
            bool canInit,
            bool canShow,
            bool canHide,
            bool canUpdatePos,
            bool canRefreshGroup,
            string[] utilityApiNames,
            params string[] publicApiNames)
        {
            return new WorkspaceInfo
            {
                Key = key,
                Title = title,
                HasPanel = true,
                HasLogic = logic != null,
                PublicApiNames = publicApiNames,
                UtilityApiNames = utilityApiNames,
                CanIgnore = canIgnore,
                CanSelectGroup = false,
                CanSelectPosition = false,
                CanInit = canInit,
                CanShow = canShow,
                CanHide = canHide,
                CanUpdatePosition = canUpdatePos,
                CanRefreshGroup = canRefreshGroup,
            };
        }

        private static WorkspaceInfo BuildMultiWorkspaceInfo(
            string key,
            string title,
            UnityEngine.Object logic,
            bool canIgnore,
            bool canHide,
            bool canUpdatePos,
            string[] utilityApiNames,
            params string[] publicApiNames)
        {
            return new WorkspaceInfo
            {
                Key = key,
                Title = title,
                HasPanel = true,
                HasLogic = logic != null,
                PublicApiNames = publicApiNames,
                UtilityApiNames = utilityApiNames,
                CanIgnore = canIgnore,
                CanSelectGroup = true,
                CanSelectPosition = true,
                CanInit = true,
                CanShow = true,
                CanHide = canHide,
                CanUpdatePosition = canUpdatePos,
                CanRefreshGroup = true,
            };
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null)
                button.gameObject.SetActive(visible);
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
                return;

            var text = button.GetComponentInChildren<Text>(true);
            if (text != null)
                text.text = label ?? string.Empty;
        }

        private static string SafeInvoke(Func<string> getter)
        {
            try
            {
                return getter != null ? getter.Invoke() ?? string.Empty : string.Empty;
            }
            catch (Exception ex)
            {
                return $"[debug read failed] {ex.Message}";
            }
        }

        private static string CombineDebugSections(params string[] values)
        {
            var lines = new List<string>();
            for (int i = 0; i + 1 < values.Length; i += 2)
            {
                if (lines.Count > 0)
                    lines.Add(string.Empty);

                lines.Add($"--- {values[i]} ---");
                lines.Add(string.IsNullOrEmpty(values[i + 1]) ? "(empty)" : values[i + 1]);
            }

            return string.Join("\n", lines);
        }

        private static string[] DistinctNonEmpty(IEnumerable<string> values)
        {
            if (values == null)
                return Array.Empty<string>();

            var results = new List<string>();
            foreach (var value in values)
            {
                if (string.IsNullOrEmpty(value) || results.Contains(value))
                    continue;

                results.Add(value);
            }

            return results.ToArray();
        }
    }
}
