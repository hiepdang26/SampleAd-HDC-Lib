using System;
using System.Collections.Generic;
using System.IO;
using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BG_Library.BuildToolV2
{
    internal enum BuildValidationSeverity
    {
        Info,
        Warning,
        Error,
    }

    internal sealed class BuildValidationIssue
    {
        public string Code;
        public BuildValidationSeverity Severity;
        public string Message;
        public string Details;
        public string FixLabel;
        public Action FixAction;
    }

    internal enum BuildCheckStatus
    {
        Normal,
        Warning,
        Error,
    }

    internal sealed class BuildCheckItem
    {
        public string Name;
        public string Description;
        public BuildCheckStatus Status;
        public string StatusMessage;
        public string ActionLabel;
        public Action Action;
    }

    internal sealed class BuildValidationResult
    {
        public readonly List<BuildValidationIssue> Issues = new List<BuildValidationIssue>();
        public readonly List<BuildCheckItem> Checks = new List<BuildCheckItem>();

        public int ErrorCount => Count(BuildValidationSeverity.Error);
        public int WarningCount => Count(BuildValidationSeverity.Warning);
        public bool CanBuild => ErrorCount == 0;

        private int Count(BuildValidationSeverity severity)
        {
            int count = 0;
            for (int i = 0; i < Issues.Count; i++)
            {
                if (Issues[i].Severity == severity)
                    count++;
            }

            return count;
        }
    }

    internal sealed class BuildValidationContext : IDisposable
    {
        private bool primaryScenePreparedForValidation;
        private bool primarySceneLoadedTemporarily;
        private Scene primaryScene;

        public BuildValidationContext(BuildProfileV2 profile)
        {
            Profile = profile;
            NetConfigs = profile != null ? profile.ResolveNetConfigs() : null;
            PrimaryScenePath = ResolvePrimaryScenePath();
        }

        public BuildProfileV2 Profile { get; }
        public NetConfigsSO NetConfigs { get; }
        public string PrimaryScenePath { get; }

        public bool TryLoadPrimarySceneForValidation(out string errorMessage)
        {
            errorMessage = null;

            if (primaryScenePreparedForValidation)
                return true;

            if (string.IsNullOrWhiteSpace(PrimaryScenePath))
            {
                errorMessage = "Primary build scene trong Build Settings khong hop le.";
                return false;
            }

            Scene loadedScene = SceneManager.GetSceneByPath(PrimaryScenePath);
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                primaryScene = loadedScene;
                primaryScenePreparedForValidation = true;
                return true;
            }

            primaryScene = EditorSceneManager.OpenScene(PrimaryScenePath, OpenSceneMode.Additive);
            if (!primaryScene.IsValid())
            {
                errorMessage = $"Khong mo duoc primary build scene: {PrimaryScenePath}";
                return false;
            }

            primarySceneLoadedTemporarily = true;
            primaryScenePreparedForValidation = true;
            return true;
        }

        public bool TryOpenPrimarySceneForBuild(out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(PrimaryScenePath))
            {
                errorMessage = "Primary build scene trong Build Settings khong hop le.";
                return false;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path == PrimaryScenePath)
                return true;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                errorMessage = "Da huy mo primary build scene de build.";
                return false;
            }

            EditorSceneManager.OpenScene(PrimaryScenePath, OpenSceneMode.Single);
            return true;
        }

        public T FindFirstInPrimaryScene<T>() where T : UnityEngine.Object
        {
            T[] objects = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            for (int i = 0; i < objects.Length; i++)
            {
                if (IsInPrimaryScene(objects[i]))
                    return objects[i];
            }

            return null;
        }

        public GameObject FindFirstGameObjectInPrimaryScene(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return null;

            GameObject[] objects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject gameObject = objects[i];
                if (gameObject == null)
                    continue;

                if (!string.Equals(gameObject.name, objectName, StringComparison.Ordinal))
                    continue;

                if (gameObject.scene.path == PrimaryScenePath)
                    return gameObject;
            }

            return null;
        }

        public T FindFirstInEnabledBuildScenes<T>(out string scenePath) where T : Component
        {
            scenePath = null;
            string[] enabledScenePaths = GetEnabledBuildScenePaths();
            for (int i = 0; i < enabledScenePaths.Length; i++)
            {
                string currentScenePath = enabledScenePaths[i];
                T component = FindFirstInScenePath<T>(currentScenePath);
                if (component == null)
                    continue;

                scenePath = currentScenePath;
                return component;
            }

            return null;
        }

        public GameObject FindFirstGameObjectInEnabledBuildScenes(string objectName, out string scenePath)
        {
            scenePath = null;
            if (string.IsNullOrWhiteSpace(objectName))
                return null;

            string[] enabledScenePaths = GetEnabledBuildScenePaths();
            for (int i = 0; i < enabledScenePaths.Length; i++)
            {
                string currentScenePath = enabledScenePaths[i];
                GameObject gameObject = FindFirstGameObjectInScenePath(currentScenePath, objectName);
                if (gameObject == null)
                    continue;

                scenePath = currentScenePath;
                return gameObject;
            }

            return null;
        }

        public void Dispose()
        {
            if (!primarySceneLoadedTemporarily || !primaryScene.IsValid() || !primaryScene.isLoaded)
                return;

            EditorSceneManager.CloseScene(primaryScene, true);
        }

        private bool IsInPrimaryScene(UnityEngine.Object target)
        {
            switch (target)
            {
                case Component component:
                    return component.gameObject.scene.path == PrimaryScenePath;
                case GameObject gameObject:
                    return gameObject.scene.path == PrimaryScenePath;
                default:
                    return false;
            }
        }

        private T FindFirstInScenePath<T>(string scenePath) where T : Component
        {
            if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                return null;

            Scene loadedScene = SceneManager.GetSceneByPath(scenePath);
            bool loadedTemporarily = false;
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                loadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                if (!loadedScene.IsValid())
                    return null;

                loadedTemporarily = true;
            }

            try
            {
                T[] objects = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
                for (int i = 0; i < objects.Length; i++)
                {
                    T component = objects[i];
                    if (component != null && component.gameObject.scene.path == scenePath)
                        return component;
                }
            }
            finally
            {
                if (loadedTemporarily && loadedScene.IsValid() && loadedScene.isLoaded)
                    EditorSceneManager.CloseScene(loadedScene, true);
            }

            return null;
        }

        private GameObject FindFirstGameObjectInScenePath(string scenePath, string objectName)
        {
            if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath))
                return null;

            Scene loadedScene = SceneManager.GetSceneByPath(scenePath);
            bool loadedTemporarily = false;
            if (!loadedScene.IsValid() || !loadedScene.isLoaded)
            {
                loadedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                if (!loadedScene.IsValid())
                    return null;

                loadedTemporarily = true;
            }

            try
            {
                GameObject[] objects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                for (int i = 0; i < objects.Length; i++)
                {
                    GameObject gameObject = objects[i];
                    if (gameObject == null)
                        continue;

                    if (gameObject.scene.path != scenePath)
                        continue;

                    if (string.Equals(gameObject.name, objectName, StringComparison.Ordinal))
                        return gameObject;
                }
            }
            finally
            {
                if (loadedTemporarily && loadedScene.IsValid() && loadedScene.isLoaded)
                    EditorSceneManager.CloseScene(loadedScene, true);
            }

            return null;
        }

        private static string[] GetEnabledBuildScenePaths()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            var paths = new List<string>(scenes.Length);
            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (scene == null || !scene.enabled || string.IsNullOrWhiteSpace(scene.path))
                    continue;

                paths.Add(scene.path);
            }

            return paths.ToArray();
        }

        private static string ResolvePrimaryScenePath()
        {
            return BuildToolV2Utilities.GetPrimaryEnabledScenePath();
        }
    }

    internal interface IBuildValidator
    {
        string Name { get; }
        string DisplayName { get; }
        string Description { get; }
        void Validate(BuildValidationContext context, List<BuildValidationIssue> issues);
    }

    internal static class BuildToolV2ValidationRunner
    {
        private static readonly IBuildValidator[] Validators =
        {
            new PresetConsistencyValidator(),
            new BuildTargetValidator(),
            new BuildSettingsValidator(),
            new NetConfigsValidator(),
            new VersionValidator(),
            new OutputDirectoryValidator(),
            new ChangeLogValidator(),
            new BGSetupValidator(),
            new AdsLogicValidator(),
            new DebugOverlayValidator(),
            new AdmobValidator(),
            new FirebaseConfigValidator(),
            new AdjustValidator(),
            new MaxSdkValidator(),
            new SdkPresetValidator(),
            new MfuscatorValidator(),
        };

        public static BuildValidationResult Run(BuildProfileV2 profile)
        {
            var result = new BuildValidationResult();

            if (profile == null)
            {
                result.Issues.Add(new BuildValidationIssue
                {
                    Code = "PROFILE_MISSING",
                    Severity = BuildValidationSeverity.Error,
                    Message = "Chua chon Build Profile.",
                });
                return result;
            }

            using var context = new BuildValidationContext(profile);

            for (int i = 0; i < Validators.Length; i++)
            {
                IBuildValidator validator = Validators[i];
                var validatorIssues = new List<BuildValidationIssue>();
                try
                {
                    validator.Validate(context, validatorIssues);
                }
                catch (Exception exception)
                {
                    validatorIssues.Add(new BuildValidationIssue
                    {
                        Code = $"VALIDATOR_EXCEPTION_{validator.Name}",
                        Severity = BuildValidationSeverity.Error,
                        Message = $"Validator {validator.Name} gap loi runtime.",
                        Details = exception.ToString(),
                    });
                }

                for (int issueIndex = 0; issueIndex < validatorIssues.Count; issueIndex++)
                    result.Issues.Add(validatorIssues[issueIndex]);

                result.Checks.Add(BuildCheckItemFromValidator(validator, validatorIssues));
            }

            result.Checks.Sort(CompareChecks);
            return result;
        }

        private static BuildCheckItem BuildCheckItemFromValidator(IBuildValidator validator, List<BuildValidationIssue> issues)
        {
            var item = new BuildCheckItem
            {
                Name = validator.DisplayName,
                Description = validator.Description,
                Status = BuildCheckStatus.Normal,
                StatusMessage = "OK",
            };

            if (issues == null || issues.Count == 0)
                return item;

            BuildValidationIssue primaryIssue = issues[0];
            BuildValidationSeverity highestSeverity = primaryIssue.Severity;
            for (int i = 1; i < issues.Count; i++)
            {
                if (issues[i].Severity > highestSeverity)
                {
                    highestSeverity = issues[i].Severity;
                    primaryIssue = issues[i];
                }
            }

            item.Status = highestSeverity == BuildValidationSeverity.Error
                ? BuildCheckStatus.Error
                : BuildCheckStatus.Warning;
            item.StatusMessage = string.IsNullOrWhiteSpace(primaryIssue?.Details)
                ? primaryIssue?.Message ?? "Warning"
                : $"{primaryIssue.Message}\n{primaryIssue.Details}";
            item.ActionLabel = primaryIssue?.FixLabel;
            item.Action = primaryIssue?.FixAction;
            return item;
        }

        private static int CompareChecks(BuildCheckItem left, BuildCheckItem right)
        {
            int leftRank = GetStatusRank(left?.Status ?? BuildCheckStatus.Normal);
            int rightRank = GetStatusRank(right?.Status ?? BuildCheckStatus.Normal);
            int rankCompare = leftRank.CompareTo(rightRank);
            if (rankCompare != 0)
                return rankCompare;

            return string.Compare(left?.Name, right?.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetStatusRank(BuildCheckStatus status)
        {
            switch (status)
            {
                case BuildCheckStatus.Error:
                    return 0;
                case BuildCheckStatus.Warning:
                    return 1;
                default:
                    return 2;
            }
        }
    }
}
