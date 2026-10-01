# BG-Library-AndroidMediation

Version: 2.1.7

## Overview
`BG-Library-AndroidMediation` provides Android specific mediation and native ad support.
It is intended for projects that need Android only ad flows beyond the basic AdMob mediation layer.

## Main Files
- `Android_FSGroupController.cs`: fullscreen ad group control for Android.
- `Android_RectGroupController.cs`: rectangle ad group control for Android.
- `Android_Info.cs`: Android mediation metadata and runtime information.
- `NativeAdManager/`: native ad loading, callbacks, and instance management.
- `Helper/`: shared Android mediation helpers.
- `Plugins/`: native plugin files required by the Android integration.

## Dependencies
- `NetCore` for shared runtime services.
- The Android SDK and plugin setup required by this mediation flow.
- An Android AdCore module that drives higher-level ad policy.

## Setup
1. Import this submodule into the Unity project together with its required Android dependencies.
2. Verify the plugin files under `Plugins/` are included in the Android build.
3. Configure placements and initialize the Android mediation layer during app startup.
4. Validate fullscreen, rectangle, and native ad flows on a real Android device.

## Notes
- This module is Android only and should not be used as a cross-platform ad layer.
- Native ad support is centered in `NativeAdManager/`.
- Keep game-facing ad rules in the upper AdCore layer to avoid tight coupling.

## Changelog
- `v2.1.7` - 2026-04-20: Wire Android fullscreen `enableAdComeback` through config, info, controller, and logic layers so `FSInstance` receives the comeback flag alongside `pauseGameplay`.
- `v2.1.2` - 2026-03-19: Pass Android native ad load error codes through popup/native callback layers and preserve popup auto-reload behavior when `DisablePostInitReload` is enabled.
- `v2.1.1` - 2026-03-19: Guard rewarded Android fullscreen flows so duplicate hidden callbacks do not trigger duplicate reward completion after the shared fullscreen close gate consumes the first terminal callback.
- `v2.1.0` - 2026-03-12: Add `DisablePostInitReload` propagation for Android fullscreen and popup groups, including popup first-load fail cleanup and reload blocking after init.
- `v2.0.3` - 2026-03-10: Add explicit release checklist rules for version bump, changelog update, annotated tag, push, and parent repo pointer update.
- `v2.0.2` - 2026-03-10: Sync release README rules across submodules and require changelog notes for every release.

## Versioning Rule
- Use semantic versioning for this submodule: `MAJOR.MINOR.PATCH`.
- Increase `PATCH` for small fixes, text cleanup, safe UI polish, or non-breaking maintenance.
- Increase `MINOR` for new tools, new editor/runtime capabilities, workflow changes, or any non-breaking feature expansion.
- Increase `MAJOR` only for breaking API changes, breaking data flow changes, or upgrades that require downstream submodules/projects to update code or setup.

## Commit Rule
- Release commit format: `release(android-mediation): vX.Y.Z - short summary`.
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
