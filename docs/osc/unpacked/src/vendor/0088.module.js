// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 88
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(8),
    i = require(2),
    o = require(15);
  module.exports = function(e, t) {
    var n = (i.Object || {})[e] || Object[e],
      a = {};
    a[e] = t(n), r(r.S + r.F * o(function() {
      n(1);
    }), "Object", a);
  };
}
