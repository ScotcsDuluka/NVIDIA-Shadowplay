// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 402
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(16)("iterator"),
    o = !1;
  try {
    var r = [7][i]();
    r.return = function() {
      o = !0;
    }, Array.from(r, function() {
      throw 2;
    });
  } catch (e) {}
  module.exports = function(e, t) {
    if (!t && !o) return !1;
    var n = !1;
    try {
      var r = [7],
        a = r[i]();
      a.next = function() {
        return {
          done: n = !0
        };
      }, r[i] = function() {
        return a;
      }, e(r);
    } catch (e) {}
    return n;
  };
}
