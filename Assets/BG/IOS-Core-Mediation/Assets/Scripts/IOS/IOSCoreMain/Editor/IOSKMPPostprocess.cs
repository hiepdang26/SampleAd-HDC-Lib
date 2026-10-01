#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;

namespace BG_Library.NET.AdCore.MainIOS
{
    public static class IOSKMPPostprocess
    {
        private const string FallbackIosDeploymentTarget = "15.0";
        // Unity's engine libraries (libiPhone-lib.a, baselib.a) are built for the Target SDK only.
        private const string DeviceSupportedPlatforms = "iphoneos";
        private const string SimulatorSupportedPlatforms = "iphonesimulator";
        private const string BridgeLogTag = "[ios-bridge]";
        private const string EmbedDynamicPodsPhaseName = "BG Embed iOS Dynamic Pod Frameworks";
        private const string CopyComposeResourcesPhaseName = "BG Copy KMP Compose Resources";
        private const string GoogleMobileAdsUnityPluginLibraryPath = "Libraries/Plugins/iOS/unity-plugin-library.a";
        private const string SharedXcframeworkPrimaryAssetPath =
            "BG/IOS-Core-Mediation/Assets/Plugins/iOS/Shared.xcframework";
        private const string SharedXcframeworkLegacyAssetPath =
            "BG Lib/IOS-Core-Mediation/Assets/Plugins/iOS/Shared.xcframework";
        private const string NativeBridgePrimaryAssetPath =
            "BG/IOS-Core-Mediation/Assets/Plugins/iOS/NativeAdBridge.mm";
        private const string NativeBridgeLegacyAssetPath =
            "BG Lib/IOS-Core-Mediation/Assets/Plugins/iOS/NativeAdBridge.mm";
        private const string TrackingTransparencyBridgePrimaryAssetPath =
            "BG/IOS-Core-Mediation/Assets/Plugins/iOS/BGAppTrackingTransparencyBridge.mm";
        private const string TrackingTransparencyBridgeLegacyAssetPath =
            "BG Lib/IOS-Core-Mediation/Assets/Plugins/iOS/BGAppTrackingTransparencyBridge.mm";
        private const string SimulatorGoogleMobileAdsUnityPluginAssetPath =
            "BG Lib/IOS-Core-Mediation/Assets/Plugins/iOS/Simulator/unity-plugin-library-arm64-simulator.a.bytes";
        private const string FirebaseUnityPluginLibraryDirectoryPath = "Libraries/Plugins/iOS/Firebase";
        private const string SimulatorFirebaseUnityPluginAssetDirectoryPath =
            "BG Lib/IOS-Core-Mediation/Assets/Plugins/iOS/Simulator/Firebase";
        private const string AllLoadDynamicLookupFlag = "-Wl,-undefined,dynamic_lookup,-all_load";
        private const string DynamicLookupFlag = "-Wl,-undefined,dynamic_lookup";
        private static readonly string[] DynamicPodFrameworkSubpaths =
        {
            "AppLovinSDK/AppLovinSDK.framework",
            "AdjustSignature/AdjustSigSdk.framework",
            "FBAudienceNetwork/FBAudienceNetwork.framework"
        };

        private static readonly string[] DynamicPodFrameworkEmbedNames =
        {
            "AppLovinSDK.xcframework in Embed Frameworks",
            "AdjustSigSdk.xcframework in Embed Frameworks",
            "FBAudienceNetwork.xcframework in Embed Frameworks"
        };

        private static readonly string[] SharedFrameworkSlices =
        {
            "ios-arm64",
            "ios-arm64-simulator"
        };

        private static readonly string[] FirebaseUnityPluginLibraries =
        {
            "libFirebaseCppApp.a",
            "libFirebaseCppAnalytics.a",
            "libFirebaseCppRemoteConfig.a"
        };

