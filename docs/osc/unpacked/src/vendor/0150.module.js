// source-like reconstruction — beautified webpack module
// (local identifiers inside functions remain minified; every string,
//  template, and protocol name is the original)

// ==================================================================
// VENDOR MODULE 150
// (no Angular registrations — utility module)
// webpack factory params: module, exports
// ------------------------------------------------------------------

function(module, exports) {
  module.exports = function(e, t, n) {
    var r = e.byteLength;
    if (t = t || 0, n = n || r, e.slice) return e.slice(t, n);
    if (t < 0 && (t += r), n < 0 && (n += r), n > r && (n = r), t >= r || t >= n || 0 === r)
    return new ArrayBuffer(0);
    for (var i = new Uint8Array(e), o = new Uint8Array(n - t), a = t, s = 0; a < n; a++, s++) o[s] = i[a];
    return o.buffer;
  };
}
