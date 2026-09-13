// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 415
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(395),
    o = require(125),
    r = "Map";
  module.exports = require(397)(r, function(e) {
    return function() {
      return e(this, arguments.length > 0 ? arguments[0] : void 0);
    };
  }, {
    get: function(e) {
      var t = i.getEntry(o(this, r), e);
      return t && t.v;
    },
    set: function(e, t) {
      return i.def(o(this, r), 0 === e ? 0 : e, t);
    }
  }, i, !0);
}
