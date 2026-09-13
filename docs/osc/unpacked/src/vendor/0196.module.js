// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 196
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(6),
    i = require(17),
    o = require(13),
    a = require(29).f;
  module.exports = function(e) {
    return function(t) {
      for (var n, s = o(t), c = i(s), u = c.length, l = 0, d = []; u > l;) n = c[l++], r && !a.call(s,
        n) || d.push(e ? [n, s[n]] : s[n]);
      return d;
    };
  };
}
