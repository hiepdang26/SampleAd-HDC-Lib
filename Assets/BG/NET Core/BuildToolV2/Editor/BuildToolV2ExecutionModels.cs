using System.Collections.Generic;
using UnityEditor.Build.Reporting;

namespace BG_Library.BuildToolV2
{
    internal sealed class BuildScenePreparationResult
    {
        public bool Succeeded;
        public string ErrorMessage;
        public string Summary;
        public string[] Steps = System.Array.Empty<string>();
        public string[] SavedScenePaths = System.Array.Empty<string>();
        public string[] SavedAssetPaths = System.Array.Empty<string>();
    }

    internal sealed class BuildExecutionResult
    {
        public BuildValidationResult Validation;
        public BuildReport Report;
        public string PreparationSummary;
        public string[] PreparationSteps;
        public string[] SavedScenePaths;
        public string[] SavedAssetPaths;
        public string OutputPath;
        public string SummaryPath;
        public string ErrorMessage;
        public bool WasCancelled;
        public bool IsReportOnly;
        public string ResultLabel;
        public string RunDeviceSummary;
        public string RunDeviceSerial;
        public bool Succeeded => Report != null && Report.summary.result == BuildResult.Succeeded;
    }

    internal sealed class BuildSharedStateSnapshot
    {
        public bool IsValid;
        public bool DevelopmentBuild;
        public bool BuildAppBundle;
        // MFS_IGNORE PlayerPrefs snapshot — restored alongside the file-backed snapshots so
        // BuildToolV2MfuscatorPreset.Apply does not leak its per-build value past the build.
        public bool MfsIgnoreKeyExisted;
        public string MfsIgnoreOriginalValue = string.Empty;
        public UnityEditor.SceneManagement.SceneSetup[] SceneManagerSetup = System.Array.Empty<UnityEditor.SceneManagement.SceneSetup>();
        public List<BuildSharedStateFileSnapshot> Files = new List<BuildSharedStateFileSnapshot>();
    }

    internal sealed class BuildSharedStateFileSnapshot
    {
        public string RelativePath = string.Empty;
        public string FullPath = string.Empty;
        public bool Existed;
        public byte[] Content = System.Array.Empty<byte>();
    }

    internal sealed class AdjustSceneSnapshot
    {
        public bool Exists;
        public string AppToken;
        public string Environment;
        public bool StartManually;
    }
}
