// ─────────────────────────────────────────────────────────────
// APP MODULE 438
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, n) {
    n(t)
  }(this, function(e) {
    "use strict";

    function t() {
      return Math.random()
    }
    var n = function e(t) {
        function n(e, n) {
          return e = null == e ? 0 : +e, n = null == n ? 1 : +n, 1 === arguments.length ? (n = e, e = 0) : n -= e,
            function() {
              return t() * n + e
            }
        }
        return n.source = e, n
      }(t),
      i = function e(t) {
        function n(e, n) {
          var i, o;
          return e = null == e ? 0 : +e, n = null == n ? 1 : +n,
            function() {
              var r;
              if (null != i) r = i, i = null;
              else
                do i = 2 * t() - 1, r = 2 * t() - 1, o = i * i + r * r; while (!o || o > 1);
              return e + n * r * Math.sqrt(-2 * Math.log(o) / o)
            }
        }
        return n.source = e, n
      }(t),
      o = function e(t) {
        function n() {
          var e = i.source(t).apply(this, arguments);
          return function() {
            return Math.exp(e())
          }
        }
        return n.source = e, n
      }(t),
      r = function e(t) {
        function n(e) {
          return function() {
            for (var n = 0, i = 0; i < e; ++i) n += t();
            return n
          }
        }
        return n.source = e, n
      }(t),
      a = function e(t) {
        function n(e) {
          var n = r.source(t)(e);
          return function() {
            return n() / e
          }
        }
        return n.source = e, n
      }(t),
      l = function e(t) {
        function n(e) {
          return function() {
            return -Math.log(1 - t()) / e
          }
        }
        return n.source = e, n
      }(t);
    e.randomUniform = n, e.randomNormal = i, e.randomLogNormal = o, e.randomBates = a, e.randomIrwinHall = r, e
      .randomExponential = l, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
