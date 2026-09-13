// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 195
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(10),
    i = n(39),
    o = n(54)("IE_PROTO"),
    a = Object.prototype;
  e.exports = Object.getPrototypeOf || function(e) {
    return e = i(e), r(e, o) ? e[o] : "function" == typeof e.constructor && e instanceof e.constructor ? e.constructor
      .prototype : e instanceof Object ? a : null
  }
}
