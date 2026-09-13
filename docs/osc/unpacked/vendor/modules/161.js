// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 161
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";

  function r(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  t.__esModule = !0;
  var i = n(152),
    o = r(i),
    a = n(151),
    s = r(a);
  t.default = function() {
    function e(e, t) {
      var n = [],
        r = !0,
        i = !1,
        o = void 0;
      try {
        for (var a, c = (0, s.default)(e); !(r = (a = c.next()).done) && (n.push(a.value), !t || n.length !==
          t); r = !0);
      } catch (e) {
        i = !0, o = e
      } finally {
        try {
          !r && c.return && c.return()
        } finally {
          if (i) throw o
        }
      }
      return n
    }
    return function(t, n) {
      if (Array.isArray(t)) return t;
      if ((0, o.default)(Object(t))) return e(t, n);
      throw new TypeError("Invalid attempt to destructure non-iterable instance")
    }
  }()
}
