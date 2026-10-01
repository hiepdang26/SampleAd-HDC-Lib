# Mfuscator preset audit — 5-step workflow

> Status: maintained by hand. Update if the bridge/validator names or the
> preset SO fields change. Source of truth for the audit workflow that
> `/audit-mfuscator-preset` runs against a project.

Mfuscator obfuscates IL2CPP output. Several of its flags break code that
resolves managed members or native exports **by name** (reflection, JNI,
P/Invoke), so the right flag combination is **project-specific**. This doc
defines the audit that produces one `MfuscatorPresetV2` asset per game.

The preset assets live in `Mfuscator/Presets/`. The UI section that consumes
them is `BuildToolWindowV2MfuscatorSection`. The reflection bridge is
`BuildToolV2MfuscatorBridge`. The build-time MFS_IGNORE toggle is
`BuildToolV2MfuscatorPreset`. The validator that gates the build is
`MfuscatorValidator` (validator #16 in `BuildToolV2ValidationRunner`).

## What "Safe" / "Hardened" mean here

These names are **not** shipped as code. The audit decides which flags this
particular game can tolerate, and writes them into the preset asset. As a
starting reference:

- **Safe**: `removeStringLiterals` + `preserveUnityCrashHandler` +
  `removeMonoExports` + `modifyInternalStructures` on; the three high-risk
  flags off. Real anti-dumper value with zero SDK breakage.
- **Hardened**: Safe + `checkFunctionCalls` + `renameExports`. Requires a
  populated `renameExportsBlacklist` — see step 2.
- **Paranoid**: Hardened + `detectProxyLibraries`. Requires a populated
  `detectProxyLibrariesWhitelist` — see step 3.

## Step 1 — Profile the project

Confirm Mfuscator can have any effect at all:

- **Scripting backend** = IL2CPP (Mono → Mfuscator is a no-op).
- **Unity version** matches `MfuscatorEditor.asmdef`'s constraint
  (`UNITY_2021_3_OR_NEWER` today). Out-of-range = `modifyInternalStructures`
  may break.
- **Target platform** — Mfuscator ships native plugins for the dev host
  (Win64/Linux64/OSX64/OSXARM64). iOS needs the custom build processor from
  the bundled `ReadMe.txt` on Unity < 6.
- **Product identifier** — picks the preset filename. Use the AAB package
  name's last segment (e.g. `com.bg.hcmasmr` → `HCMASMR.asset`).

## Step 2 — Reflection scan (decides `renameExports`)

**Scope: custom game scripts only.** Third-party packages (NGUI, Sirenix
Odin, MaxSdk, AdMob, Adjust, Firebase, Unity Purchasing, ootii, RCC, Gley,
DOTween, etc.) ship with their own reflection patterns that are **stable
across projects** — they go into a global blacklist managed at the
Mfuscator-vendor level, not per-project. Per-project audit focuses only on
the **game-specific code** that varies between projects.

Grep these patterns under the project's custom-script folders only:

```
Assets/Scripts/App/         — game application code
Assets/Scripts/<custom>/    — game-specific scripts (avoid vendored NGUI files)
Assets/0.*/                 — project-specific gameplay folders
Assets/App/, Assets/Game/   — common custom roots
```

Skip:
- `Assets/Plugins/**` (vendored)
- `Assets/MaxSdk/**`, `Assets/GoogleMobileAds/**`, `Assets/Adjust/**`,
  `Assets/Firebase/**`, `Assets/Sirenix/**` (third-party SDKs)
- Files matching well-known package patterns: `NGUI*`, `UI*Tween*`,
  `UIPlay*`, `UICamera*`, `UIButton*`, `EventDelegate*`, `iTween*`,
  `com/ootii/**`, etc.
- `Assets/BG/`, `Assets/BG Lib/`, `Assets/BG_Lib/` (NetCore — already
  covered by the AdjustWrapper entry below)

Patterns to grep:
- `Type.GetMethod(`, `Type.GetField(`, `Type.GetProperty(`, `Type.GetType("`
- `Assembly.GetType(`, `Assembly.Load(`
- `Activator.CreateInstance(` with a string argument
- `Marshal.GetDelegateForFunctionPointer`
- Unity `SendMessage(`
- Newtonsoft / JsonUtility deserialisation with type names
- Attributes scanned by external code: `[MonoPInvokeCallback]`,
  `[AOT.MonoPInvokeCallback]`

For each game-code callsite, classify:
- **Self-contained** — game class accessing another game class by name.
  Mfuscator's managed-side rename is consistent within the project; safe.
- **Crosses to a package** (e.g. game code reads private fields of an SDK
  type) — add that type name to the custom blacklist.

Plus one mandatory cross-project entry: `AdjustSdk.*` — `AdjustWrapper.cs`
in NetCore reflects into Adjust SDK on every project.

Output: the project-specific lines added to `renameExportsBlacklist` (one
pattern per line). When the global blacklist exists at the Mfuscator-vendor
level, the preset's blacklist field = global patterns + per-project lines
from this step.

## Step 3 — Native plugin scan (decides `detectProxyLibraries`)

List every native binary the project intentionally ships:

- `Assets/Plugins/Android/**/*.so`
- `Assets/Plugins/Android/**/*.aar` (contains `.so` inside — extract names)
- `Assets/**/Plugins/**/*.dll`
- Runtime-loaded adapters from MAX / AdMob (already-present:
  `Assets/MaxSdk/Mediation/Fyber`, `Assets/MaxSdk/Mediation/Mintegral`,
  `Assets/GoogleMobileAds/Mediation/`)
- Adjust's native lib if Adjust v5 is in use

Default: `detectProxyLibraries` off. Turn on only after the whitelist covers
every library above — false positives terminate the process at startup.

Output: `detectProxyLibrariesWhitelist` content (one name per line) and a
decision on the flag.

## Step 4 — SDK matrix snapshot

Capture which ad / analytics / attribution SDKs ship with this project so the
next audit (after a Unity or SDK upgrade) can compare. Source of truth:
the `Validation Scan` section's SDK rows + `*_buildreport.txt` Section 7.

## Step 5 — Output preset asset + smoke-test plan

Produce one new asset:

- Path: `Assets/BG_Lib/NetCore/Mfuscator/Presets/<game-id>.asset` (type
  `MfuscatorPresetV2`)
- `presetName` = `<game-id> Safe` / `Hardened` / `Paranoid` as audited
- All flag fields set per the decisions above
- `notes` field records: Unity version, SDK matrix snapshot date, the
  reflection-callsite count that justified the chosen `renameExports` value

**YAML format for `notes`** — write it as a **double-quoted single-line
string with `\n` escapes**, e.g.

```yaml
  notes: "Audit date: 2026-05-27.\nUnity 2022.3.62f3 ...\n\nReflection scan: ..."
```

Do **not** use a `|-` literal block scalar with blank lines between
paragraphs. Unity's YAML parser terminates `|-` blocks at any
whitespace-only line — including lines whose only content is the
block's own indent. The deserializer fails with:
`Parser Failure at line N: Expect ':' between key and value within mapping`.
Double-quoted single-line is the only format that survives every
paragraph break unscathed.

Commit the asset to NetCore. The asset is project-specific content living
inside a shared submodule — accepted trade-off so every dev on that game
picks the same Mfuscator config without re-auditing.

**Smoke-test plan** — list the per-project paths to exercise after the
first build with this preset (paste into the audit report PR):

- App Launch ad load → Firebase event `al_*` fires
- Adjust attribution: `OnAttributionChanged` callback received, user
  network/campaign properties land in Firebase
- AdMob / MAX impression: `ad_impression` event fires with revenue
- IAP purchase (if `BG_Lib/IAP` is in this project): `iap_purchase`
  event fires; `IAPManager.PurchaseProduct` flow completes

A preset is "approved" only after smoke-test on a real device build.

## When to re-audit

- Unity major-version upgrade
- Any ad SDK version bump (Adjust, AdMob, MAX, Firebase, Unity Purchasing)
- New SDK added to the project
- Mfuscator update to v2.1+ (reflection bridge may need refresh)
