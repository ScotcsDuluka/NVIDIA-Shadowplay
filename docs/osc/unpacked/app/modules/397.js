// ─────────────────────────────────────────────────────────────
// APP MODULE 397
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(21),
    o = n(15),
    r = n(53),
    a = n(29),
    l = n(27),
    s = n(123),
    d = n(51),
    c = n(112),
    u = n(24),
    f = n(55),
    m = n(19).f,
    g = n(392)(0),
    p = n(18);
  e.exports = function(e, t, n, h, b, x) {
    var v = i[e],
      y = v,
      w = b ? "set" : "add",
      S = y && y.prototype,
      E = {};
    return p && "function" == typeof y && (x || S.forEach && !a(function() {
      (new y).entries().next()
    })) ? (y = t(function(t, n) {
      c(t, y, e, "_c"), t._c = new v, void 0 != n && d(n, b, t[w], t)
    }), g("add,clear,delete,forEach,get,has,set,keys,values,entries,toJSON".split(","), function(e) {
      var t = "add" == e || "set" == e;
      e in S && (!x || "clear" != e) && l(y.prototype, e, function(n, i) {
        if (c(this, y, e), !t && x && !u(n)) return "get" == e && void 0;
        var o = this._c[e](0 === n ? 0 : n, i);
        return t ? this : o
      })
    }), x || m(y.prototype, "size", {
      get: function() {
        return this._c.size
      }
    })) : (y = h.getConstructor(t, e, b, w), s(y.prototype, n), r.NEED = !0), f(y, e), E[e] = y, o(o.G + o.W + o.F,
      E), x || h.setStrong(y, e, b), y
  }
}
