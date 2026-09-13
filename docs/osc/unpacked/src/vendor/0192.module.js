// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 192
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var r = require(6),
    i = require(17),
    o = require(53),
    a = require(29),
    s = require(39),
    c = require(82),
    u = Object.assign;
  module.exports = !u || require(15)(function() {
    var e = {},
      t = {},
      n = Symbol(),
      r = "abcdefghijklmnopqrst";
    return e[n] = 7, r.split("").forEach(function(e) {
      t[e] = e;
    }), 7 != u({}, e)[n] || Object.keys(u({}, t)).join("") != r;
  }) ? function(e, t) {
    for (var n = s(e), u = arguments.length, l = 1, d = o.f, f = a.f; u > l;)
      for (var h, p = c(arguments[l++]), m = d ? i(p).concat(d(p)) : i(p), v = m.length, g = 0; v > g;) h =
        m[g++], r && !f.call(p, h) || (n[h] = p[h]);
    return n;
  } : u;
}
