// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 79
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(113),
    o = require(16)("iterator"),
    r = require(41);
  module.exports = require(13).getIteratorMethod = function(e) {
    if (void 0 != e) return e[o] || e["@@iterator"] || r[i(e)];
  };
}
