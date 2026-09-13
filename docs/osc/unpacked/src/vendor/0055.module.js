// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 55
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(2),
    i = require(4),
    o = "__core-js_shared__",
    a = i[o] || (i[o] = {});
  (module.exports = function(e, t) {
    return a[e] || (a[e] = void 0 !== t ? t : {});
  })("versions", []).push({
    version: r.version,
    mode: require(28) ? "pure" : "global",
    copyright: "© 2020 Denis Pushkarev (zloirock.ru)"
  });
}
