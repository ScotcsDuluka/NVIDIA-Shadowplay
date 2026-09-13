// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 87
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(10),
    i = n(13),
    o = n(180)(!1),
    a = n(54)("IE_PROTO");
  e.exports = function(e, t) {
    var n, s = i(e),
      c = 0,
      u = [];
    for (n in s) n != a && r(s, n) && u.push(n);
    for (; t.length > c;) r(s, n = t[c++]) && (~o(u, n) || u.push(n));
    return u
  }
}
