// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 133
// constant $base64 | defines angular.module("base64")
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  ! function() {
    "use strict";
    angular.module("base64", []).constant("$base64", function() {
      function e(e, t) {
        var n = o.indexOf(e.charAt(t));
        if (n == -1) throw "Cannot decode base64";
        return n;
      }

      function t(t) {
        t = "" + t;
        var n,
          r,
          o,
          a = t.length;
        if (0 == a) return t;
        if (a % 4 != 0) throw "Cannot decode base64";
        n = 0, t.charAt(a - 1) == i && (n = 1, t.charAt(a - 2) == i && (n = 2), a -= 4);
        var s = [];
        for (r = 0; r < a; r += 4) o = e(t, r) << 18 | e(t, r + 1) << 12 | e(t, r + 2) << 6 | e(t, r + 3),
          s.push(String.fromCharCode(o >> 16, o >> 8 & 255, 255 & o));
        switch (n) {
          case 1:
            o = e(t, r) << 18 | e(t, r + 1) << 12 | e(t, r + 2) << 6, s.push(String.fromCharCode(o >> 16,
              o >> 8 & 255));
            break;
          case 2:
            o = e(t, r) << 18 | e(t, r + 1) << 12, s.push(String.fromCharCode(o >> 16));
        }
        return s.join("");
      }

      function n(e, t) {
        var n = e.charCodeAt(t);
        if (n > 255) throw "INVALID_CHARACTER_ERR: DOM Exception 5";
        return n;
      }

      function r(e) {
        if (1 != arguments.length) throw "SyntaxError: Not enough arguments";
        var t,
          r,
          a = [];
        e = "" + e;
        var s = e.length - e.length % 3;
        if (0 == e.length) return e;
        for (t = 0; t < s; t += 3) r = n(e, t) << 16 | n(e, t + 1) << 8 | n(e, t + 2), a.push(o.charAt(
          r >> 18)), a.push(o.charAt(r >> 12 & 63)), a.push(o.charAt(r >> 6 & 63)), a.push(o.charAt(63 &
          r));
        switch (e.length - s) {
          case 1:
            r = n(e, t) << 16, a.push(o.charAt(r >> 18) + o.charAt(r >> 12 & 63) + i + i);
            break;
          case 2:
            r = n(e, t) << 16 | n(e, t + 1) << 8, a.push(o.charAt(r >> 18) + o.charAt(r >> 12 & 63) + o
              .charAt(r >> 6 & 63) + i);
        }
        return a.join("");
      }
      var i = "=",
        o = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
      return {
        encode: r,
        decode: t
      };
    }());
  }();
}
