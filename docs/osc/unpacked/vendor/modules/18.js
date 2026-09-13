// ─────────────────────────────────────────────────────────────
// VENDOR MODULE 18
// role       : utility
// requires   : (none)
// source     : Overlay/osc/vendor.js (minified) — beautified, unrecoverable local names remain
// ─────────────────────────────────────────────────────────────
function(e, t, n) {
  ! function(r, i) {
    e.exports = t = i(n(1))
  }(this, function(e) {
    return function() {
      function t(e, t, n) {
        for (var r = [], o = 0, a = 0; a < t; a++)
          if (a % 4) {
            var s = n[e.charCodeAt(a - 1)] << a % 4 * 2,
              c = n[e.charCodeAt(a)] >>> 6 - a % 4 * 2;
            r[o >>> 2] |= (s | c) << 24 - o % 4 * 8, o++
          } return i.create(r, o)
      }
      var n = e,
        r = n.lib,
        i = r.WordArray,
        o = n.enc;
      o.Base64 = {
        stringify: function(e) {
          var t = e.words,
            n = e.sigBytes,
            r = this._map;
          e.clamp();
          for (var i = [], o = 0; o < n; o += 3)
            for (var a = t[o >>> 2] >>> 24 - o % 4 * 8 & 255, s = t[o + 1 >>> 2] >>> 24 - (o + 1) % 4 * 8 & 255,
                c = t[o + 2 >>> 2] >>> 24 - (o + 2) % 4 * 8 & 255, u = a << 16 | s << 8 | c, l = 0; l < 4 && o +
              .75 * l < n; l++) i.push(r.charAt(u >>> 6 * (3 - l) & 63));
          var d = r.charAt(64);
          if (d)
            for (; i.length % 4;) i.push(d);
          return i.join("")
        },
        parse: function(e) {
          var n = e.length,
            r = this._map,
            i = this._reverseMap;
          if (!i) {
            i = this._reverseMap = [];
            for (var o = 0; o < r.length; o++) i[r.charCodeAt(o)] = o
          }
          var a = r.charAt(64);
          if (a) {
            var s = e.indexOf(a);
            s !== -1 && (n = s)
          }
          return t(e, n, i)
        },
        _map: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/="
      }
    }(), e.enc.Base64
  })
}
