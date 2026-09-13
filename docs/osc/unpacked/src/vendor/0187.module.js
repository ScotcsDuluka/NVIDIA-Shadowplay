// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 187
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var r = require(84),
    i = require(37),
    o = require(38),
    a = {};
  require(11)(a, require(5)("iterator"), function() {
    return this;
  }), module.exports = function(e, t, n) {
    e.prototype = r(a, {
      next: i(1, n)
    }), o(e, t + " Iterator");
  };
}
