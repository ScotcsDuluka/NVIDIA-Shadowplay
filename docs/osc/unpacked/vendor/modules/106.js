// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 106
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  function n(e, t, n) {
    return e.on(t, n), {
      destroy: function() {
        e.removeListener(t, n)
      }
    }
  }
  e.exports = n
}
