// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 281
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  (function(t) {
    var n = /^[\],:{}\s]*$/,
      r = /\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g,
      i = /"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g,
      o = /(?:^|:|,)(?:\s*\[)+/g,
      a = /^\s+/,
      s = /\s+$/;
    module.exports = function(e) {
      return "string" == typeof e && e ? (e = e.replace(a, "").replace(s, ""), t.JSON && JSON.parse ? JSON
        .parse(e) : n.test(e.replace(r, "@").replace(i, "]").replace(o, "")) ? new Function("return " +
          e)() : void 0) : null;
    };
  }).call(exports, function() {
    return this;
  }());
}
