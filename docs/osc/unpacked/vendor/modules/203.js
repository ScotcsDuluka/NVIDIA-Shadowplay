// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 203
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(48),
    i = n(5)("iterator"),
    o = n(16);
  e.exports = n(2).isIterable = function(e) {
    var t = Object(e);
    return void 0 !== t[i] || "@@iterator" in t || o.hasOwnProperty(r(t))
  }
}
