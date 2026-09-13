// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 188
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(5)("iterator"),
    i = !1;
  try {
    var o = [7][r]();
    o.return = function() {
      i = !0
    }, Array.from(o, function() {
      throw 2
    })
  } catch (e) {}
  e.exports = function(e, t) {
    if (!t && !i) return !1;
    var n = !1;
    try {
      var o = [7],
        a = o[r]();
      a.next = function() {
        return {
          done: n = !0
        }
      }, o[r] = function() {
        return a
      }, e(o)
    } catch (e) {}
    return n
  }
}
