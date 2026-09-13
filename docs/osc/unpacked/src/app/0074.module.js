// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 74
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(13),
    o = require(21),
    r = "__core-js_shared__",
    a = o[r] || (o[r] = {});
  (module.exports = function(e, t) {
    return a[e] || (a[e] = void 0 !== t ? t : {});
  })("versions", []).push({
    version: i.version,
    mode: require(52) ? "pure" : "global",
    copyright: "© 2020 Denis Pushkarev (zloirock.ru)"
  });
}
