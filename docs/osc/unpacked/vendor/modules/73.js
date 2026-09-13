// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 73
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";

  function n(e, t) {
    e = e || [], t = t || {};
    try {
      return new Blob(e, t)
    } catch (o) {
      if ("TypeError" !== o.name) throw o;
      for (var n = "undefined" != typeof BlobBuilder ? BlobBuilder : "undefined" != typeof MSBlobBuilder ?
          MSBlobBuilder : "undefined" != typeof MozBlobBuilder ? MozBlobBuilder : WebKitBlobBuilder, r = new n, i =
          0; i < e.length; i += 1) r.append(e[i]);
      return r.getBlob(t.type)
    }
  }
  Object.defineProperty(t, "__esModule", {
    value: !0
  }), t.default = n
}
