// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 61
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i) {
    e.exports = t = i(n(1))
  }(this, function(e) {
    return function() {
      var t = e,
        n = t.lib,
        r = n.WordArray,
        i = n.Hasher,
        o = t.algo,
        a = [],
        s = o.SHA1 = i.extend({
          _doReset: function() {
            this._hash = new r.init([1732584193, 4023233417, 2562383102, 271733878, 3285377520])
          },
          _doProcessBlock: function(e, t) {
            for (var n = this._hash.words, r = n[0], i = n[1], o = n[2], s = n[3], c = n[4], u = 0; u <
              80; u++) {
              if (u < 16) a[u] = 0 | e[t + u];
              else {
                var l = a[u - 3] ^ a[u - 8] ^ a[u - 14] ^ a[u - 16];
                a[u] = l << 1 | l >>> 31
              }
              var d = (r << 5 | r >>> 27) + c + a[u];
              d += u < 20 ? (i & o | ~i & s) + 1518500249 : u < 40 ? (i ^ o ^ s) + 1859775393 : u < 60 ? (i &
                  o | i & s | o & s) - 1894007588 : (i ^ o ^ s) - 899497514, c = s, s = o, o = i << 30 | i >>>
                2, i = r, r = d
            }
            n[0] = n[0] + r | 0, n[1] = n[1] + i | 0, n[2] = n[2] + o | 0, n[3] = n[3] + s | 0, n[4] = n[4] +
              c | 0
          },
          _doFinalize: function() {
            var e = this._data,
              t = e.words,
              n = 8 * this._nDataBytes,
              r = 8 * e.sigBytes;
            return t[r >>> 5] |= 128 << 24 - r % 32, t[(r + 64 >>> 9 << 4) + 14] = Math.floor(n / 4294967296),
              t[(r + 64 >>> 9 << 4) + 15] = n, e.sigBytes = 4 * t.length, this._process(), this._hash
          },
          clone: function() {
            var e = i.clone.call(this);
            return e._hash = this._hash.clone(), e
          }
        });
      t.SHA1 = i._createHelper(s), t.HmacSHA1 = i._createHmacHelper(s)
    }(), e.SHA1
  })
}
