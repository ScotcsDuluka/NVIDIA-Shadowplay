// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 38
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(9).f,
    i = require(10),
    o = require(5)("toStringTag");
  module.exports = function(e, t, n) {
    e && !i(e = n ? e : e.prototype, o) && r(e, o, {
      configurable: !0,
      value: t
    });
  };
}
