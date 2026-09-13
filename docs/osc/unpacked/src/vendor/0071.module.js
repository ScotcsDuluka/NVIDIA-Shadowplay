// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 71
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  exports.encode = function(e) {
    var t = "";
    for (var n in e) e.hasOwnProperty(n) && (t.length && (t += "&"), t += encodeURIComponent(n) + "=" +
      encodeURIComponent(e[n]));
    return t;
  }, exports.decode = function(e) {
    for (var t = {}, n = e.split("&"), r = 0, i = n.length; r < i; r++) {
      var o = n[r].split("=");
      t[decodeURIComponent(o[0])] = decodeURIComponent(o[1]);
    }
    return t;
  };
}
