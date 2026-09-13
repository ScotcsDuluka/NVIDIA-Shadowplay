// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 213
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var r = require(8),
    i = require(52),
    o = require(89);
  r(r.S, "Promise", {
    try: function(e) {
      var t = i.f(this),
        n = o(e);
      return (n.e ? t.reject : t.resolve)(n.v), t.promise;
    }
  });
}
