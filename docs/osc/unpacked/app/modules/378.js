// ─────────────────────────────────────────────────────────────
// APP MODULE 378
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(13),
    o = i.JSON || (i.JSON = {
      stringify: JSON.stringify
    });
  e.exports = function(e) {
    return o.stringify.apply(o, arguments)
  }
}
