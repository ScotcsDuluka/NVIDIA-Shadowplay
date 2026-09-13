// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 57
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  var n = 0,
    i = Math.random();
  module.exports = function(e) {
    return "Symbol(".concat(void 0 === e ? "" : e, ")_", (++n + i).toString(36));
  };
}
