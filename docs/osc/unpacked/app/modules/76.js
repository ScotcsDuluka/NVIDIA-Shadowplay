// ─────────────────────────────────────────────────────────────
// APP MODULE 76
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(24);
  e.exports = function(e, t) {
    if (!i(e)) return e;
    var n, o;
    if (t && "function" == typeof(n = e.toString) && !i(o = n.call(e))) return o;
    if ("function" == typeof(n = e.valueOf) && !i(o = n.call(e))) return o;
    if (!t && "function" == typeof(n = e.toString) && !i(o = n.call(e))) return o;
    throw TypeError("Can't convert object to primitive value")
  }
}
