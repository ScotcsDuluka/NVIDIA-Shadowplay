// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 200
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(56),
    i = Math.max,
    o = Math.min;
  module.exports = function(e, t) {
    return e = r(e), e < 0 ? i(e + t, 0) : o(e, t);
  };
}
