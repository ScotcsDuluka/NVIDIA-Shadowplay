# Unpacked legacy OSC bundles

Generated from `Overlay/osc/vendor.js` + `app.js` (minified webpack 1.x,
module id = array index; no source maps exist). Local variable names inside
functions remain minified (`e, t, n`) — everything the code *says* (Angular
registrations, templates, CSS, protocol strings, l10n keys) is intact.

## Start here: [`src/`](src/README.md) — source-like reconstruction

`src/vendor/*.js` + `src/app/*.js` — **one file per module, renamed to real
webpack semantics** (`function(module, exports, require)`), filenames carry
the real Angular name (`0004.shadowPlayService.js`, `0020.socketService.js`…),
and every `require(N)` is annotated with the target's name — including
cross-bundle calls (`vendorModule(125) /* vendor/125 — applicationLifetimeService */`
via `0007._vendor-bridge.js`). Navigate with `src/<bundle>/INDEX.md`.

## Navigation (raw dumps)

1. `names.json` — Angular name (service/directive/controller/…) → module file.
   Start here: `"cefService" → vendor/<id>`, `"socketService" → app/<id>`, …
2. `<bundle>/INDEX.md` — every module id with detected role + requires graph.
3. `<bundle>/modules/<id>.js` — the beautified module; header lists role,
   requires, QUERY_* commands, socket channels it touches.
4. `<bundle>/templates/` — inline Angular templates extracted to HTML.
5. `<bundle>/styles/` — CSS recovered from style-loader strings (heuristic).
6. `common.beautified.js` — the webpack runtime.

## Counts

| bundle | modules | templates | css blobs |
|---|---:|---:|---:|
| vendor | 291 | 1 | 5 |
| app    | 487 | 27 | 24 |

## Notes

- `app/7`-style bridge: app modules call `n(7)(<id>)` to reach vendor
  modules — INDEX shows those as `vendor/<id>`.
- Some vendor "modules" are webpack shims (module merging, polyfills);
  their headers will read `utility`.
- This tree is derived material for reading — the executable truth remains
  the original bundle; do not edit files here and expect the app to change.
