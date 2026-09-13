// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 45
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  function r() {
    return "WebkitAppearance" in document.documentElement.style || window.console && (console.firebug || console
      .exception && console.table) || navigator.userAgent.toLowerCase().match(/firefox\/(\d+)/) && parseInt(RegExp.$1,
      10) >= 31
  }

  function i() {
    var e = arguments,
      n = this.useColors;
    if (e[0] = (n ? "%c" : "") + this.namespace + (n ? " %c" : " ") + e[0] + (n ? "%c " : " ") + "+" + t.humanize(this
        .diff), !n) return e;
    var r = "color: " + this.color;
    e = [e[0], r, "color: inherit"].concat(Array.prototype.slice.call(e, 1));
    var i = 0,
      o = 0;
    return e[0].replace(/%[a-z%]/g, function(e) {
      "%%" !== e && (i++, "%c" === e && (o = i))
    }), e.splice(o, 0, r), e
  }

  function o() {
    return "object" == typeof console && "function" == typeof console.log && Function.prototype.apply.call(console.log,
      console, arguments)
  }

  function a(e) {
    try {
      null == e ? localStorage.removeItem("debug") : localStorage.debug = e
    } catch (e) {}
  }

  function s() {
    var e;
    try {
      e = localStorage.debug
    } catch (e) {}
    return e
  }
  t = e.exports = n(276), t.log = o, t.formatArgs = i, t.save = a, t.load = s, t.useColors = r, t.colors = [
    "lightseagreen", "forestgreen", "goldenrod", "dodgerblue", "darkorchid", "crimson"
  ], t.formatters.j = function(e) {
    return JSON.stringify(e)
  }, t.enable(s())
}
