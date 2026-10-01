# NetCallerAPI

Map only — no explanations. Tier 1 (Foundation) submodule of BG-Library.

> This submodule is normally developed **inside the `BG-Library-Full-Module` host
> project** — the `../../../` paths below resolve there. If cloned standalone, those
> host docs are absent; open the host project for the cross-cutting documentation.

## Read before editing here
- `README.md` — version, changelog, and release rules for **this** submodule.
- `docs/overview.md` — role, main files, tier boundary.
- `../../../docs/api-surface.md` — the public surface this submodule exposes.
- Project-wide: `../../../CLAUDE.md`, `../../../docs/architecture.md`,
  `../../../docs/submodule-map.md`.

## Hard rules
- This is a separate git repository. Commits here belong to the NetCallerAPI repo.
- This is the public API layer only — keep runtime behaviour in NetCore.
- Release: follow `README.md` rules and `../../../docs/release-process.md`.
- Do not push, tag, or make release commits unless explicitly asked.
