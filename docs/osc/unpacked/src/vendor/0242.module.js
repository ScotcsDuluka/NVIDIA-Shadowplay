// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 242
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, r) {
    r(exports, require(30), require(64));
  }(this, function(e, t, n) {
    "use strict";

    function r(e) {
      return function(t, n) {
        return e(t.source.value + t.target.value, n.source.value + n.target.value);
      };
    }

    function i() {
      function e(e) {
        var r,
          s,
          c,
          u,
          l,
          d,
          f = e.length,
          h = [],
          p = t.range(f),
          m = [],
          y = [],
          b = y.groups = new Array(f),
          E = new Array(f * f);
        for (r = 0, l = -1; ++l < f;) {
          for (s = 0, d = -1; ++d < f;) s += e[l][d];
          h.push(s), m.push(t.range(f)), r += s;
        }
        for (i && p.sort(function(e, t) {
            return i(h[e], h[t]);
          }), o && m.forEach(function(t, n) {
            t.sort(function(t, r) {
              return o(e[n][t], e[n][r]);
            });
          }), r = g(0, v - n * f) / r, u = r ? n : v / f, s = 0, l = -1; ++l < f;) {
          for (c = s, d = -1; ++d < f;) {
            var _ = p[l],
              $ = m[_][d],
              w = e[_][$],
              T = s,
              C = s += w * r;
            E[$ * f + _] = {
              index: _,
              subindex: $,
              startAngle: T,
              endAngle: C,
              value: w
            };
          }
          b[_] = {
            index: _,
            startAngle: c,
            endAngle: s,
            value: h[_]
          }, s += u;
        }
        for (l = -1; ++l < f;)
          for (d = l - 1; ++d < f;) {
            var x = E[d * f + l],
              S = E[l * f + d];
            (x.value || S.value) && y.push(x.value < S.value ? {
              source: S,
              target: x
            } : {
              source: x,
              target: S
            });
          }
        return a ? y.sort(a) : y;
      }
      var n = 0,
        i = null,
        o = null,
        a = null;
      return e.padAngle = function(t) {
        return arguments.length ? (n = g(0, t), e) : n;
      }, e.sortGroups = function(t) {
        return arguments.length ? (i = t, e) : i;
      }, e.sortSubgroups = function(t) {
        return arguments.length ? (o = t, e) : o;
      }, e.sortChords = function(t) {
        return arguments.length ? (null == t ? a = null : (a = r(t))._ = t, e) : a && a._;
      }, e;
    }

    function o(e) {
      return function() {
        return e;
      };
    }

    function a(e) {
      return e.source;
    }

    function s(e) {
      return e.target;
    }

    function c(e) {
      return e.radius;
    }

    function u(e) {
      return e.startAngle;
    }

    function l(e) {
      return e.endAngle;
    }

    function d() {
      function e() {
        var e,
          o = y.call(arguments),
          a = t.apply(this, o),
          s = r.apply(this, o),
          c = +i.apply(this, (o[0] = a, o)),
          u = d.apply(this, o) - m,
          l = p.apply(this, o) - m,
          g = c * f(u),
          b = c * h(u),
          E = +i.apply(this, (o[0] = s, o)),
          _ = d.apply(this, o) - m,
          $ = p.apply(this, o) - m;
        if (v || (v = e = n.path()), v.moveTo(g, b), v.arc(0, 0, c, u, l), u === _ && l === $ || (v
            .quadraticCurveTo(0, 0, E * f(_), E * h(_)), v.arc(0, 0, E, _, $)), v.quadraticCurveTo(0, 0, g,
            b), v.closePath(), e) return v = null, e + "" || null;
      }
      var t = a,
        r = s,
        i = c,
        d = u,
        p = l,
        v = null;
      return e.radius = function(t) {
        return arguments.length ? (i = "function" == typeof t ? t : o(+t), e) : i;
      }, e.startAngle = function(t) {
        return arguments.length ? (d = "function" == typeof t ? t : o(+t), e) : d;
      }, e.endAngle = function(t) {
        return arguments.length ? (p = "function" == typeof t ? t : o(+t), e) : p;
      }, e.source = function(n) {
        return arguments.length ? (t = n, e) : t;
      }, e.target = function(t) {
        return arguments.length ? (r = t, e) : r;
      }, e.context = function(t) {
        return arguments.length ? (v = null == t ? null : t, e) : v;
      }, e;
    }
    var f = Math.cos,
      h = Math.sin,
      p = Math.PI,
      m = p / 2,
      v = 2 * p,
      g = Math.max,
      y = Array.prototype.slice;
    e.chord = i, e.ribbon = d, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
