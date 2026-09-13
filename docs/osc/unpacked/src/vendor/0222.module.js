// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 222
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(3));
  }(this, function(e) {
    /** @preserve
     * Counter block mode compatible with  Dr Brian Gladman fileenc.c
     * derived from CryptoJS.mode.CTR
     * Jan Hruby jhruby.web@gmail.com
     */
    return e.mode.CTRGladman = function() {
      function t(e) {
        if (255 === (e >> 24 & 255)) {
          var t = e >> 16 & 255,
            n = e >> 8 & 255,
            r = 255 & e;
          255 === t ? (t = 0, 255 === n ? (n = 0, 255 === r ? r = 0 : ++r) : ++n) : ++t, e = 0, e += t <<
            16, e += n << 8, e += r;
        } else e += 1 << 24;
        return e;
      }

      function n(e) {
        return 0 === (e[0] = t(e[0])) && (e[1] = t(e[1])), e;
      }
      var r = e.lib.BlockCipherMode.extend(),
        i = r.Encryptor = r.extend({
          processBlock: function(e, t) {
            var r = this._cipher,
              i = r.blockSize,
              o = this._iv,
              a = this._counter;
            o && (a = this._counter = o.slice(0), this._iv = void 0), n(a);
            var s = a.slice(0);
            r.encryptBlock(s, 0);
            for (var c = 0; c < i; c++) e[t + c] ^= s[c];
          }
        });
      return r.Decryptor = i, r;
    }(), e.mode.CTRGladman;
  });
}
