// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 19
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(28),
    o = require(115),
    r = require(76),
    a = Object.defineProperty;
  exports.f = require(18) ? Object.defineProperty : function(e, t, n) {
    if (i(e), t = r(t, !0), i(n), o) try {
      return a(e, t, n);
    } catch (e) {}
    if ("get" in n || "set" in n) throw TypeError("Accessors not supported!");
    return "value" in n && (e[t] = n.value), e;
  };
}
