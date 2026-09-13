// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 92
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = vendorModule /* vendor bundle require */,
    i = require(35),
    o = require(5)("species");
  module.exports = function(e, t) {
    var n,
      a = r(e).constructor;
    return void 0 === a || void 0 == (n = r(a)[o]) ? t : i(n);
  };
}
