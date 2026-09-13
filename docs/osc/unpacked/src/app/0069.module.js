// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 69
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(52),
    o = require(15),
    r = require(124),
    a = require(27),
    l = require(41),
    s = require(401),
    d = require(55),
    c = require(121),
    u = require(16)("iterator"),
    f = !([].keys && "next" in [].keys()),
    m = "@@iterator",
    g = "keys",
    p = "values",
    h = function() {
      return this;
    };
  module.exports = function(e, t, n, b, x, v, y) {
    s(n, t, b);
    var w,
      S,
      E,
      k = function(e) {
        if (!f && e in O) return O[e];
        switch (e) {
          case g:
            return function() {
              return new n(this, e);
            };
          case p:
            return function() {
              return new n(this, e);
            };
        }
        return function() {
          return new n(this, e);
        };
      },
      _ = t + " Iterator",
      T = x == p,
      C = !1,
      O = e.prototype,
      A = O[u] || O[m] || x && O[x],
      I = A || k(x),
      M = x ? T ? k("entries") : I : void 0,
      R = "Array" == t ? O.entries || A : A;
    if (R && (E = c(R.call(new e())), E !== Object.prototype && E.next && (d(E, _, !0), i || "function" ==
        typeof E[u] || a(E, u, h))), T && A && A.name !== p && (C = !0, I = function() {
        return A.call(this);
      }), i && !y || !f && !C && O[u] || a(O, u, I), l[t] = I, l[_] = h, x)
      if (w = {
          values: T ? I : k(p),
          keys: v ? I : k(g),
          entries: M
        }, y)
        for (S in w) S in O || r(O, S, w[S]);
      else o(o.P + o.F * (f || C), t, w);
    return w;
  };
}
