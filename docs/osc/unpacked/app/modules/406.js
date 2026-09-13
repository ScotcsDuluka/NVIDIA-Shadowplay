// ─────────────────────────────────────────────────────────────
// APP MODULE 406
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(37),
    o = n(120).f,
    r = {}.toString,
    a = "object" == typeof window && window && Object.getOwnPropertyNames ? Object.getOwnPropertyNames(window) : [],
    l = function(e) {
      try {
        return o(e)
      } catch (e) {
        return a.slice()
      }
    };
  e.exports.f = function(e) {
    return a && "[object Window]" == r.call(e) ? l(e) : o(i(e))
  }
}
