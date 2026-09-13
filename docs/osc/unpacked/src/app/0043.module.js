// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 43
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports = function(e, t) {
    return {
      enumerable: !(1 & e),
      configurable: !(2 & e),
      writable: !(4 & e),
      value: t
    };
  };
}
