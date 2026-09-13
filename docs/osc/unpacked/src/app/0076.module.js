// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 76
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(24);
  module.exports = function(e, t) {
    if (!i(e)) return e;
    var n, o;
    if (t && "function" == typeof(n = e.toString) && !i(o = n.call(e))) return o;
    if ("function" == typeof(n = e.valueOf) && !i(o = n.call(e))) return o;
    if (!t && "function" == typeof(n = e.toString) && !i(o = n.call(e))) return o;
    throw TypeError("Can't convert object to primitive value");
  };
}
