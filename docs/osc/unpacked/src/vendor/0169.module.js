// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 169
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(2),
    i = r.JSON || (r.JSON = {
      stringify: JSON.stringify
    });
  module.exports = function(e) {
    return i.stringify.apply(i, arguments);
  };
}
