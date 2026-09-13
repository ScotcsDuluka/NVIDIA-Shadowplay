# 01 — OSC Artifact Inventory (Phase 0)

Root: `Overlay/osc/`. Sizes from `ls -la`; roles verified by reading the
artifacts (not assumed). Timestamps: bundle files (app/vendor/common/index)
were last touched by the `Sync` commit 3dd3381842; the remaining files are
the original GFE drop (2024-04-10).

## Code (the SPA)

| FILE | SIZE | TYPE | ROLE (verified) | EVIDENCE |
|---|---|---|---|---|
| `index.html` | 465 B | bootstrap | Loads vendor→common→app in `<head>`, `user-config.js`+`config.js` at end of `<body>`; `<div ui-view>` is the only mount point; `ng-app="main" ng-csp ng-strict-di` | file read |
| `vendor.js` | 1 431 519 B | webpack bundle | Third-party + shared SDKs: Angular 1.x, ngMaterial, ui-router, translate, socket.io-client 2.5.0 + engine.io-client 3.5.4, d3, and the `crimson` bridge library (`cefService`, eventAggregator) + `nvAngular*` endpoint SDKs | `angular.module` registrations, reconnect-manager source strings |
| `common.js` | 907 B | webpack runtime | `webpackJsonp` runtime, module registry `r`, async chunk loader (`{1:"app"}` chunk map — no chunks shipped), `r.p=""` public path | file read |
| `app.js` | 1 596 199 B | webpack bundle | Application: `main` module, 38 ui-router states, 26 app services, 52 controllers, 73 directives, socketService provider, all ShadowPlay feature logic | extraction (see 02) |
| `config.js` | 11 702 B | runtime config | `angular.module('main.config')` constant `OSC_CONFIG` (providers: twitch/imgur/google(youtube,photos)/facebook/weibo/swgf with **clientIds**), jsEvents, jarvis, pipl; `OSC_BUILD_INFO` = oscPackageVersion **3.28.0.412**, branch `rel_03_28`, gitHash ffe15e48d9 | file read |
| `user-config.js` | 213 B | user config | `main.userConfig` constant `OSCCLIENT_USER_CONFIG` (verbose/event/perf logging flags) | file read |
| `manifest.json` | 14 590 B | manifest | `{name, content}` — GFE module manifest (package metadata) | file read |

## Localization

| FILE | SIZE | ROLE | EVIDENCE |
|---|---|---|---|
| `l10n/<lang>.json` ×28 | 1.5 MB total | Translation tables, **flat maps of 673 dotted keys** (e.g. `l10n.instantReplay`, `l10n.stopAndSave`); en-US.json = 45 073 B, UTF-8 (BOM in some builds) | parsed en-US.json; dir listing (cs-CZ…zh-CHT) |

## Assets

| FILES | ROLE |
|---|---|
| `nvgcshare.woff2` (11 KB) | GFE display font — reused by the new UI |
| `img_gfe_branding.svg`, `lg_osc_logo_48x48.png`, `GFE_Logo_16.png`, `osc_img_appicon_*.png` | branding |
| `display_*.png/svg`, `perf_mixer.svg`, `perfstats_preview.png`, `fps_counter_icon16.png` | OSD indicator icons (display-rect overlay features) |
| `fb_reaction_*.svg` ×6 | Facebook live-comment reactions (broadcast feature) |
| `info-white-18dp.svg`, `open_in_new.svg`, `scan_complete.svg`, `wm_on/off.svg` | misc UI icons |

## What is NOT here (verified absent)

- **No `*.map` files** — `find . -name "*.map"` over the whole repo returns
  nothing; `common.js`/`vendor.js` end with `//# sourceMappingURL=…js.map`
  pointing at files that were never shipped; `app.js` has a dev-mode
  inline-data-URL branch and a prod `app.js.map` reference. ⇒ Source-map
  recovery is impossible; compiled-bundle reverse is the only path.
- **No async chunks** (`*_0.chunk.js` etc.) — common.js registers the loader
  but the app never requests one (single-chunk app).
- **No service worker / CSP meta** — transport security is the host's job.
