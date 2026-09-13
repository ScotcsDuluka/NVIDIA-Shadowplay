// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 227
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(3))
  }(this, function(e) {
    return e.pad.Iso10126 = {
      pad: function(t, n) {
        var r = 4 * n,
          i = r - t.sigBytes % r;
        t.concat(e.lib.WordArray.random(i - 1)).concat(e.lib.WordArray.create([i << 24], 1))
      },
      unpad: function(e) {
        var t = 255 & e.words[e.sigBytes - 1 >>> 2];
        e.sigBytes -= t
      }
    }, e.pad.Iso10126
  })
}
