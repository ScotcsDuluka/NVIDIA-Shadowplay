// ─────────────────────────────────────────────────────────────
// APP MODULE 36
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(111);
  e.exports = function(e, t, n) {
    if (i(e), void 0 === t) return e;
    switch (n) {
      case 1:
        return function(n) {
          return e.call(t, n)
        };
      case 2:
        return function(n, i) {
          return e.call(t, n, i)
        };
      case 3:
        return function(n, i, o) {
          return e.call(t, n, i, o)
        }
    }
    return function() {
      return e.apply(t, arguments)
    }
  }
}
