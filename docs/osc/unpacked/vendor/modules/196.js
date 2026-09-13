// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 196
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(6),
    i = n(17),
    o = n(13),
    a = n(29).f;
  e.exports = function(e) {
    return function(t) {
      for (var n, s = o(t), c = i(s), u = c.length, l = 0, d = []; u > l;) n = c[l++], r && !a.call(s, n) || d.push(
        e ? [n, s[n]] : s[n]);
      return d
    }
  }
}
