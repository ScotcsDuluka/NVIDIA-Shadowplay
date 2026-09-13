// ─────────────────────────────────────────────────────────────
// APP MODULE 413
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  "use strict";
  var i = n(36),
    o = n(15),
    r = n(31),
    a = n(118),
    l = n(116),
    s = n(56),
    d = n(398),
    c = n(79);
  o(o.S + o.F * !n(402)(function(e) {
    Array.from(e)
  }), "Array", {
    from: function(e) {
      var t, n, o, u, f = r(e),
        m = "function" == typeof this ? this : Array,
        g = arguments.length,
        p = g > 1 ? arguments[1] : void 0,
        h = void 0 !== p,
        b = 0,
        x = c(f);
      if (h && (p = i(p, g > 2 ? arguments[2] : void 0, 2)), void 0 == x || m == Array && l(x))
        for (t = s(f.length), n = new m(t); t > b; b++) d(n, b, h ? p(f[b], b) : f[b]);
      else
        for (u = x.call(f), n = new m; !(o = u.next()).done; b++) d(n, b, h ? a(u, p, [o.value, b], !0) : o
        .value);
      return n.length = b, n
    }
  })
}
