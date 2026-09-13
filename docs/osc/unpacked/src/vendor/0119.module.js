// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 119
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";

  function n() {
    try {
      return "undefined" != typeof localStorage && "setItem" in localStorage && localStorage.setItem;
    } catch (e) {
      return !1;
    }
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.default = n;
}
