// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 186
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(7);
  e.exports = function(e, t, n, i) {
    try {
      return i ? t(r(n)[0], n[1]) : t(n)
    } catch (t) {
      var o = e.return;
      throw void 0 !== o && r(o.call(e)), t
    }
  }
}
