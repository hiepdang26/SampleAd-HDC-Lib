using System.IO;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2ReportActions
    {
        internal static void GenerateReport(BuildToolWindowV2 owner)
        {
            if (owner.profile == null)
                return;

            owner.RefreshChecks();
            if (owner.validationResult == null)
                return;

            string outputDirectory = owner.profile.OutputDirectory;
            if (string.IsNullOrWhiteSpace(outputDirectory) || !Directory.Exists(outputDirectory))
            {
                outputDirectory = ChooseOutputDirectory(owner);
                if (string.IsNullOrWhiteSpace(outputDirectory))
                    return;
            }

            var executionResult = new BuildExecutionResult
            {
                Validation = owner.validationResult,
                IsReportOnly = true,
                ResultLabel = "Report only",
            };

            BuildToolV2ReportWriter.WriteSnapshotArtifacts(owner.profile, owner.profile.ResolveNetConfigs(), owner.validationResult, outputDirectory, executionResult);
            owner.lastExecutionResult = executionResult;
            if (!string.IsNullOrWhiteSpace(executionResult.SummaryPath))
                BuildToolV2Utilities.OpenAndSelectPath(executionResult.SummaryPath);
            owner.Repaint();
        }

        internal static string ChooseOutputDirectory(BuildToolWindowV2 owner)
        {
            string directory = BuildToolV2Utilities.ChooseOutputDirectory(owner.profile);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                BuildToolV2Utilities.ClearEditorCaches();
                owner.MarkValidationScanStale();
                owner.lastExecutionResult = null;
                owner.ReloadHeaderOverviewState();
                owner.Repaint();
            }

            return directory;
        }
    }
}
