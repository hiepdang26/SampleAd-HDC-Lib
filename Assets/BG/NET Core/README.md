# BG-Library-NetCore

Version: 2.14.0

## Overview
`BG-Library-NetCore` is the shared foundation module used by the BG ad and network library stack.
It contains common runtime systems, editor tooling, wrappers, dispatchers, configuration assets, and helper modules used across the other submodules.

## Main Areas
- `Common/`: shared utilities and reusable base code.
- `Systems/`: core systems used by the runtime.
- `Other modules/`: additional feature modules such as remote config and support scripts.
- `Mediation Scripts/`: shared scripts used by mediation packages.
- `Firebase/`: Firebase related wrappers and integration points.
- `AdjustWrapper/`: Adjust related wrappers.
- `BuildToolV2/`: build time utilities and NET-linked build workflow.
- `Editor/`: editor workflows and tooling.
- `MainThreadDispatcher/`: utilities for marshaling work back to the Unity main thread.
- `NetConfigsSO.cs` and `NetEventsBinder.cs`: central configuration and binding entry points.

## Dependencies
- Unity runtime and editor APIs.
- External service SDKs only when the related wrapper folder is in use.
- Other BG library modules that build on top of these shared systems.

## Setup
1. Import this submodule before the higher-level BG library packages.
2. Configure the required ScriptableObject assets and service wrappers used by your project.
3. Initialize shared systems during application startup.
4. Connect dependent submodules such as AdCore and mediation packages to the services exposed here.

## Notes
- This package is the base layer of the BG library stack.
- Changes here can affect multiple submodules, so version updates should be reviewed carefully.
- Keep project-specific gameplay logic outside this module.

## Changelog
- `v2.14.0` - 2026-04-15: Add per-channel tracking gates plus caller-driven `FA`/`PU` force-init flows that can recover ad groups even when post-init reload is disabled.
- `v2.12.0` - 2026-03-20: Rework BuildToolV2 to use source-backed values plus local-only state, keep validation scans manual/stale-aware, and sync the current debug workspace scene and SRDebugger settings.
- `v2.11.0` - 2026-03-19: Add fullscreen terminal-callback gating to block duplicate hidden/show-fail side effects, refresh BuildToolV2 device/output actions, and migrate tracking builders, explainer, filters, and matrix docs to the new grammar.
- `v2.10.0` - 2026-03-19: Add config-driven tracking Firebase routing, expand the debug config workspace with tracking preview, improve tracking token generation/explainer coverage, and ship a clearer standalone tracking reading guide.
- `v2.9.1` - 2026-03-19: Refine BuildToolV2 overview by showing tracking Firebase state, adding a direct output-folder set action, and refreshing the displayed folder immediately after selection.
- `v2.9.0` - 2026-03-18: Move public network probing into the runtime debug overlay build section and remove the matching network UI flow from BuildToolV2.
- `v2.8.0` - 2026-03-18: Harden BuildToolV2 sync sources, validation wording, build naming/reporting, release debug rules, and pre-build environment preparation.
- `v2.7.0` - 2026-03-18: Refine BuildToolV2 layout, validation scan, device actions, diagnostics preset flow, and preset-driven NET debug configuration.
- `v2.6.0` - 2026-03-12: Add resolved ad identity tracking across AdSystem and AdCore flows, support activate-view tracking for banner and MREC, and expand tracking history/debug output with more readable event descriptions.
- `v2.5.0` - 2026-03-11: Integrate scrcpy phone preview into BuildToolV2 with local-only settings, preset-based quality options, PATH/custom executable support, and lightweight refresh behavior for device preview workflows.
- `v2.4.0` - 2026-03-11: Add banner and MREC activate-view flow with dedicated tracking/events, apply Android-only `DisablePostInitReload` handling for FA/PU groups, and harden debug/runtime guards around ad system readiness.
- `v2.3.11` - 2026-03-11: Add in-tool build scene management to BuildToolV2, including primary scene handling, drag-and-drop scene add, and clearer AdMob test options in Build Config.
- `v2.3.10` - 2026-03-11: Remove noisy debug warnings from hot polling paths such as `GetReady`, `IsLoaded`, and size checks so ad systems no longer spam logs during normal readiness loops.
- `v2.3.9` - 2026-03-11: Flush deferred `NetCallerAPI` event subscriptions after `NetEventsBinder.Bind()` so early subscribers are attached safely once NetCore startup finishes.
- `v2.3.8` - 2026-03-11: Fix Debug Overlay startup timing so the panel waits for `OnAllJsonsComplete` and adcore initialization instead of refreshing too early, and guard FA/PU debug group helpers against null core crashes.
- `v2.3.7` - 2026-03-10: Sync BuildToolV2 app information with PlayerSettings for product, company, version, and keystore password.
- `v2.3.6` - 2026-03-10: Bind `RemoteConfig.OnAllJsonsComplete` through `NetEventsBinder` for the public `NetCallerAPI` event bridge.
- `v2.3.5` - 2026-03-10: Harden BuildToolV2 icon handling, adaptive icon workflow, foldout performance, and editor compatibility across different Unity setups.
- `v2.3.4` - 2026-03-10: Add configurable Developer Build in BuildToolV2 and keep preset defaults turned off.
- `v2.3.3` - 2026-03-10: Add explicit release checklist rules for version bump, changelog update, annotated tag, push, and parent repo pointer update.
- `v2.3.2` - 2026-03-10: Improve BuildToolV2 stability and scroll performance, then sync release README rules across submodules.

## Versioning Rule
- Use semantic versioning for this submodule: `MAJOR.MINOR.PATCH`.
- Increase `PATCH` for small fixes, text cleanup, safe UI polish, or non-breaking maintenance.
- Increase `MINOR` for new tools, new editor/runtime capabilities, workflow changes, or any non-breaking feature expansion.
- Increase `MAJOR` only for breaking API changes, breaking data flow changes, or upgrades that require downstream submodules/projects to update code or setup.

## Commit Rule
- Release commit format: `release(netcore): vX.Y.Z - short summary`.
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
