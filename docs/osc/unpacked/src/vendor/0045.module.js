// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 45
// (no Angular registrations — utility module)
// webpack factory params: module, exports, require
// ------------------------------------------------------------------

function(module, exports, require) {
  function r() {
    return "WebkitAppearance" in document.documentElement.style || window.console && (console.firebug ||
        console.exception && console.table) || navigator.userAgent.toLowerCase().match(/firefox\/(\d+)/) &&
      parseInt(RegExp.$1, 10) >= 31;
  }

  function i() {
    var e = arguments,
      n = this.useColors;
    if (e[0] = (n ? "%c" : "") + this.namespace + (n ? " %c" : " ") + e[0] + (n ? "%c " : " ") + "+" + exports
      .humanize(this.diff), !n) return e;
    var r = "color: " + this.color;
    e = [e[0], r, "color: inherit"].concat(Array.prototype.slice.call(e, 1));
    var i = 0,
      o = 0;
    return e[0].replace(/%[a-z%]/g, function(e) {
      "%%" !== e && (i++, "%c" === e && (o = i));
    }), e.splice(o, 0, r), e;
  }

  function o() {
    return "object" == typeof console && "function" == typeof console.log && Function.prototype.apply.call(
      console.log, console, arguments);
  }

  function a(e) {
    try {
      null == e ? localStorage.removeItem("debug") : localStorage.debug = e;
    } catch (e) {}
  }

  function s() {
    var e;
    try {
      e = localStorage.debug;
    } catch (e) {}
    return e;
  }
  exports = module.exports = require(276), exports.log = o, exports.formatArgs = i, exports.save = a, exports
    .load = s, exports.useColors = r, exports.colors = ["lightseagreen", "forestgreen", "goldenrod",
      "dodgerblue", "darkorchid", "crimson"
    ], exports.formatters.j = function(e) {
      return JSON.stringify(e);
    }, exports.enable(s());
}
