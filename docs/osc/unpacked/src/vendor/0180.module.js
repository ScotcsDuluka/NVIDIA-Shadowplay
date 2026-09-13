// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 180
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(13),
    i = require(94),
    o = require(200);
  module.exports = function(e) {
    return function(t, n, a) {
      var s,
        c = r(t),
        u = i(c.length),
        l = o(a, u);
      if (e && n != n) {
        for (; u > l;)
          if (s = c[l++], s != s) return !0;
      } else
        for (; u > l; l++)
          if ((e || l in c) && c[l] === n) return e || l || 0;
      return !e && -1;
    };
  };
}
