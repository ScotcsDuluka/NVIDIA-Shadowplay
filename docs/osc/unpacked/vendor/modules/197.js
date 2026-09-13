// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 197
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(11);
  e.exports = function(e, t, n) {
    for (var i in t) n && e[i] ? e[i] = t[i] : r(e, i, t[i]);
    return e
  }
}
