// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 58
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(410)(!0);
  require(69)(String, "String", function(e) {
    this._t = String(e), this._i = 0;
  }, function() {
    var e,
      t = this._t,
      n = this._i;
    return n >= t.length ? {
      value: void 0,
      done: !0
    } : (e = i(t, n), this._i += e.length, {
      value: e,
      done: !1
    });
  });
}
