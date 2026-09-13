// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 9
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = vendorModule /* vendor bundle require */,
    i = require(81),
    o = require(57),
    a = Object.defineProperty;
  exports.f = require(6) ? Object.defineProperty : function(e, t, n) {
    if (r(e), t = o(t, !0), r(n), i) try {
      return a(e, t, n);
    } catch (e) {}
    if ("get" in n || "set" in n) throw TypeError("Accessors not supported!");
    return "value" in n && (e[t] = n.value), e;
  };
}
