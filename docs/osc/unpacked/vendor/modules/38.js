// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 38
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(9).f,
    i = n(10),
    o = n(5)("toStringTag");
  e.exports = function(e, t, n) {
    e && !i(e = n ? e : e.prototype, o) && r(e, o, {
      configurable: !0,
      value: t
    })
  }
}
