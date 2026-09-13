// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 409
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(21),
    o = require(13),
    r = require(19),
    a = require(18),
    l = require(16)("species");
  module.exports = function(e) {
    var t = "function" == typeof o[e] ? o[e] : i[e];
    a && t && !t[l] && r.f(t, l, {
      configurable: !0,
      get: function() {
        return this;
      }
    });
  };
}
