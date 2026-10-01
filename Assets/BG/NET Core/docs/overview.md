# NetCore — overview

> Status: maintained by hand. Update when an area's responsibility changes.

**Role:** shared foundation module for the whole BG ad/network stack.
**Tier:** 1 (Foundation).

## Main areas
- `Ads Logic System/` — `AdCoreBase`, `AdsLogic`, `AdsCoreDefault`, `AdSystemConfigs`,
  and `Ad manager/` (the 8 per-ad-type systems).
- `Mediation Scripts/Controller/` — shared mediation base classes.
- `Systems/` — `Tracking/`, `Events/`, `Diagnostics/`.
- `Other modules/` — `RemoteConfig.cs`, `AdjustManager.cs`.
- `Firebase/`, `AdjustWrapper/`, `MainThreadDispatcher/`, `Common/`.
- `BuildToolV2/` — editor build tooling.
- `NetConfigsSO.cs`, `NetEventsBinder.cs` — central config and binding entry points.

## Tier boundary
Shared runtime only. No vendor SDK calls, no project-specific gameplay logic.
Changes here can affect every downstream submodule.

## See also
- This submodule's `README.md` (version, changelog, release rules).
- `Systems/Tracking/TRACKING_MATRIX.md`, `TRACKING_OVERVIEW_CURRENT.md`.
- Project architecture: `../../../../docs/architecture.md`.
