// ─────────────────────────────────────────────────────────────
// APP MODULE 64
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function i(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  t.__esModule = !0;
  var o = n(371),
    r = i(o);
  t.default = function(e, t, n) {
    return t in e ? (0, r.default)(e, t, {
      value: n,
      enumerable: !0,
      configurable: !0,
      writable: !0
    }) : e[t] = n, e
  }
}
