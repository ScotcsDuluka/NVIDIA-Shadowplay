// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 73
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(74)("keys"),
    o = require(57);
  module.exports = function(e) {
    return i[e] || (i[e] = o(e));
  };
}
