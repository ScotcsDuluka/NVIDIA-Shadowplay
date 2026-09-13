// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 40
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  var n = 0,
    r = Math.random();
  module.exports = function(e) {
    return "Symbol(".concat(void 0 === e ? "" : e, ")_", (++n + r).toString(36));
  };
}
