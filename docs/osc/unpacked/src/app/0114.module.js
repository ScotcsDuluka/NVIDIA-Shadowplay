// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 114
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(24),
    o = require(21).document,
    r = i(o) && i(o.createElement);
  module.exports = function(e) {
    return r ? o.createElement(e) : {};
  };
}
