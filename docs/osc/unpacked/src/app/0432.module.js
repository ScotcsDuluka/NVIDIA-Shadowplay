// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 432
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, i) {
    i(exports, require(44));
  }(this, function(e, t) {
    "use strict";

    function n(e, t) {
      return e - t;
    }

    function i(e) {
      for (var t = 0, n = e.length, i = e[n - 1][1] * e[0][0] - e[n - 1][0] * e[0][1]; ++t < n;) i += e[t -
        1][1] * e[t][0] - e[t - 1][0] * e[t][1];
      return i;
    }

    function o(e) {
      return function() {
        return e;
      };
    }

    function r(e, t) {
      for (var n, i = -1, o = t.length; ++i < o;)
        if (n = a(e, t[i])) return n;
      return 0;
    }

    function a(e, t) {
      for (var n = t[0], i = t[1], o = -1, r = 0, a = e.length, s = a - 1; r < a; s = r++) {
        var d = e[r],
          c = d[0],
          u = d[1],
          f = e[s],
          m = f[0],
          g = f[1];
        if (l(d, f, t)) return 0;
        u > i != g > i && n < (m - c) * (i - u) / (g - u) + c && (o = -o);
      }
      return o;
    }

    function l(e, t, n) {
      var i;
      return s(e, t, n) && d(e[i = +(e[0] === t[0])], n[i], t[i]);
    }

    function s(e, t, n) {
      return (t[0] - e[0]) * (n[1] - e[1]) === (n[0] - e[0]) * (t[1] - e[1]);
    }

    function d(e, t, n) {
      return e <= t && t <= n || n <= t && t <= e;
    }

    function c() {}

    function u() {
      function e(e) {
        var i = m(e);
        if (Array.isArray(i)) i = i.slice().sort(n);
        else {
          var o = t.extent(e),
            r = o[0],
            l = o[1];
          i = t.tickStep(r, l, i), i = t.range(Math.floor(r / i) * i, Math.floor(l / i) * i, i);
        }
        return i.map(function(t) {
          return a(e, t);
        });
      }

      function a(e, t) {
        var n = [],
          o = [];
        return l(e, t, function(r) {
          g(r, e, t), i(r) > 0 ? n.push([r]) : o.push(r);
        }), o.forEach(function(e) {
          for (var t, i = 0, o = n.length; i < o; ++i)
            if (r((t = n[i])[0], e) !== -1) return void t.push(e);
        }), {
          type: "MultiPolygon",
          value: t,
          coordinates: n
        };
      }

      function l(e, t, n) {
        function i(e) {
          var t,
            i,
            a = [e[0][0] + o, e[0][1] + r],
            l = [e[1][0] + o, e[1][1] + r],
            d = s(a),
            c = s(l);
          (t = g[d]) ? (i = m[c]) ? (delete g[t.end], delete m[i.start], t === i ? (t.ring.push(l), n(t
            .ring)) : m[t.start] = g[i.end] = {
            start: t.start,
            end: i.end,
            ring: t.ring.concat(i.ring)
          }) : (delete g[t.end], t.ring.push(l), g[t.end = c] = t) : (t = m[c]) ? (i = g[d]) ? (delete m[t
            .start], delete g[i.end], t === i ? (t.ring.push(l), n(t.ring)) : m[i.start] = g[t.end] = {
            start: i.start,
            end: t.end,
            ring: i.ring.concat(t.ring)
          }) : (delete m[t.start], t.ring.unshift(a), m[t.start = d] = t) : m[d] = g[c] = {
            start: d,
            end: c,
            ring: [a, l]
          };
        }
        var o,
          r,
          a,
          l,
          d,
          c,
          m = new Array(),
          g = new Array();
        for (o = r = -1, l = e[0] >= t, y[l << 1].forEach(i); ++o < u - 1;) a = l, l = e[o + 1] >= t, y[a |
          l << 1].forEach(i);
        for (y[l << 0].forEach(i); ++r < f - 1;) {
          for (o = -1, l = e[r * u + u] >= t, d = e[r * u] >= t, y[l << 1 | d << 2].forEach(i); ++o < u -
            1;) a = l, l = e[r * u + u + o + 1] >= t, c = d, d = e[r * u + o + 1] >= t, y[a | l << 1 | d <<
            2 | c << 3].forEach(i);
          y[l | d << 3].forEach(i);
        }
        for (o = -1, d = e[r * u] >= t, y[d << 2].forEach(i); ++o < u - 1;) c = d, d = e[r * u + o + 1] >=
          t, y[d << 2 | c << 3].forEach(i);
        y[d << 3].forEach(i);
      }

      function s(e) {
        return 2 * e[0] + e[1] * (u + 1) * 4;
      }

      function d(e, t, n) {
        e.forEach(function(e) {
          var i,
            o = e[0],
            r = e[1],
            a = 0 | o,
            l = 0 | r,
            s = t[l * u + a];
          o > 0 && o < u && a === o && (i = t[l * u + a - 1], e[0] = o + (n - i) / (s - i) - .5), r >
            0 && r < f && l === r && (i = t[(l - 1) * u + a], e[1] = r + (n - i) / (s - i) - .5);
        });
      }
      var u = 1,
        f = 1,
        m = t.thresholdSturges,
        g = d;
      return e.contour = a, e.size = function(t) {
        if (!arguments.length) return [u, f];
        var n = Math.ceil(t[0]),
          i = Math.ceil(t[1]);
        if (!(n > 0 && i > 0)) throw new Error("invalid size");
        return u = n, f = i, e;
      }, e.thresholds = function(t) {
        return arguments.length ? (m = "function" == typeof t ? t : o(Array.isArray(t) ? v.call(t) : t),
          e) : m;
      }, e.smooth = function(t) {
        return arguments.length ? (g = t ? d : c, e) : g === d;
      }, e;
    }

    function f(e, t, n) {
      for (var i = e.width, o = e.height, r = (n << 1) + 1, a = 0; a < o; ++a)
        for (var l = 0, s = 0; l < i + n; ++l) l < i && (s += e.data[l + a * i]), l >= n && (l >= r && (s -=
          e.data[l - r + a * i]), t.data[l - n + a * i] = s / Math.min(l + 1, i - 1 + r - l, r));
    }

    function m(e, t, n) {
      for (var i = e.width, o = e.height, r = (n << 1) + 1, a = 0; a < i; ++a)
        for (var l = 0, s = 0; l < o + n; ++l) l < o && (s += e.data[a + l * i]), l >= n && (l >= r && (s -=
          e.data[a + (l - r) * i]), t.data[a + (l - n) * i] = s / Math.min(l + 1, o - 1 + r - l, r));
    }

    function g(e) {
      return e[0];
    }

    function p(e) {
      return e[1];
    }

    function h() {
      return 1;
    }

    function b() {
      function e(e) {
        var i = new Float32Array(E * k),
          o = new Float32Array(E * k);
        e.forEach(function(e, t, n) {
          var o = +s(e, t, n) + S >> w,
            r = +d(e, t, n) + S >> w,
            a = +c(e, t, n);
          o >= 0 && o < E && r >= 0 && r < k && (i[o + r * E] += a);
        }), f({
          width: E,
          height: k,
          data: i
        }, {
          width: E,
          height: k,
          data: o
        }, y >> w), m({
          width: E,
          height: k,
          data: o
        }, {
          width: E,
          height: k,
          data: i
        }, y >> w), f({
          width: E,
          height: k,
          data: i
        }, {
          width: E,
          height: k,
          data: o
        }, y >> w), m({
          width: E,
          height: k,
          data: o
        }, {
          width: E,
          height: k,
          data: i
        }, y >> w), f({
          width: E,
          height: k,
          data: i
        }, {
          width: E,
          height: k,
          data: o
        }, y >> w), m({
          width: E,
          height: k,
          data: o
        }, {
          width: E,
          height: k,
          data: i
        }, y >> w);
        var r = _(i);
        if (!Array.isArray(r)) {
          var a = t.max(i);
          r = t.tickStep(0, a, r), r = t.range(0, Math.floor(a / r) * r, r), r.shift();
        }
        return u().thresholds(r).size([E, k])(i).map(n);
      }

      function n(e) {
        return e.value *= Math.pow(2, -2 * w), e.coordinates.forEach(i), e;
      }

      function i(e) {
        e.forEach(r);
      }

      function r(e) {
        e.forEach(a);
      }

      function a(e) {
        e[0] = e[0] * Math.pow(2, w) - S, e[1] = e[1] * Math.pow(2, w) - S;
      }

      function l() {
        return S = 3 * y, E = b + 2 * S >> w, k = x + 2 * S >> w, e;
      }
      var s = g,
        d = p,
        c = h,
        b = 960,
        x = 500,
        y = 20,
        w = 2,
        S = 3 * y,
        E = b + 2 * S >> w,
        k = x + 2 * S >> w,
        _ = o(20);
      return e.x = function(t) {
        return arguments.length ? (s = "function" == typeof t ? t : o(+t), e) : s;
      }, e.y = function(t) {
        return arguments.length ? (d = "function" == typeof t ? t : o(+t), e) : d;
      }, e.weight = function(t) {
        return arguments.length ? (c = "function" == typeof t ? t : o(+t), e) : c;
      }, e.size = function(e) {
        if (!arguments.length) return [b, x];
        var t = Math.ceil(e[0]),
          n = Math.ceil(e[1]);
        if (!(t >= 0 || t >= 0)) throw new Error("invalid size");
        return b = t, x = n, l();
      }, e.cellSize = function(e) {
        if (!arguments.length) return 1 << w;
        if (!((e = +e) >= 1)) throw new Error("invalid cell size");
        return w = Math.floor(Math.log(e) / Math.LN2), l();
      }, e.thresholds = function(t) {
        return arguments.length ? (_ = "function" == typeof t ? t : o(Array.isArray(t) ? v.call(t) : t),
          e) : _;
      }, e.bandwidth = function(e) {
        if (!arguments.length) return Math.sqrt(y * (y + 1));
        if (!((e = +e) >= 0)) throw new Error("invalid bandwidth");
        return y = Math.round((Math.sqrt(4 * e * e + 1) - 1) / 2), l();
      }, e;
    }
    var x = Array.prototype,
      v = x.slice,
      y = [
        [],
        [
          [
            [1, 1.5],
            [.5, 1]
          ]
        ],
        [
          [
            [1.5, 1],
            [1, 1.5]
          ]
        ],
        [
          [
            [1.5, 1],
            [.5, 1]
          ]
        ],
        [
          [
            [1, .5],
            [1.5, 1]
          ]
        ],
        [
          [
            [1, 1.5],
            [.5, 1]
          ],
          [
            [1, .5],
            [1.5, 1]
          ]
        ],
        [
          [
            [1, .5],
            [1, 1.5]
          ]
        ],
        [
          [
            [1, .5],
            [.5, 1]
          ]
        ],
        [
          [
            [.5, 1],
            [1, .5]
          ]
        ],
        [
          [
            [1, 1.5],
            [1, .5]
          ]
        ],
        [
          [
            [.5, 1],
            [1, .5]
          ],
          [
            [1.5, 1],
            [1, 1.5]
          ]
        ],
        [
          [
            [1.5, 1],
            [1, .5]
          ]
        ],
        [
          [
            [.5, 1],
            [1.5, 1]
          ]
        ],
        [
          [
            [1, 1.5],
            [1.5, 1]
          ]
        ],
        [
          [
            [.5, 1],
            [1, 1.5]
          ]
        ],
        []
      ];
    e.contours = u, e.contourDensity = b, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
