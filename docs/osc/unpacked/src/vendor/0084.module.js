// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 84
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = vendorModule /* vendor bundle require */,
    i = require(193),
    o = require(51),
    a = require(54)("IE_PROTO"),
    s = function() {},
    c = "prototype",
    u = function() {
      var e,
        t = require(50)("iframe"),
        r = o.length,
        i = "<",
        a = ">";
      for (t.style.display = "none", require(80).appendChild(t), t.src = "javascript:", e = t.contentWindow
        .document, e.open(), e.write(i + "script" + a + "document.F=Object" + i + "/script" + a), e.close(),
        u = e.F; r--;) delete u[c][o[r]];
      return u();
    };
  module.exports = Object.create || function(e, t) {
    var n;
    return null !== e ? (s[c] = r(e), n = new s(), s[c] = null, n[a] = e) : n = u(), void 0 === t ? n : i(n,
      t);
  };
}
