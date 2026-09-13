// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 180
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(13),
    i = n(94),
    o = n(200);
  e.exports = function(e) {
    return function(t, n, a) {
      var s, c = r(t),
        u = i(c.length),
        l = o(a, u);
      if (e && n != n) {
        for (; u > l;)
          if (s = c[l++], s != s) return !0
      } else
        for (; u > l; l++)
          if ((e || l in c) && c[l] === n) return e || l || 0;
      return !e && -1
    }
  }
}
