#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using BG_Library.DEBUG;

namespace BG_Library.NET.EditorTools
{
    public static class SetupDiagnosticsSystemSection
    {
        private const string BuiltinFontName = "LegacyRuntime.ttf";

        [MenuItem("BG/Tools/Debug UI/Setup Diagnostics System Section")]
        public static void Execute()
        {
            var flowSection = FindByPath("Debug Overlay Canvas/Canvas/Safe area/Flow + Tracking panel/Scroll Root/Viewport/Container/Flow Section") as RectTransform;
            if (flowSection == null)
            {
                UnityEngine.Debug.LogError("SetupDiagnosticsSystemSection: Flow Section not found.");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(flowSection.gameObject, "Setup Diagnostics System Section");
            ClearChildren(flowSection);

            var component = flowSection.GetComponent<DiagnosticsSystemSection>();
            if (component == null)
                component = Undo.AddComponent<DiagnosticsSystemSection>(flowSection.gameObject);

            var title = CreateText(flowSection, "Diagnostics Title", "Diagnostics system", 22, FontStyle.Bold);
            var hint = CreateText(flowSection, "Diagnostics Hint", "Use this section to manage NetFlowDebugSystem, debug verbosity, layers, and debug-side channel filters.", 18, FontStyle.Normal);
            var summary = CreateText(flowSection, "Diagnostics Summary", "Waiting for runtime state...", 18, FontStyle.Normal);
            SetPreferredHeight(summary.gameObject, 66f);

            var debugConfigBox = CreateBox(flowSection, "Debug Config Subsection", "Debug config", "Core switches and layer flags for NetFlowDebugSystem.");
            var enabled = CreateToggleRow(debugConfigBox, "Enabled Toggle", "Enable debug system");
            var verbose = CreateToggleRow(debugConfigBox, "Verbose Toggle", "Verbose detail");
            var onlyFirst = CreateToggleRow(debugConfigBox, "Only First Toggle", "Only first log per layer/module in one flow");
            var layerSys = CreateToggleRow(debugConfigBox, "Layer Sys Toggle", "Layer: system");
            var layerAdcore = CreateToggleRow(debugConfigBox, "Layer Adcore Toggle", "Layer: adcore");
            var layerGroup = CreateToggleRow(debugConfigBox, "Layer Group Toggle", "Layer: group");
            var layerTracking = CreateToggleRow(debugConfigBox, "Layer Tracking Toggle", "Layer: tracking");

            var filterBox = CreateBox(flowSection, "Debug Filter Subsection", "Debug channel filter", "Use this when you want runtime flow/debug logs from only a few channels.");
            var enableFilter = CreateToggleRow(filterBox, "Enable Filter Toggle", "Enable channel filter");
            var row = CreateButtonRow(filterBox, "Filter Action Row");
            var allChannels = CreateButton(row, "All Channels Button", "All Channels");
            var clearChannels = CreateButton(row, "Clear Channels Button", "Clear Channels");
            var disableFilter = CreateButton(row, "Disable Filter Button", "Disable Filter");
            var channelFa = CreateToggleRow(filterBox, "Channel FA Toggle", "ForceAd");
            var channelRw = CreateToggleRow(filterBox, "Channel RW Toggle", "Rewarded");
            var channelAl = CreateToggleRow(filterBox, "Channel AL Toggle", "AppLaunch");
            var channelAr = CreateToggleRow(filterBox, "Channel AR Toggle", "AppResume");
            var channelBn = CreateToggleRow(filterBox, "Channel BN Toggle", "Banner");
            var channelMrec = CreateToggleRow(filterBox, "Channel Mrec Toggle", "Mrec");
            var channelPu = CreateToggleRow(filterBox, "Channel PU Toggle", "Popup");
            var channelCl = CreateToggleRow(filterBox, "Channel CL Toggle", "Collap");

            var so = new SerializedObject(component);
            Assign(so, "sectionRoot", flowSection);
            Assign(so, "titleText", title);
            Assign(so, "hintText", hint);
            Assign(so, "summaryText", summary);
            Assign(so, "enabledToggle", enabled);
            Assign(so, "verboseToggle", verbose);
            Assign(so, "onlyFirstToggle", onlyFirst);
            Assign(so, "layerSysToggle", layerSys);
            Assign(so, "layerAdcoreToggle", layerAdcore);
            Assign(so, "layerGroupToggle", layerGroup);
            Assign(so, "layerTrackingToggle", layerTracking);
            Assign(so, "enableFilterToggle", enableFilter);
            Assign(so, "allChannelsButton", allChannels);
            Assign(so, "clearChannelsButton", clearChannels);
            Assign(so, "disableFilterButton", disableFilter);
            Assign(so, "channelFaToggle", channelFa);
            Assign(so, "channelRwToggle", channelRw);
            Assign(so, "channelAlToggle", channelAl);
            Assign(so, "channelArToggle", channelAr);
            Assign(so, "channelBnToggle", channelBn);
            Assign(so, "channelMrecToggle", channelMrec);
            Assign(so, "channelPuToggle", channelPu);
            Assign(so, "channelClToggle", channelCl);
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(component);
            EditorUtility.SetDirty(flowSection.gameObject);
            EditorSceneManager.MarkSceneDirty(flowSection.gameObject.scene);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            UnityEngine.Debug.Log("DiagnosticsSystemSection setup completed.");
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
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            if (component == null)
                component = Undo.AddComponent<T>(target);
            return component;
        }
    }
}
#endif
