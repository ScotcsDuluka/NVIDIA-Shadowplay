// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 279
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(257);
  try {
    e.exports = "XMLHttpRequest" in r && "withCredentials" in new r.XMLHttpRequest
  } catch (t) {
    e.exports = !1
  }
}
