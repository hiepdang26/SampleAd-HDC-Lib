#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BG_Library.DEBUG
{
    internal static class DebugUiEditorFactory
    {
        internal const string SectionPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Section.prefab";
        internal const string ButtonPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Button element.prefab";
        internal const string CloseButtonPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Close button element.prefab";
        internal const string NoticePrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Notice text.prefab";
        internal const string InformationPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Information text.prefab";
        internal const string TitlePrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Title text.prefab";

        internal static readonly Color CardColor = new Color(0.2f, 0.2f, 0.2f, 0.92f);
        internal static readonly Color ButtonColor = new Color(0.31f, 0.31f, 0.31f, 1f);
        internal static readonly Color OverlayColor = new Color(0f, 0f, 0f, 0.72f);
        internal static readonly Color ScrollColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);
        internal static readonly Color ToggleBackgroundColor = new Color(0.24f, 0.24f, 0.24f, 1f);
        internal static readonly Color ToggleCheckmarkColor = new Color(0.86f, 0.53f, 0.08f, 1f);

        internal static GameObject InstantiatePrefab(string assetPath, Transform parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                return null;

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                return null;

            instance.name = name;
            if (parent != null)
                instance.transform.SetParent(parent, false);

            return instance;
        }

        internal static GameObject CreateUiObject(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        internal static RectTransform Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (rect == null)
                return null;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return rect;
        }

        internal static GameObject CreateSectionRoot(Transform parent, string name, string title)
        {
            var root = InstantiatePrefab(SectionPrefabPath, parent, name);
            if (root == null)
                return null;

            var rootRect = root.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = new Vector2(0f, 1f);
                rootRect.anchorMax = new Vector2(0f, 1f);
                rootRect.pivot = new Vector2(0.5f, 1f);
                rootRect.anchoredPosition = Vector2.zero;
                rootRect.sizeDelta = new Vector2(1016f, 0f);
            }

            var titleLabel = root.transform.Find("Button title/Label")?.GetComponent<Text>();
            if (titleLabel != null)
                titleLabel.text = title;

            var collapsible = root.GetComponent<CollapArea>();
            if (collapsible != null)
            {
                var so = new SerializedObject(collapsible);
                so.FindProperty("startExpanded").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(collapsible);
            }

            return root;
        }

        internal static GameObject CreateNotice(Transform parent, string textValue, string name = "Notice text")
        {
            var notice = InstantiatePrefab(NoticePrefabPath, parent, name);
            var text = notice != null ? notice.GetComponent<Text>() : null;
            if (text != null)
                text.text = textValue;
            return notice;
        }

        internal static GameObject CreateInformation(Transform parent, string textValue, string name = "Information text")
        {
            var info = InstantiatePrefab(InformationPrefabPath, parent, name);
            var text = info != null ? info.GetComponent<Text>() : null;
            if (text != null)
                text.text = textValue;
            return info;
        }

        internal static GameObject CreateTitle(Transform parent, string textValue, string name = "Title text")
        {
            var title = InstantiatePrefab(TitlePrefabPath, parent, name);
            if (title == null)
                return null;

            var text = title.GetComponent<Text>();
            if (text != null)
                text.text = textValue;

            var element = title.GetComponent<LayoutElement>() ?? title.AddComponent<LayoutElement>();
            element.preferredHeight = 50f;
            element.flexibleHeight = 0f;
            return title;
        }

        internal static GameObject CreateButton(Transform parent, string name, string label, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var buttonObject = InstantiatePrefab(ButtonPrefabPath, parent, name);
            if (buttonObject == null)
                return null;

            var image = buttonObject.GetComponent<Image>();
            if (image != null)
                image.color = ButtonColor;

            var element = buttonObject.GetComponent<LayoutElement>() ?? buttonObject.AddComponent<LayoutElement>();
            element.preferredHeight = 50f;

            var labelText = buttonObject.transform.Find("Label")?.GetComponent<Text>();
            if (labelText != null)
            {
                labelText.text = label;
                labelText.alignment = alignment;
                if (alignment == TextAnchor.MiddleLeft)
                {
                    labelText.rectTransform.offsetMin = new Vector2(18f, 0f);
                    labelText.rectTransform.offsetMax = new Vector2(-12f, 0f);
                }
            }

            return buttonObject;
        }

        internal static GameObject CreateCloseButton(Transform parent, string name = "Close button")
        {
            var buttonObject = InstantiatePrefab(CloseButtonPrefabPath, parent, name);
            if (buttonObject == null)
                return null;

            var element = buttonObject.GetComponent<LayoutElement>() ?? buttonObject.AddComponent<LayoutElement>();
            element.preferredWidth = 50f;
            element.preferredHeight = 50f;
            element.flexibleWidth = 0f;
            return buttonObject;
        }

        internal static GameObject CreateCard(Transform parent, string name, float spacing = 8f, RectOffset padding = null)
        {
            var card = CreateUiObject(name, parent);
            var image = card.AddComponent<Image>();
            image.color = CardColor;

            var layout = card.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(12, 12, 12, 12);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = card.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return card;
        }

        internal static GameObject CreateRow(Transform parent, string name, float spacing = 6f, bool forceExpandWidth = true)
        {
            var row = CreateUiObject(name, parent);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = forceExpandWidth;
            layout.childForceExpandHeight = false;
            return row;
        }

        internal static GameObject CreateToggleRow(Transform parent, string name, string label)
        {
            var row = CreateRow(parent, name, 12f, false);
            var rowElement = row.AddComponent<LayoutElement>();
            rowElement.preferredHeight = 46f;

            var toggleObject = CreateUiObject("Toggle", row.transform);
            var toggleElement = toggleObject.AddComponent<LayoutElement>();
            toggleElement.minWidth = 42f;
            toggleElement.minHeight = 42f;
            toggleElement.preferredWidth = 42f;
            toggleElement.preferredHeight = 42f;

            var toggle = toggleObject.AddComponent<Toggle>();
            var background = CreateUiObject("Background", toggleObject.transform);
            var backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            backgroundRect.sizeDelta = new Vector2(28f, 28f);
            var backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = ToggleBackgroundColor;

            var checkmark = CreateUiObject("Checkmark", background.transform);
            var checkmarkRect = checkmark.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkmarkRect.pivot = new Vector2(0.5f, 0.5f);
            checkmarkRect.sizeDelta = new Vector2(18f, 18f);
            var checkmarkImage = checkmark.AddComponent<Image>();
            checkmarkImage.color = ToggleCheckmarkColor;

            toggle.targetGraphic = backgroundImage;
            toggle.graphic = checkmarkImage;

            var labelObject = CreateUiObject("Label", row.transform);
            var labelText = labelObject.AddComponent<Text>();
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 20;
            labelText.fontStyle = FontStyle.Normal;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.horizontalOverflow = HorizontalWrapMode.Wrap;
            labelText.verticalOverflow = VerticalWrapMode.Overflow;
            labelText.color = Color.white;
            labelText.text = label;

            var labelElement = labelObject.AddComponent<LayoutElement>();
            labelElement.minWidth = 120f;
            labelElement.preferredHeight = 34f;
            labelElement.flexibleWidth = 1f;

            return row;
        }
    }
}
#endif
