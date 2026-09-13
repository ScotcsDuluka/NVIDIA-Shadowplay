// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 34
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports = function(e, t) {
    var n = function() {};
    n.prototype = t.prototype, e.prototype = new n(), e.prototype.constructor = e;
  };
}
