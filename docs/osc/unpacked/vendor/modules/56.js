// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 56
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  var n = Math.ceil,
    r = Math.floor;
  e.exports = function(e) {
    return isNaN(e = +e) ? 0 : (e > 0 ? r : n)(e)
  }
}
