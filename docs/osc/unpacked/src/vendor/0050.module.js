// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 50
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(12),
    i = require(4).document,
    o = r(i) && r(i.createElement);
  module.exports = function(e) {
    return o ? i.createElement(e) : {};
  };
}
