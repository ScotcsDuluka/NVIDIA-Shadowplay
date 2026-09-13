// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 55
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(19).f,
    o = require(30),
    r = require(16)("toStringTag");
  module.exports = function(e, t, n) {
    e && !o(e = n ? e : e.prototype, r) && i(e, r, {
      configurable: !0,
      value: t
    });
  };
}
