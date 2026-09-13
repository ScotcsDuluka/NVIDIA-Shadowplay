// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 408
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(15);
  module.exports = function(e) {
    i(i.S, e, {
      of: function() {
        for (var e = arguments.length, t = new Array(e); e--;) t[e] = arguments[e];
        return new this(t);
      }
    });
  };
}
