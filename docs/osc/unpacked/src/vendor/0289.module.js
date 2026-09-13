// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 289
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports = function(e) {
    return e.webpackPolyfill || (e.deprecate = function() {}, e.paths = [], e.children = [], e
      .webpackPolyfill = 1), e;
  };
}
