# AdmobMediation

Map only — no explanations. Tier 3 (Mediation) submodule of BG-Library.

> This submodule is normally developed **inside the `BG-Library-Full-Module` host
> project** — the `../../../` paths below resolve there. If cloned standalone, those
> host docs are absent; open the host project for the cross-cutting documentation.

## Read before editing here
- `README.md` — version, changelog, and release rules for **this** submodule.
- `docs/overview.md` — role, main files, tier boundary.
- Project-wide: `../../../CLAUDE.md`, `../../../docs/architecture.md`,
  `../../../docs/submodule-map.md`.

## Hard rules
- This is a separate git repository. Commits here belong to the AdmobMediation repo.
- This is the AdMob SDK integration boundary — mirrors `MaxMediation` and
  `AndroidMediation`. Cross-check both siblings when editing shared patterns.
- Keep placement/pacing policy in the AdCore tier, not here.
- Release: follow `README.md` rules and `../../../docs/release-process.md`.
- Do not push, tag, or make release commits unless explicitly asked.
