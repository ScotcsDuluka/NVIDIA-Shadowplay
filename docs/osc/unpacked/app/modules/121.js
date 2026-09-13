// ─────────────────────────────────────────────────────────────
// APP MODULE 121
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(30),
    o = n(31),
    r = n(73)("IE_PROTO"),
    a = Object.prototype;
  e.exports = Object.getPrototypeOf || function(e) {
    return e = o(e), i(e, r) ? e[r] : "function" == typeof e.constructor && e instanceof e.constructor ? e.constructor
      .prototype : e instanceof Object ? a : null
  }
}
