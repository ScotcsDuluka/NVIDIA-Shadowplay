// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 26
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
  t.__esModule = !0;
  var i = n(158),
    o = r(i),
    a = n(157),
    s = r(a),
    c = "function" == typeof s.default && "symbol" == typeof o.default ? function(e) {
      return typeof e
    } : function(e) {
      return e && "function" == typeof s.default && e.constructor === s.default && e !== s.default.prototype ?
        "symbol" : typeof e
    };
  t.default = "function" == typeof s.default && "symbol" === c(o.default) ? function(e) {
    return "undefined" == typeof e ? "undefined" : c(e)
  } : function(e) {
    return e && "function" == typeof s.default && e.constructor === s.default && e !== s.default.prototype ?
      "symbol" : "undefined" == typeof e ? "undefined" : c(e)
  }
}
