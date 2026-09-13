// ─────────────────────────────────────────────────────────────
// APP MODULE 128
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t(e) {
      return +e
    }

    function n(e) {
      return e * e
    }

    function i(e) {
      return e * (2 - e)
    }

    function o(e) {
      return ((e *= 2) <= 1 ? e * e : --e * (2 - e) + 1) / 2
    }

    function r(e) {
      return e * e * e
    }

    function a(e) {
      return --e * e * e + 1
    }

    function l(e) {
      return ((e *= 2) <= 1 ? e * e * e : (e -= 2) * e * e + 2) / 2
    }

    function s(e) {
      return 1 === +e ? 1 : 1 - Math.cos(e * T)
    }

    function d(e) {
      return Math.sin(e * T)
    }

    function c(e) {
      return (1 - Math.cos(_ * e)) / 2
    }

    function u(e) {
      return 1.0009775171065494 * (Math.pow(2, -10 * e) - .0009765625)
    }

    function f(e) {
      return u(1 - +e)
    }

    function m(e) {
      return 1 - u(e)
    }

    function g(e) {
      return ((e *= 2) <= 1 ? u(1 - e) : 2 - u(e - 1)) / 2
    }

    function p(e) {
      return 1 - Math.sqrt(1 - e * e)
    }

    function h(e) {
      return Math.sqrt(1 - --e * e)
    }

    function b(e) {
      return ((e *= 2) <= 1 ? 1 - Math.sqrt(1 - e * e) : Math.sqrt(1 - (e -= 2) * e) + 1) / 2
    }

    function x(e) {
      return 1 - v(1 - e)
    }

    function v(e) {
      return (e = +e) < C ? L * e * e : e < A ? L * (e -= O) * e + I : e < R ? L * (e -= M) * e + P : L * (e -= D) *
        e + N
    }

    function y(e) {
      return ((e *= 2) <= 1 ? 1 - v(1 - e) : v(e - 1) + 1) / 2
    }
    var w = 3,
      S = function e(t) {
        function n(e) {
          return Math.pow(e, t)
        }
        return t = +t, n.exponent = e, n
      }(w),
      E = function e(t) {
        function n(e) {
          return 1 - Math.pow(1 - e, t)
        }
        return t = +t, n.exponent = e, n
      }(w),
      k = function e(t) {
        function n(e) {
          return ((e *= 2) <= 1 ? Math.pow(e, t) : 2 - Math.pow(2 - e, t)) / 2
        }
        return t = +t, n.exponent = e, n
      }(w),
      _ = Math.PI,
      T = _ / 2,
      C = 4 / 11,
      O = 6 / 11,
      A = 8 / 11,
      I = .75,
      M = 9 / 11,
      R = 10 / 11,
      P = .9375,
      D = 21 / 22,
      N = 63 / 64,
      L = 1 / C / C,
      F = 1.70158,
      U = function e(t) {
        function n(e) {
          return (e = +e) * e * (t * (e - 1) + e)
        }
        return t = +t, n.overshoot = e, n
      }(F),
      z = function e(t) {
        function n(e) {
          return --e * e * ((e + 1) * t + e) + 1
        }
        return t = +t, n.overshoot = e, n
      }(F),
      G = function e(t) {
        function n(e) {
          return ((e *= 2) < 1 ? e * e * ((t + 1) * e - t) : (e -= 2) * e * ((t + 1) * e + t) + 2) / 2
        }
        return t = +t, n.overshoot = e, n
      }(F),
      V = 2 * Math.PI,
      H = 1,
      B = .3,
      Y = function e(t, n) {
        function i(e) {
          return t * u(- --e) * Math.sin((o - e) / n)
        }
        var o = Math.asin(1 / (t = Math.max(1, t))) * (n /= V);
        return i.amplitude = function(t) {
          return e(t, n * V)
        }, i.period = function(n) {
          return e(t, n)
        }, i
      }(H, B),
      $ = function e(t, n) {
        function i(e) {
          return 1 - t * u(e = +e) * Math.sin((e + o) / n)
        }
        var o = Math.asin(1 / (t = Math.max(1, t))) * (n /= V);
        return i.amplitude = function(t) {
          return e(t, n * V)
        }, i.period = function(n) {
          return e(t, n)
        }, i
      }(H, B),
      W = function e(t, n) {
        function i(e) {
          return ((e = 2 * e - 1) < 0 ? t * u(-e) * Math.sin((o - e) / n) : 2 - t * u(e) * Math.sin((o + e) / n)) / 2
        }
        var o = Math.asin(1 / (t = Math.max(1, t))) * (n /= V);
        return i.amplitude = function(t) {
          return e(t, n * V)
        }, i.period = function(n) {
          return e(t, n)
        }, i
      }(H, B);
    e.easeBack = G, e.easeBackIn = U, e.easeBackInOut = G, e.easeBackOut = z, e.easeBounce = v, e.easeBounceIn = x, e
      .easeBounceInOut = y, e.easeBounceOut = v, e.easeCircle = b, e.easeCircleIn = p, e.easeCircleInOut = b, e
      .easeCircleOut = h, e.easeCubic = l, e.easeCubicIn = r, e.easeCubicInOut = l, e.easeCubicOut = a, e
      .easeElastic = $, e.easeElasticIn = Y, e.easeElasticInOut = W, e.easeElasticOut = $, e.easeExp = g, e
      .easeExpIn = f, e.easeExpInOut = g, e.easeExpOut = m, e.easeLinear = t, e.easePoly = k, e.easePolyIn = S, e
      .easePolyInOut = k, e.easePolyOut = E, e.easeQuad = o, e.easeQuadIn = n, e.easeQuadInOut = o, e.easeQuadOut = i,
      e.easeSin = c, e.easeSinIn = s, e.easeSinInOut = c, e.easeSinOut = d, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
