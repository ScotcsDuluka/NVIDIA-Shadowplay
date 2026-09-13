// ─────────────────────────────────────────────────────────────
// APP MODULE 16
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(74)("wks"),
    o = n(57),
    r = n(21).Symbol,
    a = "function" == typeof r,
    l = e.exports = function(e) {
      return i[e] || (i[e] = a && r[e] || (a ? r : o)("Symbol." + e))
    };
  l.store = i
}
