// ─────────────────────────────────────────────────────────────
// APP MODULE 79
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(113),
    o = n(16)("iterator"),
    r = n(41);
  e.exports = n(13).getIteratorMethod = function(e) {
    if (void 0 != e) return e[o] || e["@@iterator"] || r[i(e)]
  }
}
