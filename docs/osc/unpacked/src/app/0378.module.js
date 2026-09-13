// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 378
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(13),
    o = i.JSON || (i.JSON = {
      stringify: JSON.stringify
    });
  module.exports = function(e) {
    return o.stringify.apply(o, arguments);
  };
}
