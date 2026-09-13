// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 411
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(75),
    o = Math.max,
    r = Math.min;
  module.exports = function(e, t) {
    return e = i(e), e < 0 ? o(e + t, 0) : r(e, t);
  };
}
