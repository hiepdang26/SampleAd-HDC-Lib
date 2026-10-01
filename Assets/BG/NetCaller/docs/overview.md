# NetCallerAPI — overview

> Status: maintained by hand. Update when the public surface changes.

**Role:** stable public API facade into `NetCore` for project code and upper modules.
**Tier:** 1 (Foundation).

## Main files
- `NetCallerAPI.cs` — public entry point (static facade, namespace `BG_Library.NET.API`).
- `NetEventsHub.cs` — delegates / event bridge to `NetCore`.
- `NetHubLocator.cs` — locates the runtime hub.
- `NetCallerBannerPlacement.cs`, `PULayout.cs` — placement / layout value types.
- Binding code in `NetCore` (`NetEventsBinder`) connects this surface to the runtime.

## Tier boundary
API surface and call routing only. Runtime behaviour stays in `NetCore`.

## See also
- This submodule's `README.md` (version, changelog, release rules).
- Full public surface: `../../../../docs/api-surface.md`.
- Project architecture: `../../../../docs/architecture.md`.
