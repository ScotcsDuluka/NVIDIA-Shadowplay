// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 106
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  function n(e, t, n) {
    return e.on(t, n), {
      destroy: function() {
        e.removeListener(t, n);
      }
    };
  }
  module.exports = n;
}
