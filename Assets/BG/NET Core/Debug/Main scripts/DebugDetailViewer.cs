using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
#endif

namespace BG_Library.DEBUG
{
    [DisallowMultipleComponent]
    public sealed class DebugDetailViewer : MonoBehaviour
    {
        private const string SpawnedRootName = "Debug Detail Viewer UI";
        private static readonly Color SelectedButtonColor = new Color(0.86f, 0.53f, 0.08f, 1f);
        private static readonly Color NormalButtonColor = new Color(0.31f, 0.31f, 0.31f, 1f);

        [SerializeField] private RectTransform modalRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;

        [Header("Optional config viewer chrome")]
        [SerializeField] private RectTransform modeRow;
        [SerializeField] private Button remoteModeButton;
        [SerializeField] private Button cacheModeButton;
        [SerializeField] private Button realModeButton;
        [SerializeField] private RectTransform sectionRow;
        [SerializeField] private Button overviewButton;
        [SerializeField] private Button adsButton;
        [SerializeField] private Button mediationButton;
        [SerializeField] private Button customButton;
        [SerializeField] private Text customKeysTitleText;
        [SerializeField] private RectTransform customKeysRoot;

        [Header("Optional tracking viewer chrome")]
        [SerializeField] private RectTransform trackingAreaRoot;
        [SerializeField] private Toggle trackingDescriptionToggle;
        [SerializeField] private Button trackingAllTrackingButton;
        [SerializeField] private Button trackingFilterPosButton;
        [SerializeField] private Button trackingFilterChannelButton;
        [SerializeField] private RectTransform trackingFilterDetailRow;
        [SerializeField] private Button trackingFilterPrimaryButton;
        [SerializeField] private Button trackingFilterSecondaryButton;
        [SerializeField] private Button trackingFilterClearButton;
        [SerializeField] private Button trackingFilterCopyButton;

        private readonly List<Button> customKeySlots = new ();
        private readonly Dictionary<string, Button> modeButtons = new (3);
        private readonly Dictionary<string, Button> sectionButtons = new (4);
        private readonly Dictionary<string, Button> customKeyButtons = new (8);
        private Func<bool, string> trackingBodyBuilder;
        private Action<bool> trackingDescriptionChanged;
        private Action trackingViewerClosed;
        private string trackingViewerTitle = "Viewer";
        private bool trackingUiRefreshing;
        private Coroutine trackingCopyFeedbackRoutine;

        public RectTransform ModalRoot => modalRoot;

        private void Awake()
        {
            BindButton(closeButton, Close);
            BindTrackingToggle();
        }

        public bool IsTrackingViewerOpen => trackingBodyBuilder != null && trackingAreaRoot != null && trackingAreaRoot.gameObject.activeSelf;

        public void Open(string title, string body)
        {
            NotifyTrackingViewerClosed();
            ClearTrackingViewerState();
            SetConfigChromeVisible(false);
            SetTrackingChromeVisible(false);
            SetViewerText(title, body);
            ShowModal();
        }

        public void OpenTrackingViewer(string title, Func<bool, string> bodyBuilder, bool showVietnameseDescription, Action<bool> onDescriptionChanged = null, Action onClosed = null)
        {
            trackingViewerTitle = string.IsNullOrEmpty(title) ? "Tracking Viewer" : title;
            trackingBodyBuilder = bodyBuilder;
            trackingDescriptionChanged = onDescriptionChanged;
            trackingViewerClosed = onClosed;

            SetConfigChromeVisible(false);
            SetTrackingChromeVisible(true);
            SetTrackingDescriptionLabel("Show Explanation");
            SetTrackingDescriptionToggle(showVietnameseDescription);
            RefreshTrackingViewerBody();
            ShowModal();
        }

        public void Close()
        {
            NotifyTrackingViewerClosed();
            ClearTrackingViewerState();
            SetTrackingChromeVisible(false);
            modalRoot.gameObject.SetActive(false);
        }

        public void ShowModal()
        {
            modalRoot.gameObject.SetActive(true);
        }

        public void ShowConfigViewer()
        {
            NotifyTrackingViewerClosed();
            ClearTrackingViewerState();
            SetTrackingChromeVisible(false);
            SetConfigChromeVisible(true);
            ShowModal();
        }

        public void SetViewerText(string title, string body)
        {
            titleText.text = string.IsNullOrEmpty(title) ? "Viewer" : title;
            bodyText.text = string.IsNullOrEmpty(body) ? "(EMPTY)" : body;
        }

        public void SetCustomKeysHeaderText(string value)
        {
            customKeysTitleText.text = value;
        }

        public void BindConfigActions(
            Action onRemote,
            Action onCache,
            Action onReal,
            Action onOverview,
            Action onAds,
            Action onMediation,
            Action onCustom)
        {
            BindButton(remoteModeButton, onRemote);
            BindButton(cacheModeButton, onCache);
            BindButton(realModeButton, onReal);
            BindButton(overviewButton, onOverview);
            BindButton(adsButton, onAds);
            BindButton(mediationButton, onMediation);
            BindButton(customButton, onCustom);
            CacheChromeButtons();
        }

