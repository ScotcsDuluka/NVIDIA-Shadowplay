// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 89
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports = function(e) {
    try {
      return {
        e: !1,
        v: e()
      }
    } catch (e) {
      return {
        e: !0,
        v: e
      }
    }
  }
}
