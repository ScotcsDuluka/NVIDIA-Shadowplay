// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 95
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(48),
    i = require(5)("iterator"),
    o = require(16);
  module.exports = require(2).getIteratorMethod = function(e) {
    if (void 0 != e) return e[i] || e["@@iterator"] || o[r(e)];
  };
}
