// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 230
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(3))
  }(this, function(e) {
    return e.pad.ZeroPadding = {
      pad: function(e, t) {
        var n = 4 * t;
        e.clamp(), e.sigBytes += n - (e.sigBytes % n || n)
      },
      unpad: function(e) {
        for (var t = e.words, n = e.sigBytes - 1; !(t[n >>> 2] >>> 24 - n % 4 * 8 & 255);) n--;
        e.sigBytes = n + 1
      }
    }, e.pad.ZeroPadding
  })
}
