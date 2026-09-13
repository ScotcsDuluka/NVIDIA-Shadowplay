// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 73
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  "use strict";

  function n(e, t) {
    e = e || [], t = t || {};
    try {
      return new Blob(e, t);
    } catch (o) {
      if ("TypeError" !== o.name) throw o;
      for (var n = "undefined" != typeof BlobBuilder ? BlobBuilder : "undefined" != typeof MSBlobBuilder ?
          MSBlobBuilder : "undefined" != typeof MozBlobBuilder ? MozBlobBuilder : WebKitBlobBuilder, r =
          new n(), i = 0; i < e.length; i += 1) r.append(e[i]);
      return r.getBlob(t.type);
    }
  }
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.default = n;
}
