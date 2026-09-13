// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 39
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, i) {
    i(exports, require(59));
  }(this, function(e, t) {
    "use strict";

    function n(e, t, n, i, o) {
      var r = e * e,
        a = r * e;
      return ((1 - 3 * e + 3 * r - a) * t + (4 - 6 * r + 3 * a) * n + (1 + 3 * e + 3 * r - 3 * a) * i + a *
        o) / 6;
    }

    function i(e) {
      var t = e.length - 1;
      return function(i) {
        var o = i <= 0 ? i = 0 : i >= 1 ? (i = 1, t - 1) : Math.floor(i * t),
          r = e[o],
          a = e[o + 1],
          l = o > 0 ? e[o - 1] : 2 * r - a,
          s = o < t - 1 ? e[o + 2] : 2 * a - r;
        return n((i - o / t) * t, l, r, a, s);
      };
    }

    function o(e) {
      var t = e.length;
      return function(i) {
        var o = Math.floor(((i %= 1) < 0 ? ++i : i) * t),
          r = e[(o + t - 1) % t],
          a = e[o % t],
          l = e[(o + 1) % t],
          s = e[(o + 2) % t];
        return n((i - o / t) * t, r, a, l, s);
      };
    }

    function r(e) {
      return function() {
        return e;
      };
    }

    function a(e, t) {
      return function(n) {
        return e + n * t;
      };
    }

    function l(e, t, n) {
      return e = Math.pow(e, n), t = Math.pow(t, n) - e, n = 1 / n,
        function(i) {
          return Math.pow(e + i * t, n);
        };
    }

    function s(e, t) {
      var n = t - e;
      return n ? a(e, n > 180 || n < -180 ? n - 360 * Math.round(n / 360) : n) : r(isNaN(e) ? t : e);
    }

    function d(e) {
      return 1 === (e = +e) ? c : function(t, n) {
        return n - t ? l(t, n, e) : r(isNaN(t) ? n : t);
      };
    }

    function c(e, t) {
      var n = t - e;
      return n ? a(e, n) : r(isNaN(e) ? t : e);
    }

    function u(e) {
      return function(n) {
        var i,
          o,
          r = n.length,
          a = new Array(r),
          l = new Array(r),
          s = new Array(r);
        for (i = 0; i < r; ++i) o = t.rgb(n[i]), a[i] = o.r || 0, l[i] = o.g || 0, s[i] = o.b || 0;
        return a = e(a), l = e(l), s = e(s), o.opacity = 1,
          function(e) {
            return o.r = a(e), o.g = l(e), o.b = s(e), o + "";
          };
      };
    }

    function f(e, t) {
      t || (t = []);
      var n,
        i = e ? Math.min(t.length, e.length) : 0,
        o = t.slice();
      return function(r) {
        for (n = 0; n < i; ++n) o[n] = e[n] * (1 - r) + t[n] * r;
        return o;
      };
    }

    function m(e) {
      return ArrayBuffer.isView(e) && !(e instanceof DataView);
    }

    function g(e, t) {
      return (m(t) ? f : p)(e, t);
    }

    function p(e, t) {
      var n,
        i = t ? t.length : 0,
        o = e ? Math.min(i, e.length) : 0,
        r = new Array(o),
        a = new Array(i);
      for (n = 0; n < o; ++n) r[n] = S(e[n], t[n]);
      for (; n < i; ++n) a[n] = t[n];
      return function(e) {
        for (n = 0; n < o; ++n) a[n] = r[n](e);
        return a;
      };
    }

    function h(e, t) {
      var n = new Date();
      return e = +e, t = +t,
        function(i) {
          return n.setTime(e * (1 - i) + t * i), n;
        };
    }

    function b(e, t) {
      return e = +e, t = +t,
        function(n) {
          return e * (1 - n) + t * n;
        };
    }

    function x(e, t) {
      var n,
        i = {},
        o = {};
      null !== e && "object" == typeof e || (e = {}), null !== t && "object" == typeof t || (t = {});
      for (n in t) n in e ? i[n] = S(e[n], t[n]) : o[n] = t[n];
      return function(e) {
        for (n in i) o[n] = i[n](e);
        return o;
      };
    }

    function v(e) {
      return function() {
        return e;
      };
    }

    function y(e) {
      return function(t) {
        return e(t) + "";
      };
    }

    function w(e, t) {
      var n,
        i,
        o,
        r = j.lastIndex = K.lastIndex = 0,
        a = -1,
        l = [],
        s = [];
      for (e += "", t += "";
        (n = j.exec(e)) && (i = K.exec(t));)(o = i.index) > r && (o = t.slice(r, o), l[a] ? l[a] += o : l[++
        a] = o), (n = n[0]) === (i = i[0]) ? l[a] ? l[a] += i : l[++a] = i : (l[++a] = null, s.push({
        i: a,
        x: b(n, i)
      })), r = K.lastIndex;
      return r < t.length && (o = t.slice(r), l[a] ? l[a] += o : l[++a] = o), l.length < 2 ? s[0] ? y(s[0]
        .x) : v(t) : (t = s.length, function(e) {
        for (var n, i = 0; i < t; ++i) l[(n = s[i]).i] = n.x(e);
        return l.join("");
      });
    }

    function S(e, n) {
      var i,
        o = typeof n;
      return null == n || "boolean" === o ? r(n) : ("number" === o ? b : "string" === o ? (i = t.color(n)) ?
        (n = i, Y) : w : n instanceof t.color ? Y : n instanceof Date ? h : m(n) ? f : Array.isArray(n) ?
        p : "function" != typeof n.valueOf && "function" != typeof n.toString || isNaN(n) ? x : b)(e, n);
    }

    function E(e) {
      var t = e.length;
      return function(n) {
        return e[Math.max(0, Math.min(t - 1, Math.floor(n * t)))];
      };
    }

    function k(e, t) {
      var n = s(+e, +t);
      return function(e) {
        var t = n(e);
        return t - 360 * Math.floor(t / 360);
      };
    }

    function _(e, t) {
      return e = +e, t = +t,
        function(n) {
          return Math.round(e * (1 - n) + t * n);
        };
    }

    function T(e, t, n, i, o, r) {
      var a, l, s;
      return (a = Math.sqrt(e * e + t * t)) && (e /= a, t /= a), (s = e * n + t * i) && (n -= e * s, i -=
        t * s), (l = Math.sqrt(n * n + i * i)) && (n /= l, i /= l, s /= l), e * i < t * n && (e = -e,
        t = -t, s = -s, a = -a), {
        translateX: o,
        translateY: r,
        rotate: Math.atan2(t, e) * q,
        skewX: Math.atan(s) * q,
        scaleX: a,
        scaleY: l
      };
    }

    function C(e) {
      return "none" === e ? X : (G || (G = document.createElement("DIV"), V = document.documentElement, H =
          document.defaultView), G.style.transform = e, e = H.getComputedStyle(V.appendChild(G), null)
        .getPropertyValue("transform"), V.removeChild(G), e = e.slice(7, -1).split(","), T(+e[0], +e[1], +
          e[2], +e[3], +e[4], +e[5]));
    }

    function O(e) {
      return null == e ? X : (B || (B = document.createElementNS("http://www.w3.org/2000/svg", "g")), B
        .setAttribute("transform", e), (e = B.transform.baseVal.consolidate()) ? (e = e.matrix, T(e.a, e
          .b, e.c, e.d, e.e, e.f)) : X);
    }

    function A(e, t, n, i) {
      function o(e) {
        return e.length ? e.pop() + " " : "";
      }

      function r(e, i, o, r, a, l) {
        if (e !== o || i !== r) {
          var s = a.push("translate(", null, t, null, n);
          l.push({
            i: s - 4,
            x: b(e, o)
          }, {
            i: s - 2,
            x: b(i, r)
          });
        } else(o || r) && a.push("translate(" + o + t + r + n);
      }

      function a(e, t, n, r) {
        e !== t ? (e - t > 180 ? t += 360 : t - e > 180 && (e += 360), r.push({
          i: n.push(o(n) + "rotate(", null, i) - 2,
          x: b(e, t)
        })) : t && n.push(o(n) + "rotate(" + t + i);
      }

      function l(e, t, n, r) {
        e !== t ? r.push({
          i: n.push(o(n) + "skewX(", null, i) - 2,
          x: b(e, t)
        }) : t && n.push(o(n) + "skewX(" + t + i);
      }

      function s(e, t, n, i, r, a) {
        if (e !== n || t !== i) {
          var l = r.push(o(r) + "scale(", null, ",", null, ")");
          a.push({
            i: l - 4,
            x: b(e, n)
          }, {
            i: l - 2,
            x: b(t, i)
          });
        } else 1 === n && 1 === i || r.push(o(r) + "scale(" + n + "," + i + ")");
      }
      return function(t, n) {
        var i = [],
          o = [];
        return t = e(t), n = e(n), r(t.translateX, t.translateY, n.translateX, n.translateY, i, o), a(t
            .rotate, n.rotate, i, o), l(t.skewX, n.skewX, i, o), s(t.scaleX, t.scaleY, n.scaleX, n.scaleY,
            i, o), t = n = null,
          function(e) {
            for (var t, n = -1, r = o.length; ++n < r;) i[(t = o[n]).i] = t.x(e);
            return i.join("");
          };
      };
    }

    function I(e) {
      return ((e = Math.exp(e)) + 1 / e) / 2;
    }

    function M(e) {
      return ((e = Math.exp(e)) - 1 / e) / 2;
    }

    function R(e) {
      return ((e = Math.exp(2 * e)) - 1) / (e + 1);
    }

    function P(e, t) {
      var n,
        i,
        o = e[0],
        r = e[1],
        a = e[2],
        l = t[0],
        s = t[1],
        d = t[2],
        c = l - o,
        u = s - r,
        f = c * c + u * u;
      if (f < ne) i = Math.log(d / a) / J, n = function(e) {
        return [o + e * c, r + e * u, a * Math.exp(J * e * i)];
      };
      else {
        var m = Math.sqrt(f),
          g = (d * d - a * a + te * f) / (2 * a * ee * m),
          p = (d * d - a * a - te * f) / (2 * d * ee * m),
          h = Math.log(Math.sqrt(g * g + 1) - g),
          b = Math.log(Math.sqrt(p * p + 1) - p);
        i = (b - h) / J, n = function(e) {
          var t = e * i,
            n = I(h),
            l = a / (ee * m) * (n * R(J * t + h) - M(h));
          return [o + l * c, r + l * u, a * n / I(J * t + h)];
        };
      }
      return n.duration = 1e3 * i, n;
    }

    function D(e) {
      return function(n, i) {
        var o = e((n = t.hsl(n)).h, (i = t.hsl(i)).h),
          r = c(n.s, i.s),
          a = c(n.l, i.l),
          l = c(n.opacity, i.opacity);
        return function(e) {
          return n.h = o(e), n.s = r(e), n.l = a(e), n.opacity = l(e), n + "";
        };
      };
    }

    function N(e, n) {
      var i = c((e = t.lab(e)).l, (n = t.lab(n)).l),
        o = c(e.a, n.a),
        r = c(e.b, n.b),
        a = c(e.opacity, n.opacity);
      return function(t) {
        return e.l = i(t), e.a = o(t), e.b = r(t), e.opacity = a(t), e + "";
      };
    }

    function L(e) {
      return function(n, i) {
        var o = e((n = t.hcl(n)).h, (i = t.hcl(i)).h),
          r = c(n.c, i.c),
          a = c(n.l, i.l),
          l = c(n.opacity, i.opacity);
        return function(e) {
          return n.h = o(e), n.c = r(e), n.l = a(e), n.opacity = l(e), n + "";
        };
      };
    }

    function F(e) {
      return function n(i) {
        function o(n, o) {
          var r = e((n = t.cubehelix(n)).h, (o = t.cubehelix(o)).h),
            a = c(n.s, o.s),
            l = c(n.l, o.l),
            s = c(n.opacity, o.opacity);
          return function(e) {
            return n.h = r(e), n.s = a(e), n.l = l(Math.pow(e, i)), n.opacity = s(e), n + "";
          };
        }
        return i = +i, o.gamma = n, o;
      }(1);
    }

    function U(e, t) {
      for (var n = 0, i = t.length - 1, o = t[0], r = new Array(i < 0 ? 0 : i); n < i;) r[n] = e(o, o = t[++
        n]);
      return function(e) {
        var t = Math.max(0, Math.min(i - 1, Math.floor(e *= i)));
        return r[t](e - t);
      };
    }

    function z(e, t) {
      for (var n = new Array(t), i = 0; i < t; ++i) n[i] = e(i / (t - 1));
      return n;
    }
    var G,
      V,
      H,
      B,
      Y = function e(n) {
        function i(e, n) {
          var i = o((e = t.rgb(e)).r, (n = t.rgb(n)).r),
            r = o(e.g, n.g),
            a = o(e.b, n.b),
            l = c(e.opacity, n.opacity);
          return function(t) {
            return e.r = i(t), e.g = r(t), e.b = a(t), e.opacity = l(t), e + "";
          };
        }
        var o = d(n);
        return i.gamma = e, i;
      }(1),
      $ = u(i),
      W = u(o),
      j = /[-+]?(?:\d+\.?\d*|\.?\d+)(?:[eE][-+]?\d+)?/g,
      K = new RegExp(j.source, "g"),
      q = 180 / Math.PI,
      X = {
        translateX: 0,
        translateY: 0,
        rotate: 0,
        skewX: 0,
        scaleX: 1,
        scaleY: 1
      },
      Z = A(C, "px, ", "px)", "deg)"),
      Q = A(O, ", ", ")", ")"),
      J = Math.SQRT2,
      ee = 2,
      te = 4,
      ne = 1e-12,
      ie = D(s),
      oe = D(c),
      re = L(s),
      ae = L(c),
      le = F(s),
      se = F(c);
    e.interpolate = S, e.interpolateArray = g, e.interpolateBasis = i, e.interpolateBasisClosed = o, e
      .interpolateCubehelix = le, e.interpolateCubehelixLong = se, e.interpolateDate = h, e
      .interpolateDiscrete = E, e.interpolateHcl = re, e.interpolateHclLong = ae, e.interpolateHsl = ie, e
      .interpolateHslLong = oe, e.interpolateHue = k, e.interpolateLab = N, e.interpolateNumber = b, e
      .interpolateNumberArray = f, e.interpolateObject = x, e.interpolateRgb = Y, e.interpolateRgbBasis = $,
      e.interpolateRgbBasisClosed = W, e.interpolateRound = _, e.interpolateString = w, e
      .interpolateTransformCss = Z, e.interpolateTransformSvg = Q, e.interpolateZoom = P, e.piecewise = U, e
      .quantize = z, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
