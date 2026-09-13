// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 236
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(97))
  }(this, function(e) {
    return function() {
      var t = e,
        n = t.lib,
        r = n.WordArray,
        i = t.algo,
        o = i.SHA256,
        a = i.SHA224 = o.extend({
          _doReset: function() {
            this._hash = new r.init([3238371032, 914150663, 812702999, 4144912697, 4290775857, 1750603025,
              1694076839, 3204075428
            ])
          },
          _doFinalize: function() {
            var e = o._doFinalize.call(this);
            return e.sigBytes -= 4, e
          }
        });
      t.SHA224 = o._createHelper(a), t.HmacSHA224 = o._createHmacHelper(a)
    }(), e.SHA224
  })
}
