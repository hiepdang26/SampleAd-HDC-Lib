using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    [CustomEditor(typeof(SdkVersionPresetV2))]
    internal sealed class SdkVersionPresetV2Editor : Editor
    {
        private const string FoldoutPrefKeyPrefix = "BG_BuildToolV2_PresetEditor_Foldout_";
        private const string SourceProfilePrefKey = "BG_BuildToolV2_PresetEditor_SourceProfileGuid";
        private const string CustomItemMenuLabel = "(custom...)";

        private static readonly SdkPresetMediationTagV2[] MediationOrder =
        {
            SdkPresetMediationTagV2.AdMob,
            SdkPresetMediationTagV2.MAX,
            SdkPresetMediationTagV2.Firebase,
            SdkPresetMediationTagV2.Adjust,
        };

        private static readonly Dictionary<SdkPresetMediationTagV2, SdkPresetCategoryTagV2[]> CategoryOrderByTag = new Dictionary<SdkPresetMediationTagV2, SdkPresetCategoryTagV2[]>
        {
            { SdkPresetMediationTagV2.AdMob, new[] { SdkPresetCategoryTagV2.CoreSDK, SdkPresetCategoryTagV2.MediationAdapter } },
            { SdkPresetMediationTagV2.MAX, new[] { SdkPresetCategoryTagV2.CoreSDK, SdkPresetCategoryTagV2.MediationAdapter } },
            { SdkPresetMediationTagV2.Firebase, new[] { SdkPresetCategoryTagV2.FirebaseModule } },
            { SdkPresetMediationTagV2.Adjust, new[] { SdkPresetCategoryTagV2.CoreSDK, SdkPresetCategoryTagV2.Environment } },
        };

        private SerializedProperty presetCodeProperty;
        private SerializedProperty notesProperty;
        private SerializedProperty entriesProperty;
        private BuildProfileV2 sourceProfile;
        private readonly Dictionary<SdkPresetMediationTagV2, CanonicalInspectionItemV2[]> canonicalCache = new Dictionary<SdkPresetMediationTagV2, CanonicalInspectionItemV2[]>(4);
        private BuildProfileV2 canonicalCacheProfile;

        private void OnEnable()
        {
            presetCodeProperty = serializedObject.FindProperty("presetCode");
            notesProperty = serializedObject.FindProperty("notes");
            entriesProperty = serializedObject.FindProperty("entries");
            sourceProfile = LoadCachedSourceProfile();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeaderFields();
            EditorGUILayout.Space(6f);
            DrawSourceProfileAndSnapshot();
            EditorGUILayout.Space(8f);
            DrawEntryGroups();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderFields()
        {
            EditorGUILayout.PropertyField(presetCodeProperty, new GUIContent("Preset Code"));
            EditorGUILayout.PropertyField(notesProperty, new GUIContent("Notes"));
        }

        private void DrawSourceProfileAndSnapshot()
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUI.BeginChangeCheck();
                var nextProfile = (BuildProfileV2)EditorGUILayout.ObjectField(
                    new GUIContent("Source Profile", "Profile used to populate the item dropdown and the Snapshot button."),
                    sourceProfile,
                    typeof(BuildProfileV2),
                    false);
                if (EditorGUI.EndChangeCheck())
                {
                    sourceProfile = nextProfile;
                    SaveCachedSourceProfile(sourceProfile);
                    canonicalCache.Clear();
                    canonicalCacheProfile = null;
                }

                using (new EditorGUI.DisabledScope(sourceProfile == null))
                {
                    if (GUILayout.Button("Snapshot from current project"))
                        HandleSnapshotFromCurrentProject();
                }

                if (sourceProfile == null)
                    EditorGUILayout.HelpBox("Drag a BuildProfileV2 here (or open BG Build Tool V2 first) to enable Snapshot and item dropdowns.", MessageType.Info);
            }
        }

        private void DrawEntryGroups()
        {
            for (int tagIndex = 0; tagIndex < MediationOrder.Length; tagIndex++)
            {
                SdkPresetMediationTagV2 mediationTag = MediationOrder[tagIndex];
                List<int> entryIndicesForTag = GetEntryIndicesByMediation(mediationTag);
                string foldoutKey = FoldoutPrefKeyPrefix + mediationTag;
                bool expanded = EditorPrefs.GetBool(foldoutKey, true);
                bool nextExpanded = EditorGUILayout.Foldout(
                    expanded,
                    $"{mediationTag}    ({entryIndicesForTag.Count} entries · {CountDistinctItems(entryIndicesForTag)} items)",
                    true,
                    EditorStyles.foldoutHeader);
                if (nextExpanded != expanded)
                    EditorPrefs.SetBool(foldoutKey, nextExpanded);

                if (!nextExpanded)
                    continue;

                using (new EditorGUI.IndentLevelScope())
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    DrawMediationSection(mediationTag, entryIndicesForTag);
                }
            }
        }

        private void DrawMediationSection(SdkPresetMediationTagV2 mediationTag, List<int> entryIndicesForTag)
        {
            SdkPresetCategoryTagV2[] categories = CategoryOrderByTag.TryGetValue(mediationTag, out SdkPresetCategoryTagV2[] preferredOrder)
                ? preferredOrder
                : (SdkPresetCategoryTagV2[])Enum.GetValues(typeof(SdkPresetCategoryTagV2));

            for (int categoryIndex = 0; categoryIndex < categories.Length; categoryIndex++)
            {
                SdkPresetCategoryTagV2 category = categories[categoryIndex];
                List<int> entryIndicesForCategory = FilterIndicesByCategory(entryIndicesForTag, category);

                EditorGUILayout.LabelField($"── {category} ──────────────────", EditorStyles.miniBoldLabel);

                List<int> itemGroupStarts = BuildItemGroupStarts(entryIndicesForCategory);
                for (int groupIdx = 0; groupIdx < itemGroupStarts.Count; groupIdx++)
                {
                    int groupStart = itemGroupStarts[groupIdx];
                    int groupEnd = (groupIdx + 1 < itemGroupStarts.Count) ? itemGroupStarts[groupIdx + 1] : entryIndicesForCategory.Count;
                    DrawItemCard(entryIndicesForCategory, groupStart, groupEnd);
                }

                DrawAddItemButton(mediationTag, category);
                EditorGUILayout.Space(4f);
            }
        }

        private void DrawItemCard(List<int> categoryEntryIndices, int groupStart, int groupEnd)
        {
            int firstEntryIndex = categoryEntryIndices[groupStart];
            SerializedProperty firstEntry = entriesProperty.GetArrayElementAtIndex(firstEntryIndex);
            SerializedProperty itemNameProperty = firstEntry.FindPropertyRelative("item");
            string itemName = itemNameProperty.stringValue ?? string.Empty;
            bool isCanonical = IsItemInCanonicalList((SdkPresetMediationTagV2)firstEntry.FindPropertyRelative("mediationTag").enumValueIndex, itemName);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (isCanonical)
                    {
                        EditorGUILayout.LabelField(string.IsNullOrWhiteSpace(itemName) ? "(unnamed item)" : itemName, EditorStyles.boldLabel);
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        string nextItemName = EditorGUILayout.DelayedTextField(itemName, EditorStyles.boldLabel);
                        if (EditorGUI.EndChangeCheck() && !string.Equals(nextItemName, itemName, StringComparison.Ordinal))
                            RenameItemInGroup(categoryEntryIndices, groupStart, groupEnd, nextItemName ?? string.Empty);
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("×", GUILayout.Width(22f)))
                    {
                        RemoveEntriesAtIndicesDescending(categoryEntryIndices, groupStart, groupEnd);
                        return;
                    }
                }

                for (int i = groupStart; i < groupEnd; i++)
                {
                    int entryIndex = categoryEntryIndices[i];
                    SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(entryIndex);
                    DrawPlatformRow(entry);
                }
            }
        }

        private void DrawPlatformRow(SerializedProperty entry)
        {
            SerializedProperty platformProperty = entry.FindPropertyRelative("platform");
            SerializedProperty expectedProperty = entry.FindPropertyRelative("expectedVersion");
            SerializedProperty itemProperty = entry.FindPropertyRelative("item");
            SerializedProperty mediationProperty = entry.FindPropertyRelative("mediationTag");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(((SdkPresetPlatformV2)platformProperty.enumValueIndex).ToString() + ":", GUILayout.Width(72f));
                expectedProperty.stringValue = EditorGUILayout.DelayedTextField(expectedProperty.stringValue ?? string.Empty);
                string driftMarker = ComputeDriftMarker((SdkPresetMediationTagV2)mediationProperty.enumValueIndex, itemProperty.stringValue, (SdkPresetPlatformV2)platformProperty.enumValueIndex, expectedProperty.stringValue);
                if (!string.IsNullOrEmpty(driftMarker))
                {
                    Color previous = GUI.color;
                    GUI.color = new Color(1f, 0.7f, 0.2f, 1f);
                    EditorGUILayout.LabelField(driftMarker, GUILayout.Width(120f));
                    GUI.color = previous;
                }
            }
        }

        private void DrawAddItemButton(SdkPresetMediationTagV2 mediationTag, SdkPresetCategoryTagV2 category)
        {
            using (new EditorGUI.DisabledScope(sourceProfile == null))
            {
                if (!GUILayout.Button($"+ Add {category} item ▾", GUILayout.Width(220f)))
                    return;

                var menu = new GenericMenu();
                CanonicalInspectionItemV2[] canonicalItems = GetCanonicalItems(mediationTag);
                HashSet<string> alreadyPresent = BuildItemNamesPresentInPreset(mediationTag, category);
                int addedToMenu = 0;
                for (int i = 0; i < canonicalItems.Length; i++)
                {
                    CanonicalInspectionItemV2 candidate = canonicalItems[i];
                    if (candidate == null || candidate.Category != category)
                        continue;

                    if (alreadyPresent.Contains(candidate.Item))
                        continue;

                    string menuLabel = candidate.Item;
                    SdkPresetMediationTagV2 capturedTag = mediationTag;
                    SdkPresetCategoryTagV2 capturedCategory = category;
                    CanonicalInspectionItemV2 capturedCandidate = candidate;
                    menu.AddItem(new GUIContent(menuLabel), false, () => AppendEntriesFromCanonical(capturedTag, capturedCategory, capturedCandidate));
                    addedToMenu++;
                }

                if (addedToMenu == 0)
                    menu.AddDisabledItem(new GUIContent("(no missing items from project)"));

                menu.AddSeparator(string.Empty);
                SdkPresetMediationTagV2 capturedTagCustom = mediationTag;
                SdkPresetCategoryTagV2 capturedCategoryCustom = category;
                menu.AddItem(new GUIContent(CustomItemMenuLabel), false, () => AppendCustomEntry(capturedTagCustom, capturedCategoryCustom));
                menu.ShowAsContext();
            }
        }

        private void HandleSnapshotFromCurrentProject()
        {
            if (sourceProfile == null)
                return;

            if (!EditorUtility.DisplayDialog(
                "Snapshot SDK Versions",
                "Replace all preset entries with the actual versions currently in the project? This cannot be undone.",
                "Replace",
                "Cancel"))
                return;

            SdkVersionInspectionEntryV2[] inspectionEntries = BuildToolV2Utilities.GetSdkVersionInspectionEntries(sourceProfile);
            entriesProperty.ClearArray();
            for (int i = 0; i < inspectionEntries.Length; i++)
            {
                SdkVersionInspectionEntryV2 source = inspectionEntries[i];
                if (source == null)
                    continue;

                entriesProperty.InsertArrayElementAtIndex(entriesProperty.arraySize);
                SerializedProperty newEntry = entriesProperty.GetArrayElementAtIndex(entriesProperty.arraySize - 1);
                newEntry.FindPropertyRelative("enabled").boolValue = true;
                newEntry.FindPropertyRelative("mediationTag").enumValueIndex = (int)source.Key.MediationTag;
                newEntry.FindPropertyRelative("categoryTag").enumValueIndex = (int)source.Key.CategoryTag;
                newEntry.FindPropertyRelative("item").stringValue = source.Key.Item ?? string.Empty;
                newEntry.FindPropertyRelative("platform").enumValueIndex = (int)source.Key.Platform;
                newEntry.FindPropertyRelative("expectedVersion").stringValue = source.ActualVersion ?? string.Empty;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
            canonicalCache.Clear();
            canonicalCacheProfile = null;
            GUIUtility.ExitGUI();
        }

        private void AppendEntriesFromCanonical(SdkPresetMediationTagV2 mediationTag, SdkPresetCategoryTagV2 category, CanonicalInspectionItemV2 candidate)
        {
            if (candidate == null)
                return;

            serializedObject.Update();
            for (int i = 0; i < candidate.Platforms.Count; i++)
            {
                SdkPresetPlatformV2 platform = candidate.Platforms[i];
                entriesProperty.InsertArrayElementAtIndex(entriesProperty.arraySize);
                SerializedProperty newEntry = entriesProperty.GetArrayElementAtIndex(entriesProperty.arraySize - 1);
                newEntry.FindPropertyRelative("enabled").boolValue = true;
                newEntry.FindPropertyRelative("mediationTag").enumValueIndex = (int)mediationTag;
                newEntry.FindPropertyRelative("categoryTag").enumValueIndex = (int)category;
                newEntry.FindPropertyRelative("item").stringValue = candidate.Item;
                newEntry.FindPropertyRelative("platform").enumValueIndex = (int)platform;
                newEntry.FindPropertyRelative("expectedVersion").stringValue = candidate.GetActualVersion(platform) ?? string.Empty;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            Repaint();
        }

        private void AppendCustomEntry(SdkPresetMediationTagV2 mediationTag, SdkPresetCategoryTagV2 category)
        {
            serializedObject.Update();
            entriesProperty.InsertArrayElementAtIndex(entriesProperty.arraySize);
            SerializedProperty newEntry = entriesProperty.GetArrayElementAtIndex(entriesProperty.arraySize - 1);
            newEntry.FindPropertyRelative("enabled").boolValue = true;
            newEntry.FindPropertyRelative("mediationTag").enumValueIndex = (int)mediationTag;
            newEntry.FindPropertyRelative("categoryTag").enumValueIndex = (int)category;
            newEntry.FindPropertyRelative("item").stringValue = "(custom item)";
            newEntry.FindPropertyRelative("platform").enumValueIndex = (int)SdkPresetPlatformV2.Unity;
            newEntry.FindPropertyRelative("expectedVersion").stringValue = string.Empty;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            Repaint();
        }

        private void RemoveEntriesAtIndicesDescending(List<int> categoryEntryIndices, int groupStart, int groupEnd)
        {
            var indicesToRemove = new List<int>(groupEnd - groupStart);
            for (int i = groupStart; i < groupEnd; i++)
                indicesToRemove.Add(categoryEntryIndices[i]);

            indicesToRemove.Sort((a, b) => b.CompareTo(a));

            serializedObject.Update();
            for (int i = 0; i < indicesToRemove.Count; i++)
                entriesProperty.DeleteArrayElementAtIndex(indicesToRemove[i]);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            Repaint();
            GUIUtility.ExitGUI();
        }

        private List<int> GetEntryIndicesByMediation(SdkPresetMediationTagV2 mediationTag)
        {
            var result = new List<int>(entriesProperty.arraySize);
            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(i);
                if ((SdkPresetMediationTagV2)entry.FindPropertyRelative("mediationTag").enumValueIndex == mediationTag)
                    result.Add(i);
            }

            return result;
        }

        private List<int> FilterIndicesByCategory(List<int> entryIndices, SdkPresetCategoryTagV2 category)
        {
            var result = new List<int>(entryIndices.Count);
            for (int i = 0; i < entryIndices.Count; i++)
            {
                int entryIndex = entryIndices[i];
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(entryIndex);
                if ((SdkPresetCategoryTagV2)entry.FindPropertyRelative("categoryTag").enumValueIndex == category)
                    result.Add(entryIndex);
            }

            return result;
        }

        private List<int> BuildItemGroupStarts(List<int> entryIndices)
        {
            var groupStarts = new List<int>(entryIndices.Count);
            string previousItem = null;
            for (int i = 0; i < entryIndices.Count; i++)
            {
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(entryIndices[i]);
                string currentItem = entry.FindPropertyRelative("item").stringValue ?? string.Empty;
                if (i == 0 || !string.Equals(currentItem, previousItem, StringComparison.OrdinalIgnoreCase))
                    groupStarts.Add(i);

                previousItem = currentItem;
            }

            return groupStarts;
        }

        private int CountDistinctItems(List<int> entryIndices)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < entryIndices.Count; i++)
            {
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(entryIndices[i]);
                seen.Add(entry.FindPropertyRelative("item").stringValue ?? string.Empty);
            }

            return seen.Count;
        }

        private HashSet<string> BuildItemNamesPresentInPreset(SdkPresetMediationTagV2 mediationTag, SdkPresetCategoryTagV2 category)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(i);
                if ((SdkPresetMediationTagV2)entry.FindPropertyRelative("mediationTag").enumValueIndex != mediationTag)
                    continue;
                if ((SdkPresetCategoryTagV2)entry.FindPropertyRelative("categoryTag").enumValueIndex != category)
                    continue;

                result.Add(entry.FindPropertyRelative("item").stringValue ?? string.Empty);
            }

            return result;
        }

        private bool IsItemInCanonicalList(SdkPresetMediationTagV2 mediationTag, string itemName)
        {
            if (sourceProfile == null || string.IsNullOrEmpty(itemName))
                return false;

            CanonicalInspectionItemV2[] canonicalItems = GetCanonicalItems(mediationTag);
            for (int i = 0; i < canonicalItems.Length; i++)
            {
                CanonicalInspectionItemV2 candidate = canonicalItems[i];
                if (candidate != null && string.Equals(candidate.Item, itemName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private void RenameItemInGroup(List<int> categoryEntryIndices, int groupStart, int groupEnd, string newItemName)
        {
            for (int i = groupStart; i < groupEnd; i++)
            {
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(categoryEntryIndices[i]);
                entry.FindPropertyRelative("item").stringValue = newItemName;
            }

            EditorUtility.SetDirty(target);
        }

        private CanonicalInspectionItemV2[] GetCanonicalItems(SdkPresetMediationTagV2 mediationTag)
        {
            if (sourceProfile == null)
                return Array.Empty<CanonicalInspectionItemV2>();

            if (canonicalCacheProfile != sourceProfile)
            {
                canonicalCache.Clear();
                canonicalCacheProfile = sourceProfile;
            }

            if (canonicalCache.TryGetValue(mediationTag, out CanonicalInspectionItemV2[] cached))
                return cached;

            CanonicalInspectionItemV2[] items = BuildToolV2SdkInspection.GetCanonicalInspectionItemsByMediation(sourceProfile, mediationTag);
            canonicalCache[mediationTag] = items;
            return items;
        }

        private string ComputeDriftMarker(SdkPresetMediationTagV2 mediationTag, string item, SdkPresetPlatformV2 platform, string expected)
        {
            if (sourceProfile == null)
                return string.Empty;

            CanonicalInspectionItemV2[] canonicalItems = GetCanonicalItems(mediationTag);
            for (int i = 0; i < canonicalItems.Length; i++)
            {
                CanonicalInspectionItemV2 candidate = canonicalItems[i];
                if (candidate == null || !string.Equals(candidate.Item, item, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!candidate.HasPlatform(platform))
                    return "(not in project)";

                string actual = candidate.GetActualVersion(platform) ?? string.Empty;
                bool equivalent = string.Equals(NormalizeForCompare(actual), NormalizeForCompare(expected), StringComparison.OrdinalIgnoreCase);
                return equivalent ? string.Empty : $"⚠ actual {actual}";
            }

            return "(not in project)";
        }

        private static string NormalizeForCompare(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static BuildProfileV2 LoadCachedSourceProfile()
        {
            string guid = EditorPrefs.GetString(SourceProfilePrefKey, string.Empty);
            if (string.IsNullOrWhiteSpace(guid))
                guid = SessionState.GetString(BuildToolWindowV2.SessionProfileGuidKey, string.Empty);

            if (string.IsNullOrWhiteSpace(guid))
                return null;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrWhiteSpace(path) ? null : AssetDatabase.LoadAssetAtPath<BuildProfileV2>(path);
        }

        private static void SaveCachedSourceProfile(BuildProfileV2 profile)
        {
            if (profile == null)
            {
                EditorPrefs.DeleteKey(SourceProfilePrefKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(profile);
            string guid = string.IsNullOrWhiteSpace(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
            EditorPrefs.SetString(SourceProfilePrefKey, guid);
        }
    }
}
