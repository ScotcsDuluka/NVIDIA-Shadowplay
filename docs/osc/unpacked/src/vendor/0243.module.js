// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 243
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, r) {
    r(exports, require(30));
  }(this, function(e, t) {
    "use strict";

    function n(e, t) {
      return e - t;
    }

    function r(e) {
      for (var t = 0, n = e.length, r = e[n - 1][1] * e[0][0] - e[n - 1][0] * e[0][1]; ++t < n;) r += e[t -
        1][1] * e[t][0] - e[t - 1][0] * e[t][1];
      return r;
    }

    function i(e) {
      return function() {
        return e;
      };
    }

    function o(e, t) {
      for (var n, r = -1, i = t.length; ++r < i;)
        if (n = a(e, t[r])) return n;
      return 0;
    }

    function a(e, t) {
      for (var n = t[0], r = t[1], i = -1, o = 0, a = e.length, c = a - 1; o < a; c = o++) {
        var u = e[o],
          l = u[0],
          d = u[1],
          f = e[c],
          h = f[0],
          p = f[1];
        if (s(u, f, t)) return 0;
        d > r != p > r && n < (h - l) * (r - d) / (p - d) + l && (i = -i);
      }
      return i;
    }

    function s(e, t, n) {
      var r;
      return c(e, t, n) && u(e[r = +(e[0] === t[0])], n[r], t[r]);
    }

    function c(e, t, n) {
      return (t[0] - e[0]) * (n[1] - e[1]) === (n[0] - e[0]) * (t[1] - e[1]);
    }

    function u(e, t, n) {
      return e <= t && t <= n || n <= t && t <= e;
    }

    function l() {}

    function d() {
      function e(e) {
        var r = h(e);
        if (Array.isArray(r)) r = r.slice().sort(n);
        else {
          var i = t.extent(e),
            o = i[0],
            s = i[1];
          r = t.tickStep(o, s, r), r = t.range(Math.floor(o / r) * r, Math.floor(s / r) * r, r);
        }
        return r.map(function(t) {
          return a(e, t);
        });
      }

      function a(e, t) {
        var n = [],
          i = [];
        return s(e, t, function(o) {
          p(o, e, t), r(o) > 0 ? n.push([o]) : i.push(o);
        }), i.forEach(function(e) {
          for (var t, r = 0, i = n.length; r < i; ++r)
            if (o((t = n[r])[0], e) !== -1) return void t.push(e);
        }), {
          type: "MultiPolygon",
          value: t,
          coordinates: n
        };
      }

      function s(e, t, n) {
        function r(e) {
          var t,
            r,
            a = [e[0][0] + i, e[0][1] + o],
            s = [e[1][0] + i, e[1][1] + o],
            u = c(a),
            l = c(s);
          (t = p[u]) ? (r = h[l]) ? (delete p[t.end], delete h[r.start], t === r ? (t.ring.push(s), n(t
            .ring)) : h[t.start] = p[r.end] = {
            start: t.start,
            end: r.end,
            ring: t.ring.concat(r.ring)
          }) : (delete p[t.end], t.ring.push(s), p[t.end = l] = t) : (t = h[l]) ? (r = p[u]) ? (delete h[t
            .start], delete p[r.end], t === r ? (t.ring.push(s), n(t.ring)) : h[r.start] = p[t.end] = {
            start: r.start,
            end: t.end,
            ring: r.ring.concat(t.ring)
          }) : (delete h[t.start], t.ring.unshift(a), h[t.start = u] = t) : h[u] = p[l] = {
            start: u,
            end: l,
            ring: [a, s]
          };
        }
        var i,
          o,
          a,
          s,
          u,
          l,
          h = new Array(),
          p = new Array();
        for (i = o = -1, s = e[0] >= t, E[s << 1].forEach(r); ++i < d - 1;) a = s, s = e[i + 1] >= t, E[a |
          s << 1].forEach(r);
        for (E[s << 0].forEach(r); ++o < f - 1;) {
          for (i = -1, s = e[o * d + d] >= t, u = e[o * d] >= t, E[s << 1 | u << 2].forEach(r); ++i < d -
            1;) a = s, s = e[o * d + d + i + 1] >= t, l = u, u = e[o * d + i + 1] >= t, E[a | s << 1 | u <<
            2 | l << 3].forEach(r);
          E[s | u << 3].forEach(r);
        }
        for (i = -1, u = e[o * d] >= t, E[u << 2].forEach(r); ++i < d - 1;) l = u, u = e[o * d + i + 1] >=
          t, E[u << 2 | l << 3].forEach(r);
        E[u << 3].forEach(r);
      }

      function c(e) {
        return 2 * e[0] + e[1] * (d + 1) * 4;
      }

      function u(e, t, n) {
        e.forEach(function(e) {
          var r,
            i = e[0],
            o = e[1],
            a = 0 | i,
            s = 0 | o,
            c = t[s * d + a];
          i > 0 && i < d && a === i && (r = t[s * d + a - 1], e[0] = i + (n - r) / (c - r) - .5), o >
            0 && o < f && s === o && (r = t[(s - 1) * d + a], e[1] = o + (n - r) / (c - r) - .5);
        });
      }
      var d = 1,
        f = 1,
        h = t.thresholdSturges,
        p = u;
      return e.contour = a, e.size = function(t) {
        if (!arguments.length) return [d, f];
        var n = Math.ceil(t[0]),
          r = Math.ceil(t[1]);
        if (!(n > 0 && r > 0)) throw new Error("invalid size");
        return d = n, f = r, e;
      }, e.thresholds = function(t) {
        return arguments.length ? (h = "function" == typeof t ? t : i(Array.isArray(t) ? b.call(t) : t),
          e) : h;
      }, e.smooth = function(t) {
        return arguments.length ? (p = t ? u : l, e) : p === u;
      }, e;
    }

    function f(e, t, n) {
      for (var r = e.width, i = e.height, o = (n << 1) + 1, a = 0; a < i; ++a)
        for (var s = 0, c = 0; s < r + n; ++s) s < r && (c += e.data[s + a * r]), s >= n && (s >= o && (c -=
          e.data[s - o + a * r]), t.data[s - n + a * r] = c / Math.min(s + 1, r - 1 + o - s, o));
    }

    function h(e, t, n) {
      for (var r = e.width, i = e.height, o = (n << 1) + 1, a = 0; a < r; ++a)
        for (var s = 0, c = 0; s < i + n; ++s) s < i && (c += e.data[a + s * r]), s >= n && (s >= o && (c -=
          e.data[a + (s - o) * r]), t.data[a + (s - n) * r] = c / Math.min(s + 1, i - 1 + o - s, o));
    }

    function p(e) {
      return e[0];
    }

    function m(e) {
      return e[1];
    }

    function v() {
      return 1;
    }

    function g() {
      function e(e) {
        var r = new Float32Array(w * T),
          i = new Float32Array(w * T);
        e.forEach(function(e, t, n) {
          var i = +c(e, t, n) + $ >> _,
            o = +u(e, t, n) + $ >> _,
            a = +l(e, t, n);
          i >= 0 && i < w && o >= 0 && o < T && (r[i + o * w] += a);
        }), f({
          width: w,
          height: T,
          data: r
        }, {
          width: w,
          height: T,
          data: i
        }, E >> _), h({
          width: w,
          height: T,
          data: i
        }, {
          width: w,
          height: T,
          data: r
        }, E >> _), f({
          width: w,
          height: T,
          data: r
        }, {
          width: w,
          height: T,
          data: i
        }, E >> _), h({
          width: w,
          height: T,
          data: i
        }, {
          width: w,
          height: T,
          data: r
        }, E >> _), f({
          width: w,
          height: T,
          data: r
        }, {
          width: w,
          height: T,
          data: i
        }, E >> _), h({
          width: w,
          height: T,
          data: i
        }, {
          width: w,
          height: T,
          data: r
        }, E >> _);
        var o = C(r);
        if (!Array.isArray(o)) {
          var a = t.max(r);
          o = t.tickStep(0, a, o), o = t.range(0, Math.floor(a / o) * o, o), o.shift();
        }
        return d().thresholds(o).size([w, T])(r).map(n);
      }

      function n(e) {
        return e.value *= Math.pow(2, -2 * _), e.coordinates.forEach(r), e;
      }

      function r(e) {
        e.forEach(o);
      }

      function o(e) {
        e.forEach(a);
      }

      function a(e) {
        e[0] = e[0] * Math.pow(2, _) - $, e[1] = e[1] * Math.pow(2, _) - $;
      }

      function s() {
        return $ = 3 * E, w = g + 2 * $ >> _, T = y + 2 * $ >> _, e;
      }
      var c = p,
        u = m,
        l = v,
        g = 960,
        y = 500,
        E = 20,
        _ = 2,
        $ = 3 * E,
        w = g + 2 * $ >> _,
        T = y + 2 * $ >> _,
        C = i(20);
      return e.x = function(t) {
        return arguments.length ? (c = "function" == typeof t ? t : i(+t), e) : c;
      }, e.y = function(t) {
        return arguments.length ? (u = "function" == typeof t ? t : i(+t), e) : u;
      }, e.weight = function(t) {
        return arguments.length ? (l = "function" == typeof t ? t : i(+t), e) : l;
      }, e.size = function(e) {
        if (!arguments.length) return [g, y];
        var t = Math.ceil(e[0]),
          n = Math.ceil(e[1]);
        if (!(t >= 0 || t >= 0)) throw new Error("invalid size");
        return g = t, y = n, s();
      }, e.cellSize = function(e) {
        if (!arguments.length) return 1 << _;
        if (!((e = +e) >= 1)) throw new Error("invalid cell size");
        return _ = Math.floor(Math.log(e) / Math.LN2), s();
      }, e.thresholds = function(t) {
        return arguments.length ? (C = "function" == typeof t ? t : i(Array.isArray(t) ? b.call(t) : t),
          e) : C;
      }, e.bandwidth = function(e) {
        if (!arguments.length) return Math.sqrt(E * (E + 1));
        if (!((e = +e) >= 0)) throw new Error("invalid bandwidth");
        return E = Math.round((Math.sqrt(4 * e * e + 1) - 1) / 2), s();
      }, e;
    }
    var y = Array.prototype,
      b = y.slice,
      E = [
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
    e.contours = d, e.contourDensity = g, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
