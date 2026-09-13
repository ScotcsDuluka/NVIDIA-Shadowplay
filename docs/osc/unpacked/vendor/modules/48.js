// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 48
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(27),
    i = n(5)("toStringTag"),
    o = "Arguments" == r(function() {
      return arguments
    }()),
    a = function(e, t) {
      try {
        return e[t]
      } catch (e) {}
    };
  e.exports = function(e) {
    var t, n, s;
    return void 0 === e ? "Undefined" : null === e ? "Null" : "string" == typeof(n = a(t = Object(e), i)) ? n : o ? r(
      t) : "Object" == (s = r(t)) && "function" == typeof t.callee ? "Arguments" : s
  }
}
