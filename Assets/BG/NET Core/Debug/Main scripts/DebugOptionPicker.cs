using System;
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
    public sealed class DebugOptionPicker : MonoBehaviour
    {
        private const string SpawnedRootName = "Debug Option Picker UI";
        private static readonly Color NormalButtonColor = new Color(0.31f, 0.31f, 0.31f, 1f);
        private static readonly Color SelectedButtonColor = new Color(0.86f, 0.53f, 0.08f, 1f);
        private static readonly Color DisabledButtonColor = new Color(0.24f, 0.24f, 0.24f, 1f);

        [SerializeField] private RectTransform modalRoot;
        [SerializeField] private RectTransform cardRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private ScrollRect optionsScrollRect;
        [SerializeField] private RectTransform optionsRoot;
        [SerializeField] private Button optionTemplateButton;

        private readonly List<Button> optionButtons = new List<Button>(8);
        private Action<string> onOptionSelected;

        private void Awake()
        {
            BindButton(closeButton, Close);
        }

        public void Open(string title, IReadOnlyList<string> options, string selectedOption, Action<string> onSelected, string subtitle = null)
        {
            onOptionSelected = onSelected;

            if (titleText != null)
                titleText.text = string.IsNullOrEmpty(title) ? "Select Option" : title;

            if (subtitleText != null)
                subtitleText.text = string.IsNullOrEmpty(subtitle) ? "Choose one option below." : subtitle;

            RebuildOptions(options, selectedOption);

            if (modalRoot != null)
            {
                modalRoot.gameObject.SetActive(true);
                modalRoot.SetAsLastSibling();
            }
        }

        public void Close()
        {
            if (modalRoot != null)
                modalRoot.gameObject.SetActive(false);
        }

        private void RebuildOptions(IReadOnlyList<string> options, string selectedOption)
        {
            CacheOptionButtons();

            int optionCount = options != null ? options.Count : 0;
            EnsureOptionButtonCount(Mathf.Max(1, optionCount));

            for (int i = 0; i < optionButtons.Count; i++)
            {
                var button = optionButtons[i];
                if (button == null)
                    continue;

                if (optionCount == 0)
                {
                    bool active = i == 0;
                    button.gameObject.SetActive(active);
                    if (active)
                    {
                        SetButtonLabel(button, "No option available");
                        SetButtonColor(button, DisabledButtonColor);
                        button.onClick.RemoveAllListeners();
                    }

                    continue;
                }

                if (i >= optionCount)
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                string option = options[i];
                button.gameObject.SetActive(true);
                SetButtonLabel(button, option);
                SetButtonColor(button, string.Equals(option, selectedOption, StringComparison.Ordinal) ? SelectedButtonColor : NormalButtonColor);

                button.onClick.RemoveAllListeners();
                string capturedOption = option;
                button.onClick.AddListener(() =>
                {
                    Close();
                    onOptionSelected?.Invoke(capturedOption);
                });
            }

            if (optionsScrollRect != null)
                optionsScrollRect.verticalNormalizedPosition = 1f;
        }

        private void CacheOptionButtons()
        {
            optionButtons.Clear();
            if (optionsRoot == null)
                return;

            for (int i = 0; i < optionsRoot.childCount; i++)
            {
                var button = optionsRoot.GetChild(i).GetComponent<Button>();
                if (button != null)
                    optionButtons.Add(button);
            }
        }

        private void EnsureOptionButtonCount(int targetCount)
        {
            if (optionTemplateButton == null || optionsRoot == null)
                return;

            while (optionButtons.Count < targetCount)
            {
                var cloneObject = Instantiate(optionTemplateButton.gameObject, optionsRoot, false);
                cloneObject.name = $"Picker Option {optionButtons.Count + 1:00}";
                var cloneButton = cloneObject.GetComponent<Button>();
                optionButtons.Add(cloneButton);
            }
        }

        private static void BindButton(Button button, Action action)
        {
            if (button == null)
                return;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private static void SetButtonLabel(Button button, string value)
        {
            if (button == null)
                return;

            var label = button.GetComponentInChildren<Text>(true);
            if (label != null)
                label.text = value ?? string.Empty;
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
