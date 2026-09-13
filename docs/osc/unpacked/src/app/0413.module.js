// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 413
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  "use strict";
  var i = require(36),
    o = require(15),
    r = require(31),
    a = require(118),
    l = require(116),
    s = require(56),
    d = require(398),
    c = require(79);
  o(o.S + o.F * !require(402)(function(e) {
    Array.from(e);
  }), "Array", {
    from: function(e) {
      var t,
        n,
        o,
        u,
        f = r(e),
        m = "function" == typeof this ? this : Array,
        g = arguments.length,
        p = g > 1 ? arguments[1] : void 0,
        h = void 0 !== p,
        b = 0,
        x = c(f);
      if (h && (p = i(p, g > 2 ? arguments[2] : void 0, 2)), void 0 == x || m == Array && l(x))
        for (t = s(f.length), n = new m(t); t > b; b++) d(n, b, h ? p(f[b], b) : f[b]);
      else
        for (u = x.call(f), n = new m(); !(o = u.next()).done; b++) d(n, b, h ? a(u, p, [o.value, b], !
          0) : o.value);
      return n.length = b, n;
    }
  });
}
