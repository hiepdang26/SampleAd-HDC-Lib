# AndroidMediation

Map only — no explanations. Tier 3 (Mediation) submodule of BG-Library.

> This submodule is normally developed **inside the `BG-Library-Full-Module` host
> project** — the `../../../` paths below resolve there. If cloned standalone, those
> host docs are absent; open the host project for the cross-cutting documentation.

## Read before editing here
- `README.md` — version, changelog, and release rules for **this** submodule.
- `docs/overview.md` — role, main files, tier boundary.
- Project-wide: `../../../CLAUDE.md`, `../../../docs/architecture.md`,
  `../../../docs/submodule-map.md`.

## Read when relevant
- Native ad work → `NativeAdManager/`.

## Hard rules
- This is a separate git repository. Commits here belong to the AndroidMediation repo.
- Android-only mediation + native ads — mirrors `AdmobMediation` and `MaxMediation`
  for the FS/Rect pattern. Cross-check siblings when editing shared patterns.
- Native plugin files under `Plugins/` must be included in the Android build.
- Keep placement/pacing policy in the AdCore tier, not here.
- Release: follow `README.md` rules and `../../../docs/release-process.md`.
- Do not push, tag, or make release commits unless explicitly asked.
