// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 223
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(3));
  }(this, function(e) {
    return e.mode.CTR = function() {
      var t = e.lib.BlockCipherMode.extend(),
        n = t.Encryptor = t.extend({
          processBlock: function(e, t) {
            var n = this._cipher,
              r = n.blockSize,
              i = this._iv,
              o = this._counter;
            i && (o = this._counter = i.slice(0), this._iv = void 0);
            var a = o.slice(0);
            n.encryptBlock(a, 0), o[r - 1] = o[r - 1] + 1 | 0;
            for (var s = 0; s < r; s++) e[t + s] ^= a[s];
          }
        });
      return t.Decryptor = n, t;
    }(), e.mode.CTR;
  });
}
