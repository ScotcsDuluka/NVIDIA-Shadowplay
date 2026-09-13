// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 74
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";

  function n() {
    return "undefined" != typeof indexedDB ? indexedDB : "undefined" != typeof webkitIndexedDB ?
      webkitIndexedDB : "undefined" != typeof mozIndexedDB ? mozIndexedDB : "undefined" != typeof OIndexedDB ?
      OIndexedDB : "undefined" != typeof msIndexedDB ? msIndexedDB : void 0;
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  });
  var r = n();
  exports.default = r;
}
