// ─────────────────────────────────────────────────────────────
// APP MODULE 113
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(65),
    o = n(16)("toStringTag"),
    r = "Arguments" == i(function() {
      return arguments
    }()),
    a = function(e, t) {
      try {
        return e[t]
      } catch (e) {}
    };
  e.exports = function(e) {
    var t, n, l;
    return void 0 === e ? "Undefined" : null === e ? "Null" : "string" == typeof(n = a(t = Object(e), o)) ? n : r ? i(
      t) : "Object" == (l = i(t)) && "function" == typeof t.callee ? "Arguments" : l
  }
}
