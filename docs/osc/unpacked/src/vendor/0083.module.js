// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 83
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var r = require(28),
    i = require(8),
    o = require(91),
    a = require(11),
    s = require(16),
    c = require(187),
    u = require(38),
    l = require(195),
    d = require(5)("iterator"),
    f = !([].keys && "next" in [].keys()),
    h = "@@iterator",
    p = "keys",
    m = "values",
    v = function() {
      return this;
    };
  module.exports = function(e, t, n, g, y, b, E) {
    c(n, t, g);
    var _,
      $,
      w,
      T = function(e) {
        if (!f && e in A) return A[e];
        switch (e) {
          case p:
            return function() {
              return new n(this, e);
            };
          case m:
            return function() {
              return new n(this, e);
            };
        }
        return function() {
          return new n(this, e);
        };
      },
      C = t + " Iterator",
      x = y == m,
      S = !1,
      A = e.prototype,
      M = A[d] || A[h] || y && A[y],
      k = M || T(y),
      N = y ? x ? T("entries") : k : void 0,
      I = "Array" == t ? A.entries || M : M;
    if (I && (w = l(I.call(new e())), w !== Object.prototype && w.next && (u(w, C, !0), r || "function" ==
        typeof w[d] || a(w, d, v))), x && M && M.name !== m && (S = !0, k = function() {
        return M.call(this);
      }), r && !E || !f && !S && A[d] || a(A, d, k), s[t] = k, s[C] = v, y)
      if (_ = {
          values: x ? k : T(m),
          keys: b ? k : T(p),
          entries: N
        }, E)
        for ($ in _) $ in A || o(A, $, _[$]);
      else i(i.P + i.F * (f || S), t, _);
    return _;
  };
}
