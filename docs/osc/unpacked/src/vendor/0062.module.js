// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 62
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
      var r = new t();
      if (e instanceof t) e.each(function(e, t) {
        r.set(t, e);
      });
      else if (Array.isArray(e)) {
        var i,
          o = -1,
          a = e.length;
        if (null == n)
          for (; ++o < a;) r.set(o, e[o]);
        else
          for (; ++o < a;) r.set(n(i = e[o], o, e), i);
      } else if (e)
        for (var s in e) r.set(s, e[s]);
      return r;
    }

    function r() {
      function e(t, i, o, a) {
        if (i >= l.length) return null != r && t.sort(r), null != c ? c(t) : t;
        for (var s, u, d, f = -1, h = t.length, p = l[i++], m = n(), v = o(); ++f < h;)(d = m.get(s = p(u =
          t[f]) + "")) ? d.push(u) : m.set(s, [u]);
        return m.each(function(t, n) {
          a(v, n, e(t, i, o, a));
        }), v;
      }

      function t(e, n) {
        if (++n > l.length) return e;
        var r,
          i = d[n - 1];
        return null != c && n >= l.length ? r = e.entries() : (r = [], e.each(function(e, i) {
          r.push({
            key: i,
            values: t(e, n)
          });
        })), null != i ? r.sort(function(e, t) {
          return i(e.key, t.key);
        }) : r;
      }
      var r,
        c,
        u,
        l = [],
        d = [];
      return u = {
        object: function(t) {
          return e(t, 0, i, o);
        },
        map: function(t) {
          return e(t, 0, a, s);
        },
        entries: function(n) {
          return t(e(n, 0, a, s), 0);
        },
        key: function(e) {
          return l.push(e), u;
        },
        sortKeys: function(e) {
          return d[l.length - 1] = e, u;
        },
        sortValues: function(e) {
          return r = e, u;
        },
        rollup: function(e) {
          return c = e, u;
        }
      };
    }

    function i() {
      return {};
    }

    function o(e, t, n) {
      e[t] = n;
    }

    function a() {
      return n();
    }

    function s(e, t, n) {
      e.set(t, n);
    }

    function c() {}

    function u(e, t) {
      var n = new c();
      if (e instanceof c) e.each(function(e) {
        n.add(e);
      });
      else if (e) {
        var r = -1,
          i = e.length;
        if (null == t)
          for (; ++r < i;) n.add(e[r]);
        else
          for (; ++r < i;) n.add(t(e[r], r, e));
      }
      return n;
    }

    function l(e) {
      var t = [];
      for (var n in e) t.push(n);
      return t;
    }

    function d(e) {
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
    var h = "$";
    t.prototype = n.prototype = {
      constructor: t,
      has: function(e) {
        return h + e in this;
      },
      get: function(e) {
        return this[h + e];
      },
      set: function(e, t) {
        return this[h + e] = t, this;
      },
      remove: function(e) {
        var t = h + e;
        return t in this && delete this[t];
      },
      clear: function() {
        for (var e in this) e[0] === h && delete this[e];
      },
      keys: function() {
        var e = [];
        for (var t in this) t[0] === h && e.push(t.slice(1));
        return e;
      },
      values: function() {
        var e = [];
        for (var t in this) t[0] === h && e.push(this[t]);
        return e;
      },
      entries: function() {
        var e = [];
        for (var t in this) t[0] === h && e.push({
          key: t.slice(1),
          value: this[t]
        });
        return e;
      },
      size: function() {
        var e = 0;
        for (var t in this) t[0] === h && ++e;
        return e;
      },
      empty: function() {
        for (var e in this)
          if (e[0] === h) return !1;
        return !0;
      },
      each: function(e) {
        for (var t in this) t[0] === h && e(this[t], t.slice(1), this);
      }
    };
    var p = n.prototype;
    c.prototype = u.prototype = {
      constructor: c,
      has: p.has,
      add: function(e) {
        return e += "", this[h + e] = e, this;
      },
      remove: p.remove,
      clear: p.clear,
      values: p.keys,
      size: p.size,
      empty: p.empty,
      each: p.each
    }, e.nest = r, e.set = u, e.map = n, e.keys = l, e.values = d, e.entries = f, Object.defineProperty(e,
      "__esModule", {
        value: !0
      });
  });
}
