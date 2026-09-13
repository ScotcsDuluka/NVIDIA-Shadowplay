// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 55
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(2),
    i = n(4),
    o = "__core-js_shared__",
    a = i[o] || (i[o] = {});
  (e.exports = function(e, t) {
    return a[e] || (a[e] = void 0 !== t ? t : {})
  })("versions", []).push({
    version: r.version,
    mode: n(28) ? "pure" : "global",
    copyright: "© 2020 Denis Pushkarev (zloirock.ru)"
  })
}
