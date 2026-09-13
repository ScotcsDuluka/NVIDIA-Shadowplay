// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 288
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r;
  (function(e, i) {
    ! function(o) {
      function a(e) {
        for (var t, n, r = [], i = 0, o = e.length; i < o;) t = e.charCodeAt(i++), t >= 55296 && t <= 56319 && i < o ?
          (n = e.charCodeAt(i++), 56320 == (64512 & n) ? r.push(((1023 & t) << 10) + (1023 & n) + 65536) : (r.push(t),
            i--)) : r.push(t);
        return r
      }

      function s(e) {
        for (var t, n = e.length, r = -1, i = ""; ++r < n;) t = e[r], t > 65535 && (t -= 65536, i += E(t >>> 10 &
          1023 | 55296), t = 56320 | 1023 & t), i += E(t);
        return i
      }

      function c(e) {
        if (e >= 55296 && e <= 57343) throw Error("Lone surrogate U+" + e.toString(16).toUpperCase() +
          " is not a scalar value")
      }

      function u(e, t) {
        return E(e >> t & 63 | 128)
      }

      function l(e) {
        if (0 == (4294967168 & e)) return E(e);
        var t = "";
        return 0 == (4294965248 & e) ? t = E(e >> 6 & 31 | 192) : 0 == (4294901760 & e) ? (c(e), t = E(e >> 12 & 15 |
            224), t += u(e, 6)) : 0 == (4292870144 & e) && (t = E(e >> 18 & 7 | 240), t += u(e, 12), t += u(e, 6)),
          t += E(63 & e | 128)
      }

      function d(e) {
        for (var t, n = a(e), r = n.length, i = -1, o = ""; ++i < r;) t = n[i], o += l(t);
        return o
      }

      function f() {
        if (b >= y) throw Error("Invalid byte index");
        var e = 255 & g[b];
        if (b++, 128 == (192 & e)) return 63 & e;
        throw Error("Invalid continuation byte")
      }

      function h() {
        var e, t, n, r, i;
        if (b > y) throw Error("Invalid byte index");
        if (b == y) return !1;
        if (e = 255 & g[b], b++, 0 == (128 & e)) return e;
        if (192 == (224 & e)) {
          var t = f();
          if (i = (31 & e) << 6 | t, i >= 128) return i;
          throw Error("Invalid continuation byte")
        }
        if (224 == (240 & e)) {
          if (t = f(), n = f(), i = (15 & e) << 12 | t << 6 | n, i >= 2048) return c(i), i;
          throw Error("Invalid continuation byte")
        }
        if (240 == (248 & e) && (t = f(), n = f(), r = f(), i = (15 & e) << 18 | t << 12 | n << 6 | r, i >= 65536 &&
            i <= 1114111)) return i;
        throw Error("Invalid UTF-8 detected")
      }

      function p(e) {
        g = a(e), y = g.length, b = 0;
        for (var t, n = [];
          (t = h()) !== !1;) n.push(t);
        return s(n)
      }
      var m = "object" == typeof t && t,
        v = ("object" == typeof e && e && e.exports == m && e, "object" == typeof i && i);
      v.global !== v && v.window !== v || (o = v);
      var g, y, b, E = String.fromCharCode,
        _ = {
          version: "2.0.0",
          encode: d,
          decode: p
        };
      r = function() {
        return _
      }.call(t, n, t, e), !(void 0 !== r && (e.exports = r))
    }(this)
  }).call(t, n(289)(e), function() {
    return this
  }())
}
