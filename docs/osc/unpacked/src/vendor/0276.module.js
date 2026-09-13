// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 276
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  function r() {
    return exports.colors[l++ % exports.colors.length];
  }

  function i(e) {
    function n() {}

    function i() {
      var e = i,
        n = +new Date(),
        o = n - (u || n);
      e.diff = o, e.prev = u, e.curr = n, u = n, null == e.useColors && (e.useColors = exports.useColors()),
        null == e.color && e.useColors && (e.color = r());
      var a = Array.prototype.slice.call(arguments);
      a[0] = exports.coerce(a[0]), "string" != typeof a[0] && (a = ["%o"].concat(a));
      var s = 0;
      a[0] = a[0].replace(/%([a-z%])/g, function(n, r) {
        if ("%%" === n) return n;
        s++;
        var i = exports.formatters[r];
        if ("function" == typeof i) {
          var o = a[s];
          n = i.call(e, o), a.splice(s, 1), s--;
        }
        return n;
      }), "function" == typeof exports.formatArgs && (a = exports.formatArgs.apply(e, a));
      var c = i.log || exports.log || console.log.bind(console);
      c.apply(e, a);
    }
    n.enabled = !1, i.enabled = !0;
    var o = exports.enabled(e) ? i : n;
    return o.namespace = e, o;
  }

  function o(e) {
    exports.save(e);
    for (var n = (e || "").split(/[\s,]+/), r = n.length, i = 0; i < r; i++) n[i] && (e = n[i].replace(/\*/g,
      ".*?"), "-" === e[0] ? exports.skips.push(new RegExp("^" + e.substr(1) + "$")) : exports.names.push(
      new RegExp("^" + e + "$")));
  }

  function a() {
    exports.enable("");
  }

  function s(e) {
    var n, r;
    for (n = 0, r = exports.skips.length; n < r; n++)
      if (exports.skips[n].test(e)) return !1;
    for (n = 0, r = exports.names.length; n < r; n++)
      if (exports.names[n].test(e)) return !0;
    return !1;
  }

  function c(e) {
    return e instanceof Error ? e.stack || e.message : e;
  }
  exports = module.exports = i, exports.coerce = c, exports.disable = a, exports.enable = o, exports.enabled =
    s, exports.humanize = require(262), exports.names = [], exports.skips = [], exports.formatters = {};
  var u,
    l = 0;
}
