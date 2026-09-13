// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 21
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, r) {
    r(t, n(44))
  }(this, function(e, t) {
    "use strict";

    function n(e, t, n, r, i) {
      var o = e * e,
        a = o * e;
      return ((1 - 3 * e + 3 * o - a) * t + (4 - 6 * o + 3 * a) * n + (1 + 3 * e + 3 * o - 3 * a) * r + a * i) / 6
    }

    function r(e) {
      var t = e.length - 1;
      return function(r) {
        var i = r <= 0 ? r = 0 : r >= 1 ? (r = 1, t - 1) : Math.floor(r * t),
          o = e[i],
          a = e[i + 1],
          s = i > 0 ? e[i - 1] : 2 * o - a,
          c = i < t - 1 ? e[i + 2] : 2 * a - o;
        return n((r - i / t) * t, s, o, a, c)
      }
    }

    function i(e) {
      var t = e.length;
      return function(r) {
        var i = Math.floor(((r %= 1) < 0 ? ++r : r) * t),
          o = e[(i + t - 1) % t],
          a = e[i % t],
          s = e[(i + 1) % t],
          c = e[(i + 2) % t];
        return n((r - i / t) * t, o, a, s, c)
      }
    }

    function o(e) {
      return function() {
        return e
      }
    }

    function a(e, t) {
      return function(n) {
        return e + n * t
      }
    }

    function s(e, t, n) {
      return e = Math.pow(e, n), t = Math.pow(t, n) - e, n = 1 / n,
        function(r) {
          return Math.pow(e + r * t, n)
        }
    }

    function c(e, t) {
      var n = t - e;
      return n ? a(e, n > 180 || n < -180 ? n - 360 * Math.round(n / 360) : n) : o(isNaN(e) ? t : e)
    }

    function u(e) {
      return 1 === (e = +e) ? l : function(t, n) {
        return n - t ? s(t, n, e) : o(isNaN(t) ? n : t)
      }
    }

    function l(e, t) {
      var n = t - e;
      return n ? a(e, n) : o(isNaN(e) ? t : e)
    }

    function d(e) {
      return function(n) {
        var r, i, o = n.length,
          a = new Array(o),
          s = new Array(o),
          c = new Array(o);
        for (r = 0; r < o; ++r) i = t.rgb(n[r]), a[r] = i.r || 0, s[r] = i.g || 0, c[r] = i.b || 0;
        return a = e(a), s = e(s), c = e(c), i.opacity = 1,
          function(e) {
            return i.r = a(e), i.g = s(e), i.b = c(e), i + ""
          }
      }
    }

    function f(e, t) {
      t || (t = []);
      var n, r = e ? Math.min(t.length, e.length) : 0,
        i = t.slice();
      return function(o) {
        for (n = 0; n < r; ++n) i[n] = e[n] * (1 - o) + t[n] * o;
        return i
      }
    }

    function h(e) {
      return ArrayBuffer.isView(e) && !(e instanceof DataView)
    }

    function p(e, t) {
      return (h(t) ? f : m)(e, t)
    }

    function m(e, t) {
      var n, r = t ? t.length : 0,
        i = e ? Math.min(r, e.length) : 0,
        o = new Array(i),
        a = new Array(r);
      for (n = 0; n < i; ++n) o[n] = $(e[n], t[n]);
      for (; n < r; ++n) a[n] = t[n];
      return function(e) {
        for (n = 0; n < i; ++n) a[n] = o[n](e);
        return a
      }
    }

    function v(e, t) {
      var n = new Date;
      return e = +e, t = +t,
        function(r) {
          return n.setTime(e * (1 - r) + t * r), n
        }
    }

    function g(e, t) {
      return e = +e, t = +t,
        function(n) {
          return e * (1 - n) + t * n
        }
    }

    function y(e, t) {
      var n, r = {},
        i = {};
      null !== e && "object" == typeof e || (e = {}), null !== t && "object" == typeof t || (t = {});
      for (n in t) n in e ? r[n] = $(e[n], t[n]) : i[n] = t[n];
      return function(e) {
        for (n in r) i[n] = r[n](e);
        return i
      }
    }

    function b(e) {
      return function() {
        return e
      }
    }

    function E(e) {
      return function(t) {
        return e(t) + ""
      }
    }

    function _(e, t) {
      var n, r, i, o = W.lastIndex = Y.lastIndex = 0,
        a = -1,
        s = [],
        c = [];
      for (e += "", t += "";
        (n = W.exec(e)) && (r = Y.exec(t));)(i = r.index) > o && (i = t.slice(o, i), s[a] ? s[a] += i : s[++a] = i), (
        n = n[0]) === (r = r[0]) ? s[a] ? s[a] += r : s[++a] = r : (s[++a] = null, c.push({
        i: a,
        x: g(n, r)
      })), o = Y.lastIndex;
      return o < t.length && (i = t.slice(o), s[a] ? s[a] += i : s[++a] = i), s.length < 2 ? c[0] ? E(c[0].x) : b(t) :
        (t = c.length, function(e) {
          for (var n, r = 0; r < t; ++r) s[(n = c[r]).i] = n.x(e);
          return s.join("")
        })
    }

    function $(e, n) {
      var r, i = typeof n;
      return null == n || "boolean" === i ? o(n) : ("number" === i ? g : "string" === i ? (r = t.color(n)) ? (n = r,
          q) : _ : n instanceof t.color ? q : n instanceof Date ? v : h(n) ? f : Array.isArray(n) ? m :
        "function" != typeof n.valueOf && "function" != typeof n.toString || isNaN(n) ? y : g)(e, n)
    }

    function w(e) {
      var t = e.length;
      return function(n) {
        return e[Math.max(0, Math.min(t - 1, Math.floor(n * t)))]
      }
    }

    function T(e, t) {
      var n = c(+e, +t);
      return function(e) {
        var t = n(e);
        return t - 360 * Math.floor(t / 360)
      }
    }

    function C(e, t) {
      return e = +e, t = +t,
        function(n) {
          return Math.round(e * (1 - n) + t * n)
        }
    }

    function x(e, t, n, r, i, o) {
      var a, s, c;
      return (a = Math.sqrt(e * e + t * t)) && (e /= a, t /= a), (c = e * n + t * r) && (n -= e * c, r -= t * c), (s =
        Math.sqrt(n * n + r * r)) && (n /= s, r /= s, c /= s), e * r < t * n && (e = -e, t = -t, c = -c, a = -a), {
        translateX: i,
        translateY: o,
        rotate: Math.atan2(t, e) * K,
        skewX: Math.atan(c) * K,
        scaleX: a,
        scaleY: s
      }
    }

    function S(e) {
      return "none" === e ? X : (j || (j = document.createElement("DIV"), H = document.documentElement, B = document
          .defaultView), j.style.transform = e, e = B.getComputedStyle(H.appendChild(j), null).getPropertyValue(
          "transform"), H.removeChild(j), e = e.slice(7, -1).split(","), x(+e[0], +e[1], +e[2], +e[3], +e[4], +e[
        5]))
    }

    function A(e) {
      return null == e ? X : (z || (z = document.createElementNS("http://www.w3.org/2000/svg", "g")), z.setAttribute(
          "transform", e), (e = z.transform.baseVal.consolidate()) ? (e = e.matrix, x(e.a, e.b, e.c, e.d, e.e, e
        .f)) : X)
    }

    function M(e, t, n, r) {
      function i(e) {
        return e.length ? e.pop() + " " : ""
      }

      function o(e, r, i, o, a, s) {
        if (e !== i || r !== o) {
          var c = a.push("translate(", null, t, null, n);
          s.push({
            i: c - 4,
            x: g(e, i)
          }, {
            i: c - 2,
            x: g(r, o)
          })
        } else(i || o) && a.push("translate(" + i + t + o + n)
      }

      function a(e, t, n, o) {
        e !== t ? (e - t > 180 ? t += 360 : t - e > 180 && (e += 360), o.push({
          i: n.push(i(n) + "rotate(", null, r) - 2,
          x: g(e, t)
        })) : t && n.push(i(n) + "rotate(" + t + r)
      }

      function s(e, t, n, o) {
        e !== t ? o.push({
          i: n.push(i(n) + "skewX(", null, r) - 2,
          x: g(e, t)
        }) : t && n.push(i(n) + "skewX(" + t + r)
      }

      function c(e, t, n, r, o, a) {
        if (e !== n || t !== r) {
          var s = o.push(i(o) + "scale(", null, ",", null, ")");
          a.push({
            i: s - 4,
            x: g(e, n)
          }, {
            i: s - 2,
            x: g(t, r)
          })
        } else 1 === n && 1 === r || o.push(i(o) + "scale(" + n + "," + r + ")")
      }
      return function(t, n) {
        var r = [],
          i = [];
        return t = e(t), n = e(n), o(t.translateX, t.translateY, n.translateX, n.translateY, r, i), a(t.rotate, n
            .rotate, r, i), s(t.skewX, n.skewX, r, i), c(t.scaleX, t.scaleY, n.scaleX, n.scaleY, r, i), t = n =
          null,
          function(e) {
            for (var t, n = -1, o = i.length; ++n < o;) r[(t = i[n]).i] = t.x(e);
            return r.join("")
          }
      }
    }

    function k(e) {
      return ((e = Math.exp(e)) + 1 / e) / 2
    }

    function N(e) {
      return ((e = Math.exp(e)) - 1 / e) / 2
    }

    function I(e) {
      return ((e = Math.exp(2 * e)) - 1) / (e + 1)
    }

    function O(e, t) {
      var n, r, i = e[0],
        o = e[1],
        a = e[2],
        s = t[0],
        c = t[1],
        u = t[2],
        l = s - i,
        d = c - o,
        f = l * l + d * d;
      if (f < ne) r = Math.log(u / a) / Z, n = function(e) {
        return [i + e * l, o + e * d, a * Math.exp(Z * e * r)]
      };
      else {
        var h = Math.sqrt(f),
          p = (u * u - a * a + te * f) / (2 * a * ee * h),
          m = (u * u - a * a - te * f) / (2 * u * ee * h),
          v = Math.log(Math.sqrt(p * p + 1) - p),
          g = Math.log(Math.sqrt(m * m + 1) - m);
        r = (g - v) / Z, n = function(e) {
          var t = e * r,
            n = k(v),
            s = a / (ee * h) * (n * I(Z * t + v) - N(v));
          return [i + s * l, o + s * d, a * n / k(Z * t + v)]
        }
      }
      return n.duration = 1e3 * r, n
    }

    function D(e) {
      return function(n, r) {
        var i = e((n = t.hsl(n)).h, (r = t.hsl(r)).h),
          o = l(n.s, r.s),
          a = l(n.l, r.l),
          s = l(n.opacity, r.opacity);
        return function(e) {
          return n.h = i(e), n.s = o(e), n.l = a(e), n.opacity = s(e), n + ""
        }
      }
    }

    function R(e, n) {
      var r = l((e = t.lab(e)).l, (n = t.lab(n)).l),
        i = l(e.a, n.a),
        o = l(e.b, n.b),
        a = l(e.opacity, n.opacity);
      return function(t) {
        return e.l = r(t), e.a = i(t), e.b = o(t), e.opacity = a(t), e + ""
      }
    }

    function P(e) {
      return function(n, r) {
        var i = e((n = t.hcl(n)).h, (r = t.hcl(r)).h),
          o = l(n.c, r.c),
          a = l(n.l, r.l),
          s = l(n.opacity, r.opacity);
        return function(e) {
          return n.h = i(e), n.c = o(e), n.l = a(e), n.opacity = s(e), n + ""
        }
      }
    }

    function L(e) {
      return function n(r) {
        function i(n, i) {
          var o = e((n = t.cubehelix(n)).h, (i = t.cubehelix(i)).h),
            a = l(n.s, i.s),
            s = l(n.l, i.l),
            c = l(n.opacity, i.opacity);
          return function(e) {
            return n.h = o(e), n.s = a(e), n.l = s(Math.pow(e, r)), n.opacity = c(e), n + ""
          }
        }
        return r = +r, i.gamma = n, i
      }(1)
    }

    function U(e, t) {
      for (var n = 0, r = t.length - 1, i = t[0], o = new Array(r < 0 ? 0 : r); n < r;) o[n] = e(i, i = t[++n]);
      return function(e) {
        var t = Math.max(0, Math.min(r - 1, Math.floor(e *= r)));
        return o[t](e - t)
      }
    }

    function F(e, t) {
      for (var n = new Array(t), r = 0; r < t; ++r) n[r] = e(r / (t - 1));
      return n
    }
    var j, H, B, z, q = function e(n) {
        function r(e, n) {
          var r = i((e = t.rgb(e)).r, (n = t.rgb(n)).r),
            o = i(e.g, n.g),
            a = i(e.b, n.b),
            s = l(e.opacity, n.opacity);
          return function(t) {
            return e.r = r(t), e.g = o(t), e.b = a(t), e.opacity = s(t), e + ""
          }
        }
        var i = u(n);
        return r.gamma = e, r
      }(1),
      G = d(r),
      V = d(i),
      W = /[-+]?(?:\d+\.?\d*|\.?\d+)(?:[eE][-+]?\d+)?/g,
      Y = new RegExp(W.source, "g"),
      K = 180 / Math.PI,
      X = {
        translateX: 0,
        translateY: 0,
        rotate: 0,
        skewX: 0,
        scaleX: 1,
        scaleY: 1
      },
      Q = M(S, "px, ", "px)", "deg)"),
      J = M(A, ", ", ")", ")"),
      Z = Math.SQRT2,
      ee = 2,
      te = 4,
      ne = 1e-12,
      re = D(c),
      ie = D(l),
      oe = P(c),
      ae = P(l),
      se = L(c),
      ce = L(l);
    e.interpolate = $, e.interpolateArray = p, e.interpolateBasis = r, e.interpolateBasisClosed = i, e
      .interpolateCubehelix = se, e.interpolateCubehelixLong = ce, e.interpolateDate = v, e.interpolateDiscrete = w, e
      .interpolateHcl = oe, e.interpolateHclLong = ae, e.interpolateHsl = re, e.interpolateHslLong = ie, e
      .interpolateHue = T, e.interpolateLab = R, e.interpolateNumber = g, e.interpolateNumberArray = f, e
      .interpolateObject = y, e.interpolateRgb = q, e.interpolateRgbBasis = G, e.interpolateRgbBasisClosed = V, e
      .interpolateRound = C, e.interpolateString = _, e.interpolateTransformCss = Q, e.interpolateTransformSvg = J, e
      .interpolateZoom = O, e.piecewise = U, e.quantize = F, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
