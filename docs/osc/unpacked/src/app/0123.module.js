// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 123
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(27);
  module.exports = function(e, t, n) {
    for (var o in t) n && e[o] ? e[o] = t[o] : i(e, o, t[o]);
    return e;
  };
}
