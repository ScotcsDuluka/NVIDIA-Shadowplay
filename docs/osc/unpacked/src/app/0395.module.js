// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 395
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(19).f,
    o = require(70),
    r = require(123),
    a = require(36),
    l = require(112),
    s = require(51),
    d = require(69),
    c = require(119),
    u = require(409),
    f = require(18),
    m = require(53).fastKey,
    g = require(125),
    p = f ? "_s" : "size",
    h = function(e, t) {
      var n,
        i = m(t);
      if ("F" !== i) return e._i[i];
      for (n = e._f; n; n = n.n)
        if (n.k == t) return n;
    };
  module.exports = {
    getConstructor: function(e, t, n, d) {
      var c = e(function(e, i) {
        l(e, c, t, "_i"), e._t = t, e._i = o(null), e._f = void 0, e._l = void 0, e[p] = 0, void 0 !=
          i && s(i, n, e[d], e);
      });
      return r(c.prototype, {
        clear: function() {
          for (var e = g(this, t), n = e._i, i = e._f; i; i = i.n) i.r = !0, i.p && (i.p = i.p.n =
            void 0), delete n[i.i];
          e._f = e._l = void 0, e[p] = 0;
        },
        delete: function(e) {
          var n = g(this, t),
            i = h(n, e);
          if (i) {
            var o = i.n,
              r = i.p;
            delete n._i[i.i], i.r = !0, r && (r.n = o), o && (o.p = r), n._f == i && (n._f = o), n
              ._l == i && (n._l = r), n[p]--;
          }
          return !!i;
        },
        forEach: function(e) {
          g(this, t);
          for (var n, i = a(e, arguments.length > 1 ? arguments[1] : void 0, 3); n = n ? n.n : this
            ._f;)
            for (i(n.v, n.k, this); n && n.r;) n = n.p;
        },
        has: function(e) {
          return !!h(g(this, t), e);
        }
      }), f && i(c.prototype, "size", {
        get: function() {
          return g(this, t)[p];
        }
      }), c;
    },
    def: function(e, t, n) {
      var i,
        o,
        r = h(e, t);
      return r ? r.v = n : (e._l = r = {
        i: o = m(t, !0),
        k: t,
        v: n,
        p: i = e._l,
        n: void 0,
        r: !1
      }, e._f || (e._f = r), i && (i.n = r), e[p]++, "F" !== o && (e._i[o] = r)), e;
    },
    getEntry: h,
    setStrong: function(e, t, n) {
      d(e, t, function(e, n) {
        this._t = g(e, t), this._k = n, this._l = void 0;
      }, function() {
        for (var e = this, t = e._k, n = e._l; n && n.r;) n = n.p;
        return e._t && (e._l = n = n ? n.n : e._t._f) ? "keys" == t ? c(0, n.k) : "values" == t ? c(0,
          n.v) : c(0, [n.k, n.v]) : (e._t = void 0, c(1));
      }, n ? "entries" : "values", !n, !0), u(t);
    }
  };
}
