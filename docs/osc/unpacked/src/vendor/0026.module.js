// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 26
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";

  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    };
  }
  exports.__esModule = !0;
  var i = require(158),
    o = r(i),
    a = require(157),
    s = r(a),
    c = "function" == typeof s.default && "symbol" == typeof o.default ? function(e) {
      return typeof e;
    } : function(e) {
      return e && "function" == typeof s.default && e.constructor === s.default && e !== s.default.prototype ?
        "symbol" : typeof e;
    };
  exports.default = "function" == typeof s.default && "symbol" === c(o.default) ? function(e) {
    return "undefined" == typeof e ? "undefined" : c(e);
  } : function(e) {
    return e && "function" == typeof s.default && e.constructor === s.default && e !== s.default.prototype ?
      "symbol" : "undefined" == typeof e ? "undefined" : c(e);
  };
}
