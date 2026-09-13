// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 90
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(7),
    i = n(12),
    o = n(52);
  e.exports = function(e, t) {
    if (r(e), i(t) && t.constructor === e) return t;
    var n = o.f(e),
      a = n.resolve;
    return a(t), n.promise
  }
}
