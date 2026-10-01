# NetCore

Map only — no explanations. Tier 1 (Foundation) submodule of BG-Library.

> This submodule is normally developed **inside the `BG-Library-Full-Module` host
> project** — the `../../../` paths below resolve there. If cloned standalone, those
> host docs are absent; open the host project for the cross-cutting documentation.

## Read before editing here
- `README.md` — version, changelog, and release rules for **this** submodule.
- `docs/overview.md` — role, main areas, tier boundary.
- Project-wide: `../../../CLAUDE.md`, `../../../docs/architecture.md`,
  `../../../docs/submodule-map.md`.

## Read when relevant
- Tracking work → `Systems/Tracking/TRACKING_MATRIX.md`,
  `Systems/Tracking/TRACKING_OVERVIEW_CURRENT.md`.
- Public API impact → `../../../docs/api-surface.md`.

## Hard rules
- This is a separate git repository. Commits here belong to the NetCore repo.
- Release: follow `README.md` rules and `../../../docs/release-process.md`.
- Do not push, tag, or make release commits unless explicitly asked.
- Changes here can affect every downstream submodule — review carefully.
