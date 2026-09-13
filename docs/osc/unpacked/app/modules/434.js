// ─────────────────────────────────────────────────────────────
// APP MODULE 434
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, i) {
    i(t, n(130), n(81), n(38), n(85))
  }(this, function(e, t, n, i, o) {
    "use strict";

    function r(e, t) {
      function n() {
        var n, o, r = i.length,
          a = 0,
          l = 0;
        for (n = 0; n < r; ++n) o = i[n], a += o.x, l += o.y;
        for (a = a / r - e, l = l / r - t, n = 0; n < r; ++n) o = i[n], o.x -= a, o.y -= l
      }
      var i;
      return null == e && (e = 0), null == t && (t = 0), n.initialize = function(e) {
        i = e
      }, n.x = function(t) {
        return arguments.length ? (e = +t, n) : e
      }, n.y = function(e) {
        return arguments.length ? (t = +e, n) : t
      }, n
    }

    function a(e) {
      return function() {
        return e
      }
    }

    function l() {
      return 1e-6 * (Math.random() - .5)
    }

    function s(e) {
      return e.x + e.vx
    }

    function d(e) {
      return e.y + e.vy
    }

    function c(e) {
      function n() {
        function e(e, t, n, i, o) {
          var r = e.data,
            s = e.r,
            d = p + s;
          {
            if (!r) return t > m + d || i < m - d || n > g + d || o < g - d;
            if (r.index > a.index) {
              var c = m - r.x - r.vx,
                f = g - r.y - r.vy,
                b = c * c + f * f;
              b < d * d && (0 === c && (c = l(), b += c * c), 0 === f && (f = l(), b += f * f), b = (d - (b = Math
                  .sqrt(b))) / b * u, a.vx += (c *= b) * (d = (s *= s) / (h + s)), a.vy += (f *= b) * d, r.vx -= c *
                (d = 1 - d), r.vy -= f * d)
            }
          }
        }
        for (var n, o, a, m, g, p, h, b = r.length, x = 0; x < f; ++x)
          for (o = t.quadtree(r, s, d).visitAfter(i), n = 0; n < b; ++n) a = r[n], p = c[a.index], h = p * p, m = a
            .x + a.vx, g = a.y + a.vy, o.visit(e)
      }

      function i(e) {
        if (e.data) return e.r = c[e.data.index];
        for (var t = e.r = 0; t < 4; ++t) e[t] && e[t].r > e.r && (e.r = e[t].r)
      }

      function o() {
        if (r) {
          var t, n, i = r.length;
          for (c = new Array(i), t = 0; t < i; ++t) n = r[t], c[n.index] = +e(n, t, r)
        }
      }
      var r, c, u = 1,
        f = 1;
      return "function" != typeof e && (e = a(null == e ? 1 : +e)), n.initialize = function(e) {
        r = e, o()
      }, n.iterations = function(e) {
        return arguments.length ? (f = +e, n) : f
      }, n.strength = function(e) {
        return arguments.length ? (u = +e, n) : u
      }, n.radius = function(t) {
        return arguments.length ? (e = "function" == typeof t ? t : a(+t), o(), n) : e
      }, n
    }

    function u(e) {
      return e.index
    }

    function f(e, t) {
      var n = e.get(t);
      if (!n) throw new Error("missing: " + t);
      return n
    }

    function m(e) {
      function t(e) {
        return 1 / Math.min(g[e.source.index], g[e.target.index])
      }

      function i(t) {
        for (var n = 0, i = e.length; n < v; ++n)
          for (var o, r, a, s, u, f, m, g = 0; g < i; ++g) o = e[g], r = o.source, a = o.target, s = a.x + a.vx - r
            .x - r.vx || l(), u = a.y + a.vy - r.y - r.vy || l(), f = Math.sqrt(s * s + u * u), f = (f - c[g]) / f *
            t * d[g], s *= f, u *= f, a.vx -= s * (m = p[g]), a.vy -= u * m, r.vx += s * (m = 1 - m), r.vy += u * m
      }

      function o() {
        if (m) {
          var t, i, o = m.length,
            a = e.length,
            l = n.map(m, h);
          for (t = 0, g = new Array(o); t < a; ++t) i = e[t], i.index = t, "object" != typeof i.source && (i.source =
            f(l, i.source)), "object" != typeof i.target && (i.target = f(l, i.target)), g[i.source.index] = (g[i
            .source.index] || 0) + 1, g[i.target.index] = (g[i.target.index] || 0) + 1;
          for (t = 0, p = new Array(a); t < a; ++t) i = e[t], p[t] = g[i.source.index] / (g[i.source.index] + g[i
            .target.index]);
          d = new Array(a), r(), c = new Array(a), s()
        }
      }

      function r() {
        if (m)
          for (var t = 0, n = e.length; t < n; ++t) d[t] = +b(e[t], t, e)
      }

      function s() {
        if (m)
          for (var t = 0, n = e.length; t < n; ++t) c[t] = +x(e[t], t, e)
      }
      var d, c, m, g, p, h = u,
        b = t,
        x = a(30),
        v = 1;
      return null == e && (e = []), i.initialize = function(e) {
        m = e, o()
      }, i.links = function(t) {
        return arguments.length ? (e = t, o(), i) : e
      }, i.id = function(e) {
        return arguments.length ? (h = e, i) : h
      }, i.iterations = function(e) {
        return arguments.length ? (v = +e, i) : v
      }, i.strength = function(e) {
        return arguments.length ? (b = "function" == typeof e ? e : a(+e), r(), i) : b
      }, i.distance = function(e) {
        return arguments.length ? (x = "function" == typeof e ? e : a(+e), s(), i) : x
      }, i
    }

    function g(e) {
      return e.x
    }

    function p(e) {
      return e.y
    }

    function h(e) {
      function t() {
        r(), h.call("tick", s), d < c && (p.stop(), h.call("end", s))
      }

      function r(t) {
        var n, i, o = e.length;
        void 0 === t && (t = 1);
        for (var r = 0; r < t; ++r)
          for (d += (f - d) * u, g.each(function(e) {
              e(d)
            }), n = 0; n < o; ++n) i = e[n], null == i.fx ? i.x += i.vx *= m : (i.x = i.fx, i.vx = 0), null == i.fy ?
            i.y += i.vy *= m : (i.y = i.fy, i.vy = 0);
        return s
      }

      function a() {
        for (var t, n = 0, i = e.length; n < i; ++n) {
          if (t = e[n], t.index = n, null != t.fx && (t.x = t.fx), null != t.fy && (t.y = t.fy), isNaN(t.x) || isNaN(t
              .y)) {
            var o = w * Math.sqrt(n),
              r = n * S;
            t.x = o * Math.cos(r), t.y = o * Math.sin(r)
          }(isNaN(t.vx) || isNaN(t.vy)) && (t.vx = t.vy = 0)
        }
      }

      function l(t) {
        return t.initialize && t.initialize(e), t
      }
      var s, d = 1,
        c = .001,
        u = 1 - Math.pow(c, 1 / 300),
        f = 0,
        m = .6,
        g = n.map(),
        p = o.timer(t),
        h = i.dispatch("tick", "end");
      return null == e && (e = []), a(), s = {
        tick: r,
        restart: function() {
          return p.restart(t), s
        },
        stop: function() {
          return p.stop(), s
        },
        nodes: function(t) {
          return arguments.length ? (e = t, a(), g.each(l), s) : e
        },
        alpha: function(e) {
          return arguments.length ? (d = +e, s) : d
        },
        alphaMin: function(e) {
          return arguments.length ? (c = +e, s) : c
        },
        alphaDecay: function(e) {
          return arguments.length ? (u = +e, s) : +u
        },
        alphaTarget: function(e) {
          return arguments.length ? (f = +e, s) : f
        },
        velocityDecay: function(e) {
          return arguments.length ? (m = 1 - e, s) : 1 - m
        },
        force: function(e, t) {
          return arguments.length > 1 ? (null == t ? g.remove(e) : g.set(e, l(t)), s) : g.get(e)
        },
        find: function(t, n, i) {
          var o, r, a, l, s, d = 0,
            c = e.length;
          for (null == i ? i = 1 / 0 : i *= i, d = 0; d < c; ++d) l = e[d], o = t - l.x, r = n - l.y, a = o * o +
            r * r, a < i && (s = l, i = a);
          return s
        },
        on: function(e, t) {
          return arguments.length > 1 ? (h.on(e, t), s) : h.on(e)
        }
      }
    }

    function b() {
      function e(e) {
        var n, a = r.length,
          l = t.quadtree(r, g, p).visitAfter(i);
        for (d = e, n = 0; n < a; ++n) s = r[n], l.visit(o)
      }

      function n() {
        if (r) {
          var e, t, n = r.length;
          for (c = new Array(n), e = 0; e < n; ++e) t = r[e], c[t.index] = +u(t, e, r)
        }
      }

      function i(e) {
        var t, n, i, o, r, a = 0,
          l = 0;
        if (e.length) {
          for (i = o = r = 0; r < 4; ++r)(t = e[r]) && (n = Math.abs(t.value)) && (a += t.value, l += n, i += n * t.x,
            o += n * t.y);
          e.x = i / l, e.y = o / l
        } else {
          t = e, t.x = t.data.x, t.y = t.data.y;
          do a += c[t.data.index]; while (t = t.next)
        }
        e.value = a
      }

      function o(e, t, n, i) {
        if (!e.value) return !0;
        var o = e.x - s.x,
          r = e.y - s.y,
          a = i - t,
          u = o * o + r * r;
        if (a * a / h < u) return u < m && (0 === o && (o = l(), u += o * o), 0 === r && (r = l(), u += r * r), u <
          f && (u = Math.sqrt(f * u)), s.vx += o * e.value * d / u, s.vy += r * e.value * d / u), !0;
        if (!(e.length || u >= m)) {
          (e.data !== s || e.next) && (0 === o && (o = l(), u += o * o), 0 === r && (r = l(), u += r * r), u < f && (
            u = Math.sqrt(f * u)));
          do e.data !== s && (a = c[e.data.index] * d / u, s.vx += o * a, s.vy += r * a); while (e = e.next)
        }
      }
      var r, s, d, c, u = a(-30),
        f = 1,
        m = 1 / 0,
        h = .81;
      return e.initialize = function(e) {
        r = e, n()
      }, e.strength = function(t) {
        return arguments.length ? (u = "function" == typeof t ? t : a(+t), n(), e) : u
      }, e.distanceMin = function(t) {
        return arguments.length ? (f = t * t, e) : Math.sqrt(f)
      }, e.distanceMax = function(t) {
        return arguments.length ? (m = t * t, e) : Math.sqrt(m)
      }, e.theta = function(t) {
        return arguments.length ? (h = t * t, e) : Math.sqrt(h)
      }, e
    }

    function x(e, t, n) {
      function i(e) {
        for (var i = 0, o = r.length; i < o; ++i) {
          var a = r[i],
            d = a.x - t || 1e-6,
            c = a.y - n || 1e-6,
            u = Math.sqrt(d * d + c * c),
            f = (s[i] - u) * l[i] * e / u;
          a.vx += d * f, a.vy += c * f
        }
      }

      function o() {
        if (r) {
          var t, n = r.length;
          for (l = new Array(n), s = new Array(n), t = 0; t < n; ++t) s[t] = +e(r[t], t, r), l[t] = isNaN(s[t]) ? 0 :
            +d(r[t], t, r)
        }
      }
      var r, l, s, d = a(.1);
      return "function" != typeof e && (e = a(+e)), null == t && (t = 0), null == n && (n = 0), i.initialize =
        function(e) {
          r = e, o()
        }, i.strength = function(e) {
          return arguments.length ? (d = "function" == typeof e ? e : a(+e), o(), i) : d
        }, i.radius = function(t) {
          return arguments.length ? (e = "function" == typeof t ? t : a(+t), o(), i) : e
        }, i.x = function(e) {
          return arguments.length ? (t = +e, i) : t
        }, i.y = function(e) {
          return arguments.length ? (n = +e, i) : n
        }, i
    }

    function v(e) {
      function t(e) {
        for (var t, n = 0, a = i.length; n < a; ++n) t = i[n], t.vx += (r[n] - t.x) * o[n] * e
      }

      function n() {
        if (i) {
          var t, n = i.length;
          for (o = new Array(n), r = new Array(n), t = 0; t < n; ++t) o[t] = isNaN(r[t] = +e(i[t], t, i)) ? 0 : +l(i[
            t], t, i)
        }
      }
      var i, o, r, l = a(.1);
      return "function" != typeof e && (e = a(null == e ? 0 : +e)), t.initialize = function(e) {
        i = e, n()
      }, t.strength = function(e) {
        return arguments.length ? (l = "function" == typeof e ? e : a(+e), n(), t) : l
      }, t.x = function(i) {
        return arguments.length ? (e = "function" == typeof i ? i : a(+i), n(), t) : e
      }, t
    }

    function y(e) {
      function t(e) {
        for (var t, n = 0, a = i.length; n < a; ++n) t = i[n], t.vy += (r[n] - t.y) * o[n] * e
      }

      function n() {
        if (i) {
          var t, n = i.length;
          for (o = new Array(n), r = new Array(n), t = 0; t < n; ++t) o[t] = isNaN(r[t] = +e(i[t], t, i)) ? 0 : +l(i[
            t], t, i)
        }
      }
      var i, o, r, l = a(.1);
      return "function" != typeof e && (e = a(null == e ? 0 : +e)), t.initialize = function(e) {
        i = e, n()
      }, t.strength = function(e) {
        return arguments.length ? (l = "function" == typeof e ? e : a(+e), n(), t) : l
      }, t.y = function(i) {
        return arguments.length ? (e = "function" == typeof i ? i : a(+i), n(), t) : e
      }, t
    }
    var w = 10,
      S = Math.PI * (3 - Math.sqrt(5));
    e.forceCenter = r, e.forceCollide = c, e.forceLink = m, e.forceManyBody = b, e.forceRadial = x, e
      .forceSimulation = h, e.forceX = v, e.forceY = y, Object.defineProperty(e, "__esModule", {
        value: !0
      })
  })
}
