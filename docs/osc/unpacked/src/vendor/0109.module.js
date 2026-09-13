// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 109
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  (function(e) {
    function r(t) {
      var n,
        r = !1,
        s = !1,
        c = !1 !== t.jsonp;
      if (e.location) {
        var u = "https:" == location.protocol,
          l = location.port;
        l || (l = u ? 443 : 80), r = t.hostname != location.hostname || l != t.port, s = t.secure != u;
      }
      if (t.xdomain = r, t.xscheme = s, n = new i(t), "open" in n && !t.forceJSONP) return new o(t);
      if (!c) throw new Error("JSONP disabled");
      return new a(t);
    }
    var i = require(70),
      o = require(274),
      a = require(273),
      s = require(275);
    exports.polling = r, exports.websocket = s;
  }).call(exports, function() {
    return this;
  }());
}
