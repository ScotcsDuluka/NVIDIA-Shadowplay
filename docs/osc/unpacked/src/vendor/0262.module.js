// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 262
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  function n(e) {
    var t = /^((?:\d+)?\.?\d+) *(ms|seconds?|s|minutes?|m|hours?|h|days?|d|years?|y)?$/i.exec(e);
    if (t) {
      var n = parseFloat(t[1]),
        r = (t[2] || "ms").toLowerCase();
      switch (r) {
        case "years":
        case "year":
        case "y":
          return n * l;
        case "days":
        case "day":
        case "d":
          return n * u;
        case "hours":
        case "hour":
        case "h":
          return n * c;
        case "minutes":
        case "minute":
        case "m":
          return n * s;
        case "seconds":
        case "second":
        case "s":
          return n * a;
        case "ms":
          return n;
      }
    }
  }

  function r(e) {
    return e >= u ? Math.round(e / u) + "d" : e >= c ? Math.round(e / c) + "h" : e >= s ? Math.round(e / s) +
      "m" : e >= a ? Math.round(e / a) + "s" : e + "ms";
  }

  function i(e) {
    return o(e, u, "day") || o(e, c, "hour") || o(e, s, "minute") || o(e, a, "second") || e + " ms";
  }

  function o(e, t, n) {
    if (!(e < t)) return e < 1.5 * t ? Math.floor(e / t) + " " + n : Math.ceil(e / t) + " " + n + "s";
  }
  var a = 1e3,
    s = 60 * a,
    c = 60 * s,
    u = 24 * c,
    l = 365.25 * u;
  module.exports = function(e, t) {
    return t = t || {}, "string" == typeof e ? n(e) : t.long ? i(e) : r(e);
  };
}
