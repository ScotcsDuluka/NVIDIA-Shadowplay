// ─────────────────────────────────────────────────────────────
// APP MODULE 420
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(24),
    o = n(53).onFreeze;
  n(72)("freeze", function(e) {
    return function(t) {
      return e && i(t) ? e(o(t)) : t
    }
  })
}
