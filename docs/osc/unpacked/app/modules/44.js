// ─────────────────────────────────────────────────────────────
// APP MODULE 44
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t(e, t) {
      return e < t ? -1 : e > t ? 1 : e >= t ? 0 : NaN
    }

    function n(e) {
      return 1 === e.length && (e = i(e)), {
        left: function(t, n, i, o) {
          for (null == i && (i = 0), null == o && (o = t.length); i < o;) {
            var r = i + o >>> 1;
            e(t[r], n) < 0 ? i = r + 1 : o = r
          }
          return i
        },
        right: function(t, n, i, o) {
          for (null == i && (i = 0), null == o && (o = t.length); i < o;) {
            var r = i + o >>> 1;
            e(t[r], n) > 0 ? o = r : i = r + 1
          }
          return i
        }
      }
    }

    function i(e) {
      return function(n, i) {
        return t(e(n), i)
      }
    }

    function o(e, t) {
      null == t && (t = r);
      for (var n = 0, i = e.length - 1, o = e[0], a = new Array(i < 0 ? 0 : i); n < i;) a[n] = t(o, o = e[++n]);
      return a
    }

    function r(e, t) {
      return [e, t]
    }

    function a(e, t, n) {
      var i, o, a, l, s = e.length,
        d = t.length,
        c = new Array(s * d);
      for (null == n && (n = r), i = a = 0; i < s; ++i)
        for (l = e[i], o = 0; o < d; ++o, ++a) c[a] = n(l, t[o]);
      return c
    }

    function l(e, t) {
      return t < e ? -1 : t > e ? 1 : t >= e ? 0 : NaN
    }

    function s(e) {
      return null === e ? NaN : +e
    }

    function d(e, t) {
      var n, i, o = e.length,
        r = 0,
        a = -1,
        l = 0,
        d = 0;
      if (null == t)
        for (; ++a < o;) isNaN(n = s(e[a])) || (i = n - l, l += i / ++r, d += i * (n - l));
      else
        for (; ++a < o;) isNaN(n = s(t(e[a], a, e))) || (i = n - l, l += i / ++r, d += i * (n - l));
      if (r > 1) return d / (r - 1)
    }

    function c(e, t) {
      var n = d(e, t);
      return n ? Math.sqrt(n) : n
    }

    function u(e, t) {
      var n, i, o, r = e.length,
        a = -1;
      if (null == t) {
        for (; ++a < r;)
          if (null != (n = e[a]) && n >= n)
            for (i = o = n; ++a < r;) null != (n = e[a]) && (i > n && (i = n), o < n && (o = n))
      } else
        for (; ++a < r;)
          if (null != (n = t(e[a], a, e)) && n >= n)
            for (i = o = n; ++a < r;) null != (n = t(e[a], a, e)) && (i > n && (i = n), o < n && (o = n));
      return [i, o]
    }

    function f(e) {
      return function() {
        return e
      }
    }

    function m(e) {
      return e
    }

    function g(e, t, n) {
      e = +e, t = +t, n = (o = arguments.length) < 2 ? (t = e, e = 0, 1) : o < 3 ? 1 : +n;
      for (var i = -1, o = 0 | Math.max(0, Math.ceil((t - e) / n)), r = new Array(o); ++i < o;) r[i] = e + i * n;
      return r
    }

    function p(e, t, n) {
      var i, o, r, a, l = -1;
      if (t = +t, e = +e, n = +n, e === t && n > 0) return [e];
      if ((i = t < e) && (o = e, e = t, t = o), 0 === (a = h(e, t, n)) || !isFinite(a)) return [];
      if (a > 0)
        for (e = Math.ceil(e / a), t = Math.floor(t / a), r = new Array(o = Math.ceil(t - e + 1)); ++l < o;) r[l] = (
          e + l) * a;
      else
        for (e = Math.floor(e * a), t = Math.ceil(t * a), r = new Array(o = Math.ceil(e - t + 1)); ++l < o;) r[l] = (
          e - l) / a;
      return i && r.reverse(), r
    }

    function h(e, t, n) {
      var i = (t - e) / Math.max(0, n),
        o = Math.floor(Math.log(i) / Math.LN10),
        r = i / Math.pow(10, o);
      return o >= 0 ? (r >= V ? 10 : r >= H ? 5 : r >= B ? 2 : 1) * Math.pow(10, o) : -Math.pow(10, -o) / (r >= V ?
        10 : r >= H ? 5 : r >= B ? 2 : 1)
    }

    function b(e, t, n) {
      var i = Math.abs(t - e) / Math.max(0, n),
        o = Math.pow(10, Math.floor(Math.log(i) / Math.LN10)),
        r = i / o;
      return r >= V ? o *= 10 : r >= H ? o *= 5 : r >= B && (o *= 2), t < e ? -o : o
    }

    function x(e) {
      return Math.ceil(Math.log(e.length) / Math.LN2) + 1
    }

    function v() {
      function e(e) {
        var o, r, a = e.length,
          l = new Array(a);
        for (o = 0; o < a; ++o) l[o] = t(e[o], o, e);
        var s = n(l),
          d = s[0],
          c = s[1],
          u = i(l, d, c);
        Array.isArray(u) || (u = b(d, c, u), u = g(Math.ceil(d / u) * u, c, u));
        for (var f = u.length; u[0] <= d;) u.shift(), --f;
        for (; u[f - 1] > c;) u.pop(), --f;
        var m, p = new Array(f + 1);
        for (o = 0; o <= f; ++o) m = p[o] = [], m.x0 = o > 0 ? u[o - 1] : d, m.x1 = o < f ? u[o] : c;
        for (o = 0; o < a; ++o) r = l[o], d <= r && r <= c && p[L(u, r, 0, f)].push(e[o]);
        return p
      }
      var t = m,
        n = u,
        i = x;
      return e.value = function(n) {
        return arguments.length ? (t = "function" == typeof n ? n : f(n), e) : t
      }, e.domain = function(t) {
        return arguments.length ? (n = "function" == typeof t ? t : f([t[0], t[1]]), e) : n
      }, e.thresholds = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : f(Array.isArray(t) ? z.call(t) : t), e) : i
      }, e
    }

    function y(e, t, n) {
      if (null == n && (n = s), i = e.length) {
        if ((t = +t) <= 0 || i < 2) return +n(e[0], 0, e);
        if (t >= 1) return +n(e[i - 1], i - 1, e);
        var i, o = (i - 1) * t,
          r = Math.floor(o),
          a = +n(e[r], r, e),
          l = +n(e[r + 1], r + 1, e);
        return a + (l - a) * (o - r)
      }
    }

    function w(e, n, i) {
      return e = G.call(e, s).sort(t), Math.ceil((i - n) / (2 * (y(e, .75) - y(e, .25)) * Math.pow(e.length, -1 / 3)))
    }

    function S(e, t, n) {
      return Math.ceil((n - t) / (3.5 * c(e) * Math.pow(e.length, -1 / 3)))
    }

    function E(e, t) {
      var n, i, o = e.length,
        r = -1;
      if (null == t) {
        for (; ++r < o;)
          if (null != (n = e[r]) && n >= n)
            for (i = n; ++r < o;) null != (n = e[r]) && n > i && (i = n)
      } else
        for (; ++r < o;)
          if (null != (n = t(e[r], r, e)) && n >= n)
            for (i = n; ++r < o;) null != (n = t(e[r], r, e)) && n > i && (i = n);
      return i
    }

    function k(e, t) {
      var n, i = e.length,
        o = i,
        r = -1,
        a = 0;
      if (null == t)
        for (; ++r < i;) isNaN(n = s(e[r])) ? --o : a += n;
      else
        for (; ++r < i;) isNaN(n = s(t(e[r], r, e))) ? --o : a += n;
      if (o) return a / o
    }

    function _(e, n) {
      var i, o = e.length,
        r = -1,
        a = [];
      if (null == n)
        for (; ++r < o;) isNaN(i = s(e[r])) || a.push(i);
      else
        for (; ++r < o;) isNaN(i = s(n(e[r], r, e))) || a.push(i);
      return y(a.sort(t), .5)
    }

    function T(e) {
      for (var t, n, i, o = e.length, r = -1, a = 0; ++r < o;) a += e[r].length;
      for (n = new Array(a); --o >= 0;)
        for (i = e[o], t = i.length; --t >= 0;) n[--a] = i[t];
      return n
    }

    function C(e, t) {
      var n, i, o = e.length,
        r = -1;
      if (null == t) {
        for (; ++r < o;)
          if (null != (n = e[r]) && n >= n)
            for (i = n; ++r < o;) null != (n = e[r]) && i > n && (i = n)
      } else
        for (; ++r < o;)
          if (null != (n = t(e[r], r, e)) && n >= n)
            for (i = n; ++r < o;) null != (n = t(e[r], r, e)) && i > n && (i = n);
      return i
    }

    function O(e, t) {
      for (var n = t.length, i = new Array(n); n--;) i[n] = e[t[n]];
      return i
    }

    function A(e, n) {
      if (i = e.length) {
        var i, o, r = 0,
          a = 0,
          l = e[a];
        for (null == n && (n = t); ++r < i;)(n(o = e[r], l) < 0 || 0 !== n(l, l)) && (l = o, a = r);
        return 0 === n(l, l) ? a : void 0
      }
    }

    function I(e, t, n) {
      for (var i, o, r = (null == n ? e.length : n) - (t = null == t ? 0 : +t); r;) o = Math.random() * r-- | 0, i =
        e[r + t], e[r + t] = e[o + t], e[o + t] = i;
      return e
    }

    function M(e, t) {
      var n, i = e.length,
        o = -1,
        r = 0;
      if (null == t)
        for (; ++o < i;)(n = +e[o]) && (r += n);
      else
        for (; ++o < i;)(n = +t(e[o], o, e)) && (r += n);
      return r
    }

    function R(e) {
      if (!(o = e.length)) return [];
      for (var t = -1, n = C(e, P), i = new Array(n); ++t < n;)
        for (var o, r = -1, a = i[t] = new Array(o); ++r < o;) a[r] = e[r][t];
      return i
    }

    function P(e) {
      return e.length
    }

    function D() {
      return R(arguments)
    }
    var N = n(t),
      L = N.right,
      F = N.left,
      U = Array.prototype,
      z = U.slice,
      G = U.map,
      V = Math.sqrt(50),
      H = Math.sqrt(10),
      B = Math.sqrt(2);
    e.bisect = L, e.bisectRight = L, e.bisectLeft = F, e.ascending = t, e.bisector = n, e.cross = a, e.descending = l,
      e.deviation = c, e.extent = u, e.histogram = v, e.thresholdFreedmanDiaconis = w, e.thresholdScott = S, e
      .thresholdSturges = x, e.max = E, e.mean = k, e.median = _, e.merge = T, e.min = C, e.pairs = o, e.permute = O,
      e.quantile = y, e.range = g, e.scan = A, e.shuffle = I, e.sum = M, e.ticks = p, e.tickIncrement = h, e
      .tickStep = b, e.transpose = R, e.variance = d, e.zip = D, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
