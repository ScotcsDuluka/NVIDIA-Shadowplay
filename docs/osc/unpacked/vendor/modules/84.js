// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 84
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  var r = n(7),
    i = n(193),
    o = n(51),
    a = n(54)("IE_PROTO"),
    s = function() {},
    c = "prototype",
    u = function() {
      var e, t = n(50)("iframe"),
        r = o.length,
        i = "<",
        a = ">";
      for (t.style.display = "none", n(80).appendChild(t), t.src = "javascript:", e = t.contentWindow.document, e
      .open(), e.write(i + "script" + a + "document.F=Object" + i + "/script" + a), e.close(), u = e.F; r--;) delete u[
        c][o[r]];
      return u()
    };
  e.exports = Object.create || function(e, t) {
    var n;
    return null !== e ? (s[c] = r(e), n = new s, s[c] = null, n[a] = e) : n = u(), void 0 === t ? n : i(n, t)
  }
}
