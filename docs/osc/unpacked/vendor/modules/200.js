// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 200
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(56),
    i = Math.max,
    o = Math.min;
  e.exports = function(e, t) {
    return e = r(e), e < 0 ? i(e + t, 0) : o(e, t)
  }
}
