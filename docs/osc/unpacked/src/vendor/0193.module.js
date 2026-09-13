// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 193
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(9),
    i = vendorModule /* vendor bundle require */,
    o = require(17);
  module.exports = require(6) ? Object.defineProperties : function(e, t) {
    i(e);
    for (var n, a = o(t), s = a.length, c = 0; s > c;) r.f(e, n = a[c++], t[n]);
    return e;
  };
}
