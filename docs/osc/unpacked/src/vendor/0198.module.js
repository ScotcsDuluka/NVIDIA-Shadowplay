// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 198
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var r = require(4),
    i = require(2),
    o = require(9),
    a = require(6),
    s = require(5)("species");
  module.exports = function(e) {
    var t = "function" == typeof i[e] ? i[e] : r[e];
    a && t && !t[s] && o.f(t, s, {
      configurable: !0,
      get: function() {
        return this;
      }
    });
  };
}
