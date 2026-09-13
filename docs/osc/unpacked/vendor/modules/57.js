// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 57
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(12);
  e.exports = function(e, t) {
    if (!r(e)) return e;
    var n, i;
    if (t && "function" == typeof(n = e.toString) && !r(i = n.call(e))) return i;
    if ("function" == typeof(n = e.valueOf) && !r(i = n.call(e))) return i;
    if (!t && "function" == typeof(n = e.toString) && !r(i = n.call(e))) return i;
    throw TypeError("Can't convert object to primitive value")
  }
}
