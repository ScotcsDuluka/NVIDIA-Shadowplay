// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 259
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  (function(t) {
    "use strict";

    function n() {
      l = !0;
      for (var e, t, n = d.length; n;) {
        for (t = d, d = [], e = -1; ++e < n;) t[e]();
        n = d.length;
      }
      l = !1;
    }

    function r(e) {
      1 !== d.push(e) || l || i();
    }
    var i,
      o = t.MutationObserver || t.WebKitMutationObserver;
    if (o) {
      var a = 0,
        s = new o(n),
        c = t.document.createTextNode("");
      s.observe(c, {
        characterData: !0
      }), i = function() {
        c.data = a = ++a % 2;
      };
    } else if (t.setImmediate || "undefined" == typeof t.MessageChannel) i = "document" in t &&
      "onreadystatechange" in t.document.createElement("script") ? function() {
        var e = t.document.createElement("script");
        e.onreadystatechange = function() {
          n(), e.onreadystatechange = null, e.parentNode.removeChild(e), e = null;
        }, t.document.documentElement.appendChild(e);
      } : function() {
        setTimeout(n, 0);
      };
    else {
      var u = new t.MessageChannel();
      u.port1.onmessage = n, i = function() {
        u.port2.postMessage(0);
      };
    }
    var l,
      d = [];
    module.exports = r;
  }).call(exports, function() {
    return this;
  }());
}
