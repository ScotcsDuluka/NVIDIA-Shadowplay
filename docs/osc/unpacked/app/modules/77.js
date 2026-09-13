// ─────────────────────────────────────────────────────────────
// APP MODULE 77
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(21),
    o = n(13),
    r = n(52),
    a = n(78),
    l = n(19).f;
  e.exports = function(e) {
    var t = o.Symbol || (o.Symbol = r ? {} : i.Symbol || {});
    "_" == e.charAt(0) || e in t || l(t, e, {
      value: a.f(e)
    })
  }
}
