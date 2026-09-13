// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 249
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t() {
      return Math.random();
    }
    var n = function e(t) {
        function n(e, n) {
          return e = null == e ? 0 : +e, n = null == n ? 1 : +n, 1 === arguments.length ? (n = e, e = 0) :
            n -= e,
            function() {
              return t() * n + e;
            };
        }
        return n.source = e, n;
      }(t),
      r = function e(t) {
        function n(e, n) {
          var r, i;
          return e = null == e ? 0 : +e, n = null == n ? 1 : +n,
            function() {
              var o;
              if (null != r) o = r, r = null;
              else
                do r = 2 * t() - 1, o = 2 * t() - 1, i = r * r + o * o; while (!i || i > 1);
              return e + n * o * Math.sqrt(-2 * Math.log(i) / i);
            };
        }
        return n.source = e, n;
      }(t),
      i = function e(t) {
        function n() {
          var e = r.source(t).apply(this, arguments);
          return function() {
            return Math.exp(e());
          };
        }
        return n.source = e, n;
      }(t),
      o = function e(t) {
        function n(e) {
          return function() {
            for (var n = 0, r = 0; r < e; ++r) n += t();
            return n;
          };
        }
        return n.source = e, n;
      }(t),
      a = function e(t) {
        function n(e) {
          var n = o.source(t)(e);
          return function() {
            return n() / e;
          };
        }
        return n.source = e, n;
      }(t),
      s = function e(t) {
        function n(e) {
          return function() {
            return -Math.log(1 - t()) / e;
          };
        }
        return n.source = e, n;
      }(t);
    e.randomUniform = n, e.randomNormal = r, e.randomLogNormal = i, e.randomBates = a, e.randomIrwinHall =
      o, e.randomExponential = s, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
