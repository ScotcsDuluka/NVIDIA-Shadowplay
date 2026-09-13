// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 416
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(15);
  i(i.S, "Math", {
    trunc: function(e) {
      return (e > 0 ? Math.floor : Math.ceil)(e);
    }
  });
}
