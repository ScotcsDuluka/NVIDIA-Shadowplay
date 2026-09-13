// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 212
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var r = require(8),
    i = require(2),
    o = require(4),
    a = require(92),
    s = require(90);
  r(r.P + r.R, "Promise", {
    finally: function(e) {
      var t = a(this, i.Promise || o.Promise),
        n = "function" == typeof e;
      return this.then(n ? function(n) {
        return s(t, e()).then(function() {
          return n;
        });
      } : e, n ? function(n) {
        return s(t, e()).then(function() {
          throw n;
        });
      } : e);
    }
  });
}
