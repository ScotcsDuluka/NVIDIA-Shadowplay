// ─────────────────────────────────────────────────────────────
// APP MODULE 396
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(113),
    o = n(390);
  e.exports = function(e) {
    return function() {
      if (i(this) != e) throw TypeError(e + "#toJSON isn't generic");
      return o(this)
    }
  }
}
