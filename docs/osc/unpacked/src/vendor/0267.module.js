// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 267
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  function r(e, t) {
    "object" == typeof e && (t = e, e = void 0), t = t || {};
    var n,
      r = i(e),
      o = r.source,
      u = r.id;
    return t.forceNew || t["force new connection"] || !1 === t.multiplex ? (s("ignoring socket cache for %s",
      o), n = a(o, t)) : (c[u] || (s("new io instance for %s", o), c[u] = a(o, t)), n = c[u]), n.socket(r
      .path);
  }
  var i = require(108),
    o = require(72),
    a = require(105),
    s = require(32)("socket.io-client");
  module.exports = exports = r;
  var c = exports.managers = {};
  exports.protocol = o.protocol, exports.connect = r, exports.Manager = require(105), exports.Socket =
    require(107);
}
