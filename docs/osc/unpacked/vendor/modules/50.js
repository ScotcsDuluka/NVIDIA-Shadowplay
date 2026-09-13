// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 50
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(12),
    i = n(4).document,
    o = r(i) && r(i.createElement);
  e.exports = function(e) {
    return o ? i.createElement(e) : {}
  }
}
