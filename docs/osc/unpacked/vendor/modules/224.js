// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 224
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(3))
  }(this, function(e) {
    return e.mode.ECB = function() {
      var t = e.lib.BlockCipherMode.extend();
      return t.Encryptor = t.extend({
        processBlock: function(e, t) {
          this._cipher.encryptBlock(e, t)
        }
      }), t.Decryptor = t.extend({
        processBlock: function(e, t) {
          this._cipher.decryptBlock(e, t)
        }
      }), t
    }(), e.mode.ECB
  })
}
