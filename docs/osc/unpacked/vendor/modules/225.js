// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 225
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(3))
  }(this, function(e) {
    return e.mode.OFB = function() {
      var t = e.lib.BlockCipherMode.extend(),
        n = t.Encryptor = t.extend({
          processBlock: function(e, t) {
            var n = this._cipher,
              r = n.blockSize,
              i = this._iv,
              o = this._keystream;
            i && (o = this._keystream = i.slice(0), this._iv = void 0), n.encryptBlock(o, 0);
            for (var a = 0; a < r; a++) e[t + a] ^= o[a]
          }
        });
      return t.Decryptor = n, t
    }(), e.mode.OFB
  })
}
