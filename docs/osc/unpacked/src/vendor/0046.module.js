// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 46
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";

  function n(e, t) {
    t && e.then(function(e) {
      t(null, e);
    }, function(e) {
      t(e);
    });
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.default = n;
}
