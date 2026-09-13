// ─────────────────────────────────────────────────────────────
// APP MODULE 19
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(28),
    o = n(115),
    r = n(76),
    a = Object.defineProperty;
  t.f = n(18) ? Object.defineProperty : function(e, t, n) {
    if (i(e), t = r(t, !0), i(n), o) try {
      return a(e, t, n)
    } catch (e) {}
    if ("get" in n || "set" in n) throw TypeError("Accessors not supported!");
    return "value" in n && (e[t] = n.value), e
  }
}
