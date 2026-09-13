// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 212
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var r = n(8),
    i = n(2),
    o = n(4),
    a = n(92),
    s = n(90);
  r(r.P + r.R, "Promise", {
    finally: function(e) {
      var t = a(this, i.Promise || o.Promise),
        n = "function" == typeof e;
      return this.then(n ? function(n) {
        return s(t, e()).then(function() {
          return n
        })
      } : e, n ? function(n) {
        return s(t, e()).then(function() {
          throw n
        })
      } : e)
    }
  })
}
