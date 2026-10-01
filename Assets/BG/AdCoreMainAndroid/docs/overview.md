# AdCore-MainForAndroid — overview

> Status: maintained by hand. Update when responsibility or main files change.

**Role:** main Android ad coordination layer; bridges game logic to Android mediation.
**Tier:** 2 (AdCore).

## Main files
- `AdCore_MainAndroid.cs` — main runtime controller for Android ad flow.
- `AdCore_Configs.cs` — shared configuration model.
- `AdCore MainAndroid.asset` — default ScriptableObject config.
- `AdConfigsValidator.cs` — ad configuration validation.
- Fallback group system — `FsFallbackGroup.cs`, `MrecFallbackGroup.cs`,
  `FallbackCandidate.cs`, `RectFallbackGroupBase.cs`, `SequentialFallbackGroupBase.cs`.
- `Editor/` — setup and validation tooling.

## Tier boundary
Placement policy and coordination. No vendor SDK calls — those stay in mediation.
Mirrors `AdCore-MainForIOS`.

## See also
- This submodule's `README.md` (version, changelog, release rules).
- Backup/fallback flows: `../BACKUP_FLOW_GUIDE.md`, `../FORCEAD_BACKUP_FLOW.md`,
  `../BANNER_BACKUP_ACTIVATE_FLOW.md`.
- Project architecture: `../../../../docs/architecture.md`.
