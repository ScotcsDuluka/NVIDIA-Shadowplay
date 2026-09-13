// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 220
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i) {
    e.exports = t = i(n(1))
  }(this, function(e) {
    return function() {
      if ("function" == typeof ArrayBuffer) {
        var t = e,
          n = t.lib,
          r = n.WordArray,
          i = r.init,
          o = r.init = function(e) {
            if (e instanceof ArrayBuffer && (e = new Uint8Array(e)), (e instanceof Int8Array || "undefined" !=
                typeof Uint8ClampedArray && e instanceof Uint8ClampedArray || e instanceof Int16Array ||
                e instanceof Uint16Array || e instanceof Int32Array || e instanceof Uint32Array ||
                e instanceof Float32Array || e instanceof Float64Array) && (e = new Uint8Array(e.buffer, e
                .byteOffset, e.byteLength)), e instanceof Uint8Array) {
              for (var t = e.byteLength, n = [], r = 0; r < t; r++) n[r >>> 2] |= e[r] << 24 - r % 4 * 8;
              i.call(this, n, t)
            } else i.apply(this, arguments)
          };
        o.prototype = r
      }
    }(), e.lib.WordArray
  })
}
