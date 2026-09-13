// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 163
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  "use strict";

  function n(e) {
    var t = e.length;
    if (t % 4 > 0) throw new Error("Invalid string. Length must be a multiple of 4");
    var n = e.indexOf("=");
    n === -1 && (n = t);
    var r = n === t ? 0 : 4 - n % 4;
    return [n, r]
  }

  function r(e) {
    var t = n(e),
      r = t[0],
      i = t[1];
    return 3 * (r + i) / 4 - i
  }

  function i(e, t, n) {
    return 3 * (t + n) / 4 - n
  }

  function o(e) {
    var t, r, o = n(e),
      a = o[0],
      s = o[1],
      c = new d(i(e, a, s)),
      u = 0,
      f = s > 0 ? a - 4 : a;
    for (r = 0; r < f; r += 4) t = l[e.charCodeAt(r)] << 18 | l[e.charCodeAt(r + 1)] << 12 | l[e.charCodeAt(r + 2)] <<
      6 | l[e.charCodeAt(r + 3)], c[u++] = t >> 16 & 255, c[u++] = t >> 8 & 255, c[u++] = 255 & t;
    return 2 === s && (t = l[e.charCodeAt(r)] << 2 | l[e.charCodeAt(r + 1)] >> 4, c[u++] = 255 & t), 1 === s && (t = l[e
      .charCodeAt(r)] << 10 | l[e.charCodeAt(r + 1)] << 4 | l[e.charCodeAt(r + 2)] >> 2, c[u++] = t >> 8 & 255, c[
      u++] = 255 & t), c
  }

  function a(e) {
    return u[e >> 18 & 63] + u[e >> 12 & 63] + u[e >> 6 & 63] + u[63 & e]
  }

  function s(e, t, n) {
    for (var r, i = [], o = t; o < n; o += 3) r = (e[o] << 16 & 16711680) + (e[o + 1] << 8 & 65280) + (255 & e[o + 2]),
      i.push(a(r));
    return i.join("")
  }

  function c(e) {
    for (var t, n = e.length, r = n % 3, i = [], o = 16383, a = 0, c = n - r; a < c; a += o) i.push(s(e, a, a + o > c ?
      c : a + o));
    return 1 === r ? (t = e[n - 1], i.push(u[t >> 2] + u[t << 4 & 63] + "==")) : 2 === r && (t = (e[n - 2] << 8) + e[n -
      1], i.push(u[t >> 10] + u[t >> 4 & 63] + u[t << 2 & 63] + "=")), i.join("")
  }
  t.byteLength = r, t.toByteArray = o, t.fromByteArray = c;
  for (var u = [], l = [], d = "undefined" != typeof Uint8Array ? Uint8Array : Array, f =
      "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", h = 0, p = f.length; h < p; ++h) u[h] = f[h],
    l[f.charCodeAt(h)] = h;
  l["-".charCodeAt(0)] = 62, l["_".charCodeAt(0)] = 63
}
