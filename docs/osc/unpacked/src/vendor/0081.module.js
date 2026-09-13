// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 81
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  module.exports = !require(6) && !require(15)(function() {
    return 7 != Object.defineProperty(require(50)("div"), "a", {
      get: function() {
        return 7;
      }
    }).a;
  });
}
