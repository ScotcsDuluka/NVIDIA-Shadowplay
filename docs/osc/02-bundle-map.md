# 02 — Bundle Structure Map (Phase 1) + Source-Map Verdict (Phase 2)

Method: string/AST-level extraction over the minified bundles (regex over
registration patterns — registration names survive minification because they
are string literals). Confidence legend: **HIGH** = literal in bundle;
**MED** = inferred from registration shape; **LOW** = hypothesis (flagged).

## Source maps (Phase 2 verdict)

| File | sourceMappingURL | .map on disk |
|---|---|---|
| common.js | `common.js.map` | **MISSING** |
| vendor.js | `vendor.js.map` | **MISSING** |
| app.js | dev: inline `data:application/json;base64…` via `btoa(unescape(encodeURIComponent(JSON.stringify(i))))`; prod: `app.js.map` | **MISSING** |

**VERDICT: NOT FOUND — reverse the compiled bundle.** (Repo-wide `find`
returned zero `.map` files; do not fabricate maps.)

## Webpack runtime (common.js — HIGH)

- `window.webpackJsonp(chunkIds, moreModules)` — merge + run deferred factories.
- Registry `n{}`, status map `a{0:0}`; async chunk URL `r.p + "" + ({1:"app"}[e]||e) + "_" + e + ".chunk.js"`.
- `common.js` itself is module `0`'s runtime wrapper (`if(p[0])return n[0]=0,r(0)`).

## Angular module graph (HIGH — literal `angular.module(name, [deps])`)

### app.js
| Module | Depends on | Role |
|---|---|---|
| `main` | ngMainConstantsModule, ngMainCommonModule, ngMaterial, ngResource, pascalprecht.translate, ui.router, … | root (`ng-app="main"`) |
| `main.common` | crimson, ngSanitize, main.config, main.userConfig, ngMaterial, pascalprecht.translate, ngEventAggregator, ui.rou… | shared app layer: socketService, localSdk, endpoints |
| `main.localSdk` | 14 SDK submodules | SDK aggregation |
| `main.localSdk.{shadowPlay,settings,hardware,coplay,highlights,nvCamera,feedback,gfeUpdates,gfwsl,abHub,quietMode2,nis,dvc}Sdk` | nvAngularHttpEndpoint, main.common | per-feature REST endpoint SDKs |
| `main.piplConfigSdk`, `main.telemetry`, `main.constants` | — | config push, telemetry, constants |

### vendor.js
| Module | Role |
|---|---|
| `crimson` | **bridge layer**: `cefService` (window.cefQuery wrapper), eventAggregator, logger |
| `nvAngularHttpEndpoint` | REST factory (URL/header generators — carries `X_LOCAL_SECURITY_COOKIE`) |
| `nvAngular{Account,Jarvis}Sdk`, `nvDbService`, `nvJsEvents`, `underscore` | account/telemetry plumbing |
| `ugc-lib` + `connectSdk` + per-provider SDKs (`facebookSdk`, `googlePhotosSdk`, `imgurSdk`, `shotWithGeForceSdk`, `twitchSdk`, `weiboSdk`, `youtubeSdk`) | upload/share stack (not needed for M1) |
| `gallerySdk`, … | gallery stack |

## Registration census (HIGH counts)

| Kind | app.js | vendor.js |
|---|---|---|
| controller | 52 | — |
| directive | 73 | (many material/translate internals) |
| service | 26 | — |
| provider | 72 | 30 |
| factory | 30 | — |
| constant | 113 | — |
| filter/value | 7 | — |

Full name lists: extracted to `all_components.json` during analysis
(app services/controllers/directives reproduced in 06-ui-components.md).

## Key singletons (all HIGH — literal names)

- `socketService` provider (`main.common`): `setConfig`, `$get` →
  `{connect, disconnect, emit, register}`; connect = `io(server+port,
  {query:{X_LOCAL_SECURITY_COOKIE: secret}})`; `register(channel, eventName)`
  → socket.on → eventAggregator.trigger(eventName, ...args).
- `localSdk` provider: `updateNodeInfo({port, secret})` sets
  `commonHeaders.X_LOCAL_SECURITY_COOKIE`; `newLocalEndpointFactory(service,
  version)` builds URLs `server+port+"/"+service+"/"+version+url`.
- `cefService` (`crimson`, vendor): the ONLY window.cefQuery wrapper —
  full API in 03-protocol-map.md.
- `oscDisplayService` (app): window lifecycle — open/close/toggle, display
  rects, fullscreen transitions, WindowState channel handling.
- `shadowPlayService` (app): record/replay/broadcast/screenshot logic on top
  of `shadowPlayEndpoints` + socket channels.

## Boot sequence (HIGH — `$stateProvider` resolve chain on state `base`)

```
urlRouter.otherwise("/base")
state base (template <nv-base></nv-base>)
  resolve localNodeInfo   : cefService.localNodeInfo() → JSON {port, secret}
                            → localSdk.updateNodeInfo / nvAccountEndpoints / ugcLib
  resolve localizedConfig : piplConfigService.getPiplConfig() → merge into OSC_CONFIG
                            → telemetry/gfwsl/jarvis setServer
  resolve hardwareInfo    : hardwareService.getSystemInfo() → TelemetryDeviceId,
                            UserDefaultUILanguage; jarvis session (401 → "sessionExpired")
→ base renders → ui-router navigates (main.main-menu default; oscDisplayService._open
  defaults to "main.main-menu", "base" is the transition workaround when already there)
```

Confidence note: resolve bodies are minified but the anchors
(`Request NodeInfo`, `attempting to resolve localized config`) are literals.
