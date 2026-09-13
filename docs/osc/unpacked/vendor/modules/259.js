// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 259
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  (function(t) {
    "use strict";

    function n() {
      l = !0;
      for (var e, t, n = d.length; n;) {
        for (t = d, d = [], e = -1; ++e < n;) t[e]();
        n = d.length
      }
      l = !1
    }

    function r(e) {
      1 !== d.push(e) || l || i()
    }
    var i, o = t.MutationObserver || t.WebKitMutationObserver;
    if (o) {
      var a = 0,
        s = new o(n),
        c = t.document.createTextNode("");
      s.observe(c, {
        characterData: !0
      }), i = function() {
        c.data = a = ++a % 2
      }
    } else if (t.setImmediate || "undefined" == typeof t.MessageChannel) i = "document" in t &&
      "onreadystatechange" in t.document.createElement("script") ? function() {
        var e = t.document.createElement("script");
        e.onreadystatechange = function() {
          n(), e.onreadystatechange = null, e.parentNode.removeChild(e), e = null
        }, t.document.documentElement.appendChild(e)
      } : function() {
        setTimeout(n, 0)
      };
    else {
      var u = new t.MessageChannel;
      u.port1.onmessage = n, i = function() {
        u.port2.postMessage(0)
      }
    }
    var l, d = [];
    e.exports = r
  }).call(t, function() {
    return this
  }())
}
