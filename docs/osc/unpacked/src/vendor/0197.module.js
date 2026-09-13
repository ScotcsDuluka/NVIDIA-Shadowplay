// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 197
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(11);
  module.exports = function(e, t, n) {
    for (var i in t) n && e[i] ? e[i] = t[i] : r(e, i, t[i]);
    return e;
  };
}
