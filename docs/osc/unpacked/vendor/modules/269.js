// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 269
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  ! function(e) {
    "use strict";
    t.encode = function(t) {
      var n, r = new Uint8Array(t),
        i = r.length,
        o = "";
      for (n = 0; n < i; n += 3) o += e[r[n] >> 2], o += e[(3 & r[n]) << 4 | r[n + 1] >> 4], o += e[(15 & r[n + 1]) <<
        2 | r[n + 2] >> 6], o += e[63 & r[n + 2]];
      return i % 3 === 2 ? o = o.substring(0, o.length - 1) + "=" : i % 3 === 1 && (o = o.substring(0, o.length - 2) +
        "=="), o
    }, t.decode = function(t) {
      var n, r, i, o, a, s = .75 * t.length,
        c = t.length,
        u = 0;
      "=" === t[t.length - 1] && (s--, "=" === t[t.length - 2] && s--);
      var l = new ArrayBuffer(s),
        d = new Uint8Array(l);
      for (n = 0; n < c; n += 4) r = e.indexOf(t[n]), i = e.indexOf(t[n + 1]), o = e.indexOf(t[n + 2]), a = e.indexOf(
        t[n + 3]), d[u++] = r << 2 | i >> 4, d[u++] = (15 & i) << 4 | o >> 2, d[u++] = (3 & o) << 6 | 63 & a;
      return l
    }
  }("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/")
}
