// ─────────────────────────────────────────────────────────────
// APP MODULE 407
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(15),
    o = n(111),
    r = n(36),
    a = n(51);
  e.exports = function(e) {
    i(i.S, e, {
      from: function(e) {
        var t, n, i, l, s = arguments[1];
        return o(this), t = void 0 !== s, t && o(s), void 0 == e ? new this : (n = [], t ? (i = 0, l = r(s,
          arguments[2], 2), a(e, !1, function(e) {
          n.push(l(e, i++))
        })) : a(e, !1, n.push, n), new this(n))
      }
    })
  }
}
