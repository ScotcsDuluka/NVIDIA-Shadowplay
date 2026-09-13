// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 252
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, r) {
    r(exports, require(64));
  }(this, function(e, t) {
    "use strict";

    function n(e) {
      return function() {
        return e;
      };
    }

    function r(e) {
      return e > 1 ? 0 : e < -1 ? Be : Math.acos(e);
    }

    function i(e) {
      return e >= 1 ? ze : e <= -1 ? -ze : Math.asin(e);
    }

    function o(e) {
      return e.innerRadius;
    }

    function a(e) {
      return e.outerRadius;
    }

    function s(e) {
      return e.startAngle;
    }

    function c(e) {
      return e.endAngle;
    }

    function u(e) {
      return e && e.padAngle;
    }

    function l(e, t, n, r, i, o, a, s) {
      var c = n - e,
        u = r - t,
        l = a - i,
        d = s - o,
        f = d * c - l * u;
      if (!(f * f < He)) return f = (l * (t - o) - d * (e - i)) / f, [e + f * c, t + f * u];
    }

    function d(e, t, n, r, i, o, a) {
      var s = e - n,
        c = t - r,
        u = (a ? o : -o) / je(s * s + c * c),
        l = u * c,
        d = -u * s,
        f = e + l,
        h = t + d,
        p = n + l,
        m = r + d,
        v = (f + p) / 2,
        g = (h + m) / 2,
        y = p - f,
        b = m - h,
        E = y * y + b * b,
        _ = i - o,
        $ = f * m - p * h,
        w = (b < 0 ? -1 : 1) * je(Le(0, _ * _ * E - $ * $)),
        T = ($ * b - y * w) / E,
        C = (-$ * y - b * w) / E,
        x = ($ * b + y * w) / E,
        S = (-$ * y + b * w) / E,
        A = T - v,
        M = C - g,
        k = x - v,
        N = S - g;
      return A * A + M * M > k * k + N * N && (T = x, C = S), {
        cx: T,
        cy: C,
        x01: -l,
        y01: -d,
        x11: T * (i / _ - 1),
        y11: C * (i / _ - 1)
      };
    }

    function f() {
      function e() {
        var e,
          n,
          o = +f.apply(this, arguments),
          a = +h.apply(this, arguments),
          s = v.apply(this, arguments) - ze,
          c = g.apply(this, arguments) - ze,
          u = De(c - s),
          E = c > s;
        if (b || (b = e = t.path()), a < o && (n = a, a = o, o = n), a > He) {
          if (u > qe - He) b.moveTo(a * Pe(s), a * Fe(s)), b.arc(0, 0, a, s, c, !E), o > He && (b.moveTo(o *
            Pe(c), o * Fe(c)), b.arc(0, 0, o, c, s, E));
          else {
            var _,
              $,
              w = s,
              T = c,
              C = s,
              x = c,
              S = u,
              A = u,
              M = y.apply(this, arguments) / 2,
              k = M > He && (m ? +m.apply(this, arguments) : je(o * o + a * a)),
              N = Ue(De(a - o) / 2, +p.apply(this, arguments)),
              I = N,
              O = N;
            if (k > He) {
              var D = i(k / o * Fe(M)),
                R = i(k / a * Fe(M));
              (S -= 2 * D) > He ? (D *= E ? 1 : -1, C += D, x -= D) : (S = 0, C = x = (s + c) / 2), (A -=
                2 * R) > He ? (R *= E ? 1 : -1, w += R, T -= R) : (A = 0, w = T = (s + c) / 2);
            }
            var P = a * Pe(w),
              L = a * Fe(w),
              U = o * Pe(x),
              F = o * Fe(x);
            if (N > He) {
              var j,
                H = a * Pe(T),
                B = a * Fe(T),
                z = o * Pe(C),
                q = o * Fe(C);
              if (u < Be && (j = l(P, L, z, q, H, B, U, F))) {
                var G = P - j[0],
                  V = L - j[1],
                  W = H - j[0],
                  Y = B - j[1],
                  K = 1 / Fe(r((G * W + V * Y) / (je(G * G + V * V) * je(W * W + Y * Y))) / 2),
                  X = je(j[0] * j[0] + j[1] * j[1]);
                I = Ue(N, (o - X) / (K - 1)), O = Ue(N, (a - X) / (K + 1));
              }
            }
            A > He ? O > He ? (_ = d(z, q, P, L, a, O, E), $ = d(H, B, U, F, a, O, E), b.moveTo(_.cx + _
                  .x01, _.cy + _.y01), O < N ? b.arc(_.cx, _.cy, O, Re(_.y01, _.x01), Re($.y01, $.x01), !
                E) : (b.arc(_.cx, _.cy, O, Re(_.y01, _.x01), Re(_.y11, _.x11), !E), b.arc(0, 0, a, Re(_.cy +
                  _.y11, _.cx + _.x11), Re($.cy + $.y11, $.cx + $.x11), !E), b.arc($.cx, $.cy, O, Re($
                  .y11, $.x11), Re($.y01, $.x01), !E))) : (b.moveTo(P, L), b.arc(0, 0, a, w, T, !E)) : b
              .moveTo(P, L), o > He && S > He ? I > He ? (_ = d(U, F, H, B, o, -I, E), $ = d(P, L, z, q, o,
                -I, E), b.lineTo(_.cx + _.x01, _.cy + _.y01), I < N ? b.arc(_.cx, _.cy, I, Re(_.y01, _
                .x01), Re($.y01, $.x01), !E) : (b.arc(_.cx, _.cy, I, Re(_.y01, _.x01), Re(_.y11, _.x11), !
                  E), b.arc(0, 0, o, Re(_.cy + _.y11, _.cx + _.x11), Re($.cy + $.y11, $.cx + $.x11), E), b
                .arc($.cx, $.cy, I, Re($.y11, $.x11), Re($.y01, $.x01), !E))) : b.arc(0, 0, o, x, C, E) : b
              .lineTo(U, F);
          }
        } else b.moveTo(0, 0);
        if (b.closePath(), e) return b = null, e + "" || null;
      }
      var f = o,
        h = a,
        p = n(0),
        m = null,
        v = s,
        g = c,
        y = u,
        b = null;
      return e.centroid = function() {
        var e = (+f.apply(this, arguments) + +h.apply(this, arguments)) / 2,
          t = (+v.apply(this, arguments) + +g.apply(this, arguments)) / 2 - Be / 2;
        return [Pe(t) * e, Fe(t) * e];
      }, e.innerRadius = function(t) {
        return arguments.length ? (f = "function" == typeof t ? t : n(+t), e) : f;
      }, e.outerRadius = function(t) {
        return arguments.length ? (h = "function" == typeof t ? t : n(+t), e) : h;
      }, e.cornerRadius = function(t) {
        return arguments.length ? (p = "function" == typeof t ? t : n(+t), e) : p;
      }, e.padRadius = function(t) {
        return arguments.length ? (m = null == t ? null : "function" == typeof t ? t : n(+t), e) : m;
      }, e.startAngle = function(t) {
        return arguments.length ? (v = "function" == typeof t ? t : n(+t), e) : v;
      }, e.endAngle = function(t) {
        return arguments.length ? (g = "function" == typeof t ? t : n(+t), e) : g;
      }, e.padAngle = function(t) {
        return arguments.length ? (y = "function" == typeof t ? t : n(+t), e) : y;
      }, e.context = function(t) {
        return arguments.length ? (b = null == t ? null : t, e) : b;
      }, e;
    }

    function h(e) {
      this._context = e;
    }

    function p(e) {
      return new h(e);
    }

    function m(e) {
      return e[0];
    }

    function v(e) {
      return e[1];
    }

    function g() {
      function e(e) {
        var n,
          u,
          l,
          d = e.length,
          f = !1;
        for (null == a && (c = s(l = t.path())), n = 0; n <= d; ++n) !(n < d && o(u = e[n], n, e)) === f &&
          ((f = !f) ? c.lineStart() : c.lineEnd()), f && c.point(+r(u, n, e), +i(u, n, e));
        if (l) return c = null, l + "" || null;
      }
      var r = m,
        i = v,
        o = n(!0),
        a = null,
        s = p,
        c = null;
      return e.x = function(t) {
        return arguments.length ? (r = "function" == typeof t ? t : n(+t), e) : r;
      }, e.y = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : n(+t), e) : i;
      }, e.defined = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : n(!!t), e) : o;
      }, e.curve = function(t) {
        return arguments.length ? (s = t, null != a && (c = s(a)), e) : s;
      }, e.context = function(t) {
        return arguments.length ? (null == t ? a = c = null : c = s(a = t), e) : a;
      }, e;
    }

    function y() {
      function e(e) {
        var n,
          r,
          f,
          h,
          p,
          m = e.length,
          v = !1,
          g = new Array(m),
          y = new Array(m);
        for (null == u && (d = l(p = t.path())), n = 0; n <= m; ++n) {
          if (!(n < m && c(h = e[n], n, e)) === v)
            if (v = !v) r = n, d.areaStart(), d.lineStart();
            else {
              for (d.lineEnd(), d.lineStart(), f = n - 1; f >= r; --f) d.point(g[f], y[f]);
              d.lineEnd(), d.areaEnd();
            }
          v && (g[n] = +i(h, n, e), y[n] = +a(h, n, e), d.point(o ? +o(h, n, e) : g[n], s ? +s(h, n, e) : y[
            n]));
        }
        if (p) return d = null, p + "" || null;
      }

      function r() {
        return g().defined(c).curve(l).context(u);
      }
      var i = m,
        o = null,
        a = n(0),
        s = v,
        c = n(!0),
        u = null,
        l = p,
        d = null;
      return e.x = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : n(+t), o = null, e) : i;
      }, e.x0 = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : n(+t), e) : i;
      }, e.x1 = function(t) {
        return arguments.length ? (o = null == t ? null : "function" == typeof t ? t : n(+t), e) : o;
      }, e.y = function(t) {
        return arguments.length ? (a = "function" == typeof t ? t : n(+t), s = null, e) : a;
      }, e.y0 = function(t) {
        return arguments.length ? (a = "function" == typeof t ? t : n(+t), e) : a;
      }, e.y1 = function(t) {
        return arguments.length ? (s = null == t ? null : "function" == typeof t ? t : n(+t), e) : s;
      }, e.lineX0 = e.lineY0 = function() {
        return r().x(i).y(a);
      }, e.lineY1 = function() {
        return r().x(i).y(s);
      }, e.lineX1 = function() {
        return r().x(o).y(a);
      }, e.defined = function(t) {
        return arguments.length ? (c = "function" == typeof t ? t : n(!!t), e) : c;
      }, e.curve = function(t) {
        return arguments.length ? (l = t, null != u && (d = l(u)), e) : l;
      }, e.context = function(t) {
        return arguments.length ? (null == t ? u = d = null : d = l(u = t), e) : u;
      }, e;
    }

    function b(e, t) {
      return t < e ? -1 : t > e ? 1 : t >= e ? 0 : NaN;
    }

    function E(e) {
      return e;
    }

    function _() {
      function e(e) {
        var n,
          c,
          u,
          l,
          d,
          f = e.length,
          h = 0,
          p = new Array(f),
          m = new Array(f),
          v = +o.apply(this, arguments),
          g = Math.min(qe, Math.max(-qe, a.apply(this, arguments) - v)),
          y = Math.min(Math.abs(g) / f, s.apply(this, arguments)),
          b = y * (g < 0 ? -1 : 1);
        for (n = 0; n < f; ++n)(d = m[p[n] = n] = +t(e[n], n, e)) > 0 && (h += d);
        for (null != r ? p.sort(function(e, t) {
            return r(m[e], m[t]);
          }) : null != i && p.sort(function(t, n) {
            return i(e[t], e[n]);
          }), n = 0, u = h ? (g - f * b) / h : 0; n < f; ++n, v = l) c = p[n], d = m[c], l = v + (d > 0 ?
          d * u : 0) + b, m[c] = {
          data: e[c],
          index: n,
          value: d,
          startAngle: v,
          endAngle: l,
          padAngle: y
        };
        return m;
      }
      var t = E,
        r = b,
        i = null,
        o = n(0),
        a = n(qe),
        s = n(0);
      return e.value = function(r) {
        return arguments.length ? (t = "function" == typeof r ? r : n(+r), e) : t;
      }, e.sortValues = function(t) {
        return arguments.length ? (r = t, i = null, e) : r;
      }, e.sort = function(t) {
        return arguments.length ? (i = t, r = null, e) : i;
      }, e.startAngle = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : n(+t), e) : o;
      }, e.endAngle = function(t) {
        return arguments.length ? (a = "function" == typeof t ? t : n(+t), e) : a;
      }, e.padAngle = function(t) {
        return arguments.length ? (s = "function" == typeof t ? t : n(+t), e) : s;
      }, e;
    }

    function $(e) {
      this._curve = e;
    }

    function w(e) {
      function t(t) {
        return new $(e(t));
      }
      return t._curve = e, t;
    }

    function T(e) {
      var t = e.curve;
      return e.angle = e.x, delete e.x, e.radius = e.y, delete e.y, e.curve = function(e) {
        return arguments.length ? t(w(e)) : t()._curve;
      }, e;
    }

    function C() {
      return T(g().curve(Ge));
    }

    function x() {
      var e = y().curve(Ge),
        t = e.curve,
        n = e.lineX0,
        r = e.lineX1,
        i = e.lineY0,
        o = e.lineY1;
      return e.angle = e.x, delete e.x, e.startAngle = e.x0, delete e.x0, e.endAngle = e.x1, delete e.x1, e
        .radius = e.y, delete e.y, e.innerRadius = e.y0, delete e.y0, e.outerRadius = e.y1, delete e.y1, e
        .lineStartAngle = function() {
          return T(n());
        }, delete e.lineX0, e.lineEndAngle = function() {
          return T(r());
        }, delete e.lineX1, e.lineInnerRadius = function() {
          return T(i());
        }, delete e.lineY0, e.lineOuterRadius = function() {
          return T(o());
        }, delete e.lineY1, e.curve = function(e) {
          return arguments.length ? t(w(e)) : t()._curve;
        }, e;
    }

    function S(e, t) {
      return [(t = +t) * Math.cos(e -= Math.PI / 2), t * Math.sin(e)];
    }

    function A(e) {
      return e.source;
    }

    function M(e) {
      return e.target;
    }

    function k(e) {
      function r() {
        var n,
          r = Ve.call(arguments),
          u = i.apply(this, r),
          l = o.apply(this, r);
        if (c || (c = n = t.path()), e(c, +a.apply(this, (r[0] = u, r)), +s.apply(this, r), +a.apply(this, (
            r[0] = l, r)), +s.apply(this, r)), n) return c = null, n + "" || null;
      }
      var i = A,
        o = M,
        a = m,
        s = v,
        c = null;
      return r.source = function(e) {
        return arguments.length ? (i = e, r) : i;
      }, r.target = function(e) {
        return arguments.length ? (o = e, r) : o;
      }, r.x = function(e) {
        return arguments.length ? (a = "function" == typeof e ? e : n(+e), r) : a;
      }, r.y = function(e) {
        return arguments.length ? (s = "function" == typeof e ? e : n(+e), r) : s;
      }, r.context = function(e) {
        return arguments.length ? (c = null == e ? null : e, r) : c;
      }, r;
    }

    function N(e, t, n, r, i) {
      e.moveTo(t, n), e.bezierCurveTo(t = (t + r) / 2, n, t, i, r, i);
    }

    function I(e, t, n, r, i) {
      e.moveTo(t, n), e.bezierCurveTo(t, n = (n + i) / 2, r, n, r, i);
    }

    function O(e, t, n, r, i) {
      var o = S(t, n),
        a = S(t, n = (n + i) / 2),
        s = S(r, n),
        c = S(r, i);
      e.moveTo(o[0], o[1]), e.bezierCurveTo(a[0], a[1], s[0], s[1], c[0], c[1]);
    }

    function D() {
      return k(N);
    }

    function R() {
      return k(I);
    }

    function P() {
      var e = k(O);
      return e.angle = e.x, delete e.x, e.radius = e.y, delete e.y, e;
    }

    function L() {
      function e() {
        var e;
        if (o || (o = e = t.path()), r.apply(this, arguments).draw(o, +i.apply(this, arguments)), e)
        return o = null, e + "" || null;
      }
      var r = n(We),
        i = n(64),
        o = null;
      return e.type = function(t) {
        return arguments.length ? (r = "function" == typeof t ? t : n(t), e) : r;
      }, e.size = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : n(+t), e) : i;
      }, e.context = function(t) {
        return arguments.length ? (o = null == t ? null : t, e) : o;
      }, e;
    }

    function U() {}

    function F(e, t, n) {
      e._context.bezierCurveTo((2 * e._x0 + e._x1) / 3, (2 * e._y0 + e._y1) / 3, (e._x0 + 2 * e._x1) / 3, (e
        ._y0 + 2 * e._y1) / 3, (e._x0 + 4 * e._x1 + t) / 6, (e._y0 + 4 * e._y1 + n) / 6);
    }

    function j(e) {
      this._context = e;
    }

    function H(e) {
      return new j(e);
    }

    function B(e) {
      this._context = e;
    }

    function z(e) {
      return new B(e);
    }

    function q(e) {
      this._context = e;
    }

    function G(e) {
      return new q(e);
    }

    function V(e, t) {
      this._basis = new j(e), this._beta = t;
    }

    function W(e, t, n) {
      e._context.bezierCurveTo(e._x1 + e._k * (e._x2 - e._x0), e._y1 + e._k * (e._y2 - e._y0), e._x2 + e
        ._k * (e._x1 - t), e._y2 + e._k * (e._y1 - n), e._x2, e._y2);
    }

    function Y(e, t) {
      this._context = e, this._k = (1 - t) / 6;
    }

    function K(e, t) {
      this._context = e, this._k = (1 - t) / 6;
    }

    function X(e, t) {
      this._context = e, this._k = (1 - t) / 6;
    }

    function Q(e, t, n) {
      var r = e._x1,
        i = e._y1,
        o = e._x2,
        a = e._y2;
      if (e._l01_a > He) {
        var s = 2 * e._l01_2a + 3 * e._l01_a * e._l12_a + e._l12_2a,
          c = 3 * e._l01_a * (e._l01_a + e._l12_a);
        r = (r * s - e._x0 * e._l12_2a + e._x2 * e._l01_2a) / c, i = (i * s - e._y0 * e._l12_2a + e._y2 * e
          ._l01_2a) / c;
      }
      if (e._l23_a > He) {
        var u = 2 * e._l23_2a + 3 * e._l23_a * e._l12_a + e._l12_2a,
          l = 3 * e._l23_a * (e._l23_a + e._l12_a);
        o = (o * u + e._x1 * e._l23_2a - t * e._l12_2a) / l, a = (a * u + e._y1 * e._l23_2a - n * e
          ._l12_2a) / l;
      }
      e._context.bezierCurveTo(r, i, o, a, e._x2, e._y2);
    }

    function J(e, t) {
      this._context = e, this._alpha = t;
    }

    function Z(e, t) {
      this._context = e, this._alpha = t;
    }

    function ee(e, t) {
      this._context = e, this._alpha = t;
    }

    function te(e) {
      this._context = e;
    }

    function ne(e) {
      return new te(e);
    }

    function re(e) {
      return e < 0 ? -1 : 1;
    }

    function ie(e, t, n) {
      var r = e._x1 - e._x0,
        i = t - e._x1,
        o = (e._y1 - e._y0) / (r || i < 0 && -0),
        a = (n - e._y1) / (i || r < 0 && -0),
        s = (o * i + a * r) / (r + i);
      return (re(o) + re(a)) * Math.min(Math.abs(o), Math.abs(a), .5 * Math.abs(s)) || 0;
    }

    function oe(e, t) {
      var n = e._x1 - e._x0;
      return n ? (3 * (e._y1 - e._y0) / n - t) / 2 : t;
    }

    function ae(e, t, n) {
      var r = e._x0,
        i = e._y0,
        o = e._x1,
        a = e._y1,
        s = (o - r) / 3;
      e._context.bezierCurveTo(r + s, i + s * t, o - s, a - s * n, o, a);
    }

    function se(e) {
      this._context = e;
    }

    function ce(e) {
      this._context = new ue(e);
    }

    function ue(e) {
      this._context = e;
    }

    function le(e) {
      return new se(e);
    }

    function de(e) {
      return new ce(e);
    }

    function fe(e) {
      this._context = e;
    }

    function he(e) {
      var t,
        n,
        r = e.length - 1,
        i = new Array(r),
        o = new Array(r),
        a = new Array(r);
      for (i[0] = 0, o[0] = 2, a[0] = e[0] + 2 * e[1], t = 1; t < r - 1; ++t) i[t] = 1, o[t] = 4, a[t] = 4 *
        e[t] + 2 * e[t + 1];
      for (i[r - 1] = 2, o[r - 1] = 7, a[r - 1] = 8 * e[r - 1] + e[r], t = 1; t < r; ++t) n = i[t] / o[t -
        1], o[t] -= n, a[t] -= n * a[t - 1];
      for (i[r - 1] = a[r - 1] / o[r - 1], t = r - 2; t >= 0; --t) i[t] = (a[t] - i[t + 1]) / o[t];
      for (o[r - 1] = (e[r] + i[r - 1]) / 2, t = 0; t < r - 1; ++t) o[t] = 2 * e[t + 1] - i[t + 1];
      return [i, o];
    }

    function pe(e) {
      return new fe(e);
    }

    function me(e, t) {
      this._context = e, this._t = t;
    }

    function ve(e) {
      return new me(e, .5);
    }

    function ge(e) {
      return new me(e, 0);
    }

    function ye(e) {
      return new me(e, 1);
    }

    function be(e, t) {
      if ((i = e.length) > 1)
        for (var n, r, i, o = 1, a = e[t[0]], s = a.length; o < i; ++o)
          for (r = a, a = e[t[o]], n = 0; n < s; ++n) a[n][1] += a[n][0] = isNaN(r[n][1]) ? r[n][0] : r[n][
            1];
    }

    function Ee(e) {
      for (var t = e.length, n = new Array(t); --t >= 0;) n[t] = t;
      return n;
    }

    function _e(e, t) {
      return e[t];
    }

    function $e() {
      function e(e) {
        var n,
          a,
          s = t.apply(this, arguments),
          c = e.length,
          u = s.length,
          l = new Array(u);
        for (n = 0; n < u; ++n) {
          for (var d, f = s[n], h = l[n] = new Array(c), p = 0; p < c; ++p) h[p] = d = [0, +o(e[p], f, p,
            e)], d.data = e[p];
          h.key = f;
        }
        for (n = 0, a = r(l); n < u; ++n) l[a[n]].index = n;
        return i(l, a), l;
      }
      var t = n([]),
        r = Ee,
        i = be,
        o = _e;
      return e.keys = function(r) {
        return arguments.length ? (t = "function" == typeof r ? r : n(Ve.call(r)), e) : t;
      }, e.value = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : n(+t), e) : o;
      }, e.order = function(t) {
        return arguments.length ? (r = null == t ? Ee : "function" == typeof t ? t : n(Ve.call(t)), e) :
        r;
      }, e.offset = function(t) {
        return arguments.length ? (i = null == t ? be : t, e) : i;
      }, e;
    }

    function we(e, t) {
      if ((r = e.length) > 0) {
        for (var n, r, i, o = 0, a = e[0].length; o < a; ++o) {
          for (i = n = 0; n < r; ++n) i += e[n][o][1] || 0;
          if (i)
            for (n = 0; n < r; ++n) e[n][o][1] /= i;
        }
        be(e, t);
      }
    }

    function Te(e, t) {
      if ((s = e.length) > 0)
        for (var n, r, i, o, a, s, c = 0, u = e[t[0]].length; c < u; ++c)
          for (o = a = 0, n = 0; n < s; ++n)(i = (r = e[t[n]][c])[1] - r[0]) > 0 ? (r[0] = o, r[1] = o +=
            i) : i < 0 ? (r[1] = a, r[0] = a += i) : (r[0] = 0, r[1] = i);
    }

    function Ce(e, t) {
      if ((n = e.length) > 0) {
        for (var n, r = 0, i = e[t[0]], o = i.length; r < o; ++r) {
          for (var a = 0, s = 0; a < n; ++a) s += e[a][r][1] || 0;
          i[r][1] += i[r][0] = -s / 2;
        }
        be(e, t);
      }
    }

    function xe(e, t) {
      if ((i = e.length) > 0 && (r = (n = e[t[0]]).length) > 0) {
        for (var n, r, i, o = 0, a = 1; a < r; ++a) {
          for (var s = 0, c = 0, u = 0; s < i; ++s) {
            for (var l = e[t[s]], d = l[a][1] || 0, f = l[a - 1][1] || 0, h = (d - f) / 2, p = 0; p < s; ++
              p) {
              var m = e[t[p]],
                v = m[a][1] || 0,
                g = m[a - 1][1] || 0;
              h += v - g;
            }
            c += d, u += h * d;
          }
          n[a - 1][1] += n[a - 1][0] = o, c && (o -= u / c);
        }
        n[a - 1][1] += n[a - 1][0] = o, be(e, t);
      }
    }

    function Se(e) {
      var t = e.map(Ae);
      return Ee(e).sort(function(e, n) {
        return t[e] - t[n];
      });
    }

    function Ae(e) {
      for (var t, n = -1, r = 0, i = e.length, o = -(1 / 0); ++n < i;)(t = +e[n][1]) > o && (o = t, r = n);
      return r;
    }

    function Me(e) {
      var t = e.map(ke);
      return Ee(e).sort(function(e, n) {
        return t[e] - t[n];
      });
    }

    function ke(e) {
      for (var t, n = 0, r = -1, i = e.length; ++r < i;)(t = +e[r][1]) && (n += t);
      return n;
    }

    function Ne(e) {
      return Me(e).reverse();
    }

    function Ie(e) {
      var t,
        n,
        r = e.length,
        i = e.map(ke),
        o = Se(e),
        a = 0,
        s = 0,
        c = [],
        u = [];
      for (t = 0; t < r; ++t) n = o[t], a < s ? (a += i[n], c.push(n)) : (s += i[n], u.push(n));
      return u.reverse().concat(c);
    }

    function Oe(e) {
      return Ee(e).reverse();
    }
    var De = Math.abs,
      Re = Math.atan2,
      Pe = Math.cos,
      Le = Math.max,
      Ue = Math.min,
      Fe = Math.sin,
      je = Math.sqrt,
      He = 1e-12,
      Be = Math.PI,
      ze = Be / 2,
      qe = 2 * Be;
    h.prototype = {
      areaStart: function() {
        this._line = 0;
      },
      areaEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._point = 0;
      },
      lineEnd: function() {
        (this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line =
          1 - this._line;
      },
      point: function(e, t) {
        switch (e = +e, t = +t, this._point) {
          case 0:
            this._point = 1, this._line ? this._context.lineTo(e, t) : this._context.moveTo(e, t);
            break;
          case 1:
            this._point = 2;
          default:
            this._context.lineTo(e, t);
        }
      }
    };
    var Ge = w(p);
    $.prototype = {
      areaStart: function() {
        this._curve.areaStart();
      },
      areaEnd: function() {
        this._curve.areaEnd();
      },
      lineStart: function() {
        this._curve.lineStart();
      },
      lineEnd: function() {
        this._curve.lineEnd();
      },
      point: function(e, t) {
        this._curve.point(t * Math.sin(e), t * -Math.cos(e));
      }
    };
    var Ve = Array.prototype.slice,
      We = {
        draw: function(e, t) {
          var n = Math.sqrt(t / Be);
          e.moveTo(n, 0), e.arc(0, 0, n, 0, qe);
        }
      },
      Ye = {
        draw: function(e, t) {
          var n = Math.sqrt(t / 5) / 2;
          e.moveTo(-3 * n, -n), e.lineTo(-n, -n), e.lineTo(-n, -3 * n), e.lineTo(n, -3 * n), e.lineTo(n, -
            n), e.lineTo(3 * n, -n), e.lineTo(3 * n, n), e.lineTo(n, n), e.lineTo(n, 3 * n), e.lineTo(-
            n, 3 * n), e.lineTo(-n, n), e.lineTo(-3 * n, n), e.closePath();
        }
      },
      Ke = Math.sqrt(1 / 3),
      Xe = 2 * Ke,
      Qe = {
        draw: function(e, t) {
          var n = Math.sqrt(t / Xe),
            r = n * Ke;
          e.moveTo(0, -n), e.lineTo(r, 0), e.lineTo(0, n), e.lineTo(-r, 0), e.closePath();
        }
      },
      Je = .8908130915292852,
      Ze = Math.sin(Be / 10) / Math.sin(7 * Be / 10),
      et = Math.sin(qe / 10) * Ze,
      tt = -Math.cos(qe / 10) * Ze,
      nt = {
        draw: function(e, t) {
          var n = Math.sqrt(t * Je),
            r = et * n,
            i = tt * n;
          e.moveTo(0, -n), e.lineTo(r, i);
          for (var o = 1; o < 5; ++o) {
            var a = qe * o / 5,
              s = Math.cos(a),
              c = Math.sin(a);
            e.lineTo(c * n, -s * n), e.lineTo(s * r - c * i, c * r + s * i);
          }
          e.closePath();
        }
      },
      rt = {
        draw: function(e, t) {
          var n = Math.sqrt(t),
            r = -n / 2;
          e.rect(r, r, n, n);
        }
      },
      it = Math.sqrt(3),
      ot = {
        draw: function(e, t) {
          var n = -Math.sqrt(t / (3 * it));
          e.moveTo(0, 2 * n), e.lineTo(-it * n, -n), e.lineTo(it * n, -n), e.closePath();
        }
      },
      at = -.5,
      st = Math.sqrt(3) / 2,
      ct = 1 / Math.sqrt(12),
      ut = 3 * (ct / 2 + 1),
      lt = {
        draw: function(e, t) {
          var n = Math.sqrt(t / ut),
            r = n / 2,
            i = n * ct,
            o = r,
            a = n * ct + n,
            s = -o,
            c = a;
          e.moveTo(r, i), e.lineTo(o, a), e.lineTo(s, c), e.lineTo(at * r - st * i, st * r + at * i), e
            .lineTo(at * o - st * a, st * o + at * a), e.lineTo(at * s - st * c, st * s + at * c), e
            .lineTo(at * r + st * i, at * i - st * r), e.lineTo(at * o + st * a, at * a - st * o), e
            .lineTo(at * s + st * c, at * c - st * s), e.closePath();
        }
      },
      dt = [We, Ye, Qe, rt, nt, ot, lt];
    j.prototype = {
      areaStart: function() {
        this._line = 0;
      },
      areaEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._x0 = this._x1 = this._y0 = this._y1 = NaN, this._point = 0;
      },
      lineEnd: function() {
        switch (this._point) {
          case 3:
            F(this, this._x1, this._y1);
          case 2:
            this._context.lineTo(this._x1, this._y1);
        }
        (this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line =
          1 - this._line;
      },
      point: function(e, t) {
        switch (e = +e, t = +t, this._point) {
          case 0:
            this._point = 1, this._line ? this._context.lineTo(e, t) : this._context.moveTo(e, t);
            break;
          case 1:
            this._point = 2;
            break;
          case 2:
            this._point = 3, this._context.lineTo((5 * this._x0 + this._x1) / 6, (5 * this._y0 + this
              ._y1) / 6);
          default:
            F(this, e, t);
        }
        this._x0 = this._x1, this._x1 = e, this._y0 = this._y1, this._y1 = t;
      }
    }, B.prototype = {
      areaStart: U,
      areaEnd: U,
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._x3 = this._x4 = this._y0 = this._y1 = this._y2 = this
          ._y3 = this._y4 = NaN, this._point = 0;
      },
      lineEnd: function() {
        switch (this._point) {
          case 1:
            this._context.moveTo(this._x2, this._y2), this._context.closePath();
            break;
          case 2:
            this._context.moveTo((this._x2 + 2 * this._x3) / 3, (this._y2 + 2 * this._y3) / 3), this
              ._context.lineTo((this._x3 + 2 * this._x2) / 3, (this._y3 + 2 * this._y2) / 3), this
              ._context.closePath();
            break;
          case 3:
            this.point(this._x2, this._y2), this.point(this._x3, this._y3), this.point(this._x4, this
              ._y4);
        }
      },
      point: function(e, t) {
        switch (e = +e, t = +t, this._point) {
          case 0:
            this._point = 1, this._x2 = e, this._y2 = t;
            break;
          case 1:
            this._point = 2, this._x3 = e, this._y3 = t;
            break;
          case 2:
            this._point = 3, this._x4 = e, this._y4 = t, this._context.moveTo((this._x0 + 4 * this._x1 +
              e) / 6, (this._y0 + 4 * this._y1 + t) / 6);
            break;
          default:
            F(this, e, t);
        }
        this._x0 = this._x1, this._x1 = e, this._y0 = this._y1, this._y1 = t;
      }
    }, q.prototype = {
      areaStart: function() {
        this._line = 0;
      },
      areaEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._x0 = this._x1 = this._y0 = this._y1 = NaN, this._point = 0;
      },
      lineEnd: function() {
        (this._line || 0 !== this._line && 3 === this._point) && this._context.closePath(), this._line =
          1 - this._line;
      },
      point: function(e, t) {
        switch (e = +e, t = +t, this._point) {
          case 0:
            this._point = 1;
            break;
          case 1:
            this._point = 2;
            break;
          case 2:
            this._point = 3;
            var n = (this._x0 + 4 * this._x1 + e) / 6,
              r = (this._y0 + 4 * this._y1 + t) / 6;
            this._line ? this._context.lineTo(n, r) : this._context.moveTo(n, r);
            break;
          case 3:
            this._point = 4;
          default:
            F(this, e, t);
        }
        this._x0 = this._x1, this._x1 = e, this._y0 = this._y1, this._y1 = t;
      }
    }, V.prototype = {
      lineStart: function() {
        this._x = [], this._y = [], this._basis.lineStart();
      },
      lineEnd: function() {
        var e = this._x,
          t = this._y,
          n = e.length - 1;
        if (n > 0)
          for (var r, i = e[0], o = t[0], a = e[n] - i, s = t[n] - o, c = -1; ++c <= n;) r = c / n, this
            ._basis.point(this._beta * e[c] + (1 - this._beta) * (i + r * a), this._beta * t[c] + (1 -
              this._beta) * (o + r * s));
        this._x = this._y = null, this._basis.lineEnd();
      },
      point: function(e, t) {
        this._x.push(+e), this._y.push(+t);
      }
    };
    var ft = function e(t) {
      function n(e) {
        return 1 === t ? new j(e) : new V(e, t);
      }
      return n.beta = function(t) {
        return e(+t);
      }, n;
    }(.85);
    Y.prototype = {
      areaStart: function() {
        this._line = 0;
      },
      areaEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._y0 = this._y1 = this._y2 = NaN, this._point = 0;
      },
      lineEnd: function() {
        switch (this._point) {
          case 2:
            this._context.lineTo(this._x2, this._y2);
            break;
          case 3:
            W(this, this._x1, this._y1);
        }
        (this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line =
          1 - this._line;
      },
      point: function(e, t) {
        switch (e = +e, t = +t, this._point) {
          case 0:
            this._point = 1, this._line ? this._context.lineTo(e, t) : this._context.moveTo(e, t);
            break;
          case 1:
            this._point = 2, this._x1 = e, this._y1 = t;
            break;
          case 2:
            this._point = 3;
          default:
            W(this, e, t);
        }
        this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this
          ._y2, this._y2 = t;
      }
    };
    var ht = function e(t) {
      function n(e) {
        return new Y(e, t);
      }
      return n.tension = function(t) {
        return e(+t);
      }, n;
    }(0);
    K.prototype = {
      areaStart: U,
      areaEnd: U,
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._x3 = this._x4 = this._x5 = this._y0 = this._y1 = this
          ._y2 = this._y3 = this._y4 = this._y5 = NaN, this._point = 0;
      },
      lineEnd: function() {
        switch (this._point) {
          case 1:
            this._context.moveTo(this._x3, this._y3), this._context.closePath();
            break;
          case 2:
            this._context.lineTo(this._x3, this._y3), this._context.closePath();
            break;
          case 3:
            this.point(this._x3, this._y3), this.point(this._x4, this._y4), this.point(this._x5, this
              ._y5);
        }
      },
      point: function(e, t) {
        switch (e = +e, t = +t, this._point) {
          case 0:
            this._point = 1, this._x3 = e, this._y3 = t;
            break;
          case 1:
            this._point = 2, this._context.moveTo(this._x4 = e, this._y4 = t);
            break;
          case 2:
            this._point = 3, this._x5 = e, this._y5 = t;
            break;
          default:
            W(this, e, t);
        }
        this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this
          ._y2, this._y2 = t;
      }
    };
    var pt = function e(t) {
      function n(e) {
        return new K(e, t);
      }
      return n.tension = function(t) {
        return e(+t);
      }, n;
    }(0);
    X.prototype = {
      areaStart: function() {
        this._line = 0;
      },
      areaEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._y0 = this._y1 = this._y2 = NaN, this._point = 0;
      },
      lineEnd: function() {
        (this._line || 0 !== this._line && 3 === this._point) && this._context.closePath(), this._line =
          1 - this._line;
      },
      point: function(e, t) {
        switch (e = +e, t = +t, this._point) {
          case 0:
            this._point = 1;
            break;
          case 1:
            this._point = 2;
            break;
          case 2:
            this._point = 3, this._line ? this._context.lineTo(this._x2, this._y2) : this._context
              .moveTo(this._x2, this._y2);
            break;
          case 3:
            this._point = 4;
          default:
            W(this, e, t);
        }
        this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this
          ._y2, this._y2 = t;
      }
    };
    var mt = function e(t) {
      function n(e) {
        return new X(e, t);
      }
      return n.tension = function(t) {
        return e(+t);
      }, n;
    }(0);
    J.prototype = {
      areaStart: function() {
        this._line = 0;
      },
      areaEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._y0 = this._y1 = this._y2 = NaN, this._l01_a = this
          ._l12_a = this._l23_a = this._l01_2a = this._l12_2a = this._l23_2a = this._point = 0;
      },
      lineEnd: function() {
        switch (this._point) {
          case 2:
            this._context.lineTo(this._x2, this._y2);
            break;
          case 3:
            this.point(this._x2, this._y2);
        }
        (this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line =
          1 - this._line;
      },
      point: function(e, t) {
        if (e = +e, t = +t, this._point) {
          var n = this._x2 - e,
            r = this._y2 - t;
          this._l23_a = Math.sqrt(this._l23_2a = Math.pow(n * n + r * r, this._alpha));
        }
        switch (this._point) {
          case 0:
            this._point = 1, this._line ? this._context.lineTo(e, t) : this._context.moveTo(e, t);
            break;
          case 1:
            this._point = 2;
            break;
          case 2:
            this._point = 3;
          default:
            Q(this, e, t);
        }
        this._l01_a = this._l12_a, this._l12_a = this._l23_a, this._l01_2a = this._l12_2a, this
          ._l12_2a = this._l23_2a, this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 =
          this._y1, this._y1 = this._y2, this._y2 = t;
      }
    };
    var vt = function e(t) {
      function n(e) {
        return t ? new J(e, t) : new Y(e, 0);
      }
      return n.alpha = function(t) {
        return e(+t);
      }, n;
    }(.5);
    Z.prototype = {
      areaStart: U,
      areaEnd: U,
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._x3 = this._x4 = this._x5 = this._y0 = this._y1 = this
          ._y2 = this._y3 = this._y4 = this._y5 = NaN, this._l01_a = this._l12_a = this._l23_a = this
          ._l01_2a = this._l12_2a = this._l23_2a = this._point = 0;
      },
      lineEnd: function() {
        switch (this._point) {
          case 1:
            this._context.moveTo(this._x3, this._y3), this._context.closePath();
            break;
          case 2:
            this._context.lineTo(this._x3, this._y3), this._context.closePath();
            break;
          case 3:
            this.point(this._x3, this._y3), this.point(this._x4, this._y4), this.point(this._x5, this
              ._y5);
        }
      },
      point: function(e, t) {
        if (e = +e, t = +t, this._point) {
          var n = this._x2 - e,
            r = this._y2 - t;
          this._l23_a = Math.sqrt(this._l23_2a = Math.pow(n * n + r * r, this._alpha));
        }
        switch (this._point) {
          case 0:
            this._point = 1, this._x3 = e, this._y3 = t;
            break;
          case 1:
            this._point = 2, this._context.moveTo(this._x4 = e, this._y4 = t);
            break;
          case 2:
            this._point = 3, this._x5 = e, this._y5 = t;
            break;
          default:
            Q(this, e, t);
        }
        this._l01_a = this._l12_a, this._l12_a = this._l23_a, this._l01_2a = this._l12_2a, this
          ._l12_2a = this._l23_2a, this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 =
          this._y1, this._y1 = this._y2, this._y2 = t;
      }
    };
    var gt = function e(t) {
      function n(e) {
        return t ? new Z(e, t) : new K(e, 0);
      }
      return n.alpha = function(t) {
        return e(+t);
      }, n;
    }(.5);
    ee.prototype = {
      areaStart: function() {
        this._line = 0;
      },
      areaEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._y0 = this._y1 = this._y2 = NaN, this._l01_a = this
          ._l12_a = this._l23_a = this._l01_2a = this._l12_2a = this._l23_2a = this._point = 0;
      },
      lineEnd: function() {
        (this._line || 0 !== this._line && 3 === this._point) && this._context.closePath(), this._line =
          1 - this._line;
      },
      point: function(e, t) {
        if (e = +e, t = +t, this._point) {
          var n = this._x2 - e,
            r = this._y2 - t;
          this._l23_a = Math.sqrt(this._l23_2a = Math.pow(n * n + r * r, this._alpha));
        }
        switch (this._point) {
          case 0:
            this._point = 1;
            break;
          case 1:
            this._point = 2;
            break;
          case 2:
            this._point = 3, this._line ? this._context.lineTo(this._x2, this._y2) : this._context
              .moveTo(this._x2, this._y2);
            break;
          case 3:
            this._point = 4;
          default:
            Q(this, e, t);
        }
        this._l01_a = this._l12_a, this._l12_a = this._l23_a, this._l01_2a = this._l12_2a, this
          ._l12_2a = this._l23_2a, this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 =
          this._y1, this._y1 = this._y2, this._y2 = t;
      }
    };
    var yt = function e(t) {
      function n(e) {
        return t ? new ee(e, t) : new X(e, 0);
      }
      return n.alpha = function(t) {
        return e(+t);
      }, n;
    }(.5);
    te.prototype = {
        areaStart: U,
        areaEnd: U,
        lineStart: function() {
          this._point = 0;
        },
        lineEnd: function() {
          this._point && this._context.closePath();
        },
        point: function(e, t) {
          e = +e, t = +t, this._point ? this._context.lineTo(e, t) : (this._point = 1, this._context
            .moveTo(e, t));
        }
      }, se.prototype = {
        areaStart: function() {
          this._line = 0;
        },
        areaEnd: function() {
          this._line = NaN;
        },
        lineStart: function() {
          this._x0 = this._x1 = this._y0 = this._y1 = this._t0 = NaN, this._point = 0;
        },
        lineEnd: function() {
          switch (this._point) {
            case 2:
              this._context.lineTo(this._x1, this._y1);
              break;
            case 3:
              ae(this, this._t0, oe(this, this._t0));
          }
          (this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line =
            1 - this._line;
        },
        point: function(e, t) {
          var n = NaN;
          if (e = +e, t = +t, e !== this._x1 || t !== this._y1) {
            switch (this._point) {
              case 0:
                this._point = 1, this._line ? this._context.lineTo(e, t) : this._context.moveTo(e, t);
                break;
              case 1:
                this._point = 2;
                break;
              case 2:
                this._point = 3, ae(this, oe(this, n = ie(this, e, t)), n);
                break;
              default:
                ae(this, this._t0, n = ie(this, e, t));
            }
            this._x0 = this._x1, this._x1 = e, this._y0 = this._y1, this._y1 = t, this._t0 = n;
          }
        }
      }, (ce.prototype = Object.create(se.prototype)).point = function(e, t) {
        se.prototype.point.call(this, t, e);
      }, ue.prototype = {
        moveTo: function(e, t) {
          this._context.moveTo(t, e);
        },
        closePath: function() {
          this._context.closePath();
        },
        lineTo: function(e, t) {
          this._context.lineTo(t, e);
        },
        bezierCurveTo: function(e, t, n, r, i, o) {
          this._context.bezierCurveTo(t, e, r, n, o, i);
        }
      }, fe.prototype = {
        areaStart: function() {
          this._line = 0;
        },
        areaEnd: function() {
          this._line = NaN;
        },
        lineStart: function() {
          this._x = [], this._y = [];
        },
        lineEnd: function() {
          var e = this._x,
            t = this._y,
            n = e.length;
          if (n)
            if (this._line ? this._context.lineTo(e[0], t[0]) : this._context.moveTo(e[0], t[0]), 2 === n)
              this._context.lineTo(e[1], t[1]);
            else
              for (var r = he(e), i = he(t), o = 0, a = 1; a < n; ++o, ++a) this._context.bezierCurveTo(r[
                0][o], i[0][o], r[1][o], i[1][o], e[a], t[a]);
          (this._line || 0 !== this._line && 1 === n) && this._context.closePath(), this._line = 1 - this
            ._line, this._x = this._y = null;
        },
        point: function(e, t) {
          this._x.push(+e), this._y.push(+t);
        }
      }, me.prototype = {
        areaStart: function() {
          this._line = 0;
        },
        areaEnd: function() {
          this._line = NaN;
        },
        lineStart: function() {
          this._x = this._y = NaN, this._point = 0;
        },
        lineEnd: function() {
          0 < this._t && this._t < 1 && 2 === this._point && this._context.lineTo(this._x, this._y), (this
              ._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line >=
            0 && (this._t = 1 - this._t, this._line = 1 - this._line);
        },
        point: function(e, t) {
          switch (e = +e, t = +t, this._point) {
            case 0:
              this._point = 1, this._line ? this._context.lineTo(e, t) : this._context.moveTo(e, t);
              break;
            case 1:
              this._point = 2;
            default:
              if (this._t <= 0) this._context.lineTo(this._x, t), this._context.lineTo(e, t);
              else {
                var n = this._x * (1 - this._t) + e * this._t;
                this._context.lineTo(n, this._y), this._context.lineTo(n, t);
              }
          }
          this._x = e, this._y = t;
        }
      }, e.arc = f, e.area = y, e.areaRadial = x, e.curveBasis = H, e.curveBasisClosed = z, e
      .curveBasisOpen = G, e.curveBundle = ft, e.curveCardinal = ht, e.curveCardinalClosed = pt, e
      .curveCardinalOpen = mt, e.curveCatmullRom = vt, e.curveCatmullRomClosed = gt, e.curveCatmullRomOpen =
      yt, e.curveLinear = p, e.curveLinearClosed = ne, e.curveMonotoneX = le, e.curveMonotoneY = de, e
      .curveNatural = pe, e.curveStep = ve, e.curveStepAfter = ye, e.curveStepBefore = ge, e.line = g, e
      .lineRadial = C, e.linkHorizontal = D, e.linkRadial = P, e.linkVertical = R, e.pie = _, e
      .pointRadial = S, e.radialArea = x, e.radialLine = C, e.stack = $e, e.stackOffsetDiverging = Te, e
      .stackOffsetExpand = we, e.stackOffsetNone = be, e.stackOffsetSilhouette = Ce, e.stackOffsetWiggle =
      xe, e.stackOrderAppearance = Se, e.stackOrderAscending = Me, e.stackOrderDescending = Ne, e
      .stackOrderInsideOut = Ie, e.stackOrderNone = Ee, e.stackOrderReverse = Oe, e.symbol = L, e
      .symbolCircle = We, e.symbolCross = Ye, e.symbolDiamond = Qe, e.symbolSquare = rt, e.symbolStar = nt,
      e.symbolTriangle = ot, e.symbolWye = lt, e.symbols = dt, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
