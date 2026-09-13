// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 276
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  function r() {
    return t.colors[l++ % t.colors.length]
  }

  function i(e) {
    function n() {}

    function i() {
      var e = i,
        n = +new Date,
        o = n - (u || n);
      e.diff = o, e.prev = u, e.curr = n, u = n, null == e.useColors && (e.useColors = t.useColors()), null == e
        .color && e.useColors && (e.color = r());
      var a = Array.prototype.slice.call(arguments);
      a[0] = t.coerce(a[0]), "string" != typeof a[0] && (a = ["%o"].concat(a));
      var s = 0;
      a[0] = a[0].replace(/%([a-z%])/g, function(n, r) {
        if ("%%" === n) return n;
        s++;
        var i = t.formatters[r];
        if ("function" == typeof i) {
          var o = a[s];
          n = i.call(e, o), a.splice(s, 1), s--
        }
        return n
      }), "function" == typeof t.formatArgs && (a = t.formatArgs.apply(e, a));
      var c = i.log || t.log || console.log.bind(console);
      c.apply(e, a)
    }
    n.enabled = !1, i.enabled = !0;
    var o = t.enabled(e) ? i : n;
    return o.namespace = e, o
  }

  function o(e) {
    t.save(e);
    for (var n = (e || "").split(/[\s,]+/), r = n.length, i = 0; i < r; i++) n[i] && (e = n[i].replace(/\*/g, ".*?"),
      "-" === e[0] ? t.skips.push(new RegExp("^" + e.substr(1) + "$")) : t.names.push(new RegExp("^" + e + "$")))
  }

  function a() {
    t.enable("")
  }

  function s(e) {
    var n, r;
    for (n = 0, r = t.skips.length; n < r; n++)
      if (t.skips[n].test(e)) return !1;
    for (n = 0, r = t.names.length; n < r; n++)
      if (t.names[n].test(e)) return !0;
    return !1
  }

  function c(e) {
    return e instanceof Error ? e.stack || e.message : e
  }
  t = e.exports = i, t.coerce = c, t.disable = a, t.enable = o, t.enabled = s, t.humanize = n(262), t.names = [], t
    .skips = [], t.formatters = {};
  var u, l = 0
}
