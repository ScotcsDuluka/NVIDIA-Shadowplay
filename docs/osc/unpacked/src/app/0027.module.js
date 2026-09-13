// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 27
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(19),
    o = require(43);
  module.exports = require(18) ? function(e, t, n) {
    return i.f(e, t, o(1, n));
  } : function(e, t, n) {
    return e[t] = n, e;
  };
}
