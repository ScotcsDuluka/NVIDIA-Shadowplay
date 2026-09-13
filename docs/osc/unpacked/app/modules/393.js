// ─────────────────────────────────────────────────────────────
// APP MODULE 393
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(24),
    o = n(117),
    r = n(16)("species");
  e.exports = function(e) {
    var t;
    return o(e) && (t = e.constructor, "function" != typeof t || t !== Array && !o(t.prototype) || (t = void 0), i(
      t) && (t = t[r], null === t && (t = void 0))), void 0 === t ? Array : t
  }
}
