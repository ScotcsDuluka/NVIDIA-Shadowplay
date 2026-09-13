// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 278
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports = Object.keys || function(e) {
    var t = [],
      n = Object.prototype.hasOwnProperty;
    for (var r in e) n.call(e, r) && t.push(r);
    return t
  }
}
