// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 8
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(4),
    i = n(2),
    o = n(36),
    a = n(11),
    s = n(10),
    c = "prototype",
    u = function(e, t, n) {
      var l, d, f, h = e & u.F,
        p = e & u.G,
        m = e & u.S,
        v = e & u.P,
        g = e & u.B,
        y = e & u.W,
        b = p ? i : i[t] || (i[t] = {}),
        E = b[c],
        _ = p ? r : m ? r[t] : (r[t] || {})[c];
      p && (n = t);
      for (l in n) d = !h && _ && void 0 !== _[l], d && s(b, l) || (f = d ? _[l] : n[l], b[l] = p && "function" !=
        typeof _[l] ? n[l] : g && d ? o(f, r) : y && _[l] == f ? function(e) {
          var t = function(t, n, r) {
            if (this instanceof e) {
              switch (arguments.length) {
                case 0:
                  return new e;
                case 1:
                  return new e(t);
                case 2:
                  return new e(t, n)
              }
              return new e(t, n, r)
            }
            return e.apply(this, arguments)
          };
          return t[c] = e[c], t
        }(f) : v && "function" == typeof f ? o(Function.call, f) : f, v && ((b.virtual || (b.virtual = {}))[l] = f,
          e & u.R && E && !E[l] && a(E, l, f)))
    };
  u.F = 1, u.G = 2, u.S = 4, u.P = 8, u.B = 16, u.W = 32, u.U = 64, u.R = 128, e.exports = u
}
