using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BG_Library.NET.AdSystem;
using BG_Library.NET.Debug;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    [Serializable]
    internal sealed class BuildArtifactIssue
    {
        public string code;
        public string severity;
        public string message;
        public string details;
    }

    [Serializable]
    internal sealed class BuildArtifactReport
    {
        public string generatedAtLocal;
        public string buildDisplayName;
        public string profileName;
        public string preset;
        public string unityVersion;
        public string productName;
        public string version;
        public int versionCode;
        public string packageName;
        public string buildTarget;
        public string buildArtifactType;
        public string result;
        public string outputPath;
        public string machineName;
        public string localIpAddress;
        public string runDeviceSerial;
        public string runDeviceSummary;
        public string preparationSummary;
        public string[] preparationSteps;
        public string[] savedScenePaths;
        public string[] savedAssetPaths;
        public bool developmentBuild;
        public bool buildAppBundle;
        public bool splitApplicationBinary;
        public bool applyNetSetupBeforeBuild;
        public bool usesSandboxAdjust;
        public bool usesDebugMode;
        public string debugPreset;
        public string presetDebugPreset;
        public bool releaseDebugEnabled;
        public bool requiresDebugOverlayCanvas;
        public bool shouldEnableSrDebugger;
        public bool hasManagedConfigOverrides;
        public string[] managedOverrideSummaries;
        public string changeLog;
        public string netConfigsAssetPath;
        public string netSetupConfigType;
        public bool netBuildHack;
        public bool netDebugEnabled;
        public bool netAdmobTestDevice;
        public bool netAdmobTestIds;
        public string adjustEventIap;
        public string sdkPresetCode;
        public string sdkPresetNotes;
        public string[] sdkPresetEntries;
        public int totalWarnings;
        public int totalErrors;
        public ulong totalSizeBytes;
        public double totalTimeSeconds;
        public BuildArtifactIssue[] validationIssues;
    }

    internal static class BuildToolV2ReportWriter
    {
        public static void WriteSnapshotArtifacts(BuildProfileV2 profile, NetConfigsSO netConfigs, BuildValidationResult validation, string outputDirectory, BuildExecutionResult executionResult)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory))
                return;

            Directory.CreateDirectory(outputDirectory);
            string outputPath = profile.BuildTarget == BuildTarget.iOS
                ? outputDirectory
                : Path.Combine(outputDirectory, BuildToolV2Utilities.GetBuildDisplayName(profile, netConfigs));

            var artifact = CreateArtifact(profile, netConfigs, validation, outputPath, "Snapshot Only", 0UL, 0d, executionResult);
            WriteArtifactFile(outputDirectory, profile, artifact, executionResult);
        }

        public static void WriteArtifacts(BuildProfileV2 profile, NetConfigsSO netConfigs, BuildExecutionResult executionResult)
        {
            BuildReport report = executionResult.Report;
            if (report == null)
                return;

            string outputPath = executionResult.OutputPath ?? report.summary.outputPath;
            string outputDirectory = profile.BuildTarget == BuildTarget.iOS
                ? outputPath
                : Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
                return;

            Directory.CreateDirectory(outputDirectory);
            var artifact = CreateArtifact(
                profile,
                netConfigs,
                executionResult.Validation,
                outputPath,
                report.summary.result.ToString(),
                report.summary.totalSize,
                report.summary.totalTime.TotalSeconds,
                executionResult);
            WriteArtifactFile(outputDirectory, profile, artifact, executionResult);
        }

        private static BuildArtifactReport CreateArtifact(
            BuildProfileV2 profile,
            NetConfigsSO netConfigs,
            BuildValidationResult validation,
            string outputPath,
            string result,
            ulong totalSizeBytes,
            double totalTimeSeconds,
            BuildExecutionResult executionResult)
        {
            SdkVersionPresetV2 sdkPreset = profile.SdkVersionPreset;
            SdkVersionPresetComparisonEntryV2[] sdkPresetComparison = sdkPreset == null
                ? Array.Empty<SdkVersionPresetComparisonEntryV2>()
                : BuildToolV2Utilities.CompareSdkVersionPreset(profile, sdkPreset);

            return new BuildArtifactReport
            {
                generatedAtLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                buildDisplayName = BuildToolV2Utilities.GetBuildDisplayName(profile, netConfigs),
                profileName = profile.DisplayName,
                preset = profile.Preset.ToString(),
                unityVersion = Application.unityVersion,
                productName = profile.ProductName,
                version = profile.ResolveVersion(),
                versionCode = profile.BuildTarget == BuildTarget.Android ? profile.VersionCode : 0,
                packageName = profile.PackageName,
                buildTarget = profile.BuildTarget.ToString(),
                buildArtifactType = profile.BuildTarget == BuildTarget.Android
                    ? (profile.BuildAppBundle ? "AAB" : "APK")
                    : profile.BuildTarget.ToString(),
                result = result,
                outputPath = outputPath,
                machineName = Environment.MachineName,
                localIpAddress = BuildToolV2Utilities.GetLocalIpAddress(),
                runDeviceSerial = executionResult?.RunDeviceSerial ?? string.Empty,
                runDeviceSummary = executionResult?.RunDeviceSummary ?? string.Empty,
                preparationSummary = executionResult?.PreparationSummary ?? "No build preparation executed.",
                preparationSteps = executionResult?.PreparationSteps ?? Array.Empty<string>(),
                savedScenePaths = executionResult?.SavedScenePaths ?? Array.Empty<string>(),
                savedAssetPaths = executionResult?.SavedAssetPaths ?? Array.Empty<string>(),
                developmentBuild = profile.DevelopmentBuild,
                buildAppBundle = profile.BuildAppBundle,
                splitApplicationBinary = profile.BuildTarget == BuildTarget.Android && profile.SplitApplicationBinary,
                applyNetSetupBeforeBuild = profile.ApplyNetSetupBeforeBuild,
                usesSandboxAdjust = profile.UsesSandboxAdjust,
                usesDebugMode = profile.UsesDebugMode,
                debugPreset = profile.DebugPreset.ToString(),
                presetDebugPreset = profile.PresetDebugPreset.ToString(),
                releaseDebugEnabled = profile.ReleaseDebugEnabled,
                requiresDebugOverlayCanvas = profile.RequiresDebugOverlayCanvas,
                shouldEnableSrDebugger = profile.ShouldEnableSRDebugger,
                hasManagedConfigOverrides = profile.HasManagedSourceMismatches(),
                managedOverrideSummaries = profile.GetManagedSourceMismatchSummaries(),
                changeLog = profile.ChangeLog,
                netConfigsAssetPath = netConfigs == null ? string.Empty : AssetDatabase.GetAssetPath(netConfigs),
                netSetupConfigType = netConfigs == null ? string.Empty : netConfigs.SetupConfigNETType.ToString(),
                netBuildHack = profile.BuildHack,
                netDebugEnabled = netConfigs != null && NetFlowDebugSystem.IsEnabledForPreset(netConfigs.Debug_Preset),
                netAdmobTestDevice = netConfigs != null && netConfigs.Admob_TestDevice,
                netAdmobTestIds = netConfigs != null && netConfigs.Admob_TestId,
                adjustEventIap = netConfigs == null ? string.Empty : netConfigs.AdjustEventIAP,
                sdkPresetCode = sdkPreset == null ? string.Empty : sdkPreset.PresetCode,
                sdkPresetNotes = sdkPreset == null ? string.Empty : sdkPreset.Notes,
                sdkPresetEntries = BuildSdkPresetLines(sdkPresetComparison),
                totalWarnings = validation == null ? 0 : validation.WarningCount,
                totalErrors = validation == null ? 0 : validation.ErrorCount,
                totalSizeBytes = totalSizeBytes,
                totalTimeSeconds = totalTimeSeconds,
                validationIssues = ToArtifactIssues(validation),
            };
        }

        private static void WriteArtifactFile(string outputDirectory, BuildProfileV2 profile, BuildArtifactReport artifact, BuildExecutionResult executionResult)
        {
            string baseName = BuildToolV2Utilities.GetBuildDisplayName(profile, profile.ResolveNetConfigs());
            string txtPath = Path.Combine(outputDirectory, baseName + "_buildreport.txt");

            File.WriteAllText(txtPath, BuildTextSummary(artifact), Encoding.UTF8);
            executionResult.SummaryPath = txtPath;
        }

        private static BuildArtifactIssue[] ToArtifactIssues(BuildValidationResult validation)
        {
            if (validation == null || validation.Issues.Count == 0)
                return Array.Empty<BuildArtifactIssue>();

            var issues = new List<BuildArtifactIssue>(validation.Issues.Count);
            for (int i = 0; i < validation.Issues.Count; i++)
            {
                BuildValidationIssue source = validation.Issues[i];
                issues.Add(new BuildArtifactIssue
                {
                    code = source.Code,
                    severity = source.Severity.ToString(),
                    message = source.Message,
                    details = source.Details,
                });
            }

            return issues.ToArray();
        }

        private static string BuildTextSummary(BuildArtifactReport artifact)
        {
            var builder = new StringBuilder(8192);
            builder.AppendLine("BUILD REPORT");
            builder.AppendLine(new string('=', 72));
            builder.AppendLine();

            AppendSectionHeader(builder, "1. Tong quan");
            AppendPair(builder, "Ten file build", artifact.buildDisplayName);
            AppendPair(builder, "Ket qua", artifact.result);
            AppendPair(builder, "Thoi gian tao", artifact.generatedAtLocal);
            AppendPair(builder, "Unity version", artifact.unityVersion);
            AppendPair(builder, "Profile", artifact.profileName);
            AppendPair(builder, "Preset", artifact.preset);
            AppendPair(builder, "Build target", artifact.buildTarget);
            AppendPair(builder, "Loai artifact", artifact.buildArtifactType);
            AppendPair(builder, "Output path", artifact.outputPath);
            AppendPair(builder, "Thoi gian build (s)", artifact.totalTimeSeconds.ToString("F2"));
            AppendPair(builder, "Dung luong build (bytes)", artifact.totalSizeBytes.ToString());
            AppendPair(builder, "Tong warning", artifact.totalWarnings.ToString());
            AppendPair(builder, "Tong error", artifact.totalErrors.ToString());
            builder.AppendLine();

            AppendSectionHeader(builder, "2. Player Settings");
            AppendPair(builder, "Product Name", artifact.productName);
            AppendPair(builder, "Version", artifact.version);
            AppendPair(builder, "Version Code", artifact.versionCode <= 0 ? "-" : artifact.versionCode.ToString());
            AppendPair(builder, "Package Name", DisplayValue(artifact.packageName));
            AppendPair(builder, "Development Build", ToOnOff(artifact.developmentBuild));
            AppendPair(builder, "Build App Bundle", ToOnOff(artifact.buildAppBundle));
            AppendPair(builder, "Split Application Binary", ToOnOff(artifact.splitApplicationBinary));
            builder.AppendLine();

            AppendSectionHeader(builder, "3. Preset va Build Rule");
            AppendPair(builder, "Apply NET setup truoc build", ToOnOff(artifact.applyNetSetupBeforeBuild));
            AppendPair(builder, "Uses Sandbox Adjust", ToOnOff(artifact.usesSandboxAdjust));
            AppendPair(builder, "Build Hack", ToOnOff(artifact.netBuildHack));
            AppendPair(builder, "Debug preset hien tai", artifact.debugPreset);
            AppendPair(builder, "Debug preset mac dinh cua preset", artifact.presetDebugPreset);
            AppendPair(builder, "Co bat debug cho release", ToOnOff(artifact.releaseDebugEnabled));
            AppendPair(builder, "Can Debug Overlay Canvas", ToOnOff(artifact.requiresDebugOverlayCanvas));
            AppendPair(builder, "Can bat SRDebugger", ToOnOff(artifact.shouldEnableSrDebugger));
            AppendPair(builder, "Co override khac preset", ToOnOff(artifact.hasManagedConfigOverrides));
            AppendBulletList(builder, "Danh sach override", artifact.managedOverrideSummaries);
            builder.AppendLine();

            AppendSectionHeader(builder, "4. Thiet bi va moi truong");
            AppendPair(builder, "May build", artifact.machineName);
            AppendPair(builder, "Dia chi IP noi bo", artifact.localIpAddress);
            AppendPair(builder, "Run device serial", DisplayValue(artifact.runDeviceSerial));
            AppendPair(builder, "Run device", DisplayValue(artifact.runDeviceSummary));
            builder.AppendLine();

            AppendSectionHeader(builder, "5. Change Log");
            if (string.IsNullOrWhiteSpace(artifact.changeLog))
                builder.AppendLine("- Khong co change log.");
            else
                builder.AppendLine(artifact.changeLog.Trim());
            builder.AppendLine();

            AppendSectionHeader(builder, "6. NET Config");
            AppendPair(builder, "NetConfigsSO", DisplayValue(artifact.netConfigsAssetPath));
            AppendPair(builder, "SetupConfigNETType", DisplayValue(artifact.netSetupConfigType));
            AppendPair(builder, "NET debug dang bat", ToOnOff(artifact.netDebugEnabled));
            AppendPair(builder, "AdMob Test Device", ToOnOff(artifact.netAdmobTestDevice));
            AppendPair(builder, "AdMob Test IDs", ToOnOff(artifact.netAdmobTestIds));
            builder.AppendLine();

            AppendSectionHeader(builder, "7. SDK Preset");
            if (string.IsNullOrWhiteSpace(artifact.sdkPresetCode))
            {
                builder.AppendLine("- Khong chon SDK Preset. Bo qua check version/adapters theo preset.");
            }
            else
            {
                AppendPair(builder, "Preset", DisplayValue(artifact.sdkPresetCode));
                AppendPair(builder, "Notes", DisplayValue(artifact.sdkPresetNotes));
                AppendBulletList(builder, "Entries", artifact.sdkPresetEntries);
            }
            builder.AppendLine();

            AppendSectionHeader(builder, "8. Chuan bi truoc build");
            AppendPair(builder, "Tong ket", DisplayValue(artifact.preparationSummary));
            AppendBulletList(builder, "Cac buoc da chay", artifact.preparationSteps);
            AppendBulletList(builder, "Scene da save", artifact.savedScenePaths);
            AppendBulletList(builder, "Asset da save", artifact.savedAssetPaths);
            builder.AppendLine();

            AppendSectionHeader(builder, "9. Validation Scan");
            AppendPair(builder, "Tong warning", artifact.totalWarnings.ToString());
            AppendPair(builder, "Tong error", artifact.totalErrors.ToString());
            if (artifact.validationIssues == null || artifact.validationIssues.Length == 0)
            {
                builder.AppendLine("- Khong co issue.");
            }
            else
            {
                for (int i = 0; i < artifact.validationIssues.Length; i++)
                {
                    BuildArtifactIssue issue = artifact.validationIssues[i];
                    builder.AppendLine($"- [{issue.severity}] {issue.message}");
                    if (!string.IsNullOrWhiteSpace(issue.code))
                        builder.AppendLine($"  Code    : {issue.code}");
                    if (!string.IsNullOrWhiteSpace(issue.details))
                        builder.AppendLine($"  Details : {issue.details}");
                }
            }

            return builder.ToString().TrimEnd() + Environment.NewLine;
        }

        private static void AppendPair(StringBuilder builder, string label, object value)
        {
            builder.AppendLine($"{label}: {value}");
        }

        private static void AppendSectionHeader(StringBuilder builder, string title)
        {
            builder.AppendLine(title);
            builder.AppendLine(new string('-', 72));
        }

        private static void AppendBulletList(StringBuilder builder, string label, string[] lines)
        {
            builder.AppendLine(label + ":");
            AppendList(builder, lines);
        }

        private static void AppendList(StringBuilder builder, string[] lines)
        {
            if (lines == null || lines.Length == 0)
            {
                builder.AppendLine("- none");
                return;
            }

            for (int i = 0; i < lines.Length; i++)
                builder.AppendLine($"- {lines[i]}");
        }

        private static string ToOnOff(bool value)
        {
            return value ? "ON" : "OFF";
        }

        private static string DisplayValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "null" : value;
        }

        private static string[] BuildSdkPresetLines(SdkVersionPresetComparisonEntryV2[] comparisonEntries)
        {
            if (comparisonEntries == null || comparisonEntries.Length == 0)
                return Array.Empty<string>();

            var lines = new List<string>(comparisonEntries.Length);
            for (int i = 0; i < comparisonEntries.Length; i++)
            {
                SdkVersionPresetComparisonEntryV2 entry = comparisonEntries[i];
                if (entry == null)
                    continue;

                string status = entry.IsExtraInProject
                    ? "EXTRA"
                    : entry.IsMatch
                        ? "OK"
                        : entry.ExistsInProject
                            ? "MISMATCH"
                            : "MISSING";

                string expectedValue = entry.ExistsInPreset ? DisplayValue(entry.ExpectedVersion) : "(not in preset)";
                string actualValue = entry.ExistsInProject ? DisplayValue(entry.ActualVersion) : "missing";
                lines.Add(
                    $"[{status}] {entry.Key.MediationTag} / {entry.Key.CategoryTag} / {entry.Key.Item} / {entry.Key.Platform} | expected {expectedValue} | actual {actualValue}");
            }

            return lines.ToArray();
        }
    }
}
