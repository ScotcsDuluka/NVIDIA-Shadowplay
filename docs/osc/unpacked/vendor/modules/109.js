// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 109
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  (function(e) {
    function r(t) {
      var n, r = !1,
        s = !1,
        c = !1 !== t.jsonp;
      if (e.location) {
        var u = "https:" == location.protocol,
          l = location.port;
        l || (l = u ? 443 : 80), r = t.hostname != location.hostname || l != t.port, s = t.secure != u
      }
      if (t.xdomain = r, t.xscheme = s, n = new i(t), "open" in n && !t.forceJSONP) return new o(t);
      if (!c) throw new Error("JSONP disabled");
      return new a(t)
    }
    var i = n(70),
      o = n(274),
      a = n(273),
      s = n(275);
    t.polling = r, t.websocket = s
  }).call(t, function() {
    return this
  }())
}
