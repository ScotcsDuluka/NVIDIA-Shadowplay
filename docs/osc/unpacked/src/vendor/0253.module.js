// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 253
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

    function r(e) {
      return e[1];
    }

    function i() {
      this._ = null;
    }

    function o(e) {
      e.U = e.C = e.L = e.R = e.P = e.N = null;
    }

    function a(e, t) {
      var n = t,
        r = t.R,
        i = n.U;
      i ? i.L === n ? i.L = r : i.R = r : e._ = r, r.U = i, n.U = r, n.R = r.L, n.R && (n.R.U = n), r.L = n;
    }

    function s(e, t) {
      var n = t,
        r = t.L,
        i = n.U;
      i ? i.L === n ? i.L = r : i.R = r : e._ = r, r.U = i, n.U = r, n.L = r.R, n.L && (n.L.U = n), r.R = n;
    }

    function c(e) {
      for (; e.L;) e = e.L;
      return e;
    }

    function u(e, t, n, r) {
      var i = [null, null],
        o = F.push(i) - 1;
      return i.left = e, i.right = t, n && d(i, e, t, n), r && d(i, t, e, r), L[e.index].halfedges.push(o),
        L[t.index].halfedges.push(o), i;
    }

    function l(e, t, n) {
      var r = [t, n];
      return r.left = e, r;
    }

    function d(e, t, n, r) {
      e[0] || e[1] ? e.left === n ? e[1] = r : e[0] = r : (e[0] = r, e.left = t, e.right = n);
    }

    function f(e, t, n, r, i) {
      var o,
        a = e[0],
        s = e[1],
        c = a[0],
        u = a[1],
        l = s[0],
        d = s[1],
        f = 0,
        h = 1,
        p = l - c,
        m = d - u;
      if (o = t - c, p || !(o > 0)) {
        if (o /= p, p < 0) {
          if (o < f) return;
          o < h && (h = o);
        } else if (p > 0) {
          if (o > h) return;
          o > f && (f = o);
        }
        if (o = r - c, p || !(o < 0)) {
          if (o /= p, p < 0) {
            if (o > h) return;
            o > f && (f = o);
          } else if (p > 0) {
            if (o < f) return;
            o < h && (h = o);
          }
          if (o = n - u, m || !(o > 0)) {
            if (o /= m, m < 0) {
              if (o < f) return;
              o < h && (h = o);
            } else if (m > 0) {
              if (o > h) return;
              o > f && (f = o);
            }
            if (o = i - u, m || !(o < 0)) {
              if (o /= m, m < 0) {
                if (o > h) return;
                o > f && (f = o);
              } else if (m > 0) {
                if (o < f) return;
                o < h && (h = o);
              }
              return !(f > 0 || h < 1) || (f > 0 && (e[0] = [c + f * p, u + f * m]), h < 1 && (e[1] = [c +
                h * p, u + h * m
              ]), !0);
            }
          }
        }
      }
    }

    function h(e, t, n, r, i) {
      var o = e[1];
      if (o) return !0;
      var a,
        s,
        c = e[0],
        u = e.left,
        l = e.right,
        d = u[0],
        f = u[1],
        h = l[0],
        p = l[1],
        m = (d + h) / 2,
        v = (f + p) / 2;
      if (p === f) {
        if (m < t || m >= r) return;
        if (d > h) {
          if (c) {
            if (c[1] >= i) return;
          } else c = [m, n];
          o = [m, i];
        } else {
          if (c) {
            if (c[1] < n) return;
          } else c = [m, i];
          o = [m, n];
        }
      } else if (a = (d - h) / (p - f), s = v - a * m, a < -1 || a > 1) {
        if (d > h) {
          if (c) {
            if (c[1] >= i) return;
          } else c = [(n - s) / a, n];
          o = [(i - s) / a, i];
        } else {
          if (c) {
            if (c[1] < n) return;
          } else c = [(i - s) / a, i];
          o = [(n - s) / a, n];
        }
      } else if (f < p) {
        if (c) {
          if (c[0] >= r) return;
        } else c = [t, a * t + s];
        o = [r, a * r + s];
      } else {
        if (c) {
          if (c[0] < t) return;
        } else c = [r, a * r + s];
        o = [t, a * t + s];
      }
      return e[0] = c, e[1] = o, !0;
    }

    function p(e, t, n, r) {
      for (var i, o = F.length; o--;) h(i = F[o], e, t, n, r) && f(i, e, t, n, r) && (Math.abs(i[0][0] - i[
        1][0]) > B || Math.abs(i[0][1] - i[1][1]) > B) || delete F[o];
    }

    function m(e) {
      return L[e.index] = {
        site: e,
        halfedges: []
      };
    }

    function v(e, t) {
      var n = e.site,
        r = t.left,
        i = t.right;
      return n === i && (i = r, r = n), i ? Math.atan2(i[1] - r[1], i[0] - r[0]) : (n === r ? (r = t[1], i =
        t[0]) : (r = t[0], i = t[1]), Math.atan2(r[0] - i[0], i[1] - r[1]));
    }

    function g(e, t) {
      return t[+(t.left !== e.site)];
    }

    function y(e, t) {
      return t[+(t.left === e.site)];
    }

    function b() {
      for (var e, t, n, r, i = 0, o = L.length; i < o; ++i)
        if ((e = L[i]) && (r = (t = e.halfedges).length)) {
          var a = new Array(r),
            s = new Array(r);
          for (n = 0; n < r; ++n) a[n] = n, s[n] = v(e, F[t[n]]);
          for (a.sort(function(e, t) {
              return s[t] - s[e];
            }), n = 0; n < r; ++n) s[n] = t[a[n]];
          for (n = 0; n < r; ++n) t[n] = s[n];
        }
    }

    function E(e, t, n, r) {
      var i,
        o,
        a,
        s,
        c,
        u,
        d,
        f,
        h,
        p,
        m,
        v,
        b = L.length,
        E = !0;
      for (i = 0; i < b; ++i)
        if (o = L[i]) {
          for (a = o.site, c = o.halfedges, s = c.length; s--;) F[c[s]] || c.splice(s, 1);
          for (s = 0, u = c.length; s < u;) p = y(o, F[c[s]]), m = p[0], v = p[1], d = g(o, F[c[++s % u]]),
            f = d[0], h = d[1], (Math.abs(m - f) > B || Math.abs(v - h) > B) && (c.splice(s, 0, F.push(l(a,
              p, Math.abs(m - e) < B && r - v > B ? [e, Math.abs(f - e) < B ? h : r] : Math.abs(v -
              r) < B && n - m > B ? [Math.abs(h - r) < B ? f : n, r] : Math.abs(m - n) < B && v - t >
              B ? [n, Math.abs(f - n) < B ? h : t] : Math.abs(v - t) < B && m - e > B ? [Math.abs(h -
                t) < B ? f : e, t] : null)) - 1), ++u);
          u && (E = !1);
        }
      if (E) {
        var _,
          $,
          w,
          T = 1 / 0;
        for (i = 0, E = null; i < b; ++i)(o = L[i]) && (a = o.site, _ = a[0] - e, $ = a[1] - t, w = _ * _ +
          $ * $, w < T && (T = w, E = o));
        if (E) {
          var C = [e, t],
            x = [e, r],
            S = [n, r],
            A = [n, t];
          E.halfedges.push(F.push(l(a = E.site, C, x)) - 1, F.push(l(a, x, S)) - 1, F.push(l(a, S, A)) - 1,
            F.push(l(a, A, C)) - 1);
        }
      }
      for (i = 0; i < b; ++i)(o = L[i]) && (o.halfedges.length || delete L[i]);
    }

    function _() {
      o(this), this.x = this.y = this.arc = this.site = this.cy = null;
    }

    function $(e) {
      var t = e.P,
        n = e.N;
      if (t && n) {
        var r = t.site,
          i = e.site,
          o = n.site;
        if (r !== o) {
          var a = i[0],
            s = i[1],
            c = r[0] - a,
            u = r[1] - s,
            l = o[0] - a,
            d = o[1] - s,
            f = 2 * (c * d - u * l);
          if (!(f >= -z)) {
            var h = c * c + u * u,
              p = l * l + d * d,
              m = (d * h - u * p) / f,
              v = (c * p - l * h) / f,
              g = j.pop() || new _();
            g.arc = e, g.site = i, g.x = m + a, g.y = (g.cy = v + s) + Math.sqrt(m * m + v * v), e.circle =
              g;
            for (var y = null, b = U._; b;)
              if (g.y < b.y || g.y === b.y && g.x <= b.x) {
                if (!b.L) {
                  y = b.P;
                  break;
                }
                b = b.L;
              } else {
                if (!b.R) {
                  y = b;
                  break;
                }
                b = b.R;
              }
            U.insert(y, g), y || (R = g);
          }
        }
      }
    }

    function w(e) {
      var t = e.circle;
      t && (t.P || (R = t.N), U.remove(t), j.push(t), o(t), e.circle = null);
    }

    function T() {
      o(this), this.edge = this.site = this.circle = null;
    }

    function C(e) {
      var t = H.pop() || new T();
      return t.site = e, t;
    }

    function x(e) {
      w(e), P.remove(e), H.push(e), o(e);
    }

    function S(e) {
      var t = e.circle,
        n = t.x,
        r = t.cy,
        i = [n, r],
        o = e.P,
        a = e.N,
        s = [e];
      x(e);
      for (var c = o; c.circle && Math.abs(n - c.circle.x) < B && Math.abs(r - c.circle.cy) < B;) o = c.P, s
        .unshift(c), x(c), c = o;
      s.unshift(c), w(c);
      for (var l = a; l.circle && Math.abs(n - l.circle.x) < B && Math.abs(r - l.circle.cy) < B;) a = l.N, s
        .push(l), x(l), l = a;
      s.push(l), w(l);
      var f,
        h = s.length;
      for (f = 1; f < h; ++f) l = s[f], c = s[f - 1], d(l.edge, c.site, l.site, i);
      c = s[0], l = s[h - 1], l.edge = u(c.site, l.site, null, i), $(c), $(l);
    }

    function A(e) {
      for (var t, n, r, i, o = e[0], a = e[1], s = P._; s;)
        if (r = M(s, a) - o, r > B) s = s.L;
        else {
          if (i = o - k(s, a), !(i > B)) {
            r > -B ? (t = s.P, n = s) : i > -B ? (t = s, n = s.N) : t = n = s;
            break;
          }
          if (!s.R) {
            t = s;
            break;
          }
          s = s.R;
        }
      m(e);
      var c = C(e);
      if (P.insert(t, c), t || n) {
        if (t === n) return w(t), n = C(t.site), P.insert(c, n), c.edge = n.edge = u(t.site, c.site), $(t),
          void $(n);
        if (!n) return void(c.edge = u(t.site, c.site));
        w(t), w(n);
        var l = t.site,
          f = l[0],
          h = l[1],
          p = e[0] - f,
          v = e[1] - h,
          g = n.site,
          y = g[0] - f,
          b = g[1] - h,
          E = 2 * (p * b - v * y),
          _ = p * p + v * v,
          T = y * y + b * b,
          x = [(b * _ - v * T) / E + f, (p * T - y * _) / E + h];
        d(n.edge, l, g, x), c.edge = u(l, e, null, x), n.edge = u(e, g, null, x), $(t), $(n);
      }
    }

    function M(e, t) {
      var n = e.site,
        r = n[0],
        i = n[1],
        o = i - t;
      if (!o) return r;
      var a = e.P;
      if (!a) return -(1 / 0);
      n = a.site;
      var s = n[0],
        c = n[1],
        u = c - t;
      if (!u) return s;
      var l = s - r,
        d = 1 / o - 1 / u,
        f = l / u;
      return d ? (-f + Math.sqrt(f * f - 2 * d * (l * l / (-2 * u) - c + u / 2 + i - o / 2))) / d + r : (r +
        s) / 2;
    }

    function k(e, t) {
      var n = e.N;
      if (n) return M(n, t);
      var r = e.site;
      return r[1] === t ? r[0] : 1 / 0;
    }

    function N(e, t, n) {
      return (e[0] - n[0]) * (t[1] - e[1]) - (e[0] - t[0]) * (n[1] - e[1]);
    }

    function I(e, t) {
      return t[1] - e[1] || t[0] - e[0];
    }

    function O(e, t) {
      var n,
        r,
        o,
        a = e.sort(I).pop();
      for (F = [], L = new Array(e.length), P = new i(), U = new i();;)
        if (o = R, a && (!o || a[1] < o.y || a[1] === o.y && a[0] < o.x)) a[0] === n && a[1] === r || (A(a),
          n = a[0], r = a[1]), a = e.pop();
        else {
          if (!o) break;
          S(o.arc);
        }
      if (b(), t) {
        var s = +t[0][0],
          c = +t[0][1],
          u = +t[1][0],
          l = +t[1][1];
        p(s, c, u, l), E(s, c, u, l);
      }
      this.edges = F, this.cells = L, P = U = F = L = null;
    }

    function D() {
      function e(e) {
        return new O(e.map(function(t, n) {
          var r = [Math.round(i(t, n, e) / B) * B, Math.round(o(t, n, e) / B) * B];
          return r.index = n, r.data = t, r;
        }), a);
      }
      var i = n,
        o = r,
        a = null;
      return e.polygons = function(t) {
        return e(t).polygons();
      }, e.links = function(t) {
        return e(t).links();
      }, e.triangles = function(t) {
        return e(t).triangles();
      }, e.x = function(n) {
        return arguments.length ? (i = "function" == typeof n ? n : t(+n), e) : i;
      }, e.y = function(n) {
        return arguments.length ? (o = "function" == typeof n ? n : t(+n), e) : o;
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
    i.prototype = {
      constructor: i,
      insert: function(e, t) {
        var n, r, i;
        if (e) {
          if (t.P = e, t.N = e.N, e.N && (e.N.P = t), e.N = t, e.R) {
            for (e = e.R; e.L;) e = e.L;
            e.L = t;
          } else e.R = t;
          n = e;
        } else this._ ? (e = c(this._), t.P = null, t.N = e, e.P = e.L = t, n = e) : (t.P = t.N = null,
          this._ = t, n = null);
        for (t.L = t.R = null, t.U = n, t.C = !0, e = t; n && n.C;) r = n.U, n === r.L ? (i = r.R, i &&
          i.C ? (n.C = i.C = !1, r.C = !0, e = r) : (e === n.R && (a(this, n), e = n, n = e.U), n
            .C = !1, r.C = !0, s(this, r))) : (i = r.L, i && i.C ? (n.C = i.C = !1, r.C = !0, e = r) :
          (e === n.L && (s(this, n), e = n, n = e.U), n.C = !1, r.C = !0, a(this, r))), n = e.U;
        this._.C = !1;
      },
      remove: function(e) {
        e.N && (e.N.P = e.P), e.P && (e.P.N = e.N), e.N = e.P = null;
        var t,
          n,
          r,
          i = e.U,
          o = e.L,
          u = e.R;
        if (n = o ? u ? c(u) : o : u, i ? i.L === e ? i.L = n : i.R = n : this._ = n, o && u ? (r = n.C,
            n.C = e.C, n.L = o, o.U = n, n !== u ? (i = n.U, n.U = e.U, e = n.R, i.L = e, n.R = u, u.U =
              n) : (n.U = i, i = n, e = n.R)) : (r = e.C, e = n), e && (e.U = i), !r) {
          if (e && e.C) return void(e.C = !1);
          do {
            if (e === this._) break;
            if (e === i.L) {
              if (t = i.R, t.C && (t.C = !1, i.C = !0, a(this, i), t = i.R), t.L && t.L.C || t.R && t.R
                .C) {
                t.R && t.R.C || (t.L.C = !1, t.C = !0, s(this, t), t = i.R), t.C = i.C, i.C = t.R.C = !
                  1, a(this, i), e = this._;
                break;
              }
            } else if (t = i.L, t.C && (t.C = !1, i.C = !0, s(this, i), t = i.L), t.L && t.L.C || t.R &&
              t.R.C) {
              t.L && t.L.C || (t.R.C = !1, t.C = !0, a(this, t), t = i.L), t.C = i.C, i.C = t.L.C = !1,
                s(this, i), e = this._;
              break;
            }
            t.C = !0, e = i, i = i.U;
          } while (!e.C);
          e && (e.C = !1);
        }
      }
    };
    var R,
      P,
      L,
      U,
      F,
      j = [],
      H = [],
      B = 1e-6,
      z = 1e-12;
    O.prototype = {
      constructor: O,
      polygons: function() {
        var e = this.edges;
        return this.cells.map(function(t) {
          var n = t.halfedges.map(function(n) {
            return g(t, e[n]);
          });
          return n.data = t.site.data, n;
        });
      },
      triangles: function() {
        var e = [],
          t = this.edges;
        return this.cells.forEach(function(n, r) {
          if (o = (i = n.halfedges).length)
            for (var i, o, a, s = n.site, c = -1, u = t[i[o - 1]], l = u.left === s ? u.right : u
                .left; ++c < o;) a = l, u = t[i[c]], l = u.left === s ? u.right : u.left, a && l &&
              r < a.index && r < l.index && N(s, a, l) < 0 && e.push([s.data, a.data, l.data]);
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
        for (var r, i, o = this, a = o._found || 0, s = o.cells.length; !(i = o.cells[a]);)
          if (++a >= s) return null;
        var c = e - i.site[0],
          u = t - i.site[1],
          l = c * c + u * u;
        do i = o.cells[r = a], a = null, i.halfedges.forEach(function(n) {
          var r = o.edges[n],
            s = r.left;
          if (s !== i.site && s || (s = r.right)) {
            var c = e - s[0],
              u = t - s[1],
              d = c * c + u * u;
            d < l && (l = d, a = s.index);
          }
        }); while (null !== a);
        return o._found = r, null == n || l <= n * n ? i.site : null;
      }
    }, e.voronoi = D, Object.defineProperty(e, "__esModule", {
      value: !0
    });
  });
}
