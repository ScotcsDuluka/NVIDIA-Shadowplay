// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 404
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(19),
    o = require(28),
    r = require(42);
  module.exports = require(18) ? Object.defineProperties : function(e, t) {
    o(e);
    for (var n, a = r(t), l = a.length, s = 0; l > s;) i.f(e, n = a[s++], t[n]);
    return e;
  };
}
