// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 258
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  exports.read = function(e, t, n, r, i) {
    var o,
      a,
      s = 8 * i - r - 1,
      c = (1 << s) - 1,
      u = c >> 1,
      l = -7,
      d = n ? i - 1 : 0,
      f = n ? -1 : 1,
      h = e[t + d];
    for (d += f, o = h & (1 << -l) - 1, h >>= -l, l += s; l > 0; o = 256 * o + e[t + d], d += f, l -= 8);
    for (a = o & (1 << -l) - 1, o >>= -l, l += r; l > 0; a = 256 * a + e[t + d], d += f, l -= 8);
    if (0 === o) o = 1 - u;
    else {
      if (o === c) return a ? NaN : (h ? -1 : 1) * (1 / 0);
      a += Math.pow(2, r), o -= u;
    }
    return (h ? -1 : 1) * a * Math.pow(2, o - r);
  }, exports.write = function(e, t, n, r, i, o) {
    var a,
      s,
      c,
      u = 8 * o - i - 1,
      l = (1 << u) - 1,
      d = l >> 1,
      f = 23 === i ? Math.pow(2, -24) - Math.pow(2, -77) : 0,
      h = r ? 0 : o - 1,
      p = r ? 1 : -1,
      m = t < 0 || 0 === t && 1 / t < 0 ? 1 : 0;
    for (t = Math.abs(t), isNaN(t) || t === 1 / 0 ? (s = isNaN(t) ? 1 : 0, a = l) : (a = Math.floor(Math
          .log(t) / Math.LN2), t * (c = Math.pow(2, -a)) < 1 && (a--, c *= 2), t += a + d >= 1 ? f / c : f *
        Math.pow(2, 1 - d), t * c >= 2 && (a++, c /= 2), a + d >= l ? (s = 0, a = l) : a + d >= 1 ? (s = (
          t * c - 1) * Math.pow(2, i), a += d) : (s = t * Math.pow(2, d - 1) * Math.pow(2, i), a = 0)); i >=
      8; e[n + h] = 255 & s, h += p, s /= 256, i -= 8);
    for (a = a << i | s, u += i; u > 0; e[n + h] = 255 & a, h += p, a /= 256, u -= 8);
    e[n + h - p] |= 128 * m;
  };
}
