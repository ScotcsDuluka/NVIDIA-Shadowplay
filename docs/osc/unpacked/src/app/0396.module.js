// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 396
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(113),
    o = require(390);
  module.exports = function(e) {
    return function() {
      if (i(this) != e) throw TypeError(e + "#toJSON isn't generic");
      return o(this);
    };
  };
}
