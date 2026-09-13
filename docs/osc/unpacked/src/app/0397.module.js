// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 397
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(21),
    o = require(15),
    r = require(53),
    a = require(29),
    l = require(27),
    s = require(123),
    d = require(51),
    c = require(112),
    u = require(24),
    f = require(55),
    m = require(19).f,
    g = require(392)(0),
    p = require(18);
  module.exports = function(e, t, n, h, b, x) {
    var v = i[e],
      y = v,
      w = b ? "set" : "add",
      S = y && y.prototype,
      E = {};
    return p && "function" == typeof y && (x || S.forEach && !a(function() {
      new y().entries().next();
    })) ? (y = t(function(t, n) {
      c(t, y, e, "_c"), t._c = new v(), void 0 != n && d(n, b, t[w], t);
    }), g("add,clear,delete,forEach,get,has,set,keys,values,entries,toJSON".split(","), function(e) {
      var t = "add" == e || "set" == e;
      e in S && (!x || "clear" != e) && l(y.prototype, e, function(n, i) {
        if (c(this, y, e), !t && x && !u(n)) return "get" == e && void 0;
        var o = this._c[e](0 === n ? 0 : n, i);
        return t ? this : o;
      });
    }), x || m(y.prototype, "size", {
      get: function() {
        return this._c.size;
      }
    })) : (y = h.getConstructor(t, e, b, w), s(y.prototype, n), r.NEED = !0), f(y, e), E[e] = y, o(o.G + o
      .W + o.F, E), x || h.setStrong(y, e, b), y;
  };
}
