// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 248
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  ! function(e, n) {
    n(exports);
  }(this, function(e) {
    "use strict";

    function t(e) {
      for (var t, n = -1, r = e.length, i = e[r - 1], o = 0; ++n < r;) t = i, i = e[n], o += t[1] * i[0] -
        t[0] * i[1];
      return o / 2;
    }

    function n(e) {
      for (var t, n, r = -1, i = e.length, o = 0, a = 0, s = e[i - 1], c = 0; ++r < i;) t = s, s = e[r],
        c += n = t[0] * s[1] - s[0] * t[1], o += (t[0] + s[0]) * n, a += (t[1] + s[1]) * n;
      return c *= 3, [o / c, a / c];
    }

    function r(e, t, n) {
      return (t[0] - e[0]) * (n[1] - e[1]) - (t[1] - e[1]) * (n[0] - e[0]);
    }

    function i(e, t) {
      return e[0] - t[0] || e[1] - t[1];
    }

    function o(e) {
      for (var t = e.length, n = [0, 1], i = 2, o = 2; o < t; ++o) {
        for (; i > 1 && r(e[n[i - 2]], e[n[i - 1]], e[o]) <= 0;) --i;
        n[i++] = o;
      }
      return n.slice(0, i);
    }

    function a(e) {
      if ((n = e.length) < 3) return null;
      var t,
        n,
        r = new Array(n),
        a = new Array(n);
      for (t = 0; t < n; ++t) r[t] = [+e[t][0], +e[t][1], t];
      for (r.sort(i), t = 0; t < n; ++t) a[t] = [r[t][0], -r[t][1]];
      var s = o(r),
        c = o(a),
        u = c[0] === s[0],
        l = c[c.length - 1] === s[s.length - 1],
        d = [];
      for (t = s.length - 1; t >= 0; --t) d.push(e[r[s[t]][2]]);
      for (t = +u; t < c.length - l; ++t) d.push(e[r[c[t]][2]]);
      return d;
    }

    function s(e, t) {
      for (var n, r, i = e.length, o = e[i - 1], a = t[0], s = t[1], c = o[0], u = o[1], l = !1, d = 0; d <
        i; ++d) o = e[d], n = o[0], r = o[1], r > s != u > s && a < (c - n) * (s - r) / (u - r) + n && (
        l = !l), c = n, u = r;
      return l;
    }

    function c(e) {
      for (var t, n, r = -1, i = e.length, o = e[i - 1], a = o[0], s = o[1], c = 0; ++r < i;) t = a, n = s,
        o = e[r], a = o[0], s = o[1], t -= a, n -= s, c += Math.sqrt(t * t + n * n);
      return c;
    }
    e.polygonArea = t, e.polygonCentroid = n, e.polygonContains = s, e.polygonHull = a, e.polygonLength = c,
      Object.defineProperty(e, "__esModule", {
        value: !0
      });
  });
}
