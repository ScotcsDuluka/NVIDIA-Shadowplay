// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 93
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r, i, o, a = n(36),
    s = n(183),
    c = n(80),
    u = n(50),
    l = n(4),
    d = l.process,
    f = l.setImmediate,
    h = l.clearImmediate,
    p = l.MessageChannel,
    m = l.Dispatch,
    v = 0,
    g = {},
    y = "onreadystatechange",
    b = function() {
      var e = +this;
      if (g.hasOwnProperty(e)) {
        var t = g[e];
        delete g[e], t()
      }
    },
    E = function(e) {
      b.call(e.data)
    };
  f && h || (f = function(e) {
      for (var t = [], n = 1; arguments.length > n;) t.push(arguments[n++]);
      return g[++v] = function() {
        s("function" == typeof e ? e : Function(e), t)
      }, r(v), v
    }, h = function(e) {
      delete g[e]
    }, "process" == n(27)(d) ? r = function(e) {
      d.nextTick(a(b, e, 1))
    } : m && m.now ? r = function(e) {
      m.now(a(b, e, 1))
    } : p ? (i = new p, o = i.port2, i.port1.onmessage = E, r = a(o.postMessage, o, 1)) : l.addEventListener &&
    "function" == typeof postMessage && !l.importScripts ? (r = function(e) {
      l.postMessage(e + "", "*")
    }, l.addEventListener("message", E, !1)) : r = y in u("script") ? function(e) {
      c.appendChild(u("script"))[y] = function() {
        c.removeChild(this), b.call(e)
      }
    } : function(e) {
      setTimeout(a(b, e, 1), 0)
    }), e.exports = {
    set: f,
    clear: h
  }
}
