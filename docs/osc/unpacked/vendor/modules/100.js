// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 100
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
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

    function r(e) {
      return e * (2 - e)
    }

    function i(e) {
      return ((e *= 2) <= 1 ? e * e : --e * (2 - e) + 1) / 2
    }

    function o(e) {
      return e * e * e
    }

    function a(e) {
      return --e * e * e + 1
    }

    function s(e) {
      return ((e *= 2) <= 1 ? e * e * e : (e -= 2) * e * e + 2) / 2
    }

    function c(e) {
      return 1 === +e ? 1 : 1 - Math.cos(e * x)
    }

    function u(e) {
      return Math.sin(e * x)
    }

    function l(e) {
      return (1 - Math.cos(C * e)) / 2
    }

    function d(e) {
      return 1.0009775171065494 * (Math.pow(2, -10 * e) - .0009765625)
    }

    function f(e) {
      return d(1 - +e)
    }

    function h(e) {
      return 1 - d(e)
    }

    function p(e) {
      return ((e *= 2) <= 1 ? d(1 - e) : 2 - d(e - 1)) / 2
    }

    function m(e) {
      return 1 - Math.sqrt(1 - e * e)
    }

    function v(e) {
      return Math.sqrt(1 - --e * e)
    }

    function g(e) {
      return ((e *= 2) <= 1 ? 1 - Math.sqrt(1 - e * e) : Math.sqrt(1 - (e -= 2) * e) + 1) / 2
    }

    function y(e) {
      return 1 - b(1 - e)
    }

    function b(e) {
      return (e = +e) < S ? P * e * e : e < M ? P * (e -= A) * e + k : e < I ? P * (e -= N) * e + O : P * (e -= D) *
        e + R
    }

    function E(e) {
      return ((e *= 2) <= 1 ? 1 - b(1 - e) : b(e - 1) + 1) / 2
    }
    var _ = 3,
      $ = function e(t) {
        function n(e) {
          return Math.pow(e, t)
        }
        return t = +t, n.exponent = e, n
      }(_),
      w = function e(t) {
        function n(e) {
          return 1 - Math.pow(1 - e, t)
        }
        return t = +t, n.exponent = e, n
      }(_),
      T = function e(t) {
        function n(e) {
          return ((e *= 2) <= 1 ? Math.pow(e, t) : 2 - Math.pow(2 - e, t)) / 2
        }
        return t = +t, n.exponent = e, n
      }(_),
      C = Math.PI,
      x = C / 2,
      S = 4 / 11,
      A = 6 / 11,
      M = 8 / 11,
      k = .75,
      N = 9 / 11,
      I = 10 / 11,
      O = .9375,
      D = 21 / 22,
      R = 63 / 64,
      P = 1 / S / S,
      L = 1.70158,
      U = function e(t) {
        function n(e) {
          return (e = +e) * e * (t * (e - 1) + e)
        }
        return t = +t, n.overshoot = e, n
      }(L),
      F = function e(t) {
        function n(e) {
          return --e * e * ((e + 1) * t + e) + 1
        }
        return t = +t, n.overshoot = e, n
      }(L),
      j = function e(t) {
        function n(e) {
          return ((e *= 2) < 1 ? e * e * ((t + 1) * e - t) : (e -= 2) * e * ((t + 1) * e + t) + 2) / 2
        }
        return t = +t, n.overshoot = e, n
      }(L),
      H = 2 * Math.PI,
      B = 1,
      z = .3,
      q = function e(t, n) {
        function r(e) {
          return t * d(- --e) * Math.sin((i - e) / n)
        }
        var i = Math.asin(1 / (t = Math.max(1, t))) * (n /= H);
        return r.amplitude = function(t) {
          return e(t, n * H)
        }, r.period = function(n) {
          return e(t, n)
        }, r
      }(B, z),
      G = function e(t, n) {
        function r(e) {
          return 1 - t * d(e = +e) * Math.sin((e + i) / n)
        }
        var i = Math.asin(1 / (t = Math.max(1, t))) * (n /= H);
        return r.amplitude = function(t) {
          return e(t, n * H)
        }, r.period = function(n) {
          return e(t, n)
        }, r
      }(B, z),
      V = function e(t, n) {
        function r(e) {
          return ((e = 2 * e - 1) < 0 ? t * d(-e) * Math.sin((i - e) / n) : 2 - t * d(e) * Math.sin((i + e) / n)) / 2
        }
        var i = Math.asin(1 / (t = Math.max(1, t))) * (n /= H);
        return r.amplitude = function(t) {
          return e(t, n * H)
        }, r.period = function(n) {
          return e(t, n)
        }, r
      }(B, z);
    e.easeBack = j, e.easeBackIn = U, e.easeBackInOut = j, e.easeBackOut = F, e.easeBounce = b, e.easeBounceIn = y, e
      .easeBounceInOut = E, e.easeBounceOut = b, e.easeCircle = g, e.easeCircleIn = m, e.easeCircleInOut = g, e
      .easeCircleOut = v, e.easeCubic = s, e.easeCubicIn = o, e.easeCubicInOut = s, e.easeCubicOut = a, e
      .easeElastic = G, e.easeElasticIn = q, e.easeElasticInOut = V, e.easeElasticOut = G, e.easeExp = p, e
      .easeExpIn = f, e.easeExpInOut = p, e.easeExpOut = h, e.easeLinear = t, e.easePoly = T, e.easePolyIn = $, e
      .easePolyInOut = T, e.easePolyOut = w, e.easeQuad = i, e.easeQuadIn = n, e.easeQuadInOut = i, e.easeQuadOut = r,
      e.easeSin = l, e.easeSinIn = c, e.easeSinInOut = l, e.easeSinOut = u, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
