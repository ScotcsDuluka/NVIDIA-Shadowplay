// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// APP MODULE 51
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  var i = require(36),
    o = require(118),
    r = require(116),
    a = require(28),
    l = require(56),
    s = require(79),
    d = {},
    c = {},
    exports = module.exports = function(e, t, n, u, f) {
      var m,
        g,
        p,
        h,
        b = f ? function() {
          return e;
        } : s(e),
        x = i(n, u, t ? 2 : 1),
        v = 0;
      if ("function" != typeof b) throw TypeError(e + " is not iterable!");
      if (r(b)) {
        for (m = l(e.length); m > v; v++)
          if (h = t ? x(a(g = e[v])[0], g[1]) : x(e[v]), h === d || h === c) return h;
      } else
        for (p = b.call(e); !(g = p.next()).done;)
          if (h = o(p, x, g.value, t), h === d || h === c) return h;
    };
  exports.BREAK = d, exports.RETURN = c;
}
