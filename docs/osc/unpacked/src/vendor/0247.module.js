// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 247
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
      return e.reduce(r, 0) / e.length;
    }

    function r(e, t) {
      return e + t.x;
    }

    function i(e) {
      return 1 + e.reduce(o, 0);
    }

    function o(e, t) {
      return Math.max(e, t.y);
    }

    function a(e) {
      for (var t; t = e.children;) e = t[0];
      return e;
    }

    function s(e) {
      for (var t; t = e.children;) e = t[t.length - 1];
      return e;
    }

    function c() {
      function e(e) {
        var t,
          l = 0;
        e.eachAfter(function(e) {
          var o = e.children;
          o ? (e.x = n(o), e.y = i(o)) : (e.x = t ? l += r(e, t) : 0, e.y = 0, t = e);
        });
        var d = a(e),
          f = s(e),
          h = d.x - r(d, f) / 2,
          p = f.x + r(f, d) / 2;
        return e.eachAfter(u ? function(t) {
          t.x = (t.x - e.x) * o, t.y = (e.y - t.y) * c;
        } : function(t) {
          t.x = (t.x - h) / (p - h) * o, t.y = (1 - (e.y ? t.y / e.y : 1)) * c;
        });
      }
      var r = t,
        o = 1,
        c = 1,
        u = !1;
      return e.separation = function(t) {
        return arguments.length ? (r = t, e) : r;
      }, e.size = function(t) {
        return arguments.length ? (u = !1, o = +t[0], c = +t[1], e) : u ? null : [o, c];
      }, e.nodeSize = function(t) {
        return arguments.length ? (u = !0, o = +t[0], c = +t[1], e) : u ? [o, c] : null;
      }, e;
    }

    function u(e) {
      var t = 0,
        n = e.children,
        r = n && n.length;
      if (r)
        for (; --r >= 0;) t += n[r].value;
      else t = 1;
      e.value = t;
    }

    function l() {
      return this.eachAfter(u);
    }

    function d(e) {
      var t,
        n,
        r,
        i,
        o = this,
        a = [o];
      do
        for (t = a.reverse(), a = []; o = t.pop();)
          if (e(o), n = o.children)
            for (r = 0, i = n.length; r < i; ++r) a.push(n[r]); while (a.length);
      return this;
    }

    function f(e) {
      for (var t, n, r = this, i = [r]; r = i.pop();)
        if (e(r), t = r.children)
          for (n = t.length - 1; n >= 0; --n) i.push(t[n]);
      return this;
    }

    function h(e) {
      for (var t, n, r, i = this, o = [i], a = []; i = o.pop();)
        if (a.push(i), t = i.children)
          for (n = 0, r = t.length; n < r; ++n) o.push(t[n]);
      for (; i = a.pop();) e(i);
      return this;
    }

    function p(e) {
      return this.eachAfter(function(t) {
        for (var n = +e(t.data) || 0, r = t.children, i = r && r.length; --i >= 0;) n += r[i].value;
        t.value = n;
      });
    }

    function m(e) {
      return this.eachBefore(function(t) {
        t.children && t.children.sort(e);
      });
    }

    function v(e) {
      for (var t = this, n = g(t, e), r = [t]; t !== n;) t = t.parent, r.push(t);
      for (var i = r.length; e !== n;) r.splice(i, 0, e), e = e.parent;
      return r;
    }

    function g(e, t) {
      if (e === t) return e;
      var n = e.ancestors(),
        r = t.ancestors(),
        i = null;
      for (e = n.pop(), t = r.pop(); e === t;) i = e, e = n.pop(), t = r.pop();
      return i;
    }

    function y() {
      for (var e = this, t = [e]; e = e.parent;) t.push(e);
      return t;
    }

    function b() {
      var e = [];
      return this.each(function(t) {
        e.push(t);
      }), e;
    }

    function E() {
      var e = [];
      return this.eachBefore(function(t) {
        t.children || e.push(t);
      }), e;
    }

    function _() {
      var e = this,
        t = [];
      return e.each(function(n) {
        n !== e && t.push({
          source: n.parent,
          target: n
        });
      }), t;
    }

    function $(e, t) {
      var n,
        r,
        i,
        o,
        a,
        s = new S(e),
        c = +e.value && (s.value = e.value),
        u = [s];
      for (null == t && (t = T); n = u.pop();)
        if (c && (n.value = +n.data.value), (i = t(n.data)) && (a = i.length))
          for (n.children = new Array(a), o = a - 1; o >= 0; --o) u.push(r = n.children[o] = new S(i[o])), r
            .parent = n, r.depth = n.depth + 1;
      return s.eachBefore(x);
    }

    function w() {
      return $(this).eachBefore(C);
    }

    function T(e) {
      return e.children;
    }

    function C(e) {
      e.data = e.data.data;
    }

    function x(e) {
      var t = 0;
      do e.height = t; while ((e = e.parent) && e.height < ++t);
    }

    function S(e) {
      this.data = e, this.depth = this.height = 0, this.parent = null;
    }

    function A(e) {
      for (var t, n, r = e.length; r;) n = Math.random() * r-- | 0, t = e[r], e[r] = e[n], e[n] = t;
      return e;
    }

    function M(e) {
      for (var t, n, r = 0, i = (e = A(be.call(e))).length, o = []; r < i;) t = e[r], n && I(n, t) ? ++r : (
        n = D(o = k(o, t)), r = 0);
      return n;
    }

    function k(e, t) {
      var n, r;
      if (O(t, e)) return [t];
      for (n = 0; n < e.length; ++n)
        if (N(t, e[n]) && O(P(e[n], t), e)) return [e[n], t];
      for (n = 0; n < e.length - 1; ++n)
        for (r = n + 1; r < e.length; ++r)
          if (N(P(e[n], e[r]), t) && N(P(e[n], t), e[r]) && N(P(e[r], t), e[n]) && O(L(e[n], e[r], t), e))
            return [e[n], e[r], t];
      throw new Error();
    }

    function N(e, t) {
      var n = e.r - t.r,
        r = t.x - e.x,
        i = t.y - e.y;
      return n < 0 || n * n < r * r + i * i;
    }

    function I(e, t) {
      var n = e.r - t.r + 1e-6,
        r = t.x - e.x,
        i = t.y - e.y;
      return n > 0 && n * n > r * r + i * i;
    }

    function O(e, t) {
      for (var n = 0; n < t.length; ++n)
        if (!I(e, t[n])) return !1;
      return !0;
    }

    function D(e) {
      switch (e.length) {
        case 1:
          return R(e[0]);
        case 2:
          return P(e[0], e[1]);
        case 3:
          return L(e[0], e[1], e[2]);
      }
    }

    function R(e) {
      return {
        x: e.x,
        y: e.y,
        r: e.r
      };
    }

    function P(e, t) {
      var n = e.x,
        r = e.y,
        i = e.r,
        o = t.x,
        a = t.y,
        s = t.r,
        c = o - n,
        u = a - r,
        l = s - i,
        d = Math.sqrt(c * c + u * u);
      return {
        x: (n + o + c / d * l) / 2,
        y: (r + a + u / d * l) / 2,
        r: (d + i + s) / 2
      };
    }

    function L(e, t, n) {
      var r = e.x,
        i = e.y,
        o = e.r,
        a = t.x,
        s = t.y,
        c = t.r,
        u = n.x,
        l = n.y,
        d = n.r,
        f = r - a,
        h = r - u,
        p = i - s,
        m = i - l,
        v = c - o,
        g = d - o,
        y = r * r + i * i - o * o,
        b = y - a * a - s * s + c * c,
        E = y - u * u - l * l + d * d,
        _ = h * p - f * m,
        $ = (p * E - m * b) / (2 * _) - r,
        w = (m * v - p * g) / _,
        T = (h * b - f * E) / (2 * _) - i,
        C = (f * g - h * v) / _,
        x = w * w + C * C - 1,
        S = 2 * (o + $ * w + T * C),
        A = $ * $ + T * T - o * o,
        M = -(x ? (S + Math.sqrt(S * S - 4 * x * A)) / (2 * x) : A / S);
      return {
        x: r + $ + w * M,
        y: i + T + C * M,
        r: M
      };
    }

    function U(e, t, n) {
      var r,
        i,
        o,
        a,
        s = e.x - t.x,
        c = e.y - t.y,
        u = s * s + c * c;
      u ? (i = t.r + n.r, i *= i, a = e.r + n.r, a *= a, i > a ? (r = (u + a - i) / (2 * u), o = Math.sqrt(
        Math.max(0, a / u - r * r)), n.x = e.x - r * s - o * c, n.y = e.y - r * c + o * s) : (r = (u +
          i - a) / (2 * u), o = Math.sqrt(Math.max(0, i / u - r * r)), n.x = t.x + r * s - o * c, n.y =
        t.y + r * c + o * s)) : (n.x = t.x + n.r, n.y = t.y);
    }

    function F(e, t) {
      var n = e.r + t.r - 1e-6,
        r = t.x - e.x,
        i = t.y - e.y;
      return n > 0 && n * n > r * r + i * i;
    }

    function j(e) {
      var t = e._,
        n = e.next._,
        r = t.r + n.r,
        i = (t.x * n.r + n.x * t.r) / r,
        o = (t.y * n.r + n.y * t.r) / r;
      return i * i + o * o;
    }

    function H(e) {
      this._ = e, this.next = null, this.previous = null;
    }

    function B(e) {
      if (!(i = e.length)) return 0;
      var t, n, r, i, o, a, s, c, u, l, d;
      if (t = e[0], t.x = 0, t.y = 0, !(i > 1)) return t.r;
      if (n = e[1], t.x = -n.r, n.x = t.r, n.y = 0, !(i > 2)) return t.r + n.r;
      U(n, t, r = e[2]), t = new H(t), n = new H(n), r = new H(r), t.next = r.previous = n, n.next = t
        .previous = r, r.next = n.previous = t;
      e: for (s = 3; s < i; ++s) {
        U(t._, n._, r = e[s]), r = new H(r), c = n.next, u = t.previous, l = n._.r, d = t._.r;
        do
          if (l <= d) {
            if (F(c._, r._)) {
              n = c, t.next = n, n.previous = t, --s;
              continue e;
            }
            l += c._.r, c = c.next;
          } else {
            if (F(u._, r._)) {
              t = u, t.next = n, n.previous = t, --s;
              continue e;
            }
            d += u._.r, u = u.previous;
          } while (c !== u.next);
        for (r.previous = t, r.next = n, t.next = n.previous = n = r, o = j(t);
          (r = r.next) !== n;)(a = j(r)) < o && (t = r, o = a);
        n = t.next;
      }
      for (t = [n._], r = n;
        (r = r.next) !== n;) t.push(r._);
      for (r = M(t), s = 0; s < i; ++s) t = e[s], t.x -= r.x, t.y -= r.y;
      return r.r;
    }

    function z(e) {
      return B(e), e;
    }

    function q(e) {
      return null == e ? null : G(e);
    }

    function G(e) {
      if ("function" != typeof e) throw new Error();
      return e;
    }

    function V() {
      return 0;
    }

    function W(e) {
      return function() {
        return e;
      };
    }

    function Y(e) {
      return Math.sqrt(e.value);
    }

    function K() {
      function e(e) {
        return e.x = n / 2, e.y = r / 2, t ? e.eachBefore(X(t)).eachAfter(Q(i, .5)).eachBefore(J(1)) : e
          .eachBefore(X(Y)).eachAfter(Q(V, 1)).eachAfter(Q(i, e.r / Math.min(n, r))).eachBefore(J(Math.min(
            n, r) / (2 * e.r))), e;
      }
      var t = null,
        n = 1,
        r = 1,
        i = V;
      return e.radius = function(n) {
        return arguments.length ? (t = q(n), e) : t;
      }, e.size = function(t) {
        return arguments.length ? (n = +t[0], r = +t[1], e) : [n, r];
      }, e.padding = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : W(+t), e) : i;
      }, e;
    }

    function X(e) {
      return function(t) {
        t.children || (t.r = Math.max(0, +e(t) || 0));
      };
    }

    function Q(e, t) {
      return function(n) {
        if (r = n.children) {
          var r,
            i,
            o,
            a = r.length,
            s = e(n) * t || 0;
          if (s)
            for (i = 0; i < a; ++i) r[i].r += s;
          if (o = B(r), s)
            for (i = 0; i < a; ++i) r[i].r -= s;
          n.r = o + s;
        }
      };
    }

    function J(e) {
      return function(t) {
        var n = t.parent;
        t.r *= e, n && (t.x = n.x + e * t.x, t.y = n.y + e * t.y);
      };
    }

    function Z(e) {
      e.x0 = Math.round(e.x0), e.y0 = Math.round(e.y0), e.x1 = Math.round(e.x1), e.y1 = Math.round(e.y1);
    }

    function ee(e, t, n, r, i) {
      for (var o, a = e.children, s = -1, c = a.length, u = e.value && (r - t) / e.value; ++s < c;) o = a[
        s], o.y0 = n, o.y1 = i, o.x0 = t, o.x1 = t += o.value * u;
    }

    function te() {
      function e(e) {
        var a = e.height + 1;
        return e.x0 = e.y0 = i, e.x1 = n, e.y1 = r / a, e.eachBefore(t(r, a)), o && e.eachBefore(Z), e;
      }

      function t(e, t) {
        return function(n) {
          n.children && ee(n, n.x0, e * (n.depth + 1) / t, n.x1, e * (n.depth + 2) / t);
          var r = n.x0,
            o = n.y0,
            a = n.x1 - i,
            s = n.y1 - i;
          a < r && (r = a = (r + a) / 2), s < o && (o = s = (o + s) / 2), n.x0 = r, n.y0 = o, n.x1 = a, n
            .y1 = s;
        };
      }
      var n = 1,
        r = 1,
        i = 0,
        o = !1;
      return e.round = function(t) {
        return arguments.length ? (o = !!t, e) : o;
      }, e.size = function(t) {
        return arguments.length ? (n = +t[0], r = +t[1], e) : [n, r];
      }, e.padding = function(t) {
        return arguments.length ? (i = +t, e) : i;
      }, e;
    }

    function ne(e) {
      return e.id;
    }

    function re(e) {
      return e.parentId;
    }

    function ie() {
      function e(e) {
        var r,
          i,
          o,
          a,
          s,
          c,
          u,
          l = e.length,
          d = new Array(l),
          f = {};
        for (i = 0; i < l; ++i) r = e[i], s = d[i] = new S(r), null != (c = t(r, i, e)) && (c += "") && (u =
          Ee + (s.id = c), f[u] = u in f ? $e : s);
        for (i = 0; i < l; ++i)
          if (s = d[i], c = n(e[i], i, e), null != c && (c += "")) {
            if (a = f[Ee + c], !a) throw new Error("missing: " + c);
            if (a === $e) throw new Error("ambiguous: " + c);
            a.children ? a.children.push(s) : a.children = [s], s.parent = a;
          } else {
            if (o) throw new Error("multiple roots");
            o = s;
          }
        if (!o) throw new Error("no root");
        if (o.parent = _e, o.eachBefore(function(e) {
            e.depth = e.parent.depth + 1, --l;
          }).eachBefore(x), o.parent = null, l > 0) throw new Error("cycle");
        return o;
      }
      var t = ne,
        n = re;
      return e.id = function(n) {
        return arguments.length ? (t = G(n), e) : t;
      }, e.parentId = function(t) {
        return arguments.length ? (n = G(t), e) : n;
      }, e;
    }

    function oe(e, t) {
      return e.parent === t.parent ? 1 : 2;
    }

    function ae(e) {
      var t = e.children;
      return t ? t[0] : e.t;
    }

    function se(e) {
      var t = e.children;
      return t ? t[t.length - 1] : e.t;
    }

    function ce(e, t, n) {
      var r = n / (t.i - e.i);
      t.c -= r, t.s += n, e.c += r, t.z += n, t.m += n;
    }

    function ue(e) {
      for (var t, n = 0, r = 0, i = e.children, o = i.length; --o >= 0;) t = i[o], t.z += n, t.m += n, n +=
        t.s + (r += t.c);
    }

    function le(e, t, n) {
      return e.a.parent === t.parent ? e.a : n;
    }

    function de(e, t) {
      this._ = e, this.parent = null, this.children = null, this.A = null, this.a = this, this.z = 0, this
        .m = 0, this.c = 0, this.s = 0, this.t = null, this.i = t;
    }

    function fe(e) {
      for (var t, n, r, i, o, a = new de(e, 0), s = [a]; t = s.pop();)
        if (r = t._.children)
          for (t.children = new Array(o = r.length), i = o - 1; i >= 0; --i) s.push(n = t.children[i] =
            new de(r[i], i)), n.parent = t;
      return (a.parent = new de(null, 0)).children = [a], a;
    }

    function he() {
      function e(e) {
        var r = fe(e);
        if (r.eachAfter(t), r.parent.m = -r.z, r.eachBefore(n), c) e.eachBefore(i);
        else {
          var u = e,
            l = e,
            d = e;
          e.eachBefore(function(e) {
            e.x < u.x && (u = e), e.x > l.x && (l = e), e.depth > d.depth && (d = e);
          });
          var f = u === l ? 1 : o(u, l) / 2,
            h = f - u.x,
            p = a / (l.x + f + h),
            m = s / (d.depth || 1);
          e.eachBefore(function(e) {
            e.x = (e.x + h) * p, e.y = e.depth * m;
          });
        }
        return e;
      }

      function t(e) {
        var t = e.children,
          n = e.parent.children,
          i = e.i ? n[e.i - 1] : null;
        if (t) {
          ue(e);
          var a = (t[0].z + t[t.length - 1].z) / 2;
          i ? (e.z = i.z + o(e._, i._), e.m = e.z - a) : e.z = a;
        } else i && (e.z = i.z + o(e._, i._));
        e.parent.A = r(e, i, e.parent.A || n[0]);
      }

      function n(e) {
        e._.x = e.z + e.parent.m, e.m += e.parent.m;
      }

      function r(e, t, n) {
        if (t) {
          for (var r, i = e, a = e, s = t, c = i.parent.children[0], u = i.m, l = a.m, d = s.m, f = c.m; s =
            se(s), i = ae(i), s && i;) c = ae(c), a = se(a), a.a = e, r = s.z + d - i.z - u + o(s._, i._),
            r > 0 && (ce(le(s, e, n), e, r), u += r, l += r), d += s.m, u += i.m, f += c.m, l += a.m;
          s && !se(a) && (a.t = s, a.m += d - l), i && !ae(c) && (c.t = i, c.m += u - f, n = e);
        }
        return n;
      }

      function i(e) {
        e.x *= a, e.y = e.depth * s;
      }
      var o = oe,
        a = 1,
        s = 1,
        c = null;
      return e.separation = function(t) {
        return arguments.length ? (o = t, e) : o;
      }, e.size = function(t) {
        return arguments.length ? (c = !1, a = +t[0], s = +t[1], e) : c ? null : [a, s];
      }, e.nodeSize = function(t) {
        return arguments.length ? (c = !0, a = +t[0], s = +t[1], e) : c ? [a, s] : null;
      }, e;
    }

    function pe(e, t, n, r, i) {
      for (var o, a = e.children, s = -1, c = a.length, u = e.value && (i - n) / e.value; ++s < c;) o = a[
        s], o.x0 = t, o.x1 = r, o.y0 = n, o.y1 = n += o.value * u;
    }

    function me(e, t, n, r, i, o) {
      for (var a, s, c, u, l, d, f, h, p, m, v, g = [], y = t.children, b = 0, E = 0, _ = y.length, $ = t
          .value; b < _;) {
        c = i - n, u = o - r;
        do l = y[E++].value; while (!l && E < _);
        for (d = f = l, m = Math.max(u / c, c / u) / ($ * e), v = l * l * m, p = Math.max(f / v, v / d); E <
          _; ++E) {
          if (l += s = y[E].value, s < d && (d = s), s > f && (f = s), v = l * l * m, h = Math.max(f / v,
              v / d), h > p) {
            l -= s;
            break;
          }
          p = h;
        }
        g.push(a = {
            value: l,
            dice: c < u,
            children: y.slice(b, E)
          }), a.dice ? ee(a, n, r, i, $ ? r += u * l / $ : o) : pe(a, n, r, $ ? n += c * l / $ : i, o), $ -=
          l, b = E;
      }
      return g;
    }

    function ve() {
      function e(e) {
        return e.x0 = e.y0 = 0, e.x1 = i, e.y1 = o, e.eachBefore(t), a = [0], r && e.eachBefore(Z), e;
      }

      function t(e) {
        var t = a[e.depth],
          r = e.x0 + t,
          i = e.y0 + t,
          o = e.x1 - t,
          f = e.y1 - t;
        o < r && (r = o = (r + o) / 2), f < i && (i = f = (i + f) / 2), e.x0 = r, e.y0 = i, e.x1 = o, e.y1 =
          f, e.children && (t = a[e.depth + 1] = s(e) / 2, r += d(e) - t, i += c(e) - t, o -= u(e) - t, f -=
            l(e) - t, o < r && (r = o = (r + o) / 2), f < i && (i = f = (i + f) / 2), n(e, r, i, o, f));
      }
      var n = Te,
        r = !1,
        i = 1,
        o = 1,
        a = [0],
        s = V,
        c = V,
        u = V,
        l = V,
        d = V;
      return e.round = function(t) {
        return arguments.length ? (r = !!t, e) : r;
      }, e.size = function(t) {
        return arguments.length ? (i = +t[0], o = +t[1], e) : [i, o];
      }, e.tile = function(t) {
        return arguments.length ? (n = G(t), e) : n;
      }, e.padding = function(t) {
        return arguments.length ? e.paddingInner(t).paddingOuter(t) : e.paddingInner();
      }, e.paddingInner = function(t) {
        return arguments.length ? (s = "function" == typeof t ? t : W(+t), e) : s;
      }, e.paddingOuter = function(t) {
        return arguments.length ? e.paddingTop(t).paddingRight(t).paddingBottom(t).paddingLeft(t) : e
          .paddingTop();
      }, e.paddingTop = function(t) {
        return arguments.length ? (c = "function" == typeof t ? t : W(+t), e) : c;
      }, e.paddingRight = function(t) {
        return arguments.length ? (u = "function" == typeof t ? t : W(+t), e) : u;
      }, e.paddingBottom = function(t) {
        return arguments.length ? (l = "function" == typeof t ? t : W(+t), e) : l;
      }, e.paddingLeft = function(t) {
        return arguments.length ? (d = "function" == typeof t ? t : W(+t), e) : d;
      }, e;
    }

    function ge(e, t, n, r, i) {
      function o(e, t, n, r, i, a, s) {
        if (e >= t - 1) {
          var u = c[e];
          return u.x0 = r, u.y0 = i, u.x1 = a, u.y1 = s, void 0;
        }
        for (var d = l[e], f = n / 2 + d, h = e + 1, p = t - 1; h < p;) {
          var m = h + p >>> 1;
          l[m] < f ? h = m + 1 : p = m;
        }
        f - l[h - 1] < l[h] - f && e + 1 < h && --h;
        var v = l[h] - d,
          g = n - v;
        if (a - r > s - i) {
          var y = (r * g + a * v) / n;
          o(e, h, v, r, i, y, s), o(h, t, g, y, i, a, s);
        } else {
          var b = (i * g + s * v) / n;
          o(e, h, v, r, i, a, b), o(h, t, g, r, b, a, s);
        }
      }
      var a,
        s,
        c = e.children,
        u = c.length,
        l = new Array(u + 1);
      for (l[0] = s = a = 0; a < u; ++a) l[a + 1] = s += c[a].value;
      o(0, u, e.value, t, n, r, i);
    }

    function ye(e, t, n, r, i) {
      (1 & e.depth ? pe : ee)(e, t, n, r, i);
    }
    S.prototype = $.prototype = {
      constructor: S,
      count: l,
      each: d,
      eachAfter: h,
      eachBefore: f,
      sum: p,
      sort: m,
      path: v,
      ancestors: y,
      descendants: b,
      leaves: E,
      links: _,
      copy: w
    };
    var be = Array.prototype.slice,
      Ee = "$",
      _e = {
        depth: -1
      },
      $e = {};
    de.prototype = Object.create(S.prototype);
    var we = (1 + Math.sqrt(5)) / 2,
      Te = function e(t) {
        function n(e, n, r, i, o) {
          me(t, e, n, r, i, o);
        }
        return n.ratio = function(t) {
          return e((t = +t) > 1 ? t : 1);
        }, n;
      }(we),
      Ce = function e(t) {
        function n(e, n, r, i, o) {
          if ((a = e._squarify) && a.ratio === t)
            for (var a, s, c, u, l, d = -1, f = a.length, h = e.value; ++d < f;) {
              for (s = a[d], c = s.children, u = s.value = 0, l = c.length; u < l; ++u) s.value += c[u]
                .value;
              s.dice ? ee(s, n, r, i, r += (o - r) * s.value / h) : pe(s, n, r, n += (i - n) * s.value / h,
                o), h -= s.value;
            } else e._squarify = a = me(t, e, n, r, i, o), a.ratio = t;
        }
        return n.ratio = function(t) {
          return e((t = +t) > 1 ? t : 1);
        }, n;
      }(we);
    e.cluster = c, e.hierarchy = $, e.pack = K, e.packEnclose = M, e.packSiblings = z, e.partition = te, e
      .stratify = ie, e.tree = he, e.treemap = ve, e.treemapBinary = ge, e.treemapDice = ee, e
      .treemapResquarify = Ce, e.treemapSlice = pe, e.treemapSliceDice = ye, e.treemapSquarify = Te, Object
      .defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
