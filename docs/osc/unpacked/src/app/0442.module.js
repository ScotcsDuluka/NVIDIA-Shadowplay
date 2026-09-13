// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 442
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e) {
      return function() {
        return e;
      };
    }

    function n(e) {
      return e[0];
    }

    function i(e) {
      return e[1];
    }

    function o() {
      this._ = null;
    }

    function r(e) {
      e.U = e.C = e.L = e.R = e.P = e.N = null;
    }

    function a(e, t) {
      var n = t,
        i = t.R,
        o = n.U;
      o ? o.L === n ? o.L = i : o.R = i : e._ = i, i.U = o, n.U = i, n.R = i.L, n.R && (n.R.U = n), i.L = n;
    }

    function l(e, t) {
      var n = t,
        i = t.L,
        o = n.U;
      o ? o.L === n ? o.L = i : o.R = i : e._ = i, i.U = o, n.U = i, n.L = i.R, n.L && (n.L.U = n), i.R = n;
    }

    function s(e) {
      for (; e.L;) e = e.L;
      return e;
    }

    function d(e, t, n, i) {
      var o = [null, null],
        r = z.push(o) - 1;
      return o.left = e, o.right = t, n && u(o, e, t, n), i && u(o, t, e, i), F[e.index].halfedges.push(r),
        F[t.index].halfedges.push(r), o;
    }

    function c(e, t, n) {
      var i = [t, n];
      return i.left = e, i;
    }

    function u(e, t, n, i) {
      e[0] || e[1] ? e.left === n ? e[1] = i : e[0] = i : (e[0] = i, e.left = t, e.right = n);
    }

    function f(e, t, n, i, o) {
      var r,
        a = e[0],
        l = e[1],
        s = a[0],
        d = a[1],
        c = l[0],
        u = l[1],
        f = 0,
        m = 1,
        g = c - s,
        p = u - d;
      if (r = t - s, g || !(r > 0)) {
        if (r /= g, g < 0) {
          if (r < f) return;
          r < m && (m = r);
        } else if (g > 0) {
          if (r > m) return;
          r > f && (f = r);
        }
        if (r = i - s, g || !(r < 0)) {
          if (r /= g, g < 0) {
            if (r > m) return;
            r > f && (f = r);
          } else if (g > 0) {
            if (r < f) return;
            r < m && (m = r);
          }
          if (r = n - d, p || !(r > 0)) {
            if (r /= p, p < 0) {
              if (r < f) return;
              r < m && (m = r);
            } else if (p > 0) {
              if (r > m) return;
              r > f && (f = r);
            }
            if (r = o - d, p || !(r < 0)) {
              if (r /= p, p < 0) {
                if (r > m) return;
                r > f && (f = r);
              } else if (p > 0) {
                if (r < f) return;
                r < m && (m = r);
              }
              return !(f > 0 || m < 1) || (f > 0 && (e[0] = [s + f * g, d + f * p]), m < 1 && (e[1] = [s +
                m * g, d + m * p
              ]), !0);
            }
          }
        }
      }
    }

    function m(e, t, n, i, o) {
      var r = e[1];
      if (r) return !0;
      var a,
        l,
        s = e[0],
        d = e.left,
        c = e.right,
        u = d[0],
        f = d[1],
        m = c[0],
        g = c[1],
        p = (u + m) / 2,
        h = (f + g) / 2;
      if (g === f) {
        if (p < t || p >= i) return;
        if (u > m) {
          if (s) {
            if (s[1] >= o) return;
          } else s = [p, n];
          r = [p, o];
        } else {
          if (s) {
            if (s[1] < n) return;
          } else s = [p, o];
          r = [p, n];
        }
      } else if (a = (u - m) / (g - f), l = h - a * p, a < -1 || a > 1) {
        if (u > m) {
          if (s) {
            if (s[1] >= o) return;
          } else s = [(n - l) / a, n];
          r = [(o - l) / a, o];
        } else {
          if (s) {
            if (s[1] < n) return;
          } else s = [(o - l) / a, o];
          r = [(n - l) / a, n];
        }
      } else if (f < g) {
        if (s) {
          if (s[0] >= i) return;
        } else s = [t, a * t + l];
        r = [i, a * i + l];
      } else {
        if (s) {
          if (s[0] < t) return;
        } else s = [i, a * i + l];
        r = [t, a * t + l];
      }
      return e[0] = s, e[1] = r, !0;
    }

    function g(e, t, n, i) {
      for (var o, r = z.length; r--;) m(o = z[r], e, t, n, i) && f(o, e, t, n, i) && (Math.abs(o[0][0] - o[
        1][0]) > H || Math.abs(o[0][1] - o[1][1]) > H) || delete z[r];
    }

    function p(e) {
      return F[e.index] = {
        site: e,
        halfedges: []
      };
    }

    function h(e, t) {
      var n = e.site,
        i = t.left,
        o = t.right;
      return n === o && (o = i, i = n), o ? Math.atan2(o[1] - i[1], o[0] - i[0]) : (n === i ? (i = t[1], o =
        t[0]) : (i = t[0], o = t[1]), Math.atan2(i[0] - o[0], o[1] - i[1]));
    }

    function b(e, t) {
      return t[+(t.left !== e.site)];
    }

    function x(e, t) {
      return t[+(t.left === e.site)];
    }

    function v() {
      for (var e, t, n, i, o = 0, r = F.length; o < r; ++o)
        if ((e = F[o]) && (i = (t = e.halfedges).length)) {
          var a = new Array(i),
            l = new Array(i);
          for (n = 0; n < i; ++n) a[n] = n, l[n] = h(e, z[t[n]]);
          for (a.sort(function(e, t) {
              return l[t] - l[e];
            }), n = 0; n < i; ++n) l[n] = t[a[n]];
          for (n = 0; n < i; ++n) t[n] = l[n];
        }
    }

    function y(e, t, n, i) {
      var o,
        r,
        a,
        l,
        s,
        d,
        u,
        f,
        m,
        g,
        p,
        h,
        v = F.length,
        y = !0;
      for (o = 0; o < v; ++o)
        if (r = F[o]) {
          for (a = r.site, s = r.halfedges, l = s.length; l--;) z[s[l]] || s.splice(l, 1);
          for (l = 0, d = s.length; l < d;) g = x(r, z[s[l]]), p = g[0], h = g[1], u = b(r, z[s[++l % d]]),
            f = u[0], m = u[1], (Math.abs(p - f) > H || Math.abs(h - m) > H) && (s.splice(l, 0, z.push(c(a,
              g, Math.abs(p - e) < H && i - h > H ? [e, Math.abs(f - e) < H ? m : i] : Math.abs(h -
              i) < H && n - p > H ? [Math.abs(m - i) < H ? f : n, i] : Math.abs(p - n) < H && h - t >
              H ? [n, Math.abs(f - n) < H ? m : t] : Math.abs(h - t) < H && p - e > H ? [Math.abs(m -
                t) < H ? f : e, t] : null)) - 1), ++d);
          d && (y = !1);
        }
      if (y) {
        var w,
          S,
          E,
          k = 1 / 0;
        for (o = 0, y = null; o < v; ++o)(r = F[o]) && (a = r.site, w = a[0] - e, S = a[1] - t, E = w * w +
          S * S, E < k && (k = E, y = r));
        if (y) {
          var _ = [e, t],
            T = [e, i],
            C = [n, i],
            O = [n, t];
          y.halfedges.push(z.push(c(a = y.site, _, T)) - 1, z.push(c(a, T, C)) - 1, z.push(c(a, C, O)) - 1,
            z.push(c(a, O, _)) - 1);
        }
      }
      for (o = 0; o < v; ++o)(r = F[o]) && (r.halfedges.length || delete F[o]);
    }

    function w() {
      r(this), this.x = this.y = this.arc = this.site = this.cy = null;
    }

    function S(e) {
      var t = e.P,
        n = e.N;
      if (t && n) {
        var i = t.site,
          o = e.site,
          r = n.site;
        if (i !== r) {
          var a = o[0],
            l = o[1],
            s = i[0] - a,
            d = i[1] - l,
            c = r[0] - a,
            u = r[1] - l,
            f = 2 * (s * u - d * c);
          if (!(f >= -B)) {
            var m = s * s + d * d,
              g = c * c + u * u,
              p = (u * m - d * g) / f,
              h = (s * g - c * m) / f,
              b = G.pop() || new w();
            b.arc = e, b.site = o, b.x = p + a, b.y = (b.cy = h + l) + Math.sqrt(p * p + h * h), e.circle =
              b;
            for (var x = null, v = U._; v;)
              if (b.y < v.y || b.y === v.y && b.x <= v.x) {
                if (!v.L) {
                  x = v.P;
                  break;
                }
                v = v.L;
              } else {
                if (!v.R) {
                  x = v;
                  break;
                }
                v = v.R;
              }
            U.insert(x, b), x || (N = b);
          }
        }
      }
    }

    function E(e) {
      var t = e.circle;
      t && (t.P || (N = t.N), U.remove(t), G.push(t), r(t), e.circle = null);
    }

    function k() {
      r(this), this.edge = this.site = this.circle = null;
    }

    function _(e) {
      var t = V.pop() || new k();
      return t.site = e, t;
    }

    function T(e) {
      E(e), L.remove(e), V.push(e), r(e);
    }

    function C(e) {
      var t = e.circle,
        n = t.x,
        i = t.cy,
        o = [n, i],
        r = e.P,
        a = e.N,
        l = [e];
      T(e);
      for (var s = r; s.circle && Math.abs(n - s.circle.x) < H && Math.abs(i - s.circle.cy) < H;) r = s.P, l
        .unshift(s), T(s), s = r;
      l.unshift(s), E(s);
      for (var c = a; c.circle && Math.abs(n - c.circle.x) < H && Math.abs(i - c.circle.cy) < H;) a = c.N, l
        .push(c), T(c), c = a;
      l.push(c), E(c);
      var f,
        m = l.length;
      for (f = 1; f < m; ++f) c = l[f], s = l[f - 1], u(c.edge, s.site, c.site, o);
      s = l[0], c = l[m - 1], c.edge = d(s.site, c.site, null, o), S(s), S(c);
    }

    function O(e) {
      for (var t, n, i, o, r = e[0], a = e[1], l = L._; l;)
        if (i = A(l, a) - r, i > H) l = l.L;
        else {
          if (o = r - I(l, a), !(o > H)) {
            i > -H ? (t = l.P, n = l) : o > -H ? (t = l, n = l.N) : t = n = l;
            break;
          }
          if (!l.R) {
            t = l;
            break;
          }
          l = l.R;
        }
      p(e);
      var s = _(e);
      if (L.insert(t, s), t || n) {
        if (t === n) return E(t), n = _(t.site), L.insert(s, n), s.edge = n.edge = d(t.site, s.site), S(t),
          void S(n);
        if (!n) return void(s.edge = d(t.site, s.site));
        E(t), E(n);
        var c = t.site,
          f = c[0],
          m = c[1],
          g = e[0] - f,
          h = e[1] - m,
          b = n.site,
          x = b[0] - f,
          v = b[1] - m,
          y = 2 * (g * v - h * x),
          w = g * g + h * h,
          k = x * x + v * v,
          T = [(v * w - h * k) / y + f, (g * k - x * w) / y + m];
        u(n.edge, c, b, T), s.edge = d(c, e, null, T), n.edge = d(e, b, null, T), S(t), S(n);
      }
    }

    function A(e, t) {
      var n = e.site,
        i = n[0],
        o = n[1],
        r = o - t;
      if (!r) return i;
      var a = e.P;
      if (!a) return -(1 / 0);
      n = a.site;
      var l = n[0],
        s = n[1],
        d = s - t;
      if (!d) return l;
      var c = l - i,
        u = 1 / r - 1 / d,
        f = c / d;
      return u ? (-f + Math.sqrt(f * f - 2 * u * (c * c / (-2 * d) - s + d / 2 + o - r / 2))) / u + i : (i +
        l) / 2;
    }

    function I(e, t) {
      var n = e.N;
      if (n) return A(n, t);
      var i = e.site;
      return i[1] === t ? i[0] : 1 / 0;
    }

    function M(e, t, n) {
      return (e[0] - n[0]) * (t[1] - e[1]) - (e[0] - t[0]) * (n[1] - e[1]);
    }

    function R(e, t) {
      return t[1] - e[1] || t[0] - e[0];
    }

    function P(e, t) {
      var n,
        i,
        r,
        a = e.sort(R).pop();
      for (z = [], F = new Array(e.length), L = new o(), U = new o();;)
        if (r = N, a && (!r || a[1] < r.y || a[1] === r.y && a[0] < r.x)) a[0] === n && a[1] === i || (O(a),
          n = a[0], i = a[1]), a = e.pop();
        else {
          if (!r) break;
          C(r.arc);
        }
      if (v(), t) {
        var l = +t[0][0],
          s = +t[0][1],
          d = +t[1][0],
          c = +t[1][1];
        g(l, s, d, c), y(l, s, d, c);
      }
      this.edges = z, this.cells = F, L = U = z = F = null;
    }

    function D() {
      function e(e) {
        return new P(e.map(function(t, n) {
          var i = [Math.round(o(t, n, e) / H) * H, Math.round(r(t, n, e) / H) * H];
          return i.index = n, i.data = t, i;
        }), a);
      }
      var o = n,
        r = i,
        a = null;
      return e.polygons = function(t) {
        return e(t).polygons();
      }, e.links = function(t) {
        return e(t).links();
      }, e.triangles = function(t) {
        return e(t).triangles();
      }, e.x = function(n) {
        return arguments.length ? (o = "function" == typeof n ? n : t(+n), e) : o;
      }, e.y = function(n) {
        return arguments.length ? (r = "function" == typeof n ? n : t(+n), e) : r;
      }, e.extent = function(t) {
        return arguments.length ? (a = null == t ? null : [
          [+t[0][0], +t[0][1]],
          [+t[1][0], +t[1][1]]
        ], e) : a && [
          [a[0][0], a[0][1]],
          [a[1][0], a[1][1]]
        ];
      }, e.size = function(t) {
        return arguments.length ? (a = null == t ? null : [
          [0, 0],
          [+t[0], +t[1]]
        ], e) : a && [a[1][0] - a[0][0], a[1][1] - a[0][1]];
      }, e;
    }
    o.prototype = {
      constructor: o,
      insert: function(e, t) {
        var n, i, o;
        if (e) {
          if (t.P = e, t.N = e.N, e.N && (e.N.P = t), e.N = t, e.R) {
            for (e = e.R; e.L;) e = e.L;
            e.L = t;
          } else e.R = t;
          n = e;
        } else this._ ? (e = s(this._), t.P = null, t.N = e, e.P = e.L = t, n = e) : (t.P = t.N = null,
          this._ = t, n = null);
        for (t.L = t.R = null, t.U = n, t.C = !0, e = t; n && n.C;) i = n.U, n === i.L ? (o = i.R, o &&
          o.C ? (n.C = o.C = !1, i.C = !0, e = i) : (e === n.R && (a(this, n), e = n, n = e.U), n
            .C = !1, i.C = !0, l(this, i))) : (o = i.L, o && o.C ? (n.C = o.C = !1, i.C = !0, e = i) :
          (e === n.L && (l(this, n), e = n, n = e.U), n.C = !1, i.C = !0, a(this, i))), n = e.U;
        this._.C = !1;
      },
      remove: function(e) {
        e.N && (e.N.P = e.P), e.P && (e.P.N = e.N), e.N = e.P = null;
        var t,
          n,
          i,
          o = e.U,
          r = e.L,
          d = e.R;
        if (n = r ? d ? s(d) : r : d, o ? o.L === e ? o.L = n : o.R = n : this._ = n, r && d ? (i = n.C,
            n.C = e.C, n.L = r, r.U = n, n !== d ? (o = n.U, n.U = e.U, e = n.R, o.L = e, n.R = d, d.U =
              n) : (n.U = o, o = n, e = n.R)) : (i = e.C, e = n), e && (e.U = o), !i) {
          if (e && e.C) return void(e.C = !1);
          do {
            if (e === this._) break;
            if (e === o.L) {
              if (t = o.R, t.C && (t.C = !1, o.C = !0, a(this, o), t = o.R), t.L && t.L.C || t.R && t.R
                .C) {
                t.R && t.R.C || (t.L.C = !1, t.C = !0, l(this, t), t = o.R), t.C = o.C, o.C = t.R.C = !
                  1, a(this, o), e = this._;
                break;
              }
            } else if (t = o.L, t.C && (t.C = !1, o.C = !0, l(this, o), t = o.L), t.L && t.L.C || t.R &&
              t.R.C) {
              t.L && t.L.C || (t.R.C = !1, t.C = !0, a(this, t), t = o.L), t.C = o.C, o.C = t.L.C = !1,
                l(this, o), e = this._;
              break;
            }
            t.C = !0, e = o, o = o.U;
          } while (!e.C);
          e && (e.C = !1);
        }
      }
    };
    var N,
      L,
      F,
      U,
      z,
      G = [],
      V = [],
      H = 1e-6,
      B = 1e-12;
    P.prototype = {
      constructor: P,
      polygons: function() {
        var e = this.edges;
        return this.cells.map(function(t) {
          var n = t.halfedges.map(function(n) {
            return b(t, e[n]);
          });
          return n.data = t.site.data, n;
        });
      },
      triangles: function() {
        var e = [],
          t = this.edges;
        return this.cells.forEach(function(n, i) {
          if (r = (o = n.halfedges).length)
            for (var o, r, a, l = n.site, s = -1, d = t[o[r - 1]], c = d.left === l ? d.right : d
                .left; ++s < r;) a = c, d = t[o[s]], c = d.left === l ? d.right : d.left, a && c &&
              i < a.index && i < c.index && M(l, a, c) < 0 && e.push([l.data, a.data, c.data]);
        }), e;
      },
      links: function() {
        return this.edges.filter(function(e) {
          return e.right;
        }).map(function(e) {
          return {
            source: e.left.data,
            target: e.right.data
          };
        });
      },
      find: function(e, t, n) {
        for (var i, o, r = this, a = r._found || 0, l = r.cells.length; !(o = r.cells[a]);)
          if (++a >= l) return null;
        var s = e - o.site[0],
          d = t - o.site[1],
          c = s * s + d * d;
        do o = r.cells[i = a], a = null, o.halfedges.forEach(function(n) {
          var i = r.edges[n],
            l = i.left;
          if (l !== o.site && l || (l = i.right)) {
            var s = e - l[0],
              d = t - l[1],
              u = s * s + d * d;
            u < c && (c = u, a = l.index);
          }
        }); while (null !== a);
        return r._found = i, null == n || c <= n * n ? o.site : null;
      }
    }, e.voronoi = D, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
