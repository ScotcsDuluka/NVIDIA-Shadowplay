// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 401
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(70),
    o = require(43),
    r = require(55),
    a = {};
  require(27)(a, require(16)("iterator"), function() {
    return this;
  }), module.exports = function(e, t, n) {
    e.prototype = i(a, {
      next: o(1, n)
    }), r(e, t + " Iterator");
  };
}
