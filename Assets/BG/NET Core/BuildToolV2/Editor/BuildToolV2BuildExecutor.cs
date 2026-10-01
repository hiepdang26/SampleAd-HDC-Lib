using BG_Library.NET.AdSystem;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolV2BuildExecutor
    {
        public static BuildExecutionResult Run(BuildProfileV2 profile, bool buildAndRun, string preferredDeviceSerial = null)
        {
            var executionResult = new BuildExecutionResult
            {
                Validation = BuildToolV2ValidationRunner.Run(profile),
            };

            if (buildAndRun && profile != null && profile.BuildTarget == BuildTarget.Android)
            {
                BuildAndroidDeviceInfo targetDevice = ResolvePreferredDevice(preferredDeviceSerial);
                if (targetDevice != null)
                {
                    executionResult.RunDeviceSerial = targetDevice.Serial;
                    executionResult.RunDeviceSummary = targetDevice.Summary;
                }
                else
                {
                    executionResult.RunDeviceSerial = preferredDeviceSerial ?? string.Empty;
                    executionResult.RunDeviceSummary = "No Android device detected at build start.";
                }
            }

            if (!executionResult.Validation.CanBuild)
            {
                executionResult.ErrorMessage = "Validation failed.";
                return executionResult;
            }

            BuildSharedStateSnapshot sharedStateSnapshot = BuildToolV2SharedStateSnapshot.Capture(profile);
            var context = new BuildValidationContext(profile);
            if (!context.TryOpenPrimarySceneForBuild(out string errorMessage))
            {
                executionResult.ErrorMessage = errorMessage;
                return executionResult;
            }

            try
            {
                NetConfigsSO netConfigs = profile.ResolveNetConfigs();
                BuildScenePreparationResult preparation = BuildToolV2BuildPreparation.Prepare(profile, netConfigs);
                executionResult.PreparationSummary = preparation.Summary;
                executionResult.PreparationSteps = preparation.Steps;
                executionResult.SavedScenePaths = preparation.SavedScenePaths;
                executionResult.SavedAssetPaths = preparation.SavedAssetPaths;
                if (!preparation.Succeeded)
                {
                    executionResult.ErrorMessage = preparation.ErrorMessage;
                    return executionResult;
                }

                string outputPath = BuildToolV2OutputTools.GetOutputPath(profile, netConfigs);
                if (string.IsNullOrWhiteSpace(outputPath))
                {
                    executionResult.WasCancelled = true;
                    executionResult.ErrorMessage = "Da huy chon output path.";
                    return executionResult;
                }

                BuildToolV2ProfileSettingsApplier.Apply(profile);

                BuildOptions options = BuildOptions.StrictMode;
                if (profile.DevelopmentBuild)
                    options |= BuildOptions.Development;
                if (buildAndRun)
                    options |= BuildOptions.AutoRunPlayer | BuildOptions.ShowBuiltPlayer;

                var buildPlayerOptions = new BuildPlayerOptions
                {
                    scenes = BuildToolV2SceneNavigation.GetEnabledScenes(),
                    locationPathName = outputPath,
                    target = profile.BuildTarget,
                    targetGroup = BuildPipeline.GetBuildTargetGroup(profile.BuildTarget),
                    options = options,
                };

                executionResult.OutputPath = outputPath;
                executionResult.Report = BuildPipeline.BuildPlayer(buildPlayerOptions);

                if (profile.GenerateReportArtifacts && executionResult.Report != null)
                    BuildToolV2ReportWriter.WriteArtifacts(profile, netConfigs, executionResult);

                if (executionResult.Succeeded)
                    BuildToolV2OutputTools.OpenAndSelectPath(outputPath);

                return executionResult;
            }
            finally
            {
                BuildToolV2SharedStateSnapshot.Restore(sharedStateSnapshot);
            }
        }

        private static BuildAndroidDeviceInfo ResolvePreferredDevice(string preferredDeviceSerial)
        {
            BuildAndroidDeviceInfo[] devices = BuildToolV2Utilities.GetConnectedAndroidDevices();
            if (devices.Length == 0)
                return null;

            if (!string.IsNullOrWhiteSpace(preferredDeviceSerial))
            {
                for (int i = 0; i < devices.Length; i++)
                {
                    if (string.Equals(devices[i].Serial, preferredDeviceSerial, System.StringComparison.OrdinalIgnoreCase))
                        return devices[i];
                }
            }

            for (int i = 0; i < devices.Length; i++)
            {
                if (string.Equals(devices[i].State, "device", System.StringComparison.OrdinalIgnoreCase))
                    return devices[i];
            }

            return devices[0];
        }
    }
}
