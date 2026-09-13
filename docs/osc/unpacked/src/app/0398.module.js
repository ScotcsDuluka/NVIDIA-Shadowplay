// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 398
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(19),
    o = require(43);
  module.exports = function(e, t, n) {
    t in e ? i.f(e, t, o(0, n)) : e[t] = n;
  };
}