        public void RenderConfigViewer(
            string title,
            string body,
            bool showOptionKeys,
            string optionKeysHeader,
            IReadOnlyList<string> optionIds,
            IReadOnlyList<string> optionLabels,
            string selectedModeId,
            string selectedSectionId,
            string selectedOptionKey,
            Action<string> onOptionSelected)
        {
            ClearTrackingViewerState();
            SetTrackingChromeVisible(false);
            SetConfigChromeVisible(true);
            SetViewerText(title, body);
            SetCustomKeysHeaderText(optionKeysHeader);
            SetActive(customKeysTitleText, showOptionKeys);
            SetActive(customKeysRoot, showOptionKeys);
            CacheChromeButtons();
            CacheCustomKeySlots();
            RebuildCustomKeyButtons(optionIds, optionLabels, onOptionSelected);
            UpdateConfigHighlights(selectedModeId, selectedSectionId, selectedOptionKey);
        }

        public void SetConfigChromeVisible(bool visible)
        {
            SetActive(modeRow, visible);
            SetActive(sectionRow, visible);
            SetActive(customKeysTitleText, visible);
            SetActive(customKeysRoot, visible);
        }

        public void SetTrackingChromeVisible(bool visible)
        {
            SetActive(trackingAreaRoot, visible);
        }

        public void ConfigureTrackingFilters(
            string selectedModeId,
            string allModeLabel,
            string posModeLabel,
            string channelModeLabel,
            string primaryLabel,
            string secondaryLabel,
            bool detailRowVisible,
            bool secondaryVisible,
            Action onAllPressed,
            Action onPosPressed,
            Action onChannelPressed,
            Action onPrimaryPressed,
            Action onSecondaryPressed,
            Action onClearPressed,
            Action onCopyPressed)
        {
            SetButtonLabel(trackingAllTrackingButton, allModeLabel);
            SetButtonLabel(trackingFilterPosButton, posModeLabel);
            SetButtonLabel(trackingFilterChannelButton, channelModeLabel);
            SetButtonLabel(trackingFilterPrimaryButton, primaryLabel);
            SetButtonLabel(trackingFilterSecondaryButton, secondaryLabel);
            SetButtonLabel(trackingFilterCopyButton, "Copy");
            SetActive(trackingFilterDetailRow, detailRowVisible);
            SetActive(trackingFilterSecondaryButton, secondaryVisible);
            SetActive(trackingFilterCopyButton, onCopyPressed != null);

            BindButton(trackingAllTrackingButton, onAllPressed);
            BindButton(trackingFilterPosButton, onPosPressed);
            BindButton(trackingFilterChannelButton, onChannelPressed);
            BindButton(trackingFilterPrimaryButton, onPrimaryPressed);
            BindButton(trackingFilterSecondaryButton, onSecondaryPressed);
            BindButton(trackingFilterClearButton, onClearPressed);
            BindButton(trackingFilterCopyButton, onCopyPressed);
            UpdateTrackingFilterModeHighlights(selectedModeId);
        }

        public void ShowTrackingCopyFeedback()
        {
            if (trackingFilterCopyButton == null)
                return;

            if (trackingCopyFeedbackRoutine != null)
                StopCoroutine(trackingCopyFeedbackRoutine);

            trackingCopyFeedbackRoutine = StartCoroutine(CoShowTrackingCopyFeedback());
        }

        public void RefreshTrackingViewer()
        {
            RefreshTrackingViewerBody();
        }

        private static void SetActive(Component component, bool active)
        {
            if (component == null)
                return;

            component.gameObject.SetActive(active);
        }

        private void ClearTrackingViewerState()
        {
            trackingBodyBuilder = null;
            trackingDescriptionChanged = null;
            trackingViewerClosed = null;
            trackingViewerTitle = "Viewer";
            if (trackingCopyFeedbackRoutine != null)
            {
                StopCoroutine(trackingCopyFeedbackRoutine);
                trackingCopyFeedbackRoutine = null;
            }
            SetButtonLabel(trackingFilterCopyButton, "Copy");
        }

        private void NotifyTrackingViewerClosed()
        {
            if (trackingViewerClosed == null)
                return;

            var callback = trackingViewerClosed;
            trackingViewerClosed = null;
            callback.Invoke();
        }

        private void BindTrackingToggle()
        {
            if (trackingDescriptionToggle == null)
                return;

            SetTrackingDescriptionLabel("Show Explanation");
            trackingDescriptionToggle.onValueChanged.RemoveAllListeners();
            trackingDescriptionToggle.onValueChanged.AddListener(OnTrackingDescriptionToggleChanged);
        }

        private void OnTrackingDescriptionToggleChanged(bool value)
        {
            if (trackingUiRefreshing)
                return;

            trackingDescriptionChanged?.Invoke(value);
            RefreshTrackingViewerBody();
        }

