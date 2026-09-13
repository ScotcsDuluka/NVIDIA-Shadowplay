// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 412
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(28),
    o = require(79);
  module.exports = require(13).getIterator = function(e) {
    var t = o(e);
    if ("function" != typeof t) throw TypeError(e + " is not iterable!");
    return i(t.call(e));
  };
}
