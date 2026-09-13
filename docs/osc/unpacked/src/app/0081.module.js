// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 81
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t() {}

    function n(e, n) {
      var i = new t();
      if (e instanceof t) e.each(function(e, t) {
        i.set(t, e);
      });
      else if (Array.isArray(e)) {
        var o,
          r = -1,
          a = e.length;
        if (null == n)
          for (; ++r < a;) i.set(r, e[r]);
        else
          for (; ++r < a;) i.set(n(o = e[r], r, e), o);
      } else if (e)
        for (var l in e) i.set(l, e[l]);
      return i;
    }

    function i() {
      function e(t, o, r, a) {
        if (o >= c.length) return null != i && t.sort(i), null != s ? s(t) : t;
        for (var l, d, u, f = -1, m = t.length, g = c[o++], p = n(), h = r(); ++f < m;)(u = p.get(l = g(d =
          t[f]) + "")) ? u.push(d) : p.set(l, [d]);
        return p.each(function(t, n) {
          a(h, n, e(t, o, r, a));
        }), h;
      }

      function t(e, n) {
        if (++n > c.length) return e;
        var i,
          o = u[n - 1];
        return null != s && n >= c.length ? i = e.entries() : (i = [], e.each(function(e, o) {
          i.push({
            key: o,
            values: t(e, n)
          });
        })), null != o ? i.sort(function(e, t) {
          return o(e.key, t.key);
        }) : i;
      }
      var i,
        s,
        d,
        c = [],
        u = [];
      return d = {
        object: function(t) {
          return e(t, 0, o, r);
        },
        map: function(t) {
          return e(t, 0, a, l);
        },
        entries: function(n) {
          return t(e(n, 0, a, l), 0);
        },
        key: function(e) {
          return c.push(e), d;
        },
        sortKeys: function(e) {
          return u[c.length - 1] = e, d;
        },
        sortValues: function(e) {
          return i = e, d;
        },
        rollup: function(e) {
          return s = e, d;
        }
      };
    }

    function o() {
      return {};
    }

    function r(e, t, n) {
      e[t] = n;
    }

    function a() {
      return n();
    }

    function l(e, t, n) {
      e.set(t, n);
    }

    function s() {}

    function d(e, t) {
      var n = new s();
      if (e instanceof s) e.each(function(e) {
        n.add(e);
      });
      else if (e) {
        var i = -1,
          o = e.length;
        if (null == t)
          for (; ++i < o;) n.add(e[i]);
        else
          for (; ++i < o;) n.add(t(e[i], i, e));
      }
      return n;
    }

    function c(e) {
      var t = [];
      for (var n in e) t.push(n);
      return t;
    }

    function u(e) {
      var t = [];
      for (var n in e) t.push(e[n]);
      return t;
    }

    function f(e) {
      var t = [];
      for (var n in e) t.push({
        key: n,
        value: e[n]
      });
      return t;
    }
    var m = "$";
    t.prototype = n.prototype = {
      constructor: t,
      has: function(e) {
        return m + e in this;
      },
      get: function(e) {
        return this[m + e];
      },
      set: function(e, t) {
        return this[m + e] = t, this;
      },
      remove: function(e) {
        var t = m + e;
        return t in this && delete this[t];
      },
      clear: function() {
        for (var e in this) e[0] === m && delete this[e];
      },
      keys: function() {
        var e = [];
        for (var t in this) t[0] === m && e.push(t.slice(1));
        return e;
      },
      values: function() {
        var e = [];
        for (var t in this) t[0] === m && e.push(this[t]);
        return e;
      },
      entries: function() {
        var e = [];
        for (var t in this) t[0] === m && e.push({
          key: t.slice(1),
          value: this[t]
        });
        return e;
      },
      size: function() {
        var e = 0;
        for (var t in this) t[0] === m && ++e;
        return e;
      },
      empty: function() {
        for (var e in this)
          if (e[0] === m) return !1;
        return !0;
      },
      each: function(e) {
        for (var t in this) t[0] === m && e(this[t], t.slice(1), this);
      }
    };
    var g = n.prototype;
    s.prototype = d.prototype = {
      constructor: s,
      has: g.has,
      add: function(e) {
        return e += "", this[m + e] = e, this;
      },
      remove: g.remove,
      clear: g.clear,
      values: g.keys,
      size: g.size,
      empty: g.empty,
      each: g.each
    }, e.nest = i, e.set = d, e.map = n, e.keys = c, e.values = u, e.entries = f, Object.defineProperty(e,
      "__esModule", {
        value: !0
      });
  });
}
