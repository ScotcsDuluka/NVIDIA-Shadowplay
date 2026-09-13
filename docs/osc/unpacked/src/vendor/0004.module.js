// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 4
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  var n = module.exports = "undefined" != typeof window && window.Math == Math ? window : "undefined" !=
    typeof self && self.Math == Math ? self : Function("return this")();
  "number" == typeof __g && (__g = n);
}
