// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 437
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e) {
      for (var t, n = -1, i = e.length, o = e[i - 1], r = 0; ++n < i;) t = o, o = e[n], r += t[1] * o[0] -
        t[0] * o[1];
      return r / 2;
    }

    function n(e) {
      for (var t, n, i = -1, o = e.length, r = 0, a = 0, l = e[o - 1], s = 0; ++i < o;) t = l, l = e[i],
        s += n = t[0] * l[1] - l[0] * t[1], r += (t[0] + l[0]) * n, a += (t[1] + l[1]) * n;
      return s *= 3, [r / s, a / s];
    }

    function i(e, t, n) {
      return (t[0] - e[0]) * (n[1] - e[1]) - (t[1] - e[1]) * (n[0] - e[0]);
    }

    function o(e, t) {
      return e[0] - t[0] || e[1] - t[1];
    }

    function r(e) {
      for (var t = e.length, n = [0, 1], o = 2, r = 2; r < t; ++r) {
        for (; o > 1 && i(e[n[o - 2]], e[n[o - 1]], e[r]) <= 0;) --o;
        n[o++] = r;
      }
      return n.slice(0, o);
    }

    function a(e) {
      if ((n = e.length) < 3) return null;
      var t,
        n,
        i = new Array(n),
        a = new Array(n);
      for (t = 0; t < n; ++t) i[t] = [+e[t][0], +e[t][1], t];
      for (i.sort(o), t = 0; t < n; ++t) a[t] = [i[t][0], -i[t][1]];
      var l = r(i),
        s = r(a),
        d = s[0] === l[0],
        c = s[s.length - 1] === l[l.length - 1],
        u = [];
      for (t = l.length - 1; t >= 0; --t) u.push(e[i[l[t]][2]]);
      for (t = +d; t < s.length - c; ++t) u.push(e[i[s[t]][2]]);
      return u;
    }

    function l(e, t) {
      for (var n, i, o = e.length, r = e[o - 1], a = t[0], l = t[1], s = r[0], d = r[1], c = !1, u = 0; u <
        o; ++u) r = e[u], n = r[0], i = r[1], i > l != d > l && a < (s - n) * (l - i) / (d - i) + n && (
        c = !c), s = n, d = i;
      return c;
    }

    function s(e) {
      for (var t, n, i = -1, o = e.length, r = e[o - 1], a = r[0], l = r[1], s = 0; ++i < o;) t = a, n = l,
        r = e[i], a = r[0], l = r[1], t -= a, n -= l, s += Math.sqrt(t * t + n * n);
      return s;
    }
    e.polygonArea = t, e.polygonCentroid = n, e.polygonContains = l, e.polygonHull = a, e.polygonLength = s,
      Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
