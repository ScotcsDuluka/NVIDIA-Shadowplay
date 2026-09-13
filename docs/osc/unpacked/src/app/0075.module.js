// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 75
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  var n = Math.ceil,
    i = Math.floor;
  module.exports = function(e) {
    return isNaN(e = +e) ? 0 : (e > 0 ? i : n)(e);
  };
}
