// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 193
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(9),
    i = n(7),
    o = n(17);
  e.exports = n(6) ? Object.defineProperties : function(e, t) {
    i(e);
    for (var n, a = o(t), s = a.length, c = 0; s > c;) r.f(e, n = a[c++], t[n]);
    return e
  }
}
