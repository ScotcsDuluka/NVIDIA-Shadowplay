// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 405
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(54),
    o = require(43),
    r = require(37),
    a = require(76),
    l = require(30),
    s = require(115),
    d = Object.getOwnPropertyDescriptor;
  exports.f = require(18) ? d : function(e, t) {
    if (e = r(e), t = a(t, !0), s) try {
      return d(e, t);
    } catch (e) {}
    if (l(e, t)) return o(!i.f.call(e, t), e[t]);
  };
}
