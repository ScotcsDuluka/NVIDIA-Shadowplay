// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 267
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  function r(e, t) {
    "object" == typeof e && (t = e, e = void 0), t = t || {};
    var n, r = i(e),
      o = r.source,
      u = r.id;
    return t.forceNew || t["force new connection"] || !1 === t.multiplex ? (s("ignoring socket cache for %s", o), n = a(
      o, t)) : (c[u] || (s("new io instance for %s", o), c[u] = a(o, t)), n = c[u]), n.socket(r.path)
  }
  var i = n(108),
    o = n(72),
    a = n(105),
    s = n(32)("socket.io-client");
  e.exports = t = r;
  var c = t.managers = {};
  t.protocol = o.protocol, t.connect = r, t.Manager = n(105), t.Socket = n(107)
}
