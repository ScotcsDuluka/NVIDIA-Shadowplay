// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 121
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(30),
    o = require(31),
    r = require(73)("IE_PROTO"),
    a = Object.prototype;
  module.exports = Object.getPrototypeOf || function(e) {
    return e = o(e), i(e, r) ? e[r] : "function" == typeof e.constructor && e instanceof e.constructor ? e
      .constructor.prototype : e instanceof Object ? a : null;
  };
}
