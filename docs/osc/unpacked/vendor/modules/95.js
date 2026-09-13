// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 95
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(48),
    i = n(5)("iterator"),
    o = n(16);
  e.exports = n(2).getIteratorMethod = function(e) {
    if (void 0 != e) return e[i] || e["@@iterator"] || o[r(e)]
  }
}