        public static void ApplyKMPBuildSettings(BuildTarget target, string buildPath)
        {
            LogBridge($"postprocess start target={target} buildPath={buildPath}");
            if (target != BuildTarget.iOS)
            {
                LogBridge($"postprocess skipped reason=target_not_ios target={target}");
                return;
            }

            LogBridgeAssetStatus();

            string plistPath = Path.Combine(buildPath, "Info.plist");
            if (File.Exists(plistPath))
            {
                var plist = new PlistDocument();
                plist.ReadFromFile(plistPath);
                plist.root.SetBoolean("CADisableMinimumFrameDurationOnPhone", true);
                plist.WriteToFile(plistPath);
            }

            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            RemoveExistingBuildPhase(projectPath, EmbedDynamicPodsPhaseName);
            RemoveExistingBuildPhase(projectPath, CopyComposeResourcesPhaseName);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            bool isSimulatorExport = IsSimulatorExport(projectPath);
            string supportedPlatforms = isSimulatorExport ? SimulatorSupportedPlatforms : DeviceSupportedPlatforms;
            string deploymentTarget = ResolveIosDeploymentTarget();
            LogBridge(
                $"postprocess applying deploymentTarget={deploymentTarget} simulatorExport={isSimulatorExport} supportedPlatforms={supportedPlatforms}");

            ApplyIosOnlyBuildSettings(project, project.GetUnityMainTargetGuid(), deploymentTarget, supportedPlatforms);
            ApplyIosOnlyBuildSettings(project, project.GetUnityFrameworkTargetGuid(), deploymentTarget, supportedPlatforms);
            AddRequiredSystemFrameworks(project, project.GetUnityFrameworkTargetGuid());
            AddDynamicPodFrameworkEmbedPhase(project, projectPath, project.GetUnityMainTargetGuid());
            AddKmpComposeResourcesPhase(project, projectPath, project.GetUnityMainTargetGuid());

            string gameAssemblyTarget = project.TargetGuidByName("GameAssembly");
            if (!string.IsNullOrEmpty(gameAssemblyTarget))
                ApplyIosOnlyBuildSettings(project, gameAssemblyTarget, deploymentTarget, supportedPlatforms);

            project.WriteToFile(projectPath);
            RemoveRedundantDynamicPodFrameworkEmbedPhase(projectPath);
            RemoveAllLoadLinkerFlag(projectPath);

            if (isSimulatorExport)
            {
                ReplaceGoogleMobileAdsUnityPluginForSimulator(buildPath);
                ReplaceFirebaseUnityPluginLibrariesForSimulator(buildPath);
            }

            LogBridge("postprocess finished");
        }

        private static void LogBridge(string message)
        {
            UnityEngine.Debug.Log($"{BridgeLogTag} {message}");
        }

        private static void WarnBridge(string message)
        {
            UnityEngine.Debug.LogWarning($"{BridgeLogTag} {message}");
        }

        private static void LogBridgeAssetStatus()
        {
            string sharedPath = ResolveFirstExistingAssetPath(
                SharedXcframeworkPrimaryAssetPath,
                SharedXcframeworkLegacyAssetPath);
            string nativeBridgePath = ResolveFirstExistingAssetPath(
                NativeBridgePrimaryAssetPath,
                NativeBridgeLegacyAssetPath);
            string trackingTransparencyBridgePath = ResolveFirstExistingAssetPath(
                TrackingTransparencyBridgePrimaryAssetPath,
                TrackingTransparencyBridgeLegacyAssetPath);

            if (Directory.Exists(sharedPath))
            {
                LogBridge($"Shared.xcframework found path={sharedPath}");
                foreach (string slice in SharedFrameworkSlices)
                {
                    string binaryPath = Path.Combine(sharedPath, slice, "Shared.framework", "Shared");
                    if (File.Exists(binaryPath))
                        LogBridge($"Shared.xcframework slice found slice={slice} binary={binaryPath}");
                    else
                        WarnBridge($"Shared.xcframework slice missing slice={slice} binary={binaryPath}");
                }
            }
            else
            {
                WarnBridge($"Shared.xcframework missing path={sharedPath}");
            }

            if (File.Exists(nativeBridgePath))
                LogBridge($"NativeAdBridge.mm found path={nativeBridgePath}");
            else
                WarnBridge($"NativeAdBridge.mm missing path={nativeBridgePath}");

            if (File.Exists(trackingTransparencyBridgePath))
                LogBridge($"BGAppTrackingTransparencyBridge.mm found path={trackingTransparencyBridgePath}");
            else
                WarnBridge($"BGAppTrackingTransparencyBridge.mm missing path={trackingTransparencyBridgePath}");
        }

