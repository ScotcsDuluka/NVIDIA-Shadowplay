// ─────────────────────────────────────────────────────────────
// APP MODULE 15
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(21),
    o = n(13),
    r = n(36),
    a = n(27),
    l = n(30),
    s = "prototype",
    d = function(e, t, n) {
      var c, u, f, m = e & d.F,
        g = e & d.G,
        p = e & d.S,
        h = e & d.P,
        b = e & d.B,
        x = e & d.W,
        v = g ? o : o[t] || (o[t] = {}),
        y = v[s],
        w = g ? i : p ? i[t] : (i[t] || {})[s];
      g && (n = t);
      for (c in n) u = !m && w && void 0 !== w[c], u && l(v, c) || (f = u ? w[c] : n[c], v[c] = g && "function" !=
        typeof w[c] ? n[c] : b && u ? r(f, i) : x && w[c] == f ? function(e) {
          var t = function(t, n, i) {
            if (this instanceof e) {
              switch (arguments.length) {
                case 0:
                  return new e;
                case 1:
                  return new e(t);
                case 2:
                  return new e(t, n)
              }
              return new e(t, n, i)
            }
            return e.apply(this, arguments)
          };
          return t[s] = e[s], t
        }(f) : h && "function" == typeof f ? r(Function.call, f) : f, h && ((v.virtual || (v.virtual = {}))[c] = f,
          e & d.R && y && !y[c] && a(y, c, f)))
    };
  d.F = 1, d.G = 2, d.S = 4, d.P = 8, d.B = 16, d.W = 32, d.U = 64, d.R = 128, e.exports = d
}
