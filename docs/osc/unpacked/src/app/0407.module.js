// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 407
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(15),
    o = require(111),
    r = require(36),
    a = require(51);
  module.exports = function(e) {
    i(i.S, e, {
      from: function(e) {
        var t,
          n,
          i,
          l,
          s = arguments[1];
        return o(this), t = void 0 !== s, t && o(s), void 0 == e ? new this() : (n = [], t ? (i = 0,
          l = r(s, arguments[2], 2), a(e, !1, function(e) {
            n.push(l(e, i++));
          })) : a(e, !1, n.push, n), new this(n));
      }
    });
  };
}
