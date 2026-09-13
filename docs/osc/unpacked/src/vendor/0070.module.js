// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 70
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(279);
  module.exports = function(e) {
    var t = e.xdomain,
      n = e.xscheme,
      i = e.enablesXDR;
    try {
      if ("undefined" != typeof XMLHttpRequest && (!t || r)) return new XMLHttpRequest();
    } catch (e) {}
    try {
      if ("undefined" != typeof XDomainRequest && !n && i) return new XDomainRequest();
    } catch (e) {}
    if (!t) try {
      return new ActiveXObject("Microsoft.XMLHTTP");
    } catch (e) {}
  };
}
