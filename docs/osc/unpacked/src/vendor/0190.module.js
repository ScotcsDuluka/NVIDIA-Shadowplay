// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 190
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(40)("meta"),
    i = require(12),
    o = require(10),
    a = require(9).f,
    s = 0,
    c = Object.isExtensible || function() {
      return !0;
    },
    u = !require(15)(function() {
      return c(Object.preventExtensions({}));
    }),
    l = function(e) {
      a(e, r, {
        value: {
          i: "O" + ++s,
          w: {}
        }
      });
    },
    d = function(e, t) {
      if (!i(e)) return "symbol" == typeof e ? e : ("string" == typeof e ? "S" : "P") + e;
      if (!o(e, r)) {
        if (!c(e)) return "F";
        if (!t) return "E";
        l(e);
      }
      return e[r].i;
    },
    f = function(e, t) {
      if (!o(e, r)) {
        if (!c(e)) return !0;
        if (!t) return !1;
        l(e);
      }
      return e[r].w;
    },
    h = function(e) {
      return u && p.NEED && c(e) && !o(e, r) && l(e), e;
    },
    p = module.exports = {
      KEY: r,
      NEED: !1,
      fastKey: d,
      getWeak: f,
      onFreeze: h
    };
}
