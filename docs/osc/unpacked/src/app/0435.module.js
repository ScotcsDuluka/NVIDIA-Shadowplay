// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 435
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, i) {
    i(exports, require(44));
  }(this, function(e, t) {
    "use strict";

    function n() {
      return new i();
    }

    function i() {
      this.reset();
    }

    function o(e, t, n) {
      var i = e.s = t + n,
        o = i - t,
        r = i - o;
      e.t = t - r + (n - o);
    }

    function r(e) {
      return e > 1 ? 0 : e < -1 ? Fn : Math.acos(e);
    }

    function a(e) {
      return e > 1 ? Un : e < -1 ? -Un : Math.asin(e);
    }

    function l(e) {
      return (e = Zn(e / 2)) * e;
    }

    function s() {}

    function d(e, t) {
      e && ni.hasOwnProperty(e.type) && ni[e.type](e, t);
    }

    function c(e, t, n) {
      var i,
        o = -1,
        r = e.length - n;
      for (t.lineStart(); ++o < r;) i = e[o], t.point(i[0], i[1], i[2]);
      t.lineEnd();
    }

    function u(e, t) {
      var n = -1,
        i = e.length;
      for (t.polygonStart(); ++n < i;) c(e[n], t, 1);
      t.polygonEnd();
    }

    function f(e, t) {
      e && ti.hasOwnProperty(e.type) ? ti[e.type](e, t) : d(e, t);
    }

    function m() {
      ri.point = p;
    }

    function g() {
      h(nn, on);
    }

    function p(e, t) {
      ri.point = h, nn = e, on = t, e *= Hn, t *= Hn, rn = e, an = Wn(t = t / 2 + zn), ln = Zn(t);
    }

    function h(e, t) {
      e *= Hn, t *= Hn, t = t / 2 + zn;
      var n = e - rn,
        i = n >= 0 ? 1 : -1,
        o = i * n,
        r = Wn(t),
        a = Zn(t),
        l = ln * a,
        s = an * r + l * Wn(o),
        d = l * i * Zn(o);
      ii.add($n(d, s)), rn = e, an = r, ln = a;
    }

    function b(e) {
      return oi.reset(), f(e, ri), 2 * oi;
    }

    function x(e) {
      return [$n(e[1], e[0]), a(e[2])];
    }

    function v(e) {
      var t = e[0],
        n = e[1],
        i = Wn(n);
      return [i * Wn(t), i * Zn(t), Zn(n)];
    }

    function y(e, t) {
      return e[0] * t[0] + e[1] * t[1] + e[2] * t[2];
    }

    function w(e, t) {
      return [e[1] * t[2] - e[2] * t[1], e[2] * t[0] - e[0] * t[2], e[0] * t[1] - e[1] * t[0]];
    }

    function S(e, t) {
      e[0] += t[0], e[1] += t[1], e[2] += t[2];
    }

    function E(e, t) {
      return [e[0] * t, e[1] * t, e[2] * t];
    }

    function k(e) {
      var t = Jn(e[0] * e[0] + e[1] * e[1] + e[2] * e[2]);
      e[0] /= t, e[1] /= t, e[2] /= t;
    }

    function _(e, t) {
      hn.push(bn = [sn = e, cn = e]), t < dn && (dn = t), t > un && (un = t);
    }

    function T(e, t) {
      var n = v([e * Hn, t * Hn]);
      if (pn) {
        var i = w(pn, n),
          o = [i[1], -i[0], 0],
          r = w(o, i);
        k(r), r = x(r);
        var a,
          l = e - fn,
          s = l > 0 ? 1 : -1,
          d = r[0] * Vn * s,
          c = Bn(l) > 180;
        c ^ (s * fn < d && d < s * e) ? (a = r[1] * Vn, a > un && (un = a)) : (d = (d + 360) % 360 - 180,
            c ^ (s * fn < d && d < s * e) ? (a = -r[1] * Vn, a < dn && (dn = a)) : (t < dn && (dn = t), t >
              un && (un = t))), c ? e < fn ? R(sn, e) > R(sn, cn) && (cn = e) : R(e, cn) > R(sn, cn) && (
            sn = e) : cn >= sn ? (e < sn && (sn = e), e > cn && (cn = e)) : e > fn ? R(sn, e) > R(sn, cn) &&
          (cn = e) : R(e, cn) > R(sn, cn) && (sn = e);
      } else hn.push(bn = [sn = e, cn = e]);
      t < dn && (dn = t), t > un && (un = t), pn = n, fn = e;
    }

    function C() {
      li.point = T;
    }

    function O() {
      bn[0] = sn, bn[1] = cn, li.point = _, pn = null;
    }

    function A(e, t) {
      if (pn) {
        var n = e - fn;
        ai.add(Bn(n) > 180 ? n + (n > 0 ? 360 : -360) : n);
      } else mn = e, gn = t;
      ri.point(e, t), T(e, t);
    }

    function I() {
      ri.lineStart();
    }

    function M() {
      A(mn, gn), ri.lineEnd(), Bn(ai) > Nn && (sn = -(cn = 180)), bn[0] = sn, bn[1] = cn, pn = null;
    }

    function R(e, t) {
      return (t -= e) < 0 ? t + 360 : t;
    }

    function P(e, t) {
      return e[0] - t[0];
    }

    function D(e, t) {
      return e[0] <= e[1] ? e[0] <= t && t <= e[1] : t < e[0] || e[1] < t;
    }

    function N(e) {
      var t, n, i, o, r, a, l;
      if (un = cn = -(sn = dn = 1 / 0), hn = [], f(e, li), n = hn.length) {
        for (hn.sort(P), t = 1, i = hn[0], r = [i]; t < n; ++t) o = hn[t], D(i, o[0]) || D(i, o[1]) ? (R(i[
            0], o[1]) > R(i[0], i[1]) && (i[1] = o[1]), R(o[0], i[1]) > R(i[0], i[1]) && (i[0] = o[0])) : r
          .push(i = o);
        for (a = -(1 / 0), n = r.length - 1, t = 0, i = r[n]; t <= n; i = o, ++t) o = r[t], (l = R(i[1], o[
          0])) > a && (a = l, sn = o[0], cn = i[1]);
      }
      return hn = bn = null, sn === 1 / 0 || dn === 1 / 0 ? [
        [NaN, NaN],
        [NaN, NaN]
      ] : [
        [sn, dn],
        [cn, un]
      ];
    }

    function L(e, t) {
      e *= Hn, t *= Hn;
      var n = Wn(t);
      F(n * Wn(e), n * Zn(e), Zn(t));
    }

    function F(e, t, n) {
      ++xn, yn += (e - yn) / xn, wn += (t - wn) / xn, Sn += (n - Sn) / xn;
    }

    function U() {
      si.point = z;
    }

    function z(e, t) {
      e *= Hn, t *= Hn;
      var n = Wn(t);
      Mn = n * Wn(e), Rn = n * Zn(e), Pn = Zn(t), si.point = G, F(Mn, Rn, Pn);
    }

    function G(e, t) {
      e *= Hn, t *= Hn;
      var n = Wn(t),
        i = n * Wn(e),
        o = n * Zn(e),
        r = Zn(t),
        a = $n(Jn((a = Rn * r - Pn * o) * a + (a = Pn * i - Mn * r) * a + (a = Mn * o - Rn * i) * a), Mn *
          i + Rn * o + Pn * r);
      vn += a, En += a * (Mn + (Mn = i)), kn += a * (Rn + (Rn = o)), _n += a * (Pn + (Pn = r)), F(Mn, Rn,
        Pn);
    }

    function V() {
      si.point = L;
    }

    function H() {
      si.point = Y;
    }

    function B() {
      $(An, In), si.point = L;
    }

    function Y(e, t) {
      An = e, In = t, e *= Hn, t *= Hn, si.point = $;
      var n = Wn(t);
      Mn = n * Wn(e), Rn = n * Zn(e), Pn = Zn(t), F(Mn, Rn, Pn);
    }

    function $(e, t) {
      e *= Hn, t *= Hn;
      var n = Wn(t),
        i = n * Wn(e),
        o = n * Zn(e),
        r = Zn(t),
        l = Rn * r - Pn * o,
        s = Pn * i - Mn * r,
        d = Mn * o - Rn * i,
        c = Jn(l * l + s * s + d * d),
        u = a(c),
        f = c && -u / c;
      Tn += f * l, Cn += f * s, On += f * d, vn += u, En += u * (Mn + (Mn = i)), kn += u * (Rn + (Rn = o)),
        _n += u * (Pn + (Pn = r)), F(Mn, Rn, Pn);
    }

    function W(e) {
      xn = vn = yn = wn = Sn = En = kn = _n = Tn = Cn = On = 0, f(e, si);
      var t = Tn,
        n = Cn,
        i = On,
        o = t * t + n * n + i * i;
      return o < Ln && (t = En, n = kn, i = _n, vn < Nn && (t = yn, n = wn, i = Sn), o = t * t + n * n + i *
        i, o < Ln) ? [NaN, NaN] : [$n(n, t) * Vn, a(i / Jn(o)) * Vn];
    }

    function j(e) {
      return function() {
        return e;
      };
    }

    function K(e, t) {
      function n(n, i) {
        return n = e(n, i), t(n[0], n[1]);
      }
      return e.invert && t.invert && (n.invert = function(n, i) {
        return n = t.invert(n, i), n && e.invert(n[0], n[1]);
      }), n;
    }

    function q(e, t) {
      return [Bn(e) > Fn ? e + Math.round(-e / Gn) * Gn : e, t];
    }

    function X(e, t, n) {
      return (e %= Gn) ? t || n ? K(Q(e), J(t, n)) : Q(e) : t || n ? J(t, n) : q;
    }

    function Z(e) {
      return function(t, n) {
        return t += e, [t > Fn ? t - Gn : t < -Fn ? t + Gn : t, n];
      };
    }

    function Q(e) {
      var t = Z(e);
      return t.invert = Z(-e), t;
    }

    function J(e, t) {
      function n(e, t) {
        var n = Wn(t),
          s = Wn(e) * n,
          d = Zn(e) * n,
          c = Zn(t),
          u = c * i + s * o;
        return [$n(d * r - u * l, s * i - c * o), a(u * r + d * l)];
      }
      var i = Wn(e),
        o = Zn(e),
        r = Wn(t),
        l = Zn(t);
      return n.invert = function(e, t) {
        var n = Wn(t),
          s = Wn(e) * n,
          d = Zn(e) * n,
          c = Zn(t),
          u = c * r - d * l;
        return [$n(d * r + c * l, s * i + u * o), a(u * i - s * o)];
      }, n;
    }

    function ee(e) {
      function t(t) {
        return t = e(t[0] * Hn, t[1] * Hn), t[0] *= Vn, t[1] *= Vn, t;
      }
      return e = X(e[0] * Hn, e[1] * Hn, e.length > 2 ? e[2] * Hn : 0), t.invert = function(t) {
        return t = e.invert(t[0] * Hn, t[1] * Hn), t[0] *= Vn, t[1] *= Vn, t;
      }, t;
    }

    function te(e, t, n, i, o, r) {
      if (n) {
        var a = Wn(t),
          l = Zn(t),
          s = i * n;
        null == o ? (o = t + i * Gn, r = t - s / 2) : (o = ne(a, o), r = ne(a, r), (i > 0 ? o < r : o >
          r) && (o += i * Gn));
        for (var d, c = o; i > 0 ? c > r : c < r; c -= s) d = x([a, -l * Wn(c), -l * Zn(c)]), e.point(d[0],
          d[1]);
      }
    }

    function ne(e, t) {
      t = v(t), t[0] -= e, k(t);
      var n = r(-t[1]);
      return ((-t[2] < 0 ? -n : n) + Gn - Nn) % Gn;
    }

    function ie() {
      function e(e, t) {
        n.push(e = i(e, t)), e[0] *= Vn, e[1] *= Vn;
      }

      function t() {
        var e = o.apply(this, arguments),
          t = r.apply(this, arguments) * Hn,
          s = a.apply(this, arguments) * Hn;
        return n = [], i = X(-e[0] * Hn, -e[1] * Hn, 0).invert, te(l, t, s, 1), e = {
          type: "Polygon",
          coordinates: [n]
        }, n = i = null, e;
      }
      var n,
        i,
        o = j([0, 0]),
        r = j(90),
        a = j(6),
        l = {
          point: e
        };
      return t.center = function(e) {
        return arguments.length ? (o = "function" == typeof e ? e : j([+e[0], +e[1]]), t) : o;
      }, t.radius = function(e) {
        return arguments.length ? (r = "function" == typeof e ? e : j(+e), t) : r;
      }, t.precision = function(e) {
        return arguments.length ? (a = "function" == typeof e ? e : j(+e), t) : a;
      }, t;
    }

    function oe() {
      var e,
        t = [];
      return {
        point: function(t, n, i) {
          e.push([t, n, i]);
        },
        lineStart: function() {
          t.push(e = []);
        },
        lineEnd: s,
        rejoin: function() {
          t.length > 1 && t.push(t.pop().concat(t.shift()));
        },
        result: function() {
          var n = t;
          return t = [], e = null, n;
        }
      };
    }

    function re(e, t) {
      return Bn(e[0] - t[0]) < Nn && Bn(e[1] - t[1]) < Nn;
    }

    function ae(e, t, n, i) {
      this.x = e, this.z = t, this.o = n, this.e = i, this.v = !1, this.n = this.p = null;
    }

    function le(e, t, n, i, o) {
      var r,
        a,
        l = [],
        s = [];
      if (e.forEach(function(e) {
          if (!((t = e.length - 1) <= 0)) {
            var t,
              n,
              i = e[0],
              a = e[t];
            if (re(i, a)) {
              if (!i[2] && !a[2]) {
                for (o.lineStart(), r = 0; r < t; ++r) o.point((i = e[r])[0], i[1]);
                return void o.lineEnd();
              }
              a[0] += 2 * Nn;
            }
            l.push(n = new ae(i, e, null, !0)), s.push(n.o = new ae(i, null, n, !1)), l.push(n = new ae(a,
              e, null, !1)), s.push(n.o = new ae(a, null, n, !0));
          }
        }), l.length) {
        for (s.sort(t), se(l), se(s), r = 0, a = s.length; r < a; ++r) s[r].e = n = !n;
        for (var d, c, u = l[0];;) {
          for (var f = u, m = !0; f.v;)
            if ((f = f.n) === u) return;
          d = f.z, o.lineStart();
          do {
            if (f.v = f.o.v = !0, f.e) {
              if (m)
                for (r = 0, a = d.length; r < a; ++r) o.point((c = d[r])[0], c[1]);
              else i(f.x, f.n.x, 1, o);
              f = f.n;
            } else {
              if (m)
                for (d = f.p.z, r = d.length - 1; r >= 0; --r) o.point((c = d[r])[0], c[1]);
              else i(f.x, f.p.x, -1, o);
              f = f.p;
            }
            f = f.o, d = f.z, m = !m;
          } while (!f.v);
          o.lineEnd();
        }
      }
    }

    function se(e) {
      if (t = e.length) {
        for (var t, n, i = 0, o = e[0]; ++i < t;) o.n = n = e[i], n.p = o, o = n;
        o.n = n = e[0], n.p = o;
      }
    }

    function de(e) {
      return Bn(e[0]) <= Fn ? e[0] : Qn(e[0]) * ((Bn(e[0]) + Fn) % Gn - Fn);
    }

    function ce(e, t) {
      var n = de(t),
        i = t[1],
        o = Zn(i),
        r = [Zn(n), -Wn(n), 0],
        l = 0,
        s = 0;
      yi.reset(), 1 === o ? i = Un + Nn : o === -1 && (i = -Un - Nn);
      for (var d = 0, c = e.length; d < c; ++d)
        if (f = (u = e[d]).length)
          for (var u, f, m = u[f - 1], g = de(m), p = m[1] / 2 + zn, h = Zn(p), b = Wn(p), x = 0; x < f; ++
            x, g = S, h = _, b = T, m = y) {
            var y = u[x],
              S = de(y),
              E = y[1] / 2 + zn,
              _ = Zn(E),
              T = Wn(E),
              C = S - g,
              O = C >= 0 ? 1 : -1,
              A = O * C,
              I = A > Fn,
              M = h * _;
            if (yi.add($n(M * O * Zn(A), b * T + M * Wn(A))), l += I ? C + O * Gn : C, I ^ g >= n ^ S >=
              n) {
              var R = w(v(m), v(y));
              k(R);
              var P = w(r, R);
              k(P);
              var D = (I ^ C >= 0 ? -1 : 1) * a(P[2]);
              (i > D || i === D && (R[0] || R[1])) && (s += I ^ C >= 0 ? 1 : -1);
            }
          }
      return (l < -Nn || l < Nn && yi < -Nn) ^ 1 & s;
    }

    function ue(e, n, i, o) {
      return function(r) {
        function a(t, n) {
          e(t, n) && r.point(t, n);
        }

        function l(e, t) {
          h.point(e, t);
        }

        function s() {
          y.point = l, h.lineStart();
        }

        function d() {
          y.point = a, h.lineEnd();
        }

        function c(e, t) {
          p.push([e, t]), x.point(e, t);
        }

        function u() {
          x.lineStart(), p = [];
        }

        function f() {
          c(p[0][0], p[0][1]), x.lineEnd();
          var e,
            t,
            n,
            i,
            o = x.clean(),
            a = b.result(),
            l = a.length;
          if (p.pop(), m.push(p), p = null, l)
            if (1 & o) {
              if (n = a[0], (t = n.length - 1) > 0) {
                for (v || (r.polygonStart(), v = !0), r.lineStart(), e = 0; e < t; ++e) r.point((i = n[
                  e])[0], i[1]);
                r.lineEnd();
              }
            } else l > 1 && 2 & o && a.push(a.pop().concat(a.shift())), g.push(a.filter(fe));
        }
        var m,
          g,
          p,
          h = n(r),
          b = oe(),
          x = n(b),
          v = !1,
          y = {
            point: a,
            lineStart: s,
            lineEnd: d,
            polygonStart: function() {
              y.point = c, y.lineStart = u, y.lineEnd = f, g = [], m = [];
            },
            polygonEnd: function() {
              y.point = a, y.lineStart = s, y.lineEnd = d, g = t.merge(g);
              var e = ce(m, o);
              g.length ? (v || (r.polygonStart(), v = !0), le(g, me, e, i, r)) : e && (v || (r
                .polygonStart(), v = !0), r.lineStart(), i(null, null, 1, r), r.lineEnd()), v && (r
                .polygonEnd(), v = !1), g = m = null;
            },
            sphere: function() {
              r.polygonStart(), r.lineStart(), i(null, null, 1, r), r.lineEnd(), r.polygonEnd();
            }
          };
        return y;
      };
    }

    function fe(e) {
      return e.length > 1;
    }

    function me(e, t) {
      return ((e = e.x)[0] < 0 ? e[1] - Un - Nn : Un - e[1]) - ((t = t.x)[0] < 0 ? t[1] - Un - Nn : Un - t[
        1]);
    }

    function ge(e) {
      var t,
        n = NaN,
        i = NaN,
        o = NaN;
      return {
        lineStart: function() {
          e.lineStart(), t = 1;
        },
        point: function(r, a) {
          var l = r > 0 ? Fn : -Fn,
            s = Bn(r - n);
          Bn(s - Fn) < Nn ? (e.point(n, i = (i + a) / 2 > 0 ? Un : -Un), e.point(o, i), e.lineEnd(), e
            .lineStart(), e.point(l, i), e.point(r, i), t = 0) : o !== l && s >= Fn && (Bn(n - o) <
            Nn && (n -= o * Nn), Bn(r - l) < Nn && (r -= l * Nn), i = pe(n, i, r, a), e.point(o, i), e
            .lineEnd(), e.lineStart(), e.point(l, i), t = 0), e.point(n = r, i = a), o = l;
        },
        lineEnd: function() {
          e.lineEnd(), n = i = NaN;
        },
        clean: function() {
          return 2 - t;
        }
      };
    }

    function pe(e, t, n, i) {
      var o,
        r,
        a = Zn(e - n);
      return Bn(a) > Nn ? Yn((Zn(t) * (r = Wn(i)) * Zn(n) - Zn(i) * (o = Wn(t)) * Zn(e)) / (o * r * a)) : (
        t + i) / 2;
    }

    function he(e, t, n, i) {
      var o;
      if (null == e) o = n * Un, i.point(-Fn, o), i.point(0, o), i.point(Fn, o), i.point(Fn, 0), i.point(Fn,
        -o), i.point(0, -o), i.point(-Fn, -o), i.point(-Fn, 0), i.point(-Fn, o);
      else if (Bn(e[0] - t[0]) > Nn) {
        var r = e[0] < t[0] ? Fn : -Fn;
        o = n * r / 2, i.point(-r, o), i.point(0, o), i.point(r, o);
      } else i.point(t[0], t[1]);
    }

    function be(e) {
      function t(t, n, i, o) {
        te(o, e, l, i, t, n);
      }

      function n(e, t) {
        return Wn(e) * Wn(t) > a;
      }

      function i(e) {
        var t, i, a, l, c;
        return {
          lineStart: function() {
            l = a = !1, c = 1;
          },
          point: function(u, f) {
            var m,
              g = [u, f],
              p = n(u, f),
              h = s ? p ? 0 : r(u, f) : p ? r(u + (u < 0 ? Fn : -Fn), f) : 0;
            if (!t && (l = a = p) && e.lineStart(), p !== a && (m = o(t, g), (!m || re(t, m) || re(g,
                m)) && (g[2] = 1)), p !== a) c = 0, p ? (e.lineStart(), m = o(g, t), e.point(m[0], m[
              1])) : (m = o(t, g), e.point(m[0], m[1], 2), e.lineEnd()), t = m;
            else if (d && t && s ^ p) {
              var b;
              h & i || !(b = o(g, t, !0)) || (c = 0, s ? (e.lineStart(), e.point(b[0][0], b[0][1]), e
                .point(b[1][0], b[1][1]), e.lineEnd()) : (e.point(b[1][0], b[1][1]), e.lineEnd(), e
                .lineStart(), e.point(b[0][0], b[0][1], 3)));
            }!p || t && re(t, g) || e.point(g[0], g[1]), t = g, a = p, i = h;
          },
          lineEnd: function() {
            a && e.lineEnd(), t = null;
          },
          clean: function() {
            return c | (l && a) << 1;
          }
        };
      }

      function o(e, t, n) {
        var i = v(e),
          o = v(t),
          r = [1, 0, 0],
          l = w(i, o),
          s = y(l, l),
          d = l[0],
          c = s - d * d;
        if (!c) return !n && e;
        var u = a * s / c,
          f = -a * d / c,
          m = w(r, l),
          g = E(r, u),
          p = E(l, f);
        S(g, p);
        var h = m,
          b = y(g, h),
          k = y(h, h),
          _ = b * b - k * (y(g, g) - 1);
        if (!(_ < 0)) {
          var T = Jn(_),
            C = E(h, (-b - T) / k);
          if (S(C, g), C = x(C), !n) return C;
          var O,
            A = e[0],
            I = t[0],
            M = e[1],
            R = t[1];
          I < A && (O = A, A = I, I = O);
          var P = I - A,
            D = Bn(P - Fn) < Nn,
            N = D || P < Nn;
          if (!D && R < M && (O = M, M = R, R = O), N ? D ? M + R > 0 ^ C[1] < (Bn(C[0] - A) < Nn ? M : R) :
            M <= C[1] && C[1] <= R : P > Fn ^ (A <= C[0] && C[0] <= I)) {
            var L = E(h, (-b + T) / k);
            return S(L, g), [C, x(L)];
          }
        }
      }

      function r(t, n) {
        var i = s ? e : Fn - e,
          o = 0;
        return t < -i ? o |= 1 : t > i && (o |= 2), n < -i ? o |= 4 : n > i && (o |= 8), o;
      }
      var a = Wn(e),
        l = 6 * Hn,
        s = a > 0,
        d = Bn(a) > Nn;
      return ue(n, i, t, s ? [0, -e] : [-Fn, e - Fn]);
    }

    function xe(e, t, n, i, o, r) {
      var a,
        l = e[0],
        s = e[1],
        d = t[0],
        c = t[1],
        u = 0,
        f = 1,
        m = d - l,
        g = c - s;
      if (a = n - l, m || !(a > 0)) {
        if (a /= m, m < 0) {
          if (a < u) return;
          a < f && (f = a);
        } else if (m > 0) {
          if (a > f) return;
          a > u && (u = a);
        }
        if (a = o - l, m || !(a < 0)) {
          if (a /= m, m < 0) {
            if (a > f) return;
            a > u && (u = a);
          } else if (m > 0) {
            if (a < u) return;
            a < f && (f = a);
          }
          if (a = i - s, g || !(a > 0)) {
            if (a /= g, g < 0) {
              if (a < u) return;
              a < f && (f = a);
            } else if (g > 0) {
              if (a > f) return;
              a > u && (u = a);
            }
            if (a = r - s, g || !(a < 0)) {
              if (a /= g, g < 0) {
                if (a > f) return;
                a > u && (u = a);
              } else if (g > 0) {
                if (a < u) return;
                a < f && (f = a);
              }
              return u > 0 && (e[0] = l + u * m, e[1] = s + u * g), f < 1 && (t[0] = l + f * m, t[1] = s +
                f * g), !0;
            }
          }
        }
      }
    }

    function ve(e, n, i, o) {
      function r(t, r) {
        return e <= t && t <= i && n <= r && r <= o;
      }

      function a(t, r, a, s) {
        var c = 0,
          u = 0;
        if (null == t || (c = l(t, a)) !== (u = l(r, a)) || d(t, r) < 0 ^ a > 0) {
          do s.point(0 === c || 3 === c ? e : i, c > 1 ? o : n); while ((c = (c + a + 4) % 4) !== u);
        } else s.point(r[0], r[1]);
      }

      function l(t, o) {
        return Bn(t[0] - e) < Nn ? o > 0 ? 0 : 3 : Bn(t[0] - i) < Nn ? o > 0 ? 2 : 1 : Bn(t[1] - n) < Nn ?
          o > 0 ? 1 : 0 : o > 0 ? 3 : 2;
      }

      function s(e, t) {
        return d(e.x, t.x);
      }

      function d(e, t) {
        var n = l(e, 1),
          i = l(t, 1);
        return n !== i ? n - i : 0 === n ? t[1] - e[1] : 1 === n ? e[0] - t[0] : 2 === n ? e[1] - t[1] : t[
          0] - e[0];
      }
      return function(l) {
        function d(e, t) {
          r(e, t) && C.point(e, t);
        }

        function c() {
          for (var t = 0, n = 0, i = b.length; n < i; ++n)
            for (var r, a, l = b[n], s = 1, d = l.length, c = l[0], u = c[0], f = c[1]; s < d; ++s) r = u,
              a = f, c = l[s], u = c[0], f = c[1], a <= o ? f > o && (u - r) * (o - a) > (f - a) * (e -
              r) && ++t : f <= o && (u - r) * (o - a) < (f - a) * (e - r) && --t;
          return t;
        }

        function u() {
          C = O, h = [], b = [], T = !0;
        }

        function f() {
          var e = c(),
            n = T && e,
            i = (h = t.merge(h)).length;
          (n || i) && (l.polygonStart(), n && (l.lineStart(), a(null, null, 1, l), l.lineEnd()), i && le(
            h, s, e, a, l), l.polygonEnd()), C = l, h = b = x = null;
        }

        function m() {
          A.point = p, b && b.push(x = []), _ = !0, k = !1, S = E = NaN;
        }

        function g() {
          h && (p(v, y), w && k && O.rejoin(), h.push(O.result())), A.point = d, k && C.lineEnd();
        }

        function p(t, a) {
          var l = r(t, a);
          if (b && x.push([t, a]), _) v = t, y = a, w = l, _ = !1, l && (C.lineStart(), C.point(t, a));
          else if (l && k) C.point(t, a);
          else {
            var s = [S = Math.max(Ei, Math.min(Si, S)), E = Math.max(Ei, Math.min(Si, E))],
              d = [t = Math.max(Ei, Math.min(Si, t)), a = Math.max(Ei, Math.min(Si, a))];
            xe(s, d, e, n, i, o) ? (k || (C.lineStart(), C.point(s[0], s[1])), C.point(d[0], d[1]), l || C
              .lineEnd(), T = !1) : l && (C.lineStart(), C.point(t, a), T = !1);
          }
          S = t, E = a, k = l;
        }
        var h,
          b,
          x,
          v,
          y,
          w,
          S,
          E,
          k,
          _,
          T,
          C = l,
          O = oe(),
          A = {
            point: d,
            lineStart: m,
            lineEnd: g,
            polygonStart: u,
            polygonEnd: f
          };
        return A;
      };
    }

    function ye() {
      var e,
        t,
        n,
        i = 0,
        o = 0,
        r = 960,
        a = 500;
      return n = {
        stream: function(n) {
          return e && t === n ? e : e = ve(i, o, r, a)(t = n);
        },
        extent: function(l) {
          return arguments.length ? (i = +l[0][0], o = +l[0][1], r = +l[1][0], a = +l[1][1], e = t =
            null, n) : [
            [i, o],
            [r, a]
          ];
        }
      };
    }

    function we() {
      _i.point = Ee, _i.lineEnd = Se;
    }

    function Se() {
      _i.point = _i.lineEnd = s;
    }

    function Ee(e, t) {
      e *= Hn, t *= Hn, di = e, ci = Zn(t), ui = Wn(t), _i.point = ke;
    }

    function ke(e, t) {
      e *= Hn, t *= Hn;
      var n = Zn(t),
        i = Wn(t),
        o = Bn(e - di),
        r = Wn(o),
        a = Zn(o),
        l = i * a,
        s = ui * n - ci * i * r,
        d = ci * n + ui * i * r;
      ki.add($n(Jn(l * l + s * s), d)), di = e, ci = n, ui = i;
    }

    function _e(e) {
      return ki.reset(), f(e, _i), +ki;
    }

    function Te(e, t) {
      return Ti[0] = e, Ti[1] = t, _e(Ci);
    }

    function Ce(e, t) {
      return !(!e || !Ai.hasOwnProperty(e.type)) && Ai[e.type](e, t);
    }

    function Oe(e, t) {
      return 0 === Te(e, t);
    }

    function Ae(e, t) {
      for (var n, i, o, r = 0, a = e.length; r < a; r++) {
        if (i = Te(e[r], t), 0 === i) return !0;
        if (r > 0 && (o = Te(e[r], e[r - 1]), o > 0 && n <= o && i <= o && (n + i - o) * (1 - Math.pow((n -
            i) / o, 2)) < Ln * o)) return !0;
        n = i;
      }
      return !1;
    }

    function Ie(e, t) {
      return !!ce(e.map(Me), Re(t));
    }

    function Me(e) {
      return e = e.map(Re), e.pop(), e;
    }

    function Re(e) {
      return [e[0] * Hn, e[1] * Hn];
    }

    function Pe(e, t) {
      return (e && Oi.hasOwnProperty(e.type) ? Oi[e.type] : Ce)(e, t);
    }

    function De(e, n, i) {
      var o = t.range(e, n - Nn, i).concat(n);
      return function(e) {
        return o.map(function(t) {
          return [e, t];
        });
      };
    }

    function Ne(e, n, i) {
      var o = t.range(e, n - Nn, i).concat(n);
      return function(e) {
        return o.map(function(t) {
          return [t, e];
        });
      };
    }

    function Le() {
      function e() {
        return {
          type: "MultiLineString",
          coordinates: n()
        };
      }

      function n() {
        return t.range(jn(a / b) * b, r, b).map(m).concat(t.range(jn(c / x) * x, d, x).map(g)).concat(t
          .range(jn(o / p) * p, i, p).filter(function(e) {
            return Bn(e % b) > Nn;
          }).map(u)).concat(t.range(jn(s / h) * h, l, h).filter(function(e) {
          return Bn(e % x) > Nn;
        }).map(f));
      }
      var i,
        o,
        r,
        a,
        l,
        s,
        d,
        c,
        u,
        f,
        m,
        g,
        p = 10,
        h = p,
        b = 90,
        x = 360,
        v = 2.5;
      return e.lines = function() {
        return n().map(function(e) {
          return {
            type: "LineString",
            coordinates: e
          };
        });
      }, e.outline = function() {
        return {
          type: "Polygon",
          coordinates: [m(a).concat(g(d).slice(1), m(r).reverse().slice(1), g(c).reverse().slice(1))]
        };
      }, e.extent = function(t) {
        return arguments.length ? e.extentMajor(t).extentMinor(t) : e.extentMinor();
      }, e.extentMajor = function(t) {
        return arguments.length ? (a = +t[0][0], r = +t[1][0], c = +t[0][1], d = +t[1][1], a > r && (t =
          a, a = r, r = t), c > d && (t = c, c = d, d = t), e.precision(v)) : [
          [a, c],
          [r, d]
        ];
      }, e.extentMinor = function(t) {
        return arguments.length ? (o = +t[0][0], i = +t[1][0], s = +t[0][1], l = +t[1][1], o > i && (t =
          o, o = i, i = t), s > l && (t = s, s = l, l = t), e.precision(v)) : [
          [o, s],
          [i, l]
        ];
      }, e.step = function(t) {
        return arguments.length ? e.stepMajor(t).stepMinor(t) : e.stepMinor();
      }, e.stepMajor = function(t) {
        return arguments.length ? (b = +t[0], x = +t[1], e) : [b, x];
      }, e.stepMinor = function(t) {
        return arguments.length ? (p = +t[0], h = +t[1], e) : [p, h];
      }, e.precision = function(t) {
        return arguments.length ? (v = +t, u = De(s, l, 90), f = Ne(o, i, v), m = De(c, d, 90), g = Ne(a,
          r, v), e) : v;
      }, e.extentMajor([
        [-180, -90 + Nn],
        [180, 90 - Nn]
      ]).extentMinor([
        [-180, -80 - Nn],
        [180, 80 + Nn]
      ]);
    }

    function Fe() {
      return Le()();
    }

    function Ue(e, t) {
      var n = e[0] * Hn,
        i = e[1] * Hn,
        o = t[0] * Hn,
        r = t[1] * Hn,
        s = Wn(i),
        d = Zn(i),
        c = Wn(r),
        u = Zn(r),
        f = s * Wn(n),
        m = s * Zn(n),
        g = c * Wn(o),
        p = c * Zn(o),
        h = 2 * a(Jn(l(r - i) + s * c * l(o - n))),
        b = Zn(h),
        x = h ? function(e) {
          var t = Zn(e *= h) / b,
            n = Zn(h - e) / b,
            i = n * f + t * g,
            o = n * m + t * p,
            r = n * d + t * u;
          return [$n(o, i) * Vn, $n(r, Jn(i * i + o * o)) * Vn];
        } : function() {
          return [n * Vn, i * Vn];
        };
      return x.distance = h, x;
    }

    function ze(e) {
      return e;
    }

    function Ge() {
      Ri.point = Ve;
    }

    function Ve(e, t) {
      Ri.point = He, fi = gi = e, mi = pi = t;
    }

    function He(e, t) {
      Mi.add(pi * e - gi * t), gi = e, pi = t;
    }

    function Be() {
      He(fi, mi);
    }

    function Ye(e, t) {
      e < Pi && (Pi = e), e > Ni && (Ni = e), t < Di && (Di = t), t > Li && (Li = t);
    }

    function $e(e, t) {
      Ui += e, zi += t, ++Gi;
    }

    function We() {
      ji.point = je;
    }

    function je(e, t) {
      ji.point = Ke, $e(xi = e, vi = t);
    }

    function Ke(e, t) {
      var n = e - xi,
        i = t - vi,
        o = Jn(n * n + i * i);
      Vi += o * (xi + e) / 2, Hi += o * (vi + t) / 2, Bi += o, $e(xi = e, vi = t);
    }

    function qe() {
      ji.point = $e;
    }

    function Xe() {
      ji.point = Qe;
    }

    function Ze() {
      Je(hi, bi);
    }

    function Qe(e, t) {
      ji.point = Je, $e(hi = xi = e, bi = vi = t);
    }

    function Je(e, t) {
      var n = e - xi,
        i = t - vi,
        o = Jn(n * n + i * i);
      Vi += o * (xi + e) / 2, Hi += o * (vi + t) / 2, Bi += o, o = vi * e - xi * t, Yi += o * (xi + e),
        $i += o * (vi + t), Wi += 3 * o, $e(xi = e, vi = t);
    }

    function et(e) {
      this._context = e;
    }

    function tt(e, t) {
      eo.point = nt, qi = Zi = e, Xi = Qi = t;
    }

    function nt(e, t) {
      Zi -= e, Qi -= t, Ji.add(Jn(Zi * Zi + Qi * Qi)), Zi = e, Qi = t;
    }

    function it() {
      this._string = [];
    }

    function ot(e) {
      return "m0," + e + "a" + e + "," + e + " 0 1,1 0," + -2 * e + "a" + e + "," + e + " 0 1,1 0," + 2 *
        e + "z";
    }

    function rt(e, t) {
      function n(e) {
        return e && ("function" == typeof r && o.pointRadius(+r.apply(this, arguments)), f(e, i(o))), o
          .result();
      }
      var i,
        o,
        r = 4.5;
      return n.area = function(e) {
        return f(e, i(Ri)), Ri.result();
      }, n.measure = function(e) {
        return f(e, i(eo)), eo.result();
      }, n.bounds = function(e) {
        return f(e, i(Fi)), Fi.result();
      }, n.centroid = function(e) {
        return f(e, i(ji)), ji.result();
      }, n.projection = function(t) {
        return arguments.length ? (i = null == t ? (e = null, ze) : (e = t).stream, n) : e;
      }, n.context = function(e) {
        return arguments.length ? (o = null == e ? (t = null, new it()) : new et(t = e), "function" !=
          typeof r && o.pointRadius(r), n) : t;
      }, n.pointRadius = function(e) {
        return arguments.length ? (r = "function" == typeof e ? e : (o.pointRadius(+e), +e), n) : r;
      }, n.projection(e).context(t);
    }

    function at(e) {
      return {
        stream: lt(e)
      };
    }

    function lt(e) {
      return function(t) {
        var n = new st();
        for (var i in e) n[i] = e[i];
        return n.stream = t, n;
      };
    }

    function st() {}

    function dt(e, t, n) {
      var i = e.clipExtent && e.clipExtent();
      return e.scale(150).translate([0, 0]), null != i && e.clipExtent(null), f(n, e.stream(Fi)), t(Fi
        .result()), null != i && e.clipExtent(i), e;
    }

    function ct(e, t, n) {
      return dt(e, function(n) {
        var i = t[1][0] - t[0][0],
          o = t[1][1] - t[0][1],
          r = Math.min(i / (n[1][0] - n[0][0]), o / (n[1][1] - n[0][1])),
          a = +t[0][0] + (i - r * (n[1][0] + n[0][0])) / 2,
          l = +t[0][1] + (o - r * (n[1][1] + n[0][1])) / 2;
        e.scale(150 * r).translate([a, l]);
      }, n);
    }

    function ut(e, t, n) {
      return ct(e, [
        [0, 0], t
      ], n);
    }

    function ft(e, t, n) {
      return dt(e, function(n) {
        var i = +t,
          o = i / (n[1][0] - n[0][0]),
          r = (i - o * (n[1][0] + n[0][0])) / 2,
          a = -o * n[0][1];
        e.scale(150 * o).translate([r, a]);
      }, n);
    }

    function mt(e, t, n) {
      return dt(e, function(n) {
        var i = +t,
          o = i / (n[1][1] - n[0][1]),
          r = -o * n[0][0],
          a = (i - o * (n[1][1] + n[0][1])) / 2;
        e.scale(150 * o).translate([r, a]);
      }, n);
    }

    function gt(e, t) {
      return +t ? ht(e, t) : pt(e);
    }

    function pt(e) {
      return lt({
        point: function(t, n) {
          t = e(t, n), this.stream.point(t[0], t[1]);
        }
      });
    }

    function ht(e, t) {
      function n(i, o, r, l, s, d, c, u, f, m, g, p, h, b) {
        var x = c - i,
          v = u - o,
          y = x * x + v * v;
        if (y > 4 * t && h--) {
          var w = l + m,
            S = s + g,
            E = d + p,
            k = Jn(w * w + S * S + E * E),
            _ = a(E /= k),
            T = Bn(Bn(E) - 1) < Nn || Bn(r - f) < Nn ? (r + f) / 2 : $n(S, w),
            C = e(T, _),
            O = C[0],
            A = C[1],
            I = O - i,
            M = A - o,
            R = v * I - x * M;
          (R * R / y > t || Bn((x * I + v * M) / y - .5) > .3 || l * m + s * g + d * p < no) && (n(i, o, r,
            l, s, d, O, A, T, w /= k, S /= k, E, h, b), b.point(O, A), n(O, A, T, w, S, E, c, u, f, m, g,
            p, h, b));
        }
      }
      return function(t) {
        function i(n, i) {
          n = e(n, i), t.point(n[0], n[1]);
        }

        function o() {
          b = NaN, E.point = r, t.lineStart();
        }

        function r(i, o) {
          var r = v([i, o]),
            a = e(i, o);
          n(b, x, h, y, w, S, b = a[0], x = a[1], h = i, y = r[0], w = r[1], S = r[2], to, t), t.point(b,
            x);
        }

        function a() {
          E.point = i, t.lineEnd();
        }

        function l() {
          o(), E.point = s, E.lineEnd = d;
        }

        function s(e, t) {
          r(c = e, t), u = b, f = x, m = y, g = w, p = S, E.point = r;
        }

        function d() {
          n(b, x, h, y, w, S, u, f, c, m, g, p, to, t), E.lineEnd = a, a();
        }
        var c,
          u,
          f,
          m,
          g,
          p,
          h,
          b,
          x,
          y,
          w,
          S,
          E = {
            point: i,
            lineStart: o,
            lineEnd: a,
            polygonStart: function() {
              t.polygonStart(), E.lineStart = l;
            },
            polygonEnd: function() {
              t.polygonEnd(), E.lineStart = o;
            }
          };
        return E;
      };
    }

    function bt(e) {
      return lt({
        point: function(t, n) {
          var i = e(t, n);
          return this.stream.point(i[0], i[1]);
        }
      });
    }

    function xt(e, t, n, i, o) {
      function r(r, a) {
        return r *= i, a *= o, [t + e * r, n - e * a];
      }
      return r.invert = function(r, a) {
        return [(r - t) / e * i, (n - a) / e * o];
      }, r;
    }

    function vt(e, t, n, i, o, r) {
      function a(e, r) {
        return e *= i, r *= o, [d * e - c * r + t, n - c * e - d * r];
      }
      var l = Wn(r),
        s = Zn(r),
        d = l * e,
        c = s * e,
        u = l / e,
        f = s / e,
        m = (s * n - l * t) / e,
        g = (s * t + l * n) / e;
      return a.invert = function(e, t) {
        return [i * (u * e - f * t + m), o * (g - f * e - u * t)];
      }, a;
    }

    function yt(e) {
      return wt(function() {
        return e;
      })();
    }

    function wt(e) {
      function t(e) {
        return f(e[0] * Hn, e[1] * Hn);
      }

      function n(e) {
        return e = f.invert(e[0], e[1]), e && [e[0] * Vn, e[1] * Vn];
      }

      function i() {
        var e = vt(p, 0, 0, k, _, E).apply(null, r(x, v)),
          t = (E ? vt : xt)(p, h - e[0], b - e[1], k, _, E);
        return a = X(y, w, S), u = K(r, t), f = K(a, u), c = gt(u, I), o();
      }

      function o() {
        return m = g = null, t;
      }
      var r,
        a,
        l,
        s,
        d,
        c,
        u,
        f,
        m,
        g,
        p = 150,
        h = 480,
        b = 250,
        x = 0,
        v = 0,
        y = 0,
        w = 0,
        S = 0,
        E = 0,
        k = 1,
        _ = 1,
        T = null,
        C = wi,
        O = null,
        A = ze,
        I = .5;
      return t.stream = function(e) {
          return m && g === e ? m : m = io(bt(a)(C(c(A(g = e)))));
        }, t.preclip = function(e) {
          return arguments.length ? (C = e, T = void 0, o()) : C;
        }, t.postclip = function(e) {
          return arguments.length ? (A = e, O = l = s = d = null, o()) : A;
        }, t.clipAngle = function(e) {
          return arguments.length ? (C = +e ? be(T = e * Hn) : (T = null, wi), o()) : T * Vn;
        }, t.clipExtent = function(e) {
          return arguments.length ? (A = null == e ? (O = l = s = d = null, ze) : ve(O = +e[0][0], l = +e[0]
            [1], s = +e[1][0], d = +e[1][1]), o()) : null == O ? null : [
            [O, l],
            [s, d]
          ];
        }, t.scale = function(e) {
          return arguments.length ? (p = +e, i()) : p;
        }, t.translate = function(e) {
          return arguments.length ? (h = +e[0], b = +e[1], i()) : [h, b];
        }, t.center = function(e) {
          return arguments.length ? (x = e[0] % 360 * Hn, v = e[1] % 360 * Hn, i()) : [x * Vn, v * Vn];
        }, t.rotate = function(e) {
          return arguments.length ? (y = e[0] % 360 * Hn, w = e[1] % 360 * Hn, S = e.length > 2 ? e[2] %
            360 * Hn : 0, i()) : [y * Vn, w * Vn, S * Vn];
        }, t.angle = function(e) {
          return arguments.length ? (E = e % 360 * Hn, i()) : E * Vn;
        }, t.reflectX = function(e) {
          return arguments.length ? (k = e ? -1 : 1, i()) : k < 0;
        }, t.reflectY = function(e) {
          return arguments.length ? (_ = e ? -1 : 1, i()) : _ < 0;
        }, t.precision = function(e) {
          return arguments.length ? (c = gt(u, I = e * e), o()) : Jn(I);
        }, t.fitExtent = function(e, n) {
          return ct(t, e, n);
        }, t.fitSize = function(e, n) {
          return ut(t, e, n);
        }, t.fitWidth = function(e, n) {
          return ft(t, e, n);
        }, t.fitHeight = function(e, n) {
          return mt(t, e, n);
        },
        function() {
          return r = e.apply(this, arguments), t.invert = r.invert && n, i();
        };
    }

    function St(e) {
      var t = 0,
        n = Fn / 3,
        i = wt(e),
        o = i(t, n);
      return o.parallels = function(e) {
        return arguments.length ? i(t = e[0] * Hn, n = e[1] * Hn) : [t * Vn, n * Vn];
      }, o;
    }

    function Et(e) {
      function t(e, t) {
        return [e * n, Zn(t) / n];
      }
      var n = Wn(e);
      return t.invert = function(e, t) {
        return [e / n, a(t * n)];
      }, t;
    }

    function kt(e, t) {
      function n(e, t) {
        var n = Jn(r - 2 * o * Zn(t)) / o;
        return [n * Zn(e *= o), l - n * Wn(e)];
      }
      var i = Zn(e),
        o = (i + Zn(t)) / 2;
      if (Bn(o) < Nn) return Et(e);
      var r = 1 + i * (2 * o - i),
        l = Jn(r) / o;
      return n.invert = function(e, t) {
        var n = l - t,
          i = $n(e, Bn(n)) * Qn(n);
        return n * o < 0 && (i -= Fn * Qn(e) * Qn(n)), [i / o, a((r - (e * e + n * n) * o * o) / (2 *
        o))];
      }, n;
    }

    function _t() {
      return St(kt).scale(155.424).center([0, 33.6442]);
    }

    function Tt() {
      return _t().parallels([29.5, 45.5]).scale(1070).translate([480, 250]).rotate([96, 0]).center([-.6,
        38.7
      ]);
    }

    function Ct(e) {
      var t = e.length;
      return {
        point: function(n, i) {
          for (var o = -1; ++o < t;) e[o].point(n, i);
        },
        sphere: function() {
          for (var n = -1; ++n < t;) e[n].sphere();
        },
        lineStart: function() {
          for (var n = -1; ++n < t;) e[n].lineStart();
        },
        lineEnd: function() {
          for (var n = -1; ++n < t;) e[n].lineEnd();
        },
        polygonStart: function() {
          for (var n = -1; ++n < t;) e[n].polygonStart();
        },
        polygonEnd: function() {
          for (var n = -1; ++n < t;) e[n].polygonEnd();
        }
      };
    }

    function Ot() {
      function e(e) {
        var t = e[0],
          n = e[1];
        return l = null, o.point(t, n), l || (r.point(t, n), l) || (a.point(t, n), l);
      }

      function t() {
        return n = i = null, e;
      }
      var n,
        i,
        o,
        r,
        a,
        l,
        s = Tt(),
        d = _t().rotate([154, 0]).center([-2, 58.5]).parallels([55, 65]),
        c = _t().rotate([157, 0]).center([-3, 19.9]).parallels([8, 18]),
        u = {
          point: function(e, t) {
            l = [e, t];
          }
        };
      return e.invert = function(e) {
        var t = s.scale(),
          n = s.translate(),
          i = (e[0] - n[0]) / t,
          o = (e[1] - n[1]) / t;
        return (o >= .12 && o < .234 && i >= -.425 && i < -.214 ? d : o >= .166 && o < .234 && i >= -
          .214 && i < -.115 ? c : s).invert(e);
      }, e.stream = function(e) {
        return n && i === e ? n : n = Ct([s.stream(i = e), d.stream(e), c.stream(e)]);
      }, e.precision = function(e) {
        return arguments.length ? (s.precision(e), d.precision(e), c.precision(e), t()) : s.precision();
      }, e.scale = function(t) {
        return arguments.length ? (s.scale(t), d.scale(.35 * t), c.scale(t), e.translate(s.translate())) :
          s.scale();
      }, e.translate = function(e) {
        if (!arguments.length) return s.translate();
        var n = s.scale(),
          i = +e[0],
          l = +e[1];
        return o = s.translate(e).clipExtent([
          [i - .455 * n, l - .238 * n],
          [i + .455 * n, l + .238 * n]
        ]).stream(u), r = d.translate([i - .307 * n, l + .201 * n]).clipExtent([
          [i - .425 * n + Nn, l + .12 * n + Nn],
          [i - .214 * n - Nn, l + .234 * n - Nn]
        ]).stream(u), a = c.translate([i - .205 * n, l + .212 * n]).clipExtent([
          [i - .214 * n + Nn, l + .166 * n + Nn],
          [i - .115 * n - Nn, l + .234 * n - Nn]
        ]).stream(u), t();
      }, e.fitExtent = function(t, n) {
        return ct(e, t, n);
      }, e.fitSize = function(t, n) {
        return ut(e, t, n);
      }, e.fitWidth = function(t, n) {
        return ft(e, t, n);
      }, e.fitHeight = function(t, n) {
        return mt(e, t, n);
      }, e.scale(1070);
    }

    function At(e) {
      return function(t, n) {
        var i = Wn(t),
          o = Wn(n),
          r = e(i * o);
        return [r * o * Zn(t), r * Zn(n)];
      };
    }

    function It(e) {
      return function(t, n) {
        var i = Jn(t * t + n * n),
          o = e(i),
          r = Zn(o),
          l = Wn(o);
        return [$n(t * r, i * l), a(i && n * r / i)];
      };
    }

    function Mt() {
      return yt(oo).scale(124.75).clipAngle(179.999);
    }

    function Rt() {
      return yt(ro).scale(79.4188).clipAngle(179.999);
    }

    function Pt(e, t) {
      return [e, qn(ei((Un + t) / 2))];
    }

    function Dt() {
      return Nt(Pt).scale(961 / Gn);
    }

    function Nt(e) {
      function t() {
        var t = Fn * l(),
          a = r(ee(r.rotate()).invert([0, 0]));
        return d(null == c ? [
          [a[0] - t, a[1] - t],
          [a[0] + t, a[1] + t]
        ] : e === Pt ? [
          [Math.max(a[0] - t, c), n],
          [Math.min(a[0] + t, i), o]
        ] : [
          [c, Math.max(a[1] - t, n)],
          [i, Math.min(a[1] + t, o)]
        ]);
      }
      var n,
        i,
        o,
        r = yt(e),
        a = r.center,
        l = r.scale,
        s = r.translate,
        d = r.clipExtent,
        c = null;
      return r.scale = function(e) {
        return arguments.length ? (l(e), t()) : l();
      }, r.translate = function(e) {
        return arguments.length ? (s(e), t()) : s();
      }, r.center = function(e) {
        return arguments.length ? (a(e), t()) : a();
      }, r.clipExtent = function(e) {
        return arguments.length ? (null == e ? c = n = i = o = null : (c = +e[0][0], n = +e[0][1], i = +e[
          1][0], o = +e[1][1]), t()) : null == c ? null : [
          [c, n],
          [i, o]
        ];
      }, t();
    }

    function Lt(e) {
      return ei((Un + e) / 2);
    }

    function Ft(e, t) {
      function n(e, t) {
        r > 0 ? t < -Un + Nn && (t = -Un + Nn) : t > Un - Nn && (t = Un - Nn);
        var n = r / Xn(Lt(t), o);
        return [n * Zn(o * e), r - n * Wn(o * e)];
      }
      var i = Wn(e),
        o = e === t ? Zn(e) : qn(i / Wn(t)) / qn(Lt(t) / Lt(e)),
        r = i * Xn(Lt(e), o) / o;
      return o ? (n.invert = function(e, t) {
        var n = r - t,
          i = Qn(o) * Jn(e * e + n * n),
          a = $n(e, Bn(n)) * Qn(n);
        return n * o < 0 && (a -= Fn * Qn(e) * Qn(n)), [a / o, 2 * Yn(Xn(r / i, 1 / o)) - Un];
      }, n) : Pt;
    }

    function Ut() {
      return St(Ft).scale(109.5).parallels([30, 30]);
    }

    function zt(e, t) {
      return [e, t];
    }

    function Gt() {
      return yt(zt).scale(152.63);
    }

    function Vt(e, t) {
      function n(e, t) {
        var n = r - t,
          i = o * e;
        return [n * Zn(i), r - n * Wn(i)];
      }
      var i = Wn(e),
        o = e === t ? Zn(e) : (i - Wn(t)) / (t - e),
        r = i / o + e;
      return Bn(o) < Nn ? zt : (n.invert = function(e, t) {
        var n = r - t,
          i = $n(e, Bn(n)) * Qn(n);
        return n * o < 0 && (i -= Fn * Qn(e) * Qn(n)), [i / o, r - Qn(o) * Jn(e * e + n * n)];
      }, n);
    }

    function Ht() {
      return St(Vt).scale(131.154).center([0, 13.9389]);
    }

    function Bt(e, t) {
      var n = a(uo * Zn(t)),
        i = n * n,
        o = i * i * i;
      return [e * Wn(n) / (uo * (ao + 3 * lo * i + o * (7 * so + 9 * co * i))), n * (ao + lo * i + o * (so +
        co * i))];
    }

    function Yt() {
      return yt(Bt).scale(177.158);
    }

    function $t(e, t) {
      var n = Wn(t),
        i = Wn(e) * n;
      return [n * Zn(e) / i, Zn(t) / i];
    }

    function Wt() {
      return yt($t).scale(144.049).clipAngle(60);
    }

    function jt() {
      function e() {
        return h = d * f, b = d * m, l = s = null, t;
      }

      function t(e) {
        var t = e[0] * h,
          o = e[1] * b;
        if (g) {
          var r = o * n - t * i;
          t = t * n + o * i, o = r;
        }
        return [t + c, o + u];
      }
      var n,
        i,
        o,
        r,
        a,
        l,
        s,
        d = 1,
        c = 0,
        u = 0,
        f = 1,
        m = 1,
        g = 0,
        p = null,
        h = 1,
        b = 1,
        x = lt({
          point: function(e, n) {
            var i = t([e, n]);
            this.stream.point(i[0], i[1]);
          }
        }),
        v = ze;
      return t.invert = function(e) {
        var t = e[0] - c,
          o = e[1] - u;
        if (g) {
          var r = o * n + t * i;
          t = t * n - o * i, o = r;
        }
        return [t / h, o / b];
      }, t.stream = function(e) {
        return l && s === e ? l : l = x(v(s = e));
      }, t.postclip = function(t) {
        return arguments.length ? (v = t, p = o = r = a = null, e()) : v;
      }, t.clipExtent = function(t) {
        return arguments.length ? (v = null == t ? (p = o = r = a = null, ze) : ve(p = +t[0][0], o = +t[0]
          [1], r = +t[1][0], a = +t[1][1]), e()) : null == p ? null : [
          [p, o],
          [r, a]
        ];
      }, t.scale = function(t) {
        return arguments.length ? (d = +t, e()) : d;
      }, t.translate = function(t) {
        return arguments.length ? (c = +t[0], u = +t[1], e()) : [c, u];
      }, t.angle = function(t) {
        return arguments.length ? (g = t % 360 * Hn, i = Zn(g), n = Wn(g), e()) : g * Vn;
      }, t.reflectX = function(t) {
        return arguments.length ? (f = t ? -1 : 1, e()) : f < 0;
      }, t.reflectY = function(t) {
        return arguments.length ? (m = t ? -1 : 1, e()) : m < 0;
      }, t.fitExtent = function(e, n) {
        return ct(t, e, n);
      }, t.fitSize = function(e, n) {
        return ut(t, e, n);
      }, t.fitWidth = function(e, n) {
        return ft(t, e, n);
      }, t.fitHeight = function(e, n) {
        return mt(t, e, n);
      }, t;
    }

    function Kt(e, t) {
      var n = t * t,
        i = n * n;
      return [e * (.8707 - .131979 * n + i * (-.013791 + i * (.003971 * n - .001529 * i))), t * (1.007226 +
        n * (.015085 + i * (-.044475 + .028874 * n - .005916 * i)))];
    }

    function qt() {
      return yt(Kt).scale(175.295);
    }

    function Xt(e, t) {
      return [Wn(t) * Zn(e), Zn(t)];
    }

    function Zt() {
      return yt(Xt).scale(249.5).clipAngle(90 + Nn);
    }

    function Qt(e, t) {
      var n = Wn(t),
        i = 1 + Wn(e) * n;
      return [n * Zn(e) / i, Zn(t) / i];
    }

    function Jt() {
      return yt(Qt).scale(250).clipAngle(142);
    }

    function en(e, t) {
      return [qn(ei((Un + t) / 2)), -e];
    }

    function tn() {
      var e = Nt(en),
        t = e.center,
        n = e.rotate;
      return e.center = function(e) {
        return arguments.length ? t([-e[1], e[0]]) : (e = t(), [e[1], -e[0]]);
      }, e.rotate = function(e) {
        return arguments.length ? n([e[0], e[1], e.length > 2 ? e[2] + 90 : 90]) : (e = n(), [e[0], e[1],
          e[2] - 90
        ]);
      }, n([0, 0, 90]).scale(159.155);
    }
    i.prototype = {
      constructor: i,
      reset: function() {
        this.s = this.t = 0;
      },
      add: function(e) {
        o(Dn, e, this.t), o(this, Dn.s, this.s), this.s ? this.t += Dn.t : this.s = Dn.t;
      },
      valueOf: function() {
        return this.s;
      }
    };
    var nn,
      on,
      rn,
      an,
      ln,
      sn,
      dn,
      cn,
      un,
      fn,
      mn,
      gn,
      pn,
      hn,
      bn,
      xn,
      vn,
      yn,
      wn,
      Sn,
      En,
      kn,
      _n,
      Tn,
      Cn,
      On,
      An,
      In,
      Mn,
      Rn,
      Pn,
      Dn = new i(),
      Nn = 1e-6,
      Ln = 1e-12,
      Fn = Math.PI,
      Un = Fn / 2,
      zn = Fn / 4,
      Gn = 2 * Fn,
      Vn = 180 / Fn,
      Hn = Fn / 180,
      Bn = Math.abs,
      Yn = Math.atan,
      $n = Math.atan2,
      Wn = Math.cos,
      jn = Math.ceil,
      Kn = Math.exp,
      qn = Math.log,
      Xn = Math.pow,
      Zn = Math.sin,
      Qn = Math.sign || function(e) {
        return e > 0 ? 1 : e < 0 ? -1 : 0;
      },
      Jn = Math.sqrt,
      ei = Math.tan,
      ti = {
        Feature: function(e, t) {
          d(e.geometry, t);
        },
        FeatureCollection: function(e, t) {
          for (var n = e.features, i = -1, o = n.length; ++i < o;) d(n[i].geometry, t);
        }
      },
      ni = {
        Sphere: function(e, t) {
          t.sphere();
        },
        Point: function(e, t) {
          e = e.coordinates, t.point(e[0], e[1], e[2]);
        },
        MultiPoint: function(e, t) {
          for (var n = e.coordinates, i = -1, o = n.length; ++i < o;) e = n[i], t.point(e[0], e[1], e[2]);
        },
        LineString: function(e, t) {
          c(e.coordinates, t, 0);
        },
        MultiLineString: function(e, t) {
          for (var n = e.coordinates, i = -1, o = n.length; ++i < o;) c(n[i], t, 0);
        },
        Polygon: function(e, t) {
          u(e.coordinates, t);
        },
        MultiPolygon: function(e, t) {
          for (var n = e.coordinates, i = -1, o = n.length; ++i < o;) u(n[i], t);
        },
        GeometryCollection: function(e, t) {
          for (var n = e.geometries, i = -1, o = n.length; ++i < o;) d(n[i], t);
        }
      },
      ii = n(),
      oi = n(),
      ri = {
        point: s,
        lineStart: s,
        lineEnd: s,
        polygonStart: function() {
          ii.reset(), ri.lineStart = m, ri.lineEnd = g;
        },
        polygonEnd: function() {
          var e = +ii;
          oi.add(e < 0 ? Gn + e : e), this.lineStart = this.lineEnd = this.point = s;
        },
        sphere: function() {
          oi.add(Gn);
        }
      },
      ai = n(),
      li = {
        point: _,
        lineStart: C,
        lineEnd: O,
        polygonStart: function() {
          li.point = A, li.lineStart = I, li.lineEnd = M, ai.reset(), ri.polygonStart();
        },
        polygonEnd: function() {
          ri.polygonEnd(), li.point = _, li.lineStart = C, li.lineEnd = O, ii < 0 ? (sn = -(cn = 180),
            dn = -(un = 90)) : ai > Nn ? un = 90 : ai < -Nn && (dn = -90), bn[0] = sn, bn[1] = cn;
        },
        sphere: function() {
          sn = -(cn = 180), dn = -(un = 90);
        }
      },
      si = {
        sphere: s,
        point: L,
        lineStart: U,
        lineEnd: V,
        polygonStart: function() {
          si.lineStart = H, si.lineEnd = B;
        },
        polygonEnd: function() {
          si.lineStart = U, si.lineEnd = V;
        }
      };
    q.invert = q;
    var di,
      ci,
      ui,
      fi,
      mi,
      gi,
      pi,
      hi,
      bi,
      xi,
      vi,
      yi = n(),
      wi = ue(function() {
        return !0;
      }, ge, he, [-Fn, -Un]),
      Si = 1e9,
      Ei = -Si,
      ki = n(),
      _i = {
        sphere: s,
        point: s,
        lineStart: we,
        lineEnd: s,
        polygonStart: s,
        polygonEnd: s
      },
      Ti = [null, null],
      Ci = {
        type: "LineString",
        coordinates: Ti
      },
      Oi = {
        Feature: function(e, t) {
          return Ce(e.geometry, t);
        },
        FeatureCollection: function(e, t) {
          for (var n = e.features, i = -1, o = n.length; ++i < o;)
            if (Ce(n[i].geometry, t)) return !0;
          return !1;
        }
      },
      Ai = {
        Sphere: function() {
          return !0;
        },
        Point: function(e, t) {
          return Oe(e.coordinates, t);
        },
        MultiPoint: function(e, t) {
          for (var n = e.coordinates, i = -1, o = n.length; ++i < o;)
            if (Oe(n[i], t)) return !0;
          return !1;
        },
        LineString: function(e, t) {
          return Ae(e.coordinates, t);
        },
        MultiLineString: function(e, t) {
          for (var n = e.coordinates, i = -1, o = n.length; ++i < o;)
            if (Ae(n[i], t)) return !0;
          return !1;
        },
        Polygon: function(e, t) {
          return Ie(e.coordinates, t);
        },
        MultiPolygon: function(e, t) {
          for (var n = e.coordinates, i = -1, o = n.length; ++i < o;)
            if (Ie(n[i], t)) return !0;
          return !1;
        },
        GeometryCollection: function(e, t) {
          for (var n = e.geometries, i = -1, o = n.length; ++i < o;)
            if (Ce(n[i], t)) return !0;
          return !1;
        }
      },
      Ii = n(),
      Mi = n(),
      Ri = {
        point: s,
        lineStart: s,
        lineEnd: s,
        polygonStart: function() {
          Ri.lineStart = Ge, Ri.lineEnd = Be;
        },
        polygonEnd: function() {
          Ri.lineStart = Ri.lineEnd = Ri.point = s, Ii.add(Bn(Mi)), Mi.reset();
        },
        result: function() {
          var e = Ii / 2;
          return Ii.reset(), e;
        }
      },
      Pi = 1 / 0,
      Di = Pi,
      Ni = -Pi,
      Li = Ni,
      Fi = {
        point: Ye,
        lineStart: s,
        lineEnd: s,
        polygonStart: s,
        polygonEnd: s,
        result: function() {
          var e = [
            [Pi, Di],
            [Ni, Li]
          ];
          return Ni = Li = -(Di = Pi = 1 / 0), e;
        }
      },
      Ui = 0,
      zi = 0,
      Gi = 0,
      Vi = 0,
      Hi = 0,
      Bi = 0,
      Yi = 0,
      $i = 0,
      Wi = 0,
      ji = {
        point: $e,
        lineStart: We,
        lineEnd: qe,
        polygonStart: function() {
          ji.lineStart = Xe, ji.lineEnd = Ze;
        },
        polygonEnd: function() {
          ji.point = $e, ji.lineStart = We, ji.lineEnd = qe;
        },
        result: function() {
          var e = Wi ? [Yi / Wi, $i / Wi] : Bi ? [Vi / Bi, Hi / Bi] : Gi ? [Ui / Gi, zi / Gi] : [NaN,
          NaN];
          return Ui = zi = Gi = Vi = Hi = Bi = Yi = $i = Wi = 0, e;
        }
      };
    et.prototype = {
      _radius: 4.5,
      pointRadius: function(e) {
        return this._radius = e, this;
      },
      polygonStart: function() {
        this._line = 0;
      },
      polygonEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._point = 0;
      },
      lineEnd: function() {
        0 === this._line && this._context.closePath(), this._point = NaN;
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
            this._context.moveTo(e + this._radius, t), this._context.arc(e, t, this._radius, 0, Gn);
        }
      },
      result: s
    };
    var Ki,
      qi,
      Xi,
      Zi,
      Qi,
      Ji = n(),
      eo = {
        point: s,
        lineStart: function() {
          eo.point = tt;
        },
        lineEnd: function() {
          Ki && nt(qi, Xi), eo.point = s;
        },
        polygonStart: function() {
          Ki = !0;
        },
        polygonEnd: function() {
          Ki = null;
        },
        result: function() {
          var e = +Ji;
          return Ji.reset(), e;
        }
      };
    it.prototype = {
      _radius: 4.5,
      _circle: ot(4.5),
      pointRadius: function(e) {
        return (e = +e) !== this._radius && (this._radius = e, this._circle = null), this;
      },
      polygonStart: function() {
        this._line = 0;
      },
      polygonEnd: function() {
        this._line = NaN;
      },
      lineStart: function() {
        this._point = 0;
      },
      lineEnd: function() {
        0 === this._line && this._string.push("Z"), this._point = NaN;
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
            null == this._circle && (this._circle = ot(this._radius)), this._string.push("M", e, ",", t,
              this._circle);
        }
      },
      result: function() {
        if (this._string.length) {
          var e = this._string.join("");
          return this._string = [], e;
        }
        return null;
      }
    }, st.prototype = {
      constructor: st,
      point: function(e, t) {
        this.stream.point(e, t);
      },
      sphere: function() {
        this.stream.sphere();
      },
      lineStart: function() {
        this.stream.lineStart();
      },
      lineEnd: function() {
        this.stream.lineEnd();
      },
      polygonStart: function() {
        this.stream.polygonStart();
      },
      polygonEnd: function() {
        this.stream.polygonEnd();
      }
    };
    var to = 16,
      no = Wn(30 * Hn),
      io = lt({
        point: function(e, t) {
          this.stream.point(e * Hn, t * Hn);
        }
      }),
      oo = At(function(e) {
        return Jn(2 / (1 + e));
      });
    oo.invert = It(function(e) {
      return 2 * a(e / 2);
    });
    var ro = At(function(e) {
      return (e = r(e)) && e / Zn(e);
    });
    ro.invert = It(function(e) {
      return e;
    }), Pt.invert = function(e, t) {
      return [e, 2 * Yn(Kn(t)) - Un];
    }, zt.invert = zt;
    var ao = 1.340264,
      lo = -.081106,
      so = 893e-6,
      co = .003796,
      uo = Jn(3) / 2,
      fo = 12;
    Bt.invert = function(e, t) {
        for (var n, i, o, r = t, l = r * r, s = l * l * l, d = 0; d < fo && (i = r * (ao + lo * l + s * (
              so + co * l)) - t, o = ao + 3 * lo * l + s * (7 * so + 9 * co * l), r -= n = i / o, l = r * r,
            s = l * l * l, !(Bn(n) < Ln)); ++d);
        return [uo * e * (ao + 3 * lo * l + s * (7 * so + 9 * co * l)) / Wn(r), a(Zn(r) / uo)];
      }, $t.invert = It(Yn), Kt.invert = function(e, t) {
        var n,
          i = t,
          o = 25;
        do {
          var r = i * i,
            a = r * r;
          i -= n = (i * (1.007226 + r * (.015085 + a * (-.044475 + .028874 * r - .005916 * a))) - t) / (
            1.007226 + r * (.045255 + a * (-.311325 + .259866 * r - .005916 * 11 * a)));
        } while (Bn(n) > Nn && --o > 0);
        return [e / (.8707 + (r = i * i) * (-.131979 + r * (-.013791 + r * r * r * (.003971 - .001529 *
          r)))), i];
      }, Xt.invert = It(a), Qt.invert = It(function(e) {
        return 2 * Yn(e);
      }), en.invert = function(e, t) {
        return [-t, 2 * Yn(Kn(e)) - Un];
      }, e.geoAlbers = Tt, e.geoAlbersUsa = Ot, e.geoArea = b, e.geoAzimuthalEqualArea = Mt, e
      .geoAzimuthalEqualAreaRaw = oo, e.geoAzimuthalEquidistant = Rt, e.geoAzimuthalEquidistantRaw = ro, e
      .geoBounds = N, e.geoCentroid = W, e.geoCircle = ie, e.geoClipAntimeridian = wi, e.geoClipCircle = be,
      e.geoClipExtent = ye, e.geoClipRectangle = ve, e.geoConicConformal = Ut, e.geoConicConformalRaw = Ft,
      e.geoConicEqualArea = _t, e.geoConicEqualAreaRaw = kt, e.geoConicEquidistant = Ht, e
      .geoConicEquidistantRaw = Vt, e.geoContains = Pe, e.geoDistance = Te, e.geoEqualEarth = Yt, e
      .geoEqualEarthRaw = Bt, e.geoEquirectangular = Gt, e.geoEquirectangularRaw = zt, e.geoGnomonic = Wt, e
      .geoGnomonicRaw = $t, e.geoGraticule = Le, e.geoGraticule10 = Fe, e.geoIdentity = jt, e
      .geoInterpolate = Ue, e.geoLength = _e, e.geoMercator = Dt, e.geoMercatorRaw = Pt, e
      .geoNaturalEarth1 = qt, e.geoNaturalEarth1Raw = Kt, e.geoOrthographic = Zt, e.geoOrthographicRaw = Xt,
      e.geoPath = rt, e.geoProjection = yt, e.geoProjectionMutator = wt, e.geoRotation = ee, e
      .geoStereographic = Jt, e.geoStereographicRaw = Qt, e.geoStream = f, e.geoTransform = at, e
      .geoTransverseMercator = tn, e.geoTransverseMercatorRaw = en, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
