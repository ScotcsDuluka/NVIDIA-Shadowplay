// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 90
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = vendorModule /* vendor bundle require */,
    i = require(12),
    o = require(52);
  module.exports = function(e, t) {
    if (r(e), i(t) && t.constructor === e) return t;
    var n = o.f(e),
      a = n.resolve;
    return a(t), n.promise;
  };
}
