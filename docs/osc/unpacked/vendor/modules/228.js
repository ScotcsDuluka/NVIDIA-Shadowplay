// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 228
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(3))
  }(this, function(e) {
    return e.pad.Iso97971 = {
      pad: function(t, n) {
        t.concat(e.lib.WordArray.create([2147483648], 1)), e.pad.ZeroPadding.pad(t, n)
      },
      unpad: function(t) {
        e.pad.ZeroPadding.unpad(t), t.sigBytes--
      }
    }, e.pad.Iso97971
  })
}
