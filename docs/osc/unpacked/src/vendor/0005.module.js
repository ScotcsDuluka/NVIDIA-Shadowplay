// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 5
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(55)("wks"),
    i = require(40),
    o = require(4).Symbol,
    a = "function" == typeof o,
    s = module.exports = function(e) {
      return r[e] || (r[e] = a && o[e] || (a ? o : i)("Symbol." + e));
    };
  s.store = r;
}
