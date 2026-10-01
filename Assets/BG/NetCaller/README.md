# BG-Library-NetCallerAPI

Version: 2.2.0

## Overview
`BG-Library-NetCallerAPI` exposes a ScriptableObject-driven API layer for calling into `NetCore`.
It is intended to give higher-level modules and project code a stable entry point without reaching directly into `NetCore` internals.

## Main Areas
- `NetCallerAPI.cs`: public entry point used by game code and dependent modules.
- `NetEventsHub.cs`: delegates and event bridge used to connect callers to `NetCore`.
- Related binding code in `NetCore` connects this API surface to the runtime systems.

## Dependencies
- `NetCore` for the real runtime implementation and bindings.
- Other BG library modules may depend on this package as a stable access layer.

## Setup
1. Import this submodule together with `NetCore`.
2. Ensure `NetCore` bindings are initialized in the project startup flow.
3. Use `NetCallerAPI` from game code or upper modules instead of calling `NetCore` internals directly.

## Notes
- Keep this package focused on API surface and call routing.
- Runtime behavior changes should stay in `NetCore`, not in the caller layer.
- Prefer extending this package when you need a stable public API for new `NetCore` features.

## Changelog
- `v2.2.0` - 2026-04-15: Add public `FA_ForceInit(group)` and `PU_ForceInit(group)` APIs so callers can explicitly re-arm post-init-reload-disabled flows without changing existing manual init behavior.
- `v2.1.1` - 2026-03-17: Expose `PULayout` target accessors and invalidate cached layout values when the target changes.
- `v2.1.0` - 2026-03-11: Add public banner and MREC activate-view APIs and keep event subscriptions safe even when callers subscribe before NetCore binding is ready.
- `v2.0.5` - 2026-03-11: Make public event subscriptions resilient to early calls before `NetEventsBinder.Bind()`, including RemoteConfig, AppLaunch, and BreakAd events.
- `v2.0.4` - 2026-03-10: Add public subscribe and unsubscribe API for `RemoteConfig.OnAllJsonsComplete`.
- `v2.0.3` - 2026-03-10: Add explicit release checklist rules for version bump, changelog update, annotated tag, push, and parent repo pointer update.
- `v2.0.2` - 2026-03-10: Sync release README rules across submodules and require changelog notes for every release.

## Versioning Rule
- Use semantic versioning for this submodule: `MAJOR.MINOR.PATCH`.
- Increase `PATCH` for small fixes, text cleanup, safe UI polish, or non-breaking maintenance.
- Increase `MINOR` for new tools, new editor/runtime capabilities, workflow changes, or any non-breaking feature expansion.
- Increase `MAJOR` only for breaking API changes, breaking data flow changes, or upgrades that require downstream submodules/projects to update code or setup.

## Commit Rule
- Release commit format: `release(netcallerapi): vX.Y.Z - short summary`.
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
