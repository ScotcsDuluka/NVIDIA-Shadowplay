// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 65
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  var n = {}.toString;
  module.exports = function(e) {
    return n.call(e).slice(8, -1);
  };
}
