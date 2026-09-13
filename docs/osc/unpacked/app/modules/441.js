// ─────────────────────────────────────────────────────────────
// APP MODULE 441
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, i) {
    i(t, n(83))
  }(this, function(e, t) {
    "use strict";

    function n(e) {
      return function() {
        return e
      }
    }

    function i(e) {
      return e > 1 ? 0 : e < -1 ? He : Math.acos(e)
    }

    function o(e) {
      return e >= 1 ? Be : e <= -1 ? -Be : Math.asin(e)
    }

    function r(e) {
      return e.innerRadius
    }

    function a(e) {
      return e.outerRadius
    }

    function l(e) {
      return e.startAngle
    }

    function s(e) {
      return e.endAngle
    }

    function d(e) {
      return e && e.padAngle
    }

    function c(e, t, n, i, o, r, a, l) {
      var s = n - e,
        d = i - t,
        c = a - o,
        u = l - r,
        f = u * s - c * d;
      if (!(f * f < Ve)) return f = (c * (t - r) - u * (e - o)) / f, [e + f * s, t + f * d]
    }

    function u(e, t, n, i, o, r, a) {
      var l = e - n,
        s = t - i,
        d = (a ? r : -r) / Ge(l * l + s * s),
        c = d * s,
        u = -d * l,
        f = e + c,
        m = t + u,
        g = n + c,
        p = i + u,
        h = (f + g) / 2,
        b = (m + p) / 2,
        x = g - f,
        v = p - m,
        y = x * x + v * v,
        w = o - r,
        S = f * p - g * m,
        E = (v < 0 ? -1 : 1) * Ge(Fe(0, w * w * y - S * S)),
        k = (S * v - x * E) / y,
        _ = (-S * x - v * E) / y,
        T = (S * v + x * E) / y,
        C = (-S * x + v * E) / y,
        O = k - h,
        A = _ - b,
        I = T - h,
        M = C - b;
      return O * O + A * A > I * I + M * M && (k = T, _ = C), {
        cx: k,
        cy: _,
        x01: -c,
        y01: -u,
        x11: k * (o / w - 1),
        y11: _ * (o / w - 1)
      }
    }

    function f() {
      function e() {
        var e, n, r = +f.apply(this, arguments),
          a = +m.apply(this, arguments),
          l = h.apply(this, arguments) - Be,
          s = b.apply(this, arguments) - Be,
          d = De(s - l),
          y = s > l;
        if (v || (v = e = t.path()), a < r && (n = a, a = r, r = n), a > Ve)
          if (d > Ye - Ve) v.moveTo(a * Le(l), a * ze(l)), v.arc(0, 0, a, l, s, !y), r > Ve && (v.moveTo(r * Le(s),
            r * ze(s)), v.arc(0, 0, r, s, l, y));
          else {
            var w, S, E = l,
              k = s,
              _ = l,
              T = s,
              C = d,
              O = d,
              A = x.apply(this, arguments) / 2,
              I = A > Ve && (p ? +p.apply(this, arguments) : Ge(r * r + a * a)),
              M = Ue(De(a - r) / 2, +g.apply(this, arguments)),
              R = M,
              P = M;
            if (I > Ve) {
              var D = o(I / r * ze(A)),
                N = o(I / a * ze(A));
              (C -= 2 * D) > Ve ? (D *= y ? 1 : -1, _ += D, T -= D) : (C = 0, _ = T = (l + s) / 2), (O -= 2 * N) >
                Ve ? (N *= y ? 1 : -1, E += N, k -= N) : (O = 0, E = k = (l + s) / 2)
            }
            var L = a * Le(E),
              F = a * ze(E),
              U = r * Le(T),
              z = r * ze(T);
            if (M > Ve) {
              var G, V = a * Le(k),
                H = a * ze(k),
                B = r * Le(_),
                Y = r * ze(_);
              if (d < He && (G = c(L, F, B, Y, V, H, U, z))) {
                var $ = L - G[0],
                  W = F - G[1],
                  j = V - G[0],
                  K = H - G[1],
                  q = 1 / ze(i(($ * j + W * K) / (Ge($ * $ + W * W) * Ge(j * j + K * K))) / 2),
                  X = Ge(G[0] * G[0] + G[1] * G[1]);
                R = Ue(M, (r - X) / (q - 1)), P = Ue(M, (a - X) / (q + 1))
              }
            }
            O > Ve ? P > Ve ? (w = u(B, Y, L, F, a, P, y), S = u(V, H, U, z, a, P, y), v.moveTo(w.cx + w.x01, w.cy + w
              .y01), P < M ? v.arc(w.cx, w.cy, P, Ne(w.y01, w.x01), Ne(S.y01, S.x01), !y) : (v.arc(w.cx, w.cy, P,
              Ne(w.y01, w.x01), Ne(w.y11, w.x11), !y), v.arc(0, 0, a, Ne(w.cy + w.y11, w.cx + w.x11), Ne(S.cy +
              S.y11, S.cx + S.x11), !y), v.arc(S.cx, S.cy, P, Ne(S.y11, S.x11), Ne(S.y01, S.x01), !y))) : (v.moveTo(
              L, F), v.arc(0, 0, a, E, k, !y)) : v.moveTo(L, F), r > Ve && C > Ve ? R > Ve ? (w = u(U, z, V, H, r, -
              R, y), S = u(L, F, B, Y, r, -R, y), v.lineTo(w.cx + w.x01, w.cy + w.y01), R < M ? v.arc(w.cx, w.cy,
              R, Ne(w.y01, w.x01), Ne(S.y01, S.x01), !y) : (v.arc(w.cx, w.cy, R, Ne(w.y01, w.x01), Ne(w.y11, w
                .x11), !y), v.arc(0, 0, r, Ne(w.cy + w.y11, w.cx + w.x11), Ne(S.cy + S.y11, S.cx + S.x11), y), v
              .arc(S.cx, S.cy, R, Ne(S.y11, S.x11), Ne(S.y01, S.x01), !y))) : v.arc(0, 0, r, T, _, y) : v.lineTo(U,
              z)
          }
        else v.moveTo(0, 0);
        if (v.closePath(), e) return v = null, e + "" || null
      }
      var f = r,
        m = a,
        g = n(0),
        p = null,
        h = l,
        b = s,
        x = d,
        v = null;
      return e.centroid = function() {
        var e = (+f.apply(this, arguments) + +m.apply(this, arguments)) / 2,
          t = (+h.apply(this, arguments) + +b.apply(this, arguments)) / 2 - He / 2;
        return [Le(t) * e, ze(t) * e]
      }, e.innerRadius = function(t) {
        return arguments.length ? (f = "function" == typeof t ? t : n(+t), e) : f
      }, e.outerRadius = function(t) {
        return arguments.length ? (m = "function" == typeof t ? t : n(+t), e) : m
      }, e.cornerRadius = function(t) {
        return arguments.length ? (g = "function" == typeof t ? t : n(+t), e) : g
      }, e.padRadius = function(t) {
        return arguments.length ? (p = null == t ? null : "function" == typeof t ? t : n(+t), e) : p
      }, e.startAngle = function(t) {
        return arguments.length ? (h = "function" == typeof t ? t : n(+t), e) : h
      }, e.endAngle = function(t) {
        return arguments.length ? (b = "function" == typeof t ? t : n(+t), e) : b
      }, e.padAngle = function(t) {
        return arguments.length ? (x = "function" == typeof t ? t : n(+t), e) : x
      }, e.context = function(t) {
        return arguments.length ? (v = null == t ? null : t, e) : v
      }, e
    }

    function m(e) {
      this._context = e
    }

    function g(e) {
      return new m(e)
    }

    function p(e) {
      return e[0]
    }

    function h(e) {
      return e[1]
    }

    function b() {
      function e(e) {
        var n, d, c, u = e.length,
          f = !1;
        for (null == a && (s = l(c = t.path())), n = 0; n <= u; ++n) !(n < u && r(d = e[n], n, e)) === f && ((f = !
          f) ? s.lineStart() : s.lineEnd()), f && s.point(+i(d, n, e), +o(d, n, e));
        if (c) return s = null, c + "" || null
      }
      var i = p,
        o = h,
        r = n(!0),
        a = null,
        l = g,
        s = null;
      return e.x = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : n(+t), e) : i
      }, e.y = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : n(+t), e) : o
      }, e.defined = function(t) {
        return arguments.length ? (r = "function" == typeof t ? t : n(!!t), e) : r
      }, e.curve = function(t) {
        return arguments.length ? (l = t, null != a && (s = l(a)), e) : l
      }, e.context = function(t) {
        return arguments.length ? (null == t ? a = s = null : s = l(a = t), e) : a
      }, e
    }

    function x() {
      function e(e) {
        var n, i, f, m, g, p = e.length,
          h = !1,
          b = new Array(p),
          x = new Array(p);
        for (null == d && (u = c(g = t.path())), n = 0; n <= p; ++n) {
          if (!(n < p && s(m = e[n], n, e)) === h)
            if (h = !h) i = n, u.areaStart(), u.lineStart();
            else {
              for (u.lineEnd(), u.lineStart(), f = n - 1; f >= i; --f) u.point(b[f], x[f]);
              u.lineEnd(), u.areaEnd()
            } h && (b[n] = +o(m, n, e), x[n] = +a(m, n, e), u.point(r ? +r(m, n, e) : b[n], l ? +l(m, n, e) : x[n]))
        }
        if (g) return u = null, g + "" || null
      }

      function i() {
        return b().defined(s).curve(c).context(d)
      }
      var o = p,
        r = null,
        a = n(0),
        l = h,
        s = n(!0),
        d = null,
        c = g,
        u = null;
      return e.x = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : n(+t), r = null, e) : o
      }, e.x0 = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : n(+t), e) : o
      }, e.x1 = function(t) {
        return arguments.length ? (r = null == t ? null : "function" == typeof t ? t : n(+t), e) : r
      }, e.y = function(t) {
        return arguments.length ? (a = "function" == typeof t ? t : n(+t), l = null, e) : a
      }, e.y0 = function(t) {
        return arguments.length ? (a = "function" == typeof t ? t : n(+t), e) : a
      }, e.y1 = function(t) {
        return arguments.length ? (l = null == t ? null : "function" == typeof t ? t : n(+t), e) : l
      }, e.lineX0 = e.lineY0 = function() {
        return i().x(o).y(a)
      }, e.lineY1 = function() {
        return i().x(o).y(l)
      }, e.lineX1 = function() {
        return i().x(r).y(a)
      }, e.defined = function(t) {
        return arguments.length ? (s = "function" == typeof t ? t : n(!!t), e) : s
      }, e.curve = function(t) {
        return arguments.length ? (c = t, null != d && (u = c(d)), e) : c
      }, e.context = function(t) {
        return arguments.length ? (null == t ? d = u = null : u = c(d = t), e) : d
      }, e
    }

    function v(e, t) {
      return t < e ? -1 : t > e ? 1 : t >= e ? 0 : NaN
    }

    function y(e) {
      return e
    }

    function w() {
      function e(e) {
        var n, s, d, c, u, f = e.length,
          m = 0,
          g = new Array(f),
          p = new Array(f),
          h = +r.apply(this, arguments),
          b = Math.min(Ye, Math.max(-Ye, a.apply(this, arguments) - h)),
          x = Math.min(Math.abs(b) / f, l.apply(this, arguments)),
          v = x * (b < 0 ? -1 : 1);
        for (n = 0; n < f; ++n)(u = p[g[n] = n] = +t(e[n], n, e)) > 0 && (m += u);
        for (null != i ? g.sort(function(e, t) {
            return i(p[e], p[t])
          }) : null != o && g.sort(function(t, n) {
            return o(e[t], e[n])
          }), n = 0, d = m ? (b - f * v) / m : 0; n < f; ++n, h = c) s = g[n], u = p[s], c = h + (u > 0 ? u * d : 0) +
          v, p[s] = {
            data: e[s],
            index: n,
            value: u,
            startAngle: h,
            endAngle: c,
            padAngle: x
          };
        return p
      }
      var t = y,
        i = v,
        o = null,
        r = n(0),
        a = n(Ye),
        l = n(0);
      return e.value = function(i) {
        return arguments.length ? (t = "function" == typeof i ? i : n(+i), e) : t
      }, e.sortValues = function(t) {
        return arguments.length ? (i = t, o = null, e) : i
      }, e.sort = function(t) {
        return arguments.length ? (o = t, i = null, e) : o
      }, e.startAngle = function(t) {
        return arguments.length ? (r = "function" == typeof t ? t : n(+t), e) : r
      }, e.endAngle = function(t) {
        return arguments.length ? (a = "function" == typeof t ? t : n(+t), e) : a
      }, e.padAngle = function(t) {
        return arguments.length ? (l = "function" == typeof t ? t : n(+t), e) : l
      }, e
    }

    function S(e) {
      this._curve = e
    }

    function E(e) {
      function t(t) {
        return new S(e(t))
      }
      return t._curve = e, t
    }

    function k(e) {
      var t = e.curve;
      return e.angle = e.x, delete e.x, e.radius = e.y, delete e.y, e.curve = function(e) {
        return arguments.length ? t(E(e)) : t()._curve
      }, e
    }

    function _() {
      return k(b().curve($e))
    }

    function T() {
      var e = x().curve($e),
        t = e.curve,
        n = e.lineX0,
        i = e.lineX1,
        o = e.lineY0,
        r = e.lineY1;
      return e.angle = e.x, delete e.x, e.startAngle = e.x0, delete e.x0, e.endAngle = e.x1, delete e.x1, e.radius = e
        .y, delete e.y, e.innerRadius = e.y0, delete e.y0, e.outerRadius = e.y1, delete e.y1, e.lineStartAngle =
        function() {
          return k(n())
        }, delete e.lineX0, e.lineEndAngle = function() {
          return k(i())
        }, delete e.lineX1, e.lineInnerRadius = function() {
          return k(o())
        }, delete e.lineY0, e.lineOuterRadius = function() {
          return k(r())
        }, delete e.lineY1, e.curve = function(e) {
          return arguments.length ? t(E(e)) : t()._curve
        }, e
    }

    function C(e, t) {
      return [(t = +t) * Math.cos(e -= Math.PI / 2), t * Math.sin(e)]
    }

    function O(e) {
      return e.source
    }

    function A(e) {
      return e.target
    }

    function I(e) {
      function i() {
        var n, i = We.call(arguments),
          d = o.apply(this, i),
          c = r.apply(this, i);
        if (s || (s = n = t.path()), e(s, +a.apply(this, (i[0] = d, i)), +l.apply(this, i), +a.apply(this, (i[0] = c,
            i)), +l.apply(this, i)), n) return s = null, n + "" || null
      }
      var o = O,
        r = A,
        a = p,
        l = h,
        s = null;
      return i.source = function(e) {
        return arguments.length ? (o = e, i) : o
      }, i.target = function(e) {
        return arguments.length ? (r = e, i) : r
      }, i.x = function(e) {
        return arguments.length ? (a = "function" == typeof e ? e : n(+e), i) : a
      }, i.y = function(e) {
        return arguments.length ? (l = "function" == typeof e ? e : n(+e), i) : l
      }, i.context = function(e) {
        return arguments.length ? (s = null == e ? null : e, i) : s
      }, i
    }

    function M(e, t, n, i, o) {
      e.moveTo(t, n), e.bezierCurveTo(t = (t + i) / 2, n, t, o, i, o)
    }

    function R(e, t, n, i, o) {
      e.moveTo(t, n), e.bezierCurveTo(t, n = (n + o) / 2, i, n, i, o)
    }

    function P(e, t, n, i, o) {
      var r = C(t, n),
        a = C(t, n = (n + o) / 2),
        l = C(i, n),
        s = C(i, o);
      e.moveTo(r[0], r[1]), e.bezierCurveTo(a[0], a[1], l[0], l[1], s[0], s[1])
    }

    function D() {
      return I(M)
    }

    function N() {
      return I(R)
    }

    function L() {
      var e = I(P);
      return e.angle = e.x, delete e.x, e.radius = e.y, delete e.y, e
    }

    function F() {
      function e() {
        var e;
        if (r || (r = e = t.path()), i.apply(this, arguments).draw(r, +o.apply(this, arguments)), e) return r = null,
          e + "" || null
      }
      var i = n(je),
        o = n(64),
        r = null;
      return e.type = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : n(t), e) : i
      }, e.size = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : n(+t), e) : o
      }, e.context = function(t) {
        return arguments.length ? (r = null == t ? null : t, e) : r
      }, e
    }

    function U() {}

    function z(e, t, n) {
      e._context.bezierCurveTo((2 * e._x0 + e._x1) / 3, (2 * e._y0 + e._y1) / 3, (e._x0 + 2 * e._x1) / 3, (e._y0 + 2 *
        e._y1) / 3, (e._x0 + 4 * e._x1 + t) / 6, (e._y0 + 4 * e._y1 + n) / 6)
    }

    function G(e) {
      this._context = e
    }

    function V(e) {
      return new G(e)
    }

    function H(e) {
      this._context = e
    }

    function B(e) {
      return new H(e)
    }

    function Y(e) {
      this._context = e
    }

    function $(e) {
      return new Y(e)
    }

    function W(e, t) {
      this._basis = new G(e), this._beta = t
    }

    function j(e, t, n) {
      e._context.bezierCurveTo(e._x1 + e._k * (e._x2 - e._x0), e._y1 + e._k * (e._y2 - e._y0), e._x2 + e._k * (e._x1 -
        t), e._y2 + e._k * (e._y1 - n), e._x2, e._y2)
    }

    function K(e, t) {
      this._context = e, this._k = (1 - t) / 6
    }

    function q(e, t) {
      this._context = e, this._k = (1 - t) / 6
    }

    function X(e, t) {
      this._context = e, this._k = (1 - t) / 6
    }

    function Z(e, t, n) {
      var i = e._x1,
        o = e._y1,
        r = e._x2,
        a = e._y2;
      if (e._l01_a > Ve) {
        var l = 2 * e._l01_2a + 3 * e._l01_a * e._l12_a + e._l12_2a,
          s = 3 * e._l01_a * (e._l01_a + e._l12_a);
        i = (i * l - e._x0 * e._l12_2a + e._x2 * e._l01_2a) / s, o = (o * l - e._y0 * e._l12_2a + e._y2 * e._l01_2a) /
          s
      }
      if (e._l23_a > Ve) {
        var d = 2 * e._l23_2a + 3 * e._l23_a * e._l12_a + e._l12_2a,
          c = 3 * e._l23_a * (e._l23_a + e._l12_a);
        r = (r * d + e._x1 * e._l23_2a - t * e._l12_2a) / c, a = (a * d + e._y1 * e._l23_2a - n * e._l12_2a) / c
      }
      e._context.bezierCurveTo(i, o, r, a, e._x2, e._y2)
    }

    function Q(e, t) {
      this._context = e, this._alpha = t
    }

    function J(e, t) {
      this._context = e, this._alpha = t
    }

    function ee(e, t) {
      this._context = e, this._alpha = t
    }

    function te(e) {
      this._context = e
    }

    function ne(e) {
      return new te(e)
    }

    function ie(e) {
      return e < 0 ? -1 : 1
    }

    function oe(e, t, n) {
      var i = e._x1 - e._x0,
        o = t - e._x1,
        r = (e._y1 - e._y0) / (i || o < 0 && -0),
        a = (n - e._y1) / (o || i < 0 && -0),
        l = (r * o + a * i) / (i + o);
      return (ie(r) + ie(a)) * Math.min(Math.abs(r), Math.abs(a), .5 * Math.abs(l)) || 0
    }

    function re(e, t) {
      var n = e._x1 - e._x0;
      return n ? (3 * (e._y1 - e._y0) / n - t) / 2 : t
    }

    function ae(e, t, n) {
      var i = e._x0,
        o = e._y0,
        r = e._x1,
        a = e._y1,
        l = (r - i) / 3;
      e._context.bezierCurveTo(i + l, o + l * t, r - l, a - l * n, r, a)
    }

    function le(e) {
      this._context = e
    }

    function se(e) {
      this._context = new de(e)
    }

    function de(e) {
      this._context = e
    }

    function ce(e) {
      return new le(e)
    }

    function ue(e) {
      return new se(e)
    }

    function fe(e) {
      this._context = e
    }

    function me(e) {
      var t, n, i = e.length - 1,
        o = new Array(i),
        r = new Array(i),
        a = new Array(i);
      for (o[0] = 0, r[0] = 2, a[0] = e[0] + 2 * e[1], t = 1; t < i - 1; ++t) o[t] = 1, r[t] = 4, a[t] = 4 * e[t] +
        2 * e[t + 1];
      for (o[i - 1] = 2, r[i - 1] = 7, a[i - 1] = 8 * e[i - 1] + e[i], t = 1; t < i; ++t) n = o[t] / r[t - 1], r[t] -=
        n, a[t] -= n * a[t - 1];
      for (o[i - 1] = a[i - 1] / r[i - 1], t = i - 2; t >= 0; --t) o[t] = (a[t] - o[t + 1]) / r[t];
      for (r[i - 1] = (e[i] + o[i - 1]) / 2, t = 0; t < i - 1; ++t) r[t] = 2 * e[t + 1] - o[t + 1];
      return [o, r]
    }

    function ge(e) {
      return new fe(e)
    }

    function pe(e, t) {
      this._context = e, this._t = t
    }

    function he(e) {
      return new pe(e, .5)
    }

    function be(e) {
      return new pe(e, 0)
    }

    function xe(e) {
      return new pe(e, 1)
    }

    function ve(e, t) {
      if ((o = e.length) > 1)
        for (var n, i, o, r = 1, a = e[t[0]], l = a.length; r < o; ++r)
          for (i = a, a = e[t[r]], n = 0; n < l; ++n) a[n][1] += a[n][0] = isNaN(i[n][1]) ? i[n][0] : i[n][1]
    }

    function ye(e) {
      for (var t = e.length, n = new Array(t); --t >= 0;) n[t] = t;
      return n
    }

    function we(e, t) {
      return e[t]
    }

    function Se() {
      function e(e) {
        var n, a, l = t.apply(this, arguments),
          s = e.length,
          d = l.length,
          c = new Array(d);
        for (n = 0; n < d; ++n) {
          for (var u, f = l[n], m = c[n] = new Array(s), g = 0; g < s; ++g) m[g] = u = [0, +r(e[g], f, g, e)], u
            .data = e[g];
          m.key = f
        }
        for (n = 0, a = i(c); n < d; ++n) c[a[n]].index = n;
        return o(c, a), c
      }
      var t = n([]),
        i = ye,
        o = ve,
        r = we;
      return e.keys = function(i) {
        return arguments.length ? (t = "function" == typeof i ? i : n(We.call(i)), e) : t
      }, e.value = function(t) {
        return arguments.length ? (r = "function" == typeof t ? t : n(+t), e) : r
      }, e.order = function(t) {
        return arguments.length ? (i = null == t ? ye : "function" == typeof t ? t : n(We.call(t)), e) : i
      }, e.offset = function(t) {
        return arguments.length ? (o = null == t ? ve : t, e) : o
      }, e
    }

    function Ee(e, t) {
      if ((i = e.length) > 0) {
        for (var n, i, o, r = 0, a = e[0].length; r < a; ++r) {
          for (o = n = 0; n < i; ++n) o += e[n][r][1] || 0;
          if (o)
            for (n = 0; n < i; ++n) e[n][r][1] /= o
        }
        ve(e, t)
      }
    }

    function ke(e, t) {
      if ((l = e.length) > 0)
        for (var n, i, o, r, a, l, s = 0, d = e[t[0]].length; s < d; ++s)
          for (r = a = 0, n = 0; n < l; ++n)(o = (i = e[t[n]][s])[1] - i[0]) > 0 ? (i[0] = r, i[1] = r += o) : o < 0 ?
            (i[1] = a, i[0] = a += o) : (i[0] = 0, i[1] = o)
    }

    function _e(e, t) {
      if ((n = e.length) > 0) {
        for (var n, i = 0, o = e[t[0]], r = o.length; i < r; ++i) {
          for (var a = 0, l = 0; a < n; ++a) l += e[a][i][1] || 0;
          o[i][1] += o[i][0] = -l / 2
        }
        ve(e, t)
      }
    }

    function Te(e, t) {
      if ((o = e.length) > 0 && (i = (n = e[t[0]]).length) > 0) {
        for (var n, i, o, r = 0, a = 1; a < i; ++a) {
          for (var l = 0, s = 0, d = 0; l < o; ++l) {
            for (var c = e[t[l]], u = c[a][1] || 0, f = c[a - 1][1] || 0, m = (u - f) / 2, g = 0; g < l; ++g) {
              var p = e[t[g]],
                h = p[a][1] || 0,
                b = p[a - 1][1] || 0;
              m += h - b
            }
            s += u, d += m * u
          }
          n[a - 1][1] += n[a - 1][0] = r, s && (r -= d / s)
        }
        n[a - 1][1] += n[a - 1][0] = r, ve(e, t)
      }
    }

    function Ce(e) {
      var t = e.map(Oe);
      return ye(e).sort(function(e, n) {
        return t[e] - t[n]
      })
    }

    function Oe(e) {
      for (var t, n = -1, i = 0, o = e.length, r = -(1 / 0); ++n < o;)(t = +e[n][1]) > r && (r = t, i = n);
      return i
    }

    function Ae(e) {
      var t = e.map(Ie);
      return ye(e).sort(function(e, n) {
        return t[e] - t[n]
      })
    }

    function Ie(e) {
      for (var t, n = 0, i = -1, o = e.length; ++i < o;)(t = +e[i][1]) && (n += t);
      return n
    }

    function Me(e) {
      return Ae(e).reverse()
    }

    function Re(e) {
      var t, n, i = e.length,
        o = e.map(Ie),
        r = Ce(e),
        a = 0,
        l = 0,
        s = [],
        d = [];
      for (t = 0; t < i; ++t) n = r[t], a < l ? (a += o[n], s.push(n)) : (l += o[n], d.push(n));
      return d.reverse().concat(s)
    }

    function Pe(e) {
      return ye(e).reverse()
    }
    var De = Math.abs,
      Ne = Math.atan2,
      Le = Math.cos,
      Fe = Math.max,
      Ue = Math.min,
      ze = Math.sin,
      Ge = Math.sqrt,
      Ve = 1e-12,
      He = Math.PI,
      Be = He / 2,
      Ye = 2 * He;
    m.prototype = {
      areaStart: function() {
        this._line = 0
      },
      areaEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._point = 0
      },
      lineEnd: function() {
        (this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line = 1 - this
          ._line
      },
      point: function(e, t) {
        switch (e = +e, t = +t, this._point) {
          case 0:
            this._point = 1, this._line ? this._context.lineTo(e, t) : this._context.moveTo(e, t);
            break;
          case 1:
            this._point = 2;
          default:
            this._context.lineTo(e, t)
        }
      }
    };
    var $e = E(g);
    S.prototype = {
      areaStart: function() {
        this._curve.areaStart()
      },
      areaEnd: function() {
        this._curve.areaEnd()
      },
      lineStart: function() {
        this._curve.lineStart()
      },
      lineEnd: function() {
        this._curve.lineEnd()
      },
      point: function(e, t) {
        this._curve.point(t * Math.sin(e), t * -Math.cos(e))
      }
    };
    var We = Array.prototype.slice,
      je = {
        draw: function(e, t) {
          var n = Math.sqrt(t / He);
          e.moveTo(n, 0), e.arc(0, 0, n, 0, Ye)
        }
      },
      Ke = {
        draw: function(e, t) {
          var n = Math.sqrt(t / 5) / 2;
          e.moveTo(-3 * n, -n), e.lineTo(-n, -n), e.lineTo(-n, -3 * n), e.lineTo(n, -3 * n), e.lineTo(n, -n), e
            .lineTo(3 * n, -n), e.lineTo(3 * n, n), e.lineTo(n, n), e.lineTo(n, 3 * n), e.lineTo(-n, 3 * n), e
            .lineTo(-n, n), e.lineTo(-3 * n, n), e.closePath()
        }
      },
      qe = Math.sqrt(1 / 3),
      Xe = 2 * qe,
      Ze = {
        draw: function(e, t) {
          var n = Math.sqrt(t / Xe),
            i = n * qe;
          e.moveTo(0, -n), e.lineTo(i, 0), e.lineTo(0, n), e.lineTo(-i, 0), e.closePath()
        }
      },
      Qe = .8908130915292852,
      Je = Math.sin(He / 10) / Math.sin(7 * He / 10),
      et = Math.sin(Ye / 10) * Je,
      tt = -Math.cos(Ye / 10) * Je,
      nt = {
        draw: function(e, t) {
          var n = Math.sqrt(t * Qe),
            i = et * n,
            o = tt * n;
          e.moveTo(0, -n), e.lineTo(i, o);
          for (var r = 1; r < 5; ++r) {
            var a = Ye * r / 5,
              l = Math.cos(a),
              s = Math.sin(a);
            e.lineTo(s * n, -l * n), e.lineTo(l * i - s * o, s * i + l * o)
          }
          e.closePath()
        }
      },
      it = {
        draw: function(e, t) {
          var n = Math.sqrt(t),
            i = -n / 2;
          e.rect(i, i, n, n)
        }
      },
      ot = Math.sqrt(3),
      rt = {
        draw: function(e, t) {
          var n = -Math.sqrt(t / (3 * ot));
          e.moveTo(0, 2 * n), e.lineTo(-ot * n, -n), e.lineTo(ot * n, -n), e.closePath()
        }
      },
      at = -.5,
      lt = Math.sqrt(3) / 2,
      st = 1 / Math.sqrt(12),
      dt = 3 * (st / 2 + 1),
      ct = {
        draw: function(e, t) {
          var n = Math.sqrt(t / dt),
            i = n / 2,
            o = n * st,
            r = i,
            a = n * st + n,
            l = -r,
            s = a;
          e.moveTo(i, o), e.lineTo(r, a), e.lineTo(l, s), e.lineTo(at * i - lt * o, lt * i + at * o), e.lineTo(at *
            r - lt * a, lt * r + at * a), e.lineTo(at * l - lt * s, lt * l + at * s), e.lineTo(at * i + lt * o,
            at * o - lt * i), e.lineTo(at * r + lt * a, at * a - lt * r), e.lineTo(at * l + lt * s, at * s - lt *
            l), e.closePath()
        }
      },
      ut = [je, Ke, Ze, it, nt, rt, ct];
    G.prototype = {
      areaStart: function() {
        this._line = 0
      },
      areaEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._x0 = this._x1 = this._y0 = this._y1 = NaN, this._point = 0
      },
      lineEnd: function() {
        switch (this._point) {
          case 3:
            z(this, this._x1, this._y1);
          case 2:
            this._context.lineTo(this._x1, this._y1)
        }(this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line = 1 - this
          ._line
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
            this._point = 3, this._context.lineTo((5 * this._x0 + this._x1) / 6, (5 * this._y0 + this._y1) / 6);
          default:
            z(this, e, t)
        }
        this._x0 = this._x1, this._x1 = e, this._y0 = this._y1, this._y1 = t
      }
    }, H.prototype = {
      areaStart: U,
      areaEnd: U,
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._x3 = this._x4 = this._y0 = this._y1 = this._y2 = this._y3 = this
          ._y4 = NaN, this._point = 0
      },
      lineEnd: function() {
        switch (this._point) {
          case 1:
            this._context.moveTo(this._x2, this._y2), this._context.closePath();
            break;
          case 2:
            this._context.moveTo((this._x2 + 2 * this._x3) / 3, (this._y2 + 2 * this._y3) / 3), this._context
              .lineTo((this._x3 + 2 * this._x2) / 3, (this._y3 + 2 * this._y2) / 3), this._context.closePath();
            break;
          case 3:
            this.point(this._x2, this._y2), this.point(this._x3, this._y3), this.point(this._x4, this._y4)
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
            this._point = 3, this._x4 = e, this._y4 = t, this._context.moveTo((this._x0 + 4 * this._x1 + e) / 6, (
              this._y0 + 4 * this._y1 + t) / 6);
            break;
          default:
            z(this, e, t)
        }
        this._x0 = this._x1, this._x1 = e, this._y0 = this._y1, this._y1 = t
      }
    }, Y.prototype = {
      areaStart: function() {
        this._line = 0
      },
      areaEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._x0 = this._x1 = this._y0 = this._y1 = NaN, this._point = 0
      },
      lineEnd: function() {
        (this._line || 0 !== this._line && 3 === this._point) && this._context.closePath(), this._line = 1 - this
          ._line
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
              i = (this._y0 + 4 * this._y1 + t) / 6;
            this._line ? this._context.lineTo(n, i) : this._context.moveTo(n, i);
            break;
          case 3:
            this._point = 4;
          default:
            z(this, e, t)
        }
        this._x0 = this._x1, this._x1 = e, this._y0 = this._y1, this._y1 = t
      }
    }, W.prototype = {
      lineStart: function() {
        this._x = [], this._y = [], this._basis.lineStart()
      },
      lineEnd: function() {
        var e = this._x,
          t = this._y,
          n = e.length - 1;
        if (n > 0)
          for (var i, o = e[0], r = t[0], a = e[n] - o, l = t[n] - r, s = -1; ++s <= n;) i = s / n, this._basis
            .point(this._beta * e[s] + (1 - this._beta) * (o + i * a), this._beta * t[s] + (1 - this._beta) * (r +
              i * l));
        this._x = this._y = null, this._basis.lineEnd()
      },
      point: function(e, t) {
        this._x.push(+e), this._y.push(+t)
      }
    };
    var ft = function e(t) {
      function n(e) {
        return 1 === t ? new G(e) : new W(e, t)
      }
      return n.beta = function(t) {
        return e(+t)
      }, n
    }(.85);
    K.prototype = {
      areaStart: function() {
        this._line = 0
      },
      areaEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._y0 = this._y1 = this._y2 = NaN, this._point = 0
      },
      lineEnd: function() {
        switch (this._point) {
          case 2:
            this._context.lineTo(this._x2, this._y2);
            break;
          case 3:
            j(this, this._x1, this._y1)
        }(this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line = 1 - this
          ._line
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
            j(this, e, t)
        }
        this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this._y2, this
          ._y2 = t
      }
    };
    var mt = function e(t) {
      function n(e) {
        return new K(e, t)
      }
      return n.tension = function(t) {
        return e(+t)
      }, n
    }(0);
    q.prototype = {
      areaStart: U,
      areaEnd: U,
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._x3 = this._x4 = this._x5 = this._y0 = this._y1 = this._y2 = this
          ._y3 = this._y4 = this._y5 = NaN, this._point = 0
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
            this.point(this._x3, this._y3), this.point(this._x4, this._y4), this.point(this._x5, this._y5)
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
            j(this, e, t)
        }
        this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this._y2, this
          ._y2 = t
      }
    };
    var gt = function e(t) {
      function n(e) {
        return new q(e, t)
      }
      return n.tension = function(t) {
        return e(+t)
      }, n
    }(0);
    X.prototype = {
      areaStart: function() {
        this._line = 0
      },
      areaEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._y0 = this._y1 = this._y2 = NaN, this._point = 0
      },
      lineEnd: function() {
        (this._line || 0 !== this._line && 3 === this._point) && this._context.closePath(), this._line = 1 - this
          ._line
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
            this._point = 3, this._line ? this._context.lineTo(this._x2, this._y2) : this._context.moveTo(this
              ._x2, this._y2);
            break;
          case 3:
            this._point = 4;
          default:
            j(this, e, t)
        }
        this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this._y2, this
          ._y2 = t
      }
    };
    var pt = function e(t) {
      function n(e) {
        return new X(e, t)
      }
      return n.tension = function(t) {
        return e(+t)
      }, n
    }(0);
    Q.prototype = {
      areaStart: function() {
        this._line = 0
      },
      areaEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._y0 = this._y1 = this._y2 = NaN, this._l01_a = this._l12_a = this
          ._l23_a = this._l01_2a = this._l12_2a = this._l23_2a = this._point = 0
      },
      lineEnd: function() {
        switch (this._point) {
          case 2:
            this._context.lineTo(this._x2, this._y2);
            break;
          case 3:
            this.point(this._x2, this._y2)
        }(this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line = 1 - this
          ._line
      },
      point: function(e, t) {
        if (e = +e, t = +t, this._point) {
          var n = this._x2 - e,
            i = this._y2 - t;
          this._l23_a = Math.sqrt(this._l23_2a = Math.pow(n * n + i * i, this._alpha))
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
            Z(this, e, t)
        }
        this._l01_a = this._l12_a, this._l12_a = this._l23_a, this._l01_2a = this._l12_2a, this._l12_2a = this
          ._l23_2a, this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this
          ._y2, this._y2 = t
      }
    };
    var ht = function e(t) {
      function n(e) {
        return t ? new Q(e, t) : new K(e, 0)
      }
      return n.alpha = function(t) {
        return e(+t)
      }, n
    }(.5);
    J.prototype = {
      areaStart: U,
      areaEnd: U,
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._x3 = this._x4 = this._x5 = this._y0 = this._y1 = this._y2 = this
          ._y3 = this._y4 = this._y5 = NaN, this._l01_a = this._l12_a = this._l23_a = this._l01_2a = this
          ._l12_2a = this._l23_2a = this._point = 0
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
            this.point(this._x3, this._y3), this.point(this._x4, this._y4), this.point(this._x5, this._y5)
        }
      },
      point: function(e, t) {
        if (e = +e, t = +t, this._point) {
          var n = this._x2 - e,
            i = this._y2 - t;
          this._l23_a = Math.sqrt(this._l23_2a = Math.pow(n * n + i * i, this._alpha))
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
            Z(this, e, t)
        }
        this._l01_a = this._l12_a, this._l12_a = this._l23_a, this._l01_2a = this._l12_2a, this._l12_2a = this
          ._l23_2a, this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this
          ._y2, this._y2 = t
      }
    };
    var bt = function e(t) {
      function n(e) {
        return t ? new J(e, t) : new q(e, 0)
      }
      return n.alpha = function(t) {
        return e(+t)
      }, n
    }(.5);
    ee.prototype = {
      areaStart: function() {
        this._line = 0
      },
      areaEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._x0 = this._x1 = this._x2 = this._y0 = this._y1 = this._y2 = NaN, this._l01_a = this._l12_a = this
          ._l23_a = this._l01_2a = this._l12_2a = this._l23_2a = this._point = 0
      },
      lineEnd: function() {
        (this._line || 0 !== this._line && 3 === this._point) && this._context.closePath(), this._line = 1 - this
          ._line
      },
      point: function(e, t) {
        if (e = +e, t = +t, this._point) {
          var n = this._x2 - e,
            i = this._y2 - t;
          this._l23_a = Math.sqrt(this._l23_2a = Math.pow(n * n + i * i, this._alpha))
        }
        switch (this._point) {
          case 0:
            this._point = 1;
            break;
          case 1:
            this._point = 2;
            break;
          case 2:
            this._point = 3, this._line ? this._context.lineTo(this._x2, this._y2) : this._context.moveTo(this
              ._x2, this._y2);
            break;
          case 3:
            this._point = 4;
          default:
            Z(this, e, t)
        }
        this._l01_a = this._l12_a, this._l12_a = this._l23_a, this._l01_2a = this._l12_2a, this._l12_2a = this
          ._l23_2a, this._x0 = this._x1, this._x1 = this._x2, this._x2 = e, this._y0 = this._y1, this._y1 = this
          ._y2, this._y2 = t
      }
    };
    var xt = function e(t) {
      function n(e) {
        return t ? new ee(e, t) : new X(e, 0)
      }
      return n.alpha = function(t) {
        return e(+t)
      }, n
    }(.5);
    te.prototype = {
        areaStart: U,
        areaEnd: U,
        lineStart: function() {
          this._point = 0
        },
        lineEnd: function() {
          this._point && this._context.closePath()
        },
        point: function(e, t) {
          e = +e, t = +t, this._point ? this._context.lineTo(e, t) : (this._point = 1, this._context.moveTo(e, t))
        }
      }, le.prototype = {
        areaStart: function() {
          this._line = 0
        },
        areaEnd: function() {
          this._line = NaN
        },
        lineStart: function() {
          this._x0 = this._x1 = this._y0 = this._y1 = this._t0 = NaN, this._point = 0
        },
        lineEnd: function() {
          switch (this._point) {
            case 2:
              this._context.lineTo(this._x1, this._y1);
              break;
            case 3:
              ae(this, this._t0, re(this, this._t0))
          }(this._line || 0 !== this._line && 1 === this._point) && this._context.closePath(), this._line = 1 - this
            ._line
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
                this._point = 3, ae(this, re(this, n = oe(this, e, t)), n);
                break;
              default:
                ae(this, this._t0, n = oe(this, e, t))
            }
            this._x0 = this._x1, this._x1 = e, this._y0 = this._y1, this._y1 = t, this._t0 = n
          }
        }
      }, (se.prototype = Object.create(le.prototype)).point = function(e, t) {
        le.prototype.point.call(this, t, e)
      }, de.prototype = {
        moveTo: function(e, t) {
          this._context.moveTo(t, e)
        },
        closePath: function() {
          this._context.closePath()
        },
        lineTo: function(e, t) {
          this._context.lineTo(t, e)
        },
        bezierCurveTo: function(e, t, n, i, o, r) {
          this._context.bezierCurveTo(t, e, i, n, r, o)
        }
      }, fe.prototype = {
        areaStart: function() {
          this._line = 0
        },
        areaEnd: function() {
          this._line = NaN
        },
        lineStart: function() {
          this._x = [], this._y = []
        },
        lineEnd: function() {
          var e = this._x,
            t = this._y,
            n = e.length;
          if (n)
            if (this._line ? this._context.lineTo(e[0], t[0]) : this._context.moveTo(e[0], t[0]), 2 === n) this
              ._context.lineTo(e[1], t[1]);
            else
              for (var i = me(e), o = me(t), r = 0, a = 1; a < n; ++r, ++a) this._context.bezierCurveTo(i[0][r], o[
                0][r], i[1][r], o[1][r], e[a], t[a]);
          (this._line || 0 !== this._line && 1 === n) && this._context.closePath(), this._line = 1 - this._line,
            this._x = this._y = null
        },
        point: function(e, t) {
          this._x.push(+e), this._y.push(+t)
        }
      }, pe.prototype = {
        areaStart: function() {
          this._line = 0
        },
        areaEnd: function() {
          this._line = NaN
        },
        lineStart: function() {
          this._x = this._y = NaN, this._point = 0
        },
        lineEnd: function() {
          0 < this._t && this._t < 1 && 2 === this._point && this._context.lineTo(this._x, this._y), (this._line ||
            0 !== this._line && 1 === this._point) && this._context.closePath(), this._line >= 0 && (this._t = 1 -
            this._t, this._line = 1 - this._line)
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
                this._context.lineTo(n, this._y), this._context.lineTo(n, t)
              }
          }
          this._x = e, this._y = t
        }
      }, e.arc = f, e.area = x, e.areaRadial = T, e.curveBasis = V, e.curveBasisClosed = B, e.curveBasisOpen = $, e
      .curveBundle = ft, e.curveCardinal = mt, e.curveCardinalClosed = gt, e.curveCardinalOpen = pt, e
      .curveCatmullRom = ht, e.curveCatmullRomClosed = bt, e.curveCatmullRomOpen = xt, e.curveLinear = g, e
      .curveLinearClosed = ne, e.curveMonotoneX = ce, e.curveMonotoneY = ue, e.curveNatural = ge, e.curveStep = he, e
      .curveStepAfter = xe, e.curveStepBefore = be, e.line = b, e.lineRadial = _, e.linkHorizontal = D, e.linkRadial =
      L, e.linkVertical = N, e.pie = w, e.pointRadial = C, e.radialArea = T, e.radialLine = _, e.stack = Se, e
      .stackOffsetDiverging = ke, e.stackOffsetExpand = Ee, e.stackOffsetNone = ve, e.stackOffsetSilhouette = _e, e
      .stackOffsetWiggle = Te, e.stackOrderAppearance = Ce, e.stackOrderAscending = Ae, e.stackOrderDescending = Me, e
      .stackOrderInsideOut = Re, e.stackOrderNone = ye, e.stackOrderReverse = Pe, e.symbol = F, e.symbolCircle = je, e
      .symbolCross = Ke, e.symbolDiamond = Ze, e.symbolSquare = it, e.symbolStar = nt, e.symbolTriangle = rt, e
      .symbolWye = ct, e.symbols = ut, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
