// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 169
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(2),
    i = r.JSON || (r.JSON = {
      stringify: JSON.stringify
    });
  e.exports = function(e) {
    return i.stringify.apply(i, arguments)
  }
}
