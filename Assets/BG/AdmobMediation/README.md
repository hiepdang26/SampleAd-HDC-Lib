# BG-Library-AdmobMediation

Meta Audience Network: https://developers.google.com/admob/unity/mediation/meta#meta-audience-network-unity-mediation-plugin-changelog

Version: 2.1.0

## Overview
`BG-Library-AdmobMediation` contains the AdMob mediation runtime used by BG library projects.
It covers fullscreen and rectangle ad flows, shared ad state objects, access interfaces, and consent related handling.

## Main Files
- `Admob_MediationManager.cs`: central manager for AdMob mediation lifecycle.
- `Admob_FSGroupController.cs` and `Admob_FSLogic.cs`: fullscreen ad orchestration.
- `Admob_RectGroupController.cs` and `Admob_RectLogic.cs`: rectangle ad orchestration.
- `Admob_Info.cs`: metadata and runtime information model.
- `IAdmob_Configs.cs`: configuration contract.
- `IAdmob_FSAccessAPI.cs`: fullscreen access interface.
- `GoogleMobileAdsConsentController.cs`: consent and privacy flow support.

## Dependencies
- Google Mobile Ads SDK.
- `NetCore` for shared runtime services.
- An AdCore module that drives placement level business logic.

## Setup
1. Import this submodule with the required Google Mobile Ads dependencies.
2. Provide the configuration object that implements `IAdmob_Configs`.
3. Initialize `Admob_MediationManager` before requesting or showing ads.
4. Wire consent flow if your project requires privacy prompts before ad initialization.

## Notes
- Keep project-specific retry and placement logic in the upper AdCore layer.
- Use this module as the AdMob integration boundary for the rest of the project.
- Review platform ad ids and test ids before release.

## Changelog
- `v2.1.0` - 2026-03-12: Add `DisablePostInitReload` support for fullscreen groups and forward SDK show-fail error codes into ad display failure callbacks.
- `v2.0.3` - 2026-03-10: Add explicit release checklist rules for version bump, changelog update, annotated tag, push, and parent repo pointer update.
- `v2.0.2` - 2026-03-10: Sync release README rules across submodules and require changelog notes for every release.

## Versioning Rule
- Use semantic versioning for this submodule: `MAJOR.MINOR.PATCH`.
- Increase `PATCH` for small fixes, text cleanup, safe UI polish, or non-breaking maintenance.
- Increase `MINOR` for new tools, new editor/runtime capabilities, workflow changes, or any non-breaking feature expansion.
- Increase `MAJOR` only for breaking API changes, breaking data flow changes, or upgrades that require downstream submodules/projects to update code or setup.

## Commit Rule
- Release commit format: `release(admob-mediation): vX.Y.Z - short summary`.
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
