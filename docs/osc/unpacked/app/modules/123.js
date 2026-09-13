// ─────────────────────────────────────────────────────────────
// APP MODULE 123
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(27);
  e.exports = function(e, t, n) {
    for (var o in t) n && e[o] ? e[o] = t[o] : i(e, o, t[o]);
    return e
  }
}
