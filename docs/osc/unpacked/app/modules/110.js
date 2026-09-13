// ─────────────────────────────────────────────────────────────
// APP MODULE 110
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
  var o = n(374),
    r = i(o),
    a = n(373),
    l = i(a),
    s = "function" == typeof l.default && "symbol" == typeof r.default ? function(e) {
      return typeof e
    } : function(e) {
      return e && "function" == typeof l.default && e.constructor === l.default && e !== l.default.prototype ?
        "symbol" : typeof e
    };
  t.default = "function" == typeof l.default && "symbol" === s(r.default) ? function(e) {
    return "undefined" == typeof e ? "undefined" : s(e)
  } : function(e) {
    return e && "function" == typeof l.default && e.constructor === l.default && e !== l.default.prototype ?
      "symbol" : "undefined" == typeof e ? "undefined" : s(e)
  }
}
