// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 72
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(15),
    o = require(13),
    r = require(29);
  module.exports = function(e, t) {
    var n = (o.Object || {})[e] || Object[e],
      a = {};
    a[e] = t(n), i(i.S + i.F * r(function() {
      n(1);
    }), "Object", a);
  };
}
