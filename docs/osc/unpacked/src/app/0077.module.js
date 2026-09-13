// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 77
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(21),
    o = require(13),
    r = require(52),
    a = require(78),
    l = require(19).f;
  module.exports = function(e) {
    var t = o.Symbol || (o.Symbol = r ? {} : i.Symbol || {});
    "_" == e.charAt(0) || e in t || l(t, e, {
      value: a.f(e)
    });
  };
}
