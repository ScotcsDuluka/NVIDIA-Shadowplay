// ─────────────────────────────────────────────────────────────
// APP MODULE 410
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(75),
    o = n(66);
  e.exports = function(e) {
    return function(t, n) {
      var r, a, l = String(o(t)),
        s = i(n),
        d = l.length;
      return s < 0 || s >= d ? e ? "" : void 0 : (r = l.charCodeAt(s), r < 55296 || r > 56319 || s + 1 === d || (a =
        l.charCodeAt(s + 1)) < 56320 || a > 57343 ? e ? l.charAt(s) : r : e ? l.slice(s, s + 2) : (r - 55296 <<
        10) + (a - 56320) + 65536)
    }
  }
}
