// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 102
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e) {
      var t = +this._x.call(null, e),
        r = +this._y.call(null, e);
      return n(this.cover(t, r), t, r, e);
    }

    function n(e, t, n, r) {
      if (isNaN(t) || isNaN(n)) return e;
      var i,
        o,
        a,
        s,
        c,
        u,
        l,
        d,
        f,
        h = e._root,
        p = {
          data: r
        },
        m = e._x0,
        v = e._y0,
        g = e._x1,
        y = e._y1;
      if (!h) return e._root = p, e;
      for (; h.length;)
        if ((u = t >= (o = (m + g) / 2)) ? m = o : g = o, (l = n >= (a = (v + y) / 2)) ? v = a : y = a, i =
          h, !(h = h[d = l << 1 | u])) return i[d] = p, e;
      if (s = +e._x.call(null, h.data), c = +e._y.call(null, h.data), t === s && n === c) return p.next = h,
        i ? i[d] = p : e._root = p, e;
      do i = i ? i[d] = new Array(4) : e._root = new Array(4), (u = t >= (o = (m + g) / 2)) ? m = o : g = o,
        (l = n >= (a = (v + y) / 2)) ? v = a : y = a; while ((d = l << 1 | u) === (f = (c >= a) << 1 | s >=
          o));
      return i[f] = h, i[d] = p, e;
    }

    function r(e) {
      var t,
        r,
        i,
        o,
        a = e.length,
        s = new Array(a),
        c = new Array(a),
        u = 1 / 0,
        l = 1 / 0,
        d = -(1 / 0),
        f = -(1 / 0);
      for (r = 0; r < a; ++r) isNaN(i = +this._x.call(null, t = e[r])) || isNaN(o = +this._y.call(null,
        t)) || (s[r] = i, c[r] = o, i < u && (u = i), i > d && (d = i), o < l && (l = o), o > f && (f = o));
      if (u > d || l > f) return this;
      for (this.cover(u, l).cover(d, f), r = 0; r < a; ++r) n(this, s[r], c[r], e[r]);
      return this;
    }

    function i(e, t) {
      if (isNaN(e = +e) || isNaN(t = +t)) return this;
      var n = this._x0,
        r = this._y0,
        i = this._x1,
        o = this._y1;
      if (isNaN(n)) i = (n = Math.floor(e)) + 1, o = (r = Math.floor(t)) + 1;
      else {
        for (var a, s, c = i - n, u = this._root; n > e || e >= i || r > t || t >= o;) switch (s = (t <
          r) << 1 | e < n, a = new Array(4), a[s] = u, u = a, c *= 2, s) {
          case 0:
            i = n + c, o = r + c;
            break;
          case 1:
            n = i - c, o = r + c;
            break;
          case 2:
            i = n + c, r = o - c;
            break;
          case 3:
            n = i - c, r = o - c;
        }
        this._root && this._root.length && (this._root = u);
      }
      return this._x0 = n, this._y0 = r, this._x1 = i, this._y1 = o, this;
    }

    function o() {
      var e = [];
      return this.visit(function(t) {
        if (!t.length)
          do e.push(t.data); while (t = t.next);
      }), e;
    }

    function a(e) {
      return arguments.length ? this.cover(+e[0][0], +e[0][1]).cover(+e[1][0], +e[1][1]) : isNaN(this._x0) ?
        void 0 : [
          [this._x0, this._y0],
          [this._x1, this._y1]
        ];
    }

    function s(e, t, n, r, i) {
      this.node = e, this.x0 = t, this.y0 = n, this.x1 = r, this.y1 = i;
    }

    function c(e, t, n) {
      var r,
        i,
        o,
        a,
        c,
        u,
        l,
        d = this._x0,
        f = this._y0,
        h = this._x1,
        p = this._y1,
        m = [],
        v = this._root;
      for (v && m.push(new s(v, d, f, h, p)), null == n ? n = 1 / 0 : (d = e - n, f = t - n, h = e + n, p =
          t + n, n *= n); u = m.pop();)
        if (!(!(v = u.node) || (i = u.x0) > h || (o = u.y0) > p || (a = u.x1) < d || (c = u.y1) < f))
          if (v.length) {
            var g = (i + a) / 2,
              y = (o + c) / 2;
            m.push(new s(v[3], g, y, a, c), new s(v[2], i, y, g, c), new s(v[1], g, o, a, y), new s(v[0], i,
              o, g, y)), (l = (t >= y) << 1 | e >= g) && (u = m[m.length - 1], m[m.length - 1] = m[m
              .length - 1 - l], m[m.length - 1 - l] = u);
          } else {
            var b = e - +this._x.call(null, v.data),
              E = t - +this._y.call(null, v.data),
              _ = b * b + E * E;
            if (_ < n) {
              var $ = Math.sqrt(n = _);
              d = e - $, f = t - $, h = e + $, p = t + $, r = v.data;
            }
          }
      return r;
    }

    function u(e) {
      if (isNaN(o = +this._x.call(null, e)) || isNaN(a = +this._y.call(null, e))) return this;
      var t,
        n,
        r,
        i,
        o,
        a,
        s,
        c,
        u,
        l,
        d,
        f,
        h = this._root,
        p = this._x0,
        m = this._y0,
        v = this._x1,
        g = this._y1;
      if (!h) return this;
      if (h.length)
        for (;;) {
          if ((u = o >= (s = (p + v) / 2)) ? p = s : v = s, (l = a >= (c = (m + g) / 2)) ? m = c : g = c,
            t = h, !(h = h[d = l << 1 | u])) return this;
          if (!h.length) break;
          (t[d + 1 & 3] || t[d + 2 & 3] || t[d + 3 & 3]) && (n = t, f = d);
        }
      for (; h.data !== e;)
        if (r = h, !(h = h.next)) return this;
      return (i = h.next) && delete h.next, r ? (i ? r.next = i : delete r.next, this) : t ? (i ? t[d] = i :
        delete t[d], (h = t[0] || t[1] || t[2] || t[3]) && h === (t[3] || t[2] || t[1] || t[0]) && !h
        .length && (n ? n[f] = h : this._root = h), this) : (this._root = i, this);
    }

    function l(e) {
      for (var t = 0, n = e.length; t < n; ++t) this.remove(e[t]);
      return this;
    }

    function d() {
      return this._root;
    }

    function f() {
      var e = 0;
      return this.visit(function(t) {
        if (!t.length)
          do ++e; while (t = t.next);
      }), e;
    }

    function h(e) {
      var t,
        n,
        r,
        i,
        o,
        a,
        c = [],
        u = this._root;
      for (u && c.push(new s(u, this._x0, this._y0, this._x1, this._y1)); t = c.pop();)
        if (!e(u = t.node, r = t.x0, i = t.y0, o = t.x1, a = t.y1) && u.length) {
          var l = (r + o) / 2,
            d = (i + a) / 2;
          (n = u[3]) && c.push(new s(n, l, d, o, a)), (n = u[2]) && c.push(new s(n, r, d, l, a)), (n = u[
            1]) && c.push(new s(n, l, i, o, d)), (n = u[0]) && c.push(new s(n, r, i, l, d));
        }
      return this;
    }

    function p(e) {
      var t,
        n = [],
        r = [];
      for (this._root && n.push(new s(this._root, this._x0, this._y0, this._x1, this._y1)); t = n.pop();) {
        var i = t.node;
        if (i.length) {
          var o,
            a = t.x0,
            c = t.y0,
            u = t.x1,
            l = t.y1,
            d = (a + u) / 2,
            f = (c + l) / 2;
          (o = i[0]) && n.push(new s(o, a, c, d, f)), (o = i[1]) && n.push(new s(o, d, c, u, f)), (o = i[
            2]) && n.push(new s(o, a, f, d, l)), (o = i[3]) && n.push(new s(o, d, f, u, l));
        }
        r.push(t);
      }
      for (; t = r.pop();) e(t.node, t.x0, t.y0, t.x1, t.y1);
      return this;
    }

    function m(e) {
      return e[0];
    }

    function v(e) {
      return arguments.length ? (this._x = e, this) : this._x;
    }

    function g(e) {
      return e[1];
    }

    function y(e) {
      return arguments.length ? (this._y = e, this) : this._y;
    }

    function b(e, t, n) {
      var r = new E(null == t ? m : t, null == n ? g : n, NaN, NaN, NaN, NaN);
      return null == e ? r : r.addAll(e);
    }

    function E(e, t, n, r, i, o) {
      this._x = e, this._y = t, this._x0 = n, this._y0 = r, this._x1 = i, this._y1 = o, this._root = void 0;
    }

    function _(e) {
      for (var t = {
          data: e.data
        }, n = t; e = e.next;) n = n.next = {
        data: e.data
      };
      return t;
    }
    var $ = b.prototype = E.prototype;
    $.copy = function() {
        var e,
          t,
          n = new E(this._x, this._y, this._x0, this._y0, this._x1, this._y1),
          r = this._root;
        if (!r) return n;
        if (!r.length) return n._root = _(r), n;
        for (e = [{
            source: r,
            target: n._root = new Array(4)
          }]; r = e.pop();)
          for (var i = 0; i < 4; ++i)(t = r.source[i]) && (t.length ? e.push({
            source: t,
            target: r.target[i] = new Array(4)
          }) : r.target[i] = _(t));
        return n;
      }, $.add = t, $.addAll = r, $.cover = i, $.data = o, $.extent = a, $.find = c, $.remove = u, $
      .removeAll = l, $.root = d, $.size = f, $.visit = h, $.visitAfter = p, $.x = v, $.y = y, e.quadtree =
      b, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
