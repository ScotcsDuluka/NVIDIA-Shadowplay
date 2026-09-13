// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 48
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(27),
    i = require(5)("toStringTag"),
    o = "Arguments" == r(function() {
      return arguments;
    }()),
    a = function(e, t) {
      try {
        return e[t];
      } catch (e) {}
    };
  module.exports = function(e) {
    var t, n, s;
    return void 0 === e ? "Undefined" : null === e ? "Null" : "string" == typeof(n = a(t = Object(e), i)) ?
      n : o ? r(t) : "Object" == (s = r(t)) && "function" == typeof t.callee ? "Arguments" : s;
  };
}
