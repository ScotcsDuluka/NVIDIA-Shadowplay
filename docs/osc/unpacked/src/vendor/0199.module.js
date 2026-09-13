// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 199
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(56),
    i = require(49);
  module.exports = function(e) {
    return function(t, n) {
      var o,
        a,
        s = String(i(t)),
        c = r(n),
        u = s.length;
      return c < 0 || c >= u ? e ? "" : void 0 : (o = s.charCodeAt(c), o < 55296 || o > 56319 || c + 1 ===
        u || (a = s.charCodeAt(c + 1)) < 56320 || a > 57343 ? e ? s.charAt(c) : o : e ? s.slice(c, c +
          2) : (o - 55296 << 10) + (a - 56320) + 65536);
    };
  };
}
