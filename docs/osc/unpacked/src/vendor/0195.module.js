// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 195
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(10),
    i = require(39),
    o = require(54)("IE_PROTO"),
    a = Object.prototype;
  module.exports = Object.getPrototypeOf || function(e) {
    return e = i(e), r(e, o) ? e[o] : "function" == typeof e.constructor && e instanceof e.constructor ? e
      .constructor.prototype : e instanceof Object ? a : null;
  };
}