        private static void ApplyIosOnlyBuildSettings(
            PBXProject project,
            string targetGuid,
            string deploymentTarget,
            string supportedPlatforms)
        {
            if (string.IsNullOrEmpty(targetGuid))
                return;

            project.SetBuildProperty(targetGuid, "IPHONEOS_DEPLOYMENT_TARGET", deploymentTarget);
            project.SetBuildProperty(targetGuid, "SUPPORTED_PLATFORMS", supportedPlatforms);
            project.SetBuildProperty(targetGuid, "SUPPORTS_MACCATALYST", "NO");
            project.SetBuildProperty(targetGuid, "SUPPORTS_MAC_DESIGNED_FOR_IPHONE_IPAD", "NO");
            project.SetBuildProperty(
                targetGuid,
                "LD_RUNPATH_SEARCH_PATHS",
                "$(inherited) @executable_path/Frameworks @loader_path/Frameworks");
        }

        private static string ResolveFirstExistingAssetPath(params string[] assetPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                string fullPath = Path.Combine(UnityEngine.Application.dataPath, assetPath);
                if (File.Exists(fullPath) || Directory.Exists(fullPath))
                    return fullPath;
            }

            return Path.Combine(UnityEngine.Application.dataPath, assetPaths[0]);
        }

        private static void AddRequiredSystemFrameworks(PBXProject project, string unityFrameworkTargetGuid)
        {
            if (string.IsNullOrEmpty(unityFrameworkTargetGuid))
                return;

            project.AddFrameworkToProject(unityFrameworkTargetGuid, "AppTrackingTransparency.framework", true);
        }

        private static string ResolveIosDeploymentTarget()
        {
            string configured = PlayerSettings.iOS.targetOSVersionString;
            return string.IsNullOrWhiteSpace(configured)
                ? FallbackIosDeploymentTarget
                : configured.Trim();
        }

        private static void AddDynamicPodFrameworkEmbedPhase(PBXProject project, string projectPath, string mainTargetGuid)
        {
            if (string.IsNullOrEmpty(mainTargetGuid))
                return;

            string[] frameworkSubpaths = ResolveDynamicPodFrameworksToEmbed(projectPath);
            if (frameworkSubpaths.Length == 0)
            {
                LogBridge(
                    "Skipped BG dynamic pod framework embed phase because Xcode Embed Frameworks already handles dynamic pod frameworks.");
                return;
            }

            project.AddShellScriptBuildPhase(
                mainTargetGuid,
                EmbedDynamicPodsPhaseName,
                "/bin/sh",
                BuildDynamicPodFrameworkEmbedScript(frameworkSubpaths));
        }

        private static void AddKmpComposeResourcesPhase(PBXProject project, string projectPath, string mainTargetGuid)
        {
            if (string.IsNullOrEmpty(mainTargetGuid))
                return;

            project.AddShellScriptBuildPhase(
                mainTargetGuid,
                CopyComposeResourcesPhaseName,
                "/bin/sh",
                BuildKmpComposeResourcesScript());
        }

        private static bool IsSimulatorExport(string projectPath)
        {
            if (PlayerSettings.iOS.sdkVersion == iOSSdkVersion.SimulatorSDK)
                return true;

            if (!File.Exists(projectPath))
                return false;

            return File.ReadAllText(projectPath).Contains("SUPPORTED_PLATFORMS = iphonesimulator");
        }

        private static void ReplaceGoogleMobileAdsUnityPluginForSimulator(string buildPath)
        {
            string sourcePath = Path.Combine(UnityEngine.Application.dataPath, SimulatorGoogleMobileAdsUnityPluginAssetPath);
            string destinationPath = Path.Combine(buildPath, GoogleMobileAdsUnityPluginLibraryPath);

            if (!File.Exists(sourcePath))
            {
                WarnBridge(
                    $"Missing simulator Google Mobile Ads Unity plugin library at {sourcePath}.");
                return;
            }

            if (!File.Exists(destinationPath))
            {
                WarnBridge(
                    $"Missing exported Google Mobile Ads Unity plugin library at {destinationPath}.");
                return;
            }

            File.Copy(sourcePath, destinationPath, true);
            LogBridge(
                $"Replaced Google Mobile Ads Unity plugin library with arm64 simulator build: {destinationPath}");
        }

