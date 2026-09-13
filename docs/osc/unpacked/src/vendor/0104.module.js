// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 104
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  var n = [].indexOf;
  module.exports = function(e, t) {
    if (n) return e.indexOf(t);
    for (var r = 0; r < e.length; ++r)
      if (e[r] === t) return r;
    return -1;
  };
}
