// ─────────────────────────────────────────────────────────────
// APP MODULE 402
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(16)("iterator"),
    o = !1;
  try {
    var r = [7][i]();
    r.return = function() {
      o = !0
    }, Array.from(r, function() {
      throw 2
    })
  } catch (e) {}
  e.exports = function(e, t) {
    if (!t && !o) return !1;
    var n = !1;
    try {
      var r = [7],
        a = r[i]();
      a.next = function() {
        return {
          done: n = !0
        }
      }, r[i] = function() {
        return a
      }, e(r)
    } catch (e) {}
    return n
  }
}
