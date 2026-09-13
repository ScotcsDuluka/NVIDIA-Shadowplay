// ─────────────────────────────────────────────────────────────
// APP MODULE 130
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t(e) {
      var t = +this._x.call(null, e),
        i = +this._y.call(null, e);
      return n(this.cover(t, i), t, i, e)
    }

    function n(e, t, n, i) {
      if (isNaN(t) || isNaN(n)) return e;
      var o, r, a, l, s, d, c, u, f, m = e._root,
        g = {
          data: i
        },
        p = e._x0,
        h = e._y0,
        b = e._x1,
        x = e._y1;
      if (!m) return e._root = g, e;
      for (; m.length;)
        if ((d = t >= (r = (p + b) / 2)) ? p = r : b = r, (c = n >= (a = (h + x) / 2)) ? h = a : x = a, o = m, !(m =
            m[u = c << 1 | d])) return o[u] = g, e;
      if (l = +e._x.call(null, m.data), s = +e._y.call(null, m.data), t === l && n === s) return g.next = m, o ? o[
        u] = g : e._root = g, e;
      do o = o ? o[u] = new Array(4) : e._root = new Array(4), (d = t >= (r = (p + b) / 2)) ? p = r : b = r, (c = n >=
        (a = (h + x) / 2)) ? h = a : x = a; while ((u = c << 1 | d) === (f = (s >= a) << 1 | l >= r));
      return o[f] = m, o[u] = g, e
    }

    function i(e) {
      var t, i, o, r, a = e.length,
        l = new Array(a),
        s = new Array(a),
        d = 1 / 0,
        c = 1 / 0,
        u = -(1 / 0),
        f = -(1 / 0);
      for (i = 0; i < a; ++i) isNaN(o = +this._x.call(null, t = e[i])) || isNaN(r = +this._y.call(null, t)) || (l[i] =
        o, s[i] = r, o < d && (d = o), o > u && (u = o), r < c && (c = r), r > f && (f = r));
      if (d > u || c > f) return this;
      for (this.cover(d, c).cover(u, f), i = 0; i < a; ++i) n(this, l[i], s[i], e[i]);
      return this
    }

    function o(e, t) {
      if (isNaN(e = +e) || isNaN(t = +t)) return this;
      var n = this._x0,
        i = this._y0,
        o = this._x1,
        r = this._y1;
      if (isNaN(n)) o = (n = Math.floor(e)) + 1, r = (i = Math.floor(t)) + 1;
      else {
        for (var a, l, s = o - n, d = this._root; n > e || e >= o || i > t || t >= r;) switch (l = (t < i) << 1 | e <
          n, a = new Array(4), a[l] = d, d = a, s *= 2, l) {
          case 0:
            o = n + s, r = i + s;
            break;
          case 1:
            n = o - s, r = i + s;
            break;
          case 2:
            o = n + s, i = r - s;
            break;
          case 3:
            n = o - s, i = r - s
        }
        this._root && this._root.length && (this._root = d)
      }
      return this._x0 = n, this._y0 = i, this._x1 = o, this._y1 = r, this
    }

    function r() {
      var e = [];
      return this.visit(function(t) {
        if (!t.length)
          do e.push(t.data); while (t = t.next)
      }), e
    }

    function a(e) {
      return arguments.length ? this.cover(+e[0][0], +e[0][1]).cover(+e[1][0], +e[1][1]) : isNaN(this._x0) ?
        void 0 : [
          [this._x0, this._y0],
          [this._x1, this._y1]
        ]
    }

    function l(e, t, n, i, o) {
      this.node = e, this.x0 = t, this.y0 = n, this.x1 = i, this.y1 = o
    }

    function s(e, t, n) {
      var i, o, r, a, s, d, c, u = this._x0,
        f = this._y0,
        m = this._x1,
        g = this._y1,
        p = [],
        h = this._root;
      for (h && p.push(new l(h, u, f, m, g)), null == n ? n = 1 / 0 : (u = e - n, f = t - n, m = e + n, g = t + n,
          n *= n); d = p.pop();)
        if (!(!(h = d.node) || (o = d.x0) > m || (r = d.y0) > g || (a = d.x1) < u || (s = d.y1) < f))
          if (h.length) {
            var b = (o + a) / 2,
              x = (r + s) / 2;
            p.push(new l(h[3], b, x, a, s), new l(h[2], o, x, b, s), new l(h[1], b, r, a, x), new l(h[0], o, r, b,
              x)), (c = (t >= x) << 1 | e >= b) && (d = p[p.length - 1], p[p.length - 1] = p[p.length - 1 - c], p[p
                .length - 1 - c] = d)
          } else {
            var v = e - +this._x.call(null, h.data),
              y = t - +this._y.call(null, h.data),
              w = v * v + y * y;
            if (w < n) {
              var S = Math.sqrt(n = w);
              u = e - S, f = t - S, m = e + S, g = t + S, i = h.data
            }
          } return i
    }

    function d(e) {
      if (isNaN(r = +this._x.call(null, e)) || isNaN(a = +this._y.call(null, e))) return this;
      var t, n, i, o, r, a, l, s, d, c, u, f, m = this._root,
        g = this._x0,
        p = this._y0,
        h = this._x1,
        b = this._y1;
      if (!m) return this;
      if (m.length)
        for (;;) {
          if ((d = r >= (l = (g + h) / 2)) ? g = l : h = l, (c = a >= (s = (p + b) / 2)) ? p = s : b = s, t = m, !(m =
              m[u = c << 1 | d])) return this;
          if (!m.length) break;
          (t[u + 1 & 3] || t[u + 2 & 3] || t[u + 3 & 3]) && (n = t, f = u)
        }
      for (; m.data !== e;)
        if (i = m, !(m = m.next)) return this;
      return (o = m.next) && delete m.next, i ? (o ? i.next = o : delete i.next, this) : t ? (o ? t[u] = o : delete t[
        u], (m = t[0] || t[1] || t[2] || t[3]) && m === (t[3] || t[2] || t[1] || t[0]) && !m.length && (n ? n[f] =
        m : this._root = m), this) : (this._root = o, this)
    }

    function c(e) {
      for (var t = 0, n = e.length; t < n; ++t) this.remove(e[t]);
      return this
    }

    function u() {
      return this._root
    }

    function f() {
      var e = 0;
      return this.visit(function(t) {
        if (!t.length)
          do ++e; while (t = t.next)
      }), e
    }

    function m(e) {
      var t, n, i, o, r, a, s = [],
        d = this._root;
      for (d && s.push(new l(d, this._x0, this._y0, this._x1, this._y1)); t = s.pop();)
        if (!e(d = t.node, i = t.x0, o = t.y0, r = t.x1, a = t.y1) && d.length) {
          var c = (i + r) / 2,
            u = (o + a) / 2;
          (n = d[3]) && s.push(new l(n, c, u, r, a)), (n = d[2]) && s.push(new l(n, i, u, c, a)), (n = d[1]) && s
            .push(new l(n, c, o, r, u)), (n = d[0]) && s.push(new l(n, i, o, c, u))
        } return this
    }

    function g(e) {
      var t, n = [],
        i = [];
      for (this._root && n.push(new l(this._root, this._x0, this._y0, this._x1, this._y1)); t = n.pop();) {
        var o = t.node;
        if (o.length) {
          var r, a = t.x0,
            s = t.y0,
            d = t.x1,
            c = t.y1,
            u = (a + d) / 2,
            f = (s + c) / 2;
          (r = o[0]) && n.push(new l(r, a, s, u, f)), (r = o[1]) && n.push(new l(r, u, s, d, f)), (r = o[2]) && n
            .push(new l(r, a, f, u, c)), (r = o[3]) && n.push(new l(r, u, f, d, c))
        }
        i.push(t)
      }
      for (; t = i.pop();) e(t.node, t.x0, t.y0, t.x1, t.y1);
      return this
    }

    function p(e) {
      return e[0]
    }

    function h(e) {
      return arguments.length ? (this._x = e, this) : this._x
    }

    function b(e) {
      return e[1]
    }

    function x(e) {
      return arguments.length ? (this._y = e, this) : this._y
    }

    function v(e, t, n) {
      var i = new y(null == t ? p : t, null == n ? b : n, NaN, NaN, NaN, NaN);
      return null == e ? i : i.addAll(e)
    }

    function y(e, t, n, i, o, r) {
      this._x = e, this._y = t, this._x0 = n, this._y0 = i, this._x1 = o, this._y1 = r, this._root = void 0
    }

    function w(e) {
      for (var t = {
          data: e.data
        }, n = t; e = e.next;) n = n.next = {
        data: e.data
      };
      return t
    }
    var S = v.prototype = y.prototype;
    S.copy = function() {
        var e, t, n = new y(this._x, this._y, this._x0, this._y0, this._x1, this._y1),
          i = this._root;
        if (!i) return n;
        if (!i.length) return n._root = w(i),
          n;
        for (e = [{
            source: i,
            target: n._root = new Array(4)
          }]; i = e.pop();)
          for (var o = 0; o < 4; ++o)(t = i.source[o]) && (t.length ? e.push({
            source: t,
            target: i.target[o] = new Array(4)
          }) : i.target[o] = w(t));
        return n
      }, S.add = t, S.addAll = i, S.cover = o, S.data = r, S.extent = a, S.find = s, S.remove = d, S.removeAll = c, S
      .root = u, S.size = f, S.visit = m, S.visitAfter = g, S.x = h, S.y = x, e.quadtree = v, Object.defineProperty(e,
        "__esModule", {
          value: !0
        })
  })
}
