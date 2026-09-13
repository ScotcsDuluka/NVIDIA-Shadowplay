// ─────────────────────────────────────────────────────────────
// APP MODULE 74
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(13),
    o = n(21),
    r = "__core-js_shared__",
    a = o[r] || (o[r] = {});
  (e.exports = function(e, t) {
    return a[e] || (a[e] = void 0 !== t ? t : {})
  })("versions", []).push({
    version: i.version,
    mode: n(52) ? "pure" : "global",
    copyright: "© 2020 Denis Pushkarev (zloirock.ru)"
  })
}
