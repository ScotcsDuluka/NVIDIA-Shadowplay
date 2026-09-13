// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 204
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var r = n(178),
    i = n(189),
    o = n(16),
    a = n(13);
  e.exports = n(83)(Array, "Array", function(e, t) {
    this._t = a(e), this._i = 0, this._k = t
  }, function() {
    var e = this._t,
      t = this._k,
      n = this._i++;
    return !e || n >= e.length ? (this._t = void 0, i(1)) : "keys" == t ? i(0, n) : "values" == t ? i(0, e[n]) : i(
      0, [n, e[n]])
  }, "values"), o.Arguments = o.Array, r("keys"), r("values"), r("entries")
}
