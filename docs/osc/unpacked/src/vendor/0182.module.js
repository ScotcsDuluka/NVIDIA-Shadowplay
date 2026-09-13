// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 182
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var r = require(36),
    i = require(186),
    o = require(184),
    a = vendorModule /* vendor bundle require */,
    s = require(94),
    c = require(95),
    u = {},
    l = {},
    exports = module.exports = function(e, t, n, d, f) {
      var h,
        p,
        m,
        v,
        g = f ? function() {
          return e;
        } : c(e),
        y = r(n, d, t ? 2 : 1),
        b = 0;
      if ("function" != typeof g) throw TypeError(e + " is not iterable!");
      if (o(g)) {
        for (h = s(e.length); h > b; b++)
          if (v = t ? y(a(p = e[b])[0], p[1]) : y(e[b]), v === u || v === l) return v;
      } else
        for (m = g.call(e); !(p = m.next()).done;)
          if (v = i(m, y, p.value, t), v === u || v === l) return v;
    };
  exports.BREAK = u, exports.RETURN = l;
}
