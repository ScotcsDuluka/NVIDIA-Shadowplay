// ─────────────────────────────────────────────────────────────
// APP MODULE 122
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(30),
    o = n(37),
    r = n(391)(!1),
    a = n(73)("IE_PROTO");
  e.exports = function(e, t) {
    var n, l = o(e),
      s = 0,
      d = [];
    for (n in l) n != a && i(l, n) && d.push(n);
    for (; t.length > s;) i(l, n = t[s++]) && (~r(d, n) || d.push(n));
    return d
  }
}
