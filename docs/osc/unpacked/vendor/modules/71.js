// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 71
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  t.encode = function(e) {
    var t = "";
    for (var n in e) e.hasOwnProperty(n) && (t.length && (t += "&"), t += encodeURIComponent(n) + "=" +
      encodeURIComponent(e[n]));
    return t
  }, t.decode = function(e) {
    for (var t = {}, n = e.split("&"), r = 0, i = n.length; r < i; r++) {
      var o = n[r].split("=");
      t[decodeURIComponent(o[0])] = decodeURIComponent(o[1])
    }
    return t
  }
}
