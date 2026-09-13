// ─────────────────────────────────────────────────────────────
// APP MODULE 118
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(28);
  e.exports = function(e, t, n, o) {
    try {
      return o ? t(i(n)[0], n[1]) : t(n)
    } catch (t) {
      var r = e.return;
      throw void 0 !== r && i(r.call(e)), t
    }
  }
}
