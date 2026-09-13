// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 182
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(36),
    i = n(186),
    o = n(184),
    a = n(7),
    s = n(94),
    c = n(95),
    u = {},
    l = {},
    t = e.exports = function(e, t, n, d, f) {
      var h, p, m, v, g = f ? function() {
          return e
        } : c(e),
        y = r(n, d, t ? 2 : 1),
        b = 0;
      if ("function" != typeof g) throw TypeError(e + " is not iterable!");
      if (o(g)) {
        for (h = s(e.length); h > b; b++)
          if (v = t ? y(a(p = e[b])[0], p[1]) : y(e[b]), v === u || v === l) return v
      } else
        for (m = g.call(e); !(p = m.next()).done;)
          if (v = i(m, y, p.value, t), v === u || v === l) return v
    };
  t.BREAK = u, t.RETURN = l
}
