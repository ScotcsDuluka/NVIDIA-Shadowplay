// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 436
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e, t) {
      return e.parent === t.parent ? 1 : 2;
    }

    function n(e) {
      return e.reduce(i, 0) / e.length;
    }

    function i(e, t) {
      return e + t.x;
    }

    function o(e) {
      return 1 + e.reduce(r, 0);
    }

    function r(e, t) {
      return Math.max(e, t.y);
    }

    function a(e) {
      for (var t; t = e.children;) e = t[0];
      return e;
    }

    function l(e) {
      for (var t; t = e.children;) e = t[t.length - 1];
      return e;
    }

    function s() {
      function e(e) {
        var t,
          c = 0;
        e.eachAfter(function(e) {
          var r = e.children;
          r ? (e.x = n(r), e.y = o(r)) : (e.x = t ? c += i(e, t) : 0, e.y = 0, t = e);
        });
        var u = a(e),
          f = l(e),
          m = u.x - i(u, f) / 2,
          g = f.x + i(f, u) / 2;
        return e.eachAfter(d ? function(t) {
          t.x = (t.x - e.x) * r, t.y = (e.y - t.y) * s;
        } : function(t) {
          t.x = (t.x - m) / (g - m) * r, t.y = (1 - (e.y ? t.y / e.y : 1)) * s;
        });
      }
      var i = t,
        r = 1,
        s = 1,
        d = !1;
      return e.separation = function(t) {
        return arguments.length ? (i = t, e) : i;
      }, e.size = function(t) {
        return arguments.length ? (d = !1, r = +t[0], s = +t[1], e) : d ? null : [r, s];
      }, e.nodeSize = function(t) {
        return arguments.length ? (d = !0, r = +t[0], s = +t[1], e) : d ? [r, s] : null;
      }, e;
    }

    function d(e) {
      var t = 0,
        n = e.children,
        i = n && n.length;
      if (i)
        for (; --i >= 0;) t += n[i].value;
      else t = 1;
      e.value = t;
    }

    function c() {
      return this.eachAfter(d);
    }

    function u(e) {
      var t,
        n,
        i,
        o,
        r = this,
        a = [r];
      do
        for (t = a.reverse(), a = []; r = t.pop();)
          if (e(r), n = r.children)
            for (i = 0, o = n.length; i < o; ++i) a.push(n[i]); while (a.length);
      return this;
    }

    function f(e) {
      for (var t, n, i = this, o = [i]; i = o.pop();)
        if (e(i), t = i.children)
          for (n = t.length - 1; n >= 0; --n) o.push(t[n]);
      return this;
    }

    function m(e) {
      for (var t, n, i, o = this, r = [o], a = []; o = r.pop();)
        if (a.push(o), t = o.children)
          for (n = 0, i = t.length; n < i; ++n) r.push(t[n]);
      for (; o = a.pop();) e(o);
      return this;
    }

    function g(e) {
      return this.eachAfter(function(t) {
        for (var n = +e(t.data) || 0, i = t.children, o = i && i.length; --o >= 0;) n += i[o].value;
        t.value = n;
      });
    }

    function p(e) {
      return this.eachBefore(function(t) {
        t.children && t.children.sort(e);
      });
    }

    function h(e) {
      for (var t = this, n = b(t, e), i = [t]; t !== n;) t = t.parent, i.push(t);
      for (var o = i.length; e !== n;) i.splice(o, 0, e), e = e.parent;
      return i;
    }

    function b(e, t) {
      if (e === t) return e;
      var n = e.ancestors(),
        i = t.ancestors(),
        o = null;
      for (e = n.pop(), t = i.pop(); e === t;) o = e, e = n.pop(), t = i.pop();
      return o;
    }

    function x() {
      for (var e = this, t = [e]; e = e.parent;) t.push(e);
      return t;
    }

    function v() {
      var e = [];
      return this.each(function(t) {
        e.push(t);
      }), e;
    }

    function y() {
      var e = [];
      return this.eachBefore(function(t) {
        t.children || e.push(t);
      }), e;
    }

    function w() {
      var e = this,
        t = [];
      return e.each(function(n) {
        n !== e && t.push({
          source: n.parent,
          target: n
        });
      }), t;
    }

    function S(e, t) {
      var n,
        i,
        o,
        r,
        a,
        l = new C(e),
        s = +e.value && (l.value = e.value),
        d = [l];
      for (null == t && (t = k); n = d.pop();)
        if (s && (n.value = +n.data.value), (o = t(n.data)) && (a = o.length))
          for (n.children = new Array(a), r = a - 1; r >= 0; --r) d.push(i = n.children[r] = new C(o[r])), i
            .parent = n, i.depth = n.depth + 1;
      return l.eachBefore(T);
    }

    function E() {
      return S(this).eachBefore(_);
    }

    function k(e) {
      return e.children;
    }

    function _(e) {
      e.data = e.data.data;
    }

    function T(e) {
      var t = 0;
      do e.height = t; while ((e = e.parent) && e.height < ++t);
    }

    function C(e) {
      this.data = e, this.depth = this.height = 0, this.parent = null;
    }

    function O(e) {
      for (var t, n, i = e.length; i;) n = Math.random() * i-- | 0, t = e[i], e[i] = e[n], e[n] = t;
      return e;
    }

    function A(e) {
      for (var t, n, i = 0, o = (e = O(ve.call(e))).length, r = []; i < o;) t = e[i], n && R(n, t) ? ++i : (
        n = D(r = I(r, t)), i = 0);
      return n;
    }

    function I(e, t) {
      var n, i;
      if (P(t, e)) return [t];
      for (n = 0; n < e.length; ++n)
        if (M(t, e[n]) && P(L(e[n], t), e)) return [e[n], t];
      for (n = 0; n < e.length - 1; ++n)
        for (i = n + 1; i < e.length; ++i)
          if (M(L(e[n], e[i]), t) && M(L(e[n], t), e[i]) && M(L(e[i], t), e[n]) && P(F(e[n], e[i], t), e))
            return [e[n], e[i], t];
      throw new Error();
    }

    function M(e, t) {
      var n = e.r - t.r,
        i = t.x - e.x,
        o = t.y - e.y;
      return n < 0 || n * n < i * i + o * o;
    }

    function R(e, t) {
      var n = e.r - t.r + 1e-6,
        i = t.x - e.x,
        o = t.y - e.y;
      return n > 0 && n * n > i * i + o * o;
    }

    function P(e, t) {
      for (var n = 0; n < t.length; ++n)
        if (!R(e, t[n])) return !1;
      return !0;
    }

    function D(e) {
      switch (e.length) {
        case 1:
          return N(e[0]);
        case 2:
          return L(e[0], e[1]);
        case 3:
          return F(e[0], e[1], e[2]);
      }
    }

    function N(e) {
      return {
        x: e.x,
        y: e.y,
        r: e.r
      };
    }

    function L(e, t) {
      var n = e.x,
        i = e.y,
        o = e.r,
        r = t.x,
        a = t.y,
        l = t.r,
        s = r - n,
        d = a - i,
        c = l - o,
        u = Math.sqrt(s * s + d * d);
      return {
        x: (n + r + s / u * c) / 2,
        y: (i + a + d / u * c) / 2,
        r: (u + o + l) / 2
      };
    }

    function F(e, t, n) {
      var i = e.x,
        o = e.y,
        r = e.r,
        a = t.x,
        l = t.y,
        s = t.r,
        d = n.x,
        c = n.y,
        u = n.r,
        f = i - a,
        m = i - d,
        g = o - l,
        p = o - c,
        h = s - r,
        b = u - r,
        x = i * i + o * o - r * r,
        v = x - a * a - l * l + s * s,
        y = x - d * d - c * c + u * u,
        w = m * g - f * p,
        S = (g * y - p * v) / (2 * w) - i,
        E = (p * h - g * b) / w,
        k = (m * v - f * y) / (2 * w) - o,
        _ = (f * b - m * h) / w,
        T = E * E + _ * _ - 1,
        C = 2 * (r + S * E + k * _),
        O = S * S + k * k - r * r,
        A = -(T ? (C + Math.sqrt(C * C - 4 * T * O)) / (2 * T) : O / C);
      return {
        x: i + S + E * A,
        y: o + k + _ * A,
        r: A
      };
    }

    function U(e, t, n) {
      var i,
        o,
        r,
        a,
        l = e.x - t.x,
        s = e.y - t.y,
        d = l * l + s * s;
      d ? (o = t.r + n.r, o *= o, a = e.r + n.r, a *= a, o > a ? (i = (d + a - o) / (2 * d), r = Math.sqrt(
        Math.max(0, a / d - i * i)), n.x = e.x - i * l - r * s, n.y = e.y - i * s + r * l) : (i = (d +
          o - a) / (2 * d), r = Math.sqrt(Math.max(0, o / d - i * i)), n.x = t.x + i * l - r * s, n.y =
        t.y + i * s + r * l)) : (n.x = t.x + n.r, n.y = t.y);
    }

    function z(e, t) {
      var n = e.r + t.r - 1e-6,
        i = t.x - e.x,
        o = t.y - e.y;
      return n > 0 && n * n > i * i + o * o;
    }

    function G(e) {
      var t = e._,
        n = e.next._,
        i = t.r + n.r,
        o = (t.x * n.r + n.x * t.r) / i,
        r = (t.y * n.r + n.y * t.r) / i;
      return o * o + r * r;
    }

    function V(e) {
      this._ = e, this.next = null, this.previous = null;
    }

    function H(e) {
      if (!(o = e.length)) return 0;
      var t, n, i, o, r, a, l, s, d, c, u;
      if (t = e[0], t.x = 0, t.y = 0, !(o > 1)) return t.r;
      if (n = e[1], t.x = -n.r, n.x = t.r, n.y = 0, !(o > 2)) return t.r + n.r;
      U(n, t, i = e[2]), t = new V(t), n = new V(n), i = new V(i), t.next = i.previous = n, n.next = t
        .previous = i, i.next = n.previous = t;
      e: for (l = 3; l < o; ++l) {
        U(t._, n._, i = e[l]), i = new V(i), s = n.next, d = t.previous, c = n._.r, u = t._.r;
        do
          if (c <= u) {
            if (z(s._, i._)) {
              n = s, t.next = n, n.previous = t, --l;
              continue e;
            }
            c += s._.r, s = s.next;
          } else {
            if (z(d._, i._)) {
              t = d, t.next = n, n.previous = t, --l;
              continue e;
            }
            u += d._.r, d = d.previous;
          } while (s !== d.next);
        for (i.previous = t, i.next = n, t.next = n.previous = n = i, r = G(t);
          (i = i.next) !== n;)(a = G(i)) < r && (t = i, r = a);
        n = t.next;
      }
      for (t = [n._], i = n;
        (i = i.next) !== n;) t.push(i._);
      for (i = A(t), l = 0; l < o; ++l) t = e[l], t.x -= i.x, t.y -= i.y;
      return i.r;
    }

    function B(e) {
      return H(e), e;
    }

    function Y(e) {
      return null == e ? null : $(e);
    }

    function $(e) {
      if ("function" != typeof e) throw new Error();
      return e;
    }

    function W() {
      return 0;
    }

    function j(e) {
      return function() {
        return e;
      };
    }

    function K(e) {
      return Math.sqrt(e.value);
    }

    function q() {
      function e(e) {
        return e.x = n / 2, e.y = i / 2, t ? e.eachBefore(X(t)).eachAfter(Z(o, .5)).eachBefore(Q(1)) : e
          .eachBefore(X(K)).eachAfter(Z(W, 1)).eachAfter(Z(o, e.r / Math.min(n, i))).eachBefore(Q(Math.min(
            n, i) / (2 * e.r))), e;
      }
      var t = null,
        n = 1,
        i = 1,
        o = W;
      return e.radius = function(n) {
        return arguments.length ? (t = Y(n), e) : t;
      }, e.size = function(t) {
        return arguments.length ? (n = +t[0], i = +t[1], e) : [n, i];
      }, e.padding = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : j(+t), e) : o;
      }, e;
    }

    function X(e) {
      return function(t) {
        t.children || (t.r = Math.max(0, +e(t) || 0));
      };
    }

    function Z(e, t) {
      return function(n) {
        if (i = n.children) {
          var i,
            o,
            r,
            a = i.length,
            l = e(n) * t || 0;
          if (l)
            for (o = 0; o < a; ++o) i[o].r += l;
          if (r = H(i), l)
            for (o = 0; o < a; ++o) i[o].r -= l;
          n.r = r + l;
        }
      };
    }

    function Q(e) {
      return function(t) {
        var n = t.parent;
        t.r *= e, n && (t.x = n.x + e * t.x, t.y = n.y + e * t.y);
      };
    }

    function J(e) {
      e.x0 = Math.round(e.x0), e.y0 = Math.round(e.y0), e.x1 = Math.round(e.x1), e.y1 = Math.round(e.y1);
    }

    function ee(e, t, n, i, o) {
      for (var r, a = e.children, l = -1, s = a.length, d = e.value && (i - t) / e.value; ++l < s;) r = a[
        l], r.y0 = n, r.y1 = o, r.x0 = t, r.x1 = t += r.value * d;
    }

    function te() {
      function e(e) {
        var a = e.height + 1;
        return e.x0 = e.y0 = o, e.x1 = n, e.y1 = i / a, e.eachBefore(t(i, a)), r && e.eachBefore(J), e;
      }

      function t(e, t) {
        return function(n) {
          n.children && ee(n, n.x0, e * (n.depth + 1) / t, n.x1, e * (n.depth + 2) / t);
          var i = n.x0,
            r = n.y0,
            a = n.x1 - o,
            l = n.y1 - o;
          a < i && (i = a = (i + a) / 2), l < r && (r = l = (r + l) / 2), n.x0 = i, n.y0 = r, n.x1 = a, n
            .y1 = l;
        };
      }
      var n = 1,
        i = 1,
        o = 0,
        r = !1;
      return e.round = function(t) {
        return arguments.length ? (r = !!t, e) : r;
      }, e.size = function(t) {
        return arguments.length ? (n = +t[0], i = +t[1], e) : [n, i];
      }, e.padding = function(t) {
        return arguments.length ? (o = +t, e) : o;
      }, e;
    }

    function ne(e) {
      return e.id;
    }

    function ie(e) {
      return e.parentId;
    }

    function oe() {
      function e(e) {
        var i,
          o,
          r,
          a,
          l,
          s,
          d,
          c = e.length,
          u = new Array(c),
          f = {};
        for (o = 0; o < c; ++o) i = e[o], l = u[o] = new C(i), null != (s = t(i, o, e)) && (s += "") && (d =
          ye + (l.id = s), f[d] = d in f ? Se : l);
        for (o = 0; o < c; ++o)
          if (l = u[o], s = n(e[o], o, e), null != s && (s += "")) {
            if (a = f[ye + s], !a) throw new Error("missing: " + s);
            if (a === Se) throw new Error("ambiguous: " + s);
            a.children ? a.children.push(l) : a.children = [l], l.parent = a;
          } else {
            if (r) throw new Error("multiple roots");
            r = l;
          }
        if (!r) throw new Error("no root");
        if (r.parent = we, r.eachBefore(function(e) {
            e.depth = e.parent.depth + 1, --c;
          }).eachBefore(T), r.parent = null, c > 0) throw new Error("cycle");
        return r;
      }
      var t = ne,
        n = ie;
      return e.id = function(n) {
        return arguments.length ? (t = $(n), e) : t;
      }, e.parentId = function(t) {
        return arguments.length ? (n = $(t), e) : n;
      }, e;
    }

    function re(e, t) {
      return e.parent === t.parent ? 1 : 2;
    }

    function ae(e) {
      var t = e.children;
      return t ? t[0] : e.t;
    }

    function le(e) {
      var t = e.children;
      return t ? t[t.length - 1] : e.t;
    }

    function se(e, t, n) {
      var i = n / (t.i - e.i);
      t.c -= i, t.s += n, e.c += i, t.z += n, t.m += n;
    }

    function de(e) {
      for (var t, n = 0, i = 0, o = e.children, r = o.length; --r >= 0;) t = o[r], t.z += n, t.m += n, n +=
        t.s + (i += t.c);
    }

    function ce(e, t, n) {
      return e.a.parent === t.parent ? e.a : n;
    }

    function ue(e, t) {
      this._ = e, this.parent = null, this.children = null, this.A = null, this.a = this, this.z = 0, this
        .m = 0, this.c = 0, this.s = 0, this.t = null, this.i = t;
    }

    function fe(e) {
      for (var t, n, i, o, r, a = new ue(e, 0), l = [a]; t = l.pop();)
        if (i = t._.children)
          for (t.children = new Array(r = i.length), o = r - 1; o >= 0; --o) l.push(n = t.children[o] =
            new ue(i[o], o)), n.parent = t;
      return (a.parent = new ue(null, 0)).children = [a], a;
    }

    function me() {
      function e(e) {
        var i = fe(e);
        if (i.eachAfter(t), i.parent.m = -i.z, i.eachBefore(n), s) e.eachBefore(o);
        else {
          var d = e,
            c = e,
            u = e;
          e.eachBefore(function(e) {
            e.x < d.x && (d = e), e.x > c.x && (c = e), e.depth > u.depth && (u = e);
          });
          var f = d === c ? 1 : r(d, c) / 2,
            m = f - d.x,
            g = a / (c.x + f + m),
            p = l / (u.depth || 1);
          e.eachBefore(function(e) {
            e.x = (e.x + m) * g, e.y = e.depth * p;
          });
        }
        return e;
      }

      function t(e) {
        var t = e.children,
          n = e.parent.children,
          o = e.i ? n[e.i - 1] : null;
        if (t) {
          de(e);
          var a = (t[0].z + t[t.length - 1].z) / 2;
          o ? (e.z = o.z + r(e._, o._), e.m = e.z - a) : e.z = a;
        } else o && (e.z = o.z + r(e._, o._));
        e.parent.A = i(e, o, e.parent.A || n[0]);
      }

      function n(e) {
        e._.x = e.z + e.parent.m, e.m += e.parent.m;
      }

      function i(e, t, n) {
        if (t) {
          for (var i, o = e, a = e, l = t, s = o.parent.children[0], d = o.m, c = a.m, u = l.m, f = s.m; l =
            le(l), o = ae(o), l && o;) s = ae(s), a = le(a), a.a = e, i = l.z + u - o.z - d + r(l._, o._),
            i > 0 && (se(ce(l, e, n), e, i), d += i, c += i), u += l.m, d += o.m, f += s.m, c += a.m;
          l && !le(a) && (a.t = l, a.m += u - c), o && !ae(s) && (s.t = o, s.m += d - f, n = e);
        }
        return n;
      }

      function o(e) {
        e.x *= a, e.y = e.depth * l;
      }
      var r = re,
        a = 1,
        l = 1,
        s = null;
      return e.separation = function(t) {
        return arguments.length ? (r = t, e) : r;
      }, e.size = function(t) {
        return arguments.length ? (s = !1, a = +t[0], l = +t[1], e) : s ? null : [a, l];
      }, e.nodeSize = function(t) {
        return arguments.length ? (s = !0, a = +t[0], l = +t[1], e) : s ? [a, l] : null;
      }, e;
    }

    function ge(e, t, n, i, o) {
      for (var r, a = e.children, l = -1, s = a.length, d = e.value && (o - n) / e.value; ++l < s;) r = a[
        l], r.x0 = t, r.x1 = i, r.y0 = n, r.y1 = n += r.value * d;
    }

    function pe(e, t, n, i, o, r) {
      for (var a, l, s, d, c, u, f, m, g, p, h, b = [], x = t.children, v = 0, y = 0, w = x.length, S = t
          .value; v < w;) {
        s = o - n, d = r - i;
        do c = x[y++].value; while (!c && y < w);
        for (u = f = c, p = Math.max(d / s, s / d) / (S * e), h = c * c * p, g = Math.max(f / h, h / u); y <
          w; ++y) {
          if (c += l = x[y].value, l < u && (u = l), l > f && (f = l), h = c * c * p, m = Math.max(f / h,
              h / u), m > g) {
            c -= l;
            break;
          }
          g = m;
        }
        b.push(a = {
            value: c,
            dice: s < d,
            children: x.slice(v, y)
          }), a.dice ? ee(a, n, i, o, S ? i += d * c / S : r) : ge(a, n, i, S ? n += s * c / S : o, r), S -=
          c, v = y;
      }
      return b;
    }

    function he() {
      function e(e) {
        return e.x0 = e.y0 = 0, e.x1 = o, e.y1 = r, e.eachBefore(t), a = [0], i && e.eachBefore(J), e;
      }

      function t(e) {
        var t = a[e.depth],
          i = e.x0 + t,
          o = e.y0 + t,
          r = e.x1 - t,
          f = e.y1 - t;
        r < i && (i = r = (i + r) / 2), f < o && (o = f = (o + f) / 2), e.x0 = i, e.y0 = o, e.x1 = r, e.y1 =
          f, e.children && (t = a[e.depth + 1] = l(e) / 2, i += u(e) - t, o += s(e) - t, r -= d(e) - t, f -=
            c(e) - t, r < i && (i = r = (i + r) / 2), f < o && (o = f = (o + f) / 2), n(e, i, o, r, f));
      }
      var n = ke,
        i = !1,
        o = 1,
        r = 1,
        a = [0],
        l = W,
        s = W,
        d = W,
        c = W,
        u = W;
      return e.round = function(t) {
        return arguments.length ? (i = !!t, e) : i;
      }, e.size = function(t) {
        return arguments.length ? (o = +t[0], r = +t[1], e) : [o, r];
      }, e.tile = function(t) {
        return arguments.length ? (n = $(t), e) : n;
      }, e.padding = function(t) {
        return arguments.length ? e.paddingInner(t).paddingOuter(t) : e.paddingInner();
      }, e.paddingInner = function(t) {
        return arguments.length ? (l = "function" == typeof t ? t : j(+t), e) : l;
      }, e.paddingOuter = function(t) {
        return arguments.length ? e.paddingTop(t).paddingRight(t).paddingBottom(t).paddingLeft(t) : e
          .paddingTop();
      }, e.paddingTop = function(t) {
        return arguments.length ? (s = "function" == typeof t ? t : j(+t), e) : s;
      }, e.paddingRight = function(t) {
        return arguments.length ? (d = "function" == typeof t ? t : j(+t), e) : d;
      }, e.paddingBottom = function(t) {
        return arguments.length ? (c = "function" == typeof t ? t : j(+t), e) : c;
      }, e.paddingLeft = function(t) {
        return arguments.length ? (u = "function" == typeof t ? t : j(+t), e) : u;
      }, e;
    }

    function be(e, t, n, i, o) {
      function r(e, t, n, i, o, a, l) {
        if (e >= t - 1) {
          var d = s[e];
          return d.x0 = i, d.y0 = o, d.x1 = a, d.y1 = l, void 0;
        }
        for (var u = c[e], f = n / 2 + u, m = e + 1, g = t - 1; m < g;) {
          var p = m + g >>> 1;
          c[p] < f ? m = p + 1 : g = p;
        }
        f - c[m - 1] < c[m] - f && e + 1 < m && --m;
        var h = c[m] - u,
          b = n - h;
        if (a - i > l - o) {
          var x = (i * b + a * h) / n;
          r(e, m, h, i, o, x, l), r(m, t, b, x, o, a, l);
        } else {
          var v = (o * b + l * h) / n;
          r(e, m, h, i, o, a, v), r(m, t, b, i, v, a, l);
        }
      }
      var a,
        l,
        s = e.children,
        d = s.length,
        c = new Array(d + 1);
      for (c[0] = l = a = 0; a < d; ++a) c[a + 1] = l += s[a].value;
      r(0, d, e.value, t, n, i, o);
    }

    function xe(e, t, n, i, o) {
      (1 & e.depth ? ge : ee)(e, t, n, i, o);
    }
    C.prototype = S.prototype = {
      constructor: C,
      count: c,
      each: u,
      eachAfter: m,
      eachBefore: f,
      sum: g,
      sort: p,
      path: h,
      ancestors: x,
      descendants: v,
      leaves: y,
      links: w,
      copy: E
    };
    var ve = Array.prototype.slice,
      ye = "$",
      we = {
        depth: -1
      },
      Se = {};
    ue.prototype = Object.create(C.prototype);
    var Ee = (1 + Math.sqrt(5)) / 2,
      ke = function e(t) {
        function n(e, n, i, o, r) {
          pe(t, e, n, i, o, r);
        }
        return n.ratio = function(t) {
          return e((t = +t) > 1 ? t : 1);
        }, n;
      }(Ee),
      _e = function e(t) {
        function n(e, n, i, o, r) {
          if ((a = e._squarify) && a.ratio === t)
            for (var a, l, s, d, c, u = -1, f = a.length, m = e.value; ++u < f;) {
              for (l = a[u], s = l.children, d = l.value = 0, c = s.length; d < c; ++d) l.value += s[d]
                .value;
              l.dice ? ee(l, n, i, o, i += (r - i) * l.value / m) : ge(l, n, i, n += (o - n) * l.value / m,
                r), m -= l.value;
            } else e._squarify = a = pe(t, e, n, i, o, r), a.ratio = t;
        }
        return n.ratio = function(t) {
          return e((t = +t) > 1 ? t : 1);
        }, n;
      }(Ee);
    e.cluster = s, e.hierarchy = S, e.pack = q, e.packEnclose = A, e.packSiblings = B, e.partition = te, e
      .stratify = oe, e.tree = me, e.treemap = he, e.treemapBinary = be, e.treemapDice = ee, e
      .treemapResquarify = _e, e.treemapSlice = ge, e.treemapSliceDice = xe, e.treemapSquarify = ke, Object
      .defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
