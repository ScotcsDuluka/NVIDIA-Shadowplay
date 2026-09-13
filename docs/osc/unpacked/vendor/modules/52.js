// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 52
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function r(e) {
    var t, n;
    this.promise = new e(function(e, r) {
      if (void 0 !== t || void 0 !== n) throw TypeError("Bad Promise constructor");
      t = e, n = r
    }), this.resolve = i(t), this.reject = i(n)
  }
  var i = n(35);
  e.exports.f = function(e) {
    return new r(e)
  }
}
