// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 192
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var r = n(6),
    i = n(17),
    o = n(53),
    a = n(29),
    s = n(39),
    c = n(82),
    u = Object.assign;
  e.exports = !u || n(15)(function() {
    var e = {},
      t = {},
      n = Symbol(),
      r = "abcdefghijklmnopqrst";
    return e[n] = 7, r.split("").forEach(function(e) {
      t[e] = e
    }), 7 != u({}, e)[n] || Object.keys(u({}, t)).join("") != r
  }) ? function(e, t) {
    for (var n = s(e), u = arguments.length, l = 1, d = o.f, f = a.f; u > l;)
      for (var h, p = c(arguments[l++]), m = d ? i(p).concat(d(p)) : i(p), v = m.length, g = 0; v > g;) h = m[g++],
        r && !f.call(p, h) || (n[h] = p[h]);
    return n
  } : u
}
