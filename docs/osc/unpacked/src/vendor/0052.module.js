// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 52
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function r(e) {
    var t, n;
    this.promise = new e(function(e, r) {
      if (void 0 !== t || void 0 !== n) throw TypeError("Bad Promise constructor");
      t = e, n = r;
    }), this.resolve = i(t), this.reject = i(n);
  }
  var i = require(35);
  module.exports.f = function(e) {
    return new r(e);
  };
}
