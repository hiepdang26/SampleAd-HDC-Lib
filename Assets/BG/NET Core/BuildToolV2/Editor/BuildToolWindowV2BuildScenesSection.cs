using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2BuildScenesSection
    {
        public static void Draw(BuildToolWindowV2 owner)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (!BuildToolWindowV2Ui.DrawFoldoutHeader("Build Scenes", "BG_BuildToolV2_Foldout_BuildScenes"))
                    return;

                BuildToolWindowV2.BuildScenesState state = owner.buildScenesState ?? new BuildToolWindowV2.BuildScenesState();
                EditorBuildSettingsScene[] scenes = state.Scenes ?? Array.Empty<EditorBuildSettingsScene>();

                EditorGUILayout.LabelField(
                    $"Primary scene for validation/prepare: {state.PrimarySceneLabel} (first enabled scene in Build Settings)",
                    EditorStyles.wordWrappedMiniLabel);

                if (scenes.Length == 0)
                    EditorGUILayout.HelpBox("No scenes in Build Settings.", MessageType.Warning);

                DrawToolbar(owner, scenes);
                DrawDropArea(owner);

                if (scenes.Length > 0)
                {
                    float listHeight = Mathf.Min(Mathf.Max(64f, scenes.Length * 28f), 220f);
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(owner.buildScenesScrollPosition, GUILayout.Height(listHeight)))
                    {
                        owner.buildScenesScrollPosition = scrollView.scrollPosition;
                        for (int i = 0; i < scenes.Length; i++)
                            DrawSceneRow(owner, scenes, i, state.PrimaryScenePath);
                    }
                }
            }
        }

        private static void DrawToolbar(BuildToolWindowV2 owner, EditorBuildSettingsScene[] scenes)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Open Scenes", GUILayout.Width(140f)))
                {
                    AddOpenScenesToBuildSettings(owner);
                    return;
                }

                if (GUILayout.Button("Clean Missing", GUILayout.Width(110f)))
                    RemoveMissingBuildScenes(owner, scenes);
            }
        }

        private static void DrawDropArea(BuildToolWindowV2 owner)
        {
            Rect dropRect = GUILayoutUtility.GetRect(0f, 54f, GUILayout.ExpandWidth(true));
            GUI.Box(dropRect, "Drag scene assets here to add them to Build Settings", EditorStyles.helpBox);

            Event currentEvent = Event.current;
            if (!dropRect.Contains(currentEvent.mousePosition))
                return;

            if (currentEvent.type != EventType.DragUpdated && currentEvent.type != EventType.DragPerform)
                return;

            bool hasSceneAsset = false;
            UnityEngine.Object[] draggedObjects = DragAndDrop.objectReferences;
            for (int i = 0; i < draggedObjects.Length; i++)
            {
                if (draggedObjects[i] is SceneAsset)
                {
                    hasSceneAsset = true;
                    break;
                }
            }

            if (!hasSceneAsset)
                return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddSceneAssetsToBuildSettings(owner, draggedObjects);
            }

            currentEvent.Use();
        }

        private static void DrawSceneRow(BuildToolWindowV2 owner, EditorBuildSettingsScene[] scenes, int index, string primaryScenePath)
        {
            if (scenes == null || index < 0 || index >= scenes.Length)
                return;

            EditorBuildSettingsScene scene = scenes[index];
            string scenePath = scene.path ?? string.Empty;
            bool isPrimary = string.Equals(scenePath, primaryScenePath, StringComparison.Ordinal);

            using (new EditorGUILayout.HorizontalScope("box"))
            {
                bool enabled = EditorGUILayout.Toggle(scene.enabled, GUILayout.Width(20f));
                if (enabled != scene.enabled)
                {
                    scenes[index].enabled = enabled;
                    owner.SaveBuildScenes(scenes);
                    return;
                }

                string prefix = isPrimary ? "[P]" : $"[{index}]";
                GUILayout.Label(prefix, GUILayout.Width(30f));

                SceneAsset sceneAsset = string.IsNullOrWhiteSpace(scenePath)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                SceneAsset nextSceneAsset = (SceneAsset)EditorGUILayout.ObjectField(sceneAsset, typeof(SceneAsset), false, GUILayout.Width(180f));
                if (nextSceneAsset != sceneAsset)
                {
                    string nextPath = nextSceneAsset == null ? string.Empty : AssetDatabase.GetAssetPath(nextSceneAsset);
                    ReplaceBuildSceneAsset(owner, scenes, index, nextPath, scene.enabled);
                    return;
                }

                string compactPath = GetCompactScenePath(scenePath);
                EditorGUILayout.LabelField(compactPath, EditorStyles.wordWrappedMiniLabel);

                if (GUILayout.Button(">", GUILayout.Width(26f)))
                {
                    OpenBuildScene(scenePath);
                    return;
                }

                using (new EditorGUI.DisabledScope(index <= 0))
                {
                    if (GUILayout.Button("^", GUILayout.Width(26f)))
                    {
                        MoveBuildScene(owner, scenes, index, index - 1);
                        return;
                    }
                }

                using (new EditorGUI.DisabledScope(index >= scenes.Length - 1))
                {
                    if (GUILayout.Button("v", GUILayout.Width(26f)))
                    {
                        MoveBuildScene(owner, scenes, index, index + 1);
                        return;
                    }
                }

                GUIContent removeIcon = EditorGUIUtility.IconContent("TreeEditor.Trash");
                removeIcon.tooltip = "Remove";
                if (GUILayout.Button(removeIcon, GUILayout.Width(30f), GUILayout.Height(20f)))
                {
                    RemoveBuildScene(owner, scenes, index);
                    return;
                }
            }
        }

        private static void AddOpenScenesToBuildSettings(BuildToolWindowV2 owner)
        {
            Scene[] loadedScenes = new Scene[SceneManager.sceneCount];
            for (int i = 0; i < SceneManager.sceneCount; i++)
                loadedScenes[i] = SceneManager.GetSceneAt(i);

            var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>());
            bool changed = false;

            for (int i = 0; i < loadedScenes.Length; i++)
            {
                string scenePath = loadedScenes[i].path;
                if (string.IsNullOrWhiteSpace(scenePath))
                    continue;

                if (ContainsBuildScene(existing, scenePath))
                    continue;

                existing.Add(new EditorBuildSettingsScene(scenePath, true));
                changed = true;
            }

            if (changed)
                owner.SaveBuildScenes(existing.ToArray());
        }

        private static void AddSceneAssetsToBuildSettings(BuildToolWindowV2 owner, UnityEngine.Object[] draggedObjects)
        {
            if (draggedObjects == null || draggedObjects.Length == 0)
                return;

            var scenePaths = new List<string>(draggedObjects.Length);
            for (int i = 0; i < draggedObjects.Length; i++)
            {
                if (draggedObjects[i] is not SceneAsset sceneAsset)
                    continue;

                string path = AssetDatabase.GetAssetPath(sceneAsset);
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                scenePaths.Add(path);
            }

            AddScenePathsToBuildSettings(owner, scenePaths);
        }

        private static void AddScenePathsToBuildSettings(BuildToolWindowV2 owner, IEnumerable<string> scenePaths)
        {
            if (scenePaths == null)
                return;

            var existing = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>());
            bool changed = false;
            foreach (string scenePath in scenePaths)
            {
                if (string.IsNullOrWhiteSpace(scenePath))
                    continue;

                if (ContainsBuildScene(existing, scenePath))
                    continue;

                existing.Add(new EditorBuildSettingsScene(scenePath, true));
                changed = true;
            }

            if (changed)
                owner.SaveBuildScenes(existing.ToArray());
        }

        private static void RemoveMissingBuildScenes(BuildToolWindowV2 owner, EditorBuildSettingsScene[] scenes)
        {
            if (scenes == null || scenes.Length == 0)
                return;

            var filtered = new List<EditorBuildSettingsScene>(scenes.Length);
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (scene == null || string.IsNullOrWhiteSpace(scene.path) || !File.Exists(scene.path))
                    continue;

                filtered.Add(scene);
            }

            if (filtered.Count == scenes.Length)
                return;

            owner.SaveBuildScenes(filtered.ToArray());
        }

        private static void RemoveBuildScene(BuildToolWindowV2 owner, EditorBuildSettingsScene[] scenes, int index)
        {
            if (scenes == null || index < 0 || index >= scenes.Length)
                return;

            var filtered = new EditorBuildSettingsScene[scenes.Length - 1];
            int target = 0;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (i == index)
                    continue;

                filtered[target++] = scenes[i];
            }

            owner.SaveBuildScenes(filtered);
        }

        private static void MoveBuildScene(BuildToolWindowV2 owner, EditorBuildSettingsScene[] scenes, int fromIndex, int toIndex)
        {
            if (scenes == null || fromIndex < 0 || fromIndex >= scenes.Length)
                return;

            if (toIndex < 0 || toIndex >= scenes.Length || fromIndex == toIndex)
                return;

            var reordered = (EditorBuildSettingsScene[])scenes.Clone();
            EditorBuildSettingsScene item = reordered[fromIndex];

            if (fromIndex < toIndex)
            {
                for (int i = fromIndex; i < toIndex; i++)
                    reordered[i] = reordered[i + 1];
            }
            else
            {
                for (int i = fromIndex; i > toIndex; i--)
                    reordered[i] = reordered[i - 1];
            }

            reordered[toIndex] = item;
            owner.SaveBuildScenes(reordered);
        }

        private static void ReplaceBuildSceneAsset(BuildToolWindowV2 owner, EditorBuildSettingsScene[] scenes, int index, string nextPath, bool enabled)
        {
            if (scenes == null || index < 0 || index >= scenes.Length || string.IsNullOrWhiteSpace(nextPath))
                return;

            var updated = (EditorBuildSettingsScene[])scenes.Clone();
            updated[index] = new EditorBuildSettingsScene(nextPath, enabled);
            owner.SaveBuildScenes(updated);
        }

        private static bool ContainsBuildScene(List<EditorBuildSettingsScene> scenes, string scenePath)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (scene == null)
                    continue;

                if (string.Equals(scene.path, scenePath, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void OpenBuildScene(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        private static string GetCompactScenePath(string scenePath)
        {
            if (string.IsNullOrWhiteSpace(scenePath))
                return "(missing)";

            string normalized = scenePath.Replace('\\', '/');
            const string assetsPrefix = "Assets/";
            if (normalized.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(assetsPrefix.Length);

            return normalized;
        }
    }
}
