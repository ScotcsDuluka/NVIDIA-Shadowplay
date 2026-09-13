// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 108
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  (function(t) {
    function r(e, n) {
      var r = e,
        n = n || t.location;
      return null == e && (e = n.protocol + "//" + n.host), "string" == typeof e && ("/" == e.charAt(0) && (
          e = "/" == e.charAt(1) ? n.protocol + e : n.hostname + e), /^(https?|wss?):\/\//.test(e) || (o(
            "protocol-less url %s", e), e = "undefined" != typeof n ? n.protocol + "//" + e : "https://" +
          e), o("parse %s", e), r = i(e)), r.port || (/^(http|ws)$/.test(r.protocol) ? r.port = "80" :
          /^(http|ws)s$/.test(r.protocol) && (r.port = "443")), r.path = r.path || "/", r.id = r.protocol +
        "://" + r.host + ":" + r.port, r.href = r.protocol + "://" + r.host + (n && n.port == r.port ? "" :
          ":" + r.port), r;
    }
    var i = require(282),
      o = require(32)("socket.io-client:url");
    module.exports = r;
  }).call(exports, function() {
    return this;
  }());
}
