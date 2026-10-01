using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BG_Library.DEBUG.Editor
{
    public static class AdSystemsSectionPreviewBuilder
    {
        private const string OverlayPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/Debug Overlay Canvas.prefab";
        private const string SectionPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Section.prefab";
        private const string ButtonPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Button element.prefab";
        private const string NoticePrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Notice text.prefab";
        private const string InformationPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Information text.prefab";
        private const string TitlePrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Title text.prefab";
        private const string OutputPrefabPath = "Assets/BG_Lib/NetCore/Debug/Prefabs/UI/Ad Systems Section Preview.prefab";

        private static readonly Color ButtonColor = new Color(0.31f, 0.31f, 0.31f, 1f);
        private static readonly Color SelectedButtonColor = new Color(0.86f, 0.53f, 0.08f, 1f);
        private static readonly Color PanelColor = new Color(1f, 1f, 1f, 0.07f);
        private static readonly Color ScrollColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);

        [MenuItem("BG/Tools/Debug UI/Create Ad Systems Section Preview Prefab")]
        public static void CreatePreviewPrefab()
        {
            var previewRoot = BuildPreviewHierarchy();
            try
            {
                PrefabUtility.SaveAsPrefabAsset(previewRoot, OutputPrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OutputPrefabPath);
                Selection.activeObject = prefab;
            }
            finally
            {
                Object.DestroyImmediate(previewRoot);
            }
        }

        [MenuItem("BG/Tools/Debug UI/Spawn Ad Systems Section Preview")]
        public static void SpawnPreviewInScene()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OutputPrefabPath);
            if (prefab == null)
            {
                CreatePreviewPrefab();
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(OutputPrefabPath);
                if (prefab == null)
                    return;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                return;

            var parent = ResolveSpawnParent();
            if (parent != null)
                instance.transform.SetParent(parent, false);

            Undo.RegisterCreatedObjectUndo(instance, "Spawn Ad Systems Section Preview");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = instance;
        }

        [MenuItem("BG/Tools/Debug UI/Use Spawned Ad Systems Section Preview")]
        public static void UseSpawnedPreviewInOverlay()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(OverlayPrefabPath);
            try
            {
                var section = prefabRoot.GetComponentInChildren<AdSystemWorkspaceSection>(true);
                var previewRoot = FindDescendant(prefabRoot.transform, "Ad Systems Section Preview") as RectTransform;
                if (section == null || previewRoot == null)
                    return;

                FixPreviewLayout(previewRoot);
                BindSectionToPreview(section, previewRoot);

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, OverlayPrefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(OverlayPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static GameObject BuildPreviewHierarchy()
        {
            var section = InstantiatePrefab(SectionPrefabPath, null, "Ad Systems Section Preview");
            ConfigureSection(section);

            CreateNotice(section.transform,
                "Preview layout only. This version mirrors the standardized prefab style used by Build Info, Adjust, and Configs Workspace.");

            CreateInformation(section.transform,
                "Summary preview\n" +
                "AL | AppLaunch | API: AL_InitManually()\n" +
                "AR | AppResume | API: AR_InitManually()\n" +
                "RW | Rewarded | API: RW_InitManually(), RW_Show(pos)\n" +
                "BN | Banner | API: BN_InitManually(placement), BN_ActivateView(placement), BN_Hide(placement)\n" +
                "MREC | Mrec | API: Mrec_InitManually(), Mrec_ActivateView(), Mrec_Hide() | Utility: Mrec_UpdatePos, Mrec_GetSize\n" +
                "CL | Collap | API: CL_InitManually(), CL_Show(), CL_Hide()\n" +
                "FA | ForceAd | API: FA_InitManually(group), FA_Show(pos) | Utility: FA_StartBreakAd, FA_StopBreakAd\n" +
                "PU | Popup | API: PU_InitManually(group), PU_Show(pos), PU_Hide(pos) | Utility: PU_UpdatePos(pos, layout)");

            CreateButtonRow(section.transform, "Channels Row 1", new[] { "AL", "AR", "RW", "FA" }, 3);
            CreateButtonRow(section.transform, "Channels Row 2", new[] { "BN", "MREC", "CL", "PU" }, -1);
            CreateButtonRow(section.transform, "Selection Row", new[] { "Group: gameplay", "Placement / Position: FullBottom", "BreakAd Debug" }, -1);
            CreateButtonRow(section.transform, "Actions Row", new[] { "Init", "Activate", "Hide", "Start BreakAd", "Stop BreakAd" }, -1);

            CreateDetailCard(section.transform);
            CreateNotice(section.transform,
                "Legacy Ad Systems Section remains untouched. This prefab is only for reviewing spacing, typography, button rhythm, and scroll-view treatment.");

            return section;
        }

        private static void ConfigureSection(GameObject section)
        {
            var rootRect = section.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0.5f, 1f);
            rootRect.anchoredPosition = new Vector2(520f, -12f);
            rootRect.sizeDelta = new Vector2(1016f, 0f);

            var background = section.GetComponent<Image>();
            if (background != null)
                background.color = new Color(1f, 1f, 1f, 0.03137255f);

            var titleButton = section.transform.Find("Button title")?.GetComponent<Button>();
            var titleLabel = section.transform.Find("Button title/Label")?.GetComponent<Text>();
            if (titleLabel != null)
                titleLabel.text = "Ad Systems Section Preview";

            var collapsible = section.GetComponent<CollapArea>();
            if (collapsible != null)
            {
                var so = new SerializedObject(collapsible);
                so.FindProperty("startExpanded").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (titleButton != null)
                titleButton.gameObject.name = "Preview Section Title";
        }

        private static void CreateDetailCard(Transform parent)
        {
            var detailCard = CreateUiObject("Detail Card", parent);
            var cardImage = detailCard.AddComponent<Image>();
            cardImage.color = PanelColor;

            var cardLayout = detailCard.AddComponent<VerticalLayoutGroup>();
            cardLayout.padding = new RectOffset(12, 12, 12, 12);
            cardLayout.spacing = 8f;
            cardLayout.childAlignment = TextAnchor.UpperLeft;
            cardLayout.childControlWidth = true;
            cardLayout.childControlHeight = true;
            cardLayout.childForceExpandWidth = true;
            cardLayout.childForceExpandHeight = false;

            var cardElement = detailCard.AddComponent<LayoutElement>();
            cardElement.preferredHeight = 420f;
            cardElement.flexibleHeight = 0f;

            CreateTitle(detailCard.transform, "Selected channel: FA");
            CreateInformation(detailCard.transform,
                "Channel: FA | ForceAd\n" +
                "Group / Placement: gameplay or FullBottom\n" +
                "This block previews the unified detail surface for the Ad Systems workspace.");

            var scrollRoot = CreateUiObject("Detail Scroll", detailCard.transform);
            var scrollImage = scrollRoot.AddComponent<Image>();
            scrollImage.color = ScrollColor;

            var scrollElement = scrollRoot.AddComponent<LayoutElement>();
            scrollElement.preferredHeight = 260f;
            scrollElement.flexibleHeight = 1f;

            var scrollRect = scrollRoot.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 25f;
            scrollRect.inertia = true;

            var viewport = CreateUiObject("Viewport", scrollRoot.transform);
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
            var viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8f, 8f);
            viewportRect.offsetMax = new Vector2(-8f, -8f);

            var content = CreateUiObject("Content", viewport.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var detailText = CreateUiObject("Detail Text", content.transform);
            var detailRect = detailText.GetComponent<RectTransform>();
            detailRect.anchorMin = new Vector2(0f, 1f);
            detailRect.anchorMax = new Vector2(1f, 1f);
            detailRect.pivot = new Vector2(0.5f, 1f);
            detailRect.anchoredPosition = Vector2.zero;
            detailRect.sizeDelta = new Vector2(0f, 0f);

            var text = detailText.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 25;
            text.fontStyle = FontStyle.Normal;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            text.text =
                "--- System ---\n" +
                "Preview placeholder for ForceAd debug text.\n\n" +
                "--- Selected Group ---\n" +
                "Shows the currently selected gameplay group.\n\n" +
                "--- Notes ---\n" +
                "1. Uses the same title / notice / button scale as the standardized sections.\n" +
                "2. Keeps a dedicated detail card instead of the dense legacy table layout.\n" +
                "3. Reserves enough height for long debug text without making the section too tall.";

            var textFitter = detailText.AddComponent<ContentSizeFitter>();
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
        }

        private static GameObject CreateButtonRow(Transform parent, string name, string[] labels, int highlightedIndex)
        {
            var row = CreateUiObject(name, parent);
            var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 5f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;

            var rowElement = row.AddComponent<LayoutElement>();
            rowElement.preferredHeight = 50f;
            rowElement.flexibleHeight = 0f;

            for (int i = 0; i < labels.Length; i++)
            {
                var button = InstantiatePrefab(ButtonPrefabPath, row.transform, labels[i]);
                var image = button.GetComponent<Image>();
                if (image != null)
                    image.color = i == highlightedIndex ? SelectedButtonColor : ButtonColor;

                var element = button.GetComponent<LayoutElement>();
                if (element == null)
                    element = button.AddComponent<LayoutElement>();

                element.preferredHeight = 50f;
                element.flexibleWidth = 1f;

                var label = button.transform.Find("Label")?.GetComponent<Text>();
                if (label != null)
                    label.text = labels[i];
            }

            return row;
        }

        private static GameObject CreateNotice(Transform parent, string textValue)
        {
            var notice = InstantiatePrefab(NoticePrefabPath, parent, "Notice text");
            var text = notice.GetComponent<Text>();
            if (text != null)
                text.text = textValue;

            var rect = notice.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 34f);
            return notice;
        }

        private static GameObject CreateInformation(Transform parent, string textValue)
        {
            var info = InstantiatePrefab(InformationPrefabPath, parent, "Information text");
            var text = info.GetComponent<Text>();
            if (text != null)
                text.text = textValue;

            var rect = info.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 0f);
            return info;
        }

        private static GameObject CreateTitle(Transform parent, string textValue)
        {
            var title = InstantiatePrefab(TitlePrefabPath, parent, "Title text");
            var text = title.GetComponent<Text>();
            if (text != null)
                text.text = textValue;

            var rect = title.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 50f);

            var element = title.GetComponent<LayoutElement>();
            if (element == null)
                element = title.AddComponent<LayoutElement>();

            element.preferredHeight = 50f;
            element.flexibleHeight = 0f;
            return title;
        }

        private static GameObject InstantiatePrefab(string assetPath, Transform parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.name = name;
            if (parent != null)
                instance.transform.SetParent(parent, false);
            return instance;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            go.transform.SetParent(parent, false);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 0f);
            return go;
        }

        private static Transform ResolveSpawnParent()
        {
            if (Selection.activeTransform != null)
                return Selection.activeTransform;

            var safeArea = GameObject.Find("Safe area");
            return safeArea != null ? safeArea.transform : null;
        }

        private static void BindSectionToPreview(AdSystemWorkspaceSection section, RectTransform previewRoot)
        {
            var serializedSection = new SerializedObject(section);
            var legacyRoot = serializedSection.FindProperty("sectionRoot").objectReferenceValue as RectTransform;

            AssignIfFound(serializedSection, "sectionRoot", previewRoot);
            AssignIfFound(serializedSection, "titleText", FindTitleText(previewRoot));

            var topLevelTexts = GetTopLevelTexts(previewRoot);
            AssignIfFound(serializedSection, "hintText", GetAt(topLevelTexts, 0));
            AssignIfFound(serializedSection, "summaryText", GetAt(topLevelTexts, 1));

            var row1Buttons = GetDirectButtons(FindDescendant(previewRoot, "Channels Row 1") as RectTransform);
            var row2Buttons = GetDirectButtons(FindDescendant(previewRoot, "Channels Row 2") as RectTransform);
            AssignIfFound(serializedSection, "openAlButton", GetAt(row1Buttons, 0));
            AssignIfFound(serializedSection, "openArButton", GetAt(row1Buttons, 1));
            AssignIfFound(serializedSection, "openRwButton", GetAt(row1Buttons, 2));
            AssignIfFound(serializedSection, "openFaButton", GetAt(row1Buttons, 3));
            AssignIfFound(serializedSection, "openBnButton", GetAt(row2Buttons, 0));
            AssignIfFound(serializedSection, "openMrecButton", GetAt(row2Buttons, 1));
            AssignIfFound(serializedSection, "openClButton", GetAt(row2Buttons, 2));
            AssignIfFound(serializedSection, "openPuButton", GetAt(row2Buttons, 3));

            var selectionButtons = GetDirectButtons(FindDescendant(previewRoot, "Selection Row") as RectTransform);
            AssignIfFound(serializedSection, "selectGroupButton", GetAt(selectionButtons, 0));
            AssignIfFound(serializedSection, "selectPositionButton", GetAt(selectionButtons, 1));
            AssignIfFound(serializedSection, "refreshDetailButton", GetAt(selectionButtons, 2));

            var actionButtons = GetDirectButtons(FindDescendant(previewRoot, "Actions Row") as RectTransform);
            AssignIfFound(serializedSection, "initButton", GetAt(actionButtons, 0));
            AssignIfFound(serializedSection, "showButton", GetAt(actionButtons, 1));
            AssignIfFound(serializedSection, "hideButton", GetAt(actionButtons, 2));
            AssignIfFound(serializedSection, "utilityPrimaryButton", GetAt(actionButtons, 3));
            AssignIfFound(serializedSection, "utilitySecondaryButton", GetAt(actionButtons, 4));

            var detailCard = FindDescendant(previewRoot, "Detail Card") as RectTransform;
            var detailTexts = GetTopLevelTexts(detailCard);
            AssignIfFound(serializedSection, "detailTitleText", GetAt(detailTexts, 0));
            AssignIfFound(serializedSection, "detailSelectionText", GetAt(detailTexts, 1));
            AssignIfFound(serializedSection, "detailText", FindDescendant(detailCard, "Detail Text")?.GetComponent<Text>());

            var refreshWorkspace = FindButtonByLabel(previewRoot, "Refresh Workspace");
            if (refreshWorkspace == null)
                refreshWorkspace = FindButtonByLabel(previewRoot, "Refresh");

            AssignIfFound(serializedSection, "refreshWorkspaceButton", refreshWorkspace);
            serializedSection.ApplyModifiedPropertiesWithoutUndo();

            if (legacyRoot != null && legacyRoot != previewRoot)
                legacyRoot.gameObject.SetActive(false);
        }

        private static void FixPreviewLayout(RectTransform previewRoot)
        {
            var detailCard = FindDescendant(previewRoot, "Detail Card");
            if (detailCard != null)
            {
                var cardLayout = detailCard.GetComponent<VerticalLayoutGroup>();
                if (cardLayout != null)
                    cardLayout.childControlHeight = true;
            }

            var detailScroll = FindDescendant(previewRoot, "Detail Scroll");
            if (detailScroll != null)
            {
                var element = detailScroll.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.preferredHeight = 260f;
                    element.flexibleHeight = 1f;
                }
            }
        }

        private static void AssignIfFound(SerializedObject serializedObject, string propertyName, Object value)
        {
            if (value == null)
                return;

            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
                property.objectReferenceValue = value;
        }

        private static Text FindTitleText(RectTransform previewRoot)
        {
            if (previewRoot == null)
                return null;

            var titleRoot = previewRoot.Find("Preview Section Title");
            if (titleRoot == null)
                titleRoot = previewRoot.Find("Button title");

            return titleRoot != null ? titleRoot.GetComponentInChildren<Text>(true) : null;
        }

        private static System.Collections.Generic.List<Text> GetTopLevelTexts(RectTransform parent)
        {
            var results = new System.Collections.Generic.List<Text>();
            if (parent == null)
                return results;

            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.GetComponent<Button>() != null)
                    continue;

                if (child.name == "Detail Scroll")
                    continue;

                if (GetDirectButtons(child as RectTransform).Count > 0)
                    continue;

                var text = child.GetComponentInChildren<Text>(true);
                if (text != null)
                    results.Add(text);
            }

            return results;
        }

        private static System.Collections.Generic.List<Button> GetDirectButtons(RectTransform parent)
        {
            var results = new System.Collections.Generic.List<Button>();
            if (parent == null)
                return results;

            for (int i = 0; i < parent.childCount; i++)
            {
                var button = parent.GetChild(i).GetComponent<Button>();
                if (button != null)
                    results.Add(button);
            }

            return results;
        }

        private static T GetAt<T>(System.Collections.Generic.List<T> values, int index) where T : Object
        {
            return index >= 0 && index < values.Count ? values[index] : null;
        }

        private static Button FindButtonByLabel(Transform root, string label)
        {
            if (root == null || string.IsNullOrEmpty(label))
                return null;

            var buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var text = buttons[i].GetComponentInChildren<Text>(true);
                if (text != null && string.Equals(text.text, label, System.StringComparison.OrdinalIgnoreCase))
                    return buttons[i];
            }

            return null;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name))
                return null;

            if (string.Equals(root.name, name, System.StringComparison.Ordinal))
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindDescendant(root.GetChild(i), name);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
