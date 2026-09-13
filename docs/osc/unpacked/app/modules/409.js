// ─────────────────────────────────────────────────────────────
// APP MODULE 409
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(21),
    o = n(13),
    r = n(19),
    a = n(18),
    l = n(16)("species");
  e.exports = function(e) {
    var t = "function" == typeof o[e] ? o[e] : i[e];
    a && t && !t[l] && r.f(t, l, {
      configurable: !0,
      get: function() {
        return this
      }
    })
  }
}
