// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 111
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  (function(t) {
    function r(e) {
      function n(e) {
        if (!e) return !1;
        if (t.Buffer && t.Buffer.isBuffer(e) || t.ArrayBuffer && e instanceof ArrayBuffer || t.Blob &&
          e instanceof Blob || t.File && e instanceof File) return !0;
        if (i(e)) {
          for (var r = 0; r < e.length; r++)
            if (n(e[r])) return !0
        } else if (e && "object" == typeof e) {
          e.toJSON && (e = e.toJSON());
          for (var o in e)
            if (Object.prototype.hasOwnProperty.call(e, o) && n(e[o])) return !0
        }
        return !1
      }
      return n(e)
    }
    var i = n(68);
    e.exports = r
  }).call(t, function() {
    return this
  }())
}
