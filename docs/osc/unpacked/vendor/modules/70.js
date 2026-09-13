// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 70
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(279);
  e.exports = function(e) {
    var t = e.xdomain,
      n = e.xscheme,
      i = e.enablesXDR;
    try {
      if ("undefined" != typeof XMLHttpRequest && (!t || r)) return new XMLHttpRequest
    } catch (e) {}
    try {
      if ("undefined" != typeof XDomainRequest && !n && i) return new XDomainRequest
    } catch (e) {}
    if (!t) try {
      return new ActiveXObject("Microsoft.XMLHTTP")
    } catch (e) {}
  }
}
