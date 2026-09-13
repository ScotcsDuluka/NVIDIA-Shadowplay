// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 237
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(43))
  }(this, function(e) {
    return function(t) {
      var n = e,
        r = n.lib,
        i = r.WordArray,
        o = r.Hasher,
        a = n.x64,
        s = a.Word,
        c = n.algo,
        u = [],
        l = [],
        d = [];
      ! function() {
        for (var e = 1, t = 0, n = 0; n < 24; n++) {
          u[e + 5 * t] = (n + 1) * (n + 2) / 2 % 64;
          var r = t % 5,
            i = (2 * e + 3 * t) % 5;
          e = r, t = i
        }
        for (var e = 0; e < 5; e++)
          for (var t = 0; t < 5; t++) l[e + 5 * t] = t + (2 * e + 3 * t) % 5 * 5;
        for (var o = 1, a = 0; a < 24; a++) {
          for (var c = 0, f = 0, h = 0; h < 7; h++) {
            if (1 & o) {
              var p = (1 << h) - 1;
              p < 32 ? f ^= 1 << p : c ^= 1 << p - 32
            }
            128 & o ? o = o << 1 ^ 113 : o <<= 1
          }
          d[a] = s.create(c, f)
        }
      }();
      var f = [];
      ! function() {
        for (var e = 0; e < 25; e++) f[e] = s.create()
      }();
      var h = c.SHA3 = o.extend({
        cfg: o.cfg.extend({
          outputLength: 512
        }),
        _doReset: function() {
          for (var e = this._state = [], t = 0; t < 25; t++) e[t] = new s.init;
          this.blockSize = (1600 - 2 * this.cfg.outputLength) / 32
        },
        _doProcessBlock: function(e, t) {
          for (var n = this._state, r = this.blockSize / 2, i = 0; i < r; i++) {
            var o = e[t + 2 * i],
              a = e[t + 2 * i + 1];
            o = 16711935 & (o << 8 | o >>> 24) | 4278255360 & (o << 24 | o >>> 8), a = 16711935 & (a << 8 |
              a >>> 24) | 4278255360 & (a << 24 | a >>> 8);
            var s = n[i];
            s.high ^= a, s.low ^= o
          }
          for (var c = 0; c < 24; c++) {
            for (var h = 0; h < 5; h++) {
              for (var p = 0, m = 0, v = 0; v < 5; v++) {
                var s = n[h + 5 * v];
                p ^= s.high, m ^= s.low
              }
              var g = f[h];
              g.high = p, g.low = m
            }
            for (var h = 0; h < 5; h++)
              for (var y = f[(h + 4) % 5], b = f[(h + 1) % 5], E = b.high, _ = b.low, p = y.high ^ (E << 1 |
                  _ >>> 31), m = y.low ^ (_ << 1 | E >>> 31), v = 0; v < 5; v++) {
                var s = n[h + 5 * v];
                s.high ^= p, s.low ^= m
              }
            for (var $ = 1; $ < 25; $++) {
              var s = n[$],
                w = s.high,
                T = s.low,
                C = u[$];
              if (C < 32) var p = w << C | T >>> 32 - C,
                m = T << C | w >>> 32 - C;
              else var p = T << C - 32 | w >>> 64 - C,
                m = w << C - 32 | T >>> 64 - C;
              var x = f[l[$]];
              x.high = p, x.low = m
            }
            var S = f[0],
              A = n[0];
            S.high = A.high, S.low = A.low;
            for (var h = 0; h < 5; h++)
              for (var v = 0; v < 5; v++) {
                var $ = h + 5 * v,
                  s = n[$],
                  M = f[$],
                  k = f[(h + 1) % 5 + 5 * v],
                  N = f[(h + 2) % 5 + 5 * v];
                s.high = M.high ^ ~k.high & N.high, s.low = M.low ^ ~k.low & N.low
              }
            var s = n[0],
              I = d[c];
            s.high ^= I.high, s.low ^= I.low
          }
        },
        _doFinalize: function() {
          var e = this._data,
            n = e.words,
            r = (8 * this._nDataBytes, 8 * e.sigBytes),
            o = 32 * this.blockSize;
          n[r >>> 5] |= 1 << 24 - r % 32, n[(t.ceil((r + 1) / o) * o >>> 5) - 1] |= 128, e.sigBytes = 4 * n
            .length, this._process();
          for (var a = this._state, s = this.cfg.outputLength / 8, c = s / 8, u = [], l = 0; l < c; l++) {
            var d = a[l],
              f = d.high,
              h = d.low;
            f = 16711935 & (f << 8 | f >>> 24) | 4278255360 & (f << 24 | f >>> 8), h = 16711935 & (h << 8 |
              h >>> 24) | 4278255360 & (h << 24 | h >>> 8), u.push(h), u.push(f)
          }
          return new i.init(u, s)
        },
        clone: function() {
          for (var e = o.clone.call(this), t = e._state = this._state.slice(0), n = 0; n < 25; n++) t[n] = t[
            n].clone();
          return e
        }
      });
      n.SHA3 = o._createHelper(h), n.HmacSHA3 = o._createHmacHelper(h)
    }(Math), e.SHA3
  })
}
