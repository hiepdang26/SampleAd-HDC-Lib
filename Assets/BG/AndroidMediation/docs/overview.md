# AndroidMediation — overview

> Status: maintained by hand. Update when responsibility or main files change.

**Role:** Android-only mediation and native ad support.
**Tier:** 3 (Mediation).

## Main files / folders
- `Android_FSGroupController.cs` — fullscreen ad group control.
- `Android_RectGroupController.cs` — rectangle ad group control.
- `Android_Info.cs` — Android mediation metadata / runtime info.
- `Helper/` — `Android_MediationManager.cs`, `Android_FSLogic.cs`,
  `Android_RectLogic.cs`, `IAndroid_Configs.cs`.
- `NativeAdManager/` — native ad loading, callbacks, instance management.
- `Plugins/` — native plugin files required by the Android build.

## Tier boundary
Android SDK / native ad integration boundary. Placement/pacing policy stays in the
AdCore tier. Mirrors `AdmobMediation` and `MaxMediation` for the FS/Rect pattern.

## See also
- This submodule's `README.md` (version, changelog, release rules).
- Project architecture: `../../../../docs/architecture.md`.
