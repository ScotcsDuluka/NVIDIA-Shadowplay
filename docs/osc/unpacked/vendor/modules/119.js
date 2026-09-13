// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 119
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";

  function n() {
    try {
      return "undefined" != typeof localStorage && "setItem" in localStorage && localStorage.setItem
    } catch (e) {
      return !1
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.default = n
}
