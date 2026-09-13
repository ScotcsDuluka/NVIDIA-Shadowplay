# Source-like reconstruction

`src/vendor/*.js`, `src/app/*.js` — one file per webpack module, factory
params renamed to **module / exports / require** (scope-aware via babel,
shadowed inner names untouched). Every `require(N)` is annotated with the
target module's name; app modules reach vendor modules through
`app/7._vendor-bridge.js` (`module.exports = vendor`).

Reads like CommonJS source. What cannot be recovered: original local
variable names (no source maps exist) — everything the code declares,
registers, or sends over the wire is original.

- `vendor/INDEX.md`, `app/INDEX.md` — id → file → role tables
- names: `../names.json`
- templates/CSS dumps: `../app/templates`, `../app/styles`, `../vendor/styles`
