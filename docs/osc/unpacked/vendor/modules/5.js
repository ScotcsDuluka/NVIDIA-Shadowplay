// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 5
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(55)("wks"),
    i = n(40),
    o = n(4).Symbol,
    a = "function" == typeof o,
    s = e.exports = function(e) {
      return r[e] || (r[e] = a && o[e] || (a ? o : i)("Symbol." + e))
    };
  s.store = r
}
