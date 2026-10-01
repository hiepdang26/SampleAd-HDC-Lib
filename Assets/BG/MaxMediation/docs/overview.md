# MaxMediation — overview

> Status: maintained by hand. Update when responsibility or main files change.

**Role:** AppLovin MAX mediation runtime.
**Tier:** 3 (Mediation).

## Main files
- `Max_MediationManager.cs` — main runtime manager for MAX mediation.
- `Max_FSGroupController.cs`, `Max_FSLogic.cs` — fullscreen ad flow.
- `Max_RectGroupController.cs`, `Max_RectLogic.cs` — rectangle ad flow.
- `Max_Info.cs` — runtime metadata model.
- `IMax_Configs.cs` — configuration contract.
- `IMax_FSAccessAPI.cs`, `IMax_RectAccessAPI.cs` — access interfaces.

## Tier boundary
AppLovin MAX SDK integration boundary. Placement/pacing policy stays in the AdCore tier.
Mirrors `AdmobMediation` and `AndroidMediation` for the FS/Rect pattern.

## See also
- This submodule's `README.md` (version, changelog, release rules).
- Project architecture: `../../../../docs/architecture.md`.
