// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 118
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(28);
  module.exports = function(e, t, n, o) {
    try {
      return o ? t(i(n)[0], n[1]) : t(n);
    } catch (t) {
      var r = e.return;
      throw void 0 !== r && i(r.call(e)), t;
    }
  };
}
