// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 420
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(24),
    o = require(53).onFreeze;
  require(72)("freeze", function(e) {
    return function(t) {
      return e && i(t) ? e(o(t)) : t;
    };
  });
}
