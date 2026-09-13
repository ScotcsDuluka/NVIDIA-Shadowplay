// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 186
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = vendorModule /* vendor bundle require */;
  module.exports = function(e, t, n, i) {
    try {
      return i ? t(r(n)[0], n[1]) : t(n);
    } catch (t) {
      var o = e.return;
      throw void 0 !== o && r(o.call(e)), t;
    }
  };
}
