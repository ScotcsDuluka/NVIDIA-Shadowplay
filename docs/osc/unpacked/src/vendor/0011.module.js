// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 11
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(9),
    i = require(37);
  module.exports = require(6) ? function(e, t, n) {
    return r.f(e, t, i(1, n));
  } : function(e, t, n) {
    return e[t] = n, e;
  };
}
