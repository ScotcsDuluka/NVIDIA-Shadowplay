// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 231
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(r, i, o) {
    module.exports = exports = i(require(1), require(61), require(60));
  }(this, function(e) {
    return function() {
      var t = e,
        n = t.lib,
        r = n.Base,
        i = n.WordArray,
        o = t.algo,
        a = o.SHA1,
        s = o.HMAC,
        c = o.PBKDF2 = r.extend({
          cfg: r.extend({
            keySize: 4,
            hasher: a,
            iterations: 1
          }),
          init: function(e) {
            this.cfg = this.cfg.extend(e);
          },
          compute: function(e, t) {
            for (var n = this.cfg, r = s.create(n.hasher, e), o = i.create(), a = i.create([1]), c = o
                .words, u = a.words, l = n.keySize, d = n.iterations; c.length < l;) {
              var f = r.update(t).finalize(a);
              r.reset();
              for (var h = f.words, p = h.length, m = f, v = 1; v < d; v++) {
                m = r.finalize(m), r.reset();
                for (var g = m.words, y = 0; y < p; y++) h[y] ^= g[y];
              }
              o.concat(f), u[0]++;
            }
            return o.sigBytes = 4 * l, o;
          }
        });
      t.PBKDF2 = function(e, t, n) {
        return c.create(n).compute(e, t);
      };
    }(), e.PBKDF2;
  });
}
