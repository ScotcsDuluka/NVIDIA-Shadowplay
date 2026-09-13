// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 285
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  function n(e, t, n) {
    var r;
    return r = t ? new i(e, t) : new i(e)
  }
  var r = function() {
      return this
    }(),
    i = r.WebSocket || r.MozWebSocket;
  e.exports = i ? n : null, i && (n.prototype = i.prototype)
}
