// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 58
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(4),
    i = n(2),
    o = n(28),
    a = n(59),
    s = n(9).f;
  e.exports = function(e) {
    var t = i.Symbol || (i.Symbol = o ? {} : r.Symbol || {});
    "_" == e.charAt(0) || e in t || s(t, e, {
      value: a.f(e)
    })
  }
}
