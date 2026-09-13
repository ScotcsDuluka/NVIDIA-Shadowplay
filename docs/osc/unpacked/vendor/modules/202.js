// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 202
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(7),
    i = n(95);
  e.exports = n(2).getIterator = function(e) {
    var t = i(e);
    if ("function" != typeof t) throw TypeError(e + " is not iterable!");
    return r(t.call(e))
  }
}
