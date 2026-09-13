// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 234
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(18), n(19), n(14), n(3))
  }(this, function(e) {
    return function() {
      function t() {
        for (var e = this._S, t = this._i, n = this._j, r = 0, i = 0; i < 4; i++) {
          t = (t + 1) % 256, n = (n + e[t]) % 256;
          var o = e[t];
          e[t] = e[n], e[n] = o, r |= e[(e[t] + e[n]) % 256] << 24 - 8 * i
        }
        return this._i = t, this._j = n, r
      }
      var n = e,
        r = n.lib,
        i = r.StreamCipher,
        o = n.algo,
        a = o.RC4 = i.extend({
          _doReset: function() {
            for (var e = this._key, t = e.words, n = e.sigBytes, r = this._S = [], i = 0; i < 256; i++) r[i] =
            i;
            for (var i = 0, o = 0; i < 256; i++) {
              var a = i % n,
                s = t[a >>> 2] >>> 24 - a % 4 * 8 & 255;
              o = (o + r[i] + s) % 256;
              var c = r[i];
              r[i] = r[o], r[o] = c
            }
            this._i = this._j = 0
          },
          _doProcessBlock: function(e, n) {
            e[n] ^= t.call(this)
          },
          keySize: 8,
          ivSize: 0
        });
      n.RC4 = i._createHelper(a);
      var s = o.RC4Drop = a.extend({
        cfg: a.cfg.extend({
          drop: 192
        }),
        _doReset: function() {
          a._doReset.call(this);
          for (var e = this.cfg.drop; e > 0; e--) t.call(this)
        }
      });
      n.RC4Drop = i._createHelper(s)
    }(), e.RC4
  })
}
