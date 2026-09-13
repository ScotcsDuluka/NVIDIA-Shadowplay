// ─────────────────────────────────────────────────────────────
// APP MODULE 53
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(57)("meta"),
    o = n(24),
    r = n(30),
    a = n(19).f,
    l = 0,
    s = Object.isExtensible || function() {
      return !0
    },
    d = !n(29)(function() {
      return s(Object.preventExtensions({}))
    }),
    c = function(e) {
      a(e, i, {
        value: {
          i: "O" + ++l,
          w: {}
        }
      })
    },
    u = function(e, t) {
      if (!o(e)) return "symbol" == typeof e ? e : ("string" == typeof e ? "S" : "P") + e;
      if (!r(e, i)) {
        if (!s(e)) return "F";
        if (!t) return "E";
        c(e)
      }
      return e[i].i
    },
    f = function(e, t) {
      if (!r(e, i)) {
        if (!s(e)) return !0;
        if (!t) return !1;
        c(e)
      }
      return e[i].w
    },
    m = function(e) {
      return d && g.NEED && s(e) && !r(e, i) && c(e), e
    },
    g = e.exports = {
      KEY: i,
      NEED: !1,
      fastKey: u,
      getWeak: f,
      onFreeze: m
    }
}
