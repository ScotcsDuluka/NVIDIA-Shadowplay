// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 406
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(37),
    o = require(120).f,
    r = {}.toString,
    a = "object" == typeof window && window && Object.getOwnPropertyNames ? Object.getOwnPropertyNames(
    window) : [],
    l = function(e) {
      try {
        return o(e);
      } catch (e) {
        return a.slice();
      }
    };
  module.exports.f = function(e) {
    return a && "[object Window]" == r.call(e) ? l(e) : o(i(e));
  };
}
