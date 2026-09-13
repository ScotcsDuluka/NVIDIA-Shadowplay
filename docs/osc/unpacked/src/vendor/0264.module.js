// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 264
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  var n = Object.prototype.hasOwnProperty;
  exports.keys = Object.keys || function(e) {
    var t = [];
    for (var r in e) n.call(e, r) && t.push(r);
    return t;
  }, exports.values = function(e) {
    var t = [];
    for (var r in e) n.call(e, r) && t.push(e[r]);
    return t;
  }, exports.merge = function(e, t) {
    for (var r in t) n.call(t, r) && (e[r] = t[r]);
    return e;
  }, exports.length = function(e) {
    return exports.keys(e).length;
  }, exports.isEmpty = function(e) {
    return 0 == exports.length(e);
  };
}
