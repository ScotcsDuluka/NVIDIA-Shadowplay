// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 203
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(48),
    i = require(5)("iterator"),
    o = require(16);
  module.exports = require(2).isIterable = function(e) {
    var t = Object(e);
    return void 0 !== t[i] || "@@iterator" in t || o.hasOwnProperty(r(t));
  };
}
