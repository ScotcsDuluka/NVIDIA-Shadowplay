// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 118
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }

  function i() {
    try {
      return !!a.default && (!("undefined" != typeof openDatabase && "undefined" != typeof navigator && navigator
        .userAgent && /Safari/.test(navigator.userAgent) && !/Chrome/.test(navigator.userAgent)) && (a.default &&
        "function" == typeof a.default.open && "undefined" != typeof IDBKeyRange))
    } catch (e) {
      return !1
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  });
  var o = n(74),
    a = r(o);
  t.default = i
}
