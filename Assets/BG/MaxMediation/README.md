# BG-Library-MaxMediation

Version: 2.0.4

## Overview
`BG-Library-MaxMediation` contains the AppLovin MAX mediation runtime used by BG library projects.
It provides fullscreen and rectangle ad flows together with the access interfaces and runtime information needed by upper ad systems.

## Main Files
- `Max_MediationManager.cs`: main runtime manager for MAX mediation.
- `Max_FSGroupController.cs` and `Max_FSLogic.cs`: fullscreen ad flow.
- `Max_RectGroupController.cs` and `Max_RectLogic.cs`: rectangle ad flow.
- `Max_Info.cs`: runtime information model for MAX mediation.
- `IMax_Configs.cs`: configuration contract.
- `IMax_FSAccessAPI.cs` and `IMax_RectAccessAPI.cs`: access interfaces used by higher layers.

## Dependencies
- AppLovin MAX SDK and related adapters.
- `NetCore` for shared runtime services.
- A BG AdCore module to coordinate placement level ad logic.

## Setup
1. Import this submodule and the required MAX SDK packages.
2. Provide the configuration implementation required by `IMax_Configs`.
3. Initialize `Max_MediationManager` before any ad request is made.
4. Verify fullscreen and rectangle placements on the target platforms.

## Notes
- Keep mediation specific behavior inside this module.
- Use upper layers for game-specific pacing, placement policy, and remote config rules.
- Review test mode and production ids before release.

## Changelog
- `v2.0.4` - 2026-03-12: Forward MAX fullscreen display-fail error codes into ad display failure callbacks for clearer upstream tracking and diagnostics.
- `v2.0.3` - 2026-03-10: Add explicit release checklist rules for version bump, changelog update, annotated tag, push, and parent repo pointer update.
- `v2.0.2` - 2026-03-10: Sync release README rules across submodules and require changelog notes for every release.

## Versioning Rule
- Use semantic versioning for this submodule: `MAJOR.MINOR.PATCH`.
- Increase `PATCH` for small fixes, text cleanup, safe UI polish, or non-breaking maintenance.
- Increase `MINOR` for new tools, new editor/runtime capabilities, workflow changes, or any non-breaking feature expansion.
- Increase `MAJOR` only for breaking API changes, breaking data flow changes, or upgrades that require downstream submodules/projects to update code or setup.

## Commit Rule
- Release commit format: `release(max-mediation): vX.Y.Z - short summary`.
- If a commit includes both cleanup and a new feature, version by the highest-impact change.
- Do not include unrelated scene, test, or local debug changes in the release commit.
- After each release commit, update the version in this README to match the released submodule version.
- After each release commit, add a concise changelog note in `## Changelog` for that released version.
- After each release commit, create an annotated git tag that matches the release version exactly: `vX.Y.Z`.
- After each release commit, push both the branch commit and the new release tag to the submodule remote.
- After releasing a submodule, update the parent repository submodule pointer in a separate follow-up commit.

## Release Checklist
1. Decide the next semantic version.
2. Update `Version:` in this README.
3. Add one concise entry to `## Changelog`.
4. Commit with the release message format above.
5. Create the annotated tag `vX.Y.Z`.
6. Push the commit and the tag.
7. Update the parent repo submodule pointer.
