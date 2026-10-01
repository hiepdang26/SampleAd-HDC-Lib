#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using BG_Library.DEBUG;

namespace BG_Library.NET.EditorTools
{
    public static class SetupTrackingSystemSection
    {
        private const string BuiltinFontName = "LegacyRuntime.ttf";

        [MenuItem("BG/Tools/Debug UI/Setup Tracking System Section")]
        public static void Execute()
        {
            var trackingSection = FindByPath("Debug Overlay Canvas/Canvas/Safe area/Flow + Tracking panel/Scroll Root/Viewport/Container/Tracking Section") as RectTransform;
            if (trackingSection == null)
            {
                UnityEngine.Debug.LogError("SetupTrackingSystemSection: Tracking Section not found.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(trackingSection.gameObject, "Setup Tracking System Section");
            ClearChildren(trackingSection);

            var component = trackingSection.GetComponent<TrackingSystemSection>();
            if (component == null)
                component = Undo.AddComponent<TrackingSystemSection>(trackingSection.gameObject);

            var title = CreateText(trackingSection, "Tracking Title", "Tracking system", 22, FontStyle.Bold);
            var hint = CreateText(trackingSection, "Tracking Hint", "Use this section to manage NetTrackingSystem config, history filters, and the tracking rule guide.", 18, FontStyle.Normal);
            var summary = CreateText(trackingSection, "Tracking Summary", "Waiting for runtime state...", 18, FontStyle.Normal);
            SetPreferredHeight(summary.gameObject, 82f);

            var configBox = CreateBox(trackingSection, "Tracking Config Subsection", "Tracking config", "Core switches for event logging, revenue logging, detail parameters, and Firebase routing.");
            var trackingEventToggle = CreateToggleRow(configBox, "Tracking Event Toggle", "Log tracking events");
            var trackingRevenueToggle = CreateToggleRow(configBox, "Tracking Revenue Toggle", "Log revenue and impression");
            var trackingDetailsToggle = CreateToggleRow(configBox, "Tracking Details Toggle", "Show revenue parameter details");
            var trackingFirebaseToggle = CreateToggleRow(configBox, "Tracking Firebase Toggle", "Send tracking to Firebase while debug is ON");

            var toolsBox = CreateBox(trackingSection, "Tracking Tools Subsection", "Tracking tools", "Filter history quickly, then open overview windows or clear captured tracking history.");
            var filterRow = CreateButtonRow(toolsBox, "Tracking Filter Row");
            var filterChannelButton = CreateButton(filterRow, "Tracking Filter Channel", "Channel: All");
            var filterActionButton = CreateButton(filterRow, "Tracking Filter Action", "Action: All");
            var filterResetButton = CreateButton(filterRow, "Tracking Filter Reset", "Reset Filter");

            var overviewRow = CreateButtonRow(toolsBox, "Tracking Overview Row");
            var overviewSequentialButton = CreateButton(overviewRow, "Tracking Sequential", "Overview Sequential");
            var overviewCountButton = CreateButton(overviewRow, "Tracking Count", "Overview Count");
            var clearHistoryButton = CreateButton(overviewRow, "Tracking Clear", "Clear History");

            var legendBox = CreateBox(trackingSection, "Tracking Legend Subsection", "Tracking rule legend", "Read tracking names directly from the event string without needing callsite context.");
            var legendText = CreateText(legendBox, "Tracking Rule Legend", "", 18, FontStyle.Normal);

            var so = new SerializedObject(component);
            Assign(so, "sectionRoot", trackingSection);
            Assign(so, "titleText", title);
            Assign(so, "hintText", hint);
            Assign(so, "summaryText", summary);
            Assign(so, "trackingEventToggle", trackingEventToggle);
            Assign(so, "trackingRevenueToggle", trackingRevenueToggle);
            Assign(so, "trackingDetailsToggle", trackingDetailsToggle);
            Assign(so, "trackingFirebaseToggle", trackingFirebaseToggle);
            Assign(so, "filterChannelButton", filterChannelButton);
            Assign(so, "filterActionButton", filterActionButton);
            Assign(so, "filterResetButton", filterResetButton);
            Assign(so, "overviewSequentialButton", overviewSequentialButton);
            Assign(so, "overviewCountButton", overviewCountButton);
            Assign(so, "clearHistoryButton", clearHistoryButton);
            Assign(so, "ruleLegendText", legendText);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(component);
            EditorUtility.SetDirty(trackingSection.gameObject);
            EditorSceneManager.MarkSceneDirty(trackingSection.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("TrackingSystemSection setup completed.");
        }

        private static void Assign(SerializedObject so, string propertyName, Object value)
        {
            var property = so.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }

        private static Transform FindByPath(string path)
        {
            var roots = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < roots.Length; i++)
            {
                var transform = roots[i];
                if (transform == null)
                    continue;

                var found = FindByPath(transform, path);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static Transform FindByPath(Transform current, string targetPath)
        {
            if (current == null)
                return null;

            var currentPath = BuildPath(current);
            if (currentPath == targetPath || currentPath.EndsWith("/" + targetPath))
                return current;

            for (int i = 0; i < current.childCount; i++)
            {
                var child = current.GetChild(i);
                var found = FindByPath(child, targetPath);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static string BuildPath(Transform current)
        {
            if (current == null)
                return string.Empty;

            var stack = new System.Collections.Generic.Stack<string>();
            var cursor = current;
            while (cursor != null)
            {
                stack.Push(cursor.name);
                cursor = cursor.parent;
            }

            return string.Join("/", stack.ToArray());
        }

        private static void ClearChildren(RectTransform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(root.GetChild(i).gameObject);
            }
        }

        private static RectTransform CreateBox(RectTransform parent, string name, string title, string hint)
        {
            var box = CreateRect(parent, name);
            EnsureImage(box.gameObject, new Color(0.16f, 0.16f, 0.18f, 0.95f));
            EnsureVerticalLayout(box.gameObject, 8f, new RectOffset(14, 14, 12, 12));
            EnsureContentSizeFitter(box.gameObject);
            SetPreferredHeight(box.gameObject, -1f);

            CreateText(box, $"{name} Title", title, 22, FontStyle.Bold);
            CreateText(box, $"{name} Hint", hint, 18, FontStyle.Normal);
            return box;
        }

        private static RectTransform CreateButtonRow(RectTransform parent, string name)
        {
            var row = CreateRect(parent, name);
            var layout = GetOrAdd<HorizontalLayoutGroup>(row.gameObject);
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            var element = GetOrAdd<LayoutElement>(row.gameObject);
            element.preferredHeight = 64f;
            return row;
        }

        private static Button CreateButton(RectTransform parent, string name, string labelText)
        {
            var buttonRoot = CreateRect(parent, name);
            EnsureImage(buttonRoot.gameObject, new Color(0.27f, 0.27f, 0.29f, 1f));
            var button = GetOrAdd<Button>(buttonRoot.gameObject);
            var element = GetOrAdd<LayoutElement>(buttonRoot.gameObject);
            element.preferredHeight = 64f;
            element.flexibleWidth = 1f;

            var label = CreateText(buttonRoot, "Label", labelText, 24, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            Stretch(label.rectTransform, 8f, 8f, 6f, 6f);
            return button;
        }

        private static Toggle CreateToggleRow(RectTransform parent, string name, string labelText)
        {
            var row = CreateRect(parent, name);
            var rowLayout = GetOrAdd<HorizontalLayoutGroup>(row.gameObject);
            rowLayout.spacing = 12f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childForceExpandWidth = false;

            var rowElement = GetOrAdd<LayoutElement>(row.gameObject);
            rowElement.preferredHeight = 46f;

            var toggleRoot = CreateRect(row, "Switch");
            var toggleElement = GetOrAdd<LayoutElement>(toggleRoot.gameObject);
            toggleElement.preferredWidth = 42f;
            toggleElement.preferredHeight = 42f;
            toggleElement.flexibleWidth = 0f;

            var background = CreateRect(toggleRoot, "Background");
            var backgroundImage = EnsureImage(background.gameObject, new Color32(0x33, 0x33, 0x33, 0xFF));
            background.anchorMin = new Vector2(0.5f, 0.5f);
            background.anchorMax = new Vector2(0.5f, 0.5f);
            background.pivot = new Vector2(0.5f, 0.5f);
            background.sizeDelta = new Vector2(36f, 36f);
            background.anchoredPosition = Vector2.zero;

            var checkmark = CreateRect(background, "Checkmark");
            var checkmarkImage = EnsureImage(checkmark.gameObject, new Color(1f, 0.73f, 0.08f, 1f));
            checkmark.anchorMin = new Vector2(0.5f, 0.5f);
            checkmark.anchorMax = new Vector2(0.5f, 0.5f);
            checkmark.pivot = new Vector2(0.5f, 0.5f);
            checkmark.sizeDelta = new Vector2(18f, 18f);
            checkmark.anchoredPosition = Vector2.zero;

            var toggle = GetOrAdd<Toggle>(toggleRoot.gameObject);
            toggle.targetGraphic = backgroundImage;
            toggle.graphic = checkmarkImage;

            var label = CreateText(row, "Label", labelText, 20, FontStyle.Normal);
            label.alignment = TextAnchor.MiddleLeft;
            var labelElement = GetOrAdd<LayoutElement>(label.gameObject);
            labelElement.flexibleWidth = 1f;
            return toggle;
        }

        private static Text CreateText(RectTransform parent, string name, string content, int size, FontStyle style)
        {
            var rect = CreateRect(parent, name);
            var text = GetOrAdd<Text>(rect.gameObject);
            text.font = Resources.GetBuiltinResource<Font>(BuiltinFontName);
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.text = content;
            return text;
        }

        private static RectTransform CreateRect(RectTransform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 0f);
            return rect;
        }

        private static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetPreferredHeight(GameObject target, float height)
        {
            var element = GetOrAdd<LayoutElement>(target);
            if (height > 0f)
                element.preferredHeight = height;
        }

        private static Image EnsureImage(GameObject target, Color color)
        {
            var image = GetOrAdd<Image>(target);
            image.color = color;
            return image;
        }

        private static void EnsureVerticalLayout(GameObject target, float spacing, RectOffset padding)
        {
            var layout = GetOrAdd<VerticalLayoutGroup>(target);
            layout.spacing = spacing;
            layout.padding = padding;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        private static void EnsureContentSizeFitter(GameObject target)
        {
            var fitter = GetOrAdd<ContentSizeFitter>(target);
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var existing = target.GetComponent<T>();
            if (existing != null)
                return existing;

            return Undo.AddComponent<T>(target);
        }
    }
}
#endif
