// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 183
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports = function(e, t, n) {
    var r = void 0 === n;
    switch (t.length) {
      case 0:
        return r ? e() : e.call(n);
      case 1:
        return r ? e(t[0]) : e.call(n, t[0]);
      case 2:
        return r ? e(t[0], t[1]) : e.call(n, t[0], t[1]);
      case 3:
        return r ? e(t[0], t[1], t[2]) : e.call(n, t[0], t[1], t[2]);
      case 4:
        return r ? e(t[0], t[1], t[2], t[3]) : e.call(n, t[0], t[1], t[2], t[3]);
    }
    return e.apply(n, t);
  };
}
