// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 218
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i, o) {
    e.exports = t = i(n(1), n(3))
  }(this, function(e) {
    return function(t) {
      var n = e,
        r = n.lib,
        i = r.CipherParams,
        o = n.enc,
        a = o.Hex,
        s = n.format;
      s.Hex = {
        stringify: function(e) {
          return e.ciphertext.toString(a)
        },
        parse: function(e) {
          var t = a.parse(e);
          return i.create({
            ciphertext: t
          })
        }
      }
    }(), e.format.Hex
  })
}
