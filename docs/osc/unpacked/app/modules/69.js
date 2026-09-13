// ─────────────────────────────────────────────────────────────
// APP MODULE 69
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(52),
    o = n(15),
    r = n(124),
    a = n(27),
    l = n(41),
    s = n(401),
    d = n(55),
    c = n(121),
    u = n(16)("iterator"),
    f = !([].keys && "next" in [].keys()),
    m = "@@iterator",
    g = "keys",
    p = "values",
    h = function() {
      return this
    };
  e.exports = function(e, t, n, b, x, v, y) {
    s(n, t, b);
    var w, S, E, k = function(e) {
        if (!f && e in O) return O[e];
        switch (e) {
          case g:
            return function() {
              return new n(this, e)
            };
          case p:
            return function() {
              return new n(this, e)
            }
        }
        return function() {
          return new n(this, e)
        }
      },
      _ = t + " Iterator",
      T = x == p,
      C = !1,
      O = e.prototype,
      A = O[u] || O[m] || x && O[x],
      I = A || k(x),
      M = x ? T ? k("entries") : I : void 0,
      R = "Array" == t ? O.entries || A : A;
    if (R && (E = c(R.call(new e)), E !== Object.prototype && E.next && (d(E, _, !0), i || "function" == typeof E[
        u] || a(E, u, h))), T && A && A.name !== p && (C = !0, I = function() {
        return A.call(this)
      }), i && !y || !f && !C && O[u] || a(O, u, I), l[t] = I, l[_] = h, x)
      if (w = {
          values: T ? I : k(p),
          keys: v ? I : k(g),
          entries: M
        }, y)
        for (S in w) S in O || r(O, S, w[S]);
      else o(o.P + o.F * (f || C), t, w);
    return w
  }
}
