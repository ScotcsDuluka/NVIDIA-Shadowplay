// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 89
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports = function(e) {
    try {
      return {
        e: !1,
        v: e()
      };
    } catch (e) {
      return {
        e: !0,
        v: e
      };
    }
  };
}
