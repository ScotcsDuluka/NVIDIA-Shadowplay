// ─────────────────────────────────────────────────────────────
// APP MODULE 399
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(42),
    o = n(71),
    r = n(54);
  e.exports = function(e) {
    var t = i(e),
      n = o.f;
    if (n)
      for (var a, l = n(e), s = r.f, d = 0; l.length > d;) s.call(e, a = l[d++]) && t.push(a);
    return t
  }
}
