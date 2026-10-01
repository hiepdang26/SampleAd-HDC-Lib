using UnityEditor;
using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2BuildActions
    {
        internal static void RunBuild(BuildToolWindowV2 owner, bool buildAndRun)
        {
            if (owner.profile == null)
                return;

            owner.RefreshSummaryDraft(true);
            owner.ReloadHeaderOverviewState();
            owner.RefreshChecks();
            if (owner.validationResult != null && !owner.validationResult.CanBuild)
            {
                EditorUtility.DisplayDialog(
                    "Validation Errors",
                    "Validation Scan đang có lỗi. Hãy fix toàn bộ Error trước khi build.",
                    "OK");
                return;
            }

            if (owner.profile.IsReleaseBuildWithDebugWarning)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Release Build With Debug",
                    "Preset release hiện đang bật debug. Build này sẽ giữ debug runtime và thêm hậu tố releaseDebug vào tên file. Bạn có chắc muốn tiếp tục?",
                    "Continue Build",
                    "Cancel");
                if (!confirmed)
                    return;
            }

            BuildAndroidDeviceInfo selectedDevice = buildAndRun ? owner.GetSelectedDevice() : null;
            string preferredSerial = selectedDevice?.Serial;
            owner.lastExecutionResult = BuildToolV2Utilities.RunBuild(owner.profile, buildAndRun, preferredSerial);
            owner.validationResult = owner.lastExecutionResult?.Validation;

            if (buildAndRun
                && selectedDevice != null
                && owner.lastExecutionResult != null
                && owner.lastExecutionResult.Succeeded
                && owner.scrcpySettings != null
                && owner.scrcpySettings.autoOpenAfterBuildAndRun)
            {
                BuildToolV2Utilities.ScheduleScrcpyLaunch(selectedDevice, owner.scrcpySettings, restartIfRunning: true);
                owner.SetScrcpyMessage($"Preview scheduled for {selectedDevice.DisplayName}.", MessageType.Info);
            }

            owner.Repaint();
        }
    }
}
