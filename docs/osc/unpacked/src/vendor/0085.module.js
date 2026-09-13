// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 85
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(13),
    i = require(86).f,
    o = {}.toString,
    a = "object" == typeof window && window && Object.getOwnPropertyNames ? Object.getOwnPropertyNames(
    window) : [],
    s = function(e) {
      try {
        return i(e);
      } catch (e) {
        return a.slice();
      }
    };
  module.exports.f = function(e) {
    return a && "[object Window]" == o.call(e) ? s(e) : i(r(e));
  };
}
