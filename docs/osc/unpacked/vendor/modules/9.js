// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 9
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(7),
    i = n(81),
    o = n(57),
    a = Object.defineProperty;
  t.f = n(6) ? Object.defineProperty : function(e, t, n) {
    if (r(e), t = o(t, !0), r(n), i) try {
      return a(e, t, n)
    } catch (e) {}
    if ("get" in n || "set" in n) throw TypeError("Accessors not supported!");
    return "value" in n && (e[t] = n.value), e
  }
}
