// ─────────────────────────────────────────────────────────────
// APP MODULE 431
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(e, i) {
    i(t, n(44), n(83))
  }(this, function(e, t, n) {
    "use strict";

    function i(e) {
      return function(t, n) {
        return e(t.source.value + t.target.value, n.source.value + n.target.value)
      }
    }

    function o() {
      function e(e) {
        var i, l, s, d, c, u, f = e.length,
          m = [],
          g = t.range(f),
          p = [],
          x = [],
          v = x.groups = new Array(f),
          y = new Array(f * f);
        for (i = 0, c = -1; ++c < f;) {
          for (l = 0, u = -1; ++u < f;) l += e[c][u];
          m.push(l), p.push(t.range(f)), i += l
        }
        for (o && g.sort(function(e, t) {
            return o(m[e], m[t])
          }), r && p.forEach(function(t, n) {
            t.sort(function(t, i) {
              return r(e[n][t], e[n][i])
            })
          }), i = b(0, h - n * f) / i, d = i ? n : h / f, l = 0, c = -1; ++c < f;) {
          for (s = l, u = -1; ++u < f;) {
            var w = g[c],
              S = p[w][u],
              E = e[w][S],
              k = l,
              _ = l += E * i;
            y[S * f + w] = {
              index: w,
              subindex: S,
              startAngle: k,
              endAngle: _,
              value: E
            }
          }
          v[w] = {
            index: w,
            startAngle: s,
            endAngle: l,
            value: m[w]
          }, l += d
        }
        for (c = -1; ++c < f;)
          for (u = c - 1; ++u < f;) {
            var T = y[u * f + c],
              C = y[c * f + u];
            (T.value || C.value) && x.push(T.value < C.value ? {
              source: C,
              target: T
            } : {
              source: T,
              target: C
            })
          }
        return a ? x.sort(a) : x
      }
      var n = 0,
        o = null,
        r = null,
        a = null;
      return e.padAngle = function(t) {
        return arguments.length ? (n = b(0, t), e) : n
      }, e.sortGroups = function(t) {
        return arguments.length ? (o = t, e) : o
      }, e.sortSubgroups = function(t) {
        return arguments.length ? (r = t, e) : r
      }, e.sortChords = function(t) {
        return arguments.length ? (null == t ? a = null : (a = i(t))._ = t, e) : a && a._
      }, e
    }

    function r(e) {
      return function() {
        return e
      }
    }

    function a(e) {
      return e.source
    }

    function l(e) {
      return e.target
    }

    function s(e) {
      return e.radius
    }

    function d(e) {
      return e.startAngle
    }

    function c(e) {
      return e.endAngle
    }

    function u() {
      function e() {
        var e, r = x.call(arguments),
          a = t.apply(this, r),
          l = i.apply(this, r),
          s = +o.apply(this, (r[0] = a, r)),
          d = u.apply(this, r) - p,
          c = g.apply(this, r) - p,
          b = s * f(d),
          v = s * m(d),
          y = +o.apply(this, (r[0] = l, r)),
          w = u.apply(this, r) - p,
          S = g.apply(this, r) - p;
        if (h || (h = e = n.path()), h.moveTo(b, v), h.arc(0, 0, s, d, c), d === w && c === S || (h.quadraticCurveTo(
            0, 0, y * f(w), y * m(w)), h.arc(0, 0, y, w, S)), h.quadraticCurveTo(0, 0, b, v), h.closePath(), e)
        return h = null, e + "" || null
      }
      var t = a,
        i = l,
        o = s,
        u = d,
        g = c,
        h = null;
      return e.radius = function(t) {
        return arguments.length ? (o = "function" == typeof t ? t : r(+t), e) : o
      }, e.startAngle = function(t) {
        return arguments.length ? (u = "function" == typeof t ? t : r(+t), e) : u
      }, e.endAngle = function(t) {
        return arguments.length ? (g = "function" == typeof t ? t : r(+t), e) : g
      }, e.source = function(n) {
        return arguments.length ? (t = n, e) : t
      }, e.target = function(t) {
        return arguments.length ? (i = t, e) : i
      }, e.context = function(t) {
        return arguments.length ? (h = null == t ? null : t, e) : h
      }, e
    }
    var f = Math.cos,
      m = Math.sin,
      g = Math.PI,
      p = g / 2,
      h = 2 * g,
      b = Math.max,
      x = Array.prototype.slice;
    e.chord = o, e.ribbon = u, Object.defineProperty(e, "__esModule", {
      value: !0
    })
  })
}
