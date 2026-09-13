// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 202
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = vendorModule /* vendor bundle require */,
    i = require(95);
  module.exports = require(2).getIterator = function(e) {
    var t = i(e);
    if ("function" != typeof t) throw TypeError(e + " is not iterable!");
    return r(t.call(e));
  };
}
