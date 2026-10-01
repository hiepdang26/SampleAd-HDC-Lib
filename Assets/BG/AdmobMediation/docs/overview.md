# AdmobMediation — overview

> Status: maintained by hand. Update when responsibility or main files change.

**Role:** Google Mobile Ads (AdMob) mediation runtime.
**Tier:** 3 (Mediation).

## Main files
- `Admob_MediationManager.cs` — central manager for AdMob mediation lifecycle.
- `Admob_FSGroupController.cs`, `Admob_FSLogic.cs` — fullscreen ad flow.
- `Admob_RectGroupController.cs`, `Admob_RectLogic.cs` — rectangle ad flow.
- `Admob_Info.cs` — runtime metadata model.
- `IAdmob_Configs.cs` — configuration contract.
- `IAdmob_FSAccessAPI.cs` — fullscreen access interface.
- `GoogleMobileAdsConsentController.cs` — consent / privacy flow.

## Tier boundary
AdMob SDK integration boundary. Placement/pacing policy stays in the AdCore tier.
Mirrors `MaxMediation` and `AndroidMediation` for the FS/Rect pattern.

## See also
- This submodule's `README.md` (version, changelog, release rules).
- Project architecture: `../../../../docs/architecture.md`.
