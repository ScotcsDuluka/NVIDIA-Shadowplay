// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 226
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(3))
  }(this, function(e) {
    return e.pad.AnsiX923 = {
      pad: function(e, t) {
        var n = e.sigBytes,
          r = 4 * t,
          i = r - n % r,
          o = n + i - 1;
        e.clamp(), e.words[o >>> 2] |= i << 24 - o % 4 * 8, e.sigBytes += i
      },
      unpad: function(e) {
        var t = 255 & e.words[e.sigBytes - 1 >>> 2];
        e.sigBytes -= t
      }
    }, e.pad.Ansix923
  })
}
