// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 190
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(40)("meta"),
    i = n(12),
    o = n(10),
    a = n(9).f,
    s = 0,
    c = Object.isExtensible || function() {
      return !0
    },
    u = !n(15)(function() {
      return c(Object.preventExtensions({}))
    }),
    l = function(e) {
      a(e, r, {
        value: {
          i: "O" + ++s,
          w: {}
        }
      })
    },
    d = function(e, t) {
      if (!i(e)) return "symbol" == typeof e ? e : ("string" == typeof e ? "S" : "P") + e;
      if (!o(e, r)) {
        if (!c(e)) return "F";
        if (!t) return "E";
        l(e)
      }
      return e[r].i
    },
    f = function(e, t) {
      if (!o(e, r)) {
        if (!c(e)) return !0;
        if (!t) return !1;
        l(e)
      }
      return e[r].w
    },
    h = function(e) {
      return u && p.NEED && c(e) && !o(e, r) && l(e), e
    },
    p = e.exports = {
      KEY: r,
      NEED: !1,
      fastKey: d,
      getWeak: f,
      onFreeze: h
    }
}
