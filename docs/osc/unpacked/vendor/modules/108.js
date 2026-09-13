// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 108
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  (function(t) {
    function r(e, n) {
      var r = e,
        n = n || t.location;
      return null == e && (e = n.protocol + "//" + n.host), "string" == typeof e && ("/" == e.charAt(0) && (e = "/" ==
          e.charAt(1) ? n.protocol + e : n.hostname + e), /^(https?|wss?):\/\//.test(e) || (o(
          "protocol-less url %s", e), e = "undefined" != typeof n ? n.protocol + "//" + e : "https://" + e), o(
          "parse %s", e), r = i(e)), r.port || (/^(http|ws)$/.test(r.protocol) ? r.port = "80" : /^(http|ws)s$/.test(r
          .protocol) && (r.port = "443")), r.path = r.path || "/", r.id = r.protocol + "://" + r.host + ":" + r.port,
        r.href = r.protocol + "://" + r.host + (n && n.port == r.port ? "" : ":" + r.port), r
    }
    var i = n(282),
      o = n(32)("socket.io-client:url");
    e.exports = r
  }).call(t, function() {
    return this
  }())
}
