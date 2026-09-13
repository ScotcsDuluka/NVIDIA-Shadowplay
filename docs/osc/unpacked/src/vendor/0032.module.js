// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 32
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  function n(e) {
    return n.enabled(e) ? function(t) {
      t = r(t);
      var i = new Date(),
        o = i - (n[e] || i);
      n[e] = i, t = e + " " + t + " +" + n.humanize(o), window.console && console.log && Function.prototype
        .apply.call(console.log, console, arguments);
    } : function() {};
  }

  function r(e) {
    return e instanceof Error ? e.stack || e.message : e;
  }
  module.exports = n, n.names = [], n.skips = [], n.enable = function(e) {
    try {
      localStorage.debug = e;
    } catch (e) {}
    for (var t = (e || "").split(/[\s,]+/), r = t.length, i = 0; i < r; i++) e = t[i].replace("*", ".*?"),
      "-" === e[0] ? n.skips.push(new RegExp("^" + e.substr(1) + "$")) : n.names.push(new RegExp("^" + e +
        "$"));
  }, n.disable = function() {
    n.enable("");
  }, n.humanize = function(e) {
    var t = 1e3,
      n = 6e4,
      r = 60 * n;
    return e >= r ? (e / r).toFixed(1) + "h" : e >= n ? (e / n).toFixed(1) + "m" : e >= t ? (e / t | 0) +
      "s" : e + "ms";
  }, n.enabled = function(e) {
    for (var t = 0, r = n.skips.length; t < r; t++)
      if (n.skips[t].test(e)) return !1;
    for (var t = 0, r = n.names.length; t < r; t++)
      if (n.names[t].test(e)) return !0;
    return !1;
  };
  try {
    window.localStorage && n.enable(localStorage.debug);
  } catch (e) {}
}
