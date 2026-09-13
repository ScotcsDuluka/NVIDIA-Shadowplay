// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 53
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(57)("meta"),
    o = require(24),
    r = require(30),
    a = require(19).f,
    l = 0,
    s = Object.isExtensible || function() {
      return !0;
    },
    d = !require(29)(function() {
      return s(Object.preventExtensions({}));
    }),
    c = function(e) {
      a(e, i, {
        value: {
          i: "O" + ++l,
          w: {}
        }
      });
    },
    u = function(e, t) {
      if (!o(e)) return "symbol" == typeof e ? e : ("string" == typeof e ? "S" : "P") + e;
      if (!r(e, i)) {
        if (!s(e)) return "F";
        if (!t) return "E";
        c(e);
      }
      return e[i].i;
    },
    f = function(e, t) {
      if (!r(e, i)) {
        if (!s(e)) return !0;
        if (!t) return !1;
        c(e);
      }
      return e[i].w;
    },
    m = function(e) {
      return d && g.NEED && s(e) && !r(e, i) && c(e), e;
    },
    g = module.exports = {
      KEY: i,
      NEED: !1,
      fastKey: u,
      getWeak: f,
      onFreeze: m
    };
}
