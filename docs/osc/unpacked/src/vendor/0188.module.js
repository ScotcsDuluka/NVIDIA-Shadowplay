// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 188
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(5)("iterator"),
    i = !1;
  try {
    var o = [7][r]();
    o.return = function() {
      i = !0;
    }, Array.from(o, function() {
      throw 2;
    });
  } catch (e) {}
  module.exports = function(e, t) {
    if (!t && !i) return !1;
    var n = !1;
    try {
      var o = [7],
        a = o[r]();
      a.next = function() {
        return {
          done: n = !0
        };
      }, o[r] = function() {
        return a;
      }, e(o);
    } catch (e) {}
    return n;
  };
}
