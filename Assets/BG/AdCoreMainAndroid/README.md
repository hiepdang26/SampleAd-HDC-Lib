# BG-Library-AdCore-MainForAndroid

Version: 2.2.1

## Overview
`BG-Library-AdCore-MainForAndroid` is the main Android ad coordination layer used by BG library based games.
It centralizes Android ad configuration, validates ad setup, and bridges the game layer to the underlying mediation modules.

## Main Files
- `AdCore_MainAndroid.cs`: main runtime controller for Android ad flow.
- `AdCore_Configs.cs`: shared configuration model used by the Android AdCore.
- `AdCore MainAndroid.asset`: default ScriptableObject asset with runtime configuration.
- `AdConfigsValidator.cs`: validation helper for ad configuration.
- `Editor/`: editor-side utilities for setup and validation.

## Dependencies
- `NetCore` for shared core systems.
- One or more mediation submodules such as `AdmobMediation` or `AndroidMediation`.

## Setup
1. Import this submodule and the mediation submodules required by your project.
2. Configure `AdCore MainAndroid.asset` with placement data and runtime settings.
3. Run the provided validation flow before building Android.
4. Initialize the AdCore early in the application lifecycle.

## Notes
- Intended for Android projects that need a central place to drive ad logic.
- Keep SDK-specific logic inside the mediation modules, not in game code.
- Use the validator whenever ad placements or ids change.

## Changelog
- `v2.2.1` - 2026-04-20: Forward Android fullscreen `enableAdComeback` from AdCore configs into Android mediation info so fullscreen instance creation receives the expected comeback flag.
- `v2.2.0` - 2026-04-15: Add ForceAd fallback-group support for caller-driven force init so FA chains can explicitly reload after post-init reload is disabled.
- `v2.1.1` - 2026-03-20: Guard AppLaunch and AppResume group resolution so empty ForceAd group names and missing configs no longer trigger invalid lookup errors.
- `v2.1.0` - 2026-03-12: Add `DisablePostInitReload` config mapping for Android and AdMob groups so Android AdCore can control post-init reload behavior per placement.
- `v2.0.3` - 2026-03-10: Add explicit release checklist rules for version bump, changelog update, annotated tag, push, and parent repo pointer update.
- `v2.0.2` - 2026-03-10: Sync release README rules across submodules and require changelog notes for every release.

## Versioning Rule
- Use semantic versioning for this submodule: `MAJOR.MINOR.PATCH`.
- Increase `PATCH` for small fixes, text cleanup, safe UI polish, or non-breaking maintenance.
- Increase `MINOR` for new tools, new editor/runtime capabilities, workflow changes, or any non-breaking feature expansion.
- Increase `MAJOR` only for breaking API changes, breaking data flow changes, or upgrades that require downstream submodules/projects to update code or setup.

## Commit Rule
- Release commit format: `release(adcore-main-android): vX.Y.Z - short summary`.
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
