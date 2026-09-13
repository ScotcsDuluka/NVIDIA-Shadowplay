// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 43
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i) {
    e.exports = t = i(n(1))
  }(this, function(e) {
    return function(t) {
      var n = e,
        r = n.lib,
        i = r.Base,
        o = r.WordArray,
        a = n.x64 = {};
      a.Word = i.extend({
        init: function(e, t) {
          this.high = e, this.low = t
        }
      }), a.WordArray = i.extend({
        init: function(e, n) {
          e = this.words = e || [], n != t ? this.sigBytes = n : this.sigBytes = 8 * e.length
        },
        toX32: function() {
          for (var e = this.words, t = e.length, n = [], r = 0; r < t; r++) {
            var i = e[r];
            n.push(i.high), n.push(i.low)
          }
          return o.create(n, this.sigBytes)
        },
        clone: function() {
          for (var e = i.clone.call(this), t = e.words = this.words.slice(0), n = t.length, r = 0; r < n; r++)
            t[r] = t[r].clone();
          return e
        }
      })
    }(), e
  })
}
