// ─────────────────────────────────────────────────────────────
// APP MODULE 412
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(28),
    o = n(79);
  e.exports = n(13).getIterator = function(e) {
    var t = o(e);
    if ("function" != typeof t) throw TypeError(e + " is not iterable!");
    return i(t.call(e))
  }
}
