// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 225
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(3));
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
            for (var a = 0; a < r; a++) e[t + a] ^= o[a];
          }
        });
      return t.Decryptor = n, t;
    }(), e.mode.OFB;
  });
}
