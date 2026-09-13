// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 198
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var r = n(4),
    i = n(2),
    o = n(9),
    a = n(6),
    s = n(5)("species");
  e.exports = function(e) {
    var t = "function" == typeof i[e] ? i[e] : r[e];
    a && t && !t[s] && o.f(t, s, {
      configurable: !0,
      get: function() {
        return this
      }
    })
  }
}