        private void SetTrackingDescriptionToggle(bool value)
        {
            if (trackingDescriptionToggle == null)
                return;

            trackingUiRefreshing = true;
            trackingDescriptionToggle.SetIsOnWithoutNotify(value);
            trackingUiRefreshing = false;
        }

        private IEnumerator CoShowTrackingCopyFeedback()
        {
            SetButtonLabel(trackingFilterCopyButton, "Copied to clipboard");
            yield return new WaitForSecondsRealtime(1.5f);
            SetButtonLabel(trackingFilterCopyButton, "Copy");
            trackingCopyFeedbackRoutine = null;
        }

        private void RefreshTrackingViewerBody()
        {
            if (trackingBodyBuilder == null)
                return;

            bool showVietnameseDescription = trackingDescriptionToggle != null && trackingDescriptionToggle.isOn;
            SetViewerText(trackingViewerTitle, trackingBodyBuilder(showVietnameseDescription));
        }

        private void CacheChromeButtons()
        {
            modeButtons.Clear();
            sectionButtons.Clear();

            RegisterButton(modeButtons, "remote", remoteModeButton);
            RegisterButton(modeButtons, "cache", cacheModeButton);
            RegisterButton(modeButtons, "real", realModeButton);

            RegisterButton(sectionButtons, "overview", overviewButton);
            RegisterButton(sectionButtons, "ads", adsButton);
            RegisterButton(sectionButtons, "mediation", mediationButton);
            RegisterButton(sectionButtons, "custom", customButton);
        }

        private void CacheCustomKeySlots()
        {
            customKeySlots.Clear();

            for (int i = 0; i < customKeysRoot.childCount; i++)
            {
                var button = customKeysRoot.GetChild(i).GetComponent<Button>();
                customKeySlots.Add(button);
            }
        }

        private void RebuildCustomKeyButtons(IReadOnlyList<string> optionIds, IReadOnlyList<string> optionLabels, Action<string> onOptionSelected)
        {
            customKeyButtons.Clear();
            int count = optionIds != null ? optionIds.Count : 0;
            EnsureCustomKeySlotCount(count);

            for (int i = 0; i < customKeySlots.Count; i++)
            {
                var button = customKeySlots[i];
                if (i >= count)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                string key = optionIds[i];
                string labelTextValue = optionLabels != null && i < optionLabels.Count ? optionLabels[i] : key;
                button.gameObject.SetActive(true);

                var label = button.GetComponentInChildren<Text>(true);
                label.text = labelTextValue;

                button.onClick.RemoveAllListeners();
                string capturedKey = key;
                button.onClick.AddListener(() => onOptionSelected?.Invoke(capturedKey));
                customKeyButtons[key] = button;
            }
        }

        private void EnsureCustomKeySlotCount(int targetCount)
        {
            if (targetCount <= customKeySlots.Count || customKeySlots.Count == 0)
                return;

            var template = customKeySlots[0];
            for (int i = customKeySlots.Count; i < targetCount; i++)
            {
                var cloneObject = Instantiate(template.gameObject, customKeysRoot, false);
                cloneObject.name = $"{template.name} ({i + 1})";
                var cloneButton = cloneObject.GetComponent<Button>();
                customKeySlots.Add(cloneButton);
            }
        }

        private void UpdateConfigHighlights(string selectedModeId, string selectedSectionId, string selectedCustomKey)
        {
            UpdateButtonColors(modeButtons, selectedModeId);
            UpdateButtonColors(sectionButtons, selectedSectionId);
            UpdateButtonColors(customKeyButtons, selectedCustomKey);
        }

        private static void UpdateButtonColors(Dictionary<string, Button> buttons, string selectedKey)
        {
            foreach (var item in buttons)
            {
                var image = item.Value.GetComponent<Image>();
                image.color = item.Key == selectedKey ? SelectedButtonColor : NormalButtonColor;
            }
        }

        private static void RegisterButton(Dictionary<string, Button> cache, string key, Button button)
        {
            cache[key] = button;
        }

        private static void BindButton(Button button, Action action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
                return;

            var labelText = button.GetComponentInChildren<Text>(true);
            if (labelText != null)
                labelText.text = string.IsNullOrEmpty(label) ? "-" : label;
        }

        private void SetTrackingDescriptionLabel(string label)
        {
            if (trackingDescriptionToggle == null || trackingDescriptionToggle.transform.parent == null)
                return;

            var labelText = trackingDescriptionToggle.transform.parent.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
                labelText.text = label;
        }

        private void UpdateTrackingFilterModeHighlights(string selectedModeId)
        {
            SetButtonColor(trackingAllTrackingButton, selectedModeId == "all" ? SelectedButtonColor : NormalButtonColor);
            SetButtonColor(trackingFilterPosButton, selectedModeId == "request" ? SelectedButtonColor : NormalButtonColor);
            SetButtonColor(trackingFilterChannelButton, selectedModeId == "action" ? SelectedButtonColor : NormalButtonColor);
        }

        private static void SetButtonColor(Button button, Color color)
        {
            if (button == null)
                return;

            var image = button.GetComponent<Image>();
            if (image != null)
                image.color = color;
        }
    }
}
