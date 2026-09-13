// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 125
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(24);
  module.exports = function(e, t) {
    if (!i(e) || e._t !== t) throw TypeError("Incompatible receiver, " + t + " required!");
    return e;
  };
}
