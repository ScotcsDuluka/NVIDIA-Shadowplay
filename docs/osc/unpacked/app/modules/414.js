// ─────────────────────────────────────────────────────────────
// APP MODULE 414
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(389),
    o = n(119),
    r = n(41),
    a = n(37);
  e.exports = n(69)(Array, "Array", function(e, t) {
    this._t = a(e), this._i = 0, this._k = t
  }, function() {
    var e = this._t,
      t = this._k,
      n = this._i++;
    return !e || n >= e.length ? (this._t = void 0, o(1)) : "keys" == t ? o(0, n) : "values" == t ? o(0, e[n]) : o(
      0, [n, e[n]])
  }, "values"), r.Arguments = r.Array, i("keys"), i("values"), i("entries")
}
