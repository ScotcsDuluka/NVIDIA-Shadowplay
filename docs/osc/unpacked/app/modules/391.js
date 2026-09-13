// ─────────────────────────────────────────────────────────────
// APP MODULE 391
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(37),
    o = n(56),
    r = n(411);
  e.exports = function(e) {
    return function(t, n, a) {
      var l, s = i(t),
        d = o(s.length),
        c = r(a, d);
      if (e && n != n) {
        for (; d > c;)
          if (l = s[c++], l != l) return !0
      } else
        for (; d > c; c++)
          if ((e || c in s) && s[c] === n) return e || c || 0;
      return !e && -1
    }
  }
}
