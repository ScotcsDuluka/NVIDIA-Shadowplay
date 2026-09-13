// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 79
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  var n = [].slice;
  e.exports = function(e, t) {
    if ("string" == typeof t && (t = e[t]), "function" != typeof t) throw new Error("bind() requires a function");
    var r = n.call(arguments, 2);
    return function() {
      return t.apply(e, r.concat(n.call(arguments)))
    }
  }
}
