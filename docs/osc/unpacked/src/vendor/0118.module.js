// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 118
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

  function i() {
    try {
      return !!a.default && !("undefined" != typeof openDatabase && "undefined" != typeof navigator &&
          navigator.userAgent && /Safari/.test(navigator.userAgent) && !/Chrome/.test(navigator.userAgent)) &&
        a.default && "function" == typeof a.default.open && "undefined" != typeof IDBKeyRange;
    } catch (e) {
      return !1;
    }
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var o = require(74),
    a = r(o);
  exports.default = i;
}
