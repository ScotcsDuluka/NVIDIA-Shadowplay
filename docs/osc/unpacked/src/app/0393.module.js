// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 393
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(24),
    o = require(117),
    r = require(16)("species");
  module.exports = function(e) {
    var t;
    return o(e) && (t = e.constructor, "function" != typeof t || t !== Array && !o(t.prototype) || (t =
      void 0), i(t) && (t = t[r], null === t && (t = void 0))), void 0 === t ? Array : t;
  };
}
