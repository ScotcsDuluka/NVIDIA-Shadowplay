// ─────────────────────────────────────────────────────────────
// APP MODULE 51
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(36),
    o = n(118),
    r = n(116),
    a = n(28),
    l = n(56),
    s = n(79),
    d = {},
    c = {},
    t = e.exports = function(e, t, n, u, f) {
      var m, g, p, h, b = f ? function() {
          return e
        } : s(e),
        x = i(n, u, t ? 2 : 1),
        v = 0;
      if ("function" != typeof b) throw TypeError(e + " is not iterable!");
      if (r(b)) {
        for (m = l(e.length); m > v; v++)
          if (h = t ? x(a(g = e[v])[0], g[1]) : x(e[v]), h === d || h === c) return h
      } else
        for (p = b.call(e); !(g = p.next()).done;)
          if (h = o(p, x, g.value, t), h === d || h === c) return h
    };
  t.BREAK = d, t.RETURN = c
}
