// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 113
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(65),
    o = require(16)("toStringTag"),
    r = "Arguments" == i(function() {
      return arguments;
    }()),
    a = function(e, t) {
      try {
        return e[t];
      } catch (e) {}
    };
  module.exports = function(e) {
    var t, n, l;
    return void 0 === e ? "Undefined" : null === e ? "Null" : "string" == typeof(n = a(t = Object(e), o)) ?
      n : r ? i(t) : "Object" == (l = i(t)) && "function" == typeof t.callee ? "Arguments" : l;
  };
}
