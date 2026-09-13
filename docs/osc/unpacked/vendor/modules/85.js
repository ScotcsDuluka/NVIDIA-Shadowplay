// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 85
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(13),
    i = n(86).f,
    o = {}.toString,
    a = "object" == typeof window && window && Object.getOwnPropertyNames ? Object.getOwnPropertyNames(window) : [],
    s = function(e) {
      try {
        return i(e)
      } catch (e) {
        return a.slice()
      }
    };
  e.exports.f = function(e) {
    return a && "[object Window]" == o.call(e) ? s(e) : i(r(e))
  }
}
