// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 36
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(35);
  module.exports = function(e, t, n) {
    if (r(e), void 0 === t) return e;
    switch (n) {
      case 1:
        return function(n) {
          return e.call(t, n);
        };
      case 2:
        return function(n, r) {
          return e.call(t, n, r);
        };
      case 3:
        return function(n, r, i) {
          return e.call(t, n, r, i);
        };
    }
    return function() {
      return e.apply(t, arguments);
    };
  };
}
