// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 228
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(3));
  }(this, function(e) {
    return e.pad.Iso97971 = {
      pad: function(t, n) {
        t.concat(e.lib.WordArray.create([2147483648], 1)), e.pad.ZeroPadding.pad(t, n);
      },
      unpad: function(t) {
        e.pad.ZeroPadding.unpad(t), t.sigBytes--;
      }
    }, e.pad.Iso97971;
  });
}
