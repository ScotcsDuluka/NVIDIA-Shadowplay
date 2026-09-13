// ─────────────────────────────────────────────────────────────
// APP MODULE 411
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(75),
    o = Math.max,
    r = Math.min;
  e.exports = function(e, t) {
    return e = i(e), e < 0 ? o(e + t, 0) : r(e, t)
  }
}