        private static void ReplaceFirebaseUnityPluginLibrariesForSimulator(string buildPath)
        {
            foreach (string libraryName in FirebaseUnityPluginLibraries)
            {
                string sourcePath = Path.Combine(
                    UnityEngine.Application.dataPath,
                    SimulatorFirebaseUnityPluginAssetDirectoryPath,
                    $"{libraryName}.bytes");
                string destinationPath = Path.Combine(
                    buildPath,
                    FirebaseUnityPluginLibraryDirectoryPath,
                    libraryName);

                if (!File.Exists(sourcePath))
                {
                    WarnBridge($"Missing simulator Firebase Unity plugin library at {sourcePath}.");
                    continue;
                }

                if (!File.Exists(destinationPath))
                {
                    WarnBridge($"Missing exported Firebase Unity plugin library at {destinationPath}.");
                    continue;
                }

                File.Copy(sourcePath, destinationPath, true);
                LogBridge($"Replaced Firebase Unity plugin library with arm64 simulator build: {destinationPath}");
            }
        }

        private static void RemoveAllLoadLinkerFlag(string projectPath)
        {
            if (!File.Exists(projectPath))
                return;

            string projectText = File.ReadAllText(projectPath);
            string updatedProjectText = projectText.Replace(AllLoadDynamicLookupFlag, DynamicLookupFlag);

            if (updatedProjectText == projectText)
                return;

            File.WriteAllText(projectPath, updatedProjectText);
            LogBridge("Removed -all_load from iOS linker flags to avoid static framework duplicate symbols.");
        }

        private static string[] ResolveDynamicPodFrameworksToEmbed(string projectPath)
        {
            string projectText = File.Exists(projectPath) ? File.ReadAllText(projectPath) : string.Empty;
            var frameworkSubpaths = new List<string>(DynamicPodFrameworkSubpaths.Length);

            for (int i = 0; i < DynamicPodFrameworkSubpaths.Length; i++)
            {
                string embedName = i < DynamicPodFrameworkEmbedNames.Length ? DynamicPodFrameworkEmbedNames[i] : string.Empty;
                if (!string.IsNullOrEmpty(embedName) && projectText.Contains(embedName))
                    continue;

                frameworkSubpaths.Add(DynamicPodFrameworkSubpaths[i]);
            }

            return frameworkSubpaths.ToArray();
        }

        private static void RemoveRedundantDynamicPodFrameworkEmbedPhase(string projectPath)
        {
            if (!File.Exists(projectPath))
                return;

            string projectText = File.ReadAllText(projectPath);
            if (!projectText.Contains(EmbedDynamicPodsPhaseName) || !ProjectEmbedsAllDynamicPodFrameworks(projectText))
                return;

            string updatedProjectText = RemoveBuildPhaseReferenceLine(projectText, EmbedDynamicPodsPhaseName);
            updatedProjectText = RemoveBuildPhaseBlock(updatedProjectText, EmbedDynamicPodsPhaseName);
            if (updatedProjectText == projectText)
                return;

            File.WriteAllText(projectPath, updatedProjectText);
            LogBridge("Removed redundant BG dynamic pod framework embed phase from iOS Xcode project.");
        }

        private static bool ProjectEmbedsAllDynamicPodFrameworks(string projectText)
        {
            foreach (string embedName in DynamicPodFrameworkEmbedNames)
            {
                if (!projectText.Contains(embedName))
                    return false;
            }

            return true;
        }

        private static string RemoveBuildPhaseReferenceLine(string projectText, string phaseName)
        {
            string[] lines = projectText.Split('\n');
            var keptLines = new List<string>(lines.Length);
            string marker = $"/* {phaseName} */";

            foreach (string line in lines)
            {
                if (line.Contains(marker) && !line.Contains($"{marker} = {{"))
                    continue;

                keptLines.Add(line);
            }

            return string.Join("\n", keptLines);
        }

        private static string RemoveBuildPhaseBlock(string projectText, string phaseName)
        {
            string marker = $"/* {phaseName} */ = {{";
            int markerIndex = projectText.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0)
                return projectText;

            int blockStart = projectText.LastIndexOf('\n', markerIndex);
            blockStart = blockStart < 0 ? markerIndex : blockStart + 1;

            string endMarker = "\n\t\t};";
            int blockEnd = projectText.IndexOf(endMarker, markerIndex, StringComparison.Ordinal);
            if (blockEnd < 0)
                return projectText;

