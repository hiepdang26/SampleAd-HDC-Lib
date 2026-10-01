using UnityEngine;

namespace BG_Library.BuildToolV2
{
    internal static class BuildToolWindowV2LifecycleCoordinator
    {
        internal static void OnEnable(BuildToolWindowV2 owner)
        {
            owner.EnsureDefaultProfiles();
            owner.RefreshProfileCache();
            owner.RestoreSelectedProfile();
            RefreshWindowData(owner, true, refreshDevices: true, refreshValidation: false);
            owner.MarkValidationScanStale(clearResult: owner.validationResult == null);
        }

        internal static void RefreshSummaryDraft(BuildToolWindowV2 owner, bool force = false)
        {
            owner.summaryDraft = BuildToolV2SummaryLogic.RefreshDraft(owner.profile, owner.cachedDraftProfile, owner.summaryDraft, force);
            owner.cachedDraftProfile = owner.profile;
        }

        internal static void RefreshWindowData(BuildToolWindowV2 owner, bool refreshSdk, bool refreshDevices = false, bool refreshValidation = false)
        {
            BuildToolWindowV2DisplayState.ReloadDisplayState(owner, refreshSdk);
            owner.RefreshScrcpySettings();
            if (refreshDevices)
                owner.RefreshDeviceCache();
            RefreshSummaryDraft(owner, true);
            owner.RefreshIconSelectionFromCurrent(true);
            if (refreshValidation)
                owner.RefreshChecks();
            owner.Repaint();
        }

        internal static void HandleRefreshShortcut(BuildToolWindowV2 owner)
        {
            Event currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type != EventType.KeyDown)
                return;

            if (!currentEvent.control || currentEvent.keyCode != KeyCode.R)
                return;

            RefreshWindowData(owner, true, refreshDevices: true, refreshValidation: true);
            currentEvent.Use();
        }
    }
}
