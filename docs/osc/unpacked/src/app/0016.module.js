// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 16
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(74)("wks"),
    o = require(57),
    r = require(21).Symbol,
    a = "function" == typeof r,
    l = module.exports = function(e) {
      return i[e] || (i[e] = a && r[e] || (a ? r : o)("Symbol." + e));
    };
  l.store = i;
}
