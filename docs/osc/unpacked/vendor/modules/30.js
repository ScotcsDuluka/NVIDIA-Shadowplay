// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 30
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
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
      return 1 === e.length && (e = r(e)), {
        left: function(t, n, r, i) {
          for (null == r && (r = 0), null == i && (i = t.length); r < i;) {
            var o = r + i >>> 1;
            e(t[o], n) < 0 ? r = o + 1 : i = o
          }
          return r
        },
        right: function(t, n, r, i) {
          for (null == r && (r = 0), null == i && (i = t.length); r < i;) {
            var o = r + i >>> 1;
            e(t[o], n) > 0 ? i = o : r = o + 1
          }
          return r
        }
      }
    }

    function r(e) {
      return function(n, r) {
        return t(e(n), r)
      }
    }

    function i(e, t) {
      null == t && (t = o);
      for (var n = 0, r = e.length - 1, i = e[0], a = new Array(r < 0 ? 0 : r); n < r;) a[n] = t(i, i = e[++n]);
      return a
    }

    function o(e, t) {
      return [e, t]
    }

    function a(e, t, n) {
      var r, i, a, s, c = e.length,
        u = t.length,
        l = new Array(c * u);
      for (null == n && (n = o), r = a = 0; r < c; ++r)
        for (s = e[r], i = 0; i < u; ++i, ++a) l[a] = n(s, t[i]);
      return l
    }

    function s(e, t) {
      return t < e ? -1 : t > e ? 1 : t >= e ? 0 : NaN
    }

    function c(e) {
      return null === e ? NaN : +e
    }

    function u(e, t) {
      var n, r, i = e.length,
        o = 0,
        a = -1,
        s = 0,
        u = 0;
      if (null == t)
        for (; ++a < i;) isNaN(n = c(e[a])) || (r = n - s, s += r / ++o, u += r * (n - s));
      else
        for (; ++a < i;) isNaN(n = c(t(e[a], a, e))) || (r = n - s, s += r / ++o, u += r * (n - s));
      if (o > 1) return u / (o - 1)
    }

    function l(e, t) {
      var n = u(e, t);
      return n ? Math.sqrt(n) : n
    }

    function d(e, t) {
      var n, r, i, o = e.length,
        a = -1;
      if (null == t) {
        for (; ++a < o;)
          if (null != (n = e[a]) && n >= n)
            for (r = i = n; ++a < o;) null != (n = e[a]) && (r > n && (r = n), i < n && (i = n))
      } else
        for (; ++a < o;)
          if (null != (n = t(e[a], a, e)) && n >= n)
            for (r = i = n; ++a < o;) null != (n = t(e[a], a, e)) && (r > n && (r = n), i < n && (i = n));
      return [r, i]
    }

    function f(e) {
      return function() {
        return e
      }
    }

    function h(e) {
      return e
    }

    function p(e, t, n) {
      e = +e, t = +t, n = (i = arguments.length) < 2 ? (t = e, e = 0, 1) : i < 3 ? 1 : +n;
      for (var r = -1, i = 0 | Math.max(0, Math.ceil((t - e) / n)), o = new Array(i); ++r < i;) o[r] = e + r * n;
      return o
    }

    function m(e, t, n) {
      var r, i, o, a, s = -1;
      if (t = +t, e = +e, n = +n, e === t && n > 0) return [e];
      if ((r = t < e) && (i = e, e = t, t = i), 0 === (a = v(e, t, n)) || !isFinite(a)) return [];
      if (a > 0)
        for (e = Math.ceil(e / a), t = Math.floor(t / a), o = new Array(i = Math.ceil(t - e + 1)); ++s < i;) o[s] = (
          e + s) * a;
      else
        for (e = Math.floor(e * a), t = Math.ceil(t * a), o = new Array(i = Math.ceil(e - t + 1)); ++s < i;) o[s] = (
          e - s) / a;
      return r && o.reverse(), o
    }

    function v(e, t, n) {
      var r = (t - e) / Math.max(0, n),
        i = Math.floor(Math.log(r) / Math.LN10),
        o = r / Math.pow(10, i);
      return i >= 0 ? (o >= H ? 10 : o >= B ? 5 : o >= z ? 2 : 1) * Math.pow(10, i) : -Math.pow(10, -i) / (o >= H ?
        10 : o >= B ? 5 : o >= z ? 2 : 1)
    }

    function g(e, t, n) {
      var r = Math.abs(t - e) / Math.max(0, n),
        i = Math.pow(10, Math.floor(Math.log(r) / Math.LN10)),
        o = r / i;
      return o >= H ? i *= 10 : o >= B ? i *= 5 : o >= z && (i *= 2), t < e ? -i : i
    }

    function y(e) {
      return Math.ceil(Math.log(e.length) / Math.LN2) + 1
    }

    function b() {
      function e(e) {
        var i, o, a = e.length,
          s = new Array(a);
        for (i = 0; i < a; ++i) s[i] = t(e[i], i, e);
        var c = n(s),
          u = c[0],
          l = c[1],
          d = r(s, u, l);
        Array.isArray(d) || (d = g(u, l, d), d = p(Math.ceil(u / d) * d, l, d));
        for (var f = d.length; d[0] <= u;) d.shift(), --f;
        for (; d[f - 1] > l;) d.pop(), --f;
        var h, m = new Array(f + 1);
        for (i = 0; i <= f; ++i) h = m[i] = [], h.x0 = i > 0 ? d[i - 1] : u, h.x1 = i < f ? d[i] : l;
        for (i = 0; i < a; ++i) o = s[i], u <= o && o <= l && m[P(d, o, 0, f)].push(e[i]);
        return m
      }
      var t = h,
        n = d,
        r = y;
      return e.value = function(n) {
        return arguments.length ? (t = "function" == typeof n ? n : f(n), e) : t
      }, e.domain = function(t) {
        return arguments.length ? (n = "function" == typeof t ? t : f([t[0], t[1]]), e) : n
      }, e.thresholds = function(t) {
        return arguments.length ? (r = "function" == typeof t ? t : f(Array.isArray(t) ? F.call(t) : t), e) : r
      }, e
    }

    function E(e, t, n) {
      if (null == n && (n = c), r = e.length) {
        if ((t = +t) <= 0 || r < 2) return +n(e[0], 0, e);
        if (t >= 1) return +n(e[r - 1], r - 1, e);
        var r, i = (r - 1) * t,
          o = Math.floor(i),
          a = +n(e[o], o, e),
          s = +n(e[o + 1], o + 1, e);
        return a + (s - a) * (i - o)
      }
    }

    function _(e, n, r) {
      return e = j.call(e, c).sort(t), Math.ceil((r - n) / (2 * (E(e, .75) - E(e, .25)) * Math.pow(e.length, -1 / 3)))
    }

    function $(e, t, n) {
      return Math.ceil((n - t) / (3.5 * l(e) * Math.pow(e.length, -1 / 3)))
    }

    function w(e, t) {
      var n, r, i = e.length,
        o = -1;
      if (null == t) {
        for (; ++o < i;)
          if (null != (n = e[o]) && n >= n)
            for (r = n; ++o < i;) null != (n = e[o]) && n > r && (r = n)
      } else
        for (; ++o < i;)
          if (null != (n = t(e[o], o, e)) && n >= n)
            for (r = n; ++o < i;) null != (n = t(e[o], o, e)) && n > r && (r = n);
      return r
    }

    function T(e, t) {
      var n, r = e.length,
        i = r,
        o = -1,
        a = 0;
      if (null == t)
        for (; ++o < r;) isNaN(n = c(e[o])) ? --i : a += n;
      else
        for (; ++o < r;) isNaN(n = c(t(e[o], o, e))) ? --i : a += n;
      if (i) return a / i
    }

    function C(e, n) {
      var r, i = e.length,
        o = -1,
        a = [];
      if (null == n)
        for (; ++o < i;) isNaN(r = c(e[o])) || a.push(r);
      else
        for (; ++o < i;) isNaN(r = c(n(e[o], o, e))) || a.push(r);
      return E(a.sort(t), .5)
    }

    function x(e) {
      for (var t, n, r, i = e.length, o = -1, a = 0; ++o < i;) a += e[o].length;
      for (n = new Array(a); --i >= 0;)
        for (r = e[i], t = r.length; --t >= 0;) n[--a] = r[t];
      return n
    }

    function S(e, t) {
      var n, r, i = e.length,
        o = -1;
      if (null == t) {
        for (; ++o < i;)
          if (null != (n = e[o]) && n >= n)
            for (r = n; ++o < i;) null != (n = e[o]) && r > n && (r = n)
      } else
        for (; ++o < i;)
          if (null != (n = t(e[o], o, e)) && n >= n)
            for (r = n; ++o < i;) null != (n = t(e[o], o, e)) && r > n && (r = n);
      return r
    }

    function A(e, t) {
      for (var n = t.length, r = new Array(n); n--;) r[n] = e[t[n]];
      return r
    }

    function M(e, n) {
      if (r = e.length) {
        var r, i, o = 0,
          a = 0,
          s = e[a];
        for (null == n && (n = t); ++o < r;)(n(i = e[o], s) < 0 || 0 !== n(s, s)) && (s = i, a = o);
        return 0 === n(s, s) ? a : void 0
      }
    }

    function k(e, t, n) {
      for (var r, i, o = (null == n ? e.length : n) - (t = null == t ? 0 : +t); o;) i = Math.random() * o-- | 0, r =
        e[o + t], e[o + t] = e[i + t], e[i + t] = r;
      return e
    }

    function N(e, t) {
      var n, r = e.length,
        i = -1,
        o = 0;
      if (null == t)
        for (; ++i < r;)(n = +e[i]) && (o += n);
      else
        for (; ++i < r;)(n = +t(e[i], i, e)) && (o += n);
      return o
    }

    function I(e) {
      if (!(i = e.length)) return [];
      for (var t = -1, n = S(e, O), r = new Array(n); ++t < n;)
        for (var i, o = -1, a = r[t] = new Array(i); ++o < i;) a[o] = e[o][t];
      return r
    }

    function O(e) {
      return e.length
    }

    function D() {
      return I(arguments)
    }
    var R = n(t),
      P = R.right,
      L = R.left,
      U = Array.prototype,
      F = U.slice,
      j = U.map,
      H = Math.sqrt(50),
      B = Math.sqrt(10),
      z = Math.sqrt(2);
    e.bisect = P, e.bisectRight = P, e.bisectLeft = L, e.ascending = t, e.bisector = n, e.cross = a, e.descending = s,
      e.deviation = l, e.extent = d, e.histogram = b, e.thresholdFreedmanDiaconis = _, e.thresholdScott = $, e
      .thresholdSturges = y, e.max = w, e.mean = T, e.median = C, e.merge = x, e.min = S, e.pairs = i, e.permute = A,
      e.quantile = E, e.range = p, e.scan = M, e.shuffle = k, e.sum = N, e.ticks = m, e.tickIncrement = v, e
      .tickStep = g, e.transpose = I, e.variance = u, e.zip = D, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
