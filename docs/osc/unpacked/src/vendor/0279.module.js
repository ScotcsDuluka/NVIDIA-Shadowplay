// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 279
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(257);
  try {
    module.exports = "XMLHttpRequest" in r && "withCredentials" in new r.XMLHttpRequest();
  } catch (t) {
    module.exports = !1;
  }
}
