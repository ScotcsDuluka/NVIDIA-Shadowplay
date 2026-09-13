// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 58
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(4),
    i = require(2),
    o = require(28),
    a = require(59),
    s = require(9).f;
  module.exports = function(e) {
    var t = i.Symbol || (i.Symbol = o ? {} : r.Symbol || {});
    "_" == e.charAt(0) || e in t || s(t, e, {
      value: a.f(e)
    });
  };
}
