// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 70
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(28),
    o = require(404),
    r = require(67),
    a = require(73)("IE_PROTO"),
    l = function() {},
    s = "prototype",
    d = function() {
      var e,
        t = require(114)("iframe"),
        i = r.length,
        o = "<",
        a = ">";
      for (t.style.display = "none", require(400).appendChild(t), t.src = "javascript:", e = t.contentWindow
        .document, e.open(), e.write(o + "script" + a + "document.F=Object" + o + "/script" + a), e.close(),
        d = e.F; i--;) delete d[s][r[i]];
      return d();
    };
  module.exports = Object.create || function(e, t) {
    var n;
    return null !== e ? (l[s] = i(e), n = new l(), l[s] = null, n[a] = e) : n = d(), void 0 === t ? n : o(n,
      t);
  };
}
