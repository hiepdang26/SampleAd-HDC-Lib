using System;
using System.Collections.Generic;
using System.IO;
using BG_Library.Common;
using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal sealed class BuildTargetValidator : IBuildValidator
    {
        public string Name => nameof(BuildTargetValidator);
        public string DisplayName => "Build Target";
        public string Description => "Đối chiếu Active Build Target hiện tại của Unity với target trong preset để tránh build sai nền tảng.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (EditorUserBuildSettings.activeBuildTarget == context.Profile.BuildTarget)
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "TARGET_MISMATCH",
                Severity = BuildValidationSeverity.Error,
                Message = $"Active Build Target hiện là {EditorUserBuildSettings.activeBuildTarget}, trong khi profile yêu cầu {context.Profile.BuildTarget}.",
                FixLabel = "Switch Target",
                FixAction = () =>
                {
                    BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(context.Profile.BuildTarget);
                    EditorUserBuildSettings.SwitchActiveBuildTarget(group, context.Profile.BuildTarget);
                },
            });
        }
    }

    internal sealed class PresetConsistencyValidator : IBuildValidator
    {
        public string Name => nameof(PresetConsistencyValidator);
        public string DisplayName => "Preset Consistency";
        public string Description => "So sánh các managed setting của profile với preset mặc định để phát hiện cấu hình custom đang lệch preset.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (context.Profile == null || !context.Profile.HasManagedSourceMismatches())
                return;

            string[] summaries = context.Profile.GetManagedSourceMismatchSummaries();
            string details = summaries == null || summaries.Length == 0
                ? "Profile đang có override custom khác preset mặc định."
                : string.Join(" | ", summaries);

            issues.Add(new BuildValidationIssue
            {
                Code = "PRESET_OVERRIDES",
                Severity = BuildValidationSeverity.Warning,
                Message = "Profile đang có managed setting lệch preset mặc định.",
                Details = details,
                FixLabel = "Reset Preset",
                FixAction = () =>
                {
                    context.Profile.ResetManagedOverridesToPreset();
                    BuildToolV2Utilities.ApplyPresetManagedSourcesImmediately(context.Profile);
                },
            });
        }
    }

    internal sealed class BuildSettingsValidator : IBuildValidator
    {
        public string Name => nameof(BuildSettingsValidator);
        public string DisplayName => "Build Settings";
        public string Description => "Kiểm tra danh sách scene enabled trong Build Settings và xác nhận scene đầu tiên dùng làm primary scene là hợp lệ.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes ?? Array.Empty<EditorBuildSettingsScene>();
            bool hasEnabledScene = false;

            for (int i = 0; i < scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = scenes[i];
                if (scene == null || !scene.enabled)
                    continue;

                hasEnabledScene = true;
                if (!File.Exists(scene.path))
                {
                    issues.Add(new BuildValidationIssue
                    {
                        Code = "SCENE_ENABLED_INVALID",
                        Severity = BuildValidationSeverity.Error,
                        Message = $"Có scene enabled bị missing: {scene.path}",
                    });
                }
            }

            if (!hasEnabledScene || string.IsNullOrWhiteSpace(context.PrimaryScenePath))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "SCENE0_MISSING",
                    Severity = BuildValidationSeverity.Error,
                    Message = "Build Settings chưa có scene enabled nào hợp lệ.",
                });
                return;
            }

            if (!File.Exists(context.PrimaryScenePath))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "SCENE0_INVALID",
                    Severity = BuildValidationSeverity.Error,
                    Message = $"Primary build scene không tồn tại: {context.PrimaryScenePath}",
                });
            }
        }
    }

    internal sealed class NetConfigsValidator : IBuildValidator
    {
        public string Name => nameof(NetConfigsValidator);
        public string DisplayName => "NET Configs";
        public string Description => "Xác nhận build preset hiện tại đang resolve được NetConfigsSO để mọi bước đồng bộ NET có nguồn cấu hình rõ ràng.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (context.NetConfigs != null)
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "NET_CONFIGS_MISSING",
                Severity = BuildValidationSeverity.Error,
                Message = "Không tìm thấy NetConfigsSO cho profile hiện tại.",
            });
        }
    }

    internal sealed class VersionValidator : IBuildValidator
    {
        public string Name => nameof(VersionValidator);
        public string DisplayName => "Version";
        public string Description => "Kiểm tra Version và Android Version Code hiện tại có giá trị hợp lệ trước khi bắt đầu build.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            string version = context.Profile.ResolveVersion();
            if (string.IsNullOrWhiteSpace(version))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "VERSION_EMPTY",
                    Severity = BuildValidationSeverity.Error,
                    Message = "Version đang rỗng.",
                });
                return;
            }

            if (context.Profile.BuildTarget != BuildTarget.Android)
                return;

            if (context.Profile.VersionCode <= 0)
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "ANDROID_VERSIONCODE_INVALID",
                    Severity = BuildValidationSeverity.Error,
                    Message = "Android Version Code phải lớn hơn 0.",
                });
            }
        }
    }

    internal sealed class OutputDirectoryValidator : IBuildValidator
    {
        public string Name => nameof(OutputDirectoryValidator);
        public string DisplayName => "Output Folder";
        public string Description => "Kiểm tra thư mục output hiện tại đã được chọn và còn tồn tại để tránh dừng build ở bước xuất file.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            string outputDirectory = context.Profile.OutputDirectory;
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "OUTPUT_DIRECTORY_EMPTY",
                    Severity = BuildValidationSeverity.Info,
                    Message = "Chưa chọn Output Folder. Tool sẽ hỏi thư mục khi bắt đầu build.",
                    FixLabel = "Choose Folder",
                    FixAction = () => BuildToolV2Utilities.ChooseOutputDirectory(context.Profile),
                });
                return;
            }

            if (Directory.Exists(outputDirectory))
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "OUTPUT_DIRECTORY_MISSING",
                Severity = BuildValidationSeverity.Warning,
                Message = $"Output Folder không tồn tại: {outputDirectory}",
                FixLabel = "Choose Folder",
                FixAction = () => BuildToolV2Utilities.ChooseOutputDirectory(context.Profile),
            });
        }
    }

    internal sealed class ChangeLogValidator : IBuildValidator
    {
        public string Name => nameof(ChangeLogValidator);
        public string DisplayName => "Change Log";
        public string Description => "Yêu cầu có Change Log với preset không phải Test Debug để việc theo dõi build và phát hành rõ ràng hơn.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (!context.Profile.RequiresChangeLog)
                return;

            if (!string.IsNullOrWhiteSpace(context.Profile.ChangeLog))
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "CHANGELOG_REQUIRED",
                Severity = BuildValidationSeverity.Error,
                Message = $"Preset {context.Profile.Preset} bắt buộc phải có change log trước khi build.",
            });
        }
    }

    internal sealed class BGSetupValidator : IBuildValidator
    {
        public string Name => nameof(BGSetupValidator);
        public string DisplayName => "BG_SETUP";
        public string Description => "Kiểm tra BG_SETUP ở scene 0. Nếu thiếu, báo lỗi trực tiếp tại scene 0 và gợi ý người dùng tự sửa scene này hoặc tự đặt scene đúng lên vị trí 0.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (!context.TryLoadPrimarySceneForValidation(out string errorMessage))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "SCENE0_OPEN_FAILED",
                    Severity = BuildValidationSeverity.Error,
                    Message = errorMessage,
                });
                return;
            }

            BG_SETUP setup = context.FindFirstInPrimaryScene<BG_SETUP>();
            if (setup != null)
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "BG_SETUP_MISSING",
                Severity = BuildValidationSeverity.Error,
                Message = "Scene 0 hiện tại không có BG_SETUP.",
                Details = "Hãy tự kiểm tra scene đúng cần đứng ở vị trí 0 trong Build Settings, hoặc bổ sung BG_SETUP để scene 0 hiện tại khớp với cấu hình mong muốn.",
                FixLabel = "Create BG_SETUP",
                FixAction = () =>
                {
                    if (!BuildToolV2Utilities.TryOpenSceneSingle(context.PrimaryScenePath, out string message))
                    {
                        Debug.LogError(message);
                        return;
                    }

                    BuildToolV2Utilities.CreateBGSetupInCurrentScene();
                },
            });
        }
    }

    internal sealed class AdsLogicValidator : IBuildValidator
    {
        public string Name => nameof(AdsLogicValidator);
        public string DisplayName => "AdsLogic / NET";
        public string Description => "Kiểm tra AdsLogic hoặc NET ở scene 0. Nếu thiếu, báo lỗi trực tiếp tại scene 0 và gợi ý người dùng tự sửa scene này hoặc tự đặt scene đúng lên vị trí 0.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (!context.TryLoadPrimarySceneForValidation(out string errorMessage))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "ADSLOGIC_SCENE_OPEN_FAILED",
                    Severity = BuildValidationSeverity.Error,
                    Message = errorMessage,
                });
                return;
            }

            AdsLogic adsLogic = context.FindFirstInPrimaryScene<AdsLogic>();
            if (adsLogic != null)
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "ADSLOGIC_MISSING",
                Severity = BuildValidationSeverity.Error,
                Message = "Scene 0 hiện tại không có AdsLogic/NET prefab.",
                Details = "Hãy tự kiểm tra scene đúng cần đứng ở vị trí 0 trong Build Settings, hoặc bổ sung AdsLogic/NET để scene 0 hiện tại khớp với cấu hình mong muốn.",
                FixLabel = "Ensure NET",
                FixAction = () =>
                {
                    if (!BuildToolV2Utilities.TryOpenSceneSingle(context.PrimaryScenePath, out string openError))
                    {
                        Debug.LogError(openError);
                        return;
                    }

                    if (!BuildToolV2Utilities.EnsureNetPrefabInCurrentScene(out string message))
                        Debug.LogError(message);
                },
            });
        }
    }

    internal sealed class DebugOverlayValidator : IBuildValidator
    {
        public string Name => nameof(DebugOverlayValidator);
        public string DisplayName => "Debug Overlay";
        public string Description => "Kiểm tra scene 0 hiện tại có đang thiếu hoặc dư Debug Overlay Canvas so với preset hay không.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (!context.TryLoadPrimarySceneForValidation(out string errorMessage))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "DEBUG_OVERLAY_SCENE_OPEN_FAILED",
                    Severity = BuildValidationSeverity.Error,
                    Message = errorMessage,
                });
                return;
            }

            bool requiresOverlay = context.Profile.RequiresDebugOverlayCanvas;
            GameObject overlayRoot = context.FindFirstGameObjectInPrimaryScene("Debug Overlay Canvas");

            if (!requiresOverlay)
            {
                if (overlayRoot == null)
                    return;

                issues.Add(new BuildValidationIssue
                {
                    Code = "DEBUG_OVERLAY_NOT_ALLOWED",
                    Severity = BuildValidationSeverity.Error,
                    Message = "Preset hiện tại không nên giữ Debug Overlay Canvas trong scene 0.",
                    Details = "Nếu scene 0 hiện tại là scene phát hành, hãy bỏ Debug Overlay Canvas khỏi scene này. Nếu đây chưa phải scene đúng, hãy tự chỉnh lại Build Settings để scene đúng đứng ở vị trí 0.",
                    FixLabel = "Remove Overlay",
                    FixAction = () =>
                    {
                        if (!BuildToolV2Utilities.TryOpenSceneSingle(context.PrimaryScenePath, out string openError))
                        {
                            Debug.LogError(openError);
                            return;
                        }

                        if (!BuildToolV2Utilities.RemoveDebugOverlayInCurrentScene(out string message))
                            Debug.LogError(message);
                    },
                });
                return;
            }

            if (overlayRoot != null)
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "DEBUG_OVERLAY_MISSING",
                Severity = BuildValidationSeverity.Error,
                Message = "Preset hiện tại yêu cầu Debug Overlay Canvas trong scene 0 để mở bảng debug NET.",
                Details = "Hãy tự kiểm tra scene đúng cần đứng ở vị trí 0 trong Build Settings, hoặc bổ sung Debug Overlay Canvas vào scene 0 hiện tại.",
                FixLabel = "Ensure Debug",
                FixAction = () =>
                {
                    if (!BuildToolV2Utilities.TryOpenSceneSingle(context.PrimaryScenePath, out string openError))
                    {
                        Debug.LogError(openError);
                        return;
                    }

                    if (!BuildToolV2Utilities.EnsureDebugOverlayInCurrentScene(out string message))
                        Debug.LogError(message);
                },
            });
        }
    }

    internal sealed class AdmobValidator : IBuildValidator
    {
        public string Name => nameof(AdmobValidator);
        public string DisplayName => "Google Mobile Ads";
        public string Description => "Kiểm tra GoogleMobileAdsSettings tồn tại và App ID theo target hiện tại đã được điền đầy đủ.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (context.Profile.IgnoreAdmobValidation)
                return;

            if (!BuildToolV2Utilities.HasGoogleMobileAdsSettings())
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "ADMOB_SETTINGS_MISSING",
                    Severity = BuildValidationSeverity.Warning,
                    Message = "Không tìm thấy GoogleMobileAdsSettings asset.",
                    FixLabel = "Select Asset",
                    FixAction = BuildToolV2Utilities.SelectGoogleMobileAdsSettings,
                });
                return;
            }

            (string androidId, string iosId) = BuildToolV2Utilities.GetAdmobAppIds();
            bool missingForTarget =
                context.Profile.BuildTarget == BuildTarget.Android ? string.IsNullOrWhiteSpace(androidId) :
                context.Profile.BuildTarget == BuildTarget.iOS ? string.IsNullOrWhiteSpace(iosId) :
                false;

            if (!missingForTarget)
                return;

            string label = context.Profile.BuildTarget == BuildTarget.Android ? "Android AdMob App ID" : "iOS AdMob App ID";
            issues.Add(new BuildValidationIssue
            {
                Code = "ADMOB_APPID_MISSING",
                Severity = BuildValidationSeverity.Error,
                Message = $"Thiếu {label} trong GoogleMobileAdsSettings.",
                FixLabel = "Select Asset",
                FixAction = BuildToolV2Utilities.SelectGoogleMobileAdsSettings,
            });
        }
    }

    internal sealed class FirebaseConfigValidator : IBuildValidator
    {
        public string Name => nameof(FirebaseConfigValidator);
        public string DisplayName => "Firebase Config";
        public string Description => "Xác nhận file cấu hình Firebase theo target build hiện tại đang có trong project trước khi export build.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            string configPath = BuildToolV2Utilities.GetFirebaseConfigFilePath(context.Profile.BuildTarget);
            if (!string.IsNullOrWhiteSpace(configPath))
                return;

            string expectedFileName = context.Profile.BuildTarget == BuildTarget.Android
                ? "google-services.json"
                : context.Profile.BuildTarget == BuildTarget.iOS
                    ? "GoogleService-Info.plist"
                    : string.Empty;

            if (string.IsNullOrWhiteSpace(expectedFileName))
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "FIREBASE_CONFIG_MISSING",
                Severity = BuildValidationSeverity.Error,
                Message = $"Thiếu file Firebase config cho {context.Profile.BuildTarget}: {expectedFileName}",
            });
        }
    }

    internal sealed class AdjustValidator : IBuildValidator
    {
        public string Name => nameof(AdjustValidator);
        public string DisplayName => "Adjust";
        public string Description => "Kiểm tra Adjust ở scene 0. Nếu thiếu, báo trực tiếp tại scene 0 và gợi ý người dùng tự sửa scene này hoặc tự đặt scene đúng lên vị trí 0.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (context.Profile.IgnoreAdjustValidation)
                return;

            if (!context.TryLoadPrimarySceneForValidation(out string errorMessage))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "ADJUST_SCENE_OPEN_FAILED",
                    Severity = BuildValidationSeverity.Error,
                    Message = errorMessage,
                });
                return;
            }

            AdjustSdk.Adjust adjust = context.FindFirstInPrimaryScene<AdjustSdk.Adjust>();
            if (adjust == null)
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "ADJUST_MISSING",
                    Severity = BuildValidationSeverity.Warning,
                    Message = "Scene 0 hiện tại không có Adjust prefab.",
                    Details = "Hãy tự kiểm tra scene đúng cần đứng ở vị trí 0 trong Build Settings, hoặc bổ sung Adjust vào scene 0 hiện tại.",
                    FixLabel = "Ensure Adjust",
                    FixAction = () =>
                    {
                        if (!BuildToolV2Utilities.TryOpenSceneSingle(context.PrimaryScenePath, out string openError))
                        {
                            Debug.LogError(openError);
                            return;
                        }

                        if (!BuildToolV2Utilities.EnsureAdjustInCurrentScene(out string message))
                            Debug.LogError(message);
                    },
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(adjust.appToken))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "ADJUST_TOKEN_MISSING",
                    Severity = BuildValidationSeverity.Error,
                    Message = "Adjust đang có trong scene nhưng App Token rỗng.",
                    FixLabel = "Open Adjust",
                    FixAction = () =>
                    {
                        if (!BuildToolV2Utilities.TryOpenSceneSingle(context.PrimaryScenePath, out string openError))
                        {
                            Debug.LogError(openError);
                            return;
                        }

                        if (!BuildToolV2Utilities.TryPingFirstInCurrentScene<AdjustSdk.Adjust>(out string pingError))
                            Debug.LogError(pingError);
                    },
                });
            }
        }
    }

    internal sealed class MaxSdkValidator : IBuildValidator
    {
        public string Name => nameof(MaxSdkValidator);
        public string DisplayName => "MAX SDK";
        public string Description => "Kiểm tra AppLovin MAX đang có SDK Key, đã bật Terms & Privacy Policy Flow, và đã điền Privacy Policy URL.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            if (context.Profile.IgnoreMaxValidation)
                return;

            if (string.IsNullOrWhiteSpace(BuildToolV2Utilities.GetMaxSdkKey()))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MAX_SDKKEY_MISSING",
                    Severity = BuildValidationSeverity.Error,
                    Message = "MAX đang thiếu AppLovin SDK Key.",
                    FixLabel = "Open MAX",
                    FixAction = BuildToolV2Utilities.OpenAppLovinIntegrationManager,
                });
            }

            if (!BuildToolV2Utilities.IsMaxTermsAndPrivacyPolicyFlowEnabled())
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MAX_CONSENT_FLOW_DISABLED",
                    Severity = BuildValidationSeverity.Warning,
                    Message = "MAX chưa bật Terms & Privacy Policy Flow.",
                    FixLabel = "Open MAX",
                    FixAction = BuildToolV2Utilities.OpenAppLovinIntegrationManager,
                });
            }

            if (string.IsNullOrWhiteSpace(BuildToolV2Utilities.GetMaxPrivacyPolicyUrl()))
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "MAX_PRIVACY_URL_MISSING",
                    Severity = BuildValidationSeverity.Warning,
                    Message = "MAX chưa điền Privacy Policy URL cho Consent Flow.",
                    FixLabel = "Open MAX",
                    FixAction = BuildToolV2Utilities.OpenAppLovinIntegrationManager,
                });
            }
        }
    }

    internal sealed class SdkPresetValidator : IBuildValidator
    {
        public string Name => nameof(SdkPresetValidator);
        public string DisplayName => "SDK Preset";
        public string Description => "Doi chieu moi SDK / adapter / environment cua project voi SDK Preset dang chon. Bao cao mismatch, missing va extra.";

        public void Validate(BuildValidationContext context, List<BuildValidationIssue> issues)
        {
            SdkVersionPresetV2 preset = context.Profile?.SdkVersionPreset;
            if (preset == null)
            {
                issues.Add(new BuildValidationIssue
                {
                    Code = "SDK_PRESET_NOT_SELECTED",
                    Severity = BuildValidationSeverity.Info,
                    Message = "No SDK preset selected for this profile - version drift is not being checked.",
                });
                return;
            }

            SdkVersionPresetComparisonEntryV2[] comparisonEntries = BuildToolV2Utilities.CompareSdkVersionPreset(context.Profile, preset);
            var driftLines = new List<string>(comparisonEntries.Length);
            for (int i = 0; i < comparisonEntries.Length; i++)
            {
                SdkVersionPresetComparisonEntryV2 entry = comparisonEntries[i];
                if (entry == null || entry.IsMatch)
                    continue;

                string label = $"{entry.Key.MediationTag} / {entry.Key.CategoryTag} / {entry.Key.Item} / {entry.Key.Platform}";
                if (entry.IsExtraInProject)
                {
                    driftLines.Add($"[EXTRA]    {label} => actual {entry.ActualVersion}, not declared in preset");
                    continue;
                }

                if (!entry.ExistsInProject)
                {
                    driftLines.Add($"[MISSING]  {label} => expected {entry.ExpectedVersion}, missing in project");
                    continue;
                }

                driftLines.Add($"[MISMATCH] {label} => expected {entry.ExpectedVersion}, actual {entry.ActualVersion}");
            }

            if (driftLines.Count == 0)
                return;

            issues.Add(new BuildValidationIssue
            {
                Code = "SDK_PRESET_DRIFT",
                Severity = BuildValidationSeverity.Warning,
                Message = $"SDK Preset {preset.PresetCode} co {driftLines.Count} muc khac biet so voi project.",
                Details = string.Join(Environment.NewLine, driftLines),
                FixLabel = "Ping Preset",
                FixAction = () => BuildToolV2Utilities.PingObject(preset),
            });
        }
    }
}
