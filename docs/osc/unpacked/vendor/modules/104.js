// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 104
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  var n = [].indexOf;
  e.exports = function(e, t) {
    if (n) return e.indexOf(t);
    for (var r = 0; r < e.length; ++r)
      if (e[r] === t) return r;
    return -1
  }
}
