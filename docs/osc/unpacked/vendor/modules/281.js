// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 281
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  (function(t) {
    var n = /^[\],:{}\s]*$/,
      r = /\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g,
      i = /"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g,
      o = /(?:^|:|,)(?:\s*\[)+/g,
      a = /^\s+/,
      s = /\s+$/;
    e.exports = function(e) {
      return "string" == typeof e && e ? (e = e.replace(a, "").replace(s, ""), t.JSON && JSON.parse ? JSON.parse(
          e) : n.test(e.replace(r, "@").replace(i, "]").replace(o, "")) ? new Function("return " + e)() : void 0) :
        null
    }
  }).call(t, function() {
    return this
  }())
}
