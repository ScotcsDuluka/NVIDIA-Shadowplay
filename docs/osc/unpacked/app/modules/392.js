// ─────────────────────────────────────────────────────────────
// APP MODULE 392
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(36),
    o = n(68),
    r = n(31),
    a = n(56),
    l = n(394);
  e.exports = function(e, t) {
    var n = 1 == e,
      s = 2 == e,
      d = 3 == e,
      c = 4 == e,
      u = 6 == e,
      f = 5 == e || u,
      m = t || l;
    return function(t, l, g) {
      for (var p, h, b = r(t), x = o(b), v = i(l, g, 3), y = a(x.length), w = 0, S = n ? m(t, y) : s ? m(t, 0) :
          void 0; y > w; w++)
        if ((f || w in x) && (p = x[w], h = v(p, w, b), e))
          if (n) S[w] = h;
          else if (h) switch (e) {
        case 3:
          return !0;
        case 5:
          return p;
        case 6:
          return w;
        case 2:
          S.push(p)
      } else if (c) return !1;
      return u ? -1 : d || c ? c : S
    }
  }
}