            blockEnd += endMarker.Length;
            if (blockEnd < projectText.Length && projectText[blockEnd] == '\n')
                blockEnd++;

            return projectText.Remove(blockStart, blockEnd - blockStart);
        }

        private static string BuildDynamicPodFrameworkArguments(string[] frameworkSubpaths)
        {
            string arguments = string.Empty;
            foreach (string frameworkSubpath in frameworkSubpaths)
                arguments += $" \"{frameworkSubpath}\"";

            return arguments;
        }

        private static string BuildDynamicPodFrameworkEmbedScript(string[] frameworkSubpaths)
        {
            return string.Join("\n", new[]
            {
                "set -e",
                "",
                "BG_XCFRAMEWORKS_BUILD_DIR=\"${PODS_XCFRAMEWORKS_BUILD_DIR:-${BUILT_PRODUCTS_DIR}/XCFrameworkIntermediates}\"",
                "BG_FRAMEWORKS_DIR=\"${TARGET_BUILD_DIR}/${FRAMEWORKS_FOLDER_PATH}\"",
                "mkdir -p \"${BG_FRAMEWORKS_DIR}\"",
                "",
                "for BG_FRAMEWORK_SUBPATH in" + BuildDynamicPodFrameworkArguments(frameworkSubpaths) + "; do",
                "    BG_SOURCE_FRAMEWORK=\"${BG_XCFRAMEWORKS_BUILD_DIR}/${BG_FRAMEWORK_SUBPATH}\"",
                "    if [ ! -d \"${BG_SOURCE_FRAMEWORK}\" ]; then",
                "        echo \"[ios-bridge] ${BG_FRAMEWORK_SUBPATH} not found; skip embed\"",
                "        continue",
                "    fi",
                "",
                "    BG_DEST_FRAMEWORK=\"${BG_FRAMEWORKS_DIR}/$(basename \"${BG_SOURCE_FRAMEWORK}\")\"",
                "    echo \"[ios-bridge] Embedding ${BG_SOURCE_FRAMEWORK}\"",
                "    rsync -av --delete --exclude \"Headers\" --exclude \"PrivateHeaders\" \"${BG_SOURCE_FRAMEWORK}\" \"${BG_FRAMEWORKS_DIR}/\"",
                "",
                "    if [ \"${CODE_SIGNING_ALLOWED:-NO}\" = \"YES\" ] && [ -n \"${EXPANDED_CODE_SIGN_IDENTITY:-}\" ]; then",
                "        /usr/bin/codesign --force --sign \"${EXPANDED_CODE_SIGN_IDENTITY}\" --preserve-metadata=identifier,entitlements,flags --timestamp=none \"${BG_DEST_FRAMEWORK}\"",
                "    fi",
                "done",
                ""
            });
        }

        private static string BuildKmpComposeResourcesScript()
        {
            return string.Join("\n", new[]
            {
                "set -e",
                "",
                "BG_SHARED_XCFRAMEWORK=\"\"",
                "for BG_CANDIDATE in \\",
                "    \"${PROJECT_DIR}/Frameworks/BG/IOS-Core-Mediation/Assets/Plugins/iOS/Shared.xcframework\" \\",
                "    \"${PROJECT_DIR}/Libraries/BG/IOS-Core-Mediation/Assets/Plugins/iOS/Shared.xcframework\" \\",
                "    \"${PROJECT_DIR}/Frameworks/BG Lib/IOS-Core-Mediation/Assets/Plugins/iOS/Shared.xcframework\" \\",
                "    \"${PROJECT_DIR}/Libraries/BG Lib/IOS-Core-Mediation/Assets/Plugins/iOS/Shared.xcframework\" \\",
                "    \"${PROJECT_DIR}/Frameworks/Plugins/iOS/Shared.xcframework\" \\",
                "    \"${PROJECT_DIR}/Libraries/Plugins/iOS/Shared.xcframework\" \\",
                "    \"${PROJECT_DIR}/Frameworks/AdsMultiplatform/Plugins/iOS/Shared.xcframework\" \\",
                "    \"${PROJECT_DIR}/Libraries/AdsMultiplatform/Plugins/iOS/Shared.xcframework\"; do",
                "    if [ -d \"${BG_CANDIDATE}\" ]; then",
                "        BG_SHARED_XCFRAMEWORK=\"${BG_CANDIDATE}\"",
                "        break",
                "    fi",
                "done",
                "",
                "if [ -z \"${BG_SHARED_XCFRAMEWORK}\" ]; then",
                "    echo \"[ios-bridge] Shared.xcframework not found; skip KMP compose resources\"",
                "    exit 0",
                "fi",
                "",
                "if [[ \"${PLATFORM_NAME}\" == *simulator* ]]; then",
                "    BG_SHARED_IDENTIFIER=\"ios-arm64-simulator\"",
                "else",
                "    BG_SHARED_IDENTIFIER=\"ios-arm64\"",
                "fi",
                "",
                "BG_SOURCE_RESOURCES=\"${BG_SHARED_XCFRAMEWORK}/${BG_SHARED_IDENTIFIER}/Shared.framework/composeResources\"",
                "BG_APP_RESOURCES=\"${TARGET_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/compose-resources/composeResources\"",
                "BG_CTA_RESOURCE=\"adsmultiplatform.shared.generated.resources/drawable/bg_btn_bg005_02.png\"",
                "BG_CLOSE_RESOURCE=\"adsmultiplatform.shared.generated.resources/drawable/ic_close.xml\"",
                "if [ ! -d \"${BG_SOURCE_RESOURCES}\" ]; then",
                "    echo \"[ios-bridge] ${BG_SOURCE_RESOURCES} not found; skip KMP compose resources\"",
                "    exit 0",
                "fi",
                "",
                "copy_bg_compose_resources() {",
                "    BG_DEST_RESOURCES=\"$1\"",
                "    mkdir -p \"${BG_DEST_RESOURCES}\"",
                "    echo \"[ios-bridge] Copying KMP compose resources from ${BG_SOURCE_RESOURCES} to ${BG_DEST_RESOURCES}\"",
                "    rsync -a --delete \"${BG_SOURCE_RESOURCES}/\" \"${BG_DEST_RESOURCES}/\"",
                "}",
                "",
                "copy_bg_compose_resources \"${BG_APP_RESOURCES}\"",
                "for BG_FRAMEWORK_RESOURCES in \\",
                "    \"${TARGET_BUILD_DIR}/${FRAMEWORKS_FOLDER_PATH}/Shared.framework/composeResources\" \\",
                "    \"${TARGET_BUILD_DIR}/${UNLOCALIZED_RESOURCES_FOLDER_PATH}/Frameworks/Shared.framework/composeResources\" \\",
                "    \"${BUILT_PRODUCTS_DIR}/Shared.framework/composeResources\"; do",
                "    BG_FRAMEWORK_DIR=\"$(dirname \"${BG_FRAMEWORK_RESOURCES}\")\"",
                "    if [ -d \"${BG_FRAMEWORK_DIR}\" ]; then",
                "        copy_bg_compose_resources \"${BG_FRAMEWORK_RESOURCES}\"",
                "    fi",
                "done",
                "",
                "if [ ! -f \"${BG_APP_RESOURCES}/${BG_CTA_RESOURCE}\" ]; then",
                "    echo \"[ios-bridge] Required KMP CTA drawable missing after copy: ${BG_APP_RESOURCES}/${BG_CTA_RESOURCE}\"",
                "    exit 1",
                "fi",
                "",
                "if [ ! -f \"${BG_APP_RESOURCES}/${BG_CLOSE_RESOURCE}\" ]; then",
                "    echo \"[ios-bridge] Required KMP close drawable missing after copy: ${BG_APP_RESOURCES}/${BG_CLOSE_RESOURCE}\"",
                "    exit 1",
                "fi",
                "",
                "echo \"[ios-bridge] Verified KMP compose resources: ${BG_APP_RESOURCES}\"",
                ""
            });
        }

        private static void RemoveExistingBuildPhase(string projectPath, string phaseName)
        {
            if (!File.Exists(projectPath))
                return;

            string projectText = File.ReadAllText(projectPath);
            if (!projectText.Contains(phaseName))
                return;

            string updatedProjectText = RemoveBuildPhaseReferenceLine(projectText, phaseName);
            updatedProjectText = RemoveBuildPhaseBlock(updatedProjectText, phaseName);
            if (updatedProjectText == projectText)
                return;

            File.WriteAllText(projectPath, updatedProjectText);
            LogBridge($"Removed stale {phaseName} build phase from iOS Xcode project.");
        }
    }
}
#endif
