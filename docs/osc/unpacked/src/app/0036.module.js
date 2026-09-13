// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 36
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(111);
  module.exports = function(e, t, n) {
    if (i(e), void 0 === t) return e;
    switch (n) {
      case 1:
        return function(n) {
          return e.call(t, n);
        };
      case 2:
        return function(n, i) {
          return e.call(t, n, i);
        };
      case 3:
        return function(n, i, o) {
          return e.call(t, n, i, o);
        };
    }
    return function() {
      return e.apply(t, arguments);
    };
  };
}
