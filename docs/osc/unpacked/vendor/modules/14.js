// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 14
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(61), n(60))
  }(this, function(e) {
    return function() {
      var t = e,
        n = t.lib,
        r = n.Base,
        i = n.WordArray,
        o = t.algo,
        a = o.MD5,
        s = o.EvpKDF = r.extend({
          cfg: r.extend({
            keySize: 4,
            hasher: a,
            iterations: 1
          }),
          init: function(e) {
            this.cfg = this.cfg.extend(e)
          },
          compute: function(e, t) {
            for (var n = this.cfg, r = n.hasher.create(), o = i.create(), a = o.words, s = n.keySize, c = n
                .iterations; a.length < s;) {
              u && r.update(u);
              var u = r.update(e).finalize(t);
              r.reset();
              for (var l = 1; l < c; l++) u = r.finalize(u), r.reset();
              o.concat(u)
            }
            return o.sigBytes = 4 * s, o
          }
        });
      t.EvpKDF = function(e, t, n) {
        return s.create(n).compute(e, t)
      }
    }(), e.EvpKDF
  })
}
