// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 221
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(3))
  }(this, function(e) {
    return e.mode.CFB = function() {
      function t(e, t, n, r) {
        var i = this._iv;
        if (i) {
          var o = i.slice(0);
          this._iv = void 0
        } else var o = this._prevBlock;
        r.encryptBlock(o, 0);
        for (var a = 0; a < n; a++) e[t + a] ^= o[a]
      }
      var n = e.lib.BlockCipherMode.extend();
      return n.Encryptor = n.extend({
        processBlock: function(e, n) {
          var r = this._cipher,
            i = r.blockSize;
          t.call(this, e, n, i, r), this._prevBlock = e.slice(n, n + i)
        }
      }), n.Decryptor = n.extend({
        processBlock: function(e, n) {
          var r = this._cipher,
            i = r.blockSize,
            o = e.slice(n, n + i);
          t.call(this, e, n, i, r), this._prevBlock = o
        }
      }), n
    }(), e.mode.CFB
  })
}
