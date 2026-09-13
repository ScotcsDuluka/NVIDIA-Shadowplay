// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 391
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(37),
    o = require(56),
    r = require(411);
  module.exports = function(e) {
    return function(t, n, a) {
      var l,
        s = i(t),
        d = o(s.length),
        c = r(a, d);
      if (e && n != n) {
        for (; d > c;)
          if (l = s[c++], l != l) return !0;
      } else
        for (; d > c; c++)
          if ((e || c in s) && s[c] === n) return e || c || 0;
      return !e && -1;
    };
  };
}
