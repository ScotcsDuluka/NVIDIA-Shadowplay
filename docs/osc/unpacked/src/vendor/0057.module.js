// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 57
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(12);
  module.exports = function(e, t) {
    if (!r(e)) return e;
    var n, i;
    if (t && "function" == typeof(n = e.toString) && !r(i = n.call(e))) return i;
    if ("function" == typeof(n = e.valueOf) && !r(i = n.call(e))) return i;
    if (!t && "function" == typeof(n = e.toString) && !r(i = n.call(e))) return i;
    throw TypeError("Can't convert object to primitive value");
  };
}
