# AdCore-MainForAndroid

Map only — no explanations. Tier 2 (AdCore) submodule of BG-Library.

> This submodule is normally developed **inside the `BG-Library-Full-Module` host
> project** — the `../../../` paths below resolve there. If cloned standalone, those
> host docs are absent; open the host project for the cross-cutting documentation.

## Read before editing here
- `README.md` — version, changelog, and release rules for **this** submodule.
- `docs/overview.md` — role, main files, tier boundary.
- Project-wide: `../../../CLAUDE.md`, `../../../docs/architecture.md`,
  `../../../docs/submodule-map.md`.

## Read when relevant
- Backup / fallback flows → `BACKUP_FLOW_GUIDE.md`, `FORCEAD_BACKUP_FLOW.md`,
  `BANNER_BACKUP_ACTIVATE_FLOW.md`.
- Tracing an ad type → `../../../docs/flows/`.

## Hard rules
- This is a separate git repository. Commits here belong to the AdCore-MainForAndroid repo.
- This module mirrors `AdCore-MainForIOS` — cross-check the sibling when editing shared patterns.
- Keep SDK-specific logic in the mediation submodules, not here.
- Release: follow `README.md` rules and `../../../docs/release-process.md`.
- Do not push, tag, or make release commits unless explicitly asked.
