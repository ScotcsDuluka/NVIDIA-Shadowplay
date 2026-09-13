// ─────────────────────────────────────────────────────────────
// APP MODULE 403
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(18),
    o = n(42),
    r = n(71),
    a = n(54),
    l = n(31),
    s = n(68),
    d = Object.assign;
  e.exports = !d || n(29)(function() {
    var e = {},
      t = {},
      n = Symbol(),
      i = "abcdefghijklmnopqrst";
    return e[n] = 7, i.split("").forEach(function(e) {
      t[e] = e
    }), 7 != d({}, e)[n] || Object.keys(d({}, t)).join("") != i
  }) ? function(e, t) {
    for (var n = l(e), d = arguments.length, c = 1, u = r.f, f = a.f; d > c;)
      for (var m, g = s(arguments[c++]), p = u ? o(g).concat(u(g)) : o(g), h = p.length, b = 0; h > b;) m = p[b++],
        i && !f.call(g, m) || (n[m] = g[m]);
    return n
  } : d
}
