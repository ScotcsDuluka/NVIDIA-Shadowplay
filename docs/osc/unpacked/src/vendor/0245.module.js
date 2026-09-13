// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 245
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, r) {
    r(exports, require(102), require(62), require(20), require(66));
  }(this, function(e, t, n, r, i) {
    "use strict";

    function o(e, t) {
      function n() {
        var n,
          i,
          o = r.length,
          a = 0,
          s = 0;
        for (n = 0; n < o; ++n) i = r[n], a += i.x, s += i.y;
        for (a = a / o - e, s = s / o - t, n = 0; n < o; ++n) i = r[n], i.x -= a, i.y -= s;
      }
      var r;
      return null == e && (e = 0), null == t && (t = 0), n.initialize = function(e) {
        r = e;
      }, n.x = function(t) {
        return arguments.length ? (e = +t, n) : e;
      }, n.y = function(e) {
        return arguments.length ? (t = +e, n) : t;
      }, n;
    }

    function a(e) {
      return function() {
        return e;
      };
    }

    function s() {
      return 1e-6 * (Math.random() - .5);
    }

    function c(e) {
      return e.x + e.vx;
    }

    function u(e) {
      return e.y + e.vy;
    }

    function l(e) {
      function n() {
        function e(e, t, n, r, i) {
          var o = e.data,
            c = e.r,
            u = m + c;
          {
            if (!o) return t > h + u || r < h - u || n > p + u || i < p - u;
            if (o.index > a.index) {
              var l = h - o.x - o.vx,
                f = p - o.y - o.vy,
                g = l * l + f * f;
              g < u * u && (0 === l && (l = s(), g += l * l), 0 === f && (f = s(), g += f * f), g = (u - (
                g = Math.sqrt(g))) / g * d, a.vx += (l *= g) * (u = (c *= c) / (v + c)), a.vy += (f *=
                g) * u, o.vx -= l * (u = 1 - u), o.vy -= f * u);
            }
          }
        }
        for (var n, i, a, h, p, m, v, g = o.length, y = 0; y < f; ++y)
          for (i = t.quadtree(o, c, u).visitAfter(r), n = 0; n < g; ++n) a = o[n], m = l[a.index], v = m *
            m, h = a.x + a.vx, p = a.y + a.vy, i.visit(e);
      }

      function r(e) {
        if (e.data) return e.r = l[e.data.index];
        for (var t = e.r = 0; t < 4; ++t) e[t] && e[t].r > e.r && (e.r = e[t].r);
      }

      function i() {
        if (o) {
          var t,
            n,
            r = o.length;
          for (l = new Array(r), t = 0; t < r; ++t) n = o[t], l[n.index] = +e(n, t, o);
        }
      }
      var o,
        l,
        d = 1,
        f = 1;
      return "function" != typeof e && (e = a(null == e ? 1 : +e)), n.initialize = function(e) {
        o = e, i();
      }, n.iterations = function(e) {
        return arguments.length ? (f = +e, n) : f;
      }, n.strength = function(e) {
        return arguments.length ? (d = +e, n) : d;
      }, n.radius = function(t) {
        return arguments.length ? (e = "function" == typeof t ? t : a(+t), i(), n) : e;
      }, n;
    }

    function d(e) {
      return e.index;
    }

    function f(e, t) {
      var n = e.get(t);
      if (!n) throw new Error("missing: " + t);
      return n;
    }

    function h(e) {
      function t(e) {
        return 1 / Math.min(p[e.source.index], p[e.target.index]);
      }

      function r(t) {
        for (var n = 0, r = e.length; n < b; ++n)
          for (var i, o, a, c, d, f, h, p = 0; p < r; ++p) i = e[p], o = i.source, a = i.target, c = a.x + a
            .vx - o.x - o.vx || s(), d = a.y + a.vy - o.y - o.vy || s(), f = Math.sqrt(c * c + d * d), f = (
              f - l[p]) / f * t * u[p], c *= f, d *= f, a.vx -= c * (h = m[p]), a.vy -= d * h, o.vx += c * (
              h = 1 - h), o.vy += d * h;
      }

      function i() {
        if (h) {
          var t,
            r,
            i = h.length,
            a = e.length,
            s = n.map(h, v);
          for (t = 0, p = new Array(i); t < a; ++t) r = e[t], r.index = t, "object" != typeof r.source && (r
              .source = f(s, r.source)), "object" != typeof r.target && (r.target = f(s, r.target)), p[r
              .source.index] = (p[r.source.index] || 0) + 1, p[r.target.index] = (p[r.target.index] || 0) +
            1;
          for (t = 0, m = new Array(a); t < a; ++t) r = e[t], m[t] = p[r.source.index] / (p[r.source
            .index] + p[r.target.index]);
          u = new Array(a), o(), l = new Array(a), c();
        }
      }

      function o() {
        if (h)
          for (var t = 0, n = e.length; t < n; ++t) u[t] = +g(e[t], t, e);
      }

      function c() {
        if (h)
          for (var t = 0, n = e.length; t < n; ++t) l[t] = +y(e[t], t, e);
      }
      var u,
        l,
        h,
        p,
        m,
        v = d,
        g = t,
        y = a(30),
        b = 1;
      return null == e && (e = []), r.initialize = function(e) {
        h = e, i();
      }, r.links = function(t) {
        return arguments.length ? (e = t, i(), r) : e;
      }, r.id = function(e) {
        return arguments.length ? (v = e, r) : v;
      }, r.iterations = function(e) {
        return arguments.length ? (b = +e, r) : b;
      }, r.strength = function(e) {
        return arguments.length ? (g = "function" == typeof e ? e : a(+e), o(), r) : g;
      }, r.distance = function(e) {
        return arguments.length ? (y = "function" == typeof e ? e : a(+e), c(), r) : y;
      }, r;
    }

    function p(e) {
      return e.x;
    }

    function m(e) {
      return e.y;
    }

    function v(e) {
      function t() {
        o(), v.call("tick", c), u < l && (m.stop(), v.call("end", c));
      }

      function o(t) {
        var n,
          r,
          i = e.length;
        void 0 === t && (t = 1);
        for (var o = 0; o < t; ++o)
          for (u += (f - u) * d, p.each(function(e) {
              e(u);
            }), n = 0; n < i; ++n) r = e[n], null == r.fx ? r.x += r.vx *= h : (r.x = r.fx, r.vx = 0),
            null == r.fy ? r.y += r.vy *= h : (r.y = r.fy, r.vy = 0);
        return c;
      }

      function a() {
        for (var t, n = 0, r = e.length; n < r; ++n) {
          if (t = e[n], t.index = n, null != t.fx && (t.x = t.fx), null != t.fy && (t.y = t.fy), isNaN(t
            .x) || isNaN(t.y)) {
            var i = _ * Math.sqrt(n),
              o = n * $;
            t.x = i * Math.cos(o), t.y = i * Math.sin(o);
          }
          (isNaN(t.vx) || isNaN(t.vy)) && (t.vx = t.vy = 0);
        }
      }

      function s(t) {
        return t.initialize && t.initialize(e), t;
      }
      var c,
        u = 1,
        l = .001,
        d = 1 - Math.pow(l, 1 / 300),
        f = 0,
        h = .6,
        p = n.map(),
        m = i.timer(t),
        v = r.dispatch("tick", "end");
      return null == e && (e = []), a(), c = {
        tick: o,
        restart: function() {
          return m.restart(t), c;
        },
        stop: function() {
          return m.stop(), c;
        },
        nodes: function(t) {
          return arguments.length ? (e = t, a(), p.each(s), c) : e;
        },
        alpha: function(e) {
          return arguments.length ? (u = +e, c) : u;
        },
        alphaMin: function(e) {
          return arguments.length ? (l = +e, c) : l;
        },
        alphaDecay: function(e) {
          return arguments.length ? (d = +e, c) : +d;
        },
        alphaTarget: function(e) {
          return arguments.length ? (f = +e, c) : f;
        },
        velocityDecay: function(e) {
          return arguments.length ? (h = 1 - e, c) : 1 - h;
        },
        force: function(e, t) {
          return arguments.length > 1 ? (null == t ? p.remove(e) : p.set(e, s(t)), c) : p.get(e);
        },
        find: function(t, n, r) {
          var i,
            o,
            a,
            s,
            c,
            u = 0,
            l = e.length;
          for (null == r ? r = 1 / 0 : r *= r, u = 0; u < l; ++u) s = e[u], i = t - s.x, o = n - s.y,
            a = i * i + o * o, a < r && (c = s, r = a);
          return c;
        },
        on: function(e, t) {
          return arguments.length > 1 ? (v.on(e, t), c) : v.on(e);
        }
      };
    }

    function g() {
      function e(e) {
        var n,
          a = o.length,
          s = t.quadtree(o, p, m).visitAfter(r);
        for (u = e, n = 0; n < a; ++n) c = o[n], s.visit(i);
      }

      function n() {
        if (o) {
          var e,
            t,
            n = o.length;
          for (l = new Array(n), e = 0; e < n; ++e) t = o[e], l[t.index] = +d(t, e, o);
        }
      }

      function r(e) {
        var t,
          n,
          r,
          i,
          o,
          a = 0,
          s = 0;
        if (e.length) {
          for (r = i = o = 0; o < 4; ++o)(t = e[o]) && (n = Math.abs(t.value)) && (a += t.value, s += n,
            r += n * t.x, i += n * t.y);
          e.x = r / s, e.y = i / s;
        } else {
          t = e, t.x = t.data.x, t.y = t.data.y;
          do a += l[t.data.index]; while (t = t.next);
        }
        e.value = a;
      }

      function i(e, t, n, r) {
        if (!e.value) return !0;
        var i = e.x - c.x,
          o = e.y - c.y,
          a = r - t,
          d = i * i + o * o;
        if (a * a / v < d) return d < h && (0 === i && (i = s(), d += i * i), 0 === o && (o = s(), d += o *
            o), d < f && (d = Math.sqrt(f * d)), c.vx += i * e.value * u / d, c.vy += o * e.value * u /
          d), !0;
        if (!(e.length || d >= h)) {
          (e.data !== c || e.next) && (0 === i && (i = s(), d += i * i), 0 === o && (o = s(), d += o * o),
            d < f && (d = Math.sqrt(f * d)));
          do e.data !== c && (a = l[e.data.index] * u / d, c.vx += i * a, c.vy += o * a); while (e = e
            .next);
        }
      }
      var o,
        c,
        u,
        l,
        d = a(-30),
        f = 1,
        h = 1 / 0,
        v = .81;
      return e.initialize = function(e) {
        o = e, n();
      }, e.strength = function(t) {
        return arguments.length ? (d = "function" == typeof t ? t : a(+t), n(), e) : d;
      }, e.distanceMin = function(t) {
        return arguments.length ? (f = t * t, e) : Math.sqrt(f);
      }, e.distanceMax = function(t) {
        return arguments.length ? (h = t * t, e) : Math.sqrt(h);
      }, e.theta = function(t) {
        return arguments.length ? (v = t * t, e) : Math.sqrt(v);
      }, e;
    }

    function y(e, t, n) {
      function r(e) {
        for (var r = 0, i = o.length; r < i; ++r) {
          var a = o[r],
            u = a.x - t || 1e-6,
            l = a.y - n || 1e-6,
            d = Math.sqrt(u * u + l * l),
            f = (c[r] - d) * s[r] * e / d;
          a.vx += u * f, a.vy += l * f;
        }
      }

      function i() {
        if (o) {
          var t,
            n = o.length;
          for (s = new Array(n), c = new Array(n), t = 0; t < n; ++t) c[t] = +e(o[t], t, o), s[t] = isNaN(c[
            t]) ? 0 : +u(o[t], t, o);
        }
      }
      var o,
        s,
        c,
        u = a(.1);
      return "function" != typeof e && (e = a(+e)), null == t && (t = 0), null == n && (n = 0), r
        .initialize = function(e) {
          o = e, i();
        }, r.strength = function(e) {
          return arguments.length ? (u = "function" == typeof e ? e : a(+e), i(), r) : u;
        }, r.radius = function(t) {
          return arguments.length ? (e = "function" == typeof t ? t : a(+t), i(), r) : e;
        }, r.x = function(e) {
          return arguments.length ? (t = +e, r) : t;
        }, r.y = function(e) {
          return arguments.length ? (n = +e, r) : n;
        }, r;
    }

    function b(e) {
      function t(e) {
        for (var t, n = 0, a = r.length; n < a; ++n) t = r[n], t.vx += (o[n] - t.x) * i[n] * e;
      }

      function n() {
        if (r) {
          var t,
            n = r.length;
          for (i = new Array(n), o = new Array(n), t = 0; t < n; ++t) i[t] = isNaN(o[t] = +e(r[t], t, r)) ?
            0 : +s(r[t], t, r);
        }
      }
      var r,
        i,
        o,
        s = a(.1);
      return "function" != typeof e && (e = a(null == e ? 0 : +e)), t.initialize = function(e) {
        r = e, n();
      }, t.strength = function(e) {
        return arguments.length ? (s = "function" == typeof e ? e : a(+e), n(), t) : s;
      }, t.x = function(r) {
        return arguments.length ? (e = "function" == typeof r ? r : a(+r), n(), t) : e;
      }, t;
    }

    function E(e) {
      function t(e) {
        for (var t, n = 0, a = r.length; n < a; ++n) t = r[n], t.vy += (o[n] - t.y) * i[n] * e;
      }

      function n() {
        if (r) {
          var t,
            n = r.length;
          for (i = new Array(n), o = new Array(n), t = 0; t < n; ++t) i[t] = isNaN(o[t] = +e(r[t], t, r)) ?
            0 : +s(r[t], t, r);
        }
      }
      var r,
        i,
        o,
        s = a(.1);
      return "function" != typeof e && (e = a(null == e ? 0 : +e)), t.initialize = function(e) {
        r = e, n();
      }, t.strength = function(e) {
        return arguments.length ? (s = "function" == typeof e ? e : a(+e), n(), t) : s;
      }, t.y = function(r) {
        return arguments.length ? (e = "function" == typeof r ? r : a(+r), n(), t) : e;
      }, t;
    }
    var _ = 10,
      $ = Math.PI * (3 - Math.sqrt(5));
    e.forceCenter = o, e.forceCollide = l, e.forceLink = h, e.forceManyBody = g, e.forceRadial = y, e
      .forceSimulation = v, e.forceX = b, e.forceY = E, Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
