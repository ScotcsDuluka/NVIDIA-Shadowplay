// ─────────────────────────────────────────────────────────────
// APP MODULE 75
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  var n = Math.ceil,
    i = Math.floor;
  e.exports = function(e) {
    return isNaN(e = +e) ? 0 : (e > 0 ? i : n)(e)
  }
}
