// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 216
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(18), n(19), n(14), n(3))
  }(this, function(e) {
    return function() {
      var t = e,
        n = t.lib,
        r = n.BlockCipher,
        i = t.algo,
        o = [],
        a = [],
        s = [],
        c = [],
        u = [],
        l = [],
        d = [],
        f = [],
        h = [],
        p = [];
      ! function() {
        for (var e = [], t = 0; t < 256; t++) t < 128 ? e[t] = t << 1 : e[t] = t << 1 ^ 283;
        for (var n = 0, r = 0, t = 0; t < 256; t++) {
          var i = r ^ r << 1 ^ r << 2 ^ r << 3 ^ r << 4;
          i = i >>> 8 ^ 255 & i ^ 99, o[n] = i, a[i] = n;
          var m = e[n],
            v = e[m],
            g = e[v],
            y = 257 * e[i] ^ 16843008 * i;
          s[n] = y << 24 | y >>> 8, c[n] = y << 16 | y >>> 16, u[n] = y << 8 | y >>> 24, l[n] = y;
          var y = 16843009 * g ^ 65537 * v ^ 257 * m ^ 16843008 * n;
          d[i] = y << 24 | y >>> 8, f[i] = y << 16 | y >>> 16, h[i] = y << 8 | y >>> 24, p[i] = y, n ? (n = m ^ e[e[
            e[g ^ m]]], r ^= e[e[r]]) : n = r = 1
        }
      }();
      var m = [0, 1, 2, 4, 8, 16, 32, 64, 128, 27, 54],
        v = i.AES = r.extend({
          _doReset: function() {
            if (!this._nRounds || this._keyPriorReset !== this._key) {
              for (var e = this._keyPriorReset = this._key, t = e.words, n = e.sigBytes / 4, r = this._nRounds =
                  n + 6, i = 4 * (r + 1), a = this._keySchedule = [], s = 0; s < i; s++)
                if (s < n) a[s] = t[s];
                else {
                  var c = a[s - 1];
                  s % n ? n > 6 && s % n == 4 && (c = o[c >>> 24] << 24 | o[c >>> 16 & 255] << 16 | o[c >>> 8 &
                      255] << 8 | o[255 & c]) : (c = c << 8 | c >>> 24, c = o[c >>> 24] << 24 | o[c >>> 16 &
                      255] << 16 | o[c >>> 8 & 255] << 8 | o[255 & c], c ^= m[s / n | 0] << 24), a[s] = a[s -
                    n] ^ c
                } for (var u = this._invKeySchedule = [], l = 0; l < i; l++) {
                var s = i - l;
                if (l % 4) var c = a[s];
                else var c = a[s - 4];
                l < 4 || s <= 4 ? u[l] = c : u[l] = d[o[c >>> 24]] ^ f[o[c >>> 16 & 255]] ^ h[o[c >>> 8 &
                  255]] ^ p[o[255 & c]]
              }
            }
          },
          encryptBlock: function(e, t) {
            this._doCryptBlock(e, t, this._keySchedule, s, c, u, l, o)
          },
          decryptBlock: function(e, t) {
            var n = e[t + 1];
            e[t + 1] = e[t + 3], e[t + 3] = n, this._doCryptBlock(e, t, this._invKeySchedule, d, f, h, p, a);
            var n = e[t + 1];
            e[t + 1] = e[t + 3], e[t + 3] = n
          },
          _doCryptBlock: function(e, t, n, r, i, o, a, s) {
            for (var c = this._nRounds, u = e[t] ^ n[0], l = e[t + 1] ^ n[1], d = e[t + 2] ^ n[2], f = e[t +
                3] ^ n[3], h = 4, p = 1; p < c; p++) {
              var m = r[u >>> 24] ^ i[l >>> 16 & 255] ^ o[d >>> 8 & 255] ^ a[255 & f] ^ n[h++],
                v = r[l >>> 24] ^ i[d >>> 16 & 255] ^ o[f >>> 8 & 255] ^ a[255 & u] ^ n[h++],
                g = r[d >>> 24] ^ i[f >>> 16 & 255] ^ o[u >>> 8 & 255] ^ a[255 & l] ^ n[h++],
                y = r[f >>> 24] ^ i[u >>> 16 & 255] ^ o[l >>> 8 & 255] ^ a[255 & d] ^ n[h++];
              u = m, l = v, d = g, f = y
            }
            var m = (s[u >>> 24] << 24 | s[l >>> 16 & 255] << 16 | s[d >>> 8 & 255] << 8 | s[255 & f]) ^ n[h++],
              v = (s[l >>> 24] << 24 | s[d >>> 16 & 255] << 16 | s[f >>> 8 & 255] << 8 | s[255 & u]) ^ n[h++],
              g = (s[d >>> 24] << 24 | s[f >>> 16 & 255] << 16 | s[u >>> 8 & 255] << 8 | s[255 & l]) ^ n[h++],
              y = (s[f >>> 24] << 24 | s[u >>> 16 & 255] << 16 | s[l >>> 8 & 255] << 8 | s[255 & d]) ^ n[h++];
            e[t] = m, e[t + 1] = v, e[t + 2] = g, e[t + 3] = y
          },
          keySize: 8
        });
      t.AES = r._createHelper(v)
    }(), e.AES
  })
}
