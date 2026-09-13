// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 246
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, r) {
    r(t, n(30))
  }(this, function(e, t) {
    "use strict";

    function n() {
      return new r
    }

    function r() {
      this.reset()
    }

    function i(e, t, n) {
      var r = e.s = t + n,
        i = r - t,
        o = r - i;
      e.t = t - o + (n - i)
    }

    function o(e) {
      return e > 1 ? 0 : e < -1 ? Ln : Math.acos(e)
    }

    function a(e) {
      return e > 1 ? Un : e < -1 ? -Un : Math.asin(e)
    }

    function s(e) {
      return (e = Qn(e / 2)) * e
    }

    function c() {}

    function u(e, t) {
      e && nr.hasOwnProperty(e.type) && nr[e.type](e, t)
    }

    function l(e, t, n) {
      var r, i = -1,
        o = e.length - n;
      for (t.lineStart(); ++i < o;) r = e[i], t.point(r[0], r[1], r[2]);
      t.lineEnd()
    }

    function d(e, t) {
      var n = -1,
        r = e.length;
      for (t.polygonStart(); ++n < r;) l(e[n], t, 1);
      t.polygonEnd()
    }

    function f(e, t) {
      e && tr.hasOwnProperty(e.type) ? tr[e.type](e, t) : u(e, t)
    }

    function h() {
      or.point = m
    }

    function p() {
      v(nn, rn)
    }

    function m(e, t) {
      or.point = v, nn = e, rn = t, e *= Bn, t *= Bn, on = e, an = Vn(t = t / 2 + Fn), sn = Qn(t)
    }

    function v(e, t) {
      e *= Bn, t *= Bn, t = t / 2 + Fn;
      var n = e - on,
        r = n >= 0 ? 1 : -1,
        i = r * n,
        o = Vn(t),
        a = Qn(t),
        s = sn * a,
        c = an * o + s * Vn(i),
        u = s * r * Qn(i);
      rr.add(Gn(u, c)), on = e, an = o, sn = a
    }

    function g(e) {
      return ir.reset(), f(e, or), 2 * ir
    }

    function y(e) {
      return [Gn(e[1], e[0]), a(e[2])]
    }

    function b(e) {
      var t = e[0],
        n = e[1],
        r = Vn(n);
      return [r * Vn(t), r * Qn(t), Qn(n)]
    }

    function E(e, t) {
      return e[0] * t[0] + e[1] * t[1] + e[2] * t[2]
    }

    function _(e, t) {
      return [e[1] * t[2] - e[2] * t[1], e[2] * t[0] - e[0] * t[2], e[0] * t[1] - e[1] * t[0]]
    }

    function $(e, t) {
      e[0] += t[0], e[1] += t[1], e[2] += t[2]
    }

    function w(e, t) {
      return [e[0] * t, e[1] * t, e[2] * t]
    }

    function T(e) {
      var t = Zn(e[0] * e[0] + e[1] * e[1] + e[2] * e[2]);
      e[0] /= t, e[1] /= t, e[2] /= t
    }

    function C(e, t) {
      vn.push(gn = [cn = e, ln = e]), t < un && (un = t), t > dn && (dn = t)
    }

    function x(e, t) {
      var n = b([e * Bn, t * Bn]);
      if (mn) {
        var r = _(mn, n),
          i = [r[1], -r[0], 0],
          o = _(i, r);
        T(o), o = y(o);
        var a, s = e - fn,
          c = s > 0 ? 1 : -1,
          u = o[0] * Hn * c,
          l = zn(s) > 180;
        l ^ (c * fn < u && u < c * e) ? (a = o[1] * Hn, a > dn && (dn = a)) : (u = (u + 360) % 360 - 180, l ^ (c *
            fn < u && u < c * e) ? (a = -o[1] * Hn, a < un && (un = a)) : (t < un && (un = t), t > dn && (dn = t))),
          l ? e < fn ? I(cn, e) > I(cn, ln) && (ln = e) : I(e, ln) > I(cn, ln) && (cn = e) : ln >= cn ? (e < cn && (
            cn = e), e > ln && (ln = e)) : e > fn ? I(cn, e) > I(cn, ln) && (ln = e) : I(e, ln) > I(cn, ln) && (cn =
            e)
      } else vn.push(gn = [cn = e, ln = e]);
      t < un && (un = t), t > dn && (dn = t), mn = n, fn = e
    }

    function S() {
      sr.point = x
    }

    function A() {
      gn[0] = cn, gn[1] = ln, sr.point = C, mn = null
    }

    function M(e, t) {
      if (mn) {
        var n = e - fn;
        ar.add(zn(n) > 180 ? n + (n > 0 ? 360 : -360) : n)
      } else hn = e, pn = t;
      or.point(e, t), x(e, t)
    }

    function k() {
      or.lineStart()
    }

    function N() {
      M(hn, pn), or.lineEnd(), zn(ar) > Rn && (cn = -(ln = 180)), gn[0] = cn, gn[1] = ln, mn = null
    }

    function I(e, t) {
      return (t -= e) < 0 ? t + 360 : t
    }

    function O(e, t) {
      return e[0] - t[0]
    }

    function D(e, t) {
      return e[0] <= e[1] ? e[0] <= t && t <= e[1] : t < e[0] || e[1] < t
    }

    function R(e) {
      var t, n, r, i, o, a, s;
      if (dn = ln = -(cn = un = 1 / 0), vn = [], f(e, sr), n = vn.length) {
        for (vn.sort(O), t = 1, r = vn[0], o = [r]; t < n; ++t) i = vn[t], D(r, i[0]) || D(r, i[1]) ? (I(r[0], i[1]) >
          I(r[0], r[1]) && (r[1] = i[1]), I(i[0], r[1]) > I(r[0], r[1]) && (r[0] = i[0])) : o.push(r = i);
        for (a = -(1 / 0), n = o.length - 1, t = 0, r = o[n]; t <= n; r = i, ++t) i = o[t], (s = I(r[1], i[0])) > a &&
          (a = s, cn = i[0], ln = r[1])
      }
      return vn = gn = null, cn === 1 / 0 || un === 1 / 0 ? [
        [NaN, NaN],
        [NaN, NaN]
      ] : [
        [cn, un],
        [ln, dn]
      ]
    }

    function P(e, t) {
      e *= Bn, t *= Bn;
      var n = Vn(t);
      L(n * Vn(e), n * Qn(e), Qn(t))
    }

    function L(e, t, n) {
      ++yn, En += (e - En) / yn, _n += (t - _n) / yn, $n += (n - $n) / yn
    }

    function U() {
      cr.point = F
    }

    function F(e, t) {
      e *= Bn, t *= Bn;
      var n = Vn(t);
      Nn = n * Vn(e), In = n * Qn(e), On = Qn(t), cr.point = j, L(Nn, In, On)
    }

    function j(e, t) {
      e *= Bn, t *= Bn;
      var n = Vn(t),
        r = n * Vn(e),
        i = n * Qn(e),
        o = Qn(t),
        a = Gn(Zn((a = In * o - On * i) * a + (a = On * r - Nn * o) * a + (a = Nn * i - In * r) * a), Nn * r + In *
          i + On * o);
      bn += a, wn += a * (Nn + (Nn = r)), Tn += a * (In + (In = i)), Cn += a * (On + (On = o)), L(Nn, In, On)
    }

    function H() {
      cr.point = P
    }

    function B() {
      cr.point = q
    }

    function z() {
      G(Mn, kn), cr.point = P
    }

    function q(e, t) {
      Mn = e, kn = t, e *= Bn, t *= Bn, cr.point = G;
      var n = Vn(t);
      Nn = n * Vn(e), In = n * Qn(e), On = Qn(t), L(Nn, In, On)
    }

    function G(e, t) {
      e *= Bn, t *= Bn;
      var n = Vn(t),
        r = n * Vn(e),
        i = n * Qn(e),
        o = Qn(t),
        s = In * o - On * i,
        c = On * r - Nn * o,
        u = Nn * i - In * r,
        l = Zn(s * s + c * c + u * u),
        d = a(l),
        f = l && -d / l;
      xn += f * s, Sn += f * c, An += f * u, bn += d, wn += d * (Nn + (Nn = r)), Tn += d * (In + (In = i)), Cn += d *
        (On + (On = o)), L(Nn, In, On)
    }

    function V(e) {
      yn = bn = En = _n = $n = wn = Tn = Cn = xn = Sn = An = 0, f(e, cr);
      var t = xn,
        n = Sn,
        r = An,
        i = t * t + n * n + r * r;
      return i < Pn && (t = wn, n = Tn, r = Cn, bn < Rn && (t = En, n = _n, r = $n), i = t * t + n * n + r * r, i <
        Pn) ? [NaN, NaN] : [Gn(n, t) * Hn, a(r / Zn(i)) * Hn]
    }

    function W(e) {
      return function() {
        return e
      }
    }

    function Y(e, t) {
      function n(n, r) {
        return n = e(n, r), t(n[0], n[1])
      }
      return e.invert && t.invert && (n.invert = function(n, r) {
        return n = t.invert(n, r), n && e.invert(n[0], n[1])
      }), n
    }

    function K(e, t) {
      return [zn(e) > Ln ? e + Math.round(-e / jn) * jn : e, t]
    }

    function X(e, t, n) {
      return (e %= jn) ? t || n ? Y(J(e), Z(t, n)) : J(e) : t || n ? Z(t, n) : K
    }

    function Q(e) {
      return function(t, n) {
        return t += e, [t > Ln ? t - jn : t < -Ln ? t + jn : t, n]
      }
    }

    function J(e) {
      var t = Q(e);
      return t.invert = Q(-e), t
    }

    function Z(e, t) {
      function n(e, t) {
        var n = Vn(t),
          c = Vn(e) * n,
          u = Qn(e) * n,
          l = Qn(t),
          d = l * r + c * i;
        return [Gn(u * o - d * s, c * r - l * i), a(d * o + u * s)]
      }
      var r = Vn(e),
        i = Qn(e),
        o = Vn(t),
        s = Qn(t);
      return n.invert = function(e, t) {
        var n = Vn(t),
          c = Vn(e) * n,
          u = Qn(e) * n,
          l = Qn(t),
          d = l * o - u * s;
        return [Gn(u * o + l * s, c * r + d * i), a(d * r - c * i)]
      }, n
    }

    function ee(e) {
      function t(t) {
        return t = e(t[0] * Bn, t[1] * Bn), t[0] *= Hn, t[1] *= Hn, t
      }
      return e = X(e[0] * Bn, e[1] * Bn, e.length > 2 ? e[2] * Bn : 0), t.invert = function(t) {
        return t = e.invert(t[0] * Bn, t[1] * Bn), t[0] *= Hn, t[1] *= Hn, t
      }, t
    }

    function te(e, t, n, r, i, o) {
      if (n) {
        var a = Vn(t),
          s = Qn(t),
          c = r * n;
        null == i ? (i = t + r * jn, o = t - c / 2) : (i = ne(a, i), o = ne(a, o), (r > 0 ? i < o : i > o) && (i +=
          r * jn));
        for (var u, l = i; r > 0 ? l > o : l < o; l -= c) u = y([a, -s * Vn(l), -s * Qn(l)]), e.point(u[0], u[1])
      }
    }

    function ne(e, t) {
      t = b(t), t[0] -= e, T(t);
      var n = o(-t[1]);
      return ((-t[2] < 0 ? -n : n) + jn - Rn) % jn
    }

    function re() {
      function e(e, t) {
        n.push(e = r(e, t)), e[0] *= Hn, e[1] *= Hn
      }

      function t() {
        var e = i.apply(this, arguments),
          t = o.apply(this, arguments) * Bn,
          c = a.apply(this, arguments) * Bn;
        return n = [], r = X(-e[0] * Bn, -e[1] * Bn, 0).invert, te(s, t, c, 1), e = {
          type: "Polygon",
          coordinates: [n]
        }, n = r = null, e
      }
      var n, r, i = W([0, 0]),
        o = W(90),
        a = W(6),
        s = {
          point: e
        };
      return t.center = function(e) {
        return arguments.length ? (i = "function" == typeof e ? e : W([+e[0], +e[1]]), t) : i
      }, t.radius = function(e) {
        return arguments.length ? (o = "function" == typeof e ? e : W(+e), t) : o
      }, t.precision = function(e) {
        return arguments.length ? (a = "function" == typeof e ? e : W(+e), t) : a
      }, t
    }

    function ie() {
      var e, t = [];
      return {
        point: function(t, n, r) {
          e.push([t, n, r])
        },
        lineStart: function() {
          t.push(e = [])
        },
        lineEnd: c,
        rejoin: function() {
          t.length > 1 && t.push(t.pop().concat(t.shift()))
        },
        result: function() {
          var n = t;
          return t = [], e = null, n
        }
      }
    }

    function oe(e, t) {
      return zn(e[0] - t[0]) < Rn && zn(e[1] - t[1]) < Rn
    }

    function ae(e, t, n, r) {
      this.x = e, this.z = t, this.o = n, this.e = r, this.v = !1, this.n = this.p = null
    }

    function se(e, t, n, r, i) {
      var o, a, s = [],
        c = [];
      if (e.forEach(function(e) {
          if (!((t = e.length - 1) <= 0)) {
            var t, n, r = e[0],
              a = e[t];
            if (oe(r, a)) {
              if (!r[2] && !a[2]) {
                for (i.lineStart(), o = 0; o < t; ++o) i.point((r = e[o])[0], r[1]);
                return void i.lineEnd()
              }
              a[0] += 2 * Rn
            }
            s.push(n = new ae(r, e, null, !0)), c.push(n.o = new ae(r, null, n, !1)), s.push(n = new ae(a, e, null,
              !1)), c.push(n.o = new ae(a, null, n, !0))
          }
        }), s.length) {
        for (c.sort(t), ce(s), ce(c), o = 0, a = c.length; o < a; ++o) c[o].e = n = !n;
        for (var u, l, d = s[0];;) {
          for (var f = d, h = !0; f.v;)
            if ((f = f.n) === d) return;
          u = f.z, i.lineStart();
          do {
            if (f.v = f.o.v = !0, f.e) {
              if (h)
                for (o = 0, a = u.length; o < a; ++o) i.point((l = u[o])[0], l[1]);
              else r(f.x, f.n.x, 1, i);
              f = f.n
            } else {
              if (h)
                for (u = f.p.z, o = u.length - 1; o >= 0; --o) i.point((l = u[o])[0], l[1]);
              else r(f.x, f.p.x, -1, i);
              f = f.p
            }
            f = f.o, u = f.z, h = !h
          } while (!f.v);
          i.lineEnd()
        }
      }
    }

    function ce(e) {
      if (t = e.length) {
        for (var t, n, r = 0, i = e[0]; ++r < t;) i.n = n = e[r], n.p = i, i = n;
        i.n = n = e[0], n.p = i
      }
    }

    function ue(e) {
      return zn(e[0]) <= Ln ? e[0] : Jn(e[0]) * ((zn(e[0]) + Ln) % jn - Ln)
    }

    function le(e, t) {
      var n = ue(t),
        r = t[1],
        i = Qn(r),
        o = [Qn(n), -Vn(n), 0],
        s = 0,
        c = 0;
      Er.reset(), 1 === i ? r = Un + Rn : i === -1 && (r = -Un - Rn);
      for (var u = 0, l = e.length; u < l; ++u)
        if (f = (d = e[u]).length)
          for (var d, f, h = d[f - 1], p = ue(h), m = h[1] / 2 + Fn, v = Qn(m), g = Vn(m), y = 0; y < f; ++y, p = $,
            v = C, g = x, h = E) {
            var E = d[y],
              $ = ue(E),
              w = E[1] / 2 + Fn,
              C = Qn(w),
              x = Vn(w),
              S = $ - p,
              A = S >= 0 ? 1 : -1,
              M = A * S,
              k = M > Ln,
              N = v * C;
            if (Er.add(Gn(N * A * Qn(M), g * x + N * Vn(M))), s += k ? S + A * jn : S, k ^ p >= n ^ $ >= n) {
              var I = _(b(h), b(E));
              T(I);
              var O = _(o, I);
              T(O);
              var D = (k ^ S >= 0 ? -1 : 1) * a(O[2]);
              (r > D || r === D && (I[0] || I[1])) && (c += k ^ S >= 0 ? 1 : -1)
            }
          }
      return (s < -Rn || s < Rn && Er < -Rn) ^ 1 & c
    }

    function de(e, n, r, i) {
      return function(o) {
        function a(t, n) {
          e(t, n) && o.point(t, n)
        }

        function s(e, t) {
          v.point(e, t)
        }

        function c() {
          E.point = s, v.lineStart()
        }

        function u() {
          E.point = a, v.lineEnd()
        }

        function l(e, t) {
          m.push([e, t]), y.point(e, t)
        }

        function d() {
          y.lineStart(), m = []
        }

        function f() {
          l(m[0][0], m[0][1]), y.lineEnd();
          var e, t, n, r, i = y.clean(),
            a = g.result(),
            s = a.length;
          if (m.pop(), h.push(m), m = null, s)
            if (1 & i) {
              if (n = a[0], (t = n.length - 1) > 0) {
                for (b || (o.polygonStart(), b = !0), o.lineStart(), e = 0; e < t; ++e) o.point((r = n[e])[0], r[
                1]);
                o.lineEnd()
              }
            } else s > 1 && 2 & i && a.push(a.pop().concat(a.shift())), p.push(a.filter(fe))
        }
        var h, p, m, v = n(o),
          g = ie(),
          y = n(g),
          b = !1,
          E = {
            point: a,
            lineStart: c,
            lineEnd: u,
            polygonStart: function() {
              E.point = l, E.lineStart = d, E.lineEnd = f, p = [], h = []
            },
            polygonEnd: function() {
              E.point = a, E.lineStart = c, E.lineEnd = u, p = t.merge(p);
              var e = le(h, i);
              p.length ? (b || (o.polygonStart(), b = !0), se(p, he, e, r, o)) : e && (b || (o.polygonStart(),
                  b = !0), o.lineStart(), r(null, null, 1, o), o.lineEnd()), b && (o.polygonEnd(), b = !1), p =
                h = null
            },
            sphere: function() {
              o.polygonStart(), o.lineStart(), r(null, null, 1, o), o.lineEnd(), o.polygonEnd()
            }
          };
        return E
      }
    }

    function fe(e) {
      return e.length > 1
    }

    function he(e, t) {
      return ((e = e.x)[0] < 0 ? e[1] - Un - Rn : Un - e[1]) - ((t = t.x)[0] < 0 ? t[1] - Un - Rn : Un - t[1])
    }

    function pe(e) {
      var t, n = NaN,
        r = NaN,
        i = NaN;
      return {
        lineStart: function() {
          e.lineStart(), t = 1
        },
        point: function(o, a) {
          var s = o > 0 ? Ln : -Ln,
            c = zn(o - n);
          zn(c - Ln) < Rn ? (e.point(n, r = (r + a) / 2 > 0 ? Un : -Un), e.point(i, r), e.lineEnd(), e.lineStart(),
            e.point(s, r), e.point(o, r), t = 0) : i !== s && c >= Ln && (zn(n - i) < Rn && (n -= i * Rn), zn(o -
            s) < Rn && (o -= s * Rn), r = me(n, r, o, a), e.point(i, r), e.lineEnd(), e.lineStart(), e.point(s,
            r), t = 0), e.point(n = o, r = a), i = s
        },
        lineEnd: function() {
          e.lineEnd(), n = r = NaN
        },
        clean: function() {
          return 2 - t
        }
      }
    }

    function me(e, t, n, r) {
      var i, o, a = Qn(e - n);
      return zn(a) > Rn ? qn((Qn(t) * (o = Vn(r)) * Qn(n) - Qn(r) * (i = Vn(t)) * Qn(e)) / (i * o * a)) : (t + r) / 2
    }

    function ve(e, t, n, r) {
      var i;
      if (null == e) i = n * Un, r.point(-Ln, i), r.point(0, i), r.point(Ln, i), r.point(Ln, 0), r.point(Ln, -i), r
        .point(0, -i), r.point(-Ln, -i), r.point(-Ln, 0), r.point(-Ln, i);
      else if (zn(e[0] - t[0]) > Rn) {
        var o = e[0] < t[0] ? Ln : -Ln;
        i = n * o / 2, r.point(-o, i), r.point(0, i), r.point(o, i)
      } else r.point(t[0], t[1])
    }

    function ge(e) {
      function t(t, n, r, i) {
        te(i, e, s, r, t, n)
      }

      function n(e, t) {
        return Vn(e) * Vn(t) > a
      }

      function r(e) {
        var t, r, a, s, l;
        return {
          lineStart: function() {
            s = a = !1, l = 1
          },
          point: function(d, f) {
            var h, p = [d, f],
              m = n(d, f),
              v = c ? m ? 0 : o(d, f) : m ? o(d + (d < 0 ? Ln : -Ln), f) : 0;
            if (!t && (s = a = m) && e.lineStart(), m !== a && (h = i(t, p), (!h || oe(t, h) || oe(p, h)) && (p[2] =
                1)), m !== a) l = 0, m ? (e.lineStart(), h = i(p, t), e.point(h[0], h[1])) : (h = i(t, p), e.point(
              h[0], h[1], 2), e.lineEnd()), t = h;
            else if (u && t && c ^ m) {
              var g;
              v & r || !(g = i(p, t, !0)) || (l = 0, c ? (e.lineStart(), e.point(g[0][0], g[0][1]), e.point(g[1][0],
                g[1][1]), e.lineEnd()) : (e.point(g[1][0], g[1][1]), e.lineEnd(), e.lineStart(), e.point(g[0][
                0], g[0][1], 3)))
            }!m || t && oe(t, p) || e.point(p[0], p[1]), t = p, a = m, r = v
          },
          lineEnd: function() {
            a && e.lineEnd(), t = null
          },
          clean: function() {
            return l | (s && a) << 1
          }
        }
      }

      function i(e, t, n) {
        var r = b(e),
          i = b(t),
          o = [1, 0, 0],
          s = _(r, i),
          c = E(s, s),
          u = s[0],
          l = c - u * u;
        if (!l) return !n && e;
        var d = a * c / l,
          f = -a * u / l,
          h = _(o, s),
          p = w(o, d),
          m = w(s, f);
        $(p, m);
        var v = h,
          g = E(p, v),
          T = E(v, v),
          C = g * g - T * (E(p, p) - 1);
        if (!(C < 0)) {
          var x = Zn(C),
            S = w(v, (-g - x) / T);
          if ($(S, p), S = y(S), !n) return S;
          var A, M = e[0],
            k = t[0],
            N = e[1],
            I = t[1];
          k < M && (A = M, M = k, k = A);
          var O = k - M,
            D = zn(O - Ln) < Rn,
            R = D || O < Rn;
          if (!D && I < N && (A = N, N = I, I = A), R ? D ? N + I > 0 ^ S[1] < (zn(S[0] - M) < Rn ? N : I) : N <= S[
            1] && S[1] <= I : O > Ln ^ (M <= S[0] && S[0] <= k)) {
            var P = w(v, (-g + x) / T);
            return $(P, p), [S, y(P)]
          }
        }
      }

      function o(t, n) {
        var r = c ? e : Ln - e,
          i = 0;
        return t < -r ? i |= 1 : t > r && (i |= 2), n < -r ? i |= 4 : n > r && (i |= 8), i
      }
      var a = Vn(e),
        s = 6 * Bn,
        c = a > 0,
        u = zn(a) > Rn;
      return de(n, r, t, c ? [0, -e] : [-Ln, e - Ln])
    }

    function ye(e, t, n, r, i, o) {
      var a, s = e[0],
        c = e[1],
        u = t[0],
        l = t[1],
        d = 0,
        f = 1,
        h = u - s,
        p = l - c;
      if (a = n - s, h || !(a > 0)) {
        if (a /= h, h < 0) {
          if (a < d) return;
          a < f && (f = a)
        } else if (h > 0) {
          if (a > f) return;
          a > d && (d = a)
        }
        if (a = i - s, h || !(a < 0)) {
          if (a /= h, h < 0) {
            if (a > f) return;
            a > d && (d = a)
          } else if (h > 0) {
            if (a < d) return;
            a < f && (f = a)
          }
          if (a = r - c, p || !(a > 0)) {
            if (a /= p, p < 0) {
              if (a < d) return;
              a < f && (f = a)
            } else if (p > 0) {
              if (a > f) return;
              a > d && (d = a)
            }
            if (a = o - c, p || !(a < 0)) {
              if (a /= p, p < 0) {
                if (a > f) return;
                a > d && (d = a)
              } else if (p > 0) {
                if (a < d) return;
                a < f && (f = a)
              }
              return d > 0 && (e[0] = s + d * h, e[1] = c + d * p), f < 1 && (t[0] = s + f * h, t[1] = c + f * p), !0
            }
          }
        }
      }
    }

    function be(e, n, r, i) {
      function o(t, o) {
        return e <= t && t <= r && n <= o && o <= i
      }

      function a(t, o, a, c) {
        var l = 0,
          d = 0;
        if (null == t || (l = s(t, a)) !== (d = s(o, a)) || u(t, o) < 0 ^ a > 0) {
          do c.point(0 === l || 3 === l ? e : r, l > 1 ? i : n); while ((l = (l + a + 4) % 4) !== d)
        } else c.point(o[0], o[1])
      }

      function s(t, i) {
        return zn(t[0] - e) < Rn ? i > 0 ? 0 : 3 : zn(t[0] - r) < Rn ? i > 0 ? 2 : 1 : zn(t[1] - n) < Rn ? i > 0 ? 1 :
          0 : i > 0 ? 3 : 2
      }

      function c(e, t) {
        return u(e.x, t.x)
      }

      function u(e, t) {
        var n = s(e, 1),
          r = s(t, 1);
        return n !== r ? n - r : 0 === n ? t[1] - e[1] : 1 === n ? e[0] - t[0] : 2 === n ? e[1] - t[1] : t[0] - e[0]
      }
      return function(s) {
        function u(e, t) {
          o(e, t) && S.point(e, t)
        }

        function l() {
          for (var t = 0, n = 0, r = g.length; n < r; ++n)
            for (var o, a, s = g[n], c = 1, u = s.length, l = s[0], d = l[0], f = l[1]; c < u; ++c) o = d, a = f,
              l = s[c], d = l[0], f = l[1], a <= i ? f > i && (d - o) * (i - a) > (f - a) * (e - o) && ++t : f <=
              i && (d - o) * (i - a) < (f - a) * (e - o) && --t;
          return t
        }

        function d() {
          S = A, v = [], g = [], x = !0
        }

        function f() {
          var e = l(),
            n = x && e,
            r = (v = t.merge(v)).length;
          (n || r) && (s.polygonStart(), n && (s.lineStart(), a(null, null, 1, s), s.lineEnd()), r && se(v, c, e, a,
            s), s.polygonEnd()), S = s, v = g = y = null
        }

        function h() {
          M.point = m, g && g.push(y = []), C = !0, T = !1, $ = w = NaN
        }

        function p() {
          v && (m(b, E), _ && T && A.rejoin(), v.push(A.result())), M.point = u, T && S.lineEnd()
        }

        function m(t, a) {
          var s = o(t, a);
          if (g && y.push([t, a]), C) b = t, E = a, _ = s, C = !1, s && (S.lineStart(), S.point(t, a));
          else if (s && T) S.point(t, a);
          else {
            var c = [$ = Math.max(wr, Math.min($r, $)), w = Math.max(wr, Math.min($r, w))],
              u = [t = Math.max(wr, Math.min($r, t)), a = Math.max(wr, Math.min($r, a))];
            ye(c, u, e, n, r, i) ? (T || (S.lineStart(), S.point(c[0], c[1])), S.point(u[0], u[1]), s || S
            .lineEnd(), x = !1) : s && (S.lineStart(), S.point(t, a), x = !1)
          }
          $ = t, w = a, T = s
        }
        var v, g, y, b, E, _, $, w, T, C, x, S = s,
          A = ie(),
          M = {
            point: u,
            lineStart: h,
            lineEnd: p,
            polygonStart: d,
            polygonEnd: f
          };
        return M
      }
    }

    function Ee() {
      var e, t, n, r = 0,
        i = 0,
        o = 960,
        a = 500;
      return n = {
        stream: function(n) {
          return e && t === n ? e : e = be(r, i, o, a)(t = n)
        },
        extent: function(s) {
          return arguments.length ? (r = +s[0][0], i = +s[0][1], o = +s[1][0], a = +s[1][1], e = t = null, n) : [
            [r, i],
            [o, a]
          ]
        }
      }
    }

    function _e() {
      Cr.point = we, Cr.lineEnd = $e
    }

    function $e() {
      Cr.point = Cr.lineEnd = c
    }

    function we(e, t) {
      e *= Bn, t *= Bn, ur = e, lr = Qn(t), dr = Vn(t), Cr.point = Te
    }

    function Te(e, t) {
      e *= Bn, t *= Bn;
      var n = Qn(t),
        r = Vn(t),
        i = zn(e - ur),
        o = Vn(i),
        a = Qn(i),
        s = r * a,
        c = dr * n - lr * r * o,
        u = lr * n + dr * r * o;
      Tr.add(Gn(Zn(s * s + c * c), u)), ur = e, lr = n, dr = r
    }

    function Ce(e) {
      return Tr.reset(), f(e, Cr), +Tr
    }

    function xe(e, t) {
      return xr[0] = e, xr[1] = t, Ce(Sr)
    }

    function Se(e, t) {
      return !(!e || !Mr.hasOwnProperty(e.type)) && Mr[e.type](e, t)
    }

    function Ae(e, t) {
      return 0 === xe(e, t)
    }

    function Me(e, t) {
      for (var n, r, i, o = 0, a = e.length; o < a; o++) {
        if (r = xe(e[o], t), 0 === r) return !0;
        if (o > 0 && (i = xe(e[o], e[o - 1]), i > 0 && n <= i && r <= i && (n + r - i) * (1 - Math.pow((n - r) / i,
            2)) < Pn * i)) return !0;
        n = r
      }
      return !1
    }

    function ke(e, t) {
      return !!le(e.map(Ne), Ie(t))
    }

    function Ne(e) {
      return e = e.map(Ie), e.pop(), e
    }

    function Ie(e) {
      return [e[0] * Bn, e[1] * Bn]
    }

    function Oe(e, t) {
      return (e && Ar.hasOwnProperty(e.type) ? Ar[e.type] : Se)(e, t)
    }

    function De(e, n, r) {
      var i = t.range(e, n - Rn, r).concat(n);
      return function(e) {
        return i.map(function(t) {
          return [e, t]
        })
      }
    }

    function Re(e, n, r) {
      var i = t.range(e, n - Rn, r).concat(n);
      return function(e) {
        return i.map(function(t) {
          return [t, e]
        })
      }
    }

    function Pe() {
      function e() {
        return {
          type: "MultiLineString",
          coordinates: n()
        }
      }

      function n() {
        return t.range(Wn(a / g) * g, o, g).map(h).concat(t.range(Wn(l / y) * y, u, y).map(p)).concat(t.range(Wn(i /
          m) * m, r, m).filter(function(e) {
          return zn(e % g) > Rn
        }).map(d)).concat(t.range(Wn(c / v) * v, s, v).filter(function(e) {
          return zn(e % y) > Rn
        }).map(f))
      }
      var r, i, o, a, s, c, u, l, d, f, h, p, m = 10,
        v = m,
        g = 90,
        y = 360,
        b = 2.5;
      return e.lines = function() {
        return n().map(function(e) {
          return {
            type: "LineString",
            coordinates: e
          }
        })
      }, e.outline = function() {
        return {
          type: "Polygon",
          coordinates: [h(a).concat(p(u).slice(1), h(o).reverse().slice(1), p(l).reverse().slice(1))]
        }
      }, e.extent = function(t) {
        return arguments.length ? e.extentMajor(t).extentMinor(t) : e.extentMinor()
      }, e.extentMajor = function(t) {
        return arguments.length ? (a = +t[0][0], o = +t[1][0], l = +t[0][1], u = +t[1][1], a > o && (t = a, a = o,
          o = t), l > u && (t = l, l = u, u = t), e.precision(b)) : [
          [a, l],
          [o, u]
        ]
      }, e.extentMinor = function(t) {
        return arguments.length ? (i = +t[0][0], r = +t[1][0], c = +t[0][1], s = +t[1][1], i > r && (t = i, i = r,
          r = t), c > s && (t = c, c = s, s = t), e.precision(b)) : [
          [i, c],
          [r, s]
        ]
      }, e.step = function(t) {
        return arguments.length ? e.stepMajor(t).stepMinor(t) : e.stepMinor()
      }, e.stepMajor = function(t) {
        return arguments.length ? (g = +t[0], y = +t[1], e) : [g, y]
      }, e.stepMinor = function(t) {
        return arguments.length ? (m = +t[0], v = +t[1], e) : [m, v]
      }, e.precision = function(t) {
        return arguments.length ? (b = +t, d = De(c, s, 90), f = Re(i, r, b), h = De(l, u, 90), p = Re(a, o, b),
          e) : b
      }, e.extentMajor([
        [-180, -90 + Rn],
        [180, 90 - Rn]
      ]).extentMinor([
        [-180, -80 - Rn],
        [180, 80 + Rn]
      ])
    }

    function Le() {
      return Pe()()
    }

    function Ue(e, t) {
      var n = e[0] * Bn,
        r = e[1] * Bn,
        i = t[0] * Bn,
        o = t[1] * Bn,
        c = Vn(r),
        u = Qn(r),
        l = Vn(o),
        d = Qn(o),
        f = c * Vn(n),
        h = c * Qn(n),
        p = l * Vn(i),
        m = l * Qn(i),
        v = 2 * a(Zn(s(o - r) + c * l * s(i - n))),
        g = Qn(v),
        y = v ? function(e) {
          var t = Qn(e *= v) / g,
            n = Qn(v - e) / g,
            r = n * f + t * p,
            i = n * h + t * m,
            o = n * u + t * d;
          return [Gn(i, r) * Hn, Gn(o, Zn(r * r + i * i)) * Hn]
        } : function() {
          return [n * Hn, r * Hn]
        };
      return y.distance = v, y
    }

    function Fe(e) {
      return e
    }

    function je() {
      Ir.point = He
    }

    function He(e, t) {
      Ir.point = Be, fr = pr = e, hr = mr = t
    }

    function Be(e, t) {
      Nr.add(mr * e - pr * t), pr = e, mr = t
    }

    function ze() {
      Be(fr, hr)
    }

    function qe(e, t) {
      e < Or && (Or = e), e > Rr && (Rr = e), t < Dr && (Dr = t), t > Pr && (Pr = t)
    }

    function Ge(e, t) {
      Ur += e, Fr += t, ++jr
    }

    function Ve() {
      Wr.point = We
    }

    function We(e, t) {
      Wr.point = Ye, Ge(yr = e, br = t)
    }

    function Ye(e, t) {
      var n = e - yr,
        r = t - br,
        i = Zn(n * n + r * r);
      Hr += i * (yr + e) / 2, Br += i * (br + t) / 2, zr += i, Ge(yr = e, br = t)
    }

    function Ke() {
      Wr.point = Ge
    }

    function Xe() {
      Wr.point = Je
    }

    function Qe() {
      Ze(vr, gr)
    }

    function Je(e, t) {
      Wr.point = Ze, Ge(vr = yr = e, gr = br = t)
    }

    function Ze(e, t) {
      var n = e - yr,
        r = t - br,
        i = Zn(n * n + r * r);
      Hr += i * (yr + e) / 2, Br += i * (br + t) / 2, zr += i, i = br * e - yr * t, qr += i * (yr + e), Gr += i * (
        br + t), Vr += 3 * i, Ge(yr = e, br = t)
    }

    function et(e) {
      this._context = e
    }

    function tt(e, t) {
      ei.point = nt, Kr = Qr = e, Xr = Jr = t
    }

    function nt(e, t) {
      Qr -= e, Jr -= t, Zr.add(Zn(Qr * Qr + Jr * Jr)), Qr = e, Jr = t
    }

    function rt() {
      this._string = []
    }

    function it(e) {
      return "m0," + e + "a" + e + "," + e + " 0 1,1 0," + -2 * e + "a" + e + "," + e + " 0 1,1 0," + 2 * e + "z"
    }

    function ot(e, t) {
      function n(e) {
        return e && ("function" == typeof o && i.pointRadius(+o.apply(this, arguments)), f(e, r(i))), i.result()
      }
      var r, i, o = 4.5;
      return n.area = function(e) {
        return f(e, r(Ir)), Ir.result()
      }, n.measure = function(e) {
        return f(e, r(ei)), ei.result()
      }, n.bounds = function(e) {
        return f(e, r(Lr)), Lr.result()
      }, n.centroid = function(e) {
        return f(e, r(Wr)), Wr.result()
      }, n.projection = function(t) {
        return arguments.length ? (r = null == t ? (e = null, Fe) : (e = t).stream, n) : e
      }, n.context = function(e) {
        return arguments.length ? (i = null == e ? (t = null, new rt) : new et(t = e), "function" != typeof o && i
          .pointRadius(o), n) : t
      }, n.pointRadius = function(e) {
        return arguments.length ? (o = "function" == typeof e ? e : (i.pointRadius(+e), +e), n) : o
      }, n.projection(e).context(t)
    }

    function at(e) {
      return {
        stream: st(e)
      }
    }

    function st(e) {
      return function(t) {
        var n = new ct;
        for (var r in e) n[r] = e[r];
        return n.stream = t, n
      }
    }

    function ct() {}

    function ut(e, t, n) {
      var r = e.clipExtent && e.clipExtent();
      return e.scale(150).translate([0, 0]), null != r && e.clipExtent(null), f(n, e.stream(Lr)), t(Lr.result()),
        null != r && e.clipExtent(r), e
    }

    function lt(e, t, n) {
      return ut(e, function(n) {
        var r = t[1][0] - t[0][0],
          i = t[1][1] - t[0][1],
          o = Math.min(r / (n[1][0] - n[0][0]), i / (n[1][1] - n[0][1])),
          a = +t[0][0] + (r - o * (n[1][0] + n[0][0])) / 2,
          s = +t[0][1] + (i - o * (n[1][1] + n[0][1])) / 2;
        e.scale(150 * o).translate([a, s])
      }, n)
    }

    function dt(e, t, n) {
      return lt(e, [
        [0, 0], t
      ], n)
    }

    function ft(e, t, n) {
      return ut(e, function(n) {
        var r = +t,
          i = r / (n[1][0] - n[0][0]),
          o = (r - i * (n[1][0] + n[0][0])) / 2,
          a = -i * n[0][1];
        e.scale(150 * i).translate([o, a])
      }, n)
    }

    function ht(e, t, n) {
      return ut(e, function(n) {
        var r = +t,
          i = r / (n[1][1] - n[0][1]),
          o = -i * n[0][0],
          a = (r - i * (n[1][1] + n[0][1])) / 2;
        e.scale(150 * i).translate([o, a])
      }, n)
    }

    function pt(e, t) {
      return +t ? vt(e, t) : mt(e)
    }

    function mt(e) {
      return st({
        point: function(t, n) {
          t = e(t, n), this.stream.point(t[0], t[1])
        }
      })
    }

    function vt(e, t) {
      function n(r, i, o, s, c, u, l, d, f, h, p, m, v, g) {
        var y = l - r,
          b = d - i,
          E = y * y + b * b;
        if (E > 4 * t && v--) {
          var _ = s + h,
            $ = c + p,
            w = u + m,
            T = Zn(_ * _ + $ * $ + w * w),
            C = a(w /= T),
            x = zn(zn(w) - 1) < Rn || zn(o - f) < Rn ? (o + f) / 2 : Gn($, _),
            S = e(x, C),
            A = S[0],
            M = S[1],
            k = A - r,
            N = M - i,
            I = b * k - y * N;
          (I * I / E > t || zn((y * k + b * N) / E - .5) > .3 || s * h + c * p + u * m < ni) && (n(r, i, o, s, c, u,
            A, M, x, _ /= T, $ /= T, w, v, g), g.point(A, M), n(A, M, x, _, $, w, l, d, f, h, p, m, v, g))
        }
      }
      return function(t) {
        function r(n, r) {
          n = e(n, r), t.point(n[0], n[1])
        }

        function i() {
          g = NaN, w.point = o, t.lineStart()
        }

        function o(r, i) {
          var o = b([r, i]),
            a = e(r, i);
          n(g, y, v, E, _, $, g = a[0], y = a[1], v = r, E = o[0], _ = o[1], $ = o[2], ti, t), t.point(g, y)
        }

        function a() {
          w.point = r, t.lineEnd()
        }

        function s() {
          i(), w.point = c, w.lineEnd = u
        }

        function c(e, t) {
          o(l = e, t), d = g, f = y, h = E, p = _, m = $, w.point = o
        }

        function u() {
          n(g, y, v, E, _, $, d, f, l, h, p, m, ti, t), w.lineEnd = a, a()
        }
        var l, d, f, h, p, m, v, g, y, E, _, $, w = {
          point: r,
          lineStart: i,
          lineEnd: a,
          polygonStart: function() {
            t.polygonStart(), w.lineStart = s
          },
          polygonEnd: function() {
            t.polygonEnd(), w.lineStart = i
          }
        };
        return w
      }
    }

    function gt(e) {
      return st({
        point: function(t, n) {
          var r = e(t, n);
          return this.stream.point(r[0], r[1])
        }
      })
    }

    function yt(e, t, n, r, i) {
      function o(o, a) {
        return o *= r, a *= i, [t + e * o, n - e * a]
      }
      return o.invert = function(o, a) {
        return [(o - t) / e * r, (n - a) / e * i]
      }, o
    }

    function bt(e, t, n, r, i, o) {
      function a(e, o) {
        return e *= r, o *= i, [u * e - l * o + t, n - l * e - u * o]
      }
      var s = Vn(o),
        c = Qn(o),
        u = s * e,
        l = c * e,
        d = s / e,
        f = c / e,
        h = (c * n - s * t) / e,
        p = (c * t + s * n) / e;
      return a.invert = function(e, t) {
        return [r * (d * e - f * t + h), i * (p - f * e - d * t)]
      }, a
    }

    function Et(e) {
      return _t(function() {
        return e
      })()
    }

    function _t(e) {
      function t(e) {
        return f(e[0] * Bn, e[1] * Bn)
      }

      function n(e) {
        return e = f.invert(e[0], e[1]), e && [e[0] * Hn, e[1] * Hn]
      }

      function r() {
        var e = bt(m, 0, 0, T, C, w).apply(null, o(y, b)),
          t = (w ? bt : yt)(m, v - e[0], g - e[1], T, C, w);
        return a = X(E, _, $), d = Y(o, t), f = Y(a, d), l = pt(d, k), i()
      }

      function i() {
        return h = p = null, t
      }
      var o, a, s, c, u, l, d, f, h, p, m = 150,
        v = 480,
        g = 250,
        y = 0,
        b = 0,
        E = 0,
        _ = 0,
        $ = 0,
        w = 0,
        T = 1,
        C = 1,
        x = null,
        S = _r,
        A = null,
        M = Fe,
        k = .5;
      return t.stream = function(e) {
          return h && p === e ? h : h = ri(gt(a)(S(l(M(p = e)))))
        }, t.preclip = function(e) {
          return arguments.length ? (S = e, x = void 0, i()) : S
        }, t.postclip = function(e) {
          return arguments.length ? (M = e, A = s = c = u = null, i()) : M
        }, t.clipAngle = function(e) {
          return arguments.length ? (S = +e ? ge(x = e * Bn) : (x = null, _r), i()) : x * Hn
        }, t.clipExtent = function(e) {
          return arguments.length ? (M = null == e ? (A = s = c = u = null, Fe) : be(A = +e[0][0], s = +e[0][1], c = +
            e[1][0], u = +e[1][1]), i()) : null == A ? null : [
            [A, s],
            [c, u]
          ]
        }, t.scale = function(e) {
          return arguments.length ? (m = +e, r()) : m
        }, t.translate = function(e) {
          return arguments.length ? (v = +e[0], g = +e[1], r()) : [v, g]
        }, t.center = function(e) {
          return arguments.length ? (y = e[0] % 360 * Bn, b = e[1] % 360 * Bn, r()) : [y * Hn, b * Hn]
        }, t.rotate = function(e) {
          return arguments.length ? (E = e[0] % 360 * Bn, _ = e[1] % 360 * Bn, $ = e.length > 2 ? e[2] % 360 * Bn : 0,
            r()) : [E * Hn, _ * Hn, $ * Hn]
        }, t.angle = function(e) {
          return arguments.length ? (w = e % 360 * Bn, r()) : w * Hn
        }, t.reflectX = function(e) {
          return arguments.length ? (T = e ? -1 : 1, r()) : T < 0
        }, t.reflectY = function(e) {
          return arguments.length ? (C = e ? -1 : 1, r()) : C < 0
        }, t.precision = function(e) {
          return arguments.length ? (l = pt(d, k = e * e), i()) : Zn(k)
        }, t.fitExtent = function(e, n) {
          return lt(t, e, n)
        }, t.fitSize = function(e, n) {
          return dt(t, e, n)
        }, t.fitWidth = function(e, n) {
          return ft(t, e, n)
        }, t.fitHeight = function(e, n) {
          return ht(t, e, n)
        },
        function() {
          return o = e.apply(this, arguments), t.invert = o.invert && n, r()
        }
    }

    function $t(e) {
      var t = 0,
        n = Ln / 3,
        r = _t(e),
        i = r(t, n);
      return i.parallels = function(e) {
        return arguments.length ? r(t = e[0] * Bn, n = e[1] * Bn) : [t * Hn, n * Hn]
      }, i
    }

    function wt(e) {
      function t(e, t) {
        return [e * n, Qn(t) / n]
      }
      var n = Vn(e);
      return t.invert = function(e, t) {
        return [e / n, a(t * n)]
      }, t
    }

    function Tt(e, t) {
      function n(e, t) {
        var n = Zn(o - 2 * i * Qn(t)) / i;
        return [n * Qn(e *= i), s - n * Vn(e)]
      }
      var r = Qn(e),
        i = (r + Qn(t)) / 2;
      if (zn(i) < Rn) return wt(e);
      var o = 1 + r * (2 * i - r),
        s = Zn(o) / i;
      return n.invert = function(e, t) {
        var n = s - t,
          r = Gn(e, zn(n)) * Jn(n);
        return n * i < 0 && (r -= Ln * Jn(e) * Jn(n)), [r / i, a((o - (e * e + n * n) * i * i) / (2 * i))]
      }, n
    }

    function Ct() {
      return $t(Tt).scale(155.424).center([0, 33.6442])
    }

    function xt() {
      return Ct().parallels([29.5, 45.5]).scale(1070).translate([480, 250]).rotate([96, 0]).center([-.6, 38.7])
    }

    function St(e) {
      var t = e.length;
      return {
        point: function(n, r) {
          for (var i = -1; ++i < t;) e[i].point(n, r)
        },
        sphere: function() {
          for (var n = -1; ++n < t;) e[n].sphere()
        },
        lineStart: function() {
          for (var n = -1; ++n < t;) e[n].lineStart()
        },
        lineEnd: function() {
          for (var n = -1; ++n < t;) e[n].lineEnd()
        },
        polygonStart: function() {
          for (var n = -1; ++n < t;) e[n].polygonStart()
        },
        polygonEnd: function() {
          for (var n = -1; ++n < t;) e[n].polygonEnd()
        }
      }
    }

    function At() {
      function e(e) {
        var t = e[0],
          n = e[1];
        return s = null, i.point(t, n), s || (o.point(t, n), s) || (a.point(t, n), s)
      }

      function t() {
        return n = r = null, e
      }
      var n, r, i, o, a, s, c = xt(),
        u = Ct().rotate([154, 0]).center([-2, 58.5]).parallels([55, 65]),
        l = Ct().rotate([157, 0]).center([-3, 19.9]).parallels([8, 18]),
        d = {
          point: function(e, t) {
            s = [e, t]
          }
        };
      return e.invert = function(e) {
        var t = c.scale(),
          n = c.translate(),
          r = (e[0] - n[0]) / t,
          i = (e[1] - n[1]) / t;
        return (i >= .12 && i < .234 && r >= -.425 && r < -.214 ? u : i >= .166 && i < .234 && r >= -.214 && r < -
          .115 ? l : c).invert(e)
      }, e.stream = function(e) {
        return n && r === e ? n : n = St([c.stream(r = e), u.stream(e), l.stream(e)])
      }, e.precision = function(e) {
        return arguments.length ? (c.precision(e), u.precision(e), l.precision(e), t()) : c.precision()
      }, e.scale = function(t) {
        return arguments.length ? (c.scale(t), u.scale(.35 * t), l.scale(t), e.translate(c.translate())) : c.scale()
      }, e.translate = function(e) {
        if (!arguments.length) return c.translate();
        var n = c.scale(),
          r = +e[0],
          s = +e[1];
        return i = c.translate(e).clipExtent([
          [r - .455 * n, s - .238 * n],
          [r + .455 * n, s + .238 * n]
        ]).stream(d), o = u.translate([r - .307 * n, s + .201 * n]).clipExtent([
          [r - .425 * n + Rn, s + .12 * n + Rn],
          [r - .214 * n - Rn, s + .234 * n - Rn]
        ]).stream(d), a = l.translate([r - .205 * n, s + .212 * n]).clipExtent([
          [r - .214 * n + Rn, s + .166 * n + Rn],
          [r - .115 * n - Rn, s + .234 * n - Rn]
        ]).stream(d), t()
      }, e.fitExtent = function(t, n) {
        return lt(e, t, n)
      }, e.fitSize = function(t, n) {
        return dt(e, t, n)
      }, e.fitWidth = function(t, n) {
        return ft(e, t, n)
      }, e.fitHeight = function(t, n) {
        return ht(e, t, n)
      }, e.scale(1070)
    }

    function Mt(e) {
      return function(t, n) {
        var r = Vn(t),
          i = Vn(n),
          o = e(r * i);
        return [o * i * Qn(t), o * Qn(n)]
      }
    }

    function kt(e) {
      return function(t, n) {
        var r = Zn(t * t + n * n),
          i = e(r),
          o = Qn(i),
          s = Vn(i);
        return [Gn(t * o, r * s), a(r && n * o / r)]
      }
    }

    function Nt() {
      return Et(ii).scale(124.75).clipAngle(179.999)
    }

    function It() {
      return Et(oi).scale(79.4188).clipAngle(179.999)
    }

    function Ot(e, t) {
      return [e, Kn(er((Un + t) / 2))]
    }

    function Dt() {
      return Rt(Ot).scale(961 / jn)
    }

    function Rt(e) {
      function t() {
        var t = Ln * s(),
          a = o(ee(o.rotate()).invert([0, 0]));
        return u(null == l ? [
          [a[0] - t, a[1] - t],
          [a[0] + t, a[1] + t]
        ] : e === Ot ? [
          [Math.max(a[0] - t, l), n],
          [Math.min(a[0] + t, r), i]
        ] : [
          [l, Math.max(a[1] - t, n)],
          [r, Math.min(a[1] + t, i)]
        ])
      }
      var n, r, i, o = Et(e),
        a = o.center,
        s = o.scale,
        c = o.translate,
        u = o.clipExtent,
        l = null;
      return o.scale = function(e) {
        return arguments.length ? (s(e), t()) : s()
      }, o.translate = function(e) {
        return arguments.length ? (c(e), t()) : c()
      }, o.center = function(e) {
        return arguments.length ? (a(e), t()) : a()
      }, o.clipExtent = function(e) {
        return arguments.length ? (null == e ? l = n = r = i = null : (l = +e[0][0], n = +e[0][1], r = +e[1][0],
          i = +e[1][1]), t()) : null == l ? null : [
          [l, n],
          [r, i]
        ]
      }, t()
    }

    function Pt(e) {
      return er((Un + e) / 2)
    }

    function Lt(e, t) {
      function n(e, t) {
        o > 0 ? t < -Un + Rn && (t = -Un + Rn) : t > Un - Rn && (t = Un - Rn);
        var n = o / Xn(Pt(t), i);
        return [n * Qn(i * e), o - n * Vn(i * e)]
      }
      var r = Vn(e),
        i = e === t ? Qn(e) : Kn(r / Vn(t)) / Kn(Pt(t) / Pt(e)),
        o = r * Xn(Pt(e), i) / i;
      return i ? (n.invert = function(e, t) {
        var n = o - t,
          r = Jn(i) * Zn(e * e + n * n),
          a = Gn(e, zn(n)) * Jn(n);
        return n * i < 0 && (a -= Ln * Jn(e) * Jn(n)), [a / i, 2 * qn(Xn(o / r, 1 / i)) - Un]
      }, n) : Ot
    }

    function Ut() {
      return $t(Lt).scale(109.5).parallels([30, 30])
    }

    function Ft(e, t) {
      return [e, t]
    }

    function jt() {
      return Et(Ft).scale(152.63)
    }

    function Ht(e, t) {
      function n(e, t) {
        var n = o - t,
          r = i * e;
        return [n * Qn(r), o - n * Vn(r)]
      }
      var r = Vn(e),
        i = e === t ? Qn(e) : (r - Vn(t)) / (t - e),
        o = r / i + e;
      return zn(i) < Rn ? Ft : (n.invert = function(e, t) {
        var n = o - t,
          r = Gn(e, zn(n)) * Jn(n);
        return n * i < 0 && (r -= Ln * Jn(e) * Jn(n)), [r / i, o - Jn(i) * Zn(e * e + n * n)]
      }, n)
    }

    function Bt() {
      return $t(Ht).scale(131.154).center([0, 13.9389])
    }

    function zt(e, t) {
      var n = a(li * Qn(t)),
        r = n * n,
        i = r * r * r;
      return [e * Vn(n) / (li * (ai + 3 * si * r + i * (7 * ci + 9 * ui * r))), n * (ai + si * r + i * (ci + ui * r))]
    }

    function qt() {
      return Et(zt).scale(177.158)
    }

    function Gt(e, t) {
      var n = Vn(t),
        r = Vn(e) * n;
      return [n * Qn(e) / r, Qn(t) / r]
    }

    function Vt() {
      return Et(Gt).scale(144.049).clipAngle(60)
    }

    function Wt() {
      function e() {
        return v = u * f, g = u * h, s = c = null, t
      }

      function t(e) {
        var t = e[0] * v,
          i = e[1] * g;
        if (p) {
          var o = i * n - t * r;
          t = t * n + i * r, i = o
        }
        return [t + l, i + d]
      }
      var n, r, i, o, a, s, c, u = 1,
        l = 0,
        d = 0,
        f = 1,
        h = 1,
        p = 0,
        m = null,
        v = 1,
        g = 1,
        y = st({
          point: function(e, n) {
            var r = t([e, n]);
            this.stream.point(r[0], r[1])
          }
        }),
        b = Fe;
      return t.invert = function(e) {
        var t = e[0] - l,
          i = e[1] - d;
        if (p) {
          var o = i * n + t * r;
          t = t * n - i * r, i = o
        }
        return [t / v, i / g]
      }, t.stream = function(e) {
        return s && c === e ? s : s = y(b(c = e))
      }, t.postclip = function(t) {
        return arguments.length ? (b = t, m = i = o = a = null, e()) : b
      }, t.clipExtent = function(t) {
        return arguments.length ? (b = null == t ? (m = i = o = a = null, Fe) : be(m = +t[0][0], i = +t[0][1], o = +
          t[1][0], a = +t[1][1]), e()) : null == m ? null : [
          [m, i],
          [o, a]
        ]
      }, t.scale = function(t) {
        return arguments.length ? (u = +t, e()) : u
      }, t.translate = function(t) {
        return arguments.length ? (l = +t[0], d = +t[1], e()) : [l, d]
      }, t.angle = function(t) {
        return arguments.length ? (p = t % 360 * Bn, r = Qn(p), n = Vn(p), e()) : p * Hn
      }, t.reflectX = function(t) {
        return arguments.length ? (f = t ? -1 : 1, e()) : f < 0
      }, t.reflectY = function(t) {
        return arguments.length ? (h = t ? -1 : 1, e()) : h < 0
      }, t.fitExtent = function(e, n) {
        return lt(t, e, n)
      }, t.fitSize = function(e, n) {
        return dt(t, e, n)
      }, t.fitWidth = function(e, n) {
        return ft(t, e, n)
      }, t.fitHeight = function(e, n) {
        return ht(t, e, n)
      }, t
    }

    function Yt(e, t) {
      var n = t * t,
        r = n * n;
      return [e * (.8707 - .131979 * n + r * (-.013791 + r * (.003971 * n - .001529 * r))), t * (1.007226 + n * (
        .015085 + r * (-.044475 + .028874 * n - .005916 * r)))]
    }

    function Kt() {
      return Et(Yt).scale(175.295)
    }

    function Xt(e, t) {
      return [Vn(t) * Qn(e), Qn(t)]
    }

    function Qt() {
      return Et(Xt).scale(249.5).clipAngle(90 + Rn)
    }

    function Jt(e, t) {
      var n = Vn(t),
        r = 1 + Vn(e) * n;
      return [n * Qn(e) / r, Qn(t) / r]
    }

    function Zt() {
      return Et(Jt).scale(250).clipAngle(142)
    }

    function en(e, t) {
      return [Kn(er((Un + t) / 2)), -e]
    }

    function tn() {
      var e = Rt(en),
        t = e.center,
        n = e.rotate;
      return e.center = function(e) {
        return arguments.length ? t([-e[1], e[0]]) : (e = t(), [e[1], -e[0]])
      }, e.rotate = function(e) {
        return arguments.length ? n([e[0], e[1], e.length > 2 ? e[2] + 90 : 90]) : (e = n(), [e[0], e[1], e[2] -
          90])
      }, n([0, 0, 90]).scale(159.155)
    }
    r.prototype = {
      constructor: r,
      reset: function() {
        this.s = this.t = 0
      },
      add: function(e) {
        i(Dn, e, this.t), i(this, Dn.s, this.s), this.s ? this.t += Dn.t : this.s = Dn.t
      },
      valueOf: function() {
        return this.s
      }
    };
    var nn, rn, on, an, sn, cn, un, ln, dn, fn, hn, pn, mn, vn, gn, yn, bn, En, _n, $n, wn, Tn, Cn, xn, Sn, An, Mn,
      kn, Nn, In, On, Dn = new r,
      Rn = 1e-6,
      Pn = 1e-12,
      Ln = Math.PI,
      Un = Ln / 2,
      Fn = Ln / 4,
      jn = 2 * Ln,
      Hn = 180 / Ln,
      Bn = Ln / 180,
      zn = Math.abs,
      qn = Math.atan,
      Gn = Math.atan2,
      Vn = Math.cos,
      Wn = Math.ceil,
      Yn = Math.exp,
      Kn = Math.log,
      Xn = Math.pow,
      Qn = Math.sin,
      Jn = Math.sign || function(e) {
        return e > 0 ? 1 : e < 0 ? -1 : 0
      },
      Zn = Math.sqrt,
      er = Math.tan,
      tr = {
        Feature: function(e, t) {
          u(e.geometry, t)
        },
        FeatureCollection: function(e, t) {
          for (var n = e.features, r = -1, i = n.length; ++r < i;) u(n[r].geometry, t)
        }
      },
      nr = {
        Sphere: function(e, t) {
          t.sphere()
        },
        Point: function(e, t) {
          e = e.coordinates, t.point(e[0], e[1], e[2])
        },
        MultiPoint: function(e, t) {
          for (var n = e.coordinates, r = -1, i = n.length; ++r < i;) e = n[r], t.point(e[0], e[1], e[2])
        },
        LineString: function(e, t) {
          l(e.coordinates, t, 0)
        },
        MultiLineString: function(e, t) {
          for (var n = e.coordinates, r = -1, i = n.length; ++r < i;) l(n[r], t, 0)
        },
        Polygon: function(e, t) {
          d(e.coordinates, t)
        },
        MultiPolygon: function(e, t) {
          for (var n = e.coordinates, r = -1, i = n.length; ++r < i;) d(n[r], t)
        },
        GeometryCollection: function(e, t) {
          for (var n = e.geometries, r = -1, i = n.length; ++r < i;) u(n[r], t)
        }
      },
      rr = n(),
      ir = n(),
      or = {
        point: c,
        lineStart: c,
        lineEnd: c,
        polygonStart: function() {
          rr.reset(), or.lineStart = h, or.lineEnd = p
        },
        polygonEnd: function() {
          var e = +rr;
          ir.add(e < 0 ? jn + e : e), this.lineStart = this.lineEnd = this.point = c
        },
        sphere: function() {
          ir.add(jn)
        }
      },
      ar = n(),
      sr = {
        point: C,
        lineStart: S,
        lineEnd: A,
        polygonStart: function() {
          sr.point = M, sr.lineStart = k, sr.lineEnd = N, ar.reset(), or.polygonStart()
        },
        polygonEnd: function() {
          or.polygonEnd(), sr.point = C, sr.lineStart = S, sr.lineEnd = A, rr < 0 ? (cn = -(ln = 180), un = -(dn =
            90)) : ar > Rn ? dn = 90 : ar < -Rn && (un = -90), gn[0] = cn, gn[1] = ln
        },
        sphere: function() {
          cn = -(ln = 180), un = -(dn = 90)
        }
      },
      cr = {
        sphere: c,
        point: P,
        lineStart: U,
        lineEnd: H,
        polygonStart: function() {
          cr.lineStart = B, cr.lineEnd = z
        },
        polygonEnd: function() {
          cr.lineStart = U, cr.lineEnd = H
        }
      };
    K.invert = K;
    var ur, lr, dr, fr, hr, pr, mr, vr, gr, yr, br, Er = n(),
      _r = de(function() {
        return !0
      }, pe, ve, [-Ln, -Un]),
      $r = 1e9,
      wr = -$r,
      Tr = n(),
      Cr = {
        sphere: c,
        point: c,
        lineStart: _e,
        lineEnd: c,
        polygonStart: c,
        polygonEnd: c
      },
      xr = [null, null],
      Sr = {
        type: "LineString",
        coordinates: xr
      },
      Ar = {
        Feature: function(e, t) {
          return Se(e.geometry, t)
        },
        FeatureCollection: function(e, t) {
          for (var n = e.features, r = -1, i = n.length; ++r < i;)
            if (Se(n[r].geometry, t)) return !0;
          return !1
        }
      },
      Mr = {
        Sphere: function() {
          return !0
        },
        Point: function(e, t) {
          return Ae(e.coordinates, t)
        },
        MultiPoint: function(e, t) {
          for (var n = e.coordinates, r = -1, i = n.length; ++r < i;)
            if (Ae(n[r], t)) return !0;
          return !1
        },
        LineString: function(e, t) {
          return Me(e.coordinates, t)
        },
        MultiLineString: function(e, t) {
          for (var n = e.coordinates, r = -1, i = n.length; ++r < i;)
            if (Me(n[r], t)) return !0;
          return !1
        },
        Polygon: function(e, t) {
          return ke(e.coordinates, t)
        },
        MultiPolygon: function(e, t) {
          for (var n = e.coordinates, r = -1, i = n.length; ++r < i;)
            if (ke(n[r], t)) return !0;
          return !1
        },
        GeometryCollection: function(e, t) {
          for (var n = e.geometries, r = -1, i = n.length; ++r < i;)
            if (Se(n[r], t)) return !0;
          return !1
        }
      },
      kr = n(),
      Nr = n(),
      Ir = {
        point: c,
        lineStart: c,
        lineEnd: c,
        polygonStart: function() {
          Ir.lineStart = je, Ir.lineEnd = ze
        },
        polygonEnd: function() {
          Ir.lineStart = Ir.lineEnd = Ir.point = c, kr.add(zn(Nr)), Nr.reset()
        },
        result: function() {
          var e = kr / 2;
          return kr.reset(), e
        }
      },
      Or = 1 / 0,
      Dr = Or,
      Rr = -Or,
      Pr = Rr,
      Lr = {
        point: qe,
        lineStart: c,
        lineEnd: c,
        polygonStart: c,
        polygonEnd: c,
        result: function() {
          var e = [
            [Or, Dr],
            [Rr, Pr]
          ];
          return Rr = Pr = -(Dr = Or = 1 / 0), e
        }
      },
      Ur = 0,
      Fr = 0,
      jr = 0,
      Hr = 0,
      Br = 0,
      zr = 0,
      qr = 0,
      Gr = 0,
      Vr = 0,
      Wr = {
        point: Ge,
        lineStart: Ve,
        lineEnd: Ke,
        polygonStart: function() {
          Wr.lineStart = Xe, Wr.lineEnd = Qe
        },
        polygonEnd: function() {
          Wr.point = Ge, Wr.lineStart = Ve, Wr.lineEnd = Ke
        },
        result: function() {
          var e = Vr ? [qr / Vr, Gr / Vr] : zr ? [Hr / zr, Br / zr] : jr ? [Ur / jr, Fr / jr] : [NaN, NaN];
          return Ur = Fr = jr = Hr = Br = zr = qr = Gr = Vr = 0, e
        }
      };
    et.prototype = {
      _radius: 4.5,
      pointRadius: function(e) {
        return this._radius = e, this
      },
      polygonStart: function() {
        this._line = 0
      },
      polygonEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._point = 0
      },
      lineEnd: function() {
        0 === this._line && this._context.closePath(), this._point = NaN
      },
      point: function(e, t) {
        switch (this._point) {
          case 0:
            this._context.moveTo(e, t), this._point = 1;
            break;
          case 1:
            this._context.lineTo(e, t);
            break;
          default:
            this._context.moveTo(e + this._radius, t), this._context.arc(e, t, this._radius, 0, jn)
        }
      },
      result: c
    };
    var Yr, Kr, Xr, Qr, Jr, Zr = n(),
      ei = {
        point: c,
        lineStart: function() {
          ei.point = tt
        },
        lineEnd: function() {
          Yr && nt(Kr, Xr), ei.point = c
        },
        polygonStart: function() {
          Yr = !0
        },
        polygonEnd: function() {
          Yr = null
        },
        result: function() {
          var e = +Zr;
          return Zr.reset(), e
        }
      };
    rt.prototype = {
      _radius: 4.5,
      _circle: it(4.5),
      pointRadius: function(e) {
        return (e = +e) !== this._radius && (this._radius = e, this._circle = null), this
      },
      polygonStart: function() {
        this._line = 0
      },
      polygonEnd: function() {
        this._line = NaN
      },
      lineStart: function() {
        this._point = 0
      },
      lineEnd: function() {
        0 === this._line && this._string.push("Z"), this._point = NaN
      },
      point: function(e, t) {
        switch (this._point) {
          case 0:
            this._string.push("M", e, ",", t), this._point = 1;
            break;
          case 1:
            this._string.push("L", e, ",", t);
            break;
          default:
            null == this._circle && (this._circle = it(this._radius)), this._string.push("M", e, ",", t, this
              ._circle)
        }
      },
      result: function() {
        if (this._string.length) {
          var e = this._string.join("");
          return this._string = [], e
        }
        return null
      }
    }, ct.prototype = {
      constructor: ct,
      point: function(e, t) {
        this.stream.point(e, t)
      },
      sphere: function() {
        this.stream.sphere()
      },
      lineStart: function() {
        this.stream.lineStart()
      },
      lineEnd: function() {
        this.stream.lineEnd()
      },
      polygonStart: function() {
        this.stream.polygonStart()
      },
      polygonEnd: function() {
        this.stream.polygonEnd()
      }
    };
    var ti = 16,
      ni = Vn(30 * Bn),
      ri = st({
        point: function(e, t) {
          this.stream.point(e * Bn, t * Bn)
        }
      }),
      ii = Mt(function(e) {
        return Zn(2 / (1 + e))
      });
    ii.invert = kt(function(e) {
      return 2 * a(e / 2)
    });
    var oi = Mt(function(e) {
      return (e = o(e)) && e / Qn(e)
    });
    oi.invert = kt(function(e) {
      return e
    }), Ot.invert = function(e, t) {
      return [e, 2 * qn(Yn(t)) - Un]
    }, Ft.invert = Ft;
    var ai = 1.340264,
      si = -.081106,
      ci = 893e-6,
      ui = .003796,
      li = Zn(3) / 2,
      di = 12;
    zt.invert = function(e, t) {
        for (var n, r, i, o = t, s = o * o, c = s * s * s, u = 0; u < di && (r = o * (ai + si * s + c * (ci + ui *
            s)) - t, i = ai + 3 * si * s + c * (7 * ci + 9 * ui * s), o -= n = r / i, s = o * o, c = s * s * s, !(zn(
              n) < Pn)); ++u);
        return [li * e * (ai + 3 * si * s + c * (7 * ci + 9 * ui * s)) / Vn(o), a(Qn(o) / li)]
      }, Gt.invert = kt(qn), Yt.invert = function(e, t) {
        var n, r = t,
          i = 25;
        do {
          var o = r * r,
            a = o * o;
          r -= n = (r * (1.007226 + o * (.015085 + a * (-.044475 + .028874 * o - .005916 * a))) - t) / (1.007226 + o *
            (.045255 + a * (-.311325 + .259866 * o - .005916 * 11 * a)))
        } while (zn(n) > Rn && --i > 0);
        return [e / (.8707 + (o = r * r) * (-.131979 + o * (-.013791 + o * o * o * (.003971 - .001529 * o)))), r]
      }, Xt.invert = kt(a), Jt.invert = kt(function(e) {
        return 2 * qn(e)
      }), en.invert = function(e, t) {
        return [-t, 2 * qn(Yn(e)) - Un]
      }, e.geoAlbers = xt, e.geoAlbersUsa = At, e.geoArea = g, e.geoAzimuthalEqualArea = Nt, e
      .geoAzimuthalEqualAreaRaw = ii, e.geoAzimuthalEquidistant = It, e.geoAzimuthalEquidistantRaw = oi, e.geoBounds =
      R, e.geoCentroid = V, e.geoCircle = re, e.geoClipAntimeridian = _r, e.geoClipCircle = ge, e.geoClipExtent = Ee,
      e.geoClipRectangle = be, e.geoConicConformal = Ut, e.geoConicConformalRaw = Lt, e.geoConicEqualArea = Ct, e
      .geoConicEqualAreaRaw = Tt, e.geoConicEquidistant = Bt, e.geoConicEquidistantRaw = Ht, e.geoContains = Oe, e
      .geoDistance = xe, e.geoEqualEarth = qt, e.geoEqualEarthRaw = zt, e.geoEquirectangular = jt, e
      .geoEquirectangularRaw = Ft, e.geoGnomonic = Vt, e.geoGnomonicRaw = Gt, e.geoGraticule = Pe, e.geoGraticule10 =
      Le, e.geoIdentity = Wt, e.geoInterpolate = Ue, e.geoLength = Ce, e.geoMercator = Dt, e.geoMercatorRaw = Ot, e
      .geoNaturalEarth1 = Kt, e.geoNaturalEarth1Raw = Yt, e.geoOrthographic = Qt, e.geoOrthographicRaw = Xt, e
      .geoPath = ot, e.geoProjection = Et, e.geoProjectionMutator = _t, e.geoRotation = ee, e.geoStereographic = Zt, e
      .geoStereographicRaw = Jt, e.geoStream = f, e.geoTransform = at, e.geoTransverseMercator = tn, e
      .geoTransverseMercatorRaw = en, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
