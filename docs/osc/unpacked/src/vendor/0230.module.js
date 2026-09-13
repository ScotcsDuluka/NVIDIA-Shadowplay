// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 230
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(3));
  }(this, function(e) {
    return e.pad.ZeroPadding = {
      pad: function(e, t) {
        var n = 4 * t;
        e.clamp(), e.sigBytes += n - (e.sigBytes % n || n);
      },
      unpad: function(e) {
        for (var t = e.words, n = e.sigBytes - 1; !(t[n >>> 2] >>> 24 - n % 4 * 8 & 255);) n--;
        e.sigBytes = n + 1;
      }
    }, e.pad.ZeroPadding;
  });
}
