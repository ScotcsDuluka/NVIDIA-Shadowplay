// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 285
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  function n(e, t, n) {
    var r;
    return r = t ? new i(e, t) : new i(e);
  }
  var r = function() {
      return this;
    }(),
    i = r.WebSocket || r.MozWebSocket;
  module.exports = i ? n : null, i && (n.prototype = i.prototype);
}
