# OSC v1-knowledge — knowledge synced from A:\ShadowPlay\ShadowPlay_V1

Synced 2026-10-06. Purpose: the repo previously had only scattered OSC notes; the
ShadowPlay_V1 tree on `A:` is a single-copy reconstruction of the OSC 3.28.0.412
frontend (source of truth `C:\My Project\ShadowPlay\osc` is now gone, so `A:` is the
only copy — hence this sync). Nothing here was modified during extraction; these are
verbatim copies plus one verification report produced on sync day.

Canonical representation inside V1: **`03_RECONSTRUCTED/source-tree`** (declared in
`A:\ShadowPlay\ShadowPlay_V1\README.md`). See that README for the full folder map and
the tool provenance of every view.

## Files

| File | Source in V1 | What it is |
|---|---|---|
| `app-module-index.json` | `04_ANALYSIS/app-module-index.json` | Index of all 487 app webpack modules: id, `requires` graph, byte/line sizes, and detected Angular symbols (services/controllers/directives/providers/angularModules). This is also the V1 "names" artifact — `04_ANALYSIS/named-symbol-index.json` is byte-identical (same generator output, two names). WebCrack's own `names.json` was never imported into V1; the symbol-name knowledge lives here. |
| `vendor-module-map.json` | `04_ANALYSIS/vendor-module-map.json` | Maps all 290 vendor library modules (IDs 1–290) to their original paths from the source `manifest.json` (`./crypto-js/core.js`, `./d3/…`, `./angular/…`). |
| `angular-di-graph.json` | `04_ANALYSIS/angular-di-graph.json` | AngularJS dependency-injection graph: module/service/controller/directive/provider registrations extracted from the bundle. |
| `app-symbol-catalog.json` | `04_ANALYSIS/app-symbol-catalog.json` | Symbol catalog: 19 Angular modules, 26 services, 52 controllers, 73 directives, 19 providers, 174 endpoint candidates. |
| `BOOTSTRAP_AND_SHADOWPLAY_FLOW.md` | `04_ANALYSIS/BOOTSTRAP_AND_SHADOWPLAY_FLOW.md` | Bootstrap flow: webpack entry module 0 → Angular main module 1 → ui-router (module 136, default route `/base`) → appService (module 135) → ShadowPlay branch (service module 4, endpoints provider 208, MainMenuController 210), incl. the proven Manual Record chain (`POST /Record/Enable`). |
| `deobfuscated-app.js` | `02_DECOMPILED/app/deobfuscated.js` | WebCrack deobfuscated whole-bundle app.js (2,244,001 bytes). Readable reference for any app module; per-ID normalized files live in V1 `03_RECONSTRUCTED/commonjs/app`. |
| `module-count-verification.json` | produced 2026-10-06 by `04_ANALYSIS/verify_module_counts.js` | Completeness evidence: node-vm census of the real bundles — app.js = 487 modules (IDs 0–486, single `webpackJsonp([1,0],…)` registration), vendor.js = self-executing bundle with 291 slots = webpack bootstrap shim (ID 0) + 290 library modules (IDs 1–290). Diff vs reconstruction: 0 missing, 0 extra on both sides. |

## Reproduce the census

```bash
cd A:/ShadowPlay/ShadowPlay_V1/04_ANALYSIS
node verify_module_counts.js   # writes module-count-verification.json
```

The script loads `01_SOURCE/osc/common.js` (the genuine webpack runtime) inside a node
`vm` sandbox, replaces `window.webpackJsonp` with a capture stub, and runs the bundles —
module bodies never execute. vendor.js registers no JSONP call at all; it is a
self-executing bundle whose module array is read from the require function's `.m` property.
