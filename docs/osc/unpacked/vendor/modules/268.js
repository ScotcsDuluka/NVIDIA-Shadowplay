// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 268
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  function n(e, t, n) {
    function i(e, r) {
      if (i.count <= 0) throw new Error("after called too many times");
      --i.count, e ? (o = !0, t(e), t = n) : 0 !== i.count || o || t(null, r)
    }
    var o = !1;
    return n = n || r, i.count = e, 0 === e ? t() : i
  }

  function r() {}
  e.exports = n
}
