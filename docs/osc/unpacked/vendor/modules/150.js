// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 150
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t) {
  e.exports = function(e, t, n) {
    var r = e.byteLength;
    if (t = t || 0, n = n || r, e.slice) return e.slice(t, n);
    if (t < 0 && (t += r), n < 0 && (n += r), n > r && (n = r), t >= r || t >= n || 0 === r) return new ArrayBuffer(
    0);
    for (var i = new Uint8Array(e), o = new Uint8Array(n - t), a = t, s = 0; a < n; a++, s++) o[s] = i[a];
    return o.buffer
  }
}
