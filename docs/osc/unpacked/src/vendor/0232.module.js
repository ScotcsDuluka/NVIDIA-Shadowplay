// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 232
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(18), require(19), require(14), require(3));
  }(this, function(e) {
    return function() {
      function t() {
        for (var e = this._X, t = this._C, n = 0; n < 8; n++) s[n] = t[n];
        t[0] = t[0] + 1295307597 + this._b | 0, t[1] = t[1] + 3545052371 + (t[0] >>> 0 < s[0] >>> 0 ? 1 :
            0) | 0, t[2] = t[2] + 886263092 + (t[1] >>> 0 < s[1] >>> 0 ? 1 : 0) | 0, t[3] = t[3] +
          1295307597 + (t[2] >>> 0 < s[2] >>> 0 ? 1 : 0) | 0, t[4] = t[4] + 3545052371 + (t[3] >>> 0 < s[
            3] >>> 0 ? 1 : 0) | 0, t[5] = t[5] + 886263092 + (t[4] >>> 0 < s[4] >>> 0 ? 1 : 0) | 0, t[6] =
          t[6] + 1295307597 + (t[5] >>> 0 < s[5] >>> 0 ? 1 : 0) | 0, t[7] = t[7] + 3545052371 + (t[6] >>>
            0 < s[6] >>> 0 ? 1 : 0) | 0, this._b = t[7] >>> 0 < s[7] >>> 0 ? 1 : 0;
        for (var n = 0; n < 8; n++) {
          var r = e[n] + t[n],
            i = 65535 & r,
            o = r >>> 16,
            a = ((i * i >>> 17) + i * o >>> 15) + o * o,
            u = ((4294901760 & r) * r | 0) + ((65535 & r) * r | 0);
          c[n] = a ^ u;
        }
        e[0] = c[0] + (c[7] << 16 | c[7] >>> 16) + (c[6] << 16 | c[6] >>> 16) | 0, e[1] = c[1] + (c[0] <<
            8 | c[0] >>> 24) + c[7] | 0, e[2] = c[2] + (c[1] << 16 | c[1] >>> 16) + (c[0] << 16 | c[0] >>>
            16) | 0, e[3] = c[3] + (c[2] << 8 | c[2] >>> 24) + c[1] | 0, e[4] = c[4] + (c[3] << 16 | c[
            3] >>> 16) + (c[2] << 16 | c[2] >>> 16) | 0, e[5] = c[5] + (c[4] << 8 | c[4] >>> 24) + c[3] |
          0, e[6] = c[6] + (c[5] << 16 | c[5] >>> 16) + (c[4] << 16 | c[4] >>> 16) | 0, e[7] = c[7] + (c[
            6] << 8 | c[6] >>> 24) + c[5] | 0;
      }
      var n = e,
        r = n.lib,
        i = r.StreamCipher,
        o = n.algo,
        a = [],
        s = [],
        c = [],
        u = o.RabbitLegacy = i.extend({
          _doReset: function() {
            var e = this._key.words,
              n = this.cfg.iv,
              r = this._X = [e[0], e[3] << 16 | e[2] >>> 16, e[1], e[0] << 16 | e[3] >>> 16, e[2], e[
                1] << 16 | e[0] >>> 16, e[3], e[2] << 16 | e[1] >>> 16],
              i = this._C = [e[2] << 16 | e[2] >>> 16, 4294901760 & e[0] | 65535 & e[1], e[3] << 16 |
                e[3] >>> 16, 4294901760 & e[1] | 65535 & e[2], e[0] << 16 | e[0] >>> 16, 4294901760 &
                e[2] | 65535 & e[3], e[1] << 16 | e[1] >>> 16, 4294901760 & e[3] | 65535 & e[0]
              ];
            this._b = 0;
            for (var o = 0; o < 4; o++) t.call(this);
            for (var o = 0; o < 8; o++) i[o] ^= r[o + 4 & 7];
            if (n) {
              var a = n.words,
                s = a[0],
                c = a[1],
                u = 16711935 & (s << 8 | s >>> 24) | 4278255360 & (s << 24 | s >>> 8),
                l = 16711935 & (c << 8 | c >>> 24) | 4278255360 & (c << 24 | c >>> 8),
                d = u >>> 16 | 4294901760 & l,
                f = l << 16 | 65535 & u;
              i[0] ^= u, i[1] ^= d, i[2] ^= l, i[3] ^= f, i[4] ^= u, i[5] ^= d, i[6] ^= l, i[7] ^= f;
              for (var o = 0; o < 4; o++) t.call(this);
            }
          },
          _doProcessBlock: function(e, n) {
            var r = this._X;
            t.call(this), a[0] = r[0] ^ r[5] >>> 16 ^ r[3] << 16, a[1] = r[2] ^ r[7] >>> 16 ^ r[5] <<
              16, a[2] = r[4] ^ r[1] >>> 16 ^ r[7] << 16, a[3] = r[6] ^ r[3] >>> 16 ^ r[1] << 16;
            for (var i = 0; i < 4; i++) a[i] = 16711935 & (a[i] << 8 | a[i] >>> 24) | 4278255360 & (a[
              i] << 24 | a[i] >>> 8), e[n + i] ^= a[i];
          },
          blockSize: 4,
          ivSize: 2
        });
      n.RabbitLegacy = i._createHelper(u);
    }(), e.RabbitLegacy;
  });
}
