// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 268
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  function n(e, t, n) {
    function i(e, r) {
      if (i.count <= 0) throw new Error("after called too many times");
      --i.count, e ? (o = !0, t(e), t = n) : 0 !== i.count || o || t(null, r);
    }
    var o = !1;
    return n = n || r, i.count = e, 0 === e ? t() : i;
  }

  function r() {}
  module.exports = n;
}
