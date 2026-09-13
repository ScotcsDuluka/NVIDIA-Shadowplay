// ─────────────────────────────────────────────────────────────
// APP MODULE 70
// role       : utility
// requires   : (none)
// source     : Overlay/osc/app.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var i = n(28),
    o = n(404),
    r = n(67),
    a = n(73)("IE_PROTO"),
    l = function() {},
    s = "prototype",
    d = function() {
      var e, t = n(114)("iframe"),
        i = r.length,
        o = "<",
        a = ">";
      for (t.style.display = "none", n(400).appendChild(t), t.src = "javascript:", e = t.contentWindow.document, e
        .open(), e.write(o + "script" + a + "document.F=Object" + o + "/script" + a), e.close(), d = e.F; i--;)
        delete d[s][r[i]];
      return d()
    };
  e.exports = Object.create || function(e, t) {
    var n;
    return null !== e ? (l[s] = i(e), n = new l, l[s] = null, n[a] = e) : n = d(), void 0 === t ? n : o(n, t)
  }
}
