// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 199
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(56),
    i = n(49);
  e.exports = function(e) {
    return function(t, n) {
      var o, a, s = String(i(t)),
        c = r(n),
        u = s.length;
      return c < 0 || c >= u ? e ? "" : void 0 : (o = s.charCodeAt(c), o < 55296 || o > 56319 || c + 1 === u || (a =
        s.charCodeAt(c + 1)) < 56320 || a > 57343 ? e ? s.charAt(c) : o : e ? s.slice(c, c + 2) : (o - 55296 <<
        10) + (a - 56320) + 65536)
    }
  }
}
